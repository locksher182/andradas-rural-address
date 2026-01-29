using RuralAddress.Mobile.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RuralAddress.Mobile.Services
{
    public class ApiService : IApiService
    {
        private HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // Mantendo seu IP atual
        private const string DefaultBaseUrl = "http://147.93.68.244"; 
        private const string BaseUrlKey = "ApiBaseUrl";

        public ApiService()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            InitializeClient();
        }

        private void InitializeClient()
        {
            _httpClient = new HttpClient();
            var url = Preferences.Get(BaseUrlKey, DefaultBaseUrl);
            
            if (string.IsNullOrEmpty(url)) url = DefaultBaseUrl;
            
            // Garante que a URL termine com / para evitar erros de concatenação
            if (!url.EndsWith("/")) url += "/";

            try 
            {
                _httpClient.BaseAddress = new Uri(url);
                _httpClient.Timeout = TimeSpan.FromSeconds(15);
                Console.WriteLine($"[MOBILE INIT] Cliente HTTP iniciado. Alvo: {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MOBILE ERROR] Erro ao iniciar HTTP: {ex.Message}");
                _httpClient.BaseAddress = new Uri(DefaultBaseUrl);
            }
        }

        public void UpdateBaseUrl(string newUrl)
        {
            if (Uri.TryCreate(newUrl, UriKind.Absolute, out _))
            {
                Preferences.Set(BaseUrlKey, newUrl);
                InitializeClient();
            }
        }

        public string GetCurrentBaseUrl() => Preferences.Get(BaseUrlKey, DefaultBaseUrl);

        public async Task<MobileLoginResponse> LoginAsync(string cpf, string senha)
        {
            try
            {
                Console.WriteLine($"[MOBILE LOGIN] Tentando logar CPF: {cpf} na URL {_httpClient.BaseAddress}api/mobile/login");
                
                var request = new MobileLoginRequest { Cpf = cpf, Senha = senha };
                var response = await _httpClient.PostAsJsonAsync("api/mobile/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MobileLoginResponse>(_jsonOptions) 
                           ?? new MobileLoginResponse { Success = false, Message = "Resposta vazia do servidor" };
                }
                
                // Tenta ler erro do servidor
                string serverMsg = $"Erro HTTP: {response.StatusCode}";
                try {
                     var err = await response.Content.ReadFromJsonAsync<MobileLoginResponse>(_jsonOptions);
                     if (err != null && !string.IsNullOrEmpty(err.Message)) serverMsg = err.Message;
                } catch {}

                Console.WriteLine($"[MOBILE LOGIN FAIL] {serverMsg}");

                // Se for erro de servidor/conexão (não autorizado é exceção, pois é credencial errada)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                     return new MobileLoginResponse { Success = false, Message = "CPF ou Senha inválidos." };
                }

                return new MobileLoginResponse 
                { 
                    Success = false, 
                    Message = "Não foi possível acessar o sistema.\nVerifique sua conexão ou entre em contato via 153 ou WhatsApp (35) 99746-0384." 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MOBILE LOGIN EXCEPTION] {ex.Message}");
                return new MobileLoginResponse 
                { 
                    Success = false, 
                    Message = "Não foi possível acessar o sistema.\nVerifique sua conexão ou entre em contato via 153 ou WhatsApp (35) 99746-0384." 
                };
            }
        }

        public async Task<bool> SendPanicAlertAsync(int pessoaId, double latitude, double longitude)
        {
            try
            {
                var request = new PanicAlertRequest 
                { 
                    PessoaId = pessoaId, 
                    Latitude = latitude, // Envia como double, o servidor trata
                    Longitude = longitude 
                };

                // Log para debug
                Console.WriteLine($"[MOBILE PANIC] Enviando Pânico ID {pessoaId}...");

                var response = await _httpClient.PostAsJsonAsync("api/mobile/panic", request);
                
                if(!response.IsSuccessStatusCode)
                {
                     Console.WriteLine($"[MOBILE PANIC FAIL] Status: {response.StatusCode}");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MOBILE PANIC EXCEPTION] {ex.Message}");
                return false;
            }
        }

        // --- O MÉTODO DO CHAT (AGORA COM LOGS) ---
        public async Task<bool> SendChatAsync(int pessoaId, string message)
        {
            try
            {
                // Console.WriteLine($"[MOBILE CHAT] Iniciando envio... ID={pessoaId}");

                var request = new MobileChatRequest { PessoaId = pessoaId, Message = message };
                var response = await _httpClient.PostAsJsonAsync("api/mobile/chat", request);
                
                if (response.IsSuccessStatusCode)
                {
                    // Sucesso silencioso ou UI controlada pelo ViewModel
                    return true;
                }
                else
                {
                    // Falha no servidor (500, 502, 404, etc)
                    var statusCode = (int)response.StatusCode;
                    // Log interno apenas
                    Console.WriteLine($"[MOBILE CHAT FAIL] Erro: {statusCode}");
                    
                    // Se for 502 (Bad Gateway), geralmente é o servidor down/reiniciando
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MOBILE CHAT EXCEPTION] {ex.Message}");
                return false;
            }
        }
    }
}