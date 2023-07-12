using System;

namespace QPlay.Trading.Service.Contracts;

public record PurchaseRequested(Guid UserId, Guid ItemId, int Quantity, Guid CorrelationId);