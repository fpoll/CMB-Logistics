namespace cmb.logistics;

public partial class AppShell : Shell
{
    public AppShell()
    {
        System.Diagnostics.Debug.WriteLine("[AppShell] Constructing AppShell and registering routes…");
        InitializeComponent();
        Routing.RegisterRoute(nameof(Pages.IntakePage), typeof(Pages.IntakePage));
        Routing.RegisterRoute(nameof(Pages.LoginPage), typeof(Pages.LoginPage));
        System.Diagnostics.Debug.WriteLine("[AppShell] Routes registered: IntakePage, LoginPage");
    }
}
