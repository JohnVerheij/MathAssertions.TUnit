# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-05-12: per-component delta diagnostics, axis-angle assertions, family lockstep

Feature release. Lockstep version bump for both packages. Highlights:

- Per-component / per-cell delta rendering in every compound `IsApproximatelyEqualTo` failure message.
- `HasAxisAngleApproximately` on `Quaternion` for axis-angle-form rotation assertions.
- New `HasRoundtripIdentity` cookbook entry surfacing the existing invertible-transformation primitive.
- Carries forward the `docs/reframe-positioning` work (broader scope language across the package descriptions, tags, and README taglines).
- SnapshotAssertions 0.3.0 family-wide hygiene baseline applied: `CONVENTIONS.md` v0.3, `MeziantouAnalysisMode=all-warnings` for `src/`, dependency refresh, `BannedSymbols.txt` cleanup.

### Added (MathAssertions, framework-agnostic core)

- **Rich per-component failure-message rendering** for the compound `IsApproximatelyEqual` overloads (`Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Matrix4x4`, `Plane`, `Complex`, `ReadOnlySpan<double>`, `ReadOnlySpan<float>`) plus `IsRotationallyEquivalent` and `IsGeometricallyEquivalent`. Each renderer surfaces three sections under an expectation header: `actual:` echoing the value, `delta:` showing the absolute per-axis / per-cell / first-failing-element difference, and `exceeded:` naming the components whose delta crossed tolerance. The exceeded classification routes through the boolean predicate itself (`MathTolerance.IsApproximatelyEqual`), so equal-special-value pairs (NaN vs NaN, same-sign infinity vs same-sign infinity) are correctly treated as equal even though their absolute delta is NaN. Plane geometric equivalence additionally renders the delta against both the direct and the sign-flipped candidate representation. Numeric formatting is `CultureInfo.InvariantCulture` `G`-form throughout so the rendered text is stable across locales. The renderer type itself is internal (consumed via `[InternalsVisibleTo]` by the adapter assembly); failure-message text remains explicitly not part of the stable public surface.
- **`MathTolerance.HasAxisAngleApproximately(Quaternion, Vector3, double, double)`**. Asserts a quaternion, viewed as a rotation in axis-angle form, lies within tolerance of the rotation `expectedAngleDegrees` degrees around `expectedAxis`. Normalizes both inputs internally; rejects zero-length expected axis with `ArgumentException`. Uses the rotational-equivalence formulation under the hood (`|dot(unit_q, unit_expected)| >= 1 - tolerance`) so every edge case is handled uniformly: the SO(3) `q` vs `-q` double cover; the 180-degree boundary where `(axis, +180)`, `(axis, -180)`, and `(-axis, ±180)` all encode the same rotation; non-unit inputs. Reference: Hanson, *Visualizing Quaternions*, §4.6.

### Added (MathAssertions.TUnit, TUnit adapter)

- **`QuaternionAssertions.HasAxisAngleApproximately`**. Fluent entry point delegating to `MathTolerance.HasAxisAngleApproximately`. Failure messages render the extracted axis, the extracted angle in degrees, and the delta angle (in the shorter-arc form modulo 360 degrees, so 359 vs 1 renders as 2 not 358).
- **Rich failure messages on every adapter `IsApproximatelyEqualTo` chain** (`Vector2` / `Vector3` / `Vector4` / `Quaternion` / `Matrix4x4` / `Plane` / `Complex` / `double[]` / `float[]`) plus `IsRotationallyEquivalentTo` and `IsGeometricallyEquivalentTo`. Generated extension methods now invoke the per-component renderer on mismatch instead of returning a static expectation-only template.

### Documentation

