using System.Numerics;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Quaternion"/> assertions. Distinguishes component-wise equality
/// (treats <c>q</c> and <c>-q</c> as different) from rotational equivalence (treats them
/// as the same rotation, the SO(3) double-cover view).
/// </summary>
/// <remarks>
/// v0.2.0 enriches the failure messages for <see cref="IsApproximatelyEqualTo"/> and
/// <see cref="IsRotationallyEquivalentTo"/> with per-component delta rendering via
/// <see cref="MathFailureMessage"/>. v0.2.0 also adds <see cref="HasAxisAngleApproximately"/>
/// for axis-angle-form rotation assertions.
/// </remarks>
public static class QuaternionAssertions
{
    /// <summary>Component-wise tolerance comparison.</summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsApproximatelyEqualTo(this Quaternion value, Quaternion expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));

    /// <summary>
    /// Asserts the quaternion encodes the same rotation as <paramref name="expected"/>
    /// within tolerance, treating <c>q</c> and <c>-q</c> as equivalent.
    /// </summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsRotationallyEquivalentTo(this Quaternion value, Quaternion expected, double tolerance)
        => MathTolerance.IsRotationallyEquivalent(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.RotationallyEquivalent(value, expected, tolerance));

    /// <summary>
    /// Asserts the quaternion, viewed as a rotation in axis-angle form, lies within tolerance
    /// of the rotation <paramref name="expectedAngleDegrees"/> degrees around
    /// <paramref name="expectedAxis"/>. Handles the SO(3) double cover so a <c>q</c> / <c>-q</c>
    /// pair both compare equivalent to the same axis-angle pair. See
    /// <see cref="MathTolerance.HasAxisAngleApproximately"/> for the full contract.
    /// </summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult HasAxisAngleApproximately(this Quaternion value, Vector3 expectedAxis, double expectedAngleDegrees, double tolerance)
    {
        if (MathTolerance.HasAxisAngleApproximately(value, expectedAxis, expectedAngleDegrees, tolerance))
        {
            return AssertionResult.Passed;
        }
        MathTolerance.ExtractAxisAngle(value, expectedAxis, out Vector3 extractedAxis, out double extractedAngleDegrees, out Vector3 _, out bool _);
        return AssertionResult.Failed(MathFailureMessage.AxisAngleApproximately(value, expectedAxis, expectedAngleDegrees, extractedAxis, extractedAngleDegrees, tolerance));
    }

    /// <summary>Asserts the quaternion is the identity quaternion within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be the identity quaternion within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsIdentity(this Quaternion value, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, Quaternion.Identity, tolerance);

    /// <summary>Asserts the quaternion has unit length within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a unit quaternion within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsNormalized(this Quaternion value, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value.Length(), 1.0, tolerance);
}
