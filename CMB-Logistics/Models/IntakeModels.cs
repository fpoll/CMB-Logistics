namespace cmb.logistics.Models;

public record BottlePhoto(string LocalPath, DateTime CapturedAtUtc);

public record ExtraCode(string Symbology, string Value);

public class IntakeSession
{
    public string? CompetitionId { get; set; }      // from QR
    public string? BottleNumber { get; set; }       // from QR
    public List<ExtraCode> ExtraCodes { get; } = new();
    public BottlePhoto? Photo { get; set; }         // two bottles side by side
    public string? UserId { get; set; }             // filled after login later
}

public class ApiResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}