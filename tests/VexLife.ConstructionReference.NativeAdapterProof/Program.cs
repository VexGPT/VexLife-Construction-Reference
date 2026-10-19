// [VXG RealForever]
// Dependency-free proof of the target-owned provider port/connection mapping.

using VexLife.ConstructionReference.VexStream.Playback;
using VexLife.ConstructionReference.VexStream.Playback.Native;

return await Proof.RunAsync();

static class Proof
{
    public static async Task<int> RunAsync()
    {
        await PlayPauseAcknowledgementFlow();
        await RestartFlow();
        await HeldProviderObservations();
        await NoMediaAndOpenedBoundaries();

        Console.WriteLine("P10_NATIVE_ADAPTER_CONTRACT_PROOF PASS");
        Console.WriteLine("semanticAuthority=false nativeProviderProof=false providerEffects=fake-only");
        return 0;
    }

    static async Task PlayPauseAcknowledgementFlow()
    {
        var port = new FakeMediaPort();
        using var connection = new VexStreamPlaybackMediaConnection(port);

        Equal(VexStreamPlaybackProjection.PlayPauseUnavailable, connection.Projection, "initial projection");

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaOpened);
        Equal(VexStreamPlaybackProjection.PlayAvailable, connection.Projection, "opened -> play available");
        Equal(VexStreamPlaybackPendingIntent.None, connection.State.PendingIntent, "opened pending none");

