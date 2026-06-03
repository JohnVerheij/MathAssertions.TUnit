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
        var ex = await Assert.That(() => MathTolerance.IsPoseApproximatelyEqual(
                Vector3.Zero, Quaternion.Identity, Vector3.Zero, Quaternion.Identity,
                positionTolerance: -1.0, rotationToleranceDegrees: 0.5))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(ex!.ParamName).IsEqualTo("positionTolerance");
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_NegativeRotationTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ex = await Assert.That(() => MathTolerance.IsPoseApproximatelyEqual(
                Vector3.Zero, Quaternion.Identity, Vector3.Zero, Quaternion.Identity,
                positionTolerance: 1e-3, rotationToleranceDegrees: -0.5))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(ex!.ParamName).IsEqualTo("rotationToleranceDegrees");
    }

    // --- Regression: the stable atan2 metric must satisfy d(q, q) == 0 and scale invariance. ---

    [Test]
    public async Task RotationAngleDegrees_NonUnitQuaternionAgainstItself_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // |q| ~ 0.99985 (deliberately non-unit). The old acos(|dot|) metric amplified the residual
        // float normalization error into a spurious ~0.05 degree angle; the atan2 metric returns 0.
        var q = new Quaternion(0f, 0f, 0.707f, 0.707f);
        var angle = MathTolerance.RotationAngleDegrees(q, q);
        await Assert.That(angle).IsEqualTo(0.0).Within(1e-9);
    }

    [Test]
    public async Task RotationAngleDegrees_PositiveScalarMultiple_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // q and k*q (k > 0) are the same rotation. Normalization must make d(q, k*q) == 0.
        const float k = 1.01f;
        var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var scaled = new Quaternion(q.X * k, q.Y * k, q.Z * k, q.W * k);
        var angle = MathTolerance.RotationAngleDegrees(q, scaled);
        await Assert.That(angle).IsEqualTo(0.0).Within(1e-4);
    }

    [Test]
    public async Task RotationAngleDegrees_OneEightyAgainstItself_IsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stability near the 180-degree end of the range: a half-turn compared with itself is 0.
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI);
        var angle = MathTolerance.RotationAngleDegrees(q, q);
        await Assert.That(angle).IsEqualTo(0.0).Within(1e-4);
    }

    [Test]
    public async Task RotationAngleDegrees_TinyAngle_IsSmallAndPositive(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stability near 0 degrees: a sub-milliradian rotation resolves to a small positive angle,
        // not zero and not an amplified spike.
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1e-4f);
        var angle = MathTolerance.RotationAngleDegrees(Quaternion.Identity, q);
        await Assert.That(angle).IsGreaterThan(0.0);
        await Assert.That(angle).IsLessThan(0.01);
    }

    [Test]
    public async Task RotationAngleDegrees_ZeroQuaternion_IsNaN(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // default(Quaternion) = (0,0,0,0) is not normalizable: no geodesic angle is defined.
        var angle = MathTolerance.RotationAngleDegrees(default, default);
        await Assert.That(double.IsNaN(angle)).IsTrue();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_ZeroQuaternionRoundTrip_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // A default-pose round-trip uses (0,0,0,0) on both sides; the componentwise fallback passes.
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            Vector3.Zero, default,
            Vector3.Zero, default,
            positionTolerance: 1e-6, rotationToleranceDegrees: 1e-3);
        await Assert.That(ok).IsTrue();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_ZeroQuaternionVersusRealRotation_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // (0,0,0,0) compared with a real rotation fails on the componentwise fallback.
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            Vector3.Zero, default,
            Vector3.Zero, Quaternion.Identity,
            positionTolerance: 1e-6, rotationToleranceDegrees: 1e-3);
        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task IsPoseApproximatelyEqual_ZeroQuaternionVersusRealRotation_FailsEvenWithLargeAngularTolerance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // A degenerate (0,0,0,0) orientation must not be accepted as equal to a real rotation even
        // when the angular tolerance is large. The componentwise fallback uses a tight fixed component
        // tolerance, not the degrees value: reusing the degrees value would accept Identity here, since
        // the W-component delta of 1.0 is within a tolerance of 1.1.
        var ok = MathTolerance.IsPoseApproximatelyEqual(
            Vector3.Zero, default,
            Vector3.Zero, Quaternion.Identity,
            positionTolerance: 1e-6, rotationToleranceDegrees: 1.1);
        await Assert.That(ok).IsFalse();
    }
}