- **Reframing carry-forward.** Package descriptions, tags, and README taglines reframed from "tolerance-aware numeric assertions" / "math-assertion DSL" to "math assertion library" to match the actual surface (tolerance comparisons, sequences, statistics, linear algebra, number theory, 3D geometry). Affects `<Description>` and `<PackageTags>` in both csprojs (so the nuget.org listing reflects the broader scope), the tagline in the root `README.md`, and the taglines in `src/MathAssertions.TUnit/README.md` and `src/MathAssertions/README.md`. No API surface change here.
- **Cookbook entries** on `IsApproximatelyEqualTo` (component-wise) vs `IsRotationallyEquivalentTo` for `Quaternion` (the SO(3) `q` vs `-q` double-cover) and vs `IsGeometricallyEquivalentTo` for `Plane` (the `(n, d)` vs `(-n, -d)` sign flip), with worked examples in both READMEs.
- **Cookbook entry** on asserting a quaternion is the zero-valued sentinel using `IsApproximatelyEqualTo(Quaternion.Zero, tolerance)`.
- **New `HasRoundtripIdentity` cookbook entry** in both READMEs documenting the existing framework-agnostic primitive with three worked examples (Sin / Asin, degree / radian, encode / decode delegate pair).
- **"NaN and infinity semantics" section** in `src/MathAssertions.TUnit/README.md` mirroring the table already in the root README.
- **`CONVENTIONS.md` upgraded to v0.3** with the `SnapshotAssertions.Render` namespace reservation for sibling-package text renderers.

### Changed

- **Dependency refresh** to the family-lockstep versions:
  - `TUnit` / `TUnit.Assertions` / `TUnit.Core`: 1.43.11 -> 1.44.0
  - `Microsoft.CodeAnalysis.BannedApiAnalyzers`: 3.3.4 -> 4.14.0
  - `Meziantou.Analyzer`: 3.0.72 -> 3.0.78
  - `SnapshotAssertions.TUnit`: 0.2.0 -> 0.3.0
- **`Directory.Build.props` sets `MeziantouAnalysisMode=all-warnings` for `src/` projects** (path-conditional). Test projects retain Meziantou defaults. Production-code findings surfaced and fixed at source rather than via NoWarn: redundant `(double)floatField` widening casts removed in favor of explicit `double` locals (preserves full-precision subtraction); discrete-equality `== 0` updated to pattern-matching `is 0`; `(int)Math.Floor(x)` / `(long)Math.Sqrt(value)` updated to `int.CreateChecked` / `long.CreateTruncating` (semantic-preserving for the non-negative ranges those call sites guarantee); XML-doc `<c>null</c>` updated to `<see langword="null"/>`. `<NoWarn>` extended with `MA0038;MA0137;MA0174;MA0190` per family convention.
- **`BannedSymbols.txt`** collapsed bare `#` comment lines into adjacent text-bearing lines so the file parses cleanly under the stricter BannedApiAnalyzers 4.x grammar.
- **`MathAssertions` adds `[InternalsVisibleTo("MathAssertions.TUnit")]`** so the adapter can call the internal `MathTolerance.ExtractAxisAngle` helper to render the axis-angle failure message consistently with the predicate's own extraction. The helper stays internal (not promoted to the public surface).

### Public API note

- **Compound `IsApproximatelyEqualTo` source-method return types changed from `bool` to `TUnit.Assertions.Core.AssertionResult`.** This applies to the static helpers in `VectorAssertions`, `QuaternionAssertions`, `MatrixAssertions`, `PlaneAssertions`, `ComplexAssertions`, `ArrayAssertions`, plus `IsRotationallyEquivalentTo` and `IsGeometricallyEquivalentTo`. The change enables the rich per-component failure-message rendering; the corresponding generated TUnit chain extensions (`Assert.That(x).IsApproximatelyEqualTo(...)`) are unaffected at the chain-syntax level. Consumers calling the static helpers directly (rather than through `Assert.That`) need to read `.IsPassed` off the returned `AssertionResult`. `CompatibilitySuppressions.xml` captures the signature change as accepted.

### Quality

- ApiCompat strict-mode baseline bumped `0.0.1` -> `0.1.0`. The auto-generated `CompatibilitySuppressions.xml` documents the signature changes above plus the new `HasAxisAngleApproximately` surface.
- PublicAPI snapshots regenerated to reflect the new `AssertionResult` return types and the `HasAxisAngleApproximately` addition.
- Test count: 670 (was 619 at 0.1.0); +25 new failure-message-rendering tests covering every compound renderer (Vector2 / Vector3 / Vector4 / Quaternion / Matrix4x4 / Plane / Complex / `double[]` / `float[]`) plus the equal-special-value classification, the modulo-360 shortest-arc rendering of the axis-angle delta, and the fallback-text paths; +11 new axis-angle predicate tests in `MathAssertions.Tests`; +12 new chain-fail tests in `MathAssertions.TUnit.Tests` exercising each renderer end-to-end via the fluent API; plus +3 adapter tests pinning the `HasAxisAngleApproximately` chain (happy path, wrong-axis with extracted-diagnostics rendering, SO(3) double-cover via negated quaternion). Coverage gate: 90% line / 90% branch, currently at 97.8% / 97.8%.
- AOT-publish smoke gate via `tests/MathAssertions.TUnit.SmokeTest/` continues to validate consumer-side AOT correctness; SmokeTest pin bumped to `TUnit 1.44.0` and the floating `MathAssertions.TUnit` reference to `0.2.0-*`.

