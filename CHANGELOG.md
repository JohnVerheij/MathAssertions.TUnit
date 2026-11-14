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

### Added

- **`MathTolerance.AngleDifferenceDegrees(double, double)` and `MathTolerance.AngleDifferenceDegrees(float, float)`:** the shorter arc between two angles in degrees, in `[0, 180]`: `359` and `1` are `2` apart and `180` and `-180` are `0` apart. Each angle is reduced modulo `360` first (exact, so huge angles keep their precision and nothing overflows); a NaN or infinite angle gives NaN. The `float` overload computes in `double` and rounds once.
- **`MathTolerance.IsAngleApproximatelyEqual(double, double, double)` and `MathTolerance.IsAngleApproximatelyEqual(float, float, float)`:** `true` when the wrapped difference is at most the tolerance in degrees. A NaN or infinite angle never matches, not even NaN against NaN.
- **`ScalarAssertions.IsAngleApproximatelyEqualTo` for `double` and for `float`:** the fluent form, `await Assert.That(yawDegrees).IsAngleApproximatelyEqualTo(-180f, toleranceDegrees: 0.05f)`. The failure message shows the actual angle, the expected angle and the wrapped difference. The generator adds the public assertion classes `Double_IsAngleApproximatelyEqualTo_Double_Double_Assertion` and `Float_IsAngleApproximatelyEqualTo_Float_Float_Assertion` to `MathAssertions.TUnit`.
- **.NET 11:** both packages now target `net10.0` and `net11.0` (`net10.0` only before). `System.Numerics.Tensors` is 10.0.15 for `net10.0` and 11.0.0 for `net11.0`. The family policy is now to target every .NET version in Microsoft support (`CONVENTIONS.md`, "TFM policy").

### Changed

- **`MathTolerance.IsPoseApproximatelyEqual` and `IsPoseApproximatelyEqualTo`:** a position whose distance is NaN (a NaN coordinate, or infinite coordinates that cancel) now fails; it used to pass because the distance was NaN and the comparison `NaN > tolerance` is `false`.
- **`Containment.ConvexHullContains` and `ConvexHullContainsPoint`:** a NaN point or a NaN hull vertex now fails (it passed), and a hull in which every face triangle is degenerate (zero area) now contains no point (it contained every point).
- **`Pointcloud.IsApproximatelyOnPlane`, `IsApproximatelyOnSphere` and the adapter assertions of the same names:** a cloud with a NaN point now fails (it passed).
- **`Sequences.IsMonotonicallyIncreasing`, `IsMonotonicallyDecreasing`, `IsStrictlyMonotonicallyIncreasing`, `IsStrictlyMonotonicallyDecreasing`, `IsSorted` and the adapter assertions of the same names:** a span containing NaN now returns `false`, a one-element span `[NaN]` included (NaN passed whenever its neighbors were ordered, and a single NaN was vacuously monotonic).
- **`Properties.IsCollinear` and `Properties.AreCoplanar` (and the adapter assertions):** a point with a NaN or infinite coordinate now returns `false` (an all-NaN set was collinear and coplanar, and one NaN point was ignored).
- **`Intersection.Intersects(Ray3D, AxisAlignedBox)`:** a ray whose origin has a NaN coordinate on an axis the ray runs parallel to now misses the box (it hit).
- **`LinearAlgebra.IsRotation` and the adapter assertion `IsRotation`:** now also requires the last column `0, 0, 0, 1` within the tolerance (`M14`, `M24`, `M34` near `0`, `M44` near `1`). A rotation that mixes W with X, Y or Z (for example `M14 = sin 0.5`) used to pass and now fails; `diag(1, 1, -1, -1)`, orthogonal with determinant `+1` and `M44 = -1`, now fails too. `IsOrthogonal` stays a pure 4x4 check and still accepts both. Rotations, `CreateScale(-1, -1, 1)` and the documented tolerance rows are unchanged.
- **`MathTolerance.IsRotationallyEquivalent`, `HasAxisAngleApproximately` and the adapter assertions:** now compute in `double` precision. A non-unit quaternion compared with itself or a scaled copy passes a tight tolerance (it could fail by the `float` rounding of its normalization), and the expected axis-angle rotation is built in `double`. A quaternion that cannot be normalized (the zero quaternion) is compared componentwise at a fixed tolerance of `1e-6`, the rule `IsPoseApproximatelyEqual` already had: the zero quaternion now matches itself and no real rotation (it matched nothing, itself included).
- **`Statistics` (all nine methods) and the statistics assertions:** the XML docs, both READMEs and a new README table now state what each method does with a NaN in the sample and the unit of each tolerance (the unit of the values; the squared unit for the variance; a multiple of the standard deviation for `sigmas`; percentiles on the scale 0 to 100). The behavior is unchanged.

