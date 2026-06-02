using System.Numerics;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent pose / rigid-transform assertions. A pose is a position and an orientation together;
/// these assertions compare the two halves with <em>separate</em> tolerances (position as a
/// Euclidean distance in the caller's length unit, rotation as a geodesic angle in degrees) and
/// report one combined diagnostic that names which half missed. A single shared tolerance would be
/// wrong because the two quantities carry different units.
/// </summary>
/// <remarks>
/// The orientation comparison is on the SO(3) rotation metric, so a quaternion and its negation
/// (<c>q</c> and <c>-q</c>) are treated as the same rotation. The <see cref="Matrix4x4"/> overload
/// decomposes each transform into its translation and rotation and compares those; scale is not
/// part of a rigid transform and is ignored.
/// </remarks>
public static class PoseAssertions
{
    /// <summary>
    /// Asserts that a <c>(Position, Orientation)</c> pose matches <paramref name="expected"/> within
    /// a Euclidean <paramref name="positionTolerance"/> and a geodesic
    /// <paramref name="rotationToleranceDegrees"/>. On failure the message renders both poses and the
    /// measured position and rotation deltas, flagging which half exceeded its tolerance.
    /// </summary>
    /// <param name="value">The actual pose, as a <c>(Vector3 Position, Quaternion Orientation)</c> tuple.</param>
    /// <param name="expected">The expected pose.</param>
    /// <param name="positionTolerance">Maximum allowed Euclidean distance between the positions. Non-negative, not NaN.</param>
    /// <param name="rotationToleranceDegrees">Maximum allowed geodesic angle between the orientations, in degrees. Non-negative, not NaN.</param>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsPoseApproximatelyEqualTo(
        this (Vector3 Position, Quaternion Orientation) value,
        (Vector3 Position, Quaternion Orientation) expected,
        double positionTolerance,
        double rotationToleranceDegrees)
        => MathTolerance.IsPoseApproximatelyEqual(value.Position, value.Orientation, expected.Position, expected.Orientation, positionTolerance, rotationToleranceDegrees)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.Pose(
                value.Position, value.Orientation, expected.Position, expected.Orientation,
                positionTolerance, rotationToleranceDegrees,
                MathTolerance.PositionDistance(value.Position, expected.Position),
                MathTolerance.RotationAngleDegrees(value.Orientation, expected.Orientation)));

    /// <summary>
    /// Asserts that a rigid transform (a <see cref="Matrix4x4"/>) matches <paramref name="expected"/>
    /// in its translation and rotation within separate tolerances. The translation is read from
    /// <see cref="Matrix4x4.Translation"/> and the rotation via
    /// <see cref="Quaternion.CreateFromRotationMatrix(Matrix4x4)"/>; the overload assumes a rigid
    /// transform (orthonormal rotation, unit scale), which is the contract its name advertises.
    /// </summary>
    /// <param name="value">The actual rigid transform.</param>
    /// <param name="expected">The expected rigid transform.</param>
    /// <param name="positionTolerance">Maximum allowed Euclidean distance between the translations. Non-negative, not NaN.</param>
    /// <param name="rotationToleranceDegrees">Maximum allowed geodesic angle between the rotations, in degrees. Non-negative, not NaN.</param>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsRigidTransformApproximatelyEqualTo(
        this Matrix4x4 value,
        Matrix4x4 expected,
        double positionTolerance,
        double rotationToleranceDegrees)
    {
        Vector3 actualPosition = value.Translation;
        Quaternion actualRotation = Quaternion.CreateFromRotationMatrix(value);
        Vector3 expectedPosition = expected.Translation;
        Quaternion expectedRotation = Quaternion.CreateFromRotationMatrix(expected);
        return MathTolerance.IsPoseApproximatelyEqual(actualPosition, actualRotation, expectedPosition, expectedRotation, positionTolerance, rotationToleranceDegrees)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.Pose(
                actualPosition, actualRotation, expectedPosition, expectedRotation,
                positionTolerance, rotationToleranceDegrees,
                MathTolerance.PositionDistance(actualPosition, expectedPosition),
                MathTolerance.RotationAngleDegrees(actualRotation, expectedRotation)));
    }
}
