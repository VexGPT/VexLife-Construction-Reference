# Foundation Index

`[VXG RealForever]`

This index explains the **feature-free repository foundation**. It is intentionally small.

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
feature code = absent
Behavior foundation code = absent
UI framework = absent
provider adapter = absent
external package requirement = absent
```

That absence is intentional.

The repository foundation proves orientation and target-native buildability only.

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

The next product-semantic edge is **not owned by this foundation**.

Portable Behavior remains a VexLife-owned contract formation/acceptance problem before Behavior or Play/Pause semantics can be realized here.
