# MathAssertions.TUnit

[![CI](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml)
[![CodeQL](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit/branch/main/graph/badge.svg)](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit)
[![NuGet](https://img.shields.io/nuget/v/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![Downloads](https://img.shields.io/nuget/dt/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

A TUnit-native fluent math-assertion DSL for `System.Numerics` compound types and BCL floating-point primitives. Built using TUnit's `[GenerateAssertion]` source generator, so the assertion entry points integrate directly into TUnit's `Assert.That(...)` pipeline. NaN-aware, infinity-aware, AOT-compatible, no runtime reflection in the assertion path.

> **Scope:** Test projects only. Not intended for production code.

---

## Status: v0.0.1 (initial preview)

This first release is a deliberately narrow skeleton that establishes the repository, the package identifiers on nuget.org, the quality bar, and the API style. The wider System.Numerics catalog plus statistics, linear-algebra invariants, number theory, and 3D-geometry surface land at v0.1.0. Consumers needing the full v0.1.0 surface today can install v0.0.1 to lock the dependency relationship and watch the CHANGELOG.

| Shipped now (v0.0.1) | Planned for v0.1.0 |
|---|---|
| `MathTolerance.IsApproximatelyEqual(double, double, double)` | Vector2 / Vector4 component-wise |
| `MathTolerance.IsApproximatelyEqual(float, float, float)` | Quaternion component-wise + rotational equivalence |
| `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)` | Matrix4x4, Plane, Complex, double[] / float[], spans |
| `Vector3.IsApproximatelyEqualTo(expected, tolerance)` (TUnit fluent) | Statistics: mean, median, variance, stddev, percentile, sigma bounds |
|  | Linear algebra invariants: symmetric, orthogonal, identity, determinant |
|  | Number theory: divisibility, primality, GCD/LCM, congruence |
|  | Geometry3D: Sphere, AABB, OBB, Ray, Triangle plus intersection / containment / pointcloud |

---

## Table of contents

- [Why this package](#why-this-package)
- [Install](#install)
- [Package layout](#package-layout)
- [Namespaces (and a `GlobalUsings.cs` recommendation)](#namespaces-and-a-globalusingscs-recommendation)
- [Quick start](#quick-start)
- [Why component-wise rather than Euclidean distance](#why-component-wise-rather-than-euclidean-distance)
- [Tolerance precision](#tolerance-precision)
- [Entry points](#entry-points)
- [Failure diagnostics](#failure-diagnostics)
- [Cookbook: common patterns](#cookbook-common-patterns)
- [Modern .NET 10+ practices on display](#modern-net-10-practices-on-display)
- [Design notes](#design-notes)
- [NaN and infinity semantics](#nan-and-infinity-semantics)
- [Stability intent (pre-1.0)](#stability-intent-pre-10)
- [Limitations and future work](#limitations-and-future-work)
- [Family compatibility](#family-compatibility)
- [Pair with](#pair-with)
- [Contributing](#contributing)
- [License](#license)

---

## Why this package

Asserting on tolerance-based equality of `System.Numerics` compound types (Vector2/3/4, Quaternion, Matrix4x4, Plane, Complex) typically devolves into one of:

- Manual per-component plumbing in every test:
  ```csharp
  await Assert.That(Math.Abs(actual.X - expected.X)).IsLessThanOrEqualTo(tolerance);
  await Assert.That(Math.Abs(actual.Y - expected.Y)).IsLessThanOrEqualTo(tolerance);
  await Assert.That(Math.Abs(actual.Z - expected.Z)).IsLessThanOrEqualTo(tolerance);
  ```
- A bespoke `Vector3Assertions` class re-implemented in every project, with subtle differences in NaN-handling and infinity-handling between codebases.

This library replaces both with a single fluent DSL that auto-imports alongside TUnit's own assertions. NaN, infinity, and tolerance-validation semantics match TUnit's `IsCloseTo` primitive so the mental model stays consistent.

## Install

```bash
dotnet add package MathAssertions.TUnit
```

**Requirements:** TUnit 1.43.11 or later, .NET 10. `MathAssertions` (the framework-agnostic core) and TUnit's runtime + assertion deps come transitively. The package is AOT-compatible, trimmable, and uses no runtime reflection in the assertion path.

## Package layout

This repo ships **two** NuGet packages:

| Package | Purpose | Depends on |
|---|---|---|
| [`MathAssertions`](https://www.nuget.org/packages/MathAssertions/) | Framework-agnostic core: `MathTolerance` helpers (NaN-aware, infinity-aware tolerance comparison on double/float/Vector3 in v0.0.1; wider System.Numerics catalog at v0.1.0) | BCL only |
| [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/) | TUnit-specific entry points: `Vector3.IsApproximatelyEqualTo(expected, tolerance)` in v0.0.1; wider fluent surface at v0.1.0 | `MathAssertions` + `TUnit.Assertions` + `TUnit.Core` |

You install `MathAssertions.TUnit`; `MathAssertions` comes transitively. Adapters for other test frameworks (NUnit, xUnit, MSTest) are *not* shipped today; they would reuse the `MathAssertions` core. Open a feature request if you need one.

## Namespaces (and a `GlobalUsings.cs` recommendation)

The two packages place types in two namespaces with deliberately-different scopes:

| Type / member | Namespace | Auto-imported? |
|---|---|---|
| `IsApproximatelyEqualTo()` on Vector3 (source-generated entry) | `TUnit.Assertions.Extensions` | **Yes** (TUnit auto-imports) |
| `MathTolerance` (the framework-agnostic helpers) | `MathAssertions` | **No** (needed at the call site only when invoking helpers directly, e.g. from a `[GenerateAssertion]` extension on a private type) |

The Vector3 fluent assertion auto-imports via `TUnit.Assertions.Extensions`; no extra `using` directive is needed if your test project already uses TUnit. For test projects that consume `MathTolerance` directly (as in the [extending to your own types](#cookbook-common-patterns) cookbook entry below), put the namespace into a single `GlobalUsings.cs` so every test file sees it without ceremony:

```csharp
// tests/MyApp.Tests/GlobalUsings.cs
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

That's the canonical use. The fluent extension auto-imports; `using System.Numerics;` is the only additional thing you need beyond standard TUnit usings.

---

## Why component-wise rather than Euclidean distance

`IsApproximatelyEqualTo(Vector3, Vector3, tolerance)` compares each component independently:

```text
    Math.Abs((double)a.X - (double)b.X) <= tolerance
 && Math.Abs((double)a.Y - (double)b.Y) <= tolerance
 && Math.Abs((double)a.Z - (double)b.Z) <= tolerance
```

NOT Euclidean distance (`(a - b).Length() <= tolerance`). Reasons:

1. **Easier to reason about per-coordinate error.** A test failing because Z drifted tells you exactly which coordinate misbehaved. A Euclidean-distance test failing tells you "something is off somewhere"; you have to instrument the assertion to find out which axis.
2. **No square-root cost.** Pure component arithmetic, no `Math.Sqrt`. Matters more for matrix-element loops at v0.1.0 (16 elements per Matrix4x4 comparison) than for one Vector3, but the rule applies uniformly.
3. **Matches the most common test-failure mental model.** "Is each component correct" maps onto how production code that produces these vectors typically computes them (per-axis from independent inputs).

If you want Euclidean-distance assertions, compose with TUnit's primitive `IsCloseTo`:

```csharp
await Assert.That(Vector3.Distance(a, b)).IsCloseTo(0, tolerance);
```

## Tolerance precision

Component values widen to `double` before the per-axis comparison so the caller's `double` tolerance is honored at full precision. Casting the tolerance down to `float` instead would discard up to 22 bits of mantissa and silently round tight tolerances such as `1e-9` to zero, producing surprising "every-component-equal" results for inputs that visibly differ.

This precision-preserving cast is locked behavior. Tests pin it; v0.1.0 additive overloads (Vector2, Vector4, Quaternion, Matrix4x4, etc.) inherit it.

---

## Entry points

v0.0.1 ships a single fluent entry point. v0.1.0 expands the catalog along the same DSL.

### Vector3 component-wise

| Entry point | Behaviour |
|---|---|
| `IsApproximatelyEqualTo(Vector3 expected, double tolerance)` | Asserts every component (`X`, `Y`, `Z`) of the value is within `tolerance` of the corresponding component of `expected`. Components widen to `double` for precision. NaN-aware, infinity-aware (see [NaN and infinity semantics](#nan-and-infinity-semantics)). |

```csharp
var a = new Vector3(1, 2, 3);
var b = new Vector3(1.0001f, 2.0001f, 3.0001f);

await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-3);
```

---

## Failure diagnostics

Failures render the actual value against the expected value with the supplied tolerance, with no extra `Console.WriteLine` calls needed.

**`IsApproximatelyEqualTo` mismatch:**

```text
Expected:
  to be approximately equal to <1, 2, 99> component-wise within tolerance 0.001

Actual:
  <1, 2, 3>
```

The `<1, 2, 99>` and `<1, 2, 3>` are the standard `Vector3.ToString()` rendering. Per-coordinate inspection is left to the reader; future work tracks a per-axis-difference renderer for v0.1.0+.

---

## Cookbook: common patterns

### Pattern: assert a computed Vector3 against a target

```csharp
[Test]
public async Task ComputedPositionIsApproximatelyAtTarget(CancellationToken ct)
{
    Vector3 target = new(0.300f, 0.150f, 0.450f);
    Vector3 actual = SolveTrajectory(input);

    await Assert.That(actual).IsApproximatelyEqualTo(target, tolerance: 1e-3);
}
```

### Pattern: extend the same DSL to your own domain types

The `IsApproximatelyEqualTo` DSL works on any consumer-defined type via a one-line `[GenerateAssertion]` extension that calls `MathTolerance.IsApproximatelyEqual` on each component you care about. Your domain types stay in your own code; the package never sees them.

```csharp
using MathAssertions;
using TUnit.Assertions.Attributes;

// In your test project, anywhere with a [GenerateAssertion]-eligible static class:
file static class PositionAssertions
{
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this MyPosition value, MyPosition expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value.AsVector3(), expected.AsVector3(), tolerance);
}

// Now in tests:
await Assert.That(actualPosition).IsApproximatelyEqualTo(expectedPosition, tolerance: 1e-6);
```

This is the design center of the package: consumers do not have to wait for upstream support of their types; they get a fluent DSL on whatever types they have, in five lines.

### Pattern: assert each axis individually with `Assert.Multiple`

When per-axis diagnostics matter more than the package's failure-message format, fall back to the primitive:

```csharp
using (Assert.Multiple())
{
    await Assert.That(actual.X).IsCloseTo(target.X, 1e-3);
    await Assert.That(actual.Y).IsCloseTo(target.Y, 1e-3);
    await Assert.That(actual.Z).IsCloseTo(target.Z, 1e-3);
}
```

Both axes report; the failure message names the failing one explicitly. v0.1.0 plans a built-in per-axis-difference renderer that brings this granularity into the single `IsApproximatelyEqualTo` call.

---

## Modern .NET 10+ practices on display

The package is a deliberate showcase of modern .NET conventions:

- **AOT-compatible** (`IsAotCompatible=true`), trimmable (`IsTrimmable=true`), no runtime reflection in the assertion path.
- **Source-generated assertion entries** via TUnit's `[GenerateAssertion]`. No interface implementation required, no reflection at runtime.
- **`CallerArgumentExpression`** on tolerance / value parameters surfaces the caller's expression in failure messages without manual string passing (TUnit handles the wiring).
- **C# 14 file-scoped namespaces** + `Nullable=enable` + `TreatWarningsAsErrors=true` + five Roslyn analyzer packs at full strength (Meziantou, SonarAnalyzer, Roslynator, Microsoft.VisualStudio.Threading, DotNetProjectFile).
- **`Microsoft.CodeAnalysis.BannedApiAnalyzers`** enforces no-reflection at build time via a shared `BannedSymbols.txt`.
- **Allocation-conscious failure rendering** (component cast happens inline; no intermediate vector allocations).

## Design notes

### Why `IsApproximatelyEqualTo` (not `IsCloseTo`)

TUnit core uses `IsCloseTo(expected, tolerance)` for primitive `double` / `float`. Reusing the name on compound types would suggest those overloads use the same scalar-distance semantics; that conflict between mental models would cause real bugs. `IsApproximatelyEqualTo` is the explicit "compound, component-wise, NaN/infinity-aware tolerance" name. Pick TUnit's primitive `IsCloseTo` for scalars; pick `IsApproximatelyEqualTo` for `System.Numerics` compounds. Each says exactly what it does.

### Why widen components to double, not narrow tolerance to float

See the [Tolerance precision](#tolerance-precision) section. The short version: a `double` tolerance must be honored at full `double` precision; the alternative (cast tolerance to float) silently rounds tight tolerances to zero and produces incorrect "equal-to-itself" results in real test scenarios.

### Why no primitive `IsApproximatelyEqual(double, double)` fluent extension

TUnit core already provides `Assert.That(myDouble).IsCloseTo(expected, tolerance)` with identical NaN-aware semantics. Shipping a parallel `Assert.That(myDouble).IsApproximatelyEqualTo(...)` would just create overload-resolution noise. The framework-agnostic `MathTolerance.IsApproximatelyEqual(double, double, double)` exists in the core for callers who need to check primitive tolerance from `[GenerateAssertion]` extensions on their own types; it's not promoted to a TUnit-side fluent call.

## NaN and infinity semantics

Match TUnit's `IsCloseTo` primitive semantics:

| Both | Comparison | Result |
|---|---|---|
| NaN | NaN | `true` (under any tolerance) |
| One NaN, other not | (anything) | `false` |
| Same-sign infinity | (anything) | `true` |
| Opposite-sign infinity | (anything) | `false` |
| Finite | `Math.Abs(a - b) <= tolerance` | as expected |
| Tolerance is NaN or negative | (anything) | `ArgumentOutOfRangeException` at call time |

For the Vector3 overload, every component pair is evaluated against this table; the assertion passes iff all three pairs pass.

## Stability intent (pre-1.0)

This is a 0.x release and the public API may evolve. Specifically:

- **Additive changes** (new entry points, new tolerance overloads, additional `System.Numerics` types) ship in any patch / minor without breaking ApiCompat. The Vector3 entry point shipped in v0.0.1 will still be present, with the same signature, in every subsequent release that targets the same TFM.
- **Breaking changes** to existing signatures bump the minor version (0.X.0) and are called out in the [CHANGELOG](CHANGELOG.md).
- **`PackageValidationBaselineVersion`** pins to v0.0.1 starting from v0.0.2 onward, so ApiCompat breakage is caught at pack time.

The 1.0 milestone signals API stability; see [Limitations and future work](#limitations-and-future-work) for what's still being designed.

## Limitations and future work

### Planned for v0.1.0

The wider catalog lands at v0.1.0. The full surface plus the four review fixes (M-1 through M-4 from the foundational design plan: precision-preserving casts, quaternion normalization for rotational equivalence, plane geometric-equivalence, array-shape null validation):

- **System.Numerics compounds:** Vector2, Vector4, Quaternion (component-wise + rotational equivalence handling the `q` and `-q` double-cover), Matrix4x4 element-wise, Plane component-wise + geometric-equivalence, Complex
- **Span overloads:** `ReadOnlySpan<double>` / `ReadOnlySpan<float>` element-wise
- **Array-shaped adapters:** `double[]` / `float[]` with `ArgumentNullException` on null
- **Statistics:** mean, median, variance, standard deviation, sum, sigma bounds, percentile (linear interpolation)
- **Linear algebra invariants:** symmetric, orthogonal, identity, determinant, trace, invertible, parallel / orthogonal vector pairs
- **Number theory:** divisibility, primality, GCD, LCM, coprimality, congruence, perfect-square, power-of-base
- **Geometry3D primitives** (Sphere, AxisAlignedBox, OrientedBox, Ray3D, LineSegment3D, Triangle3D, Capsule, Cylinder) plus containment, intersection (Moller-Trumbore, slab test), point-distance closed forms, coplanarity / collinearity, basic pointcloud assertions

### Planned for v0.2.0

- **`System.Numerics.Tensors.Tensor<T>` and `ReadOnlyTensorSpan<T>`** support: element-wise tolerance comparison plus shape assertions. Anchors both the geometry-test use case and ML inference output testing.
- **Geometry3D depth:** OrientedBox SAT intersection, Triangle-Triangle, Hausdorff distance, RANSAC inlier-ratio
- **Statistics depth:** distribution support (normal, Student-t, chi-squared), correlation, full percentile-method enum

### Planned for v0.3.0+

- **Mesh validity:** `IndexedMesh` record plus manifold/watertight/convex/volume/surface-area assertions
- **`BigInteger` overloads of number theory**
- **Sibling adapters:** `MathAssertions.MathNet.TUnit`, `MathAssertions.GeometRi.TUnit`, `MathAssertions.Prowl.TUnit`, `MathAssertions.HelixToolkit.TUnit`, `MathAssertions.Fft.TUnit`, `MathAssertions.Fractions.TUnit` (each a separate package; demand-driven)

### Out of scope

- **Statistical inference / hypothesis testing** (different problem domain; out of scope)
- **Symbolic math** (different mental model from tolerance-numeric; out of scope)
- **2D-Euclidean primitives in core** (only 3D; 2D arrives via adapter if a strong 2D library demands it)
- **Production-code use** (per the scope blockquote on every README)

## Family compatibility

The four assertion-family packages release independently and target the same .NET TFM at any moment (LTS-anchored, multi-target during STS support windows; see the [TFM policy in CONVENTIONS.md](CONVENTIONS.md#tfm-policy) for the rotation schedule). **Mix versions freely.** Each package ships under SemVer with `EnablePackageValidation` strict-mode ApiCompat against its previous baseline, so binary breaks within a version line are caught at pack time.

For per-package release notes:

- [LogAssertions.TUnit CHANGELOG](https://github.com/JohnVerheij/LogAssertions.TUnit/blob/main/CHANGELOG.md)
- [SnapshotAssertions.TUnit CHANGELOG](https://github.com/JohnVerheij/SnapshotAssertions.TUnit/blob/main/CHANGELOG.md)
- [TimeAssertions.TUnit CHANGELOG](https://github.com/JohnVerheij/TimeAssertions.TUnit/blob/main/CHANGELOG.md)
- [MathAssertions.TUnit CHANGELOG](https://github.com/JohnVerheij/MathAssertions.TUnit/blob/main/CHANGELOG.md)

## Pair with

- **[`LogAssertions.TUnit`](https://www.nuget.org/packages/LogAssertions.TUnit/):** fluent log assertions over `Microsoft.Extensions.Logging.Testing.FakeLogCollector`.
- **[`SnapshotAssertions.TUnit`](https://www.nuget.org/packages/SnapshotAssertions.TUnit/):** text-snapshot assertions for API-surface tests and similar deterministic-string scenarios. Coexists with Verify; covers the 80% case without coverage friction.
- **[`TimeAssertions.TUnit`](https://www.nuget.org/packages/TimeAssertions.TUnit/):** assertion-level timing budgets via `.And.WithinTimeBudget(...)`. Compose with any `IsApproximatelyEqualTo` chain for combined value+timing assertions.

## Contributing

Issues and pull requests welcome. Before opening a PR:

- Run `dotnet build` and `dotnet test` locally; the CI pipeline enforces the same quality bar (zero warnings as errors, 90% line / 90% branch coverage minimum).
- Match the existing code style (`.editorconfig` is authoritative; `dotnet format` covers formatting).
- For new assertions, include a test for both the happy path and a representative failure case so the failure-message rendering is verified.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full PR review checklist and API design principles.

## License

[MIT](LICENSE)
