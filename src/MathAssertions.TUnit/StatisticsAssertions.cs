using System;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="MathAssertions.Statistics"/> assertions over <see cref="double"/>
/// arrays. Null arrays are rejected at entry.
/// </summary>
public static class StatisticsAssertions
{
    /// <summary>Asserts the sample mean is within tolerance of <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have mean approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasMeanApproximately(this double[] value, double expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasMeanApproximately(value, expected, tolerance);
    }

    /// <summary>Asserts the unbiased sample variance is within tolerance of <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have variance approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasVarianceApproximately(this double[] value, double expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasVarianceApproximately(value, expected, tolerance);
    }

    /// <summary>Asserts the sample standard deviation is within tolerance of <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have standard deviation approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasStdDevApproximately(this double[] value, double expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasStdDevApproximately(value, expected, tolerance);
    }

    /// <summary>Asserts the sample sum is within tolerance of <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have sum approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasSumApproximately(this double[] value, double expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasSumApproximately(value, expected, tolerance);
    }

    /// <summary>Asserts the sample median is within tolerance of <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have median approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasMedianApproximately(this double[] value, double expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasMedianApproximately(value, expected, tolerance);
    }

    /// <summary>
    /// Asserts the percentile-<paramref name="percentile"/> value of the sample is within
    /// tolerance of <paramref name="expected"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to have {percentile}-percentile approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasPercentileApproximately(
        this double[] value,
        double percentile,
        double expected,
        double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.HasPercentileApproximately(value, percentile, expected, tolerance);
    }

    /// <summary>
    /// Asserts <paramref name="value"/> lies within <paramref name="sigmas"/> standard
    /// deviations of <paramref name="sample"/>'s mean.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to lie within {sigmas} sigmas of the sample mean",
        InlineMethodBody = false)]
    public static bool IsWithinSigmasOfMean(this double value, double[] sample, double sigmas)
    {
        ArgumentNullException.ThrowIfNull(sample);
        return Statistics.IsWithinSigmasOfMean(value, sample, sigmas);
    }

    /// <summary>
    /// Asserts every element of the array lies within <paramref name="sigmas"/> standard
    /// deviations of the array's own mean.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to have every element within {sigmas} sigmas of its own mean",
        InlineMethodBody = false)]
    public static bool AreAllWithinSigmasOfMean(this double[] value, double sigmas)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Statistics.AreAllWithinSigmasOfMean(value, sigmas);
    }
}
