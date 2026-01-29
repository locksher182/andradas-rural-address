using RuralAddress.Mobile.ViewModels;

namespace RuralAddress.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