## [0.1.0] - 2026-05-09

The wider catalog. 0.1.0 expands the framework-agnostic core from a 3-method skeleton (scalar, Vector3) into a ~85-method tolerance-aware mathematical-assertion library across seven clusters, and brings the `MathAssertions.TUnit` adapter to parity so consumers can write `await Assert.That(value).Method(...)` for the entire core surface. Both packages remain AOT-compatible, trimmable, and reflection-free in the assertion path.

### Added (`MathAssertions`, framework-agnostic core)

#### Cluster 1: `MathTolerance` tolerance primitives

- ULP-distance equality (`IsCloseInUlps` for `double`/`float`); positive and negative zero compare equal regardless of distance, opposite-sign operands never within finite ULPs, NaN handling matches the absolute-tolerance overloads.
- Combined relative + absolute tolerance (`IsRelativelyAndAbsolutelyClose`) per Knuth, *TAOCP Vol. 2*; absolute term is the floor near zero, relative term scales with magnitude.
- `IsFinite(double)` / `IsFinite(float)`, `IsNonNegativeFinite(double)`, `IsProbability(double)` (`[0, 1]`), `IsPercentage(double)` (`[0, 100]`) domain predicates.
- `HasRoundtripIdentity(double, Func<double,double>, Func<double,double>, double)`: invertible-transformation check; double-only on purpose, consumers compose their own predicate for other types.

#### Cluster 2: `System.Numerics` compounds

- Component-wise `MathTolerance.IsApproximatelyEqual` for `Vector2`, `Vector4` (alongside the existing `Vector3` overload), `Quaternion`, `Matrix4x4`, `Plane`, `Complex`. Components widen to `double` before comparison so a tight `double` tolerance is honored at full precision (M-1 fix from the 0.1.0 plan).
- `IsRotationallyEquivalent(Quaternion, Quaternion, double)`: SO(3) double-cover view treating `q` and `-q` as the same rotation. Normalizes inputs internally so non-unit operands produce the correct verdict (Hanson, *Visualizing Quaternions*, §4.6; M-2 fix).
- `IsGeometricallyEquivalent(Plane, Plane, double)`: plane equivalence under the `(n, d) ≡ (-n, -d)` sign flip (M-3 fix).
- `IsApproximatelyEqual(ReadOnlySpan<double>, ...)` and `(ReadOnlySpan<float>, ...)` for element-wise comparison; length mismatch returns `false` rather than throwing.
- Generic `IsApproximatelyEqual<T>(ReadOnlyTensorSpan<T>, ReadOnlyTensorSpan<T>, T) where T : INumber<T>`: element-wise tolerance comparison for `System.Numerics.Tensors.ReadOnlyTensorSpan<T>`. Shape mismatch returns `false`. NaN handling matches the floating-point scalar overloads. Iteration uses the tensor span's enumerator so strided shapes from slicing work correctly.

#### Cluster 3: `Sequences` over `ReadOnlySpan<double>`

- Monotonicity (strict and non-strict, increasing and decreasing), `IsSorted`, `IsBounded` with NaN-aware element check, arithmetic and geometric progressions with tolerance, `ConvergesTo` (last value within tolerance of a limit), `IsCauchyConvergent` (single-step proxy of the Cauchy criterion), and generic `HasLength<T>` / `HasMinLength<T>` length predicates over any `ReadOnlySpan<T>`. The geometric-progression zero check uses a bit-magnitude mask so both `+0` and `-0` are caught without tripping the operator-`==` floating-point analyzer flag.

#### Cluster 4: `Statistics` over `ReadOnlySpan<double>`

