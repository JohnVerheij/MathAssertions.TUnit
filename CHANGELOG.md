# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> *History note:* All version sections were reformatted on 2026-07-05 for one-time
> [CONVENTIONS &sect;CHANGELOG](CONVENTIONS.md) conformance: forbidden sub-headers folded into
> the six Keep a Changelog headers, non-user-facing content removed (internal refactors, test
> counts, coverage numbers, CI and build hygiene, governance churn, roadmap notes), bullets
> kept in past-tense active voice with code-formatted API leads. The nuget.org Release Notes
> tab and the GitHub Release for each shipped version are unchanged. A CI `family-lint` gate
> keeps future sections conforming; each is frozen per Rule 7 once shipped.

## [Unreleased]

## [0.5.0] - 2026-06-14: scale-invariant parallelism, angle and rotation assertions

Minor release. Fixes the scale-dependence of the parallel check and adds an angle-between assertion and a proper-rotation matrix assertion.

### Added

- **`Assert.That(vector).HasAngleBetweenApproximately(other, expectedRadians, tolerance)`** asserts the unsigned angle between two vectors (on `[0, pi]`) is within tolerance of an expected value. The angle is computed as `atan2(|u x v|, u . v)`, which stays accurate across the whole range where `acos` of the normalized dot product loses precision near `0` and `pi`. `LinearAlgebra.AngleBetween(u, v)` exposes the same computation on the framework-agnostic core.
- **`Assert.That(matrix).IsRotation(tolerance)`** asserts a `Matrix4x4` is a proper rotation: orthogonal (`M * M^T = I`) with determinant `+1`. Reflections (determinant `-1`) and matrices carrying translation or scale are rejected. Exposed on the core as `LinearAlgebra.IsRotation`.

### Fixed

- **`AreParallel` / `IsParallelTo` are now scale-invariant.** The check compared the raw cross-product magnitude `|u x v|` against an absolute tolerance, so whether two vectors read as parallel depended on their length, not just their direction: the same angular deviation passed at unit scale and failed at large scale. It now compares the sine of the angle, `|u x v| / (|u| |v|)`, against the tolerance (an angular measure), the same normalization the package already applied in `IsCollinear`. A zero or shorter-than-tolerance vector is still treated as parallel to any vector. Tolerances chosen for unit vectors are unchanged in meaning; tolerances applied to non-unit vectors now express an angle rather than a magnitude.

## [0.4.3] - 2026-06-12: double-precision magnitude for IsNormalized and HasMagnitudeApproximately

Patch release. Computes vector and quaternion magnitudes in double precision so a tight tolerance reflects the value's actual deviation from unit length rather than single-precision arithmetic noise. No public API change.

### Fixed

- **`IsNormalized` (Vector3 and Quaternion) and `HasMagnitudeApproximately` (Vector3) now compute the magnitude in double precision.** They previously called `System.Numerics.Vector3.Length()` / `Quaternion.Length()`, which compute the dot product and square root in `float`. The residual single-precision error could defeat a tight tolerance: a quaternion normalized to float precision could fail `IsNormalized` at a small tolerance, and at large coordinates the float square overflows to infinity, which makes `HasMagnitudeApproximately` unsatisfiable. The components now widen to `double` before the sum of squares and the square root, the same widening already used for `PositionDistance` and the `0.4.1` quaternion-angle fix. The comparison semantics and the meaning of `tolerance` are unchanged.

## [0.4.2] - 2026-06-05: pose-tolerance migration and projection docs

Documentation and release-tooling patch. No code, public API, or behavior change; the `0.4.1` ApiCompat baseline surface is unchanged.

### Changed

- README adds a **"Migrating from a per-component quaternion tolerance"** note under the pose section: `rotationToleranceDegrees` is a geodesic angle in degrees, not a per-component quaternion delta, so a component epsilon must not be reused as the rotation tolerance. The note gives the rough angle-to-component mapping (a component tolerance `epsilon` corresponds to about `115*epsilon` degrees) so a fresh degrees value can be chosen.
- README adds a **"Projecting a non-`System.Numerics` pose"** example: `IsPoseApproximatelyEqualTo` operates on `System.Numerics`, so a pose arriving as another shape (for example a protobuf message with separate position and orientation fields) is projected to `Vector3` / `Quaternion` first, with a short conversion snippet.
- The packed package README cross-references the migration note from the `PoseAssertions` entry.

