using cmb.logistics.Models;
using cmb.logistics.Services;
using System.Text.RegularExpressions;
using ZXing.Net.Maui;

namespace cmb.logistics.Pages;

public partial class IntakePage : ContentPage
{
    private readonly IApiClient _api;
    private readonly IAuthService _auth;
    private readonly IntakeSession _session = new();
    private static readonly Regex CmbQrRegex = new(@"^20\d{10}$", RegexOptions.Compiled);

    public IntakePage(IApiClient api, IAuthService auth)
    {
        InitializeComponent();
        _api = api;
        _auth = auth;
        System.Diagnostics.Debug.WriteLine("[IntakePage] Initialized");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("[IntakePage] OnAppearing – enabling QR detection");
        if (QrReader != null) QrReader.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        System.Diagnostics.Debug.WriteLine("[IntakePage] OnDisappearing – disabling QR detection");
        if (QrReader != null) QrReader.IsDetecting = false;
        base.OnDisappearing();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (e?.Results == null || e.Results.Count() == 0)
        {
            System.Diagnostics.Debug.WriteLine("[IntakePage] BarcodesDetected – no results");
            return;
        }

        var first = e.Results.FirstOrDefault();
        if (first is null) return;

        var text = first.Value?.Trim();
        System.Diagnostics.Debug.WriteLine($"[IntakePage] First barcode format={first.Format}, value='{text}'");
        if (string.IsNullOrWhiteSpace(text)) return;

        // Fixed format (12 digits): '20' + 5 digits competition id + 5 digits bottle number
        // Example: 200013406357 => CompetitionId=00134, BottleNumber=06357 (keep leading zeros)
        if (CmbQrRegex.IsMatch(text))
        {
            var compDigits = text.Substring(2, 5);
            var bottleDigits = text.Substring(7, 5);
            _session.CompetitionId = compDigits;   // keep leading zeros
            _session.BottleNumber = bottleDigits; // keep leading zeros

            MainThread.BeginInvokeOnMainThread(() =>
            {
                QrResultLabel.Text = $"QR OK: {text} ? Concours={_session.CompetitionId}, Bouteille={_session.BottleNumber}";
            });

            if (QrReader != null) QrReader.IsDetecting = false; // stop after first valid scan
            System.Diagnostics.Debug.WriteLine($"[IntakePage] Parsed from '{text}': comp='{_session.CompetitionId}', bottle='{_session.BottleNumber}'");
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                QrResultLabel.Text = $"QR invalide (attendu 12 chiffres commençant par '20'): {text}";
            });
            System.Diagnostics.Debug.WriteLine($"[IntakePage] Invalid QR format: '{text}'");
        }
    }

    private async void OnTakePhoto(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[IntakePage] OnTakePhoto – requested");
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                System.Diagnostics.Debug.WriteLine("[IntakePage] MediaPicker capture NOT supported on this device");
                await DisplayAlert("Erreur", "La capture photo n'est pas supportée sur cet appareil.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = $"cmb_two_bottles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg"
            });
            if (photo is null)
            {
                System.Diagnostics.Debug.WriteLine("[IntakePage] CapturePhotoAsync returned null (cancelled?)");
                return;
            }

            var localFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            System.Diagnostics.Debug.WriteLine($"[IntakePage] Saving photo to '{localFile}'");
            await using var src = await photo.OpenReadAsync();
            await using var dst = File.OpenWrite(localFile);
            await src.CopyToAsync(dst);

            Preview.Source = ImageSource.FromFile(localFile);
            _session.Photo = new BottlePhoto(localFile, DateTime.UtcNow);
            System.Diagnostics.Debug.WriteLine($"[IntakePage] Photo saved and preview updated. HasPhoto={_session.Photo != null}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[IntakePage] OnTakePhoto ERROR: " + ex);
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void OnScanExtra(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[IntakePage] OnScanExtra – start extra scanning");
        if (QrReader is null)
        {
            System.Diagnostics.Debug.WriteLine("[IntakePage] QrReader is null (OnScanExtra)");
            await DisplayAlert("Erreur", "Scanner indisponible.", "OK");
            return;
        }

        QrReader.IsDetecting = true;

        void Handler(object? s, BarcodeDetectionEventArgs args)
        {
            foreach (var code in args.Results)
            {
                _session.ExtraCodes.Add(new ExtraCode(code.Format.ToString(), code.Value ?? ""));
                System.Diagnostics.Debug.WriteLine($"[IntakePage] Extra code captured: format={code.Format}, value='{code.Value}'");
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ExtraLabel.Text = $"Codes supplémentaires: {_session.ExtraCodes.Count}";
            });
        }

        QrReader.BarcodesDetected += Handler;
        await DisplayAlert("Scan", "Présentez les codes (EAN13/QR). Appuyez sur OK quand terminé.", "OK");
        QrReader.BarcodesDetected -= Handler;
        QrReader.IsDetecting = false;
        System.Diagnostics.Debug.WriteLine($"[IntakePage] OnScanExtra – stop. Total extra codes={_session.ExtraCodes.Count}");
    }

    private void OnNoExtra(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[IntakePage] OnNoExtra – clearing extra codes");
        _session.ExtraCodes.Clear();
        ExtraLabel.Text = "Aucun code à scanner";
    }

    private async void OnSubmit(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[IntakePage] OnSubmit – validating session before submit");
        if (_session.Photo is null || string.IsNullOrWhiteSpace(_session.CompetitionId) || string.IsNullOrWhiteSpace(_session.BottleNumber))
        {
            System.Diagnostics.Debug.WriteLine($"[IntakePage] Missing info: HasPhoto={_session.Photo != null}, CompetitionId='{_session.CompetitionId}', BottleNumber='{_session.BottleNumber}'");
            await DisplayAlert("Manque d'informations", "Assurez-vous d'avoir scanné le QR et pris la photo.", "OK");
            return;
        }

        _session.UserId = _auth.CurrentUserId ?? "anonymous";
        System.Diagnostics.Debug.WriteLine($"[IntakePage] Submitting. UserId='{_session.UserId}', ExtraCodes={_session.ExtraCodes.Count}, PhotoPath='{_session.Photo?.LocalPath}'");

        SubmitStatus.Text = "Envoi en cours…";
        var result = await _api.SubmitIntakeAsync(_session);
        System.Diagnostics.Debug.WriteLine($"[IntakePage] Submit result: Success={result.Success}, Message='{result.Message}'");
        SubmitStatus.Text = result.Success ? "Envoyé (stub)." : $"Échec: {result.Message}";
    }
}