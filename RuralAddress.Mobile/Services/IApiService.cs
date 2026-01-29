using RuralAddress.Mobile.Models;
using System.Threading.Tasks;

namespace RuralAddress.Mobile.Services
{
    public interface IApiService
    {
        Task<MobileLoginResponse> LoginAsync(string cpf, string senha);
        Task<bool> SendPanicAlertAsync(int pessoaId, double latitude, double longitude);
        Task<bool> SendChatAsync(int pessoaId, string message);
        void UpdateBaseUrl(string newUrl);
        string GetCurrentBaseUrl();
    }
}
