using MassTransit;
using QPlay.Catalog.Contracts;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Trading.Service.Models.Entities;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> repository;

    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }

    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        CatalogItemDeleted message = context.Message;
        CatalogItem item = await repository.GetAsync(message.ItemId);

        if (item == null)
            return;

        await repository.RemoveAsync(message.ItemId);
    }
}