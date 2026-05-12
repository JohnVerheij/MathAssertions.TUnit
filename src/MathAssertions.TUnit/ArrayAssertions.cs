using System;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="double"/>[] / <see cref="float"/>[] array assertions delegating to
/// the <see cref="MathTolerance"/> span overloads. Arrays are used at the
/// adapter surface rather than spans because TUnit's assertion-builder infrastructure
/// cannot capture ref-struct values across an <c>await</c> boundary;
/// <see cref="ReadOnlySpan{T}"/> remains accessible to callers via
/// <c>MathTolerance.IsApproximatelyEqual</c> directly when zero-allocation matters.
/// </summary>
/// <remarks>
/// Implements the plan's M-4 fix: array overloads validate non-null at entry rather than
/// silently treating <see langword="null"/> as an empty span. The cast to
/// <see cref="ReadOnlySpan{T}"/> happens inside the wrapper after validation.
/// v0.2.0 enriches the failure message with the first failing index and the per-element
/// delta via <see cref="MathFailureMessage"/>.
/// </remarks>
public static class ArrayAssertions
{
    /// <summary>Element-wise tolerance comparison for two <see cref="double"/> arrays.</summary>
    [GenerateAssertion(InlineMethodBody = false)]
    public static AssertionResult IsApproximatelyEqualTo(this double[] value, double[] expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(expected);
        return MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));
    }

    /// <summary>Element-wise tolerance comparison for two <see cref="float"/> arrays.</summary>
    [GenerateAssertion(InlineMethodBody = false)]
    public static AssertionResult IsApproximatelyEqualTo(this float[] value, float[] expected, float tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(expected);
        return MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));
    }
}
