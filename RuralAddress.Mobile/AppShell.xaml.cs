using RuralAddress.Mobile.Views;

namespace RuralAddress.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(PanicPage), typeof(PanicPage));
        }
    }
}
