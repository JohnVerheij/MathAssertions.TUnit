using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the numeric and message fixes of the review sweep: float arithmetic that overflowed, underflowed or rounded
/// before it was widened, sums and statistics that overflowed on the way, progressions that matched on NaN, a pair of
/// NaN vectors reported as independent, and a rotation message whose dot disagreed with the verdict. Each test failed
/// on the code it replaced.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class ReviewSweepTests
{
    // ----- MathTolerance -----

    [Test]
    public async Task IsApproximatelyEqual_Float_DifferenceJustAboveTheToleranceIsNotRoundedDownToIt(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 2^24 against -1 is 2^24 + 1 apart; the float subtraction rounds that to 2^24, which met the tolerance.
        await Assert.That(MathTolerance.IsApproximatelyEqual(16777216f, -1f, 16777216f)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(16777216f, 0f, 16777216f)).IsTrue();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_InfiniteRelativeToleranceOverZeroScale_IsClose(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // infinity * 0 is NaN; Math.Max passed the NaN on and rejected two equal zeros.
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(0.0, 0.0, double.PositiveInfinity, 0.0)).IsTrue();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 2.0, double.PositiveInfinity, 0.0)).IsTrue();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 2.0, 0.1, 0.1)).IsFalse();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 1.05, 0.0, 0.1)).IsTrue();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(100.0, 105.0, 0.1, 0.0)).IsTrue();
    }

    // ----- Statistics -----

    [Test]
    public async Task HasMeanApproximately_OppositeExtremes_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The difference of the two values is beyond the double range, which made the mean infinite.
        double[] values = [double.MaxValue, -double.MaxValue];
        await Assert.That(Statistics.HasMeanApproximately(values, 0.0, 0.0)).IsTrue();
        await Assert.That(Statistics.MeanAndVariance(values).Mean).IsEqualTo(0.0);
        await Assert.That(double.IsPositiveInfinity(Statistics.MeanAndVariance(values).Variance)).IsTrue();
    }

    [Test]
    public async Task HasStdDevApproximately_VarianceBeyondTheDoubleRange_StillMeasuresTheStandardDeviation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The variance of these two values is 2e400, which does not fit; their standard deviation, 1.41e200, does.
        double[] values = [1e200, 3e200];
        await Assert.That(Statistics.HasStdDevApproximately(values, 1.4142135623730951e200, 1e186)).IsTrue();
        await Assert.That(Statistics.HasStdDevApproximately(values, 1e200, 1e186)).IsFalse();
        await Assert.That(Statistics.HasVarianceApproximately(values, double.PositiveInfinity, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMeanApproximately(values, 2e200, 1e186)).IsTrue();
    }

    [Test]
    public async Task HasStdDevApproximately_VarianceBelowTheDoubleRange_StillMeasuresTheStandardDeviation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The variance of these two values, 2e-400, underflows to zero; their standard deviation, 1.41e-200, does not.
        double[] values = [1e-200, 3e-200];
        await Assert.That(Statistics.HasStdDevApproximately(values, 1.4142135623730951e-200, 1e-214)).IsTrue();
        await Assert.That(Statistics.HasVarianceApproximately(values, 0.0, 0.0)).IsTrue();
    }

    [Test]
    public async Task MeanAndVariance_ZeroSample_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [0.0, 0.0, 0.0];
        await Assert.That(Statistics.MeanAndVariance(values)).IsEqualTo((0.0, 0.0));
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_OutlierBeyondTheDoubleRange_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The variance overflowed to infinity, so the envelope was infinite and the outlier passed.
        double[] values = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1e200];
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 1.0)).IsFalse();
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 4.0)).IsTrue();
        await Assert.That(Statistics.IsWithinSigmasOfMean(1e200, values, 1.0)).IsFalse();
        await Assert.That(Statistics.IsWithinSigmasOfMean(1e199, values, 1.0)).IsTrue();
    }

    [Test]
    public async Task HasSumApproximately_IntermediateOverflow_IsTheExactTotal(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [double.MaxValue, double.MaxValue, -double.MaxValue];
        await Assert.That(Statistics.HasSumApproximately(values, double.MaxValue, 0.0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(values, double.PositiveInfinity, 0.0)).IsFalse();
    }

    [Test]
    public async Task HasSumApproximately_SumBeyondTheDoubleRange_IsInfinite(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [double.MaxValue, double.MaxValue];
        await Assert.That(Statistics.HasSumApproximately(values, double.PositiveInfinity, 0.0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(values, double.MaxValue, 0.0)).IsFalse();
        double[] withInfinity = [1.0, double.PositiveInfinity];
        await Assert.That(Statistics.HasSumApproximately(withInfinity, double.PositiveInfinity, 0.0)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_EqualNeighbors_AreExact(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 3.3 * (1 - f) + 3.3 * f rounds to 3.2999999999999994 at 7 percent.
        double[] values = [3.3, 3.3];
        await Assert.That(Statistics.HasPercentileApproximately(values, 7.0, 3.3, 0.0)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 9.0, 3.3, 0.0)).IsTrue();
    }

    // ----- Sequences -----

    [Test]
    public async Task IsArithmeticProgression_NaNOrInfinity_IsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // NaN minus NaN is NaN, and the tolerance comparison treats NaN as equal to NaN.
        await Assert.That(Sequences.IsArithmeticProgression([double.NaN, double.NaN, double.NaN], 0.1)).IsFalse();
        await Assert.That(Sequences.IsArithmeticProgression([1.0, double.NaN, double.NaN], 0.1)).IsFalse();
        await Assert.That(Sequences.IsArithmeticProgression([double.NaN], 0.1)).IsFalse();
        await Assert.That(Sequences.IsArithmeticProgression([double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity], 0.1)).IsFalse();
        await Assert.That(Sequences.IsArithmeticProgression([1.0, 2.0, 3.0], 0.1)).IsTrue();
    }

    [Test]
    public async Task IsGeometricProgression_NaNOrInfinity_IsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsGeometricProgression([double.NaN, double.NaN, double.NaN], 0.1)).IsFalse();
        await Assert.That(Sequences.IsGeometricProgression([1.0, double.NaN, double.NaN], 0.1)).IsFalse();
        await Assert.That(Sequences.IsGeometricProgression([double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity], 0.1)).IsFalse();
        await Assert.That(Sequences.IsGeometricProgression([1.0, 2.0, 4.0], 0.1)).IsTrue();
    }

    // ----- LinearAlgebra -----

    [Test]
    public async Task AreLinearlyIndependent_NaNOrInfiniteVector_IsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // "Not parallel" was read off the NaN and reported independence for a pair.
        Vector3[] withNaN = [new(float.NaN, 0f, 0f), Vector3.UnitY];
        Vector3[] withInfinity = [new(float.PositiveInfinity, 0f, 0f), Vector3.UnitY];
        Vector3[] single = [new(float.NaN, 0f, 0f)];
        Vector3[] triple = [new(0f, float.NaN, 0f), Vector3.UnitY, Vector3.UnitZ];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(withNaN, 1e-6)).IsFalse();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(withInfinity, 1e-6)).IsFalse();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(single, 1e-6)).IsFalse();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(triple, 1e-6)).IsFalse();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent([Vector3.UnitX, Vector3.UnitY], 1e-6)).IsTrue();
    }

    [Test]
    public async Task AreLinearlyIndependent_TinyVectors_AreMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float squares and the float triple product underflow to zero.
        Vector3[] single = [new(1e-30f, 0f, 0f)];
        Vector3[] triple = [new(1e-20f, 0f, 0f), new(0f, 1e-20f, 0f), new(0f, 0f, 1e-20f)];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(single, 1e-35)).IsTrue();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(triple, 1e-70)).IsTrue();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(triple, 1e-59)).IsFalse();
    }

    [Test]
    public async Task AreOrthogonal_LargeComponents_DoNotOverflowTheDot(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float products are 1e40 each, infinite, and infinity minus infinity is NaN.
        await Assert.That(LinearAlgebra.AreOrthogonal(new Vector3(1e20f, 1e20f, 0f), new Vector3(1e20f, -1e20f, 0f), 1.0)).IsTrue();
        await Assert.That(LinearAlgebra.AreOrthogonal(new Vector3(1e20f, 1e20f, 0f), new Vector3(1e20f, 1e20f, 0f), 1.0)).IsFalse();
    }

    [Test]
    public async Task AngleBetween_TinyVectors_KeepTheirAngle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float cross and dot products underflow to zero, which read as a zero vector and gave 0.
        var angle = LinearAlgebra.AngleBetween(new Vector3(1e-30f, 0f, 0f), new Vector3(0f, 1e-30f, 0f));
        await Assert.That(angle).IsEqualTo(Math.PI / 2).Within(1e-12);
    }

    // ----- Properties -----

    [Test]
    public async Task IsCollinear_PointsThatOverflowTheFloatDifference_AreMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // (3e38, 0, 0) minus (-3e38, 0, 0) overflows in float; the NaN distance then passed the third point.
        Vector3[] points = [new(3e38f, 0f, 0f), new(-3e38f, 0f, 0f), new(0f, 5f, 0f)];
        await Assert.That(Properties.IsCollinear(points, 1.0)).IsFalse();
        Vector3[] onLine = [new(3e38f, 0f, 0f), new(-3e38f, 0f, 0f), new(0f, 0.5f, 0f)];
        await Assert.That(Properties.IsCollinear(onLine, 1.0)).IsTrue();
    }

    [Test]
    public async Task IsCollinear_TinyEdges_AreNotTakenForCoincidentPoints(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float edge length of 1e-30 underflows to zero, so no direction was found and the set was collinear.
        Vector3[] points = [Vector3.Zero, new(1e-30f, 0f, 0f), new(2e-30f, 1e-30f, 0f)];
        await Assert.That(Properties.IsCollinear(points, 0.0)).IsFalse();
        Vector3[] onLine = [Vector3.Zero, new(1e-30f, 0f, 0f), new(2e-30f, 0f, 0f)];
        await Assert.That(Properties.IsCollinear(onLine, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsDegenerate_TinyTriangleUnderAToleranceOfZero_IsNotDegenerate(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float area of 5e-51 underflows to zero.
        var tiny = new Triangle3D(Vector3.Zero, new Vector3(1e-25f, 0f, 0f), new Vector3(0f, 1e-25f, 0f));
        var flat = new Triangle3D(Vector3.Zero, new Vector3(1e-25f, 0f, 0f), new Vector3(2e-25f, 0f, 0f));
        var withNaN = new Triangle3D(Vector3.Zero, new Vector3(float.NaN, 0f, 0f), Vector3.UnitY);
        await Assert.That(Properties.IsDegenerate(tiny, 0.0)).IsFalse();
        await Assert.That(Properties.IsDegenerate(flat, 0.0)).IsTrue();
        await Assert.That(Properties.IsDegenerate(withNaN, 1e9)).IsFalse();
    }

    // ----- Containment and intersection of spheres -----

    [Test]
    public async Task SphereContainsPoint_HugeAndTinyRadii_AreMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float squares are infinite for the first sphere (everything was inside) and zero for the second.
        var huge = new Sphere(Vector3.Zero, 1e20f);
        await Assert.That(Containment.Contains(huge, new Vector3(1e30f, 0f, 0f))).IsFalse();
        await Assert.That(Containment.Contains(huge, new Vector3(1e19f, 0f, 0f))).IsTrue();
        var tiny = new Sphere(Vector3.Zero, 1e-25f);
        await Assert.That(Containment.Contains(tiny, new Vector3(2e-25f, 0f, 0f))).IsFalse();
        await Assert.That(Containment.Contains(tiny, new Vector3(5e-26f, 0f, 0f))).IsTrue();
    }

    [Test]
    public async Task BoxContainsSphere_SphereOneUlpOutsideAFace_IsNotContained(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Min.X + 1 is 2^24 + 1, which the float sum rounds down to the center at 2^24: the sphere sticks out.
        var box = new AxisAlignedBox(new Vector3(16777216f, 0f, 0f), new Vector3(1e9f, 10f, 10f));
        var sphere = new Sphere(new Vector3(16777216f, 5f, 5f), 1f);
        await Assert.That(Containment.Contains(box, sphere)).IsFalse();
        var inside = new Sphere(new Vector3(16777218f, 5f, 5f), 1f);
        await Assert.That(Containment.Contains(box, inside)).IsTrue();
    }

    [Test]
    public async Task SpheresIntersect_HugeAndTinyRadii_AreMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The squares are infinite (huge pair) or zero (tiny pair) in float, and infinity <= infinity, 0 <= 0 passed.
        await Assert.That(Intersection.Intersects(new Sphere(Vector3.Zero, 1e20f), new Sphere(new Vector3(1e30f, 0f, 0f), 1e20f))).IsFalse();
        await Assert.That(Intersection.Intersects(new Sphere(Vector3.Zero, 1e20f), new Sphere(new Vector3(1e20f, 0f, 0f), 1e20f))).IsTrue();
        await Assert.That(Intersection.Intersects(new Sphere(Vector3.Zero, 1e-25f), new Sphere(new Vector3(5e-25f, 0f, 0f), 1e-25f))).IsFalse();
        await Assert.That(Intersection.Intersects(new Sphere(Vector3.Zero, 1e-25f), new Sphere(new Vector3(1.5e-25f, 0f, 0f), 1e-25f))).IsTrue();
    }

    [Test]
    public async Task RayIntersectsSphere_DistantSphere_IsMeasuredByThePerpendicularDistance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // l2 and s * s are both 1e60: in float they overflow to infinity (NaN difference, a miss) and in double
        // their difference is rounding noise. The perpendicular distance, 0.5 here, is what decides.
        var ray = new Ray3D(Vector3.Zero, Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, new Sphere(new Vector3(1e30f, 0.5f, 0f), 1f))).IsTrue();
        await Assert.That(Intersection.Intersects(ray, new Sphere(new Vector3(1e30f, 5f, 0f), 1f))).IsFalse();
        await Assert.That(Intersection.Intersects(ray, new Sphere(new Vector3(-1e30f, 0.5f, 0f), 1f))).IsFalse();
    }

    // ----- Distance -----

    [Test]
    public async Task DistanceFromSegment_TinyAndHugeSegments_AreMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Tiny: |ab|^2 underflows to zero, the segment read as a point and the distance underflowed to zero too.
        var tiny = new LineSegment3D(Vector3.Zero, new Vector3(1e-30f, 0f, 0f));
        await Assert.That(Distance.DistanceFrom(new Vector3(5e-31f, 1e-30f, 0f), tiny)).IsEqualTo(1e-30).Within(1e-36);

        // Huge: End - Start overflows to infinity in float and the projection was NaN.
        var huge = new LineSegment3D(new Vector3(-3e38f, 0f, 0f), new Vector3(3e38f, 0f, 0f));
        await Assert.That(Distance.DistanceFrom(new Vector3(0f, 1f, 0f), huge)).IsEqualTo(1.0).Within(1e-9);
    }

    [Test]
    public async Task DistanceFromPlane_UsesDoublePrecision(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Overflow: the float dot of (3e38, 3e38, 0) with (1, 1, 0) is infinite.
        var overflowing = new Plane(new Vector3(1f, 1f, 0f), -3e38f);
        await Assert.That(Distance.DistanceFrom(new Vector3(3e38f, 3e38f, 0f), overflowing)).IsEqualTo(3e38f).Within(1e24);

        // Rounding: the float dot of these magnitudes is off by about 1; the exact value is formed in double.
        var plane = new Plane(new Vector3(0.6f, 0.8f, 0f), -1.4e7f);
        double x = 0.6f;
        double y = 0.8f;
        double expected = Math.Abs((1e7 * x) + (1e7 * y) - 1.4e7f);
        await Assert.That(Distance.DistanceFrom(new Vector3(1e7f, 1e7f, 0f), plane)).IsEqualTo(expected).Within(1e-9);
    }

    [Test]
    public async Task DistanceFromTriangle_TinyTriangle_IsMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Every float dot product of this triangle underflows to zero, which placed the point in the region of
        // vertex A and measured the distance to A (itself underflowed to zero).
        var triangle = new Triangle3D(Vector3.Zero, new Vector3(1e-25f, 0f, 0f), new Vector3(0f, 1e-25f, 0f));
        await Assert.That(Distance.DistanceFrom(new Vector3(1e-26f, 1e-26f, 1e-25f), triangle)).IsEqualTo(1e-25).Within(1e-30);
    }

    // ----- Pointcloud -----

    [Test]
    public async Task HasCentroidAt_CoordinatesNearTheFloatLimit_DoNotOverflowTheSum(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Vector3[] cloud = [new(3e38f, 0f, 0f), new(3e38f, 0f, 0f)];
        await Assert.That(Pointcloud.HasCentroidAt(cloud, new Vector3(3e38f, 0f, 0f), 0.0)).IsTrue();
        await Assert.That(Pointcloud.HasCentroidAt(cloud, new Vector3(0f, 0f, 0f), 1.0)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyOnSphere_TinySphere_IsMeasuredInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The float distance 1e-25 underflows to zero, so the point read as 1e-25 inside the surface.
        Vector3[] cloud = [new(1e-25f, 0f, 0f)];
        await Assert.That(Pointcloud.IsApproximatelyOnSphere(cloud, Vector3.Zero, 1e-25f, 1e-30)).IsTrue();
        await Assert.That(Pointcloud.IsApproximatelyOnSphere(cloud, Vector3.Zero, 2e-25f, 1e-30)).IsFalse();
    }
}