### Fixed

- **`Properties.AreCoplanar`:** now picks the plane from a triple whose third point lies farther than the tolerance (a length) from the line through the first two, as `IsCollinear` does. It compared the length of a cross product (an area) with the tolerance, so a small point set passed whatever its shape: `(0, 0, 0)`, `(0.01, 0, 0)`, `(0, 0.01, 0)` and `(0, 0, 0.01)` were coplanar under a tolerance of `0.001`. Coincident leading points are skipped.
- **`MathTolerance.IsApproximatelyEqual<T>` (`ReadOnlyTensorSpan<T>`):** no longer overflows in the difference and now applies the scalar infinity rules under every tolerance. An unsigned `0` against `1` wrapped to a huge difference and compared unequal under a tolerance of `1`; `int.MinValue` against `0` threw `OverflowException`; `int.MaxValue` against `int.MinValue` wrapped to `-1` and compared equal under any tolerance of `1` or more. Under a tolerance of positive infinity, opposite-sign infinities and an infinity against a finite value returned `true`; they now return `false`, like the scalar overloads. A difference beyond the range of an integer type counts as larger than any tolerance.
- **`MathTolerance.HasAxisAngleApproximately`:** the expected axis is normalized in `double`. An axis such as `(1e30, 0, 0)` or `(1e-30, 0, 0)` keeps its direction (the first normalized to `(0, 0, 0)` and silently expected the identity, the second threw), so the `ArgumentException` now happens only for an axis of exactly zero length or with a NaN or infinite component.
- **`LinearAlgebra.AreParallel` and the adapter assertion `IsParallelTo`:** the lengths and the cross product are now computed in `double`, so the result no longer depends on the `float` range. Before, `(1e15, 0, 0)` against `(1e15, 1e12, 0)` (a sine of `1e-3`) failed under a tolerance of `1e-2`, and `(1e20, 0, 0)` against the orthogonal `(0, 1e-3, 0)` passed as parallel under `1e-4`, because the squares overflowed. The limit of about `1.8e19` is gone.
- **`NumberTheory.IsDivisibleBy` and the adapter assertion:** now returns `true` for a divisor of `1` or `-1`, including `long.MinValue` with `-1` (it threw `OverflowException`).
- **`Statistics.HasMedianApproximately`:** the even-length midpoint no longer loses a denormal: the median of `[double.Epsilon, double.Epsilon]` is `double.Epsilon` (it was `0`, the result of halving each operand). Large values still do not overflow.

## [0.5.1] - 2026-11-09: cookbook for the geometric checks, SPDX 3.0 SBOM

Patch release. No API or behavior change.

### Changed

