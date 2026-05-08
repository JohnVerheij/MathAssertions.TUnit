# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added (`MathAssertions`, framework-agnostic core) — 0.1.0 Cluster 5: linear-algebra invariants

New static class `MathAssertions.LinearAlgebra` covering `Matrix4x4` invariants and `Vector3` pair / span properties:

- `LinearAlgebra.IsSymmetric(Matrix4x4, double)`: every off-diagonal pair is mirror-equal within tolerance.
- `LinearAlgebra.IsOrthogonal(Matrix4x4, double)`: `M * M^T ~ I` within tolerance. Translation matrices are not orthogonal (translation column breaks the product); pure rotations are. Non-uniform scaling is not (the diagonal of `M * M^T` picks up the squared scale).
- `LinearAlgebra.IsIdentity(Matrix4x4, double)`: convenience wrapper over `MathTolerance.IsApproximatelyEqual` against `Matrix4x4.Identity`.
- `LinearAlgebra.HasDeterminantApproximately(Matrix4x4, double, double)`: tolerance-aware determinant check via `Matrix4x4.GetDeterminant`.
- `LinearAlgebra.HasTraceApproximately(Matrix4x4, double, double)`: tolerance-aware trace check (sum of diagonal elements, widened to `double` before summing).
- `LinearAlgebra.IsInvertible(Matrix4x4, double)`: `|det(M)| > tolerance`. The threshold expresses how far from singular the matrix must be to invert numerically; choose a tolerance that reflects the expected condition number.
- `LinearAlgebra.AreOrthogonal(Vector3, Vector3, double)`: dot product within tolerance of zero.
- `LinearAlgebra.AreParallel(Vector3, Vector3, double)`: cross-product magnitude within tolerance of zero. The zero vector is treated as parallel to every other vector by this definition (`0 x v = 0`).
- `LinearAlgebra.AreLinearlyIndependent(ReadOnlySpan<Vector3>, double)`: triple-product test for sets of up to three vectors in `R^3`. Spans of four or more vectors are always dependent and return `false`. Empty spans are vacuously independent. The length-1 case compares vector length directly against tolerance rather than length-squared against tolerance-squared, so the verdict is well-defined for extreme tolerance magnitudes (where squaring would underflow or overflow).

All tolerance-taking methods validate the bound up front, including the
`AreLinearlyIndependent` empty-span vacuous-true path. Same family-wide
validation-order pattern that `Sequences.ConvergesTo`, `Sequences.IsBounded`, and
`Statistics.HasMeanApproximately` enforce.

### Added (`MathAssertions`, framework-agnostic core) — 0.1.0 Cluster 4: statistical-property checks

New static class `MathAssertions.Statistics`:

- `Statistics.MeanAndVariance(ReadOnlySpan<double>)`: returns the sample mean and unbiased sample variance (`N-1` denominator) in a single numerically stable pass via Welford's online algorithm (Knuth, *The Art of Computer Programming Vol. 2*, §4.2.2). Empty spans yield `(NaN, NaN)`; single-element spans yield `(value, 0)`.
- `Statistics.HasMeanApproximately`, `HasVarianceApproximately`, `HasStdDevApproximately`, `HasSumApproximately`: tolerance-aware checks of the corresponding moment against an expected value. Variance and standard deviation require at least two observations; the sum of an empty sample is zero by convention.
- `Statistics.HasMedianApproximately`: tolerance-aware median check; for even-length samples the median is the mean of the two middle values after sorting. Sorts a copy of the input so callers do not observe a side effect on the original span.
- `Statistics.HasPercentileApproximately`: tolerance-aware percentile check using linear interpolation between adjacent ranks per the NIST/SEMATECH e-Handbook of Statistical Methods §1.3.5.6. Validates the percentile is in `[0, 100]` and not NaN.
- `Statistics.IsWithinSigmasOfMean(value, sample, sigmas)`: returns `true` when the value lies within the requested number of standard deviations of the sample's mean.
- `Statistics.AreAllWithinSigmasOfMean(values, sigmas)`: returns `true` when every value lies within the requested sigma envelope of the sample's own mean. Computes mean and standard deviation once and reuses them across all element checks (O(N), not the O(N^2) shape that per-element delegation would produce).

