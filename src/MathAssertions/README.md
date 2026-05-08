# MathAssertions

[![NuGet](https://img.shields.io/nuget/v/MathAssertions.svg)](https://www.nuget.org/packages/MathAssertions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **Scope:** Test projects only. Not intended for production code.

Framework-agnostic core for the MathAssertions package family. The actual TUnit fluent assertions ship in the framework-specific adapter package (currently `MathAssertions.TUnit`).

> **Most users want [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/), not this package directly.** This package only ships the shared `MathTolerance` helpers; the adapter package adds the assertion entry points your test framework expects.

---

## What's in this package (v0.0.1)

`MathTolerance` static class with pure, NaN-aware, infinity-aware tolerance-comparison helpers:

- `IsApproximatelyEqual(double, double, double)`
- `IsApproximatelyEqual(float, float, float)`
- `IsApproximatelyEqual(Vector3, Vector3, double)` (component-wise; components widen to `double` so a tight `double` tolerance is honored at full precision)

All helpers reject NaN and negative tolerance via `ArgumentOutOfRangeException`.

## What lands at v0.1.0

Vector2 / Vector4 component-wise; Quaternion component-wise plus rotational equivalence (handling the `q` / `-q` double-cover); Matrix4x4 element-wise; Plane component-wise plus geometric equivalence; Complex; span overloads; statistics (mean, median, variance, percentile); linear-algebra invariants; number theory; 3D geometry primitives plus intersection / containment / pointcloud assertions.

## Test-framework adapters

| Package | Test framework | Status |
|---|---|---|
| [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/) | TUnit | Available now |
| `MathAssertions.NUnit` | NUnit | Possible if there is demand |
| `MathAssertions.xUnit` | xUnit | Possible if there is demand |
| `MathAssertions.MSTest` | MSTest | Possible if there is demand |

If you'd find a non-TUnit adapter useful, [open a feature request](https://github.com/JohnVerheij/MathAssertions.TUnit/issues/new?template=feature_request.yml). Adapters are not built proactively.

## When to install this package directly

Only when authoring a non-TUnit adapter for the assertion family, or when calling `MathTolerance` from a `[GenerateAssertion]` extension on a private type. For any TUnit testing use case, install `MathAssertions.TUnit`.

## Installation

```bash
dotnet add package MathAssertions.TUnit
```

`MathAssertions` comes transitively. You don't need to install it directly unless you're building your own adapter package.

## Stability

The public surface above is semver-bound. Breaking changes require a major version bump. The exact text of any future failure-message rendering produced inside `MathTolerance` is **not stable** and may gain extra detail or change formatting in any release.

## Repository

[github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit) for the full README, design notes, and roadmap.

## License

[MIT](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/LICENSE). Copyright (c) 2026 John Verheij.
