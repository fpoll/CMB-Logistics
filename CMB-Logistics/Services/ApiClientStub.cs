using System.Text.Json;
using cmb.logistics.Models;

namespace cmb.logistics.Services;

/// <summary>
/// Temporary stub that simulates sending data to your backend.
/// Replace with real HttpClient calls when your API is ready.
/// </summary>
public class ApiClientStub : IApiClient
{
    public Task<ApiResult> SubmitIntakeAsync(IntakeSession session, CancellationToken ct = default)
    {
        System.Diagnostics.Debug.WriteLine("[ApiClientStub] SubmitIntakeAsync called. CompetitionId=" + session.CompetitionId + ", BottleNumber=" + session.BottleNumber + ", ExtraCodes=" + session.ExtraCodes.Count + ", HasPhoto=" + (session.Photo != null));
        // Simulate latency
        return Task.Run(async () =>
        {
            await Task.Delay(500, ct);
            var payload = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
            System.Diagnostics.Debug.WriteLine("[ApiClientStub] Submitting payload:\n" + payload);
            return new ApiResult { Success = true, Message = "Stub accepted payload" };
        }, ct);
    }
}
