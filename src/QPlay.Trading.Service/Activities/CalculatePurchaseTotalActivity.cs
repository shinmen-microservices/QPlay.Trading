using Automatonymous;
using GreenPipes;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Trading.Service.Contracts;
using QPlay.Trading.Service.Exceptions;
using QPlay.Trading.Service.Models.Entities;
using QPlay.Trading.Service.StateMachines;
using System;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Activities;

public class CalculatePurchaseTotalActivity : Activity<PurchaseState, PurchaseRequested>
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
        Behavior<PurchaseState, PurchaseRequested> next
    )
    {
        PurchaseRequested message = context.Data;

        CatalogItem catalogItem =
            await repository.GetAsync(message.ItemId)
            ?? throw new UnknownItemException(message.ItemId);

        context.Instance.PurchaseTotal = catalogItem.Price * message.Quantity;
        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context,
        Behavior<PurchaseState, PurchaseRequested> next
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