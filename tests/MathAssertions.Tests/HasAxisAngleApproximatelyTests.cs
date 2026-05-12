using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.Tests;

/// <summary>
/// Pins <see cref="MathTolerance.HasAxisAngleApproximately"/> across the documented edge cases:
/// identity rotation, canonical-axis rotations, the SO(3) double-cover sign flip, non-unit
/// inputs, and argument validation.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class HasAxisAngleApproximatelyTests
{
    [Test]
    public async Task IdentityQuaternion_MatchesAnyAxisWithZeroAngle(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            Quaternion.Identity, Vector3.UnitX, 0.0, tolerance: 1e-6)).IsTrue();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            Quaternion.Identity, Vector3.UnitY, 0.0, tolerance: 1e-6)).IsTrue();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            Quaternion.Identity, new Vector3(1, 1, 1), 0.0, tolerance: 1e-6)).IsTrue();
    }

    [Test]
    public async Task IdentityQuaternion_NonZeroExpectedAngle_Fails(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            Quaternion.Identity, Vector3.UnitX, 90.0, tolerance: 1e-6)).IsFalse();
    }

    [Test]
    public async Task NinetyDegreesAroundX_LiteralCase_Matches(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2.0));
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            q, Vector3.UnitX, 90.0, tolerance: 1e-4)).IsTrue();
    }

    [Test]
    public async Task OneEightyAroundArbitraryAxis_CoversWZeroBoundary(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Vector3 axis = Vector3.Normalize(new Vector3(1, 2, 3));
        Quaternion q = Quaternion.CreateFromAxisAngle(axis, (float)Math.PI);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            q, axis, 180.0, tolerance: 1e-4)).IsTrue();
    }

    [Test]
    public async Task NegatedQuaternion_StillMatchesSameAxisAngle(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 3.0));
        Quaternion negated = new(-q.X, -q.Y, -q.Z, -q.W);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            q, Vector3.UnitY, 60.0, tolerance: 1e-4)).IsTrue();
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            negated, Vector3.UnitY, 60.0, tolerance: 1e-4)).IsTrue();
    }

    [Test]
    public async Task WrongAxis_Fails(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2.0));
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            q, Vector3.UnitY, 90.0, tolerance: 1e-4)).IsFalse();
    }

    [Test]
    public async Task WrongAngle_Fails(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2.0));
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            q, Vector3.UnitX, 45.0, tolerance: 1e-4)).IsFalse();
    }

    [Test]
    public async Task NonUnitInput_NormalizedInternally_Matches(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion unit = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 4.0));
        Quaternion scaled = new(unit.X * 5, unit.Y * 5, unit.Z * 5, unit.W * 5);
        await Assert.That(MathTolerance.HasAxisAngleApproximately(
            scaled, Vector3.UnitZ, 45.0, tolerance: 1e-4)).IsTrue();
    }

    [Test]
    public void ZeroLengthExpectedAxis_Throws(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Assert.Throws<ArgumentException>(() =>
            MathTolerance.HasAxisAngleApproximately(Quaternion.Identity, Vector3.Zero, 0.0, tolerance: 1e-6));
    }

    [Test]
    public void NegativeTolerance_Throws(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MathTolerance.HasAxisAngleApproximately(Quaternion.Identity, Vector3.UnitX, 0.0, tolerance: -1.0));
    }

    [Test]
    public void NaNTolerance_Throws(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MathTolerance.HasAxisAngleApproximately(Quaternion.Identity, Vector3.UnitX, 0.0, tolerance: double.NaN));
    }
}
