using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.TUnit;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// End-to-end tests for the <c>[GenerateAssertion]</c>-emitted Vector3 fluent extension.
/// One happy path, one failing case, plus a tight-tolerance precision check that fails
/// if the source generator or wrapper accidentally narrows the tolerance to float.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class AdapterAssertionsTests
{
    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_Happy(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);
        await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
    }

    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1, 2, 99);
        await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
        }).Throws<AssertionException>();
    }

    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_TightDoubleTolerance_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1.0f, 1.0f, 1.0f);
        await Assert.That(a).IsApproximatelyEqualTo(a, tolerance: 1e-9);
    }

    /// <summary>End-to-end happy path for the fluent <c>HasAxisAngleApproximately</c> chain.
    /// Verifies the source generator wires the new (v0.2.0) extension onto
    /// <c>IAssertionSource&lt;Quaternion&gt;</c> and the predicate fires green for a literal
    /// axis-angle construction.</summary>
    [Test]
    public async Task Quaternion_HasAxisAngleApproximately_Happy(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(System.Math.PI / 2.0));
        await Assert.That(q).HasAxisAngleApproximately(Vector3.UnitY, expectedAngleDegrees: 90.0, tolerance: 1e-4);
    }

    /// <summary>End-to-end fail path for <c>HasAxisAngleApproximately</c>: the wrong-axis case
    /// must throw <see cref="AssertionException"/>, and the failure message must surface the
    /// extracted axis / angle that the rich renderer (<c>MathFailureMessage.AxisAngleApproximately</c>)
    /// produces, so consumers see what the assertion saw.</summary>
    [Test]
    public async Task Quaternion_HasAxisAngleApproximately_WrongAxis_FailsWithExtractedDiagnostics(CancellationToken ct)
    {
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(System.Math.PI / 2.0));
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(q).HasAxisAngleApproximately(Vector3.UnitY, expectedAngleDegrees: 90.0, tolerance: 1e-4);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("extracted axis:");
        await Assert.That(ex.Message).Contains("extracted angle:");
        await Assert.That(ex.Message).Contains("delta angle:");
    }

    /// <summary>Pins the SO(3) double-cover handling reaches the fluent chain: a negated
    /// quaternion encodes the same physical rotation, so <c>HasAxisAngleApproximately</c>
    /// returns true for both representations.</summary>
    [Test]
    public async Task Quaternion_HasAxisAngleApproximately_NegatedQuaternion_StillMatches(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(System.Math.PI / 3.0));
        var negated = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);
        await Assert.That(negated).HasAxisAngleApproximately(Vector3.UnitY, expectedAngleDegrees: 60.0, tolerance: 1e-4);
    }
}
