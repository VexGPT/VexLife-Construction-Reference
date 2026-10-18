# AGENTS.md

`[VXG RealForever]`

This file is the repository-level routing surface for humans and AI agents working in **VexLife-Construction-Reference**.

## Entry order

Before changing source:

1. Read `README.md`.
2. Read `docs/CULTURE.md`.
3. Read `docs/foundation/INDEX.md`.
4. Read `construction/manifest.json`.
5. Re-ground every mutable external source coordinate implicated by the work.

Do not rely on predecessor chat when repository-owned source already answers the question.

## Authority

This repository owns its target-native derived representation.

It does **not** own canonical VexLife product semantics.

Canonical product meaning remains in `vgong24/VexLife`. Reusable construction/proof machinery remains in `vgong24/Vextreme-SDK`.

## Formation rule

For any feature request, ask:

```text
what meaning is already accepted?
-> what exact source owns it?
-> what relationship must survive?
-> what is target-native representation only?
-> what proof establishes the target without expanding authority?
```

Do not map by library/class-name resemblance.

## Unknown and held work

```text
UNKNOWN meaning
-> do not emit behavior or a TODO that pretends the behavior is understood

UNDERSTOOD meaning + HELD realization
-> preserve source refs, behavior summary, inputs, outputs, invariants,
   and proof obligations
-> a compile-safe seam may be introduced only when its admitted stage allows it
-> HELD remains not-PASS
```

Prefer source references and short pseudocode over copying old implementation bodies into comments.

## Current restriction

The foundation bootstrap contains **no Play/Pause or Behavior implementation**.

Do not add feature semantics until the current canonical VexLife-owned portable contract has been separately accepted and the corresponding work is admitted.

## Build hygiene

Run:

```bash
dotnet build src/VexLife.ConstructionReference/VexLife.ConstructionReference.csproj --configuration Release
git status --porcelain --untracked-files=all
```

The second command must be empty after a clean build. Build residue belongs under ignored `bin/` and `obj/`, never in source.

## Change ownership

The bootstrap generator established these repository conventions once.

After bootstrap acceptance, this repository owns its own culture and may improve its target-native organization, provided those changes preserve canonical ownership and proof boundaries.
