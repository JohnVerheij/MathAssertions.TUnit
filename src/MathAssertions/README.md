# MathAssertions

> **Scope:** Test projects only. Not intended for production code.

Framework-agnostic core for the [MathAssertions.TUnit](https://www.nuget.org/packages/MathAssertions.TUnit/) family. Most users should install **`MathAssertions.TUnit`** instead; it depends on this package transitively and ships the actual TUnit fluent assertion entry points.

## What this package contains (v0.0.1)

`MathTolerance` static class with pure, NaN-aware, infinity-aware tolerance-comparison helpers:

- `IsApproximatelyEqual(double, double, double)`
- `IsApproximatelyEqual(float, float, float)`
- `IsApproximatelyEqual(Vector3, Vector3, double)` (component-wise; components widen to `double` so a tight `double` tolerance is honored at full precision)

All helpers reject NaN and negative tolerance via `ArgumentOutOfRangeException`.

## What lands at v0.1.0

Vector2 / Vector4 component-wise; Quaternion component-wise plus rotational equivalence (`q` and `-q` double-cover); Matrix4x4 element-wise; Plane component-wise plus geometric equivalence; Complex; span overloads; statistics (mean, median, variance, percentile); linear-algebra invariants; number theory; 3D geometry primitives plus intersection / containment / pointcloud assertions.

## When to install this package directly

Only when authoring a non-TUnit adapter for the assertion family (an xUnit, NUnit, or MSTest adapter) or when calling `MathTolerance` from a `[GenerateAssertion]` extension on a private type. For any TUnit testing use case, install `MathAssertions.TUnit`.

## Repository

[github.com/JohnVerheij/MathAssertions.TUnit](https://github.com/JohnVerheij/MathAssertions.TUnit) for the full README, design notes, and roadmap.

## License

[MIT](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/LICENSE)
