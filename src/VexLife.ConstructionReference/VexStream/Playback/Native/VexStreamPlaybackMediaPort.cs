// [VXG RealForever]
// Target-owned Play/Pause provider port. This is a realization boundary, not portable VexLife meaning.

namespace VexLife.ConstructionReference.VexStream.Playback.Native;

public enum VexStreamPlaybackMediaObservationKind
{
    NoMedia,
    MediaOpened,
    Opening,
    Buffering,
    Playing,
    Paused,
    MediaEnded,
    Failed,
    Stopped,
}

public sealed class VexStreamPlaybackMediaObservationEventArgs(
    VexStreamPlaybackMediaObservationKind kind,
    string? detail = null) : EventArgs
{
    public VexStreamPlaybackMediaObservationKind Kind { get; } = kind;
    public string? Detail { get; } = detail;
}

public interface IVexStreamPlaybackMediaPort
{
    event EventHandler<VexStreamPlaybackMediaObservationEventArgs>? Observation;

    void Play();

    void Pause();

    Task SeekToStartAsync(CancellationToken cancellationToken = default);
}
