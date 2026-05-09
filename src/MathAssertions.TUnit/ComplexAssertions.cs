using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Complex"/> assertions delegating to
/// <see cref="MathAssertions.MathTolerance"/>.
/// </summary>
public static class ComplexAssertions
{
    /// <summary>Component-wise tolerance comparison across real and imaginary parts.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Complex value, Complex expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);
}
