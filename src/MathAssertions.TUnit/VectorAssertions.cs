using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Vector3"/> assertions for tolerance-based equality. Generated via
/// TUnit's <c>[GenerateAssertion]</c> source generator.
/// </summary>
public static class VectorAssertions
{
    /// <summary>
    /// Asserts that all three components of the vector are within
    /// <paramref name="tolerance"/> of the corresponding components of
    /// <paramref name="expected"/>. Component values widen to <see cref="double"/>
    /// before the per-axis comparison so a tight tolerance is honored at full precision.
    /// </summary>
    /// <param name="value">The vector being asserted.</param>
    /// <param name="expected">The expected vector.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the vectors match component-wise within tolerance.</returns>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} component-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Vector3 value, Vector3 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);
}