## [0.4.1] - 2026-06-03: rotation-angle self-comparison and degenerate-orientation fixes

Bug-fix release. No public API signature change.

### Fixed

- **`IsPoseApproximatelyEqualTo`** (and the `Matrix4x4` overload `IsRigidTransformApproximatelyEqualTo`, and the underlying `MathTolerance.RotationAngleDegrees`) reported a non-zero rotation angle for a pose compared with itself, failing on identical inputs. The geodesic angle was computed as `2 * acos(|dot|)`, and `acos` has infinite slope at `dot = 1`, so the residual float error of normalizing a non-unit quaternion (any quaternion whose magnitude is not exactly 1) was amplified into a spurious angle large enough to exceed a tight rotation tolerance. The metric is now the numerically-stable `theta = 2 * atan2(||imag(q1 * conj(q2))||, |real(q1 * conj(q2))|)`, which is well-conditioned across the whole 0-180 degree range, and both quaternions are normalized in double precision before measuring (a quaternion and any positive scalar multiple are the same rotation), so a quaternion compared with itself now measures exactly zero.
- **`IsPoseApproximatelyEqual`** now handles a degenerate, non-normalizable orientation (squared magnitude underflows to zero, e.g. `default(Quaternion)` = `(0, 0, 0, 0)`), which has no defined geodesic angle. The orientation half falls back to a componentwise comparison against a tight fixed tolerance, independent of the caller's rotation tolerance: the rotation tolerance is a degrees value, and reusing it would be far too permissive (a tolerance of 1 or more would accept a zero quaternion as equal to a real rotation, whose nearest component delta is at most 1). So a default-pose round-trip (`(0,0,0,0)` vs `(0,0,0,0)`) passes, while `(0,0,0,0)` compared with a real rotation fails regardless of the angular tolerance, with the combined diagnostic naming the rotation half.

## [0.4.0] - 2026-06-02: pose and rigid-transform approximate-equality assertions

Feature release. Adds the pose assertion the package's `PoseRenderer` was waiting for: a pose is a position and an orientation together, so `IsPoseApproximatelyEqualTo` compares both halves in one call with separate position (length) and rotation (degrees) tolerances and one combined diagnostic that names which half missed.

### Added

- **`Assert.That((position, orientation)).IsPoseApproximatelyEqualTo((expectedPosition, expectedOrientation), positionTolerance, rotationToleranceDegrees)`** (TUnit adapter) compares a `(Vector3, Quaternion)` pose in one call. The two halves use separate tolerances because they carry different units: a Euclidean position distance and a geodesic rotation angle in degrees. On failure the combined message renders both poses and the measured position and rotation deltas, flagging which half exceeded its tolerance. The orientation comparison uses the SO(3) metric, so a quaternion and its negation are the same rotation. Source-generated via `[GenerateAssertion]`.
- **`Assert.That(matrix).IsRigidTransformApproximatelyEqualTo(expectedMatrix, positionTolerance, rotationToleranceDegrees)`** (TUnit adapter) is the `Matrix4x4` overload (a pose is a rigid transform). Translation is read from `Matrix4x4.Translation` and rotation via `Quaternion.CreateFromRotationMatrix`; the overload assumes a rigid transform (orthonormal rotation, unit scale).
- **`MathTolerance.IsPoseApproximatelyEqual(...)`, `RotationAngleDegrees(Quaternion, Quaternion)`, and `PositionDistance(Vector3, Vector3)`** (framework-agnostic core) back the assertions: the geodesic rotation angle in degrees, the double-precision Euclidean position distance, and the combined pose predicate.

### Changed

- **TUnit dependency bumped `1.44.0` -> `1.44.39`** (and the external-consumer smoke-test pin). 1.44.39 carries the `[GenerateAssertion]` source-generator fix for value-type optional parameters; no behavioral change for this package, taken for family lockstep.

## [0.3.0] - 2026-05-14: PoseRenderer + family dependency lockstep

Feature release. Lockstep version bump for both packages. Adds the first concrete renderer under the family-shared `*.Render` namespace convention and brings the dependency pins back into lockstep with the rest of the assertion family. No breaking changes; the public surface grows by one additive namespace.

### Added

