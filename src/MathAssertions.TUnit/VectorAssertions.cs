using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="System.Numerics"/> vector assertions delegating to
/// <see cref="MathAssertions.MathTolerance"/>. Component values widen to
/// <see cref="double"/> internally so a tight tolerance is honored at full precision.
/// </summary>
public static class VectorAssertions
{
    /// <summary>Component-wise tolerance comparison for <see cref="Vector2"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Vector2 value, Vector2 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>Component-wise tolerance comparison for <see cref="Vector3"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Vector3 value, Vector3 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>Component-wise tolerance comparison for <see cref="Vector4"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Vector4 value, Vector4 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>
    /// Asserts the vector's magnitude is within <paramref name="tolerance"/> of
    /// <paramref name="expected"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to have magnitude approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasMagnitudeApproximately(this Vector3 value, double expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual((double)value.Length(), expected, tolerance);

    /// <summary>
    /// Asserts the vector is a unit vector (magnitude approximately <c>1</c>) within
    /// <paramref name="tolerance"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a unit (normalized) vector within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsNormalized(this Vector3 value, double tolerance)
        => MathTolerance.IsApproximatelyEqual((double)value.Length(), 1.0, tolerance);
}