- **README:** copy-paste examples for `HasAngleBetweenApproximately`, the scale-invariant `IsParallelTo`, `IsNormalized` (vector and quaternion), and `IsRotation`; a note that only `PoseRenderer` needs `using MathAssertions.Render;`; and a corrected sentence that still called per-axis delta rendering future work, although it shipped in 0.2.0.
- **SBOM:** both packages embed an SPDX 3.0 software bill of materials (SPDX 2.2 before), like the other packages of the family.
- **`TUnit.Assertions`** and **`TUnit.Core`** minimum versions raised from 1.54.0 to 1.81.10, and **`System.Numerics.Tensors`** from 10.0.9 to 10.0.14.
- **Package metadata:** the project URL of both packages now points to `https://mathassertions.dev`, and the NuGet tags now include `testing` and `dotnet`.
- **README:** now says that adapters for other test frameworks (NUnit, xUnit, MSTest) are not planned; the family targets TUnit.
- **Core README:** now says that adapters for other test frameworks (NUnit, xUnit, MSTest) are not planned; the family targets TUnit.
- **README (namespaces):** now lists the real namespaces (`MathAssertions.TUnit`, `TUnit.Assertions.Extensions`, `MathAssertions`, `MathAssertions.Geometry3D`, `MathAssertions.Render`).
- **README (`using` directives):** now says that the geometry samples need `using MathAssertions.Geometry3D;`, and no longer says that `System.Numerics` is the only extra `using`. The package README says the same.
- **README (adapter classes):** now counts thirteen adapter classes instead of twelve, in the README and the package README, and no longer calls the geometry surface "complete".
- **README (`IsCloseTo` samples):** the `Vector3.Distance` and per-axis samples now cast the `float` values to `double` so they compile.
- **README (protobuf sample):** the projection now casts its fields to `float`.
- **README (pose projection):** now warns that the cast from `double` to `float` can turn distinct positions into the same `Vector3`, so a pose assertion can pass despite a position error, and says to compare the original `double` fields when that precision matters.
- **README (failure diagnostics):** now describes what the failure messages contain, per compound type, instead of showing an exact layout. The package README says the same.
- **README (failure diagnostics, TUnit wrapper):** now says that TUnit prints `Expected ...` and `but ...` around the message, with "to satisfy" plus the method as the expectation.
- **README (NaN scope):** the NaN and infinity claims are now scoped to the tolerance comparisons, and the README says that pose position, pointcloud on-plane and on-sphere, convex-hull containment, and monotonic-sequence checks do not reject NaN. The package and core READMEs say the same.
- **README (NaN table):** now documents `NaN` vs `NaN` as `true` under any valid tolerance, in the README and the package README.
- **README (infinity table):** now says that the generic `ReadOnlyTensorSpan<T>` overload does not follow the infinity rows: under a tolerance of positive infinity it returns `true` for opposite-sign infinities and for an infinity against a finite value. The core README says the same.
- **README (tolerance precision):** now says that the `double` tolerance is kept and the component difference is computed in `double`, instead of claiming that a cast to `float` would round `1e-9` to zero.
- **README (scalar assertions):** now says that `ScalarAssertions` has fluent `IsApproximatelyEqualTo` for `double` and `float`.
- **Package README (`ScalarAssertions`):** now says that only `IsApproximatelyEqualTo` and `IsCloseInUlpsTo` have `float` overloads.
- **README (`IsParallelTo`):** now says that the scale-invariant check applies to vectors longer than the tolerance, that a shorter vector counts as parallel to anything, and that the check holds only while both lengths and their product stay below about `1.8e19`, because the lengths and the cross product are computed in `float`.
- **README (`IsRotation`):** the cookbook comment now says that two checks must both pass, `M * M^T = I` and determinant `+1`, each within the tolerance: a reflection fails under any tolerance below `2`; in an affine matrix (last column `0, 0, 0, 1`) a translation or a scale factor other than `+1` or `-1` fails unless both checks stay within the tolerance; `CreateScale(-1, -1, 1)` passes as a 180-degree rotation; and the whole 4x4 matrix is checked, not the affine form, so a 4D rotation that mixes W with X, Y or Z passes.
- **README (number theory):** now says that `IsDivisibleBy(long.MinValue, -1)` throws `OverflowException`, next to the `long.MinValue`-aware GCD and LCM claim.
- **Core README (static classes):** now lists `PoseRenderer` (`MathAssertions.Render`) next to the five static classes, and no longer says "six static classes" followed by five names.
- **Core README (stability):** now says that breaking changes before 1.0 bump the minor version, like the README.
- **Core README (failure messages):** now describes the failure-message renderer as internal and unstable.
- **`MathTolerance.IsApproximatelyEqual`:** the `Vector3` XML docs now say that the `double` tolerance is kept and the difference is computed in `double`.
- **`MathTolerance.IsApproximatelyEqual<T>` (`ReadOnlyTensorSpan<T>`):** the XML docs now say that infinities are compared through `T.Abs(a - b) > tolerance` instead of the scalar infinity rules, which differs from the scalar overloads under a tolerance of positive infinity.
- **`MathTolerance.IsCloseInUlps`:** the XML docs now say that the distance counts steps between adjacent representable values (adjacent values are distance 1) and that values of opposite sign, even `double.Epsilon` and its negation, return `false`.
- **`MathTolerance.RotationAngleDegrees`:** the XML docs now say that a quaternion compared with itself yields zero or a value within about `1e-14` degrees, instead of exactly zero.
- **`MathTolerance.ExtractAxisAngle`:** the XML docs now say that the extracted angle lies in `[-180, 180]` degrees and that its sign is aligned by the axis dot product. The internal failure-message renderer docs say the same.
- **`Sequences.IsArithmeticProgression`:** the XML docs, and those of the adapter method, now say that every difference is compared with the first one, not with the adjacent one.
- **`Sequences.IsGeometricProgression`:** the XML docs, and those of the adapter method, now say that every ratio is compared with the first one, not with the adjacent one.
- **`LinearAlgebra.AreParallel`:** the XML docs now say that the check is scale-invariant only for vectors longer than the tolerance, and only while both lengths and their product stay below about `1.8e19` (`float` range).
- **`LinearAlgebra.IsRotation` and `LinearAlgebra.IsOrthogonal`:** the XML docs, and those of the adapter method `IsRotation`, now say that both checks of `IsRotation` must pass within the tolerance, that a reflection fails only under a tolerance below `2`, that in an affine matrix a translation or a scale fails only once one of the checks exceeds the tolerance, that a scale of `-1` keeps a matrix orthogonal (so `CreateScale(-1, -1, 1)` is a rotation), and that the affine form is not checked, so a 4D rotation that mixes W with X, Y or Z passes.
- **`NumberTheory.IsDivisibleBy`:** the XML docs now say that `long.MinValue` with a divisor of `-1` throws `OverflowException`.
- **`Intersection` (class):** the XML docs now say that only ray-plane and ray-triangle have an `out float t` overload, that ray-sphere and ray-AABB are boolean only, and that `t` is the ray parameter of the hit point `origin + t * direction`, a distance only for a unit direction.
- **`Intersection.Intersects(Ray3D, Plane, out float)`:** the XML docs now say that `t` is `0` for a parallel ray and the computed negative `t` when the plane is behind the ray.
- **`Intersection.Intersects(Ray3D, Triangle3D, out float)`:** the XML docs now say that the hit requires the ray parameter `t > 1e-7` (a threshold on the parameter, not on a distance), so an origin on the triangle is not a hit, and what `t` is in each `false` case.
- **`Intersection.Intersects(Ray3D, Sphere)`:** the XML docs now say that the ray direction must be a unit vector.
- **`Ray3D`:** the XML docs now say that ray-sphere needs a unit direction and that the parallel tests use an absolute epsilon of `1e-7` (ray-plane on `|direction . normal|`, ray-triangle on `|direction . (edge2 x edge1)|`, ray-AABB per direction component, where a near-zero component only requires the origin to lie inside that axis's slab).
- **`PoseAssertions`:** the XML docs now say that the `Matrix4x4` overload assumes unit scale instead of saying that scale is ignored.
- **`Containment.ConvexHullContains`:** the XML docs no longer promise automatic triangulation in 0.2.0.
- **`Statistics.HasPercentileApproximately`:** the XML docs now describe the rank `p/100 * (N - 1)` (NumPy "linear", Excel `PERCENTILE.INC`) instead of citing NIST/SEMATECH.

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

[unreleased]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.5.1...HEAD
[0.5.1]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.3...v0.5.0
[0.4.3]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.2...v0.4.3
[0.4.2]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.1...v0.4.2
[0.4.1]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/JohnVerheij/MathAssertions.TUnit/compare/v0.0.1...v0.1.0
[0.0.1]: https://github.com/JohnVerheij/MathAssertions.TUnit/releases/tag/v0.0.1
