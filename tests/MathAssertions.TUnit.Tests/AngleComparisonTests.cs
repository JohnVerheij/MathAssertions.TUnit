using System;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the angle comparison modulo 360 degrees: the core helpers (<see cref="MathTolerance.AngleDifferenceDegrees(double, double)"/>
/// and <see cref="MathTolerance.IsAngleApproximatelyEqual(double, double, double)"/>) and the fluent
/// <c>IsAngleApproximatelyEqualTo</c> assertion, with its full failure text.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class AngleComparisonTests
{
    [Test]
    [Arguments(0.0, 0.0, 0.0, DisplayName = "Difference_Equal")]
    [Arguments(10.0, 25.0, 15.0, DisplayName = "Difference_Plain")]
    [Arguments(359.0, 1.0, 2.0, DisplayName = "Difference_WrapsAcrossZero")]
    [Arguments(1.0, 359.0, 2.0, DisplayName = "Difference_WrapsAcrossZeroReversed")]
    [Arguments(180.0, -180.0, 0.0, DisplayName = "Difference_PlusMinusHalfTurn")]
    [Arguments(179.0, -179.0, 2.0, DisplayName = "Difference_NearHalfTurn")]
    [Arguments(0.0, 180.0, 180.0, DisplayName = "Difference_Opposite")]
    [Arguments(0.0, 360.0, 0.0, DisplayName = "Difference_FullTurn")]
    [Arguments(-90.0, 270.0, 0.0, DisplayName = "Difference_NegativeAgainstPositiveEquivalent")]
    [Arguments(720.0, 0.0, 0.0, DisplayName = "Difference_TwoTurns")]
    [Arguments(1e7, 1e7 + 5.0, 5.0, DisplayName = "Difference_HugeAngles")]
    [Arguments(1e300, -1e300, 0.0, DisplayName = "Difference_OppositeHugeAnglesDoNotOverflow")]
    [Arguments(1e7, -1e7, 160.0, DisplayName = "Difference_OppositeHugeAnglesWithResidue")]
    public async Task AngleDifferenceDegrees_Double_WrapsToTheShorterArc(double x, double y, double expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var difference = MathTolerance.AngleDifferenceDegrees(x, y);
        await Assert.That(difference).IsEqualTo(expected).Within(1e-6);
        // The wrapped difference is symmetric and never leaves [0, 180].
        await Assert.That(MathTolerance.AngleDifferenceDegrees(y, x)).IsEqualTo(difference);
        await Assert.That(difference).IsGreaterThanOrEqualTo(0.0);
        await Assert.That(difference).IsLessThanOrEqualTo(180.0);
    }

    [Test]
    [Arguments(double.NaN, 0.0, DisplayName = "Difference_NaNFirst")]
    [Arguments(0.0, double.NaN, DisplayName = "Difference_NaNSecond")]
    [Arguments(double.NaN, double.NaN, DisplayName = "Difference_BothNaN")]
    [Arguments(double.PositiveInfinity, 0.0, DisplayName = "Difference_InfinityFirst")]
    [Arguments(0.0, double.NegativeInfinity, DisplayName = "Difference_NegativeInfinitySecond")]
    public async Task AngleDifferenceDegrees_NotANumber_IsNaN(double a, double b, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(double.IsNaN(MathTolerance.AngleDifferenceDegrees(a, b))).IsTrue();
        await Assert.That(MathTolerance.IsAngleApproximatelyEqual(a, b, 1000.0)).IsFalse();
    }

    [Test]
    public async Task AngleDifferenceDegrees_Float_StaysInSinglePrecision(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.AngleDifferenceDegrees(359f, 1f)).IsEqualTo(2f);
        await Assert.That(MathTolerance.AngleDifferenceDegrees(180f, -180f)).IsEqualTo(0f);
        await Assert.That(MathTolerance.AngleDifferenceDegrees(-179.5f, 179.5f)).IsEqualTo(1f);
        await Assert.That(float.IsNaN(MathTolerance.AngleDifferenceDegrees(float.NaN, 1f))).IsTrue();
        await Assert.That(float.IsNaN(MathTolerance.AngleDifferenceDegrees(1f, float.PositiveInfinity))).IsTrue();
    }

    [Test]
    [Arguments(359.0, 1.0, 2.0, true, DisplayName = "Predicate_AtTheBoundary")]
    [Arguments(359.0, 1.0, 1.999, false, DisplayName = "Predicate_JustBelowTheBoundary")]
    [Arguments(180.0, -180.0, 0.0, true, DisplayName = "Predicate_HalfTurnsAtZeroTolerance")]
    [Arguments(10.0, 20.0, 5.0, false, DisplayName = "Predicate_Outside")]
    [Arguments(10.0, 20.0, double.PositiveInfinity, true, DisplayName = "Predicate_InfiniteTolerance")]
    public async Task IsAngleApproximatelyEqual_Double_UsesTheWrappedDifference(double a, double b, double tolerance, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsAngleApproximatelyEqual(a, b, tolerance)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsAngleApproximatelyEqual_Float_UsesTheWrappedDifference(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsAngleApproximatelyEqual(179.99f, -179.99f, 0.05f)).IsTrue();
        await Assert.That(MathTolerance.IsAngleApproximatelyEqual(179f, -179f, 1.5f)).IsFalse();
        await Assert.That(MathTolerance.IsAngleApproximatelyEqual(float.NaN, float.NaN, 1f)).IsFalse();
    }

    [Test]
    [Arguments(double.NaN, DisplayName = "Tolerance_NaN")]
    [Arguments(-1.0, DisplayName = "Tolerance_Negative")]
    public async Task IsAngleApproximatelyEqual_InvalidTolerance_Throws(double tolerance, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException? doubleEx = await Assert.That(() => MathTolerance.IsAngleApproximatelyEqual(1.0, 1.0, tolerance))
            .Throws<ArgumentOutOfRangeException>();
        ArgumentOutOfRangeException? floatEx = await Assert.That(() => MathTolerance.IsAngleApproximatelyEqual(1f, 1f, (float)tolerance))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(doubleEx!.ParamName).IsEqualTo("toleranceDegrees");
        await Assert.That(floatEx!.ParamName).IsEqualTo("toleranceDegrees");
    }

    [Test]
    public async Task IsAngleApproximatelyEqualTo_Passes_ForWrappedAngles(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var almostFullTurn = 359.5;
        var minusHalfTurn = -180.0;
        var floatAngle = 179.99f;
        await Assert.That(almostFullTurn).IsAngleApproximatelyEqualTo(0.0, toleranceDegrees: 0.5);
        await Assert.That(minusHalfTurn).IsAngleApproximatelyEqualTo(180.0, toleranceDegrees: 0.0);
        await Assert.That(floatAngle).IsAngleApproximatelyEqualTo(-180f, toleranceDegrees: 0.05f);
    }

    [Test]
    public async Task IsAngleApproximatelyEqualTo_Double_Fails_WithTheWrappedDifference(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var actual = 350.0;
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(actual).IsAngleApproximatelyEqualTo(5.0, toleranceDegrees: 10.0);
        }).Throws<AssertionException>();

        // The renderer ends lines with Environment.NewLine; compare with LF so the pin holds on every platform.
        var message = ex!.Message.ReplaceLineEndings("\n");
        await Assert.That(message).IsEqualTo(
            "Expected to satisfy IsAngleApproximatelyEqualTo(5, 10)\n"
            + "but the wrapped difference is 15 degrees, outside the tolerance of 10 degrees\n"
            + "  actual:   350 degrees\n"
            + "  expected: 5 degrees (angles compared modulo 360)\n"
            + "\n"
            + "at Assert.That(actual).IsAngleApproximatelyEqualTo(5.0, 10.0)");
    }

    [Test]
    public async Task IsAngleApproximatelyEqualTo_Float_Fails_WithTheWrappedDifference(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var actual = -170f;
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(actual).IsAngleApproximatelyEqualTo(170f, toleranceDegrees: 10f);
        }).Throws<AssertionException>();

        var message = ex!.Message.ReplaceLineEndings("\n");
        await Assert.That(message).IsEqualTo(
            "Expected to satisfy IsAngleApproximatelyEqualTo(170, 10)\n"
            + "but the wrapped difference is 20 degrees, outside the tolerance of 10 degrees\n"
            + "  actual:   -170 degrees\n"
            + "  expected: 170 degrees (angles compared modulo 360)\n"
            + "\n"
            + "at Assert.That(actual).IsAngleApproximatelyEqualTo(170f, 10f)");
    }

    [Test]
    public async Task IsAngleApproximatelyEqualTo_NaN_FailsAndShowsNaN(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var notANumber = double.NaN;
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(notANumber).IsAngleApproximatelyEqualTo(notANumber, toleranceDegrees: 10.0);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("but the wrapped difference is NaN degrees, outside the tolerance of 10 degrees");
    }
}