All tolerance- and sigma-taking methods validate the bound up front so an invalid input throws even when the early-return path would otherwise skip the inner `MathTolerance` call. Same family-wide validation-order pattern that `MathAssertions.Sequences.ConvergesTo` and `IsBounded` enforce.

### Added (`MathAssertions`, framework-agnostic core) — 0.1.0 Cluster 3: sequence-property checks

New static class `MathAssertions.Sequences` for `ReadOnlySpan<double>` (and the two length predicates for any element type):

- `Sequences.IsMonotonicallyIncreasing` and `IsMonotonicallyDecreasing`: non-strict monotonicity (adjacent equal values allowed). Empty and single-element spans are vacuously monotonic.
- `Sequences.IsStrictlyMonotonicallyIncreasing` and `IsStrictlyMonotonicallyDecreasing`: strict monotonicity (adjacent equal values fail).
- `Sequences.IsSorted`: convenience alias for `IsMonotonicallyIncreasing`.
- `Sequences.IsBounded(values, min, max)`: returns `true` when every value is in `[min, max]`. NaN values fail the bound check; an empty span is vacuously bounded; `max < min` throws `ArgumentException`.
- `Sequences.IsArithmeticProgression(values, tolerance)`: adjacent differences are equal within tolerance (common difference). Empty and single-element spans pass vacuously.
- `Sequences.IsGeometricProgression(values, tolerance)`: adjacent ratios are equal within tolerance (common ratio). Returns `false` when any divisor is zero (ratio undefined). The zero check uses a bit-magnitude mask so both `+0` and `-0` are caught without tripping the operator-`==` floating-point analyzer flag.
- `Sequences.ConvergesTo(values, limit, tolerance)`: the last value of the sequence is within tolerance of the limit. The empty-span case returns `false` (no value has been observed).
- `Sequences.IsCauchyConvergent(values, tolerance)`: a single-step approximation of the Cauchy criterion — the last two values are within tolerance of each other. Documented as the practical convergence proxy for unit tests of convergent algorithms, not a substitute for analytic proof.
- `Sequences.HasLength<T>(values, expected)` and `HasMinLength<T>(values, expected)`: length predicates over any `ReadOnlySpan<T>`.

### Added (`MathAssertions`, framework-agnostic core) — 0.1.0 Cluster 2: System.Numerics compounds

- `MathTolerance.IsApproximatelyEqual(Vector2, Vector2, double)`, `(Vector4, Vector4, double)`: component-wise tolerance comparison for the remaining `System.Numerics` vector types. Components widen to `double` before comparing against the caller's `double` tolerance, the same precision-preserving rule the existing `Vector3` overload follows.
- `MathTolerance.IsApproximatelyEqual(Quaternion, Quaternion, double)`: component-wise across X, Y, Z, W. Distinguishes the quaternion `q` from `-q`; use `IsRotationallyEquivalent` for the SO(3) double-cover view.
- `MathTolerance.IsRotationallyEquivalent(Quaternion, Quaternion, double)`: returns `true` when two quaternions encode the same rotation within tolerance, treating `q` and `-q` as equivalent. Implementation normalizes both inputs first so non-unit input still produces the correct verdict (Hanson, *Visualizing Quaternions*, §4.6).
- `MathTolerance.IsApproximatelyEqual(Matrix4x4, Matrix4x4, double)`: element-wise across all sixteen elements via the row/column indexer. Elements widen to `double`.
- `MathTolerance.IsApproximatelyEqual(Plane, Plane, double)`: component-wise across `Normal` and `D`. Distinguishes `(n, d)` from its sign-flipped representation `(-n, -d)`; use `IsGeometricallyEquivalent` for the geometric-plane view.
- `MathTolerance.IsGeometricallyEquivalent(Plane, Plane, double)`: returns `true` when two planes describe the same set of points in 3-space, treating `(n, d)` and `(-n, -d)` as equivalent.
- `MathTolerance.IsApproximatelyEqual(Complex, Complex, double)`: component-wise across real and imaginary parts.
- `MathTolerance.IsApproximatelyEqual(ReadOnlySpan<double>, ReadOnlySpan<double>, double)` and `(ReadOnlySpan<float>, ReadOnlySpan<float>, float)`: element-wise comparison for spans. A length mismatch returns `false` rather than throwing; explicit length validation belongs at higher layers.
- `MathTolerance.IsApproximatelyEqual<T>(ReadOnlyTensorSpan<T>, ReadOnlyTensorSpan<T>, T) where T : INumber<T>`: element-wise tolerance comparison for `System.Numerics.Tensors.ReadOnlyTensorSpan<T>`. Shape mismatch (different rank or different per-dimension length) returns `false`. NaN handling matches the floating-point scalar overloads. Iteration uses the tensor span's enumerator so strided shapes from slicing work correctly.

### Changed (`MathAssertions`, framework-agnostic core)

- The package now depends on `System.Numerics.Tensors` (BCL, MIT, no transitive deps) for the new `ReadOnlyTensorSpan<T>` overload. The package remains AOT-compatible, trimmable, and reflection-free in the assertion path.

### Added (`MathAssertions`, framework-agnostic core) — 0.1.0 Cluster 1: tolerance primitives

- `MathTolerance.IsCloseInUlps(double, double, long)` and `(float, float, int)`: ULP-distance equality. Two values compare equal when they are within the requested number of representable floats (or doubles) of each other under IEEE 754. Both NaN compare equal; one NaN compares unequal; opposite-sign values are never within any finite ULP distance; positive and negative zero compare equal regardless of distance.
- `MathTolerance.IsRelativelyAndAbsolutelyClose(double, double, double, double)`: combined relative and absolute tolerance. Returns `true` when `|a - b| <= max(absoluteTolerance, relativeTolerance * max(|a|, |b|))`. The textbook combined-tolerance check from Knuth, *The Art of Computer Programming*, Vol. 2; the absolute term is the floor near zero, the relative term scales with magnitude.
- `MathTolerance.IsFinite(double)` and `(float)`: thin wrappers over the corresponding BCL predicates, exposed alongside the other helpers so fluent assertion chains stay in a single namespace.
- `MathTolerance.IsNonNegativeFinite(double)`: `true` when the value is finite and `>= 0`. Domain check for magnitudes, distances, durations, and similar non-negative quantities.
- `MathTolerance.IsProbability(double)`: `true` when the value is finite and in `[0, 1]`.
- `MathTolerance.IsPercentage(double)`: `true` when the value is finite and in `[0, 100]`.
- `MathTolerance.HasRoundtripIdentity(double, Func<double, double>, Func<double, double>, double)`: invertible-transformation roundtrip-identity check. Returns `true` when `inverse(forward(x))` equals `x` within tolerance. Double-only on purpose; consumers needing the same check on other types compose their own predicate with the type-specific `IsApproximatelyEqual` overload, typically inside a `[GenerateAssertion]` extension on their own type.

## [0.0.1] - Initial preview: skeleton release establishing repository, package identifiers, and quality bar

First public release. Two packages ship together: `MathAssertions` (framework-agnostic core, BCL only) and `MathAssertions.TUnit` (TUnit fluent adapter). .NET 10, AOT-compatible, trimmable, no runtime reflection in the assertion path.

The 0.0.1 scope is intentionally narrow. The release exists to establish the repository, claim the `MathAssertions` and `MathAssertions.TUnit` package identifiers on nuget.org, and lock the API style and quality bar before the wider catalog ships at 0.1.0. Consumers needing the full v0.1.0 surface can install 0.0.1 to lock the dependency relationship and watch the CHANGELOG.

### Added (`MathAssertions`, framework-agnostic core)

- `MathTolerance.IsApproximatelyEqual(double, double, double)`: NaN-aware, infinity-aware tolerance comparison. Both NaN equal under tolerance; same-sign infinity equal; opposite-sign infinity not equal; finite values use `Math.Abs(a - b) <= tolerance`. Mirrors TUnit's `IsCloseTo` primitive semantics.
- `MathTolerance.IsApproximatelyEqual(float, float, float)`: same semantics, single-precision signature.
- `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)`: component-wise tolerance comparison. Component values widen to `double` before the per-axis comparison so a `double` tolerance is honored at full precision; casting the tolerance down to `float` would discard up to 22 bits of mantissa for tight tolerances such as `1e-9`. The precision-preserving cast is pinned by tests.
- All helpers validate `tolerance` (rejects NaN and negative values via `ArgumentOutOfRangeException`).

### Added (`MathAssertions.TUnit`, TUnit adapter)

- `Vector3.IsApproximatelyEqualTo(expected, tolerance)`: fluent component-wise assertion entry point, generated via TUnit's `[GenerateAssertion]`. Calls `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)` internally.

### Roadmap to v0.1.0

The wider surface lands at 0.1.0 alongside the load-bearing review fixes M-1 through M-4 (precision-preserving casts, quaternion normalization, plane geometric equivalence, array-shape null validation). Planned 0.1.0 additions:

- Vector2, Vector4 component-wise overloads
- Quaternion component-wise plus rotational equivalence (handles `q` and `-q` representing the same rotation)
- Matrix4x4 element-wise comparison
- Plane component-wise plus geometric-equivalence (`(normal, d)` and `(-normal, -d)` describe the same plane)
- Complex tolerance comparison
- `ReadOnlySpan<double>` / `ReadOnlySpan<float>` element-wise comparison
- `double[]` / `float[]` adapter-side overloads with `ArgumentNullException` on null inputs
- Vector3 `HasMagnitudeApproximately`, `IsNormalized` plus Quaternion `IsIdentity`, `IsNormalized`
- Statistics cluster: mean, median, variance, standard deviation, sum, sigma bounds, percentile (linear interpolation)
- Linear algebra invariants: symmetric, orthogonal, identity, determinant, trace, invertible, parallel / orthogonal vector pairs
- Number theory cluster: divisibility, primality, GCD, LCM, coprimality, congruence, perfect-square, power-of-base
- Geometry3D primitives (Sphere, AxisAlignedBox, OrientedBox, Ray3D, LineSegment3D, Triangle3D, Capsule, Cylinder) plus containment, intersection (Moller-Trumbore, slab test), point-distance closed forms, coplanarity / collinearity, basic pointcloud assertions

### Quality bar (locked at 0.0.1)

- AOT-compatible (`IsAotCompatible=true`), trimmable (`IsTrimmable=true`), no runtime reflection in the assertion path.
- C# 14, `Nullable=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`.
- Five Roslyn analyzer packs at full strength (Meziantou, SonarAnalyzer, Roslynator, Microsoft.VisualStudio.Threading, DotNetProjectFile.Analyzers).
- `Microsoft.CodeAnalysis.BannedApiAnalyzers` enforces no-reflection at build time.
- ApiCompat strict mode wired; `PackageValidationBaselineVersion` will pin to 0.0.1 starting from 0.0.2.
- 90% line / 90% branch coverage CI gates.
- Public API surface pinned via snapshot tests using `SnapshotAssertions.TUnit` plus `PublicApiGenerator`; cross-package dogfooding against the family.
- External-consumer smoke test (deliberately different namespace, deliberately different package-resolution path) plus AOT-publish gate on `linux-x64`.
- Trusted Publishing (OIDC) to nuget.org; no long-lived secrets.
- SLSA v1.0 build provenance plus CycloneDX 1.6 SBOM plus SPDX 3.0 SBOM plus OpenVEX v0.2.0 plus Sigstore-signed attestations on every release.
- Source Link, deterministic builds, embedded PDB.
- TUnit dependency pinned to **1.43.11**.
