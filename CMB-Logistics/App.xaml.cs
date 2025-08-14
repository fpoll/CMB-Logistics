namespace cmb.logistics;

public partial class App : Application
{
    public App()
    {
        System.Diagnostics.Debug.WriteLine("[App] Initializing application…");
        InitializeComponent();
        MainPage = new AppShell();
        System.Diagnostics.Debug.WriteLine("[App] MainPage assigned to AppShell");
    }
}