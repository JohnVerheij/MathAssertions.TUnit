using System;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="double"/>[] / <see cref="float"/>[] array assertions delegating to
/// the <see cref="MathAssertions.MathTolerance"/> span overloads. Arrays are used at the
/// adapter surface rather than spans because TUnit's assertion-builder infrastructure
/// cannot capture ref-struct values across an <c>await</c> boundary;
/// <see cref="ReadOnlySpan{T}"/> remains accessible to callers via
/// <c>MathTolerance.IsApproximatelyEqual</c> directly when zero-allocation matters.
/// </summary>
/// <remarks>
/// Implements the plan's M-4 fix: array overloads validate non-null at entry rather than
/// silently treating <see langword="null"/> as an empty span. The cast to
/// <see cref="ReadOnlySpan{T}"/> happens inside the wrapper after validation.
/// </remarks>
public static class ArrayAssertions
{
    /// <summary>Element-wise tolerance comparison for two <see cref="double"/> arrays.</summary>
    /// <param name="value">The array being asserted. Must not be <see langword="null"/>.</param>
    /// <param name="expected">Expected array. Must not be <see langword="null"/>.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element.</param>
    /// <returns><see langword="true"/> if both arrays have the same length and every
    /// element pair is approximately equal.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} element-wise within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsApproximatelyEqualTo(this double[] value, double[] expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(expected);
        return MathTolerance.IsApproximatelyEqual(
            (ReadOnlySpan<double>)value,
            (ReadOnlySpan<double>)expected,
            tolerance);
    }

    /// <summary>Element-wise tolerance comparison for two <see cref="float"/> arrays.</summary>
    /// <param name="value">The array being asserted. Must not be <see langword="null"/>.</param>
    /// <param name="expected">Expected array. Must not be <see langword="null"/>.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element.</param>
    /// <returns><see langword="true"/> if both arrays have the same length and every
    /// element pair is approximately equal.</returns>
    /// <exception cref="ArgumentNullException">Either argument is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} element-wise within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsApproximatelyEqualTo(this float[] value, float[] expected, float tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(expected);
        return MathTolerance.IsApproximatelyEqual(
            (ReadOnlySpan<float>)value,
            (ReadOnlySpan<float>)expected,
            tolerance);
    }
}
