# Foundation Index

`[VXG RealForever]`

This index explains the accepted repository foundation and the first bounded P6 target-representation candidate. It remains intentionally small.

| Path | Role | Ownership after bootstrap |
|---|---|---|
| `.gitignore` | Keeps .NET `bin/` and `obj/` build residue out of source currentness | Target-owned |
| `README.md` | Human front door, ownership, build, source descent | Target-owned |
| `AGENTS.md` | Human/AI work routing and authority boundaries | Target-owned |
| `docs/CULTURE.md` | Repository-owned target construction culture | Target-owned |
| `docs/foundation/INDEX.md` | Foundation navigation and intent | Target-owned |
| `construction/manifest.json` | Exact bootstrap provenance, target profile, proof contract | Derived provenance; refresh requires review |
| `src/VexLife.ConstructionReference/VexLife.ConstructionReference.csproj` | Minimal cross-platform .NET/C# build entry | Target-owned |

## Foundation state

```text
foundation-owned feature implementation = absent
P6 Play/Pause C# contract data = accepted/current
P10 pure C# semantic runtime = accepted/current
Behavior foundation implementation = bounded Play/Pause semantic runtime candidate only
native adapter selection = candidate
candidate UI framework = .NET MAUI 10.0.110
candidate provider adapter = CommunityToolkit.Maui.MediaElement 10.0.0 / IMediaElement
candidate first lived target = Mac Catalyst
executable native adapter source = absent
```

That absence is intentional.

The repository foundation proves orientation and target-native buildability only.

## P6 Play/Pause contract-data candidate

```text
canonical VexLife portable meaning
-> accepted SDK deterministic mapping
-> src/VexLife.ConstructionReference/VexStream/Playback/VexStreamPlaybackContract.g.cs
```

The generated file is target representation, not a second semantic owner. Its admitted candidate bytes are bound to:

```text
sha256=0671e98db75cf7c038ead6b6c9187731a0f01115a3189b361a044238ff31da34
portableContract=contract.vexlife.vexstream.playback.play-pause.001
portableLawRows=19
sdkMerge=d4847c410330607b86323a4d641ee92f006d5a28
workClaim=github.issue.VexGPT.VexLife-Construction-Reference.3
```

P7 compile/negative-type proof, P8 language-neutral semantic-law parity, P9 semantic graph/hash parity, and native/provider conformance remain separate later claims.

## Build proof

```bash
dotnet build src/VexLife.ConstructionReference/VexLife.ConstructionReference.csproj --configuration Release
git status --porcelain --untracked-files=all
```

Required result:

```text
build = PASS
git status = empty
```

## Next semantic edge

The current target edge is review of the native adapter selection candidate:

```text
accepted pure C# semantic runtime
-> candidate .NET MAUI 10 UI host
-> candidate CommunityToolkit.Maui.MediaElement / IMediaElement provider adapter
-> candidate Mac Catalyst first lived proof
-> selection review/currentness
-> only then target-native source admission
```

Opening and Buffering remain transient provider states. MediaFailed and unexpected Stopped remain target-specific rather than silently becoming portable meaning.

No MAUI/provider executable source is admitted by this selection candidate.
