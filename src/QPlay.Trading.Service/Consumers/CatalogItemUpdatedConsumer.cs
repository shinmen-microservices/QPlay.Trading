using MassTransit;
using QPlay.Catalog.Contracts;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Trading.Service.Models.Entities;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{
    private readonly IRepository<CatalogItem> repository;

    public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        CatalogItemUpdated message = context.Message;
        CatalogItem item = await repository.GetAsync(message.ItemId);

        if (item == null)
        {
            item = new()
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description,
                Price = message.Price
            };

            await repository.CreateAsync(item);
        }
        else
        {
            item.Name = message.Name;
            item.Description = message.Description;
            item.Price = message.Price;

            await repository.UpdateAsync(item);
        }
    }
}