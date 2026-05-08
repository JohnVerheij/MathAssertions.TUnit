# MathAssertions.TUnit

[![CI](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/ci.yml)
[![CodeQL](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml/badge.svg)](https://github.com/JohnVerheij/MathAssertions.TUnit/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit/branch/main/graph/badge.svg)](https://codecov.io/gh/JohnVerheij/MathAssertions.TUnit)
[![NuGet](https://img.shields.io/nuget/v/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![Downloads](https://img.shields.io/nuget/dt/MathAssertions.TUnit.svg)](https://www.nuget.org/packages/MathAssertions.TUnit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

Tolerance-aware math assertions for .NET. Covers `System.Numerics` compound types, statistics, linear algebra invariants, number theory, and 3D geometry at full buildout. Pure-managed, AOT-compatible, source-gen extensible. Built using TUnit's `[GenerateAssertion]` source generator, so the assertion entry points integrate directly into TUnit's `Assert.That(...)` pipeline.

> **Scope:** Test projects only. Not intended for production code.

---

## Status: v0.0.1 (initial preview)

This first release is a deliberately narrow skeleton that establishes the repository, the package identifiers on nuget.org, the quality bar, and the API style. The full surface lands at v0.1.0.

| Shipped now (v0.0.1) | Planned for v0.1.0 |
|---|---|
| `MathTolerance.IsApproximatelyEqual(double, double, double)` | Vector2 / Vector4 component-wise |
| `MathTolerance.IsApproximatelyEqual(float, float, float)` | Quaternion component-wise + rotational equivalence |
| `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)` | Matrix4x4, Plane, Complex, double[] / float[], spans |
| `Vector3.IsApproximatelyEqualTo(expected, tolerance)` (TUnit fluent) | Statistics: mean, median, variance, stddev, percentile, sigma bounds |
|  | Linear algebra invariants: symmetric, orthogonal, identity, determinant |
|  | Number theory: divisibility, primality, GCD/LCM, congruence |
|  | Geometry3D primitives: Sphere, AABB, OBB, Ray, Triangle plus intersection / containment / pointcloud |

If you need any of the v0.1.0 surface today, install v0.0.1 to lock the dependency relationship and watch the CHANGELOG. Feedback issues are welcome.

## Install

```bash
dotnet add package MathAssertions.TUnit
```

The framework-agnostic core (`MathAssertions`) comes transitively. **Requirements:** TUnit 1.43.11 or later, .NET 10. The package is AOT-compatible, trimmable, and uses no runtime reflection in the assertion path.

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

The fluent assertion auto-imports via `TUnit.Assertions.Extensions`; no extra `using` directive is needed if your test project already uses TUnit.

The framework-agnostic helper (`MathTolerance.IsApproximatelyEqual`) needs an explicit `using MathAssertions;` only when consumed directly (for example, from a `[GenerateAssertion]` extension on a private type).

## Extending to your own types

The same `IsApproximatelyEqualTo` DSL works on any consumer-defined type via a one-line `[GenerateAssertion]` extension that calls `MathTolerance.IsApproximatelyEqual` on each component you care about. Your domain types stay in your own code; the package never sees them.

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

This is the design center of the package: consumers do not have to wait for upstream support of their types; they get a fluent DSL on whatever types they have, in five lines. The v0.1.0 surface scales the library-shipped catalog further along the same pattern.

## Why component-wise rather than Euclidean distance

`IsApproximatelyEqualTo(Vector3, Vector3, tolerance)` compares each component independently:

```text
    Math.Abs((double)a.X - (double)b.X) <= tolerance
 && Math.Abs((double)a.Y - (double)b.Y) <= tolerance
 && Math.Abs((double)a.Z - (double)b.Z) <= tolerance
```

NOT Euclidean distance (`(a - b).Length() <= tolerance`). Reasons:

1. Easier to reason about per-coordinate error. A test failing because Z drifted tells you exactly which coordinate misbehaved.
2. No square-root cost; pure component arithmetic.
3. Matches the most common test-failure mental model: "is each component correct."

If you want Euclidean-distance assertions, compose with TUnit's primitive `IsCloseTo`:

```csharp
await Assert.That(Vector3.Distance(a, b)).IsCloseTo(0, tolerance);
```

## Tolerance precision

Component values widen to `double` before the per-axis comparison, so the caller's `double` tolerance is honored at full precision. Casting the tolerance down to `float` instead would discard up to 22 bits of mantissa and silently round tight tolerances such as `1e-9` to zero.

This precision-preserving cast is locked behavior. Tests pin it; downstream additive overloads inherit it.

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

## Quality bar

- AOT-compatible (`IsAotCompatible=true`), trimmable (`IsTrimmable=true`)
- .NET 10, C# 14, `Nullable=enable`, `TreatWarningsAsErrors=true`
- Five Roslyn analyzer packs at full strength: Meziantou, SonarAnalyzer, Roslynator, Microsoft.VisualStudio.Threading, DotNetProjectFile
- `BannedApiAnalyzers` enforces no-reflection in the assertion path
- 90% line / 90% branch coverage CI gates
- ApiCompat strict mode wired (`PackageValidationBaselineVersion` will pin once v0.0.1 ships)
- Public API surface pinned via snapshot tests on every PR
- External-consumer smoke test plus AOT-publish CI gate on `linux-x64`
- Trusted Publishing (OIDC) to nuget.org, no long-lived secrets
- SLSA v1.0 build provenance, CycloneDX 1.6 SBOM, SPDX 3.0 SBOM, OpenVEX v0.2.0, Sigstore-signed attestations on every release
- Source Link, deterministic builds, embedded PDB
- MIT license throughout, BCL-only runtime dependencies on the consumer-facing path

## Family

Part of an assertion family for TUnit:

- [LogAssertions.TUnit](https://github.com/JohnVerheij/LogAssertions.TUnit): fluent log assertions over `Microsoft.Extensions.Logging.Testing.FakeLogCollector`
- [SnapshotAssertions.TUnit](https://github.com/JohnVerheij/SnapshotAssertions.TUnit): text-snapshot assertions
- [TimeAssertions.TUnit](https://github.com/JohnVerheij/TimeAssertions.TUnit): assertion-level timing budgets via `.And.WithinTimeBudget(...)`

The family composes; any `[GenerateAssertion]`-emitted method from any of these can be followed by any other via `.And`. Single chain, multiple concerns.

## Contributing

Issues and pull requests welcome. Before opening a PR:

- Run `dotnet build` and `dotnet test` locally; the CI pipeline enforces the same quality bar (zero warnings as errors, 90% line / 90% branch coverage minimum).
- Match the existing code style (`.editorconfig` is authoritative; `dotnet format` covers formatting).
- For new assertions, include a test for both the happy path and a representative failure case so the failure-message rendering is verified.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full PR review checklist and API design principles.

## License

[MIT](LICENSE)
