using System;
using System.ComponentModel.DataAnnotations;

namespace QPlay.Trading.Service.Models.Dtos;

public record SubmitPurchaseDto
(
    [Required] Guid? ItemId,
    [Range(1, 100)] int Quantity,
    [Required] Guid? IdempotencyId
);