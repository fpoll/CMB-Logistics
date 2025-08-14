namespace cmb.logistics.Services;

public class AuthServiceStub : IAuthService
{
    public string? CurrentUserId { get; private set; }

    public Task<bool> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var ok = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
        CurrentUserId = ok ? username.Trim() : null;
        System.Diagnostics.Debug.WriteLine($"[AuthServiceStub] Login attempt for user='{username}', success={ok}");
        return Task.FromResult(ok);
    }
}
