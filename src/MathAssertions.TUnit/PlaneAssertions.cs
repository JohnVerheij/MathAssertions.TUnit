using System.Numerics;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Plane"/> assertions. Distinguishes component-wise equality (treats
/// <c>(n, d)</c> and <c>(-n, -d)</c> as different) from geometric equivalence (treats
/// them as the same plane).
/// </summary>
/// <remarks>
/// v0.2.0 enriches the failure messages with per-component delta rendering via
/// <see cref="MathFailureMessage"/>. The geometrically-equivalent variant surfaces the
/// deltas against both the direct and the sign-flipped representation.
/// </remarks>
public static class PlaneAssertions
{
    /// <summary>Component-wise tolerance comparison across <c>Normal</c> and <c>D</c>.</summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsApproximatelyEqualTo(this Plane value, Plane expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));

    /// <summary>
    /// Asserts the two planes describe the same set of points, allowing the
    /// <c>(n, d) = (-n, -d)</c> sign-flip equivalence.
    /// </summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsGeometricallyEquivalentTo(this Plane value, Plane expected, double tolerance)
        => MathTolerance.IsGeometricallyEquivalent(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.GeometricallyEquivalent(value, expected, tolerance));
}
