using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.Tests;

/// <summary>
/// Pins core <see cref="MathTolerance"/> behavior from a project that references ONLY the
/// MathAssertions core package, not the MathAssertions.TUnit adapter. Catches accidental
/// TUnit-coupling regressions in the core: if a refactor made <c>MathTolerance</c> depend
/// on TUnit types, this project would fail to compile.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class MathToleranceCoreTests
{
    [Test]
    public async Task IsApproximatelyEqual_Double_HappyPath(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = MathTolerance.IsApproximatelyEqual(1.0, 1.0001, 0.001);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_OutsideTolerance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = MathTolerance.IsApproximatelyEqual(1.0, 1.002, 0.001);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsApproximatelyEqual_Double_BothNaN_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = MathTolerance.IsApproximatelyEqual(double.NaN, double.NaN, 1e-6);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_HappyPath(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = MathTolerance.IsApproximatelyEqual(1.0f, 1.0001f, 0.001f);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_HappyPath(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);
        var result = MathTolerance.IsApproximatelyEqual(a, b, 0.001);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsApproximatelyEqual_Vector3_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsApproximatelyEqual(Vector3.Zero, Vector3.Zero, -1.0))
            .Throws<ArgumentOutOfRangeException>();
    }
}