- **`MathAssertions.Render.PoseRenderer`**, a pure static renderer that turns a 3D pose into deterministic, snapshot-friendly text. `Render(Vector3, Quaternion, double)` emits a two-line block: a `pos: (x, y, z) tol=t` line and a `quat: (x, y, z, w) tol=t` line; the `Render(Vector3, double)` and `Render(Quaternion, double)` overloads emit the position or orientation line alone.
  - **Self-contained and snapshot-framework-agnostic.** The renderer takes no dependency on `SnapshotAssertions.TUnit`; the canonical pairing is the two-line `Assert.That(PoseRenderer.Render(...)).MatchesSnapshot()` composition.
  - **Orientation rendered verbatim.** The quaternion sign is not canonicalized, so a regression that flips a quaternion sign surfaces as a snapshot diff. Callers asserting rotational equivalence rather than component identity use `IsRotationallyEquivalentTo` instead.
  - **The tolerance is recorded, not applied.** The renderer performs no comparison; it echoes the supplied tolerance into the output so a tolerance silently loosened during a refactor surfaces as a snapshot diff.
  - **Deterministic formatting.** Components use the invariant-culture `F6` fixed format, the tolerance the invariant-culture `G` general format. Lines are terminated with the literal LF byte, never `Environment.NewLine`, so a baseline committed on one OS stays byte-stable for test runs on every other.

### Changed

- Bumped `Microsoft.SourceLink.GitHub` `10.0.203` -> `10.0.300`.
- **New cookbook section "Pin a pose as a snapshot"** in `README.md`. Worked example pairing `PoseRenderer.Render` with `MatchesSnapshot()` from `SnapshotAssertions.TUnit`.

## [0.2.0] - 2026-05-12: per-component delta diagnostics, axis-angle assertions, family lockstep

Feature release. Lockstep version bump for both packages.

### Added

- **Rich per-component failure-message rendering** for the compound `IsApproximatelyEqual` overloads (`Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Matrix4x4`, `Plane`, `Complex`, `ReadOnlySpan<double>`, `ReadOnlySpan<float>`) plus `IsRotationallyEquivalent` and `IsGeometricallyEquivalent`. Each renderer surfaces three sections under an expectation header: `actual:` echoing the value, `delta:` showing the absolute per-axis / per-cell / first-failing-element difference, and `exceeded:` naming the components whose delta crossed tolerance. The exceeded classification routes through the boolean predicate itself (`MathTolerance.IsApproximatelyEqual`), so equal-special-value pairs (NaN vs NaN, same-sign infinity vs same-sign infinity) are correctly treated as equal even though their absolute delta is NaN. Plane geometric equivalence additionally renders the delta against both the direct and the sign-flipped candidate representation. Numeric formatting is `CultureInfo.InvariantCulture` `G`-form throughout so the rendered text is stable across locales. The renderer type itself is internal (consumed via `[InternalsVisibleTo]` by the adapter assembly); failure-message text remains explicitly not part of the stable public surface.
- **`MathTolerance.HasAxisAngleApproximately(Quaternion, Vector3, double, double)`**. Asserts a quaternion, viewed as a rotation in axis-angle form, lies within tolerance of the rotation `expectedAngleDegrees` degrees around `expectedAxis`. Normalizes both inputs internally; rejects zero-length expected axis with `ArgumentException`. Uses the rotational-equivalence formulation under the hood (`|dot(unit_q, unit_expected)| >= 1 - tolerance`) so every edge case is handled uniformly: the SO(3) `q` vs `-q` double cover; the 180-degree boundary where `(axis, +180)`, `(axis, -180)`, and `(-axis, +/-180)` all encode the same rotation; non-unit inputs. Reference: Hanson, *Visualizing Quaternions*, &sect;4.6.
- **`QuaternionAssertions.HasAxisAngleApproximately`**. Fluent entry point delegating to `MathTolerance.HasAxisAngleApproximately`. Failure messages render the extracted axis, the extracted angle in degrees, and the delta angle (in the shorter-arc form modulo 360 degrees, so 359 vs 1 renders as 2 not 358).
- **Rich failure messages on every adapter `IsApproximatelyEqualTo` chain** (`Vector2` / `Vector3` / `Vector4` / `Quaternion` / `Matrix4x4` / `Plane` / `Complex` / `double[]` / `float[]`) plus `IsRotationallyEquivalentTo` and `IsGeometricallyEquivalentTo`. Generated extension methods now invoke the per-component renderer on mismatch instead of returning a static expectation-only template.

### Changed

