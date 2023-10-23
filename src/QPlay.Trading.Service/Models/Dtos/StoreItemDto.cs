using System;

namespace QPlay.Trading.Service.Models.Dtos;

public record StoreItemDto
(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int OwnedQuantity
);