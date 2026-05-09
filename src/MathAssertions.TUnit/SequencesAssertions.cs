using System;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="MathAssertions.Sequences"/> assertions over <see cref="double"/>
/// arrays plus the generic length predicates over any <c>T[]</c>. Arrays are used at
/// the adapter surface for the same ref-struct reason documented on
/// <see cref="ArrayAssertions"/>; null arrays are rejected at entry.
/// </summary>
public static class SequencesAssertions
{
    /// <summary>Asserts the sequence is non-decreasing.</summary>
    [GenerateAssertion(ExpectationMessage = "to be monotonically increasing", InlineMethodBody = false)]
    public static bool IsMonotonicallyIncreasing(this double[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsMonotonicallyIncreasing((ReadOnlySpan<double>)value);
    }

    /// <summary>Asserts the sequence is non-increasing.</summary>
    [GenerateAssertion(ExpectationMessage = "to be monotonically decreasing", InlineMethodBody = false)]
    public static bool IsMonotonicallyDecreasing(this double[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsMonotonicallyDecreasing((ReadOnlySpan<double>)value);
    }

    /// <summary>Asserts the sequence is strictly increasing (adjacent equal values fail).</summary>
    [GenerateAssertion(ExpectationMessage = "to be strictly monotonically increasing", InlineMethodBody = false)]
    public static bool IsStrictlyMonotonicallyIncreasing(this double[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsStrictlyMonotonicallyIncreasing((ReadOnlySpan<double>)value);
    }

    /// <summary>Asserts the sequence is strictly decreasing.</summary>
    [GenerateAssertion(ExpectationMessage = "to be strictly monotonically decreasing", InlineMethodBody = false)]
    public static bool IsStrictlyMonotonicallyDecreasing(this double[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsStrictlyMonotonicallyDecreasing((ReadOnlySpan<double>)value);
    }

    /// <summary>Asserts the sequence is sorted (alias for <see cref="IsMonotonicallyIncreasing"/>).</summary>
    [GenerateAssertion(ExpectationMessage = "to be sorted ascending", InlineMethodBody = false)]
    public static bool IsSorted(this double[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsSorted((ReadOnlySpan<double>)value);
    }

    /// <summary>Asserts every value is in the closed interval <c>[min, max]</c>.</summary>
    [GenerateAssertion(ExpectationMessage = "to be bounded by [{min}, {max}]", InlineMethodBody = false)]
    public static bool IsBounded(this double[] value, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsBounded((ReadOnlySpan<double>)value, min, max);
    }

    /// <summary>Asserts adjacent differences are equal within tolerance (arithmetic progression).</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be an arithmetic progression within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsArithmeticProgression(this double[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsArithmeticProgression((ReadOnlySpan<double>)value, tolerance);
    }

    /// <summary>Asserts adjacent ratios are equal within tolerance (geometric progression).</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a geometric progression within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsGeometricProgression(this double[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsGeometricProgression((ReadOnlySpan<double>)value, tolerance);
    }

    /// <summary>Asserts the last value of the sequence is within tolerance of the limit.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to converge to {limit} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool ConvergesTo(this double[] value, double limit, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.ConvergesTo((ReadOnlySpan<double>)value, limit, tolerance);
    }

    /// <summary>Single-step Cauchy criterion: last two values within tolerance of each other.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be Cauchy convergent within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsCauchyConvergent(this double[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.IsCauchyConvergent((ReadOnlySpan<double>)value, tolerance);
    }

    /// <summary>Asserts the array has exactly <paramref name="expected"/> elements.</summary>
    [GenerateAssertion(ExpectationMessage = "to have length {expected}", InlineMethodBody = false)]
    public static bool HasLength<T>(this T[] value, int expected)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.HasLength<T>((ReadOnlySpan<T>)value, expected);
    }

    /// <summary>Asserts the array has at least <paramref name="expected"/> elements.</summary>
    [GenerateAssertion(ExpectationMessage = "to have at least {expected} elements", InlineMethodBody = false)]
    public static bool HasMinLength<T>(this T[] value, int expected)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Sequences.HasMinLength<T>((ReadOnlySpan<T>)value, expected);
    }
}
