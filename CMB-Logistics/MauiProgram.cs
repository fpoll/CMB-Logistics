using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace cmb.logistics;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        System.Diagnostics.Debug.WriteLine("[MauiProgram] CreateMauiApp starting. OS=" + Environment.OSVersion + ", Is64Bit=" + Environment.Is64BitProcess);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DI registrations
        builder.Services.AddSingleton<Services.IApiClient, Services.ApiClientStub>();
        builder.Services.AddSingleton<Services.IAuthService, Services.AuthServiceStub>();

        builder.Services.AddSingleton<Pages.IntakePage>();
        builder.Services.AddSingleton<Pages.LoginPage>();

#if DEBUG
        // Optional debug logging provider. If this triggers CS1061 on your setup, leave it commented out.
        // builder.Logging.AddDebug();
        System.Diagnostics.Debug.WriteLine("[MauiProgram] DEBUG logging enabled");
#endif

        var app = builder.Build();
        System.Diagnostics.Debug.WriteLine("[MauiProgram] CreateMauiApp completed.");
        return app;
    }
} 