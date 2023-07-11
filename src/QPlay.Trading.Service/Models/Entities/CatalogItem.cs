using QPlay.Common.Entities.Interfaces;
using System;

namespace QPlay.Trading.Service.Models.Entities;

public class CatalogItem : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}