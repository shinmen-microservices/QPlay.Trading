using System;
using MassTransit;
using QPlay.Identity.Contracts;
using QPlay.Inventory.Contracts;
using QPlay.Trading.Service.Activities;
using QPlay.Trading.Service.Contracts;
using QPlay.Trading.Service.SignalR;

namespace QPlay.Trading.Service.StateMachines;

public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>
{
    private readonly MessageHub hub;
    public State Accepted { get; }
    public State ItemsGranted { get; }
    public State Completed { get; }
    public State Faulted { get; }

    public PurchaseStateMachine(MessageHub hub)
    {
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAny();
        ConfigureAccepted();
        ConfigureItemsGranted();
        ConfigureFaulted();
        ConfigureCompleted();
        this.hub = hub;
    }

    public Event<PurchaseRequested> PurchaseRequested { get; }
    public Event<GetPurchaseState> GetPurchaseState { get; }
    public Event<InventoryItemsGranted> InventoryItemsGranted { get; }
    public Event<GilDebited> GilDebited { get; }
    public Event<Fault<GrantItems>> GrantItemsFaulted { get; }
    public Event<Fault<DebitGil>> DebitGilFaulted { get; }

    private void ConfigureEvents()
    {
        Event(() => PurchaseRequested);
        Event(() => GetPurchaseState);
        Event(() => InventoryItemsGranted);
        Event(() => GilDebited);
        Event(
            () => GrantItemsFaulted,
            configurator =>
                configurator.CorrelateById(context => context.Message.Message.CorrelationId)
        );
        Event(
            () => DebitGilFaulted,
            configurator =>
                configurator.CorrelateById(context => context.Message.Message.CorrelationId)
        );
    }

    private void ConfigureInitialState()
    {
        Initially(
            When(PurchaseRequested)
                .Then(context =>
                {
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.ItemId = context.Message.ItemId;
                    context.Saga.Quantity = context.Message.Quantity;
                    context.Saga.Received = DateTimeOffset.UtcNow;
                    context.Saga.LastUpdated = context.Saga.Received;
                })
                .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                .Send(
                    context =>
                        new GrantItems(
                            context.Saga.UserId,
                            context.Saga.ItemId,
                            context.Saga.Quantity,
                            context.Saga.CorrelationId
                        )
                )
                .TransitionTo(Accepted)
                .Catch<Exception>(
                    ex =>
                        ex.Then(context =>
                            {
                                context.Saga.ErrorMessage = context.Exception.Message;
                                context.Saga.LastUpdated = DateTimeOffset.UtcNow;
                            })
                            .TransitionTo(Faulted)
                            .ThenAsync(async context => await hub.SendStatusAsync(context.Saga))
                )
        );
    }

    private void ConfigureAny()
    {
        DuringAny(When(GetPurchaseState).Respond(context => context.Saga));
    }

    private void ConfigureCompleted()
    {
        During(
            Completed,
            Ignore(PurchaseRequested),
            Ignore(InventoryItemsGranted),
            Ignore(GilDebited)
        );
    }

    private void ConfigureAccepted()
    {
        During(
            Accepted,
            Ignore(PurchaseRequested),
            When(InventoryItemsGranted)
                .Then(context => context.Saga.LastUpdated = DateTimeOffset.UtcNow)
                .Send(
                    context =>
                        new DebitGil(
                            context.Saga.UserId,
                            context.Saga.PurchaseTotal.Value,
                            context.Saga.CorrelationId
                        )
                )
                .TransitionTo(ItemsGranted),
            When(GrantItemsFaulted)
                .Then(context =>
                {
                    context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                    context.Saga.LastUpdated = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Faulted)
                .ThenAsync(async context => await hub.SendStatusAsync(context.Saga))
        );
    }

    private void ConfigureItemsGranted()
    {
        During(
            ItemsGranted,
            Ignore(PurchaseRequested),
            Ignore(InventoryItemsGranted),
            When(GilDebited)
                .Then(context => context.Saga.LastUpdated = DateTimeOffset.UtcNow)
                .TransitionTo(Completed)
                .ThenAsync(async context => await hub.SendStatusAsync(context.Saga)),
            When(DebitGilFaulted)
                .Send(
                    context =>
                        new SubtractItems(
                            context.Saga.UserId,
                            context.Saga.ItemId,
                            context.Saga.Quantity,
                            context.Saga.CorrelationId
                        )
                )
                .Then(context =>
                {
                    context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                    context.Saga.LastUpdated = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Faulted)
                .ThenAsync(async context => await hub.SendStatusAsync(context.Saga))
        );
    }

    private void ConfigureFaulted()
    {
        During(
            Faulted,
            Ignore(PurchaseRequested),
            Ignore(InventoryItemsGranted),
            Ignore(GilDebited)
        );
    }
}