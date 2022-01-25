using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.API.Models;

namespace SignalR.API.Hubs
{
    public class MyHub:Hub
    {
        private readonly AppDbContext _context;

        public MyHub(AppDbContext context)
        {
            _context = context;
        }

        // static olarak tutarsam benim api uygulamam ayakta kaldığı sürece listenin içine eklenir.
        private static List<string> Names { get; set; } = new List<string>();

        private static int ClientCount { get; set; } = 0;

        public static int TeamCount { get; set; } = 7;

        public async Task SendProduct(Product p)
        {
            await Clients.All.SendAsync("ReceiveProduct", p);

        }

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

        // Groups
        // Gruba ekleme
        public async Task AddToGroup(string teamName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
        }

        // Gruptan silme
        public async Task RemoveToGroup(string teamName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
        }

        // Gruba isim gönderme
        public async Task SendNameByGroup(string Name, string teamName)
        {
            var team = _context.Teams.FirstOrDefault(x => x.Name == teamName);


            if (team != null)
            {
                team.Users.Add(new User(){Name = Name});

            }
            else
            {
                var newTeam = new Team() {Name = teamName};

                newTeam.Users.Add(new User(){Name = Name});

                _context.Teams.Add(newTeam);
            }

            await _context.SaveChangesAsync();

            await Clients.Group(teamName).SendAsync("ReceiveMessageByGroup",Name,team.Id);

        }

        // Gruptaki tüm isimleri alma
        public async Task GetNamesByGroup()
        {
            var teams = _context.Teams.Include(x => x.Users).Select(x => new
            {
                teamId = x.Id,
                Users = x.Users.ToList()
            });

            await Clients.All.SendAsync("ReceiveNamesByGroup",teams);
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
