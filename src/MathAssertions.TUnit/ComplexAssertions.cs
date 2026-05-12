using System.Numerics;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Complex"/> assertions delegating to <see cref="MathTolerance"/>.
/// </summary>
/// <remarks>
/// v0.2.0 enriches the failure message with Real / Imaginary delta rendering via
/// <see cref="MathFailureMessage"/>.
/// </remarks>
public static class ComplexAssertions
{
    /// <summary>Component-wise tolerance comparison across real and imaginary parts.</summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsApproximatelyEqualTo(this Complex value, Complex expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));
}
