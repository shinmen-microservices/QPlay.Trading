using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QPlay.Trading.Service.StateMachines;

namespace QPlay.Trading.Service.SignalR;

[Authorize]
public class MessageHub : Hub
{
    public async Task SendStatusAsync(PurchaseState status)
    {
        if (Clients != null)
        {
            await Clients.User(Context.UserIdentifier).SendAsync("ReceivePurchaseStatus", status);
        }
    }
}