- **Compound `IsApproximatelyEqualTo` source-method return types changed from `bool` to `TUnit.Assertions.Core.AssertionResult`.** This applies to the static helpers in `VectorAssertions`, `QuaternionAssertions`, `MatrixAssertions`, `PlaneAssertions`, `ComplexAssertions`, `ArrayAssertions`, plus `IsRotationallyEquivalentTo` and `IsGeometricallyEquivalentTo`. The change enables the rich per-component failure-message rendering; the corresponding generated TUnit chain extensions (`Assert.That(x).IsApproximatelyEqualTo(...)`) are unaffected at the chain-syntax level. Consumers calling the static helpers directly (rather than through `Assert.That`) need to read `.IsPassed` off the returned `AssertionResult`.
- **Reframing carry-forward.** Package descriptions, tags, and README taglines reframed from "tolerance-aware numeric assertions" / "math-assertion DSL" to "math assertion library" to match the actual surface (tolerance comparisons, sequences, statistics, linear algebra, number theory, 3D geometry). Affects `<Description>` and `<PackageTags>` in both csprojs (so the nuget.org listing reflects the broader scope), the tagline in the root `README.md`, and the taglines in `src/MathAssertions.TUnit/README.md` and `src/MathAssertions/README.md`. No API surface change here.
- Bumped `TUnit` / `TUnit.Assertions` / `TUnit.Core` `1.43.11` -> `1.44.0`.
- **Cookbook entries** on `IsApproximatelyEqualTo` (component-wise) vs `IsRotationallyEquivalentTo` for `Quaternion` (the SO(3) `q` vs `-q` double-cover) and vs `IsGeometricallyEquivalentTo` for `Plane` (the `(n, d)` vs `(-n, -d)` sign flip), with worked examples in both READMEs.
- **Cookbook entry** on asserting a quaternion is the zero-valued sentinel using `IsApproximatelyEqualTo(Quaternion.Zero, tolerance)`.
- **New `HasRoundtripIdentity` cookbook entry** in both READMEs documenting the existing framework-agnostic primitive with three worked examples (Sin / Asin, degree / radian, encode / decode delegate pair).
- **"NaN and infinity semantics" section** in `src/MathAssertions.TUnit/README.md` mirroring the table already in the root README.

## [0.1.0] - 2026-05-09: seven-cluster tolerance-aware core and full TUnit adapter parity

The wider catalog. 0.1.0 expands the framework-agnostic core from a 3-method skeleton (scalar, Vector3) into a ~85-method tolerance-aware mathematical-assertion library across seven clusters, and brings the `MathAssertions.TUnit` adapter to parity so consumers can write `await Assert.That(value).Method(...)` for the entire core surface. Both packages remain AOT-compatible, trimmable, and reflection-free in the assertion path.

### Added

**`MathTolerance` tolerance primitives**

- ULP-distance equality (`IsCloseInUlps` for `double`/`float`); positive and negative zero compare equal regardless of distance, opposite-sign operands never within finite ULPs, NaN handling matches the absolute-tolerance overloads.
- Combined relative + absolute tolerance (`IsRelativelyAndAbsolutelyClose`) per Knuth, *TAOCP Vol. 2*; absolute term is the floor near zero, relative term scales with magnitude.
- `IsFinite(double)` / `IsFinite(float)`, `IsNonNegativeFinite(double)`, `IsProbability(double)` (`[0, 1]`), `IsPercentage(double)` (`[0, 100]`) domain predicates.
- `HasRoundtripIdentity(double, Func<double,double>, Func<double,double>, double)`: invertible-transformation check; double-only on purpose, consumers compose their own predicate for other types.

**`System.Numerics` compounds**

- Component-wise `MathTolerance.IsApproximatelyEqual` for `Vector2`, `Vector4` (alongside the existing `Vector3` overload), `Quaternion`, `Matrix4x4`, `Plane`, `Complex`. Components widen to `double` before comparison so a tight `double` tolerance is honored at full precision (M-1 fix from the 0.1.0 plan).
- `IsRotationallyEquivalent(Quaternion, Quaternion, double)`: SO(3) double-cover view treating `q` and `-q` as the same rotation. Normalizes inputs internally so non-unit operands produce the correct verdict (Hanson, *Visualizing Quaternions*, &sect;4.6; M-2 fix).
- `IsGeometricallyEquivalent(Plane, Plane, double)`: plane equivalence under the `(n, d) ~ (-n, -d)` sign flip (M-3 fix).
- `IsApproximatelyEqual(ReadOnlySpan<double>, ...)` and `(ReadOnlySpan<float>, ...)` for element-wise comparison; length mismatch returns `false` rather than throwing.
- Generic `IsApproximatelyEqual<T>(ReadOnlyTensorSpan<T>, ReadOnlyTensorSpan<T>, T) where T : INumber<T>`: element-wise tolerance comparison for `System.Numerics.Tensors.ReadOnlyTensorSpan<T>`. Shape mismatch returns `false`. NaN handling matches the floating-point scalar overloads. Iteration uses the tensor span's enumerator so strided shapes from slicing work correctly.

