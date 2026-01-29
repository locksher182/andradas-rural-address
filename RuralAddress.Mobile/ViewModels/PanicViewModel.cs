using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using RuralAddress.Mobile.Services;
using System.Collections.ObjectModel;

namespace RuralAddress.Mobile.ViewModels
{
    public partial class PanicViewModel : ObservableObject
    {
        private readonly IApiService _apiService;
        private readonly IGpsService _gpsService;
        private HubConnection? _hubConnection;

        // State
        [ObservableProperty]
        private bool isCountingDown;
        
        [ObservableProperty]
        private int countdownSeconds;

        [ObservableProperty]
        private bool isAlertActive; // True after countdown finishes (Chat Mode)

        [ObservableProperty]
        private string gpsStatusMessage = "Verificando GPS...";

        [ObservableProperty]
        private bool isGpsEnabled;

        // Chat
        public ObservableCollection<ChatMessage> ChatMessages { get; } = new();
        
        [ObservableProperty]
        private string newMessage = "";

        public PanicViewModel(IApiService apiService, IGpsService gpsService)
        {
            _apiService = apiService;
            _gpsService = gpsService;
            CheckGpsAsync();
        }

        public async Task InitializeAsync()
        {
            await CheckGpsAsync();
        }

        private async Task CheckGpsAsync()
        {
            IsGpsEnabled = await _gpsService.CheckPermissionsAsync();
            if (!IsGpsEnabled)
            {
                GpsStatusMessage = "ALERTA: GPS Desativado! Ative para usar o botão.";
                await Application.Current.MainPage.DisplayAlert("GPS Desativado", "O GPS é necessário para o Botão do Pânico.", "OK");
            }
            else
            {
                GpsStatusMessage = "GPS Ativo e Pronto.";
            }
        }

        [ObservableProperty]
        private bool isPanicError;

        [RelayCommand]
        public async Task StartPanicAsync()
        {
            if (IsCountingDown || IsAlertActive) return;
            
            if (!IsGpsEnabled)
            {
                await CheckGpsAsync();
                if (!IsGpsEnabled) return;
            }

            IsCountingDown = true;
            CountdownSeconds = 5;

            // Timer Loop
            while (CountdownSeconds > 0 && IsCountingDown)
            {
                await Task.Delay(1000);
                if (!IsCountingDown) break; // Cancelled
                CountdownSeconds--;
            }

            if (IsCountingDown) // Finished successfully
            {
                IsCountingDown = false;
                await TriggerPanicAction();
            }
        }

        [RelayCommand]
        public void CancelPanic()
        {
            IsCountingDown = false;
            CountdownSeconds = 0;
        }

        [RelayCommand]
        public async Task Logout()
        {
             Preferences.Remove("Token");
             Preferences.Remove("PessoaId");
             Preferences.Remove("Nome");
             await Shell.Current.GoToAsync("//Login");
        }

        [RelayCommand]
        public void CloseApp()
        {
            Application.Current.Quit();
        }

        [RelayCommand]
        public async Task OpenWhatsApp()
        {
            try
            {
                await Launcher.OpenAsync("https://wa.me/5535997460384");
            }
            catch {}
        }

        private async Task TriggerPanicAction()
        {
            var location = await _gpsService.GetCurrentLocationAsync();
            var lat = location?.Latitude ?? 0;
            var lon = location?.Longitude ?? 0;
            var pessoaId = Preferences.Get("PessoaId", 0);

            // 1. Send API Alert
            var success = await _apiService.SendPanicAlertAsync(pessoaId, lat, lon);

            if (success)
            {
                 IsAlertActive = true; 
                 IsPanicError = false;

                 // 2. Connect SignalR for Chat (Only if success)
                 await ConnectSignalR(pessoaId);
            
                 // Add system message
                 ChatMessages.Add(new ChatMessage("Sistema", "Alerta enviado. Aguarde contato."));
            }
            else
            {
                IsAlertActive = false;
                IsPanicError = true;
            }
        }

        private async Task ConnectSignalR(int pessoaId)
        {
            var baseUrl = _apiService.GetCurrentBaseUrl();
            if (baseUrl.EndsWith("/")) baseUrl = baseUrl.TrimEnd('/');
            var hubUrl = $"{baseUrl}/panichub";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string, int>("ReceiveChatMessage", (sender, message, pId) =>
            {
                // Simple filter (should be improved with Groups)
                if (pId == pessoaId)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ChatMessages.Add(new ChatMessage(sender, message));
                    });
                }
            });

            try 
            {
                await _hubConnection.StartAsync();
            } 
            catch (Exception ex) 
            {
                 ChatMessages.Add(new ChatMessage("Sistema", $"Erro chat (LP): {ex.Message}"));
            }
        }

        public string AppVersion => $"v{AppInfo.VersionString} (Build {AppInfo.BuildString})";

        [RelayCommand]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(NewMessage)) return;

            var pessoaId = Preferences.Get("PessoaId", 0);
            var nome = Preferences.Get("Nome", "Eu");

            var success = await _apiService.SendChatAsync(pessoaId, NewMessage);

            if (success)
            {
                ChatMessages.Add(new ChatMessage(nome, NewMessage));
                NewMessage = "";
            }
        }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string DisplayText => $"{Sender}: {Content}";

        public ChatMessage(string sender, string content)
        {
            Sender = sender;
            Content = content;
        }
    }
}
