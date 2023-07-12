using MassTransit;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Inventory.Contracts;
using QPlay.Trading.Service.Models.Entities;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Consumers;

public class InventoryItemUpdatedConsumer : IConsumer<InventoryItemUpdated>
{
    private readonly IRepository<InventoryItem> repository;

    public InventoryItemUpdatedConsumer(IRepository<InventoryItem> repository)
    {
        this.repository = repository;
    }

    public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
    {
        InventoryItemUpdated message = context.Message;
        InventoryItem inventoryItem = await repository.GetAsync(
            item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId
        );

        if (inventoryItem == null)
        {
            inventoryItem = new()
            {
                CatalogItemId = message.CatalogItemId,
                UserId = message.UserId,
                Quantity = message.NewTotalQuantity
            };

            await repository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity = message.NewTotalQuantity;
            
            await repository.UpdateAsync(inventoryItem);
        }
    }
}