using System;

namespace QPlay.Trading.Service.Models.Dtos;

public record PurchaseDto
(
    Guid UserId,
    Guid ItemId,
    decimal? PurchaseTotal,
    int Quantity,
    string State,
    string Reason,
    DateTimeOffset Received,
    DateTimeOffset LastUpdated
);