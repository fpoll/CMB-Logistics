using cmb.logistics.Models;

namespace cmb.logistics.Services;

public interface IApiClient
{
    Task<ApiResult> SubmitIntakeAsync(IntakeSession session, CancellationToken ct = default);
}