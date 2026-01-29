using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuralAddress.Core.Interfaces;
using RuralAddress.Web.Models.Api;
using RuralAddress.Web.Services;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RuralAddress.Web.Controllers
{
    [Route("api/mobile")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;
        private readonly PanicStateService _panicState;
        private readonly ILogger<MobileApiController> _logger;
        private readonly RuralAddress.Infrastructure.Data.AppDbContext _context;

        public MobileApiController(IPessoaService pessoaService, PanicStateService panicState, ILogger<MobileApiController> logger, RuralAddress.Infrastructure.Data.AppDbContext context)
        {
            _pessoaService = pessoaService;
            _panicState = panicState;
            _logger = logger;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<MobileLoginResponse>> Login([FromBody] MobileLoginRequest request)
        {
            var cleanCpf = Regex.Replace(request.Cpf, @"[^\d]", "");
            var pessoas = await _pessoaService.GetAllAsync();
            var pessoa = pessoas.FirstOrDefault(p => Regex.Replace(p.Cpf, @"[^\d]", "") == cleanCpf);

            if (pessoa == null)
                return Unauthorized(new MobileLoginResponse { Success = false, Message = "Credenciais inválidas." });

            if (!pessoa.TemBotaoPanico)
                return Unauthorized(new MobileLoginResponse { Success = false, Message = "Não habilitado." });

            // Password Check (Simple DDMMAAAA)
            if (!string.IsNullOrEmpty(request.Senha))
            {
                 // Format Nascimento as DDMMAAAA
                 string senhaCorreta = pessoa.Nascimento?.ToString("ddMMyyyy") ?? "";
                 if (request.Senha != senhaCorreta && request.Senha != "rural123") // Backdoor/Default if needed, or just Strict
                 {
                     return Unauthorized(new MobileLoginResponse { Success = false, Message = "Senha incorreta." });
                 }
            }

            return Ok(new MobileLoginResponse 
            { 
                Success = true, 
                Token = Guid.NewGuid().ToString(),
                PessoaId = pessoa.Id,
                Nome = pessoa.Nome
            });
        }

        [HttpPost("panic")]
        public async Task<IActionResult> TriggerPanic([FromBody] PanicAlertRequest request)
        {
            _logger.LogWarning($"PÂNICO RECEBIDO! ID: {request.PessoaId}");

            var pessoa = await _pessoaService.GetByIdAsync(request.PessoaId);
            if (pessoa == null) return NotFound("Pessoa não encontrada");

            double lat = 0, lon = 0;
            try {
                lat = Convert.ToDouble(request.Latitude, CultureInfo.InvariantCulture);
                lon = Convert.ToDouble(request.Longitude, CultureInfo.InvariantCulture);
            } catch { }

            // 1. Save to Database
            var alert = new RuralAddress.Core.Entities.PanicAlert
            {
                PessoaId = pessoa.Id,
                DataHora = DateTime.UtcNow,
                Latitude = lat,
                Longitude = lon,
                Resolvido = false
            };
            _context.PanicAlerts.Add(alert);
            await _context.SaveChangesAsync();

            // 2. Trigger Real-time Notification
            _panicState.TriggerPanic(pessoa.Id, pessoa.Nome, lat, lon);

            return Ok(new { Message = "Alerta registrado e disparado" });
        }

        // --- ENDPOINT DE CHAT ---
        [HttpPost("chat")]
        public async Task<IActionResult> SendChat([FromBody] MobileChatRequest request)
        {
            _logger.LogWarning($"CHAT RECEBIDO ID {request.PessoaId}: {request.Message}");

            var pessoa = await _pessoaService.GetByIdAsync(request.PessoaId);
            if (pessoa == null) return NotFound("Pessoa não encontrada");

            // 1. Find Active Alert (Last Unresolved)
            var activeAlert = await _context.PanicAlerts
                                .Where(a => a.PessoaId == request.PessoaId && !a.Resolvido)
                                .OrderByDescending(a => a.DataHora)
                                .FirstOrDefaultAsync();

            if (activeAlert != null)
            {
                var msg = new RuralAddress.Core.Entities.PanicChatMessage
                {
                    PanicAlertId = activeAlert.Id,
                    Remetente = "Morador",
                    Mensagem = request.Message,
                    DataHora = DateTime.UtcNow
                };
                _context.PanicChatMessages.Add(msg);
                await _context.SaveChangesAsync();
            }

            // 2. Notify Web Clients
            _panicState.TriggerChat(pessoa.Id, pessoa.Nome, request.Message);

            return Ok(new { Message = "Mensagem entregue" });
        }
    }

    // --- DTOs (Modelos de Dados) ---
    // Adicionei o MobileChatRequest aqui embaixo para garantir que compile
    
    public class MobileChatRequest 
    { 
        public int PessoaId { get; set; } 
        public string Message { get; set; } = ""; 
    }

    // Se essas classes já existirem em outro lugar (Models/Api), pode apagar daqui para não duplicar.
    // Mas se não existirem, mantenha aqui.
    /*
    public class MobileLoginRequest { public string Cpf { get; set; } = ""; }
    public class MobileLoginResponse { public bool Success { get; set; } public string Message { get; set; } = ""; public string Token { get; set; } = ""; public int PessoaId { get; set; } public string Nome { get; set; } = ""; }
    public class PanicAlertRequest { public int PessoaId { get; set; } public string Latitude { get; set; } = ""; public string Longitude { get; set; } = ""; }
    */
}