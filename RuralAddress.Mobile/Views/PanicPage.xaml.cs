using RuralAddress.Mobile.ViewModels;

namespace RuralAddress.Mobile.Views
{
    public partial class PanicPage : ContentPage
    {
        private readonly PanicViewModel _vm;

        public PanicPage(PanicViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
        }
    }
}
