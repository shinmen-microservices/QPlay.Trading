using QPlay.Trading.Service.Models.Dtos;
using QPlay.Trading.Service.StateMachines;

namespace QPlay.Trading.Service.Extensions;

public static class DtoExtension
{
    public static PurchaseDto AsDto(this PurchaseState purchaseState)
    {
        return new(
            purchaseState.UserId,
            purchaseState.ItemId,
            purchaseState.PurchaseTotal,
            purchaseState.Quantity,
            purchaseState.CurrentState,
            purchaseState.ErrorMessage,
            purchaseState.Received,
            purchaseState.LastUpdated
        );
    }
}