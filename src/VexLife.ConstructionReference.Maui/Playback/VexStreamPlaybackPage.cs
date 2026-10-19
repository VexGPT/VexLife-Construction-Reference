// [VXG RealForever]
// First native target surface: semantic Projection drives the affordance; MediaElement remains provider realization.

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using VexLife.ConstructionReference.VexStream.Playback;
using VexLife.ConstructionReference.VexStream.Playback.Native;

namespace VexLife.ConstructionReference.Maui.Playback;

public sealed class VexStreamPlaybackPage : ContentPage
{
    readonly MediaElement mediaElement;
    readonly MediaElementPlaybackPort port;
    readonly VexStreamPlaybackMediaConnection connection;
    readonly Button playPauseButton;
    readonly Label semanticLabel;
    readonly Label providerLabel;
    readonly MemoryStream fixtureStream;

    public VexStreamPlaybackPage()
    {
        Title = "VexStream Play/Pause";

        mediaElement = new MediaElement
        {
            ShouldAutoPlay = false,
            ShouldLoopPlayback = false,
            ShouldKeepScreenOn = false,
            ShouldShowPlaybackControls = false,
            HeightRequest = 1,
            Opacity = 0.01,
        };

        port = new MediaElementPlaybackPort(mediaElement);
        connection = new VexStreamPlaybackMediaConnection(port);
        connection.SemanticStateChanged += OnSemanticStateChanged;
        connection.ProviderFindingChanged += OnProviderFindingChanged;

        playPauseButton = new Button();
        playPauseButton.Clicked += OnPlayPauseClicked;

        semanticLabel = new Label();
        providerLabel = new Label { Text = "Provider finding: none" };

        fixtureStream = DeterministicAudioFixture.Create();
        port.SetSource(MediaSource.FromStream(fixtureStream));

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(24),
            Spacing = 12,
            Children =
            {
                new Label
                {
                    Text = "VexStream Play/Pause native practicum",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                },
                new Label
                {
                    Text = "The visible control is projected from portable semantic State; MediaElement is only the native provider surface.",
                },
                playPauseButton,
                semanticLabel,
                providerLabel,
                mediaElement,
            },
        };

        UpdatePresentation();
    }

    async void OnPlayPauseClicked(object? sender, EventArgs args)
    {
        try
        {
            await connection.ActivatePlayPauseAsync();
        }
        catch (Exception ex)
        {
            providerLabel.Text = $"Provider effect failure: {ex.GetType().Name}: {ex.Message}";
        }

        UpdatePresentation();
    }

    void OnSemanticStateChanged(object? sender, EventArgs args) => UpdatePresentation();

    void OnProviderFindingChanged(object? sender, VexStreamPlaybackProviderFindingEventArgs args) =>
        UpdatePresentation();

    void UpdatePresentation()
    {
        var providerClear = connection.ProviderFindingCode is null;
        switch (connection.Projection)
        {
            case VexStreamPlaybackProjection.PlayAvailable:
                playPauseButton.Text = "Play";
                playPauseButton.IsEnabled = providerClear;
                break;
            case VexStreamPlaybackProjection.PauseAvailable:
                playPauseButton.Text = "Pause";
                playPauseButton.IsEnabled = providerClear;
                break;
            case VexStreamPlaybackProjection.PlayPauseUnavailable:
                playPauseButton.Text = "Play / Pause unavailable";
                playPauseButton.IsEnabled = false;
                break;
            default:
                playPauseButton.Text = "Unavailable";
                playPauseButton.IsEnabled = false;
                break;
        }

        semanticLabel.Text =
            $"Semantic: kind={connection.State.Kind}; requestedPlay={connection.State.PlayRequested}; pending={connection.State.PendingIntent}; projection={connection.Projection}";

        providerLabel.Text = connection.ProviderFindingCode is null
            ? "Provider finding: none"
            : $"Provider finding: {connection.ProviderFindingCode}: {connection.ProviderFindingDetail}";
    }
}
