using cmb.logistics.Services;

namespace cmb.logistics.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _auth;

    public LoginPage(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        System.Diagnostics.Debug.WriteLine("[LoginPage] Initialized");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "";
        var user = UserEntry.Text ?? string.Empty;
        System.Diagnostics.Debug.WriteLine($"[LoginPage] Login button clicked. User='{user}'");
        var ok = await _auth.LoginAsync(user, PassEntry.Text ?? "");
        System.Diagnostics.Debug.WriteLine($"[LoginPage] Login result for '{user}': {ok}");
        StatusLabel.Text = ok ? "Connecté." : "Échec de connexion.";
        if (ok)
        {
            System.Diagnostics.Debug.WriteLine("[LoginPage] Navigating to IntakePage");
            await Shell.Current.GoToAsync($"//{nameof(Pages.IntakePage)}");
        }
    }
}