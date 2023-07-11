using QPlay.Common.Entities.Interfaces;
using System;

namespace QPlay.Trading.Service.Models.Entities;

public class ApplicationUser : IEntity
{
    public Guid Id { get; set; }
    public decimal Gil { get; set; }
}