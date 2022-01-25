using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.API.Hubs
{
    public class MyHub:Hub
    {
        // static olarak tutarsam benim api uygulamam ayakta kaldığı sürece listenin içine eklenir.
        private static List<string> Names { get; set; } = new List<string>();

        private static int ClientCount { get; set; } = 0;


        public static int TeamCount { get; set; } = 7;

        // clientlar sendmessage'a istek yapacaklar ve  message isminde bir parametre gönderecekler 
        // daha sonra bu metod çalıştığı zaman clientlar üzerindeki receivemessage' a bildiri göndereceğim.
        // clientlar üzerindeki şu metod çalışsın ve bu mesaj da gitsin. SendAsync("ReceiveMessage", message);
        // clientlar ReceiveMessage bu metoda bağlanmışsa metodlar çalışacak. bağlanmamışsa mesajları serverden almayacak.
        // 100 client 100 tane istek yaparsa MyHubdan 100 tane nesne örneği oluşur.
        public async Task SendName(string name)
        {
            if (Names.Count >= TeamCount)
            {
                await Clients.Caller.SendAsync("Error", $"Takım en fazla {TeamCount} kişi olabilir");
            }
            else
            {
                Names.Add(name);
                // clientlerdaki metodların çalışması için bir istek göndereceğim.
                // Clientlarda bu metod tanımlıysa çalışacak 
                // ALl Propertyisi benim bu huba bağlı olan tüm clientlara bildiri gönderir.
                await Clients.All.SendAsync("ReceiveName", name);
            }
           
        }

        // Clientlar o anda memoryde olan namelerin tamamını alsınlar
        public async Task GetNames()
        {
            await Clients.All.SendAsync("ReceiveNames",Names);
        }

        public async override Task OnConnectedAsync()
        {
            ClientCount++;

            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            ClientCount--;

            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
