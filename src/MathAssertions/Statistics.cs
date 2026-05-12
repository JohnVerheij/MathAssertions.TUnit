using System;

namespace MathAssertions;

/// <summary>
/// Statistical-property checks for <see cref="ReadOnlySpan{T}"/> of <see cref="double"/>:
/// mean, variance, standard deviation, sum, median, percentile, and sigma-bound checks
/// against the sample's own mean. Numerically stable single-pass mean and variance via
/// Welford's online algorithm.
/// </summary>
/// <remarks>
/// <para>
/// 0.1.0 Cluster 4. Sits alongside <see cref="MathTolerance"/> and <see cref="Sequences"/>
/// in the same package; the <c>MathAssertions.TUnit</c> adapter delegates to these helpers
/// from its <c>[GenerateAssertion]</c> extensions.
/// </para>
/// <para>
/// All variance and standard-deviation calculations use the unbiased
/// <c>N-1</c> denominator (sample variance), matching the convention in NIST/SEMATECH
/// e-Handbook of Statistical Methods §1.3.5.6 and Knuth, <i>The Art of Computer
/// Programming Vol. 2</i>, §4.2.2.
/// </para>
/// </remarks>
public static class Statistics
{
    /// <summary>
    /// Returns the sample mean and unbiased sample variance of <paramref name="values"/>
    /// in a single numerically stable pass. The empty span yields <c>(NaN, NaN)</c>; a
    /// single-element span yields <c>(value, 0)</c> because sample variance with the
    /// <c>N-1</c> denominator is undefined for one observation.
    /// </summary>
    /// <remarks>
    /// Welford's online algorithm. Reference: Knuth, <i>The Art of Computer Programming
    /// Vol. 2</i>, §4.2.2. Numerically more stable than the textbook two-pass
    /// <c>E[X^2] - E[X]^2</c> form, especially for long sequences with small variance
    /// relative to mean.
    /// </remarks>
    /// <param name="values">Sample to summarize.</param>
    /// <returns>A tuple of mean and unbiased sample variance.</returns>
    public static (double Mean, double Variance) MeanAndVariance(ReadOnlySpan<double> values)
    {
        if (values.Length is 0)
            return (double.NaN, double.NaN);
        if (values.Length is 1)
            return (values[0], 0.0);

        var mean = 0.0;
        var m2 = 0.0;
        var count = 0;
        foreach (var v in values)
        {
            count++;
            var delta = v - mean;
            mean += delta / count;
            var delta2 = v - mean;
            m2 += delta * delta2;
        }
        return (mean, m2 / (count - 1));
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sample mean is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. The empty span
    /// returns <see langword="false"/> (no value has been observed).
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected mean.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the sample mean is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasMeanApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length is 0)
            return false;
        var (mean, _) = MeanAndVariance(values);
        return MathTolerance.IsApproximatelyEqual(mean, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sample variance is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. Variance is undefined
    /// for fewer than two observations under the unbiased <c>N-1</c> convention; the
    /// method returns <see langword="false"/> for empty and single-element spans.
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected variance.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the sample variance is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasVarianceApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return false;
        var (_, variance) = MeanAndVariance(values);
        return MathTolerance.IsApproximatelyEqual(variance, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sample standard deviation is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. Standard deviation is
    /// the square root of the unbiased sample variance and is undefined for fewer than
    /// two observations; the method returns <see langword="false"/> for empty and
    /// single-element spans.
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected standard deviation.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the sample standard deviation is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasStdDevApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return false;
        var (_, variance) = MeanAndVariance(values);
        return MathTolerance.IsApproximatelyEqual(Math.Sqrt(variance), expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sum of <paramref name="values"/> is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. The empty span has a
    /// sum of zero by convention, matching the identity element for addition.
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected sum.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the sum is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasSumApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        var sum = 0.0;
        foreach (var v in values)
            sum += v;
        return MathTolerance.IsApproximatelyEqual(sum, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the median of <paramref name="values"/> is
    /// within <paramref name="tolerance"/> of <paramref name="expected"/>. For even-length
    /// samples, the median is the mean of the two middle values after sorting. The empty
    /// span returns <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sorts a copy of the input, so callers do not observe a side effect on the original
    /// span backing store. The copy is the algorithm's only allocation.
    /// </para>
    /// <para>
    /// Even-length median uses the overflow-safe form <c>a/2 + b/2</c> rather than the
    /// textbook <c>(a + b)/2</c>. The latter overflows to <see cref="double.PositiveInfinity"/>
    /// for samples whose two middle values sum past <see cref="double.MaxValue"/>
    /// (for example <c>[double.MaxValue, double.MaxValue]</c>); the half-then-add form
    /// is exact for the same input.
    /// </para>
    /// </remarks>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected median.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the median is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasMedianApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length is 0)
            return false;

        var sorted = values.ToArray();
        Array.Sort(sorted);

        double median;
        if (sorted.Length % 2 is 1)
        {
            median = sorted[sorted.Length / 2];
        }
        else
        {
            var lower = sorted[(sorted.Length / 2) - 1];
            var upper = sorted[sorted.Length / 2];
            median = (lower / 2.0) + (upper / 2.0);
        }
        return MathTolerance.IsApproximatelyEqual(median, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the percentile-<paramref name="percentile"/>
    /// value of <paramref name="values"/> is within <paramref name="tolerance"/> of
    /// <paramref name="expected"/>. Linear interpolation between adjacent ranks per the
    /// NIST/SEMATECH e-Handbook of Statistical Methods §1.3.5.6.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allocates a sorted copy of the input. The empty span returns <see langword="false"/>.
    /// </para>
    /// <para>
    /// Interpolation uses the overflow-safe lerp form <c>a*(1-f) + b*f</c> rather than the
    /// textbook <c>a + f*(b - a)</c>. The latter overflows when <c>b - a</c> exceeds
    /// <see cref="double.MaxValue"/> (for example <c>[-double.MaxValue, double.MaxValue]</c>),
    /// which would silently return <see cref="double.PositiveInfinity"/> for the median
    /// percentile of that input. The two forms are algebraically equal for finite inputs;
    /// the lerp form keeps each multiplication within the original magnitude band.
    /// </para>
    /// </remarks>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="percentile">Percentile in the closed interval <c>[0, 100]</c>.</param>
    /// <param name="expected">Expected percentile value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the percentile is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentile"/> is outside
    /// <c>[0, 100]</c> or NaN, or <paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasPercentileApproximately(
        ReadOnlySpan<double> values,
        double percentile,
        double expected,
        double tolerance)
    {
        if (double.IsNaN(percentile))
            throw new ArgumentOutOfRangeException(nameof(percentile), "percentile cannot be NaN.");
        if (percentile < 0.0 || percentile > 100.0)
            throw new ArgumentOutOfRangeException(nameof(percentile), "percentile must be in [0, 100].");
        ValidateTolerance(tolerance);

        if (values.Length is 0)
            return false;

        var sorted = values.ToArray();
        Array.Sort(sorted);

        var rank = percentile / 100.0 * (sorted.Length - 1);
        var lo = int.CreateChecked(Math.Floor(rank));
        var hi = int.CreateChecked(Math.Ceiling(rank));
        var frac = rank - lo;

        var pct = lo == hi
            ? sorted[lo]
            : (sorted[lo] * (1.0 - frac)) + (sorted[hi] * frac);
        return MathTolerance.IsApproximatelyEqual(pct, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> lies within
    /// <paramref name="sigmas"/> standard deviations of the mean of
    /// <paramref name="sample"/>. Returns <see langword="false"/> for samples with fewer
    /// than two observations, where standard deviation is undefined under the unbiased
    /// <c>N-1</c> convention.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <param name="sample">Reference sample whose mean and standard deviation define the
    /// envelope.</param>
    /// <param name="sigmas">Number of standard deviations defining the envelope. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if <c>|value - mean| &lt;= sigmas * stdDev</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sigmas"/> is NaN or negative.</exception>
    public static bool IsWithinSigmasOfMean(double value, ReadOnlySpan<double> sample, double sigmas)
    {
        ValidateSigmas(sigmas);

        if (sample.Length < 2)
            return false;
        var (mean, variance) = MeanAndVariance(sample);
        var stdDev = Math.Sqrt(variance);
        return Math.Abs(value - mean) <= sigmas * stdDev;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every value in <paramref name="values"/> lies
    /// within <paramref name="sigmas"/> standard deviations of that same sample's mean.
    /// Returns <see langword="false"/> for samples with fewer than two observations
    /// (standard deviation undefined) and for samples that contain NaN or infinity (the
    /// envelope is undefined).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Computes the sample's mean and standard deviation once and reuses them for every
    /// element check, so the method is O(N) rather than the O(N^2) shape that would
    /// result from per-element delegation to
    /// <see cref="IsWithinSigmasOfMean(double, ReadOnlySpan{double}, double)"/>.
    /// </para>
    /// <para>
    /// Non-finite inputs propagate NaN through the mean/variance/threshold path; the
    /// per-element check is written as <c>!(|v - mean| &lt;= threshold)</c> rather than
    /// the equivalent-for-finite-inputs <c>|v - mean| &gt; threshold</c> so that a NaN
    /// comparison short-circuits to <see langword="false"/> on the first element rather
    /// than silently passing every check (the IEEE 754 rule that any comparison against
    /// NaN is false would otherwise let the loop fall through to a vacuous-true return).
    /// </para>
    /// </remarks>
    /// <param name="values">Sample to inspect, both as the reference and as the test set.</param>
    /// <param name="sigmas">Number of standard deviations defining the envelope. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if every value lies within the sigma envelope.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sigmas"/> is NaN or negative.</exception>
    public static bool AreAllWithinSigmasOfMean(ReadOnlySpan<double> values, double sigmas)
    {
        ValidateSigmas(sigmas);

        if (values.Length < 2)
            return false;
        var (mean, variance) = MeanAndVariance(values);
        var stdDev = Math.Sqrt(variance);
        var threshold = sigmas * stdDev;
        foreach (var v in values)
        {
            if (!(Math.Abs(v - mean) <= threshold))
                return false;
        }
        return true;
    }

    private static void ValidateTolerance(double tolerance)
    {
        if (double.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");
        }
    }

    private static void ValidateSigmas(double sigmas)
    {
        if (double.IsNaN(sigmas))
        {
            throw new ArgumentOutOfRangeException(nameof(sigmas), "sigmas cannot be NaN.");
        }
        if (sigmas < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sigmas), "sigmas cannot be negative.");
        }
    }
}
