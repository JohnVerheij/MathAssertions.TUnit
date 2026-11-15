using System;
using System.Numerics;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the follow-up fixes to the review sweep: a ray-sphere test that assumed an exactly unit direction, statistics
/// that lost a small value or a small spread to a scaling chosen by the largest magnitude, a sum recovery that lost a
/// small value to its fixed scaling, and a tensor comparison that subtracted <see cref="float"/> and
/// <see cref="Half"/> elements in their own precision. Each test failed on the code it replaced, except two that pin
/// an answer the new code must keep: a zero direction, and the correctly rounded mean of a denormal sample.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class ReviewFollowUpTests
{
    private static readonly nint[] OneElement = [1];

    // ----- Intersection.Intersects(Ray3D, Sphere) -----

    [Test]
    public async Task RayIntersectsSphere_FloatNormalizedDirection_HitsADistantSphereOnTheRay(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // (0.6f, 0.8f) is a rounding away from unit length; projected without dividing by its squared length, the
        // center of a sphere on the ray 1e9 away lands about 50 away from its own projection.
        var ray = new Ray3D(Vector3.Zero, new Vector3(0.6f, 0.8f, 0f));

        await Assert.That(Intersection.Intersects(ray, new Sphere(new Vector3(644245120f, 858993472f, 0f), 1f))).IsTrue();
        await Assert.That(Intersection.Intersects(ray, new Sphere(new Vector3(644245120f - 800f, 858993472f + 600f, 0f), 1f))).IsFalse();
    }

    [Test]
    public async Task RayIntersectsSphere_NonUnitDirection_GivesTheSameAnswerAsTheUnitOne(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(new Vector3(6f, 8f, 0f), 1f);

        await Assert.That(Intersection.Intersects(new Ray3D(Vector3.Zero, new Vector3(3f, 4f, 0f)), sphere)).IsTrue();
        await Assert.That(Intersection.Intersects(new Ray3D(Vector3.Zero, new Vector3(-3f, -4f, 0f)), sphere)).IsFalse();
        await Assert.That(Intersection.Intersects(new Ray3D(Vector3.Zero, new Vector3(4f, -3f, 0f)), sphere)).IsFalse();
    }

    [Test]
    public async Task RayIntersectsSphere_ZeroDirection_HitsOnlyFromInside(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 2f);

        await Assert.That(Intersection.Intersects(new Ray3D(new Vector3(1f, 0f, 0f), Vector3.Zero), sphere)).IsTrue();
        await Assert.That(Intersection.Intersects(new Ray3D(new Vector3(3f, 0f, 0f), Vector3.Zero), sphere)).IsFalse();
    }

    // ----- Statistics -----

    [Test]
    public async Task HasMeanApproximately_SmallValueNextToLargeOnesThatCancel_CountsIt(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Scaling by the largest magnitude (2^-500) took 2^-600 below the smallest double.
        double[] values = [Math.ScaleB(1.0, 500), -Math.ScaleB(1.0, 500), Math.ScaleB(1.0, -600)];

        await Assert.That(Statistics.HasMeanApproximately(values, Math.ScaleB(1.0, -600) / 3, 0)).IsTrue();
        await Assert.That(Statistics.HasMeanApproximately(values, 0, 0)).IsFalse();
    }

    [Test]
    public async Task HasMeanApproximately_DenormalNextToOppositeExtremes_CountsWhatTheShiftRoundsAway(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The extremes need a shift, which rounds 3 * double.Epsilon away; it is added back, so the mean is the exact
        // double.Epsilon.
        double[] values = [double.MaxValue, -double.MaxValue, 3 * double.Epsilon];

        await Assert.That(Statistics.HasMeanApproximately(values, double.Epsilon, 0)).IsTrue();
        await Assert.That(Statistics.HasMeanApproximately(values, 0, 0)).IsFalse();
    }

    [Test]
    public async Task HasMeanApproximately_DenormalSample_IsCorrectlyRounded(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The mean of [e, 2e] is 1.5e, which rounds to 2e; a running mean over the unscaled denormals rounded the
        // half step to zero on the way and gave e. A sample below 1 is scaled up first, as before.
        double[] values = [double.Epsilon, 2 * double.Epsilon];

        await Assert.That(Statistics.HasMeanApproximately(values, 2 * double.Epsilon, 0)).IsTrue();
        await Assert.That(Statistics.HasMeanApproximately(values, double.Epsilon, 0)).IsFalse();
    }

    [Test]
    public async Task SigmaChecks_ValuesFarCloserThanTheirMagnitude_KeepTheirStandardDeviation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The largest magnitude (2^-488) needed no scaling, but the product of the differences (2^-1079) underflowed,
        // so the standard deviation was zero and both values fell outside their own one-sigma envelope.
        var a = Math.ScaleB(1.0, -488);
        double[] values = [a, a + Math.ScaleB(1.0, -539)];

        await Assert.That(Statistics.HasStdDevApproximately(values, Math.ScaleB(Math.Sqrt(0.5), -539), Math.ScaleB(1.0, -560))).IsTrue();
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 1)).IsTrue();
        await Assert.That(Statistics.IsWithinSigmasOfMean(values[0], values, 1)).IsTrue();
        await Assert.That(Statistics.IsWithinSigmasOfMean(a - Math.ScaleB(1.0, -537), values, 1)).IsFalse();
    }

    [Test]
    public async Task HasSumApproximately_SmallValueNextToExtremesThatCancel_CountsIt(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The running sum overflows; the fixed 2^-32 scaling of the recovery took 2^-1050 below the smallest double.
        var small = Math.ScaleB(1.0, -1050);
        double[] values = [double.MaxValue, double.MaxValue, -double.MaxValue, -double.MaxValue, small];
        double[] denormal = [double.MaxValue, double.MaxValue, -double.MaxValue, -double.MaxValue, double.Epsilon];

        await Assert.That(Statistics.HasSumApproximately(values, small, 0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(values, 0, 0)).IsFalse();
        await Assert.That(Statistics.HasSumApproximately(denormal, double.Epsilon, 0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(denormal, 0, 0)).IsFalse();
    }

    // ----- MathTolerance, tensor overload -----

    [Test]
    public async Task IsApproximatelyEqual_FloatAndHalfTensors_FormTheDifferenceInDouble(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 2^24 against -1 is 2^24 + 1 apart; the float subtraction rounds that to 2^24, which met the tolerance, as
        // 2048 against -1 does in Half.
        float[] large = [16777216f];
        float[] minusOne = [-1f];
        float[] zero = [0f];
        Half[] largeHalf = [Half.CreateChecked(2048)];
        Half[] minusOneHalf = [Half.NegativeOne];
        Half[] zeroHalf = [Half.Zero];

        await Assert.That(MathTolerance.IsApproximatelyEqual(Span(large), Span(minusOne), 16777216f)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(Span(large), Span(zero), 16777216f)).IsTrue();
        await Assert.That(MathTolerance.IsApproximatelyEqual(Span(largeHalf), Span(minusOneHalf), Half.CreateChecked(2048))).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(Span(largeHalf), Span(zeroHalf), Half.CreateChecked(2048))).IsTrue();
    }

    /// <summary>A one-element tensor span over <paramref name="values"/>.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="values">The one element.</param>
    /// <returns>The tensor span.</returns>
    private static ReadOnlyTensorSpan<T> Span<T>(T[] values) => new(values, OneElement, default);
}
