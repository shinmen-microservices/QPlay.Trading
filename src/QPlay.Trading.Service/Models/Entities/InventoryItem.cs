using QPlay.Common.Entities.Interfaces;
using System;

namespace QPlay.Trading.Service.Models.Entities;

public class InventoryItem : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int Quantity { get; set; }
}