        var play = await connection.ActivatePlayPauseAsync();
        Equal(VexStreamPlaybackEffect.RequestPlay, play.Effect, "play effect");
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "play admitted pending");
        Equal(VexStreamPlaybackProjection.PauseAvailable, connection.Projection, "play admitted projection");
        Sequence(new[] { "PLAY" }, port.Operations, "play operation");

        port.Emit(VexStreamPlaybackMediaObservationKind.Opening);
        port.Emit(VexStreamPlaybackMediaObservationKind.Buffering);
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "transients preserve play intent");

        port.Emit(VexStreamPlaybackMediaObservationKind.Paused);
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "stale paused preserves play intent");
        Equal(true, connection.State.PlayRequested, "stale paused preserves requested play");

        port.Emit(VexStreamPlaybackMediaObservationKind.Playing);
        Equal(VexStreamPlaybackPendingIntent.None, connection.State.PendingIntent, "playing acknowledges play");
        Equal(true, connection.State.PlayRequested, "playing semantic state");

        var pause = await connection.ActivatePlayPauseAsync();
        Equal(VexStreamPlaybackEffect.RequestPause, pause.Effect, "pause effect");
        Equal(VexStreamPlaybackPendingIntent.RequestPause, connection.State.PendingIntent, "pause admitted pending");
        Sequence(new[] { "PLAY", "PAUSE" }, port.Operations, "pause operation");

        port.Emit(VexStreamPlaybackMediaObservationKind.Playing);
        Equal(VexStreamPlaybackPendingIntent.RequestPause, connection.State.PendingIntent, "stale playing preserves pause intent");
        Equal(false, connection.State.PlayRequested, "stale playing preserves requested pause");

        port.Emit(VexStreamPlaybackMediaObservationKind.Paused);
        Equal(VexStreamPlaybackPendingIntent.None, connection.State.PendingIntent, "paused acknowledges pause");
        Equal(VexStreamPlaybackProjection.PlayAvailable, connection.Projection, "paused -> play available");
    }

    static async Task RestartFlow()
    {
        var port = new FakeMediaPort();
        using var connection = new VexStreamPlaybackMediaConnection(port);

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaOpened);
        port.Emit(VexStreamPlaybackMediaObservationKind.MediaEnded);
        Equal(VexStreamPlaybackSemanticKind.Ended, connection.State.Kind, "ended kind");
        Equal(VexStreamPlaybackProjection.PlayAvailable, connection.Projection, "ended restart available");

        var restart = await connection.ActivatePlayPauseAsync();
        Equal(VexStreamPlaybackEffect.RestartCurrent, restart.Effect, "restart effect");
        Equal(VexStreamPlaybackPendingIntent.RestartCurrent, connection.State.PendingIntent, "restart pending");
        Sequence(new[] { "SEEK_ZERO", "PLAY" }, port.Operations, "restart provider operations");

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaEnded);
        Equal(VexStreamPlaybackPendingIntent.RestartCurrent, connection.State.PendingIntent, "stale ended preserves restart");

        port.Emit(VexStreamPlaybackMediaObservationKind.Paused);
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "paused after restart preserves play intent");
        Equal(true, connection.State.PlayRequested, "paused after restart keeps requested play");

        port.Emit(VexStreamPlaybackMediaObservationKind.Playing);
        Equal(VexStreamPlaybackPendingIntent.None, connection.State.PendingIntent, "playing acknowledges restart/play");
    }

    static async Task HeldProviderObservations()
    {
        var port = new FakeMediaPort();
        using var connection = new VexStreamPlaybackMediaConnection(port);

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaOpened);
        var before = connection.State;

        port.Emit(VexStreamPlaybackMediaObservationKind.Failed, "fixture failure");
        Equal(before, connection.State, "failed does not invent semantic state");
        Equal("PROVIDER_MEDIA_FAILED", connection.ProviderFindingCode, "failed finding");

        port.Emit(VexStreamPlaybackMediaObservationKind.Stopped, "unexpected stop");
        Equal(before, connection.State, "stopped remains held");
        Equal("PROVIDER_STOPPED_UNMAPPED", connection.ProviderFindingCode, "stopped finding");

        var deniedConnectionPort = new FakeMediaPort();
        using var deniedConnection = new VexStreamPlaybackMediaConnection(deniedConnectionPort);
        var denied = await deniedConnection.ActivatePlayPauseAsync();
        Equal<VexStreamPlaybackEffect?>(null, denied.Effect, "no-media effect denied");
        Equal(0, deniedConnectionPort.Operations.Count, "denied control has no provider operation");
    }

    static async Task NoMediaAndOpenedBoundaries()
    {
        var port = new FakeMediaPort();
        using var connection = new VexStreamPlaybackMediaConnection(port);

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaOpened);
        await connection.ActivatePlayPauseAsync();
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "pending play before repeated opened");

        port.Emit(VexStreamPlaybackMediaObservationKind.MediaOpened);
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, connection.State.PendingIntent, "repeated opened cannot erase pending intent");

        port.Emit(VexStreamPlaybackMediaObservationKind.NoMedia);
        Equal(VexStreamPlaybackSemanticKind.NoMedia, connection.State.Kind, "no-media terminal observation");
        Equal(VexStreamPlaybackPendingIntent.None, connection.State.PendingIntent, "no-media clears pending intent");
    }

    static void Equal<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{label}: expected={expected} actual={actual}");
        }
    }

    static void Sequence(IReadOnlyList<string> expected, IReadOnlyList<string> actual, string label)
    {
        if (expected.Count != actual.Count || !expected.SequenceEqual(actual, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"{label}: expected=[{string.Join(',', expected)}] actual=[{string.Join(',', actual)}]");
        }
    }

    sealed class FakeMediaPort : IVexStreamPlaybackMediaPort
    {
        public event EventHandler<VexStreamPlaybackMediaObservationEventArgs>? Observation;

        public List<string> Operations { get; } = [];

        public void Play() => Operations.Add("PLAY");

        public void Pause() => Operations.Add("PAUSE");

        public Task SeekToStartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Operations.Add("SEEK_ZERO");
            return Task.CompletedTask;
        }

        public void Emit(VexStreamPlaybackMediaObservationKind kind, string? detail = null) =>
            Observation?.Invoke(this, new(kind, detail));
    }
}