**`Sequences` over `ReadOnlySpan<double>`**

- Monotonicity (strict and non-strict, increasing and decreasing), `IsSorted`, `IsBounded` with NaN-aware element check, arithmetic and geometric progressions with tolerance, `ConvergesTo` (last value within tolerance of a limit), `IsCauchyConvergent` (single-step proxy of the Cauchy criterion), and generic `HasLength<T>` / `HasMinLength<T>` length predicates over any `ReadOnlySpan<T>`. The geometric-progression zero check uses a bit-magnitude mask so both `+0` and `-0` are caught without tripping the operator-`==` floating-point analyzer flag.

**`Statistics` over `ReadOnlySpan<double>`**

- `MeanAndVariance`: numerically stable single-pass mean + unbiased sample variance via Welford's online algorithm (Knuth, *TAOCP Vol. 2*, &sect;4.2.2).
- `HasMeanApproximately`, `HasVarianceApproximately`, `HasStdDevApproximately`, `HasSumApproximately`, `HasMedianApproximately`.
- `HasPercentileApproximately`: linear interpolation between adjacent ranks per the NIST/SEMATECH e-Handbook of Statistical Methods &sect;1.3.5.6. Validates the percentile is in `[0, 100]` and not NaN.
- `IsWithinSigmasOfMean(value, sample, sigmas)` and `AreAllWithinSigmasOfMean(values, sigmas)`. The latter computes mean and standard deviation once and reuses them across all element checks (O(N), not the O(N^2) shape that per-element delegation would produce).
- Median uses `a/2 + b/2` and percentile uses the convex-combination lerp `a*(1-f) + b*f` so very-large finite operands do not overflow to infinity.
- `AreAllWithinSigmasOfMean` uses the `!(<= )` idiom so NaN-poisoned thresholds short-circuit to false rather than the `>` form's vacuous true.

**`LinearAlgebra` invariants**

- `Matrix4x4`: `IsSymmetric`, `IsOrthogonal` (`M*M^T ~= I`), `IsIdentity`, `HasDeterminantApproximately`, `HasTraceApproximately`, `IsInvertible`.
- `Vector3` pair: `AreOrthogonal`, `AreParallel` (zero vector treated as parallel to every other vector), `AreLinearlyIndependent` over `ReadOnlySpan<Vector3>` (triple-product test for sets of up to three vectors in `R^3`).

**`NumberTheory` exact integer predicates over `long`**

- `IsDivisibleBy`, `IsPrime` (wheel-of-six trial division with overflow-safe loop bound `i <= value / i`), `AreCoprime`, `GreatestCommonDivisor`, `LeastCommonMultiple` (validated `long.MinValue` rejection at entry; multiply runs in `checked()` so overflow throws `OverflowException` rather than wrapping), `IsPowerOf`, `IsPerfectSquare` (with a successor check skipped when `(sqrt+1)^2` would overflow), `IsCongruent` (canonical-residue form so signed-range straddling inputs do not overflow the subtraction).

**`Geometry3D` 3D primitives + property predicates + containment + distance + intersection + pointcloud**

