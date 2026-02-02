using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RuralAddress.Web.Hubs
{
    public class PanicHub : Hub
    {
        private readonly RuralAddress.Infrastructure.Data.AppDbContext _context;

        public PanicHub(RuralAddress.Infrastructure.Data.AppDbContext context)
        {
            _context = context;
        }

        // Mobile sends alert -> Hub -> Web Clients (Admins)
        public async Task SendPanicAlert(int pessoaId, string nome, double lat, double lon)
        {
            await Clients.All.SendAsync("ReceivePanicAlert", pessoaId, nome, lat, lon);
        }

        // Chat: Clients send message -> Hub -> All in session (or Group)
        public async Task SendChatMessage(string sender, string message, int pessoaId)
        {
             // 1. Save to DB (Admin/System messages)
             // Find Active Alert
            var activeAlert = await _context.PanicAlerts
                                .Where(a => a.PessoaId == pessoaId && !a.Resolvido)
                                .OrderByDescending(a => a.DataHora)
                                .FirstOrDefaultAsync();

            if (activeAlert != null)
            {
                var msg = new RuralAddress.Core.Entities.PanicChatMessage
                {
                    PanicAlertId = activeAlert.Id,
                    Remetente = sender, // "Admin" or Name
                    Mensagem = message,
                    DataHora = DateTime.UtcNow
                };
                _context.PanicChatMessages.Add(msg);
                await _context.SaveChangesAsync();
            }

            await Clients.All.SendAsync("ReceiveChatMessage", sender, message, pessoaId);
        }
    }
}
