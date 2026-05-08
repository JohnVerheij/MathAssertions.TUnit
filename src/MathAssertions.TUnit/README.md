# MathAssertions.TUnit

[![NuGet](https://img.shields.io/nuget/v/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![Downloads](https://img.shields.io/nuget/dt/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **Scope:** Test projects only. Not intended for production code.

TUnit-native fluent math-assertion DSL for `System.Numerics` compound types and BCL floating-point primitives. NaN-aware, infinity-aware, AOT-compatible, no runtime reflection in the assertion path.

> **Full documentation, "Why component-wise rather than Euclidean", cookbook, design notes, and roadmap:** [github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit)

## Status: v0.0.1 (initial preview)

This first release is a deliberately narrow skeleton. v0.0.1 ships a single fluent entry point: `Vector3.IsApproximatelyEqualTo(expected, tolerance)`. The wider System.Numerics catalog plus statistics, linear-algebra invariants, number theory, and 3D-geometry surface land at v0.1.0.

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

## Entry points (v0.0.1)

| Method | Purpose |
|---|---|
| `IsApproximatelyEqualTo(Vector3 expected, double tolerance)` | Component-wise tolerance check; components widen to `double` for precision; NaN-aware, infinity-aware (matches TUnit's primitive `IsCloseTo` semantics). |

The wider catalog (Vector2/4, Quaternion + rotational equivalence, Matrix4x4, Plane + geometric equivalence, Complex, double[]/float[], spans) lands at v0.1.0 alongside additional method families (statistics, linear-algebra invariants, number theory, 3D geometry).

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
