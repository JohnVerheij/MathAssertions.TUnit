using System;
using System.Numerics;
using System.Numerics.Tensors;
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
    // Tensor shape literals lifted to static readonly fields per CA1861. The
    // ReadOnlyTensorSpan constructor accepts ReadOnlySpan<nint> and the implicit
    // conversion from nint[] is what each call site relies on.
    private static readonly nint[] Shape1 = [(nint)1];
    private static readonly nint[] Shape3 = [(nint)3];
    private static readonly nint[] Shape4 = [(nint)4];
    private static readonly nint[] Shape2x2 = [(nint)2, (nint)2];
    private static readonly nint[] Shape2x3 = [(nint)2, (nint)3];

    // Single-element backing arrays for the throws-tests, lifted to static readonly per CA1861.
    private static readonly double[] OneDouble = [1.0];

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

    // ===== Cluster 2 — System.Numerics compounds =====

    // ----- Vector2 -----

    [Test]
    public async Task IsApproximatelyEqual_Vector2_AllComponentsWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1, 2);
        var b = new Vector2(1.0001f, 2.0001f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector2_FirstComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1, 2);
        var b = new Vector2(99, 2);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector2_LastComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1, 2);
        var b = new Vector2(1, 99);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector2_NaNComponent_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1, 2);
        var b = new Vector2(float.NaN, 2);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector2_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Vector2.Zero, Vector2.Zero, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Vector4 -----

    [Test]
    public async Task IsApproximatelyEqual_Vector4_AllComponentsWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector4(1, 2, 3, 4);
        var b = new Vector4(1.0001f, 2.0001f, 3.0001f, 4.0001f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector4_LastComponentOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector4(1, 2, 3, 4);
        var b = new Vector4(1, 2, 3, 99);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector4_InfinityComponent_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector4(1, 2, 3, 4);
        var b = new Vector4(1, 2, 3, float.PositiveInfinity);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector4_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Vector4.Zero, Vector4.Zero, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Quaternion (component-wise) -----

    [Test]
    public async Task IsApproximatelyEqual_Quaternion_AllComponentsWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var b = new Quaternion(0.1001f, 0.2001f, 0.3001f, 0.9001f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Quaternion_DistinguishesQFromMinusQ(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // q and -q encode the same rotation but are component-wise NOT equal — that's what
        // IsRotationallyEquivalent is for. Pin the distinction.
        var a = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var negA = new Quaternion(-0.1f, -0.2f, -0.3f, -0.9f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, negA, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Quaternion_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Quaternion.Identity, Quaternion.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsRotationallyEquivalent (M-2 fix) -----

    [Test]
    public async Task IsRotationallyEquivalent_QAndMinusQ_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // q and -q represent the same rotation under SO(3) double-cover.
        var q = Quaternion.Normalize(new Quaternion(0.1f, 0.2f, 0.3f, 0.9f));
        var negQ = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(q, negQ, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsRotationallyEquivalent_NonUnitQuaternion_StillCorrect(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // M-2 fix: scaling either input shouldn't change the verdict because the impl
        // normalizes before computing the dot product.
        var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var scaled = new Quaternion(q.X * 2, q.Y * 2, q.Z * 2, q.W * 2);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(q, scaled, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsRotationallyEquivalent_DifferentMagnitudes_SameRotation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // M-2 fix: q and 0.5*q encode the same rotation; impl normalizes first.
        var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var halfQ = new Quaternion(q.X * 0.5f, q.Y * 0.5f, q.Z * 0.5f, q.W * 0.5f);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(q, halfQ, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsRotationallyEquivalent_DifferentRotations_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ninetyAroundX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
        var ninetyAroundY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2);
        await Assert.That(MathTolerance.IsRotationallyEquivalent(ninetyAroundX, ninetyAroundY, 1e-3)).IsFalse();
    }

    [Test]
    public async Task IsRotationallyEquivalent_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsRotationallyEquivalent(Quaternion.Identity, Quaternion.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Matrix4x4 -----

    [Test]
    public async Task IsApproximatelyEqual_Matrix4x4_IdentityToItself_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(Matrix4x4.Identity, Matrix4x4.Identity, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Matrix4x4_FirstElementOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = Matrix4x4.Identity;
        var b = a;
        b.M11 = 99f;
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Matrix4x4_LastElementOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = Matrix4x4.Identity;
        var b = a;
        b.M44 = 99f;
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Matrix4x4_RotationToItself_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rotation = Matrix4x4.CreateRotationY(MathF.PI / 4);
        await Assert.That(MathTolerance.IsApproximatelyEqual(rotation, rotation, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Matrix4x4_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Matrix4x4.Identity, Matrix4x4.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Plane (component-wise + M-3 geometric equivalence) -----

    [Test]
    public async Task IsApproximatelyEqual_Plane_SameRepresentation_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(0, 1, 0, -5);
        var b = new Plane(0.0001f, 1.0001f, 0.0001f, -5.0001f);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Plane_FlippedRepresentation_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // M-3 fix: component-wise IsApproximatelyEqual must NOT recognize the flipped
        // representation as equal; that's what IsGeometricallyEquivalent is for.
        var a = new Plane(0, 1, 0, -5);
        var flipped = new Plane(-a.Normal, -a.D);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, flipped, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Plane_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(0, 1, 0, -5);
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(a, a, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsGeometricallyEquivalent_Plane_SameRepresentation_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(0, 1, 0, -5);
        var b = new Plane(0.0001f, 1.0001f, 0.0001f, -5.0001f);
        await Assert.That(MathTolerance.IsGeometricallyEquivalent(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsGeometricallyEquivalent_Plane_FlippedRepresentation_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // M-3 fix: (n, d) and (-n, -d) describe the same plane.
        var a = new Plane(0, 1, 0, -5);
        var flipped = new Plane(-a.Normal, -a.D);
        await Assert.That(MathTolerance.IsGeometricallyEquivalent(a, flipped, 1e-6)).IsTrue();
    }

    [Test]
    public async Task IsGeometricallyEquivalent_Plane_DifferentPlanes_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(0, 1, 0, -5);
        var b = new Plane(1, 0, 0, -5);
        await Assert.That(MathTolerance.IsGeometricallyEquivalent(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsGeometricallyEquivalent_Plane_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(0, 1, 0, -5);
        await Assert.That(() => MathTolerance.IsGeometricallyEquivalent(a, a, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Complex -----

    [Test]
    public async Task IsApproximatelyEqual_Complex_BothPartsWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Complex(1.0, 2.0);
        var b = new Complex(1.0001, 2.0001);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Complex_RealOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Complex(1.0, 2.0);
        var b = new Complex(99.0, 2.0);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Complex_ImaginaryOutsideTolerance_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Complex(1.0, 2.0);
        var b = new Complex(1.0, 99.0);
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Complex_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Complex.One, Complex.One, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- ReadOnlySpan<double> -----

    [Test]
    public async Task IsApproximatelyEqual_SpanDouble_BothEmpty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.IsApproximatelyEqual(
            ReadOnlySpan<double>.Empty, ReadOnlySpan<double>.Empty, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanDouble_AllElementsMatch_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> a = [1.0, 2.0, 3.0];
        ReadOnlySpan<double> b = [1.0001, 2.0001, 3.0001];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanDouble_LengthMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> a = [1.0, 2.0, 3.0];
        ReadOnlySpan<double> b = [1.0, 2.0];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanDouble_OneElementMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> a = [1.0, 2.0, 3.0];
        ReadOnlySpan<double> b = [1.0, 99.0, 3.0];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanDouble_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // CT-aware lambda style isn't an option for span-typed arguments; allocate inside.
        await Assert.That(() =>
        {
            ReadOnlySpan<double> a = [1.0];
            ReadOnlySpan<double> b = [1.0];
            return MathTolerance.IsApproximatelyEqual(a, b, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- ReadOnlySpan<float> -----

    [Test]
    public async Task IsApproximatelyEqual_SpanFloat_AllElementsMatch_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<float> a = [1.0f, 2.0f, 3.0f];
        ReadOnlySpan<float> b = [1.0001f, 2.0001f, 3.0001f];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001f)).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanFloat_LengthMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<float> a = [1.0f, 2.0f];
        ReadOnlySpan<float> b = [1.0f, 2.0f, 3.0f];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanFloat_OneElementMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<float> a = [1.0f, 2.0f, 3.0f];
        ReadOnlySpan<float> b = [1.0f, 99.0f, 3.0f];
        await Assert.That(MathTolerance.IsApproximatelyEqual(a, b, 0.001f)).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_SpanFloat_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<float> a = [1.0f];
            ReadOnlySpan<float> b = [1.0f];
            return MathTolerance.IsApproximatelyEqual(a, b, -1e-6f);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- ReadOnlyTensorSpan<T> -----

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_HappyPath_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var b = new double[] { 1.0001, 2.0001, 3.0001, 4.0001 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(a, Shape2x2, default),
            new ReadOnlyTensorSpan<double>(b, Shape2x2, default),
            0.001);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_ElementMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var b = new double[] { 1.0, 99.0, 3.0, 4.0 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(a, Shape2x2, default),
            new ReadOnlyTensorSpan<double>(b, Shape2x2, default),
            0.001);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_RankMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var data = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(data, Shape4, default),
            new ReadOnlyTensorSpan<double>(data, Shape2x2, default),
            0.001);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_PerDimensionLengthMismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var four = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var six = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(four, Shape2x2, default),
            new ReadOnlyTensorSpan<double>(six, Shape2x3, default),
            0.001);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_BothNaNAtSamePosition_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new double[] { 1.0, double.NaN, 3.0 };
        var b = new double[] { 1.0, double.NaN, 3.0 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(a, Shape3, default),
            new ReadOnlyTensorSpan<double>(b, Shape3, default),
            0.001);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_OneNaN_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new double[] { 1.0, double.NaN, 3.0 };
        var b = new double[] { 1.0, 2.0, 3.0 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<double>(a, Shape3, default),
            new ReadOnlyTensorSpan<double>(b, Shape3, default),
            0.001);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorInt_WithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new int[] { 1, 2, 3, 4 };
        var b = new int[] { 1, 2, 4, 4 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(a, Shape2x2, default),
            new ReadOnlyTensorSpan<int>(b, Shape2x2, default),
            1);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorInt_ZeroToleranceRejectsOffByOne_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new int[] { 1, 2, 3, 4 };
        var b = new int[] { 1, 2, 4, 4 };
        var result = MathTolerance.IsApproximatelyEqual(
            new ReadOnlyTensorSpan<int>(a, Shape2x2, default),
            new ReadOnlyTensorSpan<int>(b, Shape2x2, default),
            0);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(
                new ReadOnlyTensorSpan<double>(OneDouble, Shape1, default),
                new ReadOnlyTensorSpan<double>(OneDouble, Shape1, default),
                double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(
                new ReadOnlyTensorSpan<double>(OneDouble, Shape1, default),
                new ReadOnlyTensorSpan<double>(OneDouble, Shape1, default),
                -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }
}
