using System;
using System.Threading.Tasks;
using MassTransit;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Trading.Service.Contracts;
using QPlay.Trading.Service.Exceptions;
using QPlay.Trading.Service.Models.Entities;
using QPlay.Trading.Service.StateMachines;

namespace QPlay.Trading.Service.Activities;

public class CalculatePurchaseTotalActivity : IStateMachineActivity<PurchaseState, PurchaseRequested>
{
    private readonly IRepository<CatalogItem> repository;

    public CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<PurchaseState, PurchaseRequested> context,
        IBehavior<PurchaseState, PurchaseRequested> next
    )
    {
        PurchaseRequested message = context.Message;

        CatalogItem catalogItem =
            await repository.GetAsync(message.ItemId)
            ?? throw new UnknownItemException(message.ItemId);

        context.Saga.PurchaseTotal = catalogItem.Price * message.Quantity;
        context.Saga.LastUpdated = DateTimeOffset.UtcNow;

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context,
        IBehavior<PurchaseState, PurchaseRequested> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("calculate-purchase-total");
    }
}