- `MeanAndVariance`: numerically stable single-pass mean + unbiased sample variance via Welford's online algorithm (Knuth, *TAOCP Vol. 2*, §4.2.2).
- `HasMeanApproximately`, `HasVarianceApproximately`, `HasStdDevApproximately`, `HasSumApproximately`, `HasMedianApproximately`.
- `HasPercentileApproximately`: linear interpolation between adjacent ranks per the NIST/SEMATECH e-Handbook of Statistical Methods §1.3.5.6. Validates the percentile is in `[0, 100]` and not NaN.
- `IsWithinSigmasOfMean(value, sample, sigmas)` and `AreAllWithinSigmasOfMean(values, sigmas)`. The latter computes mean and standard deviation once and reuses them across all element checks (O(N), not the O(N²) shape that per-element delegation would produce).
- Median uses `a/2 + b/2` and percentile uses the convex-combination lerp `a*(1-f) + b*f` so very-large finite operands do not overflow to infinity.
- `AreAllWithinSigmasOfMean` uses the `!(<= )` idiom so NaN-poisoned thresholds short-circuit to false rather than the `>` form's vacuous true.

#### Cluster 5: `LinearAlgebra` invariants

- `Matrix4x4`: `IsSymmetric`, `IsOrthogonal` (`M*M^T ≈ I`), `IsIdentity`, `HasDeterminantApproximately`, `HasTraceApproximately`, `IsInvertible`.
- `Vector3` pair: `AreOrthogonal`, `AreParallel` (zero vector treated as parallel to every other vector), `AreLinearlyIndependent` over `ReadOnlySpan<Vector3>` (triple-product test for sets of up to three vectors in `R^3`).

#### Cluster 6: `NumberTheory` exact integer predicates over `long`

- `IsDivisibleBy`, `IsPrime` (wheel-of-six trial division with overflow-safe loop bound `i <= value / i`), `AreCoprime`, `GreatestCommonDivisor`, `LeastCommonMultiple` (validated `long.MinValue` rejection at entry; multiply runs in `checked()` so overflow throws `OverflowException` rather than wrapping), `IsPowerOf`, `IsPerfectSquare` (with a successor check skipped when `(sqrt+1)²` would overflow), `IsCongruent` (canonical-residue form so signed-range straddling inputs do not overflow the subtraction).

#### Cluster 7: `Geometry3D` 3D primitives + property predicates + containment + distance + intersection + pointcloud

- Eight primitive `record struct` types under `MathAssertions.Geometry3D`: `Sphere` (with `Volume` / `SurfaceArea`), `AxisAlignedBox` (with `Center`, `Size`, `HalfExtents`, `Volume`), `OrientedBox`, `Ray3D`, `LineSegment3D` (with `Direction`, `Length`), `Triangle3D` (with `Normal` (right-hand-rule unit normal; NaN for degenerate triangles, where `IsDegenerate` is the precondition guard), `Centroid`, `Area`), `Capsule`, `Cylinder` (distinct from `Capsule`; flat caps).
- `Geometry3D.Properties`: `IsDegenerate(Triangle3D)`, `IsCollinear(span)` and `AreCoplanar(span)` over `ReadOnlySpan<Vector3>`. Both `IsCollinear` and `AreCoplanar` scan for the first non-trivial direction or a non-collinear triple respectively, so leading-coincident points do not lock in a degenerate zero direction. `IsCollinear`'s perpendicular-distance test divides by the direction magnitude so the verdict is dimensionally invariant under baseline-length scaling.
- `Geometry3D.Containment`: `Contains(box, point/AABB/sphere)`, `Contains(sphere, point)`, `Contains(OBB, point)` (via inverse-orientation rotation into the box's local frame), `ConvexHullContains(hullVertices, point, tolerance)` over flat triples-of-three CCW-outward face triangles. The face-plane half-space test divides the signed dot value by the normal length so the verdict is invariant to face-area scaling.
- `Geometry3D.Distance`: `point.DistanceFrom(plane/segment/triangle)` per Ericson, *Real-Time Collision Detection*, §§5.1.2 and 5.1.5. Degenerate segments where both endpoints coincide are handled via a bit-magnitude zero check on `|ab|^2` and fall through to a point-to-point distance.
- `Geometry3D.Intersection`: `Intersects(Sphere, Sphere)`, `Intersects(AABB, AABB)`, ray-plane / ray-sphere / ray-triangle (Möller-Trumbore) / ray-AABB (slab test) per Akenine-Möller, Haines, Hoffman, *Real-Time Rendering* 4th ed., §§22.6, 22.7, 22.8. Each ray-intersection has both a boolean shorthand and a `out float t` overload exposing the hit-distance parameter.
- `Geometry3D.Pointcloud`: `cloud.IsBoundedBy(AxisAlignedBox/Sphere)`, `cloud.HasCentroidAt(expected, tolerance)`, `cloud.IsApproximatelyOnPlane(plane, maxResidual)`, `cloud.IsApproximatelyOnSphere(center, radius, maxResidual)`. Empty cloud is vacuously bounded / vacuously on-surface.

#### Cross-cutting

- All tolerance- and sigma-taking methods validate the bound up front so an invalid input throws even when an early-return short-circuit would otherwise skip the inner `MathTolerance` call. Family-wide validation-order pattern across `Sequences.ConvergesTo`, `Sequences.IsBounded`, `Statistics.HasMeanApproximately`, `LinearAlgebra.*`, `NumberTheory.*` (where applicable), and the `Geometry3D` surface.

### Added (`MathAssertions.TUnit`, TUnit adapter)

The adapter exposes the entire 0.1.0 core surface as fluent extensions; consumers write `await Assert.That(value).Method(...)` and TUnit's `[GenerateAssertion]` source generator emits the assertion-builder plumbing. New files:

- `ScalarAssertions`: `IsApproximatelyEqualTo` / `IsCloseInUlpsTo` / `IsRelativelyAndAbsolutelyCloseTo` / `IsNonNegativeFinite` / `IsProbability` / `IsPercentage` / `HasRoundtripIdentity` for `double` and `float`. (`IsFinite` is left to TUnit's built-in `DoubleAssertionExtensions.IsFinite` / `SingleAssertionExtensions.IsFinite`.)
- `VectorAssertions`: extended to cover `Vector2` / `Vector4` (alongside the 0.0.1 `Vector3` overload) plus `Vector3.HasMagnitudeApproximately` / `IsNormalized`.
- `QuaternionAssertions`: `IsApproximatelyEqualTo`, `IsRotationallyEquivalentTo`, `IsIdentity`, `IsNormalized`.
- `MatrixAssertions` (covers `Matrix4x4`): `IsApproximatelyEqualTo` plus the full `LinearAlgebra` matrix-invariant surface.
- `PlaneAssertions`: component-wise `IsApproximatelyEqualTo` and `IsGeometricallyEquivalentTo`.
- `ComplexAssertions`: `IsApproximatelyEqualTo`.
- `ArrayAssertions`: fluent `double[]` and `float[]` element-wise comparison with the M-4 null-array guard. Operates on arrays rather than `ReadOnlySpan<T>` because TUnit's assertion-builder cannot capture ref-struct values across an `await`.
- `SequencesAssertions`, `StatisticsAssertions`: array-based wrappers for the `Sequences` and `Statistics` surfaces. Generic `T[]` length predicates on `SequencesAssertions`.
- `LinearAlgebraAssertions`: Vector3 pair `IsOrthogonalTo` / `IsParallelTo` and Vector3-array `AreLinearlyIndependent`.
- `NumberTheoryAssertions`: predicate methods on `long`.
- `Geometry3DAssertions`: triangle / point-set property predicates, containment, predicate-style `HasDistanceFrom*`, intersection, and pointcloud aggregates over the `Geometry3D` primitives.

### Changed

- `MathAssertions` core depends on `System.Numerics.Tensors` 10.0.7 (BCL, MIT, no transitive deps on net8/9/10) for the new `ReadOnlyTensorSpan<T>` overload. Both packages remain AOT-compatible, trimmable, reflection-free.
- `<Version>` bumped from `0.0.1` to `0.1.0` in both `csproj` files.
- `<PackageValidationBaselineVersion>` set to `0.0.1` and `Proj0241` removed from `<NoWarn>` in both `csproj` files. The auto-generated `CompatibilitySuppressions.xml` files capture the additive surface vs the 0.0.1 baseline (40 entries for `MathAssertions`, 115 entries for `MathAssertions.TUnit`); these are accepted-additions, not breaking changes.

### Deferred from 0.1.0

- `ReadOnlyTensorSpan<T>` fluent adapter. TUnit's assertion-builder cannot capture ref-struct values across an `await`. Consumers call `MathTolerance.IsApproximatelyEqual(ReadOnlyTensorSpan<T>, ReadOnlyTensorSpan<T>, T)` directly from `MathAssertions` for now.
- `NumberTheory.GreatestCommonDivisor` / `LeastCommonMultiple` (non-predicate `long`-returning helpers) are not exposed as fluent assertions; consumers can call them statically and apply `ScalarAssertions` predicates on the result.

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
