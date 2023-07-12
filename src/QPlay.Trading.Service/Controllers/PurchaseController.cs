using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPlay.Trading.Service.Contracts;
using QPlay.Trading.Service.Extensions;
using QPlay.Trading.Service.Models.Dtos;
using QPlay.Trading.Service.StateMachines;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Controllers;

[ApiController]
[Route("purchase")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPublishEndpoint publishEndpoint;
    private readonly IRequestClient<GetPurchaseState> purchaseClient;

    public PurchaseController(
        IPublishEndpoint publishEndpoint,
        IRequestClient<GetPurchaseState> purchaseClient
    )
    {
        this.publishEndpoint = publishEndpoint;
        this.purchaseClient = purchaseClient;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] SubmitPurchaseDto purchase)
    {
        string userId = User.FindFirstValue("sub");

        PurchaseRequested message =
            new(
                Guid.Parse(userId),
                purchase.ItemId.Value,
                purchase.Quantity,
                purchase.IdempotencyId.Value
            );

        await publishEndpoint.Publish(message);

        return AcceptedAtAction(
            nameof(GetStatusAsync),
            new { purchase.IdempotencyId },
            new { purchase.IdempotencyId }
        );
    }

    [HttpGet("status/{idempotencyId}")]
    public async Task<ActionResult<PurchaseDto>> GetStatusAsync([FromRoute] Guid idempotencyId)
    {
        Response<PurchaseState> response = await purchaseClient.GetResponse<PurchaseState>(
            new GetPurchaseState(idempotencyId)
        );

        PurchaseState purchaseState = response.Message;

        PurchaseDto purchase = purchaseState.AsDto();

        return Ok(purchase);
    }
}