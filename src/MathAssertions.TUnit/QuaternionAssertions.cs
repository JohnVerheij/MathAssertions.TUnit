using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Quaternion"/> assertions. Distinguishes component-wise equality
/// (treats <c>q</c> and <c>-q</c> as different) from rotational equivalence (treats them
/// as the same rotation, the SO(3) double-cover view).
/// </summary>
public static class QuaternionAssertions
{
    /// <summary>Component-wise tolerance comparison.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Quaternion value, Quaternion expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>
    /// Asserts the quaternion encodes the same rotation as <paramref name="expected"/>
    /// within tolerance, treating <c>q</c> and <c>-q</c> as equivalent.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be rotationally equivalent to {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsRotationallyEquivalentTo(this Quaternion value, Quaternion expected, double tolerance)
        => MathTolerance.IsRotationallyEquivalent(value, expected, tolerance);

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
        => MathTolerance.IsApproximatelyEqual((double)value.Length(), 1.0, tolerance);
}
