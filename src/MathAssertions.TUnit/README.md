# MathAssertions.TUnit

> **Scope:** Test projects only. Not intended for production code.

Tolerance-aware math assertions for [TUnit](https://github.com/thomhurst/TUnit). NaN-aware, infinity-aware, AOT-compatible, no runtime reflection.

## Status: v0.0.1 (initial preview)

This first release ships a deliberately narrow skeleton to establish the package and lock the API style. The wider System.Numerics catalog plus statistics, linear-algebra invariants, number theory, and 3D geometry land at 0.1.0.

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

## What this package adds (v0.0.1)

A single fluent entry point: `Vector3.IsApproximatelyEqualTo(expected, tolerance)`, generated via TUnit's `[GenerateAssertion]`. Component values widen to `double` so a `double` tolerance is honored at full precision.

## What lands at v0.1.0

Vector2 / Vector4, Quaternion (component-wise plus rotational equivalence), Matrix4x4, Plane (component-wise plus geometric equivalence), Complex, double[] / float[], plus the full statistics, linear-algebra, number-theory, and 3D-geometry surface.

## Quick start

```bash
dotnet add package MathAssertions.TUnit
```

`MathAssertions` (the framework-agnostic core) comes transitively. **Requirements:** TUnit 1.43.11 or later, .NET 10.

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

## Family

- [LogAssertions.TUnit](https://github.com/JohnVerheij/LogAssertions.TUnit)
- [SnapshotAssertions.TUnit](https://github.com/JohnVerheij/SnapshotAssertions.TUnit)
- [TimeAssertions.TUnit](https://github.com/JohnVerheij/TimeAssertions.TUnit)

## Documentation

[github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit) for the full README, design notes, and roadmap.

## License

[MIT](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/LICENSE)
