# MathAssertions.TUnit

[![NuGet](https://img.shields.io/nuget/v/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![Downloads](https://img.shields.io/nuget/dt/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **Scope:** Test projects only. Not intended for production code.

TUnit-native math assertion library for .NET. Covers tolerance comparisons, sequences, statistics, linear algebra, number theory, and 3D geometry. NaN-aware, infinity-aware, AOT-compatible, no runtime reflection in the assertion path.

> **Full documentation, "Why component-wise rather than Euclidean", cookbook, design notes, and roadmap:** [github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit)

## Status: v0.1.0 (the wider catalog)

The whole 0.1.0 mathematical-assertion surface is now exposed as fluent extensions over TUnit's `Assert.That(value).Method(...)`. ~85 fluent entry points across twelve adapter classes covering scalar tolerance comparisons, the `System.Numerics` compound types (vector / quaternion / matrix / plane / complex), `double[]` / `float[]` element-wise comparison with null-array guards, sequence properties, descriptive statistics, linear-algebra invariants, integer number theory, and a complete 3D-geometry primitive surface (containment / closest-point distance / intersection / pointcloud aggregates).

## Install

```bash
dotnet add package MathAssertions.TUnit
```

`MathAssertions` (the framework-agnostic core) comes transitively. **Requirements:** TUnit 1.43.11 or later, .NET 10.

The source-generated entry point (`IsApproximatelyEqualTo` on `Vector3`) auto-imports via `TUnit.Assertions.Extensions`. The only additional `using` you typically need is `System.Numerics` for the value type itself. If you call `MathTolerance.IsApproximatelyEqual` directly from a `[GenerateAssertion]` extension on a private domain type, add `MathAssertions` to your `GlobalUsings.cs`:

```csharp
global using MathAssertions;
```

## Quick start

```csharp
using System.Numerics;

[Test]
public async Task ComputedPositionIsApproximatelyAtTarget(CancellationToken ct)
{
    Vector3 target = new(0.300f, 0.150f, 0.450f);
    Vector3 actual = SolveTrajectory(input);

    await Assert.That(actual).IsApproximatelyEqualTo(target, tolerance: 1e-3);
}
```

## Entry points (v0.1.0)

The fluent surface, organized by adapter class:

| Class | Coverage |
|---|---|
| `ScalarAssertions` | `IsApproximatelyEqualTo`, `IsCloseInUlpsTo`, `IsRelativelyAndAbsolutelyCloseTo`, `IsNonNegativeFinite`, `IsProbability`, `IsPercentage`, `HasRoundtripIdentity` for `double`/`float`. |
| `VectorAssertions` | `IsApproximatelyEqualTo` for `Vector2`/`Vector3`/`Vector4`; `Vector3.HasMagnitudeApproximately`, `IsNormalized`. |
| `QuaternionAssertions` | `IsApproximatelyEqualTo`, `IsRotationallyEquivalentTo` (SO(3) double-cover), `IsIdentity`, `IsNormalized`. |
| `MatrixAssertions` | `Matrix4x4.IsApproximatelyEqualTo` plus the full invariant surface: `IsSymmetric`, `IsOrthogonal`, `IsIdentity`, `HasDeterminantApproximately`, `HasTraceApproximately`, `IsInvertible`. |
| `PlaneAssertions` | `IsApproximatelyEqualTo`, `IsGeometricallyEquivalentTo` (sign-flip equivalence). |
| `ComplexAssertions` | `IsApproximatelyEqualTo`. |
| `ArrayAssertions` | `double[]` / `float[]` element-wise `IsApproximatelyEqualTo` with `ArgumentNullException` on null arrays. |
| `SequencesAssertions` | Monotonicity, `IsBounded`, `IsArithmeticProgression`, `IsGeometricProgression`, `ConvergesTo`, `IsCauchyConvergent`, generic `HasLength`, `HasMinLength` over `T[]`. |
| `StatisticsAssertions` | `HasMean/Variance/StdDev/Sum/Median/PercentileApproximately`, `IsWithinSigmasOfMean`, `AreAllWithinSigmasOfMean`. |
| `LinearAlgebraAssertions` | `Vector3.IsOrthogonalTo`, `IsParallelTo`; `Vector3[].AreLinearlyIndependent`. |
| `NumberTheoryAssertions` | `long`-integer predicates: `IsDivisibleBy`, `IsPrime`, `IsCoprimeWith`, `IsPowerOf`, `IsPerfectSquare`, `IsCongruentTo`. |
| `Geometry3DAssertions` | Triangle / point-set property predicates, containment (point/box/sphere/OBB/convex hull), predicate-style `HasDistanceFromPlane/Segment/Triangle`, ray-plane/sphere/triangle/AABB intersection, pointcloud aggregates. |

`IsFinite()` for `double`/`float` is provided by TUnit's built-in `DoubleAssertionExtensions`/`SingleAssertionExtensions`; this package does not duplicate it.

## Cookbook

### Pattern: comparing quaternions

`IsApproximatelyEqualTo` is component-wise. Two unit quaternions `q` and `-q` represent the same physical rotation but fail component-wise comparison. If the production code may emit either sign of a unit quaternion (calibration outputs, slerp interpolation, normalization that picks a sign), use `IsRotationallyEquivalentTo`:

```csharp
[Test]
public async Task RotationOutputMatchesExpected(CancellationToken ct)
{
    Quaternion expected = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4);
    Quaternion actual = SolveOrientation(input);  // may emit ±expected

    // Component-wise: fails if SolveOrientation returned -expected
    // await Assert.That(actual).IsApproximatelyEqualTo(expected, tolerance: 1e-6);

    // Rotational: treats q and -q as the same rotation
    await Assert.That(actual).IsRotationallyEquivalentTo(expected, tolerance: 1e-6);
}
```

Use `IsApproximatelyEqualTo` when component identity matters (serialization roundtrips, exact storage formats). Use `IsRotationallyEquivalentTo` when geometric meaning matters (physical orientations, rotations applied to vectors).

### Pattern: comparing planes

`IsApproximatelyEqualTo` is component-wise on the plane equation `(n.X, n.Y, n.Z, d)`. The same geometric plane has two valid representations: `(n, d)` and `(-n, -d)`. If the production code constructs planes via different paths (three-point, normal-and-distance, normal-and-point), the sign of the normal may differ between expected and actual without changing the plane. Use `IsGeometricallyEquivalentTo`:

```csharp
[Test]
public async Task GroundPlaneMatchesExpected(CancellationToken ct)
{
    Plane expected = new(Vector3.UnitY, -1.0f);  // y = 1
    Plane actual = ComputeGroundPlane(input);    // could be (UnitY, -1) or (-UnitY, 1)

    await Assert.That(actual).IsGeometricallyEquivalentTo(expected, tolerance: 1e-6);
}
```

Use `IsApproximatelyEqualTo` when normal direction is observable in the consumer (winding-aware shading, half-space convention). Use `IsGeometricallyEquivalentTo` when only the plane's geometry is observable.

### Pattern: detecting an unpopulated zero quaternion

A zero-valued quaternion (all four components zero, length zero) is a valid in-memory state but not a valid rotation. Common cases: a protobuf default for an unpopulated `Quaternion` field, an in-construction calibration output that has not yet been written, an explicit "no rotation set" sentinel.

The BCL exposes `Quaternion.Zero`. The existing component-wise comparison handles the assertion without a specialized API:

```csharp
[Test]
public async Task UnpopulatedRotationIsZeroSentinel(CancellationToken ct)
{
    Quaternion actual = ReadRotationFromMessage(message);

    await Assert.That(actual).IsApproximatelyEqualTo(Quaternion.Zero, tolerance: 1e-9);
}
```

This reads as "the quaternion is approximately the zero quaternion" and fails for any rotation including the identity rotation `Quaternion.Identity` (which has `W = 1`).

## NaN and infinity semantics

The fluent extensions match the underlying `MathTolerance` semantics, which match TUnit's `IsCloseTo` primitive:

| Both | Comparison | Result |
|---|---|---|
| NaN | NaN | `true` (under any tolerance) |
| One NaN, other not | (anything) | `false` |
| Same-sign infinity | (anything) | `true` |
| Opposite-sign infinity | (anything) | `false` |
| Finite | `Math.Abs(a - b) <= tolerance` | as expected |
| Tolerance is NaN or negative | (anything) | `ArgumentOutOfRangeException` at call time |

For compound types (`Vector2`/`Vector3`/`Vector4`, `Quaternion`, `Matrix4x4`, `Plane`, `Complex`), every component pair is evaluated against this table; the assertion passes iff every pair passes.

## Extending to your own types

Add tolerance assertions for your own domain types via a one-line `[GenerateAssertion]` extension calling `MathTolerance.IsApproximatelyEqual` on each component. The package never sees your private types.

```csharp
using MathAssertions;
using TUnit.Assertions.Attributes;

file static class PositionAssertions
{
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this MyPosition value, MyPosition expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value.AsVector3(), expected.AsVector3(), tolerance);
}
```

## Failure diagnostics

On a failed assertion, the exception message renders the actual `Vector3` against the expected and the supplied tolerance:

```text
Expected:
  to be approximately equal to <1, 2, 99> component-wise within tolerance 0.001

Actual:
  <1, 2, 3>
```

[Full failure-diagnostics examples, design notes, stability intent, and roadmap on GitHub.](https://github.com/JohnVerheij/MathAssertions.TUnit#failure-diagnostics)

## Family

Part of an assertion family for TUnit:

- [LogAssertions.TUnit](https://github.com/JohnVerheij/LogAssertions.TUnit)
- [SnapshotAssertions.TUnit](https://github.com/JohnVerheij/SnapshotAssertions.TUnit)
- [TimeAssertions.TUnit](https://github.com/JohnVerheij/TimeAssertions.TUnit)

## License

[MIT](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/LICENSE). Copyright (c) 2026 John Verheij.
