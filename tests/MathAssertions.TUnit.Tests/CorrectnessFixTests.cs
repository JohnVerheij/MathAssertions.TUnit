using System;
using System.Numerics;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the corrected behavior of the checks that used to pass vacuously, overflow or lose precision:
/// coplanarity measured as a length, NaN failing every comparison, quaternion equivalence and axis
/// normalization in double, overflow-free tensor comparison, degenerate hulls, the median of denormals,
/// divisibility by minus one and the affine form of a rotation matrix. Most tests failed on the code they replaced; a few pin behavior that was already right and guard it against the new code.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class CorrectnessFixTests
{
    private static readonly nint[] Shape1 = [(nint)1];
    private static readonly nint[] Shape2 = [(nint)2];

    // ----- AreCoplanar compares a length -----

    [Test]
    public async Task AreCoplanar_SmallNonCoplanarSet_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The cross product of the first two edges has length 1e-4 (an area), below the 1e-3 length tolerance,
        // so the old test never found a plane and called the set coplanar. The fourth point is 0.01 off the plane.
        Vector3[] points = [Vector3.Zero, new(0.01f, 0, 0), new(0, 0.01f, 0), new(0, 0, 0.01f)];
        await Assert.That(Properties.AreCoplanar(points, 0.001)).IsFalse();
    }

    [Test]
    public async Task AreCoplanar_SmallCoplanarSet_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] points = [Vector3.Zero, new(0.01f, 0, 0), new(0, 0.01f, 0), new(0.01f, 0.01f, 0)];
        await Assert.That(Properties.AreCoplanar(points, 0.001)).IsTrue();
    }

    [Test]
    [Arguments(0.001f)]
    [Arguments(1f)]
    [Arguments(1000f)]
    public async Task AreCoplanar_VerdictScalesWithTheGeometry(float scale, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Tolerance and points scale together: a length measure gives the same verdict at every size.
        var tolerance = 0.1 * scale;
        Vector3[] offPlane = [Vector3.Zero, new(scale, 0, 0), new(0, scale, 0), new(scale, scale, 0.5f * scale)];
        Vector3[] onPlane = [Vector3.Zero, new(scale, 0, 0), new(0, scale, 0), new(scale, scale, 0.05f * scale)];
        await Assert.That(Properties.AreCoplanar(offPlane, tolerance)).IsFalse();
        await Assert.That(Properties.AreCoplanar(onPlane, tolerance)).IsTrue();
    }

    [Test]
    public async Task AreCoplanar_CoincidentFirstPoints_StillFindsThePlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // points[1] equals points[0]: the edge has zero length and must be skipped, not divided by.
        Vector3[] points = [Vector3.Zero, Vector3.Zero, Vector3.UnitX, Vector3.UnitY, new(0, 0, 1)];
        await Assert.That(Properties.AreCoplanar(points, 0.001)).IsFalse();
    }

    [Test]
    public async Task AreCoplanar_NearDuplicateFirstPoints_DoNotPickAnUnstablePlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // points[1] lies 1e-4 from points[0], within the 1e-3 tolerance: its direction is noise and must not define the plane.
        // Every point is within 1e-4 of the z = 0 plane, so the set is coplanar.
        Vector3[] points = [Vector3.Zero, new(0, 0, 1e-4f), Vector3.UnitX, Vector3.UnitY, new(1f, 1f, 0f)];
        await Assert.That(Properties.AreCoplanar(points, 0.001)).IsTrue();
    }

    // ----- NaN fails every comparison -----

    [Test]
    public async Task IsPoseApproximatelyEqual_NaNPosition_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var nan = new Vector3(float.NaN, 0, 0);
        await Assert.That(MathTolerance.IsPoseApproximatelyEqual(nan, Quaternion.Identity, Vector3.Zero, Quaternion.Identity, 1.0, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsPoseApproximatelyEqual(Vector3.Zero, Quaternion.Identity, nan, Quaternion.Identity, 1.0, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsPoseApproximatelyEqual(nan, Quaternion.Identity, nan, Quaternion.Identity, 1.0, 1.0)).IsFalse();
    }

    [Test]
    public async Task ConvexHullContains_NaNPointOrNaNVertex_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] hull = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];
        Vector3[] nanHull = [Vector3.Zero, Vector3.UnitX, new(float.NaN, 1f, 0f)];
        await Assert.That(Containment.ConvexHullContains(hull, new Vector3(float.NaN, 0, 0), 1e-6)).IsFalse();
        await Assert.That(Containment.ConvexHullContains(nanHull, Vector3.Zero, 1e-6)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnPlane_NaNPoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] cloud = [Vector3.Zero, new(float.NaN, 0, 0)];
        await Assert.That(((ReadOnlySpan<Vector3>)cloud).IsApproximatelyOnPlane(new Plane(Vector3.UnitZ, 0f), 1.0)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnSphere_NaNPoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] cloud = [Vector3.UnitX, new(float.NaN, 0, 0)];
        await Assert.That(((ReadOnlySpan<Vector3>)cloud).IsApproximatelyOnSphere(Vector3.Zero, 1f, 1.0)).IsFalse();
    }

    [Test]
    public async Task Monotonic_NaNAnywhere_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] middle = [1.0, double.NaN, 3.0];
        double[] only = [double.NaN];
        await Assert.That(Sequences.IsMonotonicallyIncreasing(middle)).IsFalse();
        await Assert.That(Sequences.IsMonotonicallyDecreasing(middle)).IsFalse();
        await Assert.That(Sequences.IsStrictlyMonotonicallyIncreasing(middle)).IsFalse();
        await Assert.That(Sequences.IsStrictlyMonotonicallyDecreasing(middle)).IsFalse();
        await Assert.That(Sequences.IsSorted(middle)).IsFalse();
        await Assert.That(Sequences.IsMonotonicallyIncreasing(only)).IsFalse();
        await Assert.That(Sequences.IsMonotonicallyDecreasing(only)).IsFalse();
        await Assert.That(Sequences.IsStrictlyMonotonicallyIncreasing(only)).IsFalse();
        await Assert.That(Sequences.IsStrictlyMonotonicallyDecreasing(only)).IsFalse();
    }

    [Test]
    public async Task IsCollinear_NaNPoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var nan = new Vector3(float.NaN, float.NaN, float.NaN);
        Vector3[] allNaN = [nan, nan, nan];
        Vector3[] oneNaN = [Vector3.Zero, Vector3.UnitX, nan];
        Vector3[] single = [nan];
        await Assert.That(Properties.IsCollinear(allNaN, 1e-6)).IsFalse();
        await Assert.That(Properties.IsCollinear(oneNaN, 1e-6)).IsFalse();
        await Assert.That(Properties.IsCollinear(single, 1e-6)).IsFalse();
    }

    [Test]
    public async Task AreCoplanar_NaNPoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var nan = new Vector3(float.NaN, 0, 0);
        Vector3[] allNaN = [nan, nan, nan, nan];
        Vector3[] oneNaN = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY, nan];
        Vector3[] short3 = [Vector3.Zero, Vector3.UnitX, nan];
        await Assert.That(Properties.AreCoplanar(allNaN, 1e-6)).IsFalse();
        await Assert.That(Properties.AreCoplanar(oneNaN, 1e-6)).IsFalse();
        await Assert.That(Properties.AreCoplanar(short3, 1e-6)).IsFalse();
    }

    [Test]
    public async Task RayAabb_NaNOriginOnParallelAxis_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(float.NaN, 0.5f, 0.5f), new Vector3(0f, 1f, 0f));
        await Assert.That(Intersection.Intersects(ray, new AxisAlignedBox(Vector3.Zero, Vector3.One))).IsFalse();
    }

    // ----- Quaternion equivalence and axis-angle in double -----

    [Test]
    public async Task IsRotationallyEquivalent_IdenticalNonUnitQuaternion_PassesTightTolerance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = new Quaternion(0.31f, 0.52f, 0.73f, 0.94f);
        var scaled = new Quaternion(q.X * 3f, q.Y * 3f, q.Z * 3f, q.W * 3f);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(q, q, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(scaled, q, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(scaled, scaled, 1e-12)).IsTrue();
    }

    [Test]
    public async Task NaNQuaternion_NeverMatches_NotEvenItself(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var nan = new Quaternion(float.NaN, 0f, 0f, 1f);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(nan, nan, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(nan, Quaternion.Identity, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(default, nan, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsPoseApproximatelyEqual(Vector3.Zero, nan, Vector3.Zero, nan, 1.0, 1.0)).IsFalse();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(nan, Vector3.UnitX, 0.0, 1.0)).IsFalse();
    }

    [Test]
    public async Task IsRotationallyEquivalent_ZeroQuaternion_MatchesOnlyItself(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(default, default, 1e-6)).IsTrue();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(default, Quaternion.Identity, 1.0)).IsFalse();
        await Assert.That(MathTolerance.IsRotationallyEquivalent(Quaternion.Identity, default, 1.0)).IsFalse();
    }

    [Test]
    public async Task HasAxisAngleApproximately_NonUnitValueAtTightTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var axis = Vector3.Normalize(new Vector3(1f, 2f, 3f));
        var unit = Quaternion.CreateFromAxisAngle(axis, 1.1f);
        var scaled = new Quaternion(unit.X * 3f, unit.Y * 3f, unit.Z * 3f, unit.W * 3f);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(scaled, axis, 1.1 * 180.0 / Math.PI, 1e-12)).IsTrue();
    }

    [Test]
    public async Task HasAxisAngleApproximately_HugeAxis_NormalizesInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The squared length of (1e30, 0, 0) overflows float, so the float normalization returned (0, 0, 0)
        // and the expected rotation became the identity.
        var quarter = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2f);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(quarter, new Vector3(1e30f, 0f, 0f), 90.0, 1e-9)).IsTrue();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(Quaternion.Identity, new Vector3(1e30f, 0f, 0f), 90.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasAxisAngleApproximately_TinyAxis_NormalizesInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The squared length of (1e-30, 0, 0) underflows float but not double: the direction is still defined.
        var quarter = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2f);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(quarter, new Vector3(1e-30f, 0f, 0f), 90.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasAxisAngleApproximately_ZeroQuaternion_NeverMatchesARotation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The zero quaternion names no rotation: it is compared componentwise at a fixed tolerance and the
        // expected rotation is a unit quaternion, so it never matches, whatever the dot-product tolerance.
        await Assert.That(MathTolerance.HasAxisAngleApproximately(default, Vector3.UnitX, 0.0, 1.0)).IsFalse();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(default, Vector3.UnitX, 90.0, 1.0)).IsFalse();
    }

    [Test]
    public async Task HasAxisAngleApproximately_ZeroQuaternion_FailureReportsNoAxisAndNoAngle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var zero = default(Quaternion);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(zero).HasAxisAngleApproximately(Vector3.UnitX, 90.0, 1e-3);
        }).Throws<AssertionException>();

        var message = ex!.Message;
        await Assert.That(message).Contains("extracted axis:     <0, 0, 0>");
        await Assert.That(message).Contains("extracted angle:    NaN degrees");
        await Assert.That(message).Contains("delta angle:        NaN degrees");
    }

    // ----- Tensor comparison without overflow -----

    [Test]
    public async Task IsApproximatelyEqual_TensorUInt_DifferenceDoesNotWrap(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new uint[] { 0u };
        var b = new uint[] { 1u };
        var near = MathTolerance.IsApproximatelyEqual(new ReadOnlyTensorSpan<uint>(a, Shape1, default), new ReadOnlyTensorSpan<uint>(b, Shape1, default), 1u);
        var reversed = MathTolerance.IsApproximatelyEqual(new ReadOnlyTensorSpan<uint>(b, Shape1, default), new ReadOnlyTensorSpan<uint>(a, Shape1, default), 1u);
        var far = MathTolerance.IsApproximatelyEqual(new ReadOnlyTensorSpan<uint>(a, Shape1, default), new ReadOnlyTensorSpan<uint>(b, Shape1, default), 0u);
        await Assert.That(near).IsTrue();
        await Assert.That(reversed).IsTrue();
        await Assert.That(far).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorInt_ExtremesDoNotThrowOrWrap(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var min = new int[] { int.MinValue, int.MinValue };
        var zero = new int[] { 0, int.MaxValue };
        // int.MinValue against 0 threw OverflowException from T.Abs; int.MaxValue against int.MinValue wrapped to -1.
        var minAgainstZero = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(min, Shape2, default), new ReadOnlyTensorSpan<int>(zero, Shape2, default), 5);
        var maxAgainstMin = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(zero, Shape2, default), new ReadOnlyTensorSpan<int>(min, Shape2, default), int.MaxValue);
        var minAgainstItself = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(min, Shape2, default), new ReadOnlyTensorSpan<int>(min, Shape2, default), 0);
        // Single elements, so the comparison reaches int.MaxValue against int.MinValue (a difference of -1 when wrapped).
        var extremes = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(new[] { int.MaxValue }, Shape1, default),
            new ReadOnlyTensorSpan<int>(new[] { int.MinValue }, Shape1, default),
            int.MaxValue);
        await Assert.That(extremes).IsFalse();
        await Assert.That(minAgainstZero).IsFalse();
        await Assert.That(maxAgainstMin).IsFalse();
        await Assert.That(minAgainstItself).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorLong_OppositeExtremes_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new long[] { long.MaxValue };
        var b = new long[] { long.MinValue };
        var result = MathTolerance.IsApproximatelyEqual(new ReadOnlyTensorSpan<long>(a, Shape1, default), new ReadOnlyTensorSpan<long>(b, Shape1, default), long.MaxValue);
        await Assert.That(result).IsFalse();
    }

    // ----- AreParallel in double -----

    [Test]
    public async Task AreParallel_LargeVectors_UsesDoublePrecision(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Sine of the angle is 1e-3: parallel under 1e-2. The squared cross length overflowed float.
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1e15f, 0f, 0f), new Vector3(1e15f, 1e12f, 0f), 1e-2)).IsTrue();
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1e15f, 0f, 0f), new Vector3(1e15f, 1e12f, 0f), 1e-4)).IsFalse();
        // Exactly parallel at a magnitude whose length overflowed float.
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1e20f, 1e20f, 0f), new Vector3(2e20f, 2e20f, 0f), 1e-6)).IsTrue();
        // Orthogonal vectors, one huge, one small: the overflowed length used to turn the sine into 0.
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1e20f, 0f, 0f), new Vector3(0f, 1e-3f, 0f), 1e-4)).IsFalse();
    }

    // ----- Degenerate hull and the median of denormals -----

    [Test]
    public async Task ConvexHullContains_AllFacesDegenerate_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] hull = [Vector3.Zero, Vector3.UnitX, new(2f, 0, 0), Vector3.Zero, Vector3.Zero, Vector3.Zero];
        await Assert.That(Containment.ConvexHullContains(hull, new Vector3(100f, 100f, 100f), 1e-6)).IsFalse();
        await Assert.That(Containment.ConvexHullContains(hull, Vector3.Zero, 1e-6)).IsFalse();
    }

    [Test]
    public async Task HasMedianApproximately_DenormalPair_KeepsTheValue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Halving each operand rounded the smallest denormal to zero.
        double[] pair = [double.Epsilon, double.Epsilon];
        await Assert.That(Statistics.HasMedianApproximately(pair, double.Epsilon, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(pair, 0.0, 0.0)).IsFalse();
    }

    [Test]
    public async Task HasMedianApproximately_ExtremeNeighbors_NoOverflow(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] spread = [-double.MaxValue, double.MaxValue];
        double[] top = [double.MaxValue, double.MaxValue];
        double[] infinite = [double.PositiveInfinity, double.PositiveInfinity];
        await Assert.That(Statistics.HasMedianApproximately(spread, 0.0, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(top, double.MaxValue, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(infinite, double.PositiveInfinity, 0.0)).IsTrue();
    }

    // ----- Divisibility by minus one -----

    [Test]
    public async Task IsDivisibleBy_MinusOneAndOne_AlwaysTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsDivisibleBy(long.MinValue, -1)).IsTrue();
        await Assert.That(NumberTheory.IsDivisibleBy(long.MinValue, 1)).IsTrue();
        await Assert.That(NumberTheory.IsDivisibleBy(long.MaxValue, -1)).IsTrue();
        await Assert.That(NumberTheory.IsDivisibleBy(7, -1)).IsTrue();
    }

    // ----- IsRotation checks the affine form -----

    [Test]
    public async Task IsRotation_RotationMixingW_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var c = MathF.Cos(0.5f);
        var s = MathF.Sin(0.5f);
        var xw = new Matrix4x4(c, 0, 0, s, 0, 1, 0, 0, 0, 0, 1, 0, -s, 0, 0, c);
        var yw = new Matrix4x4(1, 0, 0, 0, 0, c, 0, s, 0, 0, 1, 0, 0, -s, 0, c);
        var zw = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, c, s, 0, 0, -s, c);
        await Assert.That(LinearAlgebra.IsOrthogonal(xw, 1e-5)).IsTrue();
        await Assert.That(LinearAlgebra.IsRotation(xw, 1e-5)).IsFalse();
        await Assert.That(LinearAlgebra.IsRotation(yw, 1e-5)).IsFalse();
        await Assert.That(LinearAlgebra.IsRotation(zw, 1e-5)).IsFalse();
    }

    [Test]
    public async Task IsRotation_LastDiagonalNotOne_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // diag(1, 1, -1, -1) is orthogonal with determinant +1 but its M44 is -1.
        var m = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, -1);
        await Assert.That(LinearAlgebra.IsOrthogonal(m, 1e-5)).IsTrue();
        await Assert.That(LinearAlgebra.IsRotation(m, 1e-5)).IsFalse();
    }

    [Test]
    public async Task IsRotation_AffineRotationAndSignFlipScale_StillTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.IsRotation(Matrix4x4.CreateFromYawPitchRoll(0.3f, 0.2f, 0.1f), 1e-5)).IsTrue();
        await Assert.That(LinearAlgebra.IsRotation(Matrix4x4.CreateScale(-1f, -1f, 1f), 1e-5)).IsTrue();
        await Assert.That(LinearAlgebra.IsRotation(Matrix4x4.CreateScale(-1f, 1f, 1f), 2.0)).IsTrue();
    }
}
