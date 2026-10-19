// [VXG RealForever]
// Dependency-free executable proof of all accepted VexStream Playback Play/Pause semantic-law rows.

using VexLife.ConstructionReference.VexStream.Playback;

return Proof.Run();

static class Proof
{
    private static readonly HashSet<string> CoveredRows = new(StringComparer.Ordinal);
    private static int cases;

    public static int Run()
    {
        ActivationRows();
        ObservationRows();

        Equal(19, VexStreamPlaybackContract.SemanticLawRows.Count, "portable law row count");
        Equal(19, CoveredRows.Count, "covered law row count");
        Equal(23, cases, "executed proof case count");

        var missing = VexStreamPlaybackContract.SemanticLawRows
            .Select(row => row.RowRef)
            .Where(rowRef => !CoveredRows.Contains(rowRef))
            .ToArray();
        Equal(0, missing.Length, "missing law rows: " + string.Join(", ", missing));

        Console.WriteLine($"P10_CSHARP_SEMANTIC_RUNTIME_PROOF PASS rows={CoveredRows.Count} cases={cases}");
        Console.WriteLine("semanticAuthority=false providerEffect=false uiFramework=false providerFramework=false");
        return 0;
    }

    private static void ActivationRows()
    {
        AssertActivation(
            "law.vexstream.playback.play-pause.no-media-unavailable",
            "state.vexstream.playback.no-media",
            VexStreamPlaybackSemanticRuntime.NoMedia());

        var paused = AssertActivation(
            "law.vexstream.playback.play-pause.paused-requests-play",
            "state.vexstream.playback.media.paused-controllable",
            VexStreamPlaybackSemanticRuntime.Media(false));
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, paused.State.PendingIntent, "paused -> pending play");
        Equal(true, paused.State.PlayRequested, "paused -> local play requested");

        var playing = AssertActivation(
            "law.vexstream.playback.play-pause.playing-requests-pause",
            "state.vexstream.playback.media.playing-controllable",
            VexStreamPlaybackSemanticRuntime.Media(true));
        Equal(VexStreamPlaybackPendingIntent.RequestPause, playing.State.PendingIntent, "playing -> pending pause");
        Equal(false, playing.State.PlayRequested, "playing -> local pause requested");

        AssertActivation(
            "law.vexstream.playback.play-pause.media-uncontrollable",
            "state.vexstream.playback.media.uncontrollable",
            VexStreamPlaybackSemanticRuntime.Media(false, playPauseAvailable: false));

        var restart = AssertActivation(
            "law.vexstream.playback.play-pause.ended-restarts-current",
            "state.vexstream.playback.ended.restartable",
            VexStreamPlaybackSemanticRuntime.Ended(restartAvailable: true));
        Equal(VexStreamPlaybackPendingIntent.RestartCurrent, restart.State.PendingIntent, "ended -> pending restart");

        AssertActivation(
            "law.vexstream.playback.play-pause.ended-restart-pending-denies-duplicate",
            "state.vexstream.playback.ended.restart-pending",
            VexStreamPlaybackSemanticRuntime.Ended(true, VexStreamPlaybackPendingIntent.RestartCurrent, playRequested: true));

        AssertActivation(
            "law.vexstream.playback.play-pause.ended-restart-unavailable",
            "state.vexstream.playback.ended.restart-unavailable",
            VexStreamPlaybackSemanticRuntime.Ended(restartAvailable: false));

        var replacePlay = AssertActivation(
            "law.vexstream.playback.pending-play.newer-pause-replaces-pending-play",
            "state.vexstream.playback.media.play-request-pending",
            VexStreamPlaybackSemanticRuntime.Media(true, pendingIntent: VexStreamPlaybackPendingIntent.RequestPlay));
        Equal(VexStreamPlaybackPendingIntent.RequestPause, replacePlay.State.PendingIntent, "newer pause replaces play");
        Equal(false, replacePlay.State.PlayRequested, "newer pause changes local requested value");

