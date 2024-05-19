using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class OrderHub : Hub
{
    public async Task SendOrderUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveOrderUpdate", message);
    }
}
