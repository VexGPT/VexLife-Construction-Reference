// [VXG RealForever]
// Target-owned pure C# semantic realization of the accepted VexStream Playback Play/Pause contract.
// No UI framework, provider API, provider-success claim, or execution authority is owned here.

namespace VexLife.ConstructionReference.VexStream.Playback;

public enum VexStreamPlaybackSemanticKind
{
    NoMedia,
    Media,
    Ended,
}

public enum VexStreamPlaybackPendingIntent
{
    None,
    RequestPlay,
    RequestPause,
    RestartCurrent,
}

public sealed record VexStreamPlaybackSemanticState(
    VexStreamPlaybackSemanticKind Kind,
    bool PlayRequested,
    bool PlayPauseAvailable,
    bool RestartAvailable,
    VexStreamPlaybackPendingIntent PendingIntent);

public sealed record VexStreamPlaybackProviderObservation(
    VexStreamPlaybackSemanticKind Kind,
    bool PlayRequested,
    bool PlayPauseAvailable,
    bool RestartAvailable);

public sealed record VexStreamPlaybackSemanticTransition(
    VexStreamPlaybackAct? Act,
    VexStreamPlaybackEffect? Effect,
    string Admission,
    VexStreamPlaybackSemanticState State,
    VexStreamPlaybackProjection Projection);

public static class VexStreamPlaybackSemanticRuntime
{
    public const string DeniedUnavailable = "DENIED_UNAVAILABLE";
    public const string DeniedDuplicatePendingIntent = "DENIED_DUPLICATE_PENDING_INTENT";
    public const string AdmittedIntentPendingProviderAcknowledgement = "ADMITTED_INTENT_PENDING_PROVIDER_ACKNOWLEDGEMENT";
    public const string AdmittedIntentReplacesPendingProviderAcknowledgement = "ADMITTED_INTENT_REPLACES_PENDING_PROVIDER_ACKNOWLEDGEMENT";
    public const string PreservedNewerAdmittedIntent = "PRESERVED_NEWER_ADMITTED_INTENT";
    public const string ClearedMatchingProviderAcknowledgement = "CLEARED_MATCHING_PROVIDER_ACKNOWLEDGEMENT";
    public const string ClearedIncompatibleTerminalObservation = "CLEARED_INCOMPATIBLE_TERMINAL_OBSERVATION";

    public static VexStreamPlaybackSemanticState NoMedia() =>
        new(VexStreamPlaybackSemanticKind.NoMedia, false, false, false, VexStreamPlaybackPendingIntent.None);

    public static VexStreamPlaybackSemanticState Media(
        bool playRequested,
        bool playPauseAvailable = true,
        VexStreamPlaybackPendingIntent pendingIntent = VexStreamPlaybackPendingIntent.None) =>
        new(VexStreamPlaybackSemanticKind.Media, playRequested, playPauseAvailable, false, pendingIntent);

    public static VexStreamPlaybackSemanticState Ended(
        bool restartAvailable,
        VexStreamPlaybackPendingIntent pendingIntent = VexStreamPlaybackPendingIntent.None,
        bool playRequested = false) =>
        new(VexStreamPlaybackSemanticKind.Ended, playRequested, false, restartAvailable, pendingIntent);

    public static VexStreamPlaybackSemanticState FromProviderObservation(VexStreamPlaybackProviderObservation observation) =>
        observation.Kind switch
        {
            VexStreamPlaybackSemanticKind.NoMedia => NoMedia(),
            VexStreamPlaybackSemanticKind.Media => Media(observation.PlayRequested, observation.PlayPauseAvailable),
            VexStreamPlaybackSemanticKind.Ended => Ended(observation.RestartAvailable),
            _ => throw new ArgumentOutOfRangeException(nameof(observation)),
        };

    public static VexStreamPlaybackProjection Project(VexStreamPlaybackSemanticState state) =>
        state.Kind switch
        {
            VexStreamPlaybackSemanticKind.NoMedia => VexStreamPlaybackProjection.PlayPauseUnavailable,
            VexStreamPlaybackSemanticKind.Ended when state.PendingIntent == VexStreamPlaybackPendingIntent.RestartCurrent =>
                VexStreamPlaybackProjection.PlayPauseUnavailable,
            VexStreamPlaybackSemanticKind.Ended when state.RestartAvailable => VexStreamPlaybackProjection.PlayAvailable,
            VexStreamPlaybackSemanticKind.Ended => VexStreamPlaybackProjection.PlayPauseUnavailable,
            VexStreamPlaybackSemanticKind.Media when !state.PlayPauseAvailable => VexStreamPlaybackProjection.PlayPauseUnavailable,
            VexStreamPlaybackSemanticKind.Media when state.PlayRequested => VexStreamPlaybackProjection.PauseAvailable,
            VexStreamPlaybackSemanticKind.Media => VexStreamPlaybackProjection.PlayAvailable,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };

