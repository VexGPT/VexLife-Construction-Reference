// [VXG RealForever]
// Target-owned Play/Pause connection from accepted semantic runtime to a provider port.
// Provider effects and observations remain distinct from semantic admission and ownership.

namespace VexLife.ConstructionReference.VexStream.Playback.Native;

public sealed class VexStreamPlaybackProviderFindingEventArgs(string code, string? detail) : EventArgs
{
    public string Code { get; } = code;
    public string? Detail { get; } = detail;
}

public sealed class VexStreamPlaybackMediaConnection : IDisposable
{
    readonly IVexStreamPlaybackMediaPort port;
    bool disposed;

    public VexStreamPlaybackMediaConnection(IVexStreamPlaybackMediaPort port)
    {
        this.port = port ?? throw new ArgumentNullException(nameof(port));
        State = VexStreamPlaybackSemanticRuntime.NoMedia();
        port.Observation += OnObservation;
    }

    public VexStreamPlaybackSemanticState State { get; private set; }

    public VexStreamPlaybackProjection Projection => VexStreamPlaybackSemanticRuntime.Project(State);

    public string? ProviderFindingCode { get; private set; }

    public string? ProviderFindingDetail { get; private set; }

    public event EventHandler? SemanticStateChanged;

    public event EventHandler<VexStreamPlaybackProviderFindingEventArgs>? ProviderFindingChanged;

    public async Task<VexStreamPlaybackSemanticTransition> ActivatePlayPauseAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var transition = VexStreamPlaybackSemanticRuntime.Activate(
            State,
            VexStreamPlaybackControl.PlayPause);

        SetState(transition.State);

        try
        {
            switch (transition.Effect)
            {
                case null:
                    break;
                case VexStreamPlaybackEffect.RequestPlay:
                    port.Play();
                    break;
                case VexStreamPlaybackEffect.RequestPause:
                    port.Pause();
                    break;
                case VexStreamPlaybackEffect.RestartCurrent:
                    await port.SeekToStartAsync(cancellationToken).ConfigureAwait(false);
                    port.Play();
                    break;
                default:
                    throw new InvalidOperationException($"UNSUPPORTED_PLAYBACK_EFFECT:{transition.Effect}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            SetProviderFinding("PROVIDER_EFFECT_FAILED", ex.Message);
            throw;
        }

        return transition;
    }

    void OnObservation(object? sender, VexStreamPlaybackMediaObservationEventArgs observation)
    {
        if (disposed)
        {
            return;
        }

        switch (observation.Kind)
        {
            case VexStreamPlaybackMediaObservationKind.Opening:
            case VexStreamPlaybackMediaObservationKind.Buffering:
                return;
            case VexStreamPlaybackMediaObservationKind.Failed:
                SetProviderFinding("PROVIDER_MEDIA_FAILED", observation.Detail);
                return;
            case VexStreamPlaybackMediaObservationKind.Stopped:
                SetProviderFinding("PROVIDER_STOPPED_UNMAPPED", observation.Detail);
                return;
            case VexStreamPlaybackMediaObservationKind.MediaOpened:
                if (State.PendingIntent == VexStreamPlaybackPendingIntent.None)
                {
                    SetState(VexStreamPlaybackSemanticRuntime.Media(playRequested: false, playPauseAvailable: true));
                }
                return;
            case VexStreamPlaybackMediaObservationKind.NoMedia:
                ApplyProviderObservation(new(
                    VexStreamPlaybackSemanticKind.NoMedia,
                    PlayRequested: false,
                    PlayPauseAvailable: false,
                    RestartAvailable: false));
                return;
            case VexStreamPlaybackMediaObservationKind.Playing:
                ApplyProviderObservation(new(
                    VexStreamPlaybackSemanticKind.Media,
                    PlayRequested: true,
                    PlayPauseAvailable: true,
                    RestartAvailable: false));
                return;
            case VexStreamPlaybackMediaObservationKind.Paused:
                ApplyProviderObservation(new(
                    VexStreamPlaybackSemanticKind.Media,
                    PlayRequested: false,
                    PlayPauseAvailable: true,
                    RestartAvailable: false));
                return;
            case VexStreamPlaybackMediaObservationKind.MediaEnded:
                ApplyProviderObservation(new(
                    VexStreamPlaybackSemanticKind.Ended,
                    PlayRequested: false,
                    PlayPauseAvailable: false,
                    RestartAvailable: true));
                return;
            default:
                SetProviderFinding("PROVIDER_OBSERVATION_UNKNOWN", observation.Kind.ToString());
                return;
        }
    }

    void ApplyProviderObservation(VexStreamPlaybackProviderObservation observation)
    {
        try
        {
            var next = State.PendingIntent == VexStreamPlaybackPendingIntent.None
                ? VexStreamPlaybackSemanticRuntime.FromProviderObservation(observation)
                : VexStreamPlaybackSemanticRuntime.Observe(State, observation).State;

            SetState(next);
        }
        catch (InvalidOperationException ex)
        {
            SetProviderFinding("PROVIDER_OBSERVATION_UNSUPPORTED", ex.Message);
        }
    }

    void SetState(VexStreamPlaybackSemanticState next)
    {
        if (Equals(State, next))
        {
            return;
        }

        State = next;
        SemanticStateChanged?.Invoke(this, EventArgs.Empty);
    }

    void SetProviderFinding(string code, string? detail)
    {
        ProviderFindingCode = code;
        ProviderFindingDetail = detail;
        ProviderFindingChanged?.Invoke(this, new(code, detail));
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        port.Observation -= OnObservation;
    }
}
