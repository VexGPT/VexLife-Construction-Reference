// [VXG RealForever]
// Concrete MediaElement-backed realization of the target-owned Play/Pause media port.

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using VexLife.ConstructionReference.VexStream.Playback.Native;

namespace VexLife.ConstructionReference.Maui.Playback;

public sealed class MediaElementPlaybackPort : IVexStreamPlaybackMediaPort, IDisposable
{
    readonly MediaElement element;
    readonly IMediaElement operations;
    long stoppedGeneration;
    bool disposed;

    public MediaElementPlaybackPort(MediaElement element)
    {
        this.element = element ?? throw new ArgumentNullException(nameof(element));
        operations = element;

        operations.StateChanged += OnStateChanged;
        element.MediaOpened += OnMediaOpened;
        element.MediaEnded += OnMediaEnded;
        element.MediaFailed += OnMediaFailed;
    }

    public event EventHandler<VexStreamPlaybackMediaObservationEventArgs>? Observation;

    public void SetSource(MediaSource? source)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        operations.Source = source;
        if (source is null)
        {
            Interlocked.Increment(ref stoppedGeneration);
            Raise(VexStreamPlaybackMediaObservationKind.NoMedia);
        }
    }

    public void Play()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        operations.Play();
    }

    public void Pause()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        operations.Pause();
    }

    public Task SeekToStartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return operations.SeekTo(TimeSpan.Zero, cancellationToken);
    }

    void OnStateChanged(object? sender, MediaStateChangedEventArgs args)
    {
        switch (args.NewState)
        {
            case MediaElementState.None:
                return;
            case MediaElementState.Opening:
                Raise(VexStreamPlaybackMediaObservationKind.Opening);
                return;
            case MediaElementState.Buffering:
                Raise(VexStreamPlaybackMediaObservationKind.Buffering);
                return;
            case MediaElementState.Playing:
                Raise(VexStreamPlaybackMediaObservationKind.Playing);
                return;
            case MediaElementState.Paused:
                Raise(VexStreamPlaybackMediaObservationKind.Paused);
                return;
            case MediaElementState.Stopped:
                DeferStopped();
                return;
            case MediaElementState.Failed:
                return;
            default:
                Raise(VexStreamPlaybackMediaObservationKind.Failed, $"UNKNOWN_MEDIAELEMENT_STATE:{args.NewState}");
                return;
        }
    }

    void DeferStopped()
    {
        var generation = Interlocked.Increment(ref stoppedGeneration);
        var dispatched = element.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1), () =>
        {
            if (!disposed && Volatile.Read(ref stoppedGeneration) == generation)
            {
                Raise(VexStreamPlaybackMediaObservationKind.Stopped, "MediaElementState.Stopped without matching MediaEnded");
            }
        });

        if (!dispatched)
        {
            Raise(VexStreamPlaybackMediaObservationKind.Stopped, "MediaElement dispatcher rejected deferred Stopped classification");
        }
    }

    void OnMediaOpened(object? sender, EventArgs args) =>
        Raise(VexStreamPlaybackMediaObservationKind.MediaOpened);

    void OnMediaEnded(object? sender, EventArgs args)
    {
        Interlocked.Increment(ref stoppedGeneration);
        Raise(VexStreamPlaybackMediaObservationKind.MediaEnded);
    }

    void OnMediaFailed(object? sender, MediaFailedEventArgs args)
    {
        Interlocked.Increment(ref stoppedGeneration);
        Raise(VexStreamPlaybackMediaObservationKind.Failed, args.ErrorMessage);
    }

    void Raise(VexStreamPlaybackMediaObservationKind kind, string? detail = null) =>
        Observation?.Invoke(this, new(kind, detail));

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        operations.StateChanged -= OnStateChanged;
        element.MediaOpened -= OnMediaOpened;
        element.MediaEnded -= OnMediaEnded;
        element.MediaFailed -= OnMediaFailed;
    }
}
