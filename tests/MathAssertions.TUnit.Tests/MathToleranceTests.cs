using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the NaN-aware, infinity-aware semantics and argument validation of
/// <see cref="MathTolerance"/>. Covers every public overload of the v0.0.1 surface
/// (scalar double, scalar float, <see cref="Vector3"/>) on the happy path, the boundary
/// cases (NaN, infinity, zero tolerance), and the validation failures (NaN tolerance,
/// negative tolerance).
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class MathToleranceTests
{
    [Test]
    [Arguments(1.0, 1.0001, 0.001, true)]
    [Arguments(0.0, 0.0, 0.0, true)]
    [Arguments(-1.0, -1.0001, 0.001, true)]
    [Arguments(1.0, 1.002, 0.001, false)]
    [Arguments(0.0, 0.001, 0.0, false)]
    public async Task IsApproximatelyEqual_Double_HappyPath(double a, double b, double tolerance, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = MathTolerance.IsApproximatelyEqual(a, b, tolerance);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_BothNaN_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(double.NaN, double.NaN, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_OneNaN_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(double.NaN, 1.0, 1e-6)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0, double.NaN, 1e-6)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_BothPositiveInfinity_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            double.PositiveInfinity, double.PositiveInfinity, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_BothNegativeInfinity_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            double.NegativeInfinity, double.NegativeInfinity, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_OppositeInfinity_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            double.PositiveInfinity, double.NegativeInfinity, 1e-6)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_InfinityAndFinite_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            double.PositiveInfinity, 1.0, 1e-6)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            1.0, double.NegativeInfinity, 1e-6)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(1.0, 1.0, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(1.0, 1.0, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_ZeroTolerance_RequiresExactMatch(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0, 1.0, 0.0)).IsTrue();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0, 1.0001, 0.0)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_HappyPath(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0f, 1.0001f, 0.001f)).IsTrue();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0f, 1.002f, 0.001f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_BothNaN_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(float.NaN, float.NaN, 1e-6f)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_OneNaN_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(float.NaN, 1.0f, 1e-6f)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(1.0f, float.NaN, 1e-6f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_BothPositiveInfinity_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            float.PositiveInfinity, float.PositiveInfinity, 1e-6f)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_BothNegativeInfinity_ReturnsTrue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            float.NegativeInfinity, float.NegativeInfinity, 1e-6f)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_OppositeInfinity_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            float.PositiveInfinity, float.NegativeInfinity, 1e-6f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_InfinityAndFinite_ReturnsFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            float.PositiveInfinity, 1.0f, 1e-6f)).IsFalse();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            1.0f, float.NegativeInfinity, 1e-6f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(1.0f, 1.0f, float.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(1.0f, 1.0f, -1e-6f))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_AllComponentsWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_FirstComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(99, 2, 3);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_MiddleComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1, 99, 3);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_LastComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1, 2, 99);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    /// <summary>
    /// Pins the precision-preserving cast: components widen to double before comparing
    /// against the double tolerance. If the implementation cast tolerance to float instead,
    /// a tolerance below float epsilon (~1.19e-7) would silently round to zero and reject
    /// every non-bit-identical input. This test fixes the expectation at full double precision.
    /// </summary>
    [Test]
    public async Task IsApproximatelyEqual_Vector3_TightDoubleTolerance_HonorsDoublePrecision(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1.0f, 1.0f, 1.0f);
        var b = new Vector3(1.0f, 1.0f, 1.0f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Vector3.Zero, Vector3.Zero, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Vector3.Zero, Vector3.Zero, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsCloseInUlps(double) -----

    [Test]
    public async Task IsCloseInUlps_Double_SameValue_TrueRegardlessOfUlp(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(1.0, 1.0, 0L)).IsTrue();
        await Assert.That(MathTolerance.IsCloseInUlps(double.MaxValue, double.MaxValue, 0L)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Double_OneUlpApart_TrueWithUlp1(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = 1.0;
        var b = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a) + 1);
        await Assert.That(MathTolerance.IsCloseInUlps(a, b, 1L)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Double_TenUlpsApart_FalseWithUlp1(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = 1.0;
        var b = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a) + 10);
        await Assert.That(MathTolerance.IsCloseInUlps(a, b, 1L)).IsFalse();
        await Assert.That(MathTolerance.IsCloseInUlps(a, b, 10L)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Double_OppositeSigns_AlwaysFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(1e-300, -1e-300, long.MaxValue)).IsFalse();
    }

    [Test]
    public async Task IsCloseInUlps_Double_PositiveAndNegativeZero_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(0.0, -0.0, 0L)).IsTrue();
        await Assert.That(MathTolerance.IsCloseInUlps(-0.0, 0.0, 0L)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Double_BothNaN_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(double.NaN, double.NaN, 0L)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Double_OneNaN_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(double.NaN, 1.0, long.MaxValue)).IsFalse();
        await Assert.That(MathTolerance.IsCloseInUlps(1.0, double.NaN, long.MaxValue)).IsFalse();
    }

    [Test]
    public async Task IsCloseInUlps_Double_NegativeUlpDistance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsCloseInUlps(1.0, 1.0, -1L))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsCloseInUlps(float) -----

    [Test]
    public async Task IsCloseInUlps_Float_SameValue_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(1.0f, 1.0f, 0)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Float_OneUlpApart_TrueWithUlp1(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = 1.0f;
        var b = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(a) + 1);
        await Assert.That(MathTolerance.IsCloseInUlps(a, b, 1)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Float_PositiveAndNegativeZero_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(0.0f, -0.0f, 0)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Float_NegativeUlpDistance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsCloseInUlps(1.0f, 1.0f, -1))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsCloseInUlps_Float_BothNaN_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(float.NaN, float.NaN, 0)).IsTrue();
    }

    [Test]
    public async Task IsCloseInUlps_Float_OneNaN_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(float.NaN, 1.0f, int.MaxValue)).IsFalse();
        await Assert.That(MathTolerance.IsCloseInUlps(1.0f, float.NaN, int.MaxValue)).IsFalse();
    }

    [Test]
    public async Task IsCloseInUlps_Float_OppositeSigns_AlwaysFalse(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsCloseInUlps(1e-30f, -1e-30f, int.MaxValue)).IsFalse();
    }

    // ----- IsRelativelyAndAbsolutelyClose -----

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_LargeValues_DominantRelative(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Difference is 1e6 (huge), but at scale 1e9 the relative tolerance 1e-3 allows up to 1e6.
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            1e9, 1e9 + 1e6, 1e-3, 1e-12)).IsTrue();
        // Beyond the relative envelope.
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            1e9, 1e9 + 1e7, 1e-3, 1e-12)).IsFalse();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_SmallValues_DominantAbsolute(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Near zero, the relative term collapses to ~0; the absolute term is the floor.
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            1e-15, 1e-15 + 5e-13, 1e-9, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            1e-15, 1e-15 + 5e-11, 1e-9, 1e-12)).IsFalse();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_BothNaN_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.NaN, double.NaN, 1e-9, 1e-12)).IsTrue();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_OneNaN_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.NaN, 1.0, 1e-9, 1e-12)).IsFalse();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            1.0, double.NaN, 1e-9, 1e-12)).IsFalse();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_SameInfinity_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.PositiveInfinity, double.PositiveInfinity, 1e-9, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.NegativeInfinity, double.NegativeInfinity, 1e-9, 1e-12)).IsTrue();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_OppositeInfinity_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.PositiveInfinity, double.NegativeInfinity, 1e-9, 1e-12)).IsFalse();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_InfinityAndFinite_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsRelativelyAndAbsolutelyClose(
            double.PositiveInfinity, 1e300, 1e-9, 1e-12)).IsFalse();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_NegativeRelativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 1.0, -1e-9, 1e-12))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_NegativeAbsoluteTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 1.0, 1e-9, -1e-12))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsRelativelyAndAbsolutelyClose_NaNRelativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsRelativelyAndAbsolutelyClose(1.0, 1.0, double.NaN, 1e-12))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsFinite / IsNonNegativeFinite / IsProbability / IsPercentage -----

    [Test]
    [Arguments(0.0, true)]
    [Arguments(1.0, true)]
    [Arguments(-1e308, true)]
    [Arguments(double.NaN, false)]
    [Arguments(double.PositiveInfinity, false)]
    [Arguments(double.NegativeInfinity, false)]
    public async Task IsFinite_Double(double v, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsFinite(v)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0.0f, true)]
    [Arguments(1.0f, true)]
    [Arguments(float.NaN, false)]
    [Arguments(float.PositiveInfinity, false)]
    [Arguments(float.NegativeInfinity, false)]
    public async Task IsFinite_Float(float v, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsFinite(v)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0.0, true)]
    [Arguments(1.0, true)]
    [Arguments(1e308, true)]
    [Arguments(-1e-300, false)]
    [Arguments(double.NaN, false)]
    [Arguments(double.PositiveInfinity, false)]
    public async Task IsNonNegativeFinite(double v, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsNonNegativeFinite(v)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0.0, true)]
    [Arguments(0.5, true)]
    [Arguments(1.0, true)]
    [Arguments(-1e-12, false)]
    [Arguments(1.0 + 1e-12, false)]
    [Arguments(double.NaN, false)]
    [Arguments(double.PositiveInfinity, false)]
    public async Task IsProbability(double v, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsProbability(v)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0.0, true)]
    [Arguments(50.0, true)]
    [Arguments(100.0, true)]
    [Arguments(-1e-12, false)]
    [Arguments(100.0 + 1e-9, false)]
    [Arguments(double.NaN, false)]
    public async Task IsPercentage(double v, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsPercentage(v)).IsEqualTo(expected);
    }

    // ----- HasRoundtripIdentity -----

    [Test]
    public async Task HasRoundtripIdentity_SinAsin_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // sin(asin(x)) ≡ x for x in [-1, 1].
        await Assert.That(MathTolerance.HasRoundtripIdentity(0.5, Math.Asin, Math.Sin, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.HasRoundtripIdentity(-0.7, Math.Asin, Math.Sin, 1e-12)).IsTrue();
    }

    [Test]
    public async Task HasRoundtripIdentity_LogExp_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // exp(log(x)) ≡ x for x > 0.
        await Assert.That(MathTolerance.HasRoundtripIdentity(2.5, Math.Log, Math.Exp, 1e-12)).IsTrue();
        await Assert.That(MathTolerance.HasRoundtripIdentity(1234.5, Math.Log, Math.Exp, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasRoundtripIdentity_LossyTransform_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Floor is not invertible: floor(2.7) = 2; identity(2) = 2; gap to original is 0.7.
        await Assert.That(MathTolerance.HasRoundtripIdentity(2.7, Math.Floor, x => x, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasRoundtripIdentity_NullForward_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.HasRoundtripIdentity(1.0, null!, x => x, 1e-9))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task HasRoundtripIdentity_NullInverse_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.HasRoundtripIdentity(1.0, x => x, null!, 1e-9))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task HasRoundtripIdentity_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.HasRoundtripIdentity(1.0, x => x, x => x, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasRoundtripIdentity_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.HasRoundtripIdentity(1.0, x => x, x => x, -1e-9))
            .Throws<ArgumentOutOfRangeException>();
    }
}
