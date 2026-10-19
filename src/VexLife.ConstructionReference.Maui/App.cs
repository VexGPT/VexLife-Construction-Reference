using Microsoft.Maui.Controls;
using VexLife.ConstructionReference.Maui.Playback;

namespace VexLife.ConstructionReference.Maui;

public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new VexStreamPlaybackPage());
}