        var replacePause = AssertActivation(
            "law.vexstream.playback.pending-pause.newer-play-replaces-pending-pause",
            "state.vexstream.playback.media.pause-request-pending",
            VexStreamPlaybackSemanticRuntime.Media(false, pendingIntent: VexStreamPlaybackPendingIntent.RequestPause));
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, replacePause.State.PendingIntent, "newer play replaces pause");
        Equal(true, replacePause.State.PlayRequested, "newer play changes local requested value");
    }

    private static void ObservationRows()
    {
        var pendingPlay = VexStreamPlaybackSemanticRuntime.Media(true, pendingIntent: VexStreamPlaybackPendingIntent.RequestPlay);
        var pendingPause = VexStreamPlaybackSemanticRuntime.Media(false, pendingIntent: VexStreamPlaybackPendingIntent.RequestPause);
        var pendingRestart = VexStreamPlaybackSemanticRuntime.Ended(true, VexStreamPlaybackPendingIntent.RestartCurrent, playRequested: true);

        var stalePlay = AssertObservation(
            "law.vexstream.playback.pending-play.survives-stale-paused-observation",
            "state.vexstream.playback.media.play-request-pending.stale-paused-observation",
            pendingPlay,
            MediaObservation(playRequested: false));
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, stalePlay.State.PendingIntent, "stale pause preserves pending play");
        Equal(true, stalePlay.State.PlayRequested, "stale pause preserves newer play value");

        var playAck = AssertObservation(
            "law.vexstream.playback.pending-play.clears-on-playing-acknowledgement",
            "state.vexstream.playback.media.play-request-pending.matching-playing-acknowledgement",
            pendingPlay,
            MediaObservation(playRequested: true));
        Equal(VexStreamPlaybackPendingIntent.None, playAck.State.PendingIntent, "playing acknowledgement clears pending play");

        var stalePause = AssertObservation(
            "law.vexstream.playback.pending-pause.survives-stale-playing-observation",
            "state.vexstream.playback.media.pause-request-pending.stale-playing-observation",
            pendingPause,
            MediaObservation(playRequested: true));
        Equal(VexStreamPlaybackPendingIntent.RequestPause, stalePause.State.PendingIntent, "stale play preserves pending pause");
        Equal(false, stalePause.State.PlayRequested, "stale play preserves newer pause value");

        var pauseAck = AssertObservation(
            "law.vexstream.playback.pending-pause.clears-on-paused-acknowledgement",
            "state.vexstream.playback.media.pause-request-pending.matching-paused-acknowledgement",
            pendingPause,
            MediaObservation(playRequested: false));
        Equal(VexStreamPlaybackPendingIntent.None, pauseAck.State.PendingIntent, "paused acknowledgement clears pending pause");

        var staleEnded = AssertObservation(
            "law.vexstream.playback.pending-restart.survives-stale-ended-observation",
            "state.vexstream.playback.ended.restart-request-pending.stale-ended-observation",
            pendingRestart,
            EndedObservation(restartAvailable: true));
        Equal(VexStreamPlaybackPendingIntent.RestartCurrent, staleEnded.State.PendingIntent, "stale ended preserves restart intent");

        var restartPaused = AssertObservation(
            "law.vexstream.playback.pending-restart.media-paused-observation-preserves-play-intent",
            "state.vexstream.playback.ended.restart-request-pending.media-paused-observation.play-request-remains-pending",
            pendingRestart,
            MediaObservation(playRequested: false));
        Equal(VexStreamPlaybackPendingIntent.RequestPlay, restartPaused.State.PendingIntent, "paused after restart preserves play as pending");
        Equal(true, restartPaused.State.PlayRequested, "paused after restart preserves requested play value");

        var restartAck = AssertObservation(
            "law.vexstream.playback.pending-restart.clears-on-playing-media-acknowledgement",
            "state.vexstream.playback.ended.restart-request-pending.media-playing-acknowledgement",
            pendingRestart,
            MediaObservation(playRequested: true));
        Equal(VexStreamPlaybackPendingIntent.None, restartAck.State.PendingIntent, "playing after restart clears pending restart");

        foreach (var state in new[] { pendingPlay, pendingPause, pendingRestart })
        {
            var cleared = AssertObservation(
                "law.vexstream.playback.pending-intent.clears-on-no-media-observation",
                "state.vexstream.playback.pending-intent.incompatible-no-media-observation",
                state,
                NoMediaObservation());
            Equal(VexStreamPlaybackPendingIntent.None, cleared.State.PendingIntent, "no-media clears pending intent");
            Equal(VexStreamPlaybackSemanticKind.NoMedia, cleared.State.Kind, "no-media becomes semantic no-media");
        }

        foreach (var state in new[] { pendingPlay, pendingPause })
        {
            var restartable = AssertObservation(
                "law.vexstream.playback.media-pending-intent.clears-on-restartable-ended-observation",
                "state.vexstream.playback.media-pending-intent.incompatible-ended-observation.restartable",
                state,
                EndedObservation(restartAvailable: true));
            Equal(VexStreamPlaybackPendingIntent.None, restartable.State.PendingIntent, "restartable ended clears media pending intent");
            Equal(true, restartable.State.RestartAvailable, "restartable ended remains restartable");

            var unavailable = AssertObservation(
                "law.vexstream.playback.media-pending-intent.clears-on-unrestartable-ended-observation",
                "state.vexstream.playback.media-pending-intent.incompatible-ended-observation.restart-unavailable",
                state,
                EndedObservation(restartAvailable: false));
            Equal(VexStreamPlaybackPendingIntent.None, unavailable.State.PendingIntent, "unrestartable ended clears media pending intent");
            Equal(false, unavailable.State.RestartAvailable, "unrestartable ended remains unavailable");
        }
    }

    private static VexStreamPlaybackSemanticTransition AssertActivation(
        string rowRef,
        string sourceStateRef,
        VexStreamPlaybackSemanticState state)
    {
        var transition = VexStreamPlaybackSemanticRuntime.Activate(state, VexStreamPlaybackControl.PlayPause);
        var law = Law(rowRef);
        Equal(sourceStateRef, law.SourceStateRef, rowRef + " source state");
        Equal<VexStreamPlaybackControl?>(VexStreamPlaybackControl.PlayPause, law.Control, rowRef + " control");
        Equal(law.ResolvedAct, transition.Act, rowRef + " act");
        Equal(law.Effect, transition.Effect, rowRef + " effect");
        Equal(law.Admission, transition.Admission, rowRef + " admission");
        Equal<VexStreamPlaybackProjection?>(law.ExpectedProjection, transition.Projection, rowRef + " projection");
        Cover(rowRef);
        return transition;
    }

    private static VexStreamPlaybackSemanticTransition AssertObservation(
        string rowRef,
        string sourceStateRef,
        VexStreamPlaybackSemanticState state,
        VexStreamPlaybackProviderObservation observation)
    {
        var transition = VexStreamPlaybackSemanticRuntime.Observe(state, observation);
        var law = Law(rowRef);
        Equal(sourceStateRef, law.SourceStateRef, rowRef + " source state");
        Equal<VexStreamPlaybackControl?>(null, law.Control, rowRef + " control");
        Equal<VexStreamPlaybackAct?>(null, transition.Act, rowRef + " act");
        Equal<VexStreamPlaybackEffect?>(null, transition.Effect, rowRef + " effect");
        Equal(law.Admission, transition.Admission, rowRef + " admission");
        Equal<VexStreamPlaybackProjection?>(law.ExpectedProjection, transition.Projection, rowRef + " projection");
        Cover(rowRef);
        return transition;
    }

    private static VexStreamPlaybackLawRow Law(string rowRef) =>
        VexStreamPlaybackContract.SemanticLawRows.Single(row => row.RowRef == rowRef);

    private static VexStreamPlaybackProviderObservation MediaObservation(bool playRequested) =>
        new(VexStreamPlaybackSemanticKind.Media, playRequested, true, false);

    private static VexStreamPlaybackProviderObservation EndedObservation(bool restartAvailable) =>
        new(VexStreamPlaybackSemanticKind.Ended, false, false, restartAvailable);

    private static VexStreamPlaybackProviderObservation NoMediaObservation() =>
        new(VexStreamPlaybackSemanticKind.NoMedia, false, false, false);

    private static void Cover(string rowRef)
    {
        CoveredRows.Add(rowRef);
        cases += 1;
    }

    private static void Equal<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{label}: expected={expected} actual={actual}");
        }
    }
}