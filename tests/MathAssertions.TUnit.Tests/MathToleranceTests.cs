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
}
