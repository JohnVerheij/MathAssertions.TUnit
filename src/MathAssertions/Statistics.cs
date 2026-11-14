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
/// <para>
/// NaN policy. A NaN in the sample poisons the moments: the mean, variance, standard deviation
/// and sum are NaN, so those checks match only an expected value that is itself NaN (both NaN
/// compare equal, as in <see cref="MathTolerance"/>). The median and the percentile sort a
/// copy of the sample, which places NaN first, and then read positions: they are NaN only when a
/// NaN lands on a position they read (for example the middle of a sample that is mostly NaN).
/// The sigma checks (<see cref="IsWithinSigmasOfMean(double, ReadOnlySpan{double}, double)"/>
/// and <see cref="AreAllWithinSigmasOfMean(ReadOnlySpan{double}, double)"/>) return
/// <see langword="false"/> for a NaN value or a NaN in the sample. No method ignores NaN; filter
/// the sample first when NaN is expected.
/// </para>
/// <para>
/// Units of the tolerances. <c>tolerance</c> is an absolute difference in the unit of the
/// values for the mean, standard deviation, sum, median and percentile, and in the squared unit
/// for the variance. <c>sigmas</c> is a multiple of the sample standard deviation and has no
/// unit. The percentile is on the scale 0 to 100 (50 is the median), not 0 to 1.
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
    /// <para>
    /// Welford's online algorithm. Reference: Knuth, <i>The Art of Computer Programming
    /// Vol. 2</i>, §4.2.2. Numerically more stable than the textbook two-pass
    /// <c>E[X^2] - E[X]^2</c> form, especially for long sequences with small variance
    /// relative to mean.
    /// </para>
    /// <para>
    /// A sample of finite values whose largest magnitude is at or above <c>1e150</c>, or at or below
    /// <c>1e-150</c>, is summarized on a copy scaled by a power of two (exact), so the squares neither
    /// overflow nor underflow: the mean of <c>[double.MaxValue, -double.MaxValue]</c> is <c>0</c>, not the
    /// infinity that the running difference would give. The variance is infinite only when it does not
    /// fit in a <see cref="double"/>. The standard deviation checks take the square root before scaling
    /// back, so their standard deviation is finite whenever it fits, even when the variance does not.
    /// </para>
    /// </remarks>
    /// <param name="values">Sample to summarize.</param>
    /// <returns>A tuple of mean and unbiased sample variance.</returns>
    public static (double Mean, double Variance) MeanAndVariance(ReadOnlySpan<double> values)
    {
        var (mean, variance, _) = Moments(values);
        return (mean, variance);
    }

    /// <summary>
    /// Mean, unbiased variance and standard deviation of a sample, computed by Welford's algorithm on the
    /// sample itself, or on a power-of-two scaled copy when the largest magnitude is so large or so small
    /// that the squares would overflow or underflow.
    /// </summary>
    /// <param name="values">Sample to summarize.</param>
    /// <returns>The mean, the variance and the standard deviation; NaN for the empty sample, and a variance
    /// and standard deviation of zero for one value.</returns>
    private static (double Mean, double Variance, double StdDev) Moments(ReadOnlySpan<double> values)
    {
        if (values.Length is 0)
            return (double.NaN, double.NaN, double.NaN);
        if (values.Length is 1)
            return (values[0], 0.0, 0.0);

        var largest = 0.0;
        foreach (var v in values)
            largest = Math.Max(largest, Math.Abs(v));

        // Only a finite sample at an extreme magnitude is rescaled; NaN and infinity propagate as they are.
        var exponent = double.IsFinite(largest) && (largest >= ScaleAboveMagnitude || (largest > 0.0 && largest <= ScaleBelowMagnitude))
            ? Math.ILogB(largest)
            : 0;

        var mean = 0.0;
        var m2 = 0.0;
        var count = 0;
        foreach (var raw in values)
        {
            var v = Math.ScaleB(raw, -exponent);
            count++;
            var delta = v - mean;
            mean += delta / count;
            var delta2 = v - mean;
            m2 += delta * delta2;
        }

        var scaledVariance = m2 / (count - 1);
        return (
            Math.ScaleB(mean, exponent),
            Math.ScaleB(scaledVariance, 2 * exponent),
            Math.ScaleB(Math.Sqrt(scaledVariance), exponent));
    }

    /// <summary>Largest magnitude of a finite sample at or above which <see cref="Moments"/> rescales it.</summary>
    private const double ScaleAboveMagnitude = 1e150;

    /// <summary>Largest magnitude of a finite, non-zero sample at or below which <see cref="Moments"/> rescales it.</summary>
    private const double ScaleBelowMagnitude = 1e-150;

    /// <summary>
    /// Returns <see langword="true"/> when the sample mean is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. The empty span
    /// returns <see langword="false"/> (no value has been observed).
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected mean.</param>
    /// <param name="tolerance">Maximum allowed absolute difference, in the unit of the values. Must be
    /// non-negative and not NaN.</param>
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
    /// <param name="tolerance">Maximum allowed absolute difference, in the squared unit of the values (for meters, meters squared). Must be
    /// non-negative and not NaN.</param>
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
    /// <param name="tolerance">Maximum allowed absolute difference, in the unit of the values. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the sample standard deviation is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasStdDevApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return false;
        var (_, _, stdDev) = Moments(values);
        return MathTolerance.IsApproximatelyEqual(stdDev, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sum of <paramref name="values"/> is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. The empty span has a
    /// sum of zero by convention, matching the identity element for addition. A sum that overflows only
    /// part of the way (<c>[double.MaxValue, double.MaxValue, -double.MaxValue]</c>) is the exact total,
    /// not infinity; the sum is infinite only when it does not fit in a <see cref="double"/>.
    /// </summary>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected sum.</param>
    /// <param name="tolerance">Maximum allowed absolute difference, in the unit of the values. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the sum is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasSumApproximately(ReadOnlySpan<double> values, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        var sum = 0.0;
        foreach (var v in values)
            sum += v;
        if (double.IsInfinity(sum))
            sum = SumWithoutIntermediateOverflow(values, sum);
        return MathTolerance.IsApproximatelyEqual(sum, expected, tolerance);
    }

    /// <summary>
    /// Re-sums a sample whose running sum overflowed. When every value is finite the overflow may be an
    /// intermediate one (<c>[double.MaxValue, double.MaxValue, -double.MaxValue]</c> sums to
    /// <c>double.MaxValue</c>), so the values are summed after an exact scaling by <c>2^-32</c>, which keeps
    /// every partial sum of up to <c>2^31</c> values finite, and the total is scaled back: it is infinite
    /// again only when the sum itself does not fit in a <see cref="double"/>.
    /// </summary>
    /// <param name="values">The sample.</param>
    /// <param name="overflowedSum">The running sum, infinite.</param>
    /// <returns>The sum; <paramref name="overflowedSum"/> when a value is NaN or infinite, as then the
    /// infinity is the true sum.</returns>
    private static double SumWithoutIntermediateOverflow(ReadOnlySpan<double> values, double overflowedSum)
    {
        const int Shift = 32;
        var scaledSum = 0.0;
        foreach (var v in values)
        {
            if (!double.IsFinite(v))
                return overflowedSum;
            scaledSum += Math.ScaleB(v, -Shift);
        }
        return Math.ScaleB(scaledSum, Shift);
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
    /// The even-length median is the correctly rounded midpoint of the two middle values: it neither
    /// overflows nor loses a denormal. It is <c>(a + b)/2</c> whenever that sum is finite (halving
    /// each operand first would round the smallest denormals to zero); two large same-signed values
    /// whose sum overflows (for example <c>[double.MaxValue, double.MaxValue]</c>), and an infinite
    /// neighbor, use the halves.
    /// </para>
    /// <para>
    /// NaN sorts first, so the median is NaN only when a NaN lands on a middle position.
    /// </para>
    /// </remarks>
    /// <param name="values">Sample to inspect.</param>
    /// <param name="expected">Expected median.</param>
    /// <param name="tolerance">Maximum allowed absolute difference, in the unit of the values. Must be
    /// non-negative and not NaN.</param>
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
            median = Midpoint(lower, upper);
        }
        return MathTolerance.IsApproximatelyEqual(median, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the percentile-<paramref name="percentile"/>
    /// value of <paramref name="values"/> is within <paramref name="tolerance"/> of
    /// <paramref name="expected"/>. Linear interpolation between adjacent ranks at fractional
    /// rank <c>p/100 * (N - 1)</c> (the NumPy "linear" method, equal to Excel
    /// <c>PERCENTILE.INC</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allocates a sorted copy of the input. The empty span returns <see langword="false"/>.
    /// NaN sorts first, so a NaN poisons only the ranks that interpolate it (rank 0 and the
    /// ranks between the first two positions).
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
    /// <param name="tolerance">Maximum allowed absolute difference, in the unit of the values. Must be
    /// non-negative and not NaN.</param>
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

        // Equal neighbors have that value as their percentile exactly; the lerp of two equal values can
        // round away from them (3.3 and 3.3 at 7 percent give 3.2999999999999994).
        var pct = lo == hi || MathTolerance.IsCloseInUlps(sorted[lo], sorted[hi], 0L)
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
    /// <param name="sigmas">Number of standard deviations defining the envelope (a multiple of the
    /// standard deviation, no unit). Must be non-negative and not NaN. An infinite sigmas over a sample
    /// with zero standard deviation gives a NaN envelope (<c>infinity * 0</c>), which nothing is
    /// within.</param>
    /// <returns><see langword="true"/> if <c>|value - mean| &lt;= sigmas * stdDev</c>; a NaN
    /// <paramref name="value"/> or a NaN in <paramref name="sample"/> returns
    /// <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sigmas"/> is NaN or negative.</exception>
    public static bool IsWithinSigmasOfMean(double value, ReadOnlySpan<double> sample, double sigmas)
    {
        ValidateSigmas(sigmas);

        if (sample.Length < 2)
            return false;
        var (mean, _, stdDev) = Moments(sample);
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
        var (mean, _, stdDev) = Moments(values);
        var threshold = sigmas * stdDev;
        foreach (var v in values)
        {
            if (!(Math.Abs(v - mean) <= threshold))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Midpoint of two sorted neighbors, correctly rounded. A finite sum is rounded once and halving it is
    /// exact (for a denormal result, the sum of two such small values is exact and the halving is the one
    /// rounding), so <c>(lower + upper) / 2</c> is right whenever the sum is finite. Halving each operand
    /// instead would round the smallest denormals to zero. Only two large same-signed values overflow the
    /// sum; they use the halves, which are exact for such values. An infinite operand uses the halves too
    /// (an infinity stays an infinity, and opposite infinities give NaN).
    /// </summary>
    /// <param name="lower">The smaller neighbor.</param>
    /// <param name="upper">The larger neighbor.</param>
    /// <returns>The value halfway between the two.</returns>
    private static double Midpoint(double lower, double upper)
    {
        var sum = lower + upper;
        return double.IsFinite(sum) ? sum / 2.0 : (lower / 2.0) + (upper / 2.0);
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
