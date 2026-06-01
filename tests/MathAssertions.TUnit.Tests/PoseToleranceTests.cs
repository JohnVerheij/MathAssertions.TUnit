using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Direct coverage and correctness tests for the pose primitives on <see cref="MathTolerance"/>:
/// the geodesic rotation angle, the Euclidean position distance, and the combined pose predicate
/// with its independent position / rotation tolerances. Runs in the adapter test project, which
/// the CI coverage gate instruments, so the core <c>MathAssertions.dll</c> lines are measured.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class PoseToleranceTests
{
    [Test]
    public async Task RotationAngleDegrees_Identity_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var angle = MathTolerance.RotationAngleDegrees(Quaternion.Identity, Quaternion.Identity);
        await Assert.That(angle).IsEqualTo(0.0).Within(1e-6);
    }

    [Test]
    public async Task RotationAngleDegrees_NinetyAboutY_IsNinety(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 2.0));
        var angle = MathTolerance.RotationAngleDegrees(Quaternion.Identity, q);
        await Assert.That(angle).IsEqualTo(90.0).Within(1e-2);
    }

    [Test]
    public async Task RotationAngleDegrees_DoubleCover_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1.0f);
        var negQ = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);
        var angle = MathTolerance.RotationAngleDegrees(q, negQ);
        await Assert.That(angle).IsEqualTo(0.0).Within(1e-3);
    }

    [Test]
    public async Task RotationAngleDegrees_OneEightyAboutX_IsOneEighty(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI);
        var angle = MathTolerance.RotationAngleDegrees(Quaternion.Identity, q);
        await Assert.That(angle).IsEqualTo(180.0).Within(1e-2);
    }

    [Test]
    public async Task PositionDistance_3_4_0_IsFive(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var d = MathTolerance.PositionDistance(Vector3.Zero, new Vector3(3, 4, 0));
        await Assert.That(d).IsEqualTo(5.0).Within(1e-4);
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_Close_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.5f);
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            new Vector3(1, 2, 3), rot,
            new Vector3(1.0005f, 2f, 3f), rot,
            positionTolerance: 1e-3, rotationToleranceDegrees: 0.5);
        await Assert.That(ok).IsTrue();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_PositionTooFar_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            new Vector3(1, 2, 3), Quaternion.Identity,
            new Vector3(1.5f, 2f, 3f), Quaternion.Identity,
            positionTolerance: 1e-3, rotationToleranceDegrees: 0.5);
        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_RotationTooFar_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 4.0));
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            Vector3.Zero, Quaternion.Identity,
            Vector3.Zero, rot,
            positionTolerance: 1e-3, rotationToleranceDegrees: 0.5);
        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_NegativePositionTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsPoseApproximatelyEqual(
                Vector3.Zero, Quaternion.Identity, Vector3.Zero, Quaternion.Identity,
                positionTolerance: -1.0, rotationToleranceDegrees: 0.5))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_NegativeRotationTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => MathTolerance.IsPoseApproximatelyEqual(
                Vector3.Zero, Quaternion.Identity, Vector3.Zero, Quaternion.Identity,
                positionTolerance: 1e-3, rotationToleranceDegrees: -0.5))
            .Throws<ArgumentOutOfRangeException>();
    }
}
