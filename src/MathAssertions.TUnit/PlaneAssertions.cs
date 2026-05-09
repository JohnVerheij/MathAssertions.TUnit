using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Plane"/> assertions. Distinguishes component-wise equality (treats
/// <c>(n, d)</c> and <c>(-n, -d)</c> as different) from geometric equivalence (treats
/// them as the same plane).
/// </summary>
public static class PlaneAssertions
{
    /// <summary>Component-wise tolerance comparison across <c>Normal</c> and <c>D</c>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Plane value, Plane expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>
    /// Asserts the two planes describe the same set of points, allowing the
    /// <c>(n, d) = (-n, -d)</c> sign-flip equivalence.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be the same plane as {expected} (allowing sign-flip equivalence) within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsGeometricallyEquivalentTo(this Plane value, Plane expected, double tolerance)
        => MathTolerance.IsGeometricallyEquivalent(value, expected, tolerance);
}
