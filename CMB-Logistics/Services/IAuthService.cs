namespace cmb.logistics.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password, CancellationToken ct = default);
    string? CurrentUserId { get; }
}