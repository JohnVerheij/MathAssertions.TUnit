# MathAssertions.TUnit

[![CI](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml)
[![CodeQL](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit/branch/main/graph/badge.svg)](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit)
[![NuGet](https://img.shields.io/nuget/v/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![Downloads](https://img.shields.io/nuget/dt/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

TUnit-native math assertion library for .NET. Covers tolerance comparisons (scalar, `System.Numerics` compounds, spans, tensors), sequences, statistics, linear algebra, number theory, and 3D geometry. Source-generated entry points integrate with TUnit's `Assert.That(...)` pipeline. NaN-aware, infinity-aware, AOT-compatible, reflection-free in the assertion path.

> **Scope:** Test projects only. Not intended for production code.

---

## Status: v0.2.0 (per-component diagnostics, axis-angle assertions)

The framework-agnostic core is feature-complete across seven topical clusters and the TUnit adapter exposes a near-full fluent surface over `Assert.That(value).Method(...)`; the documented exception is `ReadOnlyTensorSpan<T>`, which is exposed only at the static `MathTolerance` level (see the carve-out note below the table). v0.2.0 adds rich per-component / per-cell delta rendering to every compound `IsApproximatelyEqualTo` failure message and ships `HasAxisAngleApproximately` for axis-angle-form quaternion rotation checks. Every fluent entry point is generated via TUnit's `[GenerateAssertion]` source generator and integrates directly into the existing `Assert.That(...)` pipeline.

| Domain | Coverage |
|---|---|
| Tolerance primitives | Scalar / vector / quaternion / matrix / plane / complex / span / tensor `IsApproximatelyEqual`; ULP-distance equality; combined relative + absolute tolerance; finiteness, probability, percentage predicates; invertible-transformation roundtrip-identity |
| `System.Numerics` compounds | `Vector2`/`Vector3`/`Vector4` component-wise; `Quaternion` component-wise plus rotational equivalence (SO(3) double-cover); `Matrix4x4` element-wise; `Plane` component-wise plus geometric equivalence; `Complex`; `ReadOnlySpan<double>`/`<float>`; generic `ReadOnlyTensorSpan<T>` |
| Sequences | Monotonicity (strict and non-strict), sortedness, boundedness with NaN-aware element check, arithmetic / geometric progressions, single-step Cauchy convergence, generic length predicates |
| Statistics | Welford's mean + variance, median, percentile (overflow-safe), standard deviation, sum, sigma-bound checks |
| Linear algebra | `Matrix4x4` invariants (symmetric, orthogonal, identity, determinant, trace, invertible); `Vector3` pair properties (orthogonality, parallelism, linear independence) |
| Number theory | `long` predicates: divisibility, primality (overflow-safe), coprimality, GCD/LCM (`long.MinValue`-aware, `OverflowException`-on-LCM-overflow), powers of base, perfect square (overflow-safe successor check), modular congruence (canonical-residue form) |
| Geometry3D | Eight primitive `record struct` types (`Sphere`, `AxisAlignedBox`, `OrientedBox`, `Ray3D`, `LineSegment3D`, `Triangle3D`, `Capsule`, `Cylinder`); property predicates (`IsDegenerate`, `IsCollinear`, `AreCoplanar`); containment (point/box/sphere/OBB/convex hull); closest-point distance (point→plane/segment/triangle, citing Ericson, *Real-Time Collision Detection* §§5.1.2, 5.1.5); intersection (sphere-sphere, AABB-AABB, ray-plane/sphere/triangle/AABB, citing Akenine-Möller, Haines, Hoffman, *Real-Time Rendering* 4th ed. §§22.6, 22.7, 22.8); pointcloud aggregates (boundedness, centroid, on-plane, on-sphere) |

`MathAssertions.TUnit` ships ~85 fluent extensions across twelve adapter classes; `MathAssertions` (the framework-agnostic core) ships the underlying static helpers. See the per-package READMEs and CHANGELOG for the exhaustive method listings.

`ReadOnlyTensorSpan<T>` is exposed only at the static `MathTolerance` level, not as a fluent assertion: TUnit's `[GenerateAssertion]` assertion-builder cannot capture ref-struct values across an `await`. Tensor-fluent integration is candidate work for a later release.

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

**Requirements:** TUnit 1.44.0 or later, .NET 10. `MathAssertions` (the framework-agnostic core) and TUnit's runtime + assertion deps come transitively. The package is AOT-compatible, trimmable, and uses no runtime reflection in the assertion path.

## Package layout

This repo ships **two** NuGet packages:

| Package | Purpose | Depends on |
|---|---|---|
| [`MathAssertions`](https://www.nuget.org/packages/MathAssertions/) | Framework-agnostic core: `MathTolerance`, `Sequences`, `Statistics`, `LinearAlgebra`, `NumberTheory`, and the `Geometry3D` namespace (~85 static methods plus eight primitive `record struct` types) | BCL + `System.Numerics.Tensors` |
| [`MathAssertions.TUnit`](https://www.nuget.org/packages/MathAssertions.TUnit/) | TUnit-specific fluent entry points generated via `[GenerateAssertion]`: ~85 `Assert.That(value).Method(...)` extensions covering the whole core surface | `MathAssertions` + `TUnit.Assertions` + `TUnit.Core` |

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

~85 fluent entry points across twelve adapter classes cover scalar, `System.Numerics` compounds, `double[]`/`float[]`, sequences, statistics, linear algebra, integer number theory, and a complete 3D-geometry surface. v0.2.0 adds `HasAxisAngleApproximately` on `Quaternion` and per-component delta rendering in every compound failure message. The exhaustive method listing lives in the [package README](src/MathAssertions.TUnit/README.md#entry-points); the examples below are representative.

### Vector3 component-wise

| Entry point | Behaviour |
|---|---|
| `IsApproximatelyEqualTo(Vector3 expected, double tolerance)` | Asserts every component (`X`, `Y`, `Z`) of the value is within `tolerance` of the corresponding component of `expected`. Components widen to `double` for precision. NaN-aware, infinity-aware (see [NaN and infinity semantics](#nan-and-infinity-semantics)). |

```csharp
var a = new Vector3(1, 2, 3);
var b = new Vector3(1.0001f, 2.0001f, 3.0001f);

await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-3);
```

### Quaternion rotational equivalence

```csharp
var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2);
var negated = new Quaternion(-rotation.X, -rotation.Y, -rotation.Z, -rotation.W);

// q and -q encode the same rotation; component-wise IsApproximatelyEqualTo would fail.
await Assert.That(rotation).IsRotationallyEquivalentTo(negated, tolerance: 1e-6);
```

### Quaternion axis-angle form

```csharp
// "the rotation under test is approximately 90 degrees around the Y axis"
var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2);

await Assert.That(rotation)
    .HasAxisAngleApproximately(Vector3.UnitY, expectedAngleDegrees: 90.0, tolerance: 1e-4);
```

`HasAxisAngleApproximately` handles the SO(3) double cover and the 180-degree boundary; both `q` and `-q` for the same rotation, as well as `(axis, +180°)` / `(axis, -180°)` / `(-axis, ±180°)`, compare equivalent to the same expected axis-angle pair. Internally compares via the rotational-equivalence dot-product test, then renders the extracted axis / angle in the failure message for diagnosis.

### Statistics on a sample

```csharp
double[] sample = [1.0, 2.0, 3.0, 4.0, 5.0];

await Assert.That(sample).HasMeanApproximately(3.0, tolerance: 1e-9);
await Assert.That(sample).HasStdDevApproximately(Math.Sqrt(2.5), tolerance: 1e-9);
await Assert.That(sample).HasPercentileApproximately(50.0, expected: 3.0, tolerance: 1e-9);
```

### 3D-geometry containment / intersection

```csharp
var sphere = new Sphere(Vector3.Zero, 1f);
await Assert.That(sphere).ContainsPoint(new Vector3(0.3f, 0.3f, 0.3f));

var ray = new Ray3D(new Vector3(-3, 0, 0), Vector3.UnitX);
await Assert.That(ray).IntersectsSphere(sphere);
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

v0.2.0+ enriches the rendering with per-axis delta information directly in the failure message:

```text
Expected:
  to be approximately equal to <1, 2, 3.001> component-wise within tolerance 1E-06
    actual:   <1, 2, 3>
    delta:    (0, 0, 0.0009999275)
    exceeded: Z (0.0009999275 > 1E-06)
```

The `delta` and `exceeded` values use general (`G`) numeric formatting with `CultureInfo.InvariantCulture`, so the rendered numbers carry full precision (including any float-to-double widening residue) rather than a rounded display form. The same pattern applies to every compound type (`Vector2`/`Vector4`/`Quaternion`/`Matrix4x4`/`Plane`/`Complex`/`double[]`/`float[]`) plus `IsRotationallyEquivalentTo`, `IsGeometricallyEquivalentTo`, and `HasAxisAngleApproximately`. The exact text format is not stable; see the [Stability intent](#stability-intent-pre-10) section.

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

Both axes report; the failure message names the failing one explicitly. A built-in per-axis-difference renderer that brings this granularity into the single `IsApproximatelyEqualTo` call is candidate work for a later release.

### Pattern: comparing quaternions

`IsApproximatelyEqualTo` is component-wise. Two unit quaternions `q` and `-q` represent the same physical rotation (the SO(3) double-cover) but fail component-wise comparison. If the production code may emit either sign of a unit quaternion (calibration outputs, slerp interpolation, normalization that picks a sign), use `IsRotationallyEquivalentTo`:

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

Use `IsApproximatelyEqualTo` when component identity matters (serialization roundtrips, exact storage formats). Use `IsRotationallyEquivalentTo` when geometric meaning matters (physical orientations, rotations applied to vectors). The implementation normalizes both inputs internally, so non-unit operands produce the correct rotational verdict.

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

### Verifying invertible transformations: `HasRoundtripIdentity`

When a transformation is invertible (`f` and its inverse `g`, where `g(f(x)) == x` within tolerance), `MathTolerance.HasRoundtripIdentity(double input, Func<double,double> forward, Func<double,double> backward, double tolerance)` checks the round-trip in one call. The primitive is `double`-only on purpose; consumers compose their own predicate for other types.

Three worked examples:

```csharp
// 1. Sin and Asin compose back to the input within tolerance.
await Assert.That(MathTolerance.HasRoundtripIdentity(
    0.42, Math.Sin, Math.Asin, tolerance: 1e-12)).IsTrue();

// 2. Degree / radian conversion.
await Assert.That(MathTolerance.HasRoundtripIdentity(
    45.0,
    d => d * Math.PI / 180.0,
    r => r * 180.0 / Math.PI,
    tolerance: 1e-12)).IsTrue();

// 3. Encode / decode, with consumer-supplied delegates wrapping the under-test API.
//    Useful for serializer roundtrips when the encode / decode operate on a numeric
//    representation (e.g. epoch milliseconds <-> DateTimeOffset).
await Assert.That(MathTolerance.HasRoundtripIdentity(
    epochMs,
    ms => DateTimeOffset.FromUnixTimeMilliseconds((long)ms).ToUnixTimeMilliseconds(),
    ms => (double)ms,
    tolerance: 0.5)).IsTrue();
```

When the round-trip needs typed inputs (vector / quaternion / serializer pair), compose `[GenerateAssertion]` on your own type rather than chaining `HasRoundtripIdentity`: the primitive is intentionally `double`-only so the family does not lock in a particular cross-type roundtrip surface.

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

This reads as "the quaternion is approximately the zero quaternion" and fails for any rotation including the identity rotation `Quaternion.Identity` (which has `W = 1`). No specialized `IsZeroQuaternion` extension is needed; the BCL constant plus the existing component-wise comparison cover the case.

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

- **Additive changes** (new entry points, new tolerance overloads, additional `System.Numerics` types) ship in any patch without breaking ApiCompat. Entry points present in a prior version remain present, with compatible signatures, in every subsequent release that targets the same TFM.
- **Breaking changes** to existing signatures bump the minor version (0.X.0) and are called out in the [CHANGELOG](CHANGELOG.md). v0.2.0 evolved the source-method return types of the compound `IsApproximatelyEqualTo` family from `bool` to `AssertionResult` to enable rich per-component failure messages; the generated TUnit chain extensions (`Assert.That(value).IsApproximatelyEqualTo(...)`) are unaffected at the chain-syntax level.
- **`PackageValidationBaselineVersion`** pins to the previous shipped version (v0.1.0 as of v0.2.0), so ApiCompat breakage is caught at pack time. Strict-mode baseline validation captures additive changes and intentional API evolution as accepted entries in `CompatibilitySuppressions.xml`.

The 1.0 milestone signals API stability; see [Limitations and future work](#limitations-and-future-work) for what's still being designed.

## Limitations and future work

### Shipped at v0.1.0

Foundational catalog established in v0.1.0:

- **System.Numerics compounds:** Vector2/3/4, Quaternion (component-wise + rotational equivalence handling the `q` / `-q` double-cover), Matrix4x4 element-wise, Plane component-wise + geometric-equivalence, Complex
- **Span / tensor overloads:** `ReadOnlySpan<double>` / `ReadOnlySpan<float>` element-wise; generic `ReadOnlyTensorSpan<T>` for the static `MathTolerance` surface
- **Array-shaped adapters:** `double[]` / `float[]` fluent extensions with `ArgumentNullException` on null
- **Statistics:** Welford's mean + variance, median, percentile (overflow-safe), standard deviation, sum, sigma bounds
- **Linear algebra invariants:** symmetric, orthogonal, identity, determinant, trace, invertible, parallel / orthogonal vector pairs, linear independence
- **Number theory:** divisibility, primality, GCD, LCM, coprimality, congruence, perfect-square, power-of-base, all with overflow-safe inner loops and `long.MinValue`-aware contracts
- **Geometry3D primitives** (Sphere, AxisAlignedBox, OrientedBox, Ray3D, LineSegment3D, Triangle3D, Capsule, Cylinder) plus containment, intersection (Möller-Trumbore, slab test), point-distance closed forms (Ericson barycentric Voronoi-region classification), coplanarity / collinearity, pointcloud aggregates

### Shipped at v0.2.0

- **Per-component / per-cell delta failure messages** for every compound `IsApproximatelyEqualTo` chain plus `IsRotationallyEquivalentTo` and `IsGeometricallyEquivalentTo`. Implementation detail; failure-message text is not part of the stable public surface (callers should pin filter / match-count expectations rather than full message-text equality).
- **`HasAxisAngleApproximately`** on `Quaternion` (both as a `MathTolerance` static and as a fluent extension). Handles the SO(3) double cover and the 180-degree boundary uniformly via the rotational-equivalence dot-product test.

### Planned for v0.3.0+

- **`ReadOnlyTensorSpan<T>` fluent adapter:** the static `MathTolerance.IsApproximatelyEqual(ReadOnlyTensorSpan<T>, ...)` overload exists today; the fluent `await Assert.That(span).IsApproximatelyEqualTo(...)` form is blocked on TUnit's assertion-builder being unable to capture ref-struct values across an `await` and is candidate work for a later release
- **Geometry3D depth:** OrientedBox SAT intersection, Triangle-Triangle, Hausdorff distance, RANSAC inlier-ratio
- **Statistics depth:** distribution support (normal, Student-t, chi-squared), correlation, full percentile-method enum
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

For larger ideas (new entry points, breaking changes, cross-cutting refactors), open a [Discussion](https://github.com/JohnVerheij/MathAssertions.TUnit/discussions) first to align on direction before investing implementation time.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full PR review checklist and API design principles, and [CONVENTIONS.md](CONVENTIONS.md) for the family-wide code conventions shared across `LogAssertions.TUnit`, `SnapshotAssertions.TUnit`, `TimeAssertions.TUnit`, and this repo.

## License

[MIT](LICENSE). Copyright (c) 2026 John Verheij.
