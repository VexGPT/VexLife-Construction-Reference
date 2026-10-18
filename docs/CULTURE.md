# Repository Culture

`[VXG RealForever]`

## Purpose

VexLife-Construction-Reference exists to answer a narrow question:

> Can accepted VexLife software meaning be constructed into a clean target representation while preserving source descent, ownership, and proof?

The repository is a **reference/proof target**, not a second VexLife semantic owner.

## Core distinctions

```text
MEANING != REPRESENTATION
CANONICAL SOURCE != TARGET SOURCE
ANDROID EVIDENCE != PORTABLE TRUTH
GENERATED BOOTSTRAP != PERMANENT GENERATOR OWNERSHIP
BUILD PASS != SEMANTIC EQUIVALENCE
HELD != PASS
UNKNOWN != IMPLEMENTATION PERMISSION
```

## Target ownership

Once the foundation bootstrap is accepted, this repository owns its target-native:

- file organization;
- readability;
- build conventions;
- target-specific implementation choices;
- target-specific tests and proof helpers.

Those decisions remain subordinate to canonical VexLife product meaning when a feature claims portability.

Target-specific discoveries may be proposed back upstream. They do not become canonical merely because they work here.

## Source descent

Every portable feature realization must remain traceable to the accepted source that owns its meaning.

A fresh maintainer should be able to answer:

```text
why does this exist?
what canonical source owns the behavior?
what target choice represents it here?
what was held, unsupported, or platform-specific?
what proof was run?
```

If those questions cannot be answered from current repository source plus its referenced canonical source, the feature is not ready to be treated as established.

## Evidence before abstraction

Do not create a reusable abstraction because two names look similar.

Observe the actual relationship first. Preserve semantic laws before language/framework form.

## Build residue

Compiler and intermediate output are not repository source.

Root `.gitignore` therefore owns:

```text
**/bin/
**/obj/
```

A foundation proof must demonstrate:

```text
dotnet build ... = PASS
git status --porcelain --untracked-files=all = empty
```

## Bootstrap ownership transition

The initial files were generated from an accepted SDK construction contract.

That provenance remains recorded in `construction/manifest.json`, but the generator is not the permanent culture owner.

Future target-native cultural improvements belong here, subject to the ownership and proof boundaries above.
