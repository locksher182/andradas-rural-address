
namespace RuralAddress.Mobile.Services
{
    public interface IGpsService
    {
        Task<Location?> GetCurrentLocationAsync();
        Task<bool> CheckPermissionsAsync();
    }
}
