using Microsoft.Extensions.Logging;
using RuralAddress.Mobile.Services;
using RuralAddress.Mobile.ViewModels;
using RuralAddress.Mobile.Views;

namespace RuralAddress.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            
            // Services
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<IGpsService, GpsService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<PanicViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<PanicPage>();

            return builder.Build();
        }
    }
}
