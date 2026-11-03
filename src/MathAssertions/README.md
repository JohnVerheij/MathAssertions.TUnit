# MathAssertions

[![NuGet](https://img.shields.io/nuget/v/MathAssertions.svg)](https://www.nuget.org/packages/MathAssertions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **Scope:** Test projects only. Not intended for production code.

> Part of the **[DotNetAssertions](https://dotnetassertions.dev)** family. This is the framework-agnostic core; the TUnit assertions live in the matching `.TUnit` package.

Framework-agnostic core of the MathAssertions package family. Math assertion helpers for .NET tests, spanning tolerance comparisons, sequences, statistics, linear algebra, number theory, and 3D geometry. Test-framework-specific entry points ship in adapter packages (currently `MathAssertions.TUnit` for TUnit).

> **Most users want [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/), not this package directly.** This package ships the framework-agnostic helper classes (`MathTolerance`, `Sequences`, `Statistics`, `LinearAlgebra`, `NumberTheory`, plus the `Geometry3D` namespace and `PoseRenderer`); the adapter package adds the fluent `Assert.That(value).Method(...)` entry points your test framework expects.

---

## What's in this package

Pure mathematical-assertion helpers. The tolerance comparisons handle NaN and infinity explicitly. Five static classes in the `MathAssertions` namespace, `PoseRenderer` in `MathAssertions.Render`, and the `MathAssertions.Geometry3D` namespace cover ~85 methods:

- `MathTolerance`: scalar `IsApproximatelyEqual` for `double`/`float` plus the `System.Numerics` compounds (`Vector2`/`Vector3`/`Vector4`, `Quaternion` with `IsRotationallyEquivalent` and (v0.2.0+) `HasAxisAngleApproximately`, `Matrix4x4`, `Plane` with `IsGeometricallyEquivalent`, `Complex`), span / tensor overloads (`ReadOnlySpan<double>`/`<float>`, generic `ReadOnlyTensorSpan<T>`), ULP-distance equality (`IsCloseInUlps`), combined relative+absolute tolerance, finiteness/probability/percentage predicates, `HasRoundtripIdentity` for invertible-transformation checks, and (v0.4.0+) the pose primitives `RotationAngleDegrees`, `PositionDistance`, and `IsPoseApproximatelyEqual`.
- *(v0.2.0+)* Adapter packages such as `MathAssertions.TUnit` use an internal per-component / per-cell / first-failing-element failure-message renderer so every compound `IsApproximatelyEqualTo` failure shows the diverging axis and its delta. The renderer is reachable across the assembly boundary via `[InternalsVisibleTo]`; consumers do not interact with it directly.
- `Sequences`: monotonicity (strict + non-strict), boundedness, arithmetic and geometric progressions, `ConvergesTo`, single-step `IsCauchyConvergent`, generic length predicates over `ReadOnlySpan<T>`.
- `Statistics`: Welford's `MeanAndVariance`, `HasMean/Variance/StdDev/Sum/Median/PercentileApproximately`, `IsWithinSigmasOfMean`, `AreAllWithinSigmasOfMean`. Median and percentile use overflow-safe forms.
- `LinearAlgebra`: `Matrix4x4` invariants (`IsSymmetric`, `IsOrthogonal`, `IsRotation`, `IsIdentity`, `HasDeterminantApproximately`, `HasTraceApproximately`, `IsInvertible`) plus `Vector3` pair properties (`AreOrthogonal`, `AreParallel` (scale-invariant), `AngleBetween`, `AreLinearlyIndependent`).
- `NumberTheory`: exact integer predicates over `long`: `IsDivisibleBy`, `IsPrime`, `AreCoprime`, `GreatestCommonDivisor`, `LeastCommonMultiple`, `IsPowerOf`, `IsPerfectSquare`, `IsCongruent`. Primality and perfect-square checks are overflow-safe, and GCD/LCM handle `long.MinValue`; `IsDivisibleBy(long.MinValue, -1)` throws `OverflowException`.
- `PoseRenderer` (`MathAssertions.Render`, public since v0.3.0): renders a position / orientation pose as deterministic, snapshot-friendly text.
- `Geometry3D`: eight primitive `record struct` types (`Sphere`, `AxisAlignedBox`, `OrientedBox`, `Ray3D`, `LineSegment3D`, `Triangle3D`, `Capsule`, `Cylinder`) and five static classes: `Properties` (degeneracy, collinearity, coplanarity), `Containment` (point/box/sphere/OBB/convex hull), `Distance` (point-to-plane/segment/triangle), `Intersection` (sphere-sphere, AABB-AABB, ray-plane/sphere/triangle/AABB with citations to *Real-Time Rendering* §§22.6, 22.7, 22.8), `Pointcloud` (boundedness, centroid, on-plane / on-sphere). Algorithm citations preserved per *Real-Time Collision Detection* and *Real-Time Rendering*.

All tolerance- and sigma-taking methods validate the bound up front; invalid input throws `ArgumentOutOfRangeException` even on early-return paths.

## Test-framework adapters

| Package | Test framework | Status |
|---|---|---|
| [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/) | TUnit | Available now |

Adapters for other test frameworks (NUnit, xUnit, MSTest) are not planned; the family targets TUnit.

## When to install this package directly

Only when authoring a non-TUnit adapter for the assertion family, or when calling `MathTolerance` from a `[GenerateAssertion]` extension on a private type. For any TUnit testing use case, install `MathAssertions.TUnit`.

## Installation

```bash
dotnet add package MathAssertions.TUnit
```

`MathAssertions` comes transitively. You don't need to install it directly unless you're building your own adapter package.

## Stability

The public surface above is semver-bound. This is a 0.x release: additive changes ship in any release, and breaking changes bump the minor version (0.X.0) and are called out in the CHANGELOG. The exact text of the failure messages, rendered by an internal helper in this package, is **not stable** and may gain extra detail or change formatting in any release.

## Repository

[github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit) for the full README, design notes, and roadmap.

## License

[MIT](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/LICENSE). Copyright (c) 2026 John Verheij.
