using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuralAddress.Mobile.Services;

namespace RuralAddress.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        private string cpf = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool isLoading;

        public bool IsNotLoading => !IsLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public string AppVersion => $"Ver: {AppInfo.VersionString}.{AppInfo.BuildString}";

        [ObservableProperty]
        private string baseUrl = string.Empty;

        [ObservableProperty]
        private bool isDebugVisible;

        public LoginViewModel(IApiService apiService)
        {
            _apiService = apiService;
            BaseUrl = _apiService.GetCurrentBaseUrl();
        }

        [RelayCommand]
        public void ToggleDebug()
        {
            IsDebugVisible = !IsDebugVisible;
        }

        [RelayCommand]
        public void UpdateUrl()
        {
            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                _apiService.UpdateBaseUrl(BaseUrl);
                // Feedback? 
                ErrorMessage = "URL atualizada. Tente o login novamente.";
            }
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Cpf))
            {
                ErrorMessage = "CPF é obrigatório";
                return;
            }

            // Client-side password validation disabled
            if (string.IsNullOrWhiteSpace(Senha))
            {
                ErrorMessage = "Senha é obrigatória";
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var result = await _apiService.LoginAsync(Cpf, Senha);

                if (result.Success)
                {
                    // Store Token/User details in Preferences for session (simplified)
                    Preferences.Set("PessoaId", result.PessoaId);
                    Preferences.Set("Token", result.Token);
                    Preferences.Set("Nome", result.Nome);

                    await Shell.Current.GoToAsync("//Panic");
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
