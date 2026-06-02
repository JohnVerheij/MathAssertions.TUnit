using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.TUnit;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// End-to-end tests for the <c>[GenerateAssertion]</c>-emitted pose / rigid-transform fluent
/// extensions. Covers the tuple and <see cref="Matrix4x4"/> forms on the happy path and the two
/// failure halves (position-only and rotation-only), asserting the combined diagnostic names which
/// half exceeded.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class PoseAssertionsTests
{
    [Test]
    public async Task Pose_Close_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.5f);
        await Assert.That((new Vector3(1, 2, 3), rot))
            .IsPoseApproximatelyEqualTo(
                (new Vector3(1.0005f, 2f, 3f), rot),
                positionTolerance: 1e-3,
                rotationToleranceDegrees: 0.5);
    }

    [Test]
    public async Task Pose_PositionFails_MessageNamesPositionExceededRotationOk(CancellationToken ct)
    {
        var ex = await Assert.That(async () =>
        {
            await Assert.That((new Vector3(1, 2, 3), Quaternion.Identity))
                .IsPoseApproximatelyEqualTo(
                    (new Vector3(1.5f, 2f, 3f), Quaternion.Identity),
                    positionTolerance: 1e-3,
                    rotationToleranceDegrees: 0.5);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("position: delta=");
        await Assert.That(ex.Message).Contains("EXCEEDED");
        await Assert.That(ex.Message).Contains("rotation: delta=");
    }

    [Test]
    public async Task Pose_RotationFails_MessageNamesRotationExceeded(CancellationToken ct)
    {
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 4.0));
        var ex = await Assert.That(async () =>
        {
            await Assert.That((Vector3.Zero, Quaternion.Identity))
                .IsPoseApproximatelyEqualTo(
                    (Vector3.Zero, rot),
                    positionTolerance: 1e-3,
                    rotationToleranceDegrees: 0.5);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("rotation: delta=");
        await Assert.That(ex.Message).Contains("EXCEEDED");
    }

    [Test]
    public async Task RigidTransform_Equal_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.3f);
        var m = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(new Vector3(1, 2, 3));
        await Assert.That(m).IsRigidTransformApproximatelyEqualTo(
            m, positionTolerance: 1e-4, rotationToleranceDegrees: 0.1);
    }

    [Test]
    public async Task RigidTransform_DifferentTranslation_Fails(CancellationToken ct)
    {
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.3f);
        var m1 = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(new Vector3(1, 2, 3));
        var m2 = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(new Vector3(1, 2, 9));
        await Assert.That(async () =>
        {
            await Assert.That(m1).IsRigidTransformApproximatelyEqualTo(
                m2, positionTolerance: 1e-4, rotationToleranceDegrees: 0.1);
        }).Throws<AssertionException>();
    }
}
