using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Web.Hubs
{
    public class MyHub:Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage",message);
        }
    }
}