- Eight primitive `record struct` types under `MathAssertions.Geometry3D`: `Sphere` (with `Volume` / `SurfaceArea`), `AxisAlignedBox` (with `Center`, `Size`, `HalfExtents`, `Volume`), `OrientedBox`, `Ray3D`, `LineSegment3D` (with `Direction`, `Length`), `Triangle3D` (with `Normal` (right-hand-rule unit normal; NaN for degenerate triangles, where `IsDegenerate` is the precondition guard), `Centroid`, `Area`), `Capsule`, `Cylinder` (distinct from `Capsule`; flat caps).
- `Geometry3D.Properties`: `IsDegenerate(Triangle3D)`, `IsCollinear(span)` and `AreCoplanar(span)` over `ReadOnlySpan<Vector3>`. Both `IsCollinear` and `AreCoplanar` scan for the first non-trivial direction or a non-collinear triple respectively, so leading-coincident points do not lock in a degenerate zero direction. `IsCollinear`'s perpendicular-distance test divides by the direction magnitude so the verdict is dimensionally invariant under baseline-length scaling.
- `Geometry3D.Containment`: `Contains(box, point/AABB/sphere)`, `Contains(sphere, point)`, `Contains(OBB, point)` (via inverse-orientation rotation into the box's local frame), `ConvexHullContains(hullVertices, point, tolerance)` over flat triples-of-three CCW-outward face triangles. The face-plane half-space test divides the signed dot value by the normal length so the verdict is invariant to face-area scaling.
- `Geometry3D.Distance`: `point.DistanceFrom(plane/segment/triangle)` per Ericson, *Real-Time Collision Detection*, &sect;&sect;5.1.2 and 5.1.5. Degenerate segments where both endpoints coincide are handled via a bit-magnitude zero check on `|ab|^2` and fall through to a point-to-point distance.
- `Geometry3D.Intersection`: `Intersects(Sphere, Sphere)`, `Intersects(AABB, AABB)`, ray-plane / ray-sphere / ray-triangle (Moller-Trumbore) / ray-AABB (slab test) per Akenine-Moller, Haines, Hoffman, *Real-Time Rendering* 4th ed., &sect;&sect;22.6, 22.7, 22.8. Each ray-intersection has both a boolean shorthand and a `out float t` overload exposing the hit-distance parameter.
- `Geometry3D.Pointcloud`: `cloud.IsBoundedBy(AxisAlignedBox/Sphere)`, `cloud.HasCentroidAt(expected, tolerance)`, `cloud.IsApproximatelyOnPlane(plane, maxResidual)`, `cloud.IsApproximatelyOnSphere(center, radius, maxResidual)`. Empty cloud is vacuously bounded / vacuously on-surface.

**Cross-cutting**

- All tolerance- and sigma-taking methods validate the bound up front so an invalid input throws even when an early-return short-circuit would otherwise skip the inner `MathTolerance` call. Family-wide validation-order pattern across `Sequences.ConvergesTo`, `Sequences.IsBounded`, `Statistics.HasMeanApproximately`, `LinearAlgebra.*`, `NumberTheory.*` (where applicable), and the `Geometry3D` surface.

**TUnit adapter**

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

## [0.0.1] - 2026-05-08: skeleton release establishing repository, package identifiers, and quality bar

First public release. Two packages ship together: `MathAssertions` (framework-agnostic core, BCL only) and `MathAssertions.TUnit` (TUnit fluent adapter). .NET 10, AOT-compatible, trimmable, no runtime reflection in the assertion path.

The 0.0.1 scope is intentionally narrow. The release exists to establish the repository, claim the `MathAssertions` and `MathAssertions.TUnit` package identifiers on nuget.org, and lock the API style and quality bar before the wider catalog ships at 0.1.0. Consumers needing the full v0.1.0 surface can install 0.0.1 to lock the dependency relationship and watch the CHANGELOG.

### Added

- `MathTolerance.IsApproximatelyEqual(double, double, double)`: NaN-aware, infinity-aware tolerance comparison. Both NaN equal under tolerance; same-sign infinity equal; opposite-sign infinity not equal; finite values use `Math.Abs(a - b) <= tolerance`. Mirrors TUnit's `IsCloseTo` primitive semantics.
- `MathTolerance.IsApproximatelyEqual(float, float, float)`: same semantics, single-precision signature.
- `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)`: component-wise tolerance comparison. Component values widen to `double` before the per-axis comparison so a `double` tolerance is honored at full precision; casting the tolerance down to `float` would discard up to 22 bits of mantissa for tight tolerances such as `1e-9`. The precision-preserving cast is pinned by tests.
- All helpers validate `tolerance` (rejects NaN and negative values via `ArgumentOutOfRangeException`).
- `Vector3.IsApproximatelyEqualTo(expected, tolerance)`: fluent component-wise assertion entry point, generated via TUnit's `[GenerateAssertion]`. Calls `MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)` internally.

[unreleased]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.3...v0.5.0
[0.4.3]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.2...v0.4.3
[0.4.2]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.1...v0.4.2
[0.4.1]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.0.1...v0.1.0
[0.0.1]: https://github.com/JohnVerheij/MathAssertions.TUnit/releases/tag/v0.0.1