    public static VexStreamPlaybackSemanticTransition Activate(
        VexStreamPlaybackSemanticState state,
        VexStreamPlaybackControl control)
    {
        if (control != VexStreamPlaybackControl.PlayPause)
        {
            throw new ArgumentOutOfRangeException(nameof(control));
        }

        var projection = Project(state);
        if (projection == VexStreamPlaybackProjection.PlayPauseUnavailable)
        {
            var admission = state.Kind == VexStreamPlaybackSemanticKind.Ended
                && state.PendingIntent == VexStreamPlaybackPendingIntent.RestartCurrent
                    ? DeniedDuplicatePendingIntent
                    : DeniedUnavailable;
            return new(null, null, admission, state, projection);
        }

        if (projection == VexStreamPlaybackProjection.PlayAvailable)
        {
            if (state.Kind == VexStreamPlaybackSemanticKind.Ended)
            {
                var restartNext = state with
                {
                    PlayRequested = true,
                    PendingIntent = VexStreamPlaybackPendingIntent.RestartCurrent,
                };
                return new(
                    VexStreamPlaybackAct.Play,
                    VexStreamPlaybackEffect.RestartCurrent,
                    AdmittedIntentPendingProviderAcknowledgement,
                    restartNext,
                    Project(restartNext));
            }

            var replacing = state.PendingIntent == VexStreamPlaybackPendingIntent.RequestPause;
            var next = state with
            {
                PlayRequested = true,
                PendingIntent = VexStreamPlaybackPendingIntent.RequestPlay,
            };
            return new(
                VexStreamPlaybackAct.Play,
                VexStreamPlaybackEffect.RequestPlay,
                replacing
                    ? AdmittedIntentReplacesPendingProviderAcknowledgement
                    : AdmittedIntentPendingProviderAcknowledgement,
                next,
                Project(next));
        }

        if (state.Kind != VexStreamPlaybackSemanticKind.Media)
        {
            throw new InvalidOperationException("PAUSE_PROJECTION_REQUIRES_MEDIA_STATE");
        }

        var replacesPlay = state.PendingIntent == VexStreamPlaybackPendingIntent.RequestPlay;
        var pauseNext = state with
        {
            PlayRequested = false,
            PendingIntent = VexStreamPlaybackPendingIntent.RequestPause,
        };
        return new(
            VexStreamPlaybackAct.Pause,
            VexStreamPlaybackEffect.RequestPause,
            replacesPlay
                ? AdmittedIntentReplacesPendingProviderAcknowledgement
                : AdmittedIntentPendingProviderAcknowledgement,
            pauseNext,
            Project(pauseNext));
    }

    public static VexStreamPlaybackSemanticTransition Observe(
        VexStreamPlaybackSemanticState state,
        VexStreamPlaybackProviderObservation observation)
    {
        if (state.PendingIntent == VexStreamPlaybackPendingIntent.None)
        {
            throw new InvalidOperationException("OBSERVATION_RECONCILIATION_REQUIRES_PENDING_INTENT");
        }

        if (observation.Kind == VexStreamPlaybackSemanticKind.NoMedia)
        {
            var next = NoMedia();
            return new(null, null, ClearedIncompatibleTerminalObservation, next, Project(next));
        }

        if (state.PendingIntent is VexStreamPlaybackPendingIntent.RequestPlay or VexStreamPlaybackPendingIntent.RequestPause
            && observation.Kind == VexStreamPlaybackSemanticKind.Ended)
        {
            var next = Ended(observation.RestartAvailable);
            return new(null, null, ClearedIncompatibleTerminalObservation, next, Project(next));
        }

        if (state.PendingIntent == VexStreamPlaybackPendingIntent.RequestPlay
            && observation.Kind == VexStreamPlaybackSemanticKind.Media)
        {
            if (observation.PlayRequested)
            {
                var acknowledged = Media(true, observation.PlayPauseAvailable);
                return new(null, null, ClearedMatchingProviderAcknowledgement, acknowledged, Project(acknowledged));
            }

            var preserved = Media(true, observation.PlayPauseAvailable, VexStreamPlaybackPendingIntent.RequestPlay);
            return new(null, null, PreservedNewerAdmittedIntent, preserved, Project(preserved));
        }

        if (state.PendingIntent == VexStreamPlaybackPendingIntent.RequestPause
            && observation.Kind == VexStreamPlaybackSemanticKind.Media)
        {
            if (!observation.PlayRequested)
            {
                var acknowledged = Media(false, observation.PlayPauseAvailable);
                return new(null, null, ClearedMatchingProviderAcknowledgement, acknowledged, Project(acknowledged));
            }

            var preserved = Media(false, observation.PlayPauseAvailable, VexStreamPlaybackPendingIntent.RequestPause);
            return new(null, null, PreservedNewerAdmittedIntent, preserved, Project(preserved));
        }

        if (state.PendingIntent == VexStreamPlaybackPendingIntent.RestartCurrent)
        {
            if (observation.Kind == VexStreamPlaybackSemanticKind.Ended)
            {
                return new(null, null, PreservedNewerAdmittedIntent, state, Project(state));
            }

            if (observation.Kind == VexStreamPlaybackSemanticKind.Media)
            {
                if (observation.PlayRequested)
                {
                    var acknowledged = Media(true, observation.PlayPauseAvailable);
                    return new(null, null, ClearedMatchingProviderAcknowledgement, acknowledged, Project(acknowledged));
                }

                var preservedPlay = Media(true, observation.PlayPauseAvailable, VexStreamPlaybackPendingIntent.RequestPlay);
                return new(null, null, PreservedNewerAdmittedIntent, preservedPlay, Project(preservedPlay));
            }
        }

        throw new InvalidOperationException("UNSUPPORTED_PROVIDER_OBSERVATION_FOR_ACCEPTED_PLAY_PAUSE_LAWS");
    }
}