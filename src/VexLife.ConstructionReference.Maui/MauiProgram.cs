using CommunityToolkit.Maui;
using Microsoft.Maui.Hosting;

namespace VexLife.ConstructionReference.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement(false);

        return builder.Build();
    }
}
