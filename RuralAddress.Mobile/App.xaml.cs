namespace RuralAddress.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var token = Preferences.Get("Token", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                MainPage = new AppShell();
                Shell.Current.GoToAsync("//Panic");
            }
            else
            {
                MainPage = new AppShell();
            }
        }
    }
}
