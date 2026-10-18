# VexLife Construction Reference

`[VXG RealForever]`

This repository is a **derived clean-room construction/proof surface** for VexLife software meaning.

It does **not** own canonical VexLife product semantics.

## Ownership

```text
vgong24/VexLife
  canonical VexLife product/software Blueprint meaning

vgong24/Vextreme-SDK
  reusable observation / mapping / construction / proof machinery

vgong24/VexLife-Android
  Android-native realization and architecture/proof evidence

VexGPT/VexLife-Construction-Reference
  target-native derived representation / proof surface
  semanticAuthority=false
```

## Current state

```text
repository foundation bootstrap = CANDIDATE
Play/Pause feature code = NOT PRESENT
portable Behavior contract = NOT PRESENT
UI framework = NOT SELECTED
provider adapter = NOT PRESENT
semantic equivalence = NOT CLAIMED
native/provider conformance = NOT CLAIMED
```

The current bootstrap is intentionally feature-free. Its purpose is to make this repository understandable and buildable before any product behavior is realized here.

## Start here

```text
README.md
-> AGENTS.md
-> docs/CULTURE.md
-> docs/foundation/INDEX.md
-> construction/manifest.json
```

The machine-readable provenance and exact source bindings live in `construction/manifest.json`.

## Build

Target profile:

```text
runtime=.NET 10
targetFramework=net10.0
language=C# 14
projectKind=class library
OS-specific=false
UI framework=none
provider framework=none
```

Build:

```bash
dotnet build src/VexLife.ConstructionReference/VexLife.ConstructionReference.csproj --configuration Release
```

Then verify the source view remains clean:

```bash
git status --porcelain --untracked-files=all
```

Expected output is empty.

`bin/` and `obj/` are ignored build/intermediate residue, not target source.

## Source descent

This bootstrap descends from the accepted VexLife Blueprint Source of Truth construction contract in Vextreme-SDK.

Do not reconstruct product behavior from this repository alone.

Future feature realization must descend through current accepted canonical VexLife-owned meaning and the source/proof contracts named by the active construction manifest.

## Boundary

The repository may own its target-native representation, readability, build organization, and target-specific improvements.

It must not silently promote target implementation choices into canonical VexLife meaning.

```text
TARGET REPRESENTATION
!= CANONICAL PRODUCT MEANING

BUILD PASS
!= SEMANTIC EQUIVALENCE

SEMANTIC EQUIVALENCE
!= NATIVE PROVIDER PROOF
```

<!-- [VEXLIFE-CONSTRUCTION-REFERENCE][FOUNDATION-FRONT-DOOR] -->
