using System;

namespace MathAssertions;

/// <summary>
/// Statistical-property checks for <see cref="ReadOnlySpan{T}"/> of <see cref="double"/>:
/// mean, variance, standard deviation, sum, median, percentile, and sigma-bound checks
/// against the sample's own mean. The mean and the sum are compensated (Neumaier), and the variance
/// comes from Welford's online algorithm over the sample centered on its smallest value.
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
    /// The mean is a compensated sum (Neumaier) divided by the count: the rounding error of each addition is carried
    /// along, so a small value between two large ones that cancel still counts, whichever comes first. The compensation
    /// is one extra term, so with several levels of cancellation (<c>[1e300, 1e200, 1, -1e200, -1e300]</c>) a value can
    /// still be lost. The variance is Welford's algorithm over
    /// the sample centered on its smallest value, so values only a few units in the last place apart keep their
    /// variance.
    /// </para>
    /// <para>
    /// A sample of finite values is summarized on copies scaled by powers of two (exact), so nothing overflows
    /// or underflows on the way, whatever the magnitudes: the mean over a copy shifted down only as far as keeps
    /// every partial sum finite (scaled up when the values are below 1, not scaled inside the usual range), with
    /// the bits a shift down rounds away kept apart and added back, and the squares over the centered copy scaled
    /// by the sample's spread. The mean of <c>[double.MaxValue, -double.MaxValue]</c> is <c>0</c>, not the
    /// infinity that a running difference would give. The variance is infinite only when it does not fit in a
    /// <see cref="double"/>, and zero only when it is too small to represent. The standard deviation checks take
    /// the square root before scaling back, so their standard deviation is finite and non-zero whenever it fits,
    /// even when the variance does not. A sample with NaN or an infinity is summarized as it is, so those
    /// propagate.
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
    /// Mean, unbiased variance and standard deviation of a sample. For a finite sample the mean comes from
    /// <see cref="MeanWithoutOverflow"/>, and the squares from Welford's algorithm over a copy centered on the
    /// smallest value and scaled by the power of two of the sample's spread (<see cref="SpreadExponent"/>); a sample
    /// with NaN or an infinity is summarized by plain Welford, so those propagate.
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

        var (largest, min, max) = Extremes(values);
        if (!double.IsFinite(largest))
        {
            var (plainMean, plainM2) = Welford(values, 0, 0.0);
            var plainVariance = plainM2 / (values.Length - 1);
            return (plainMean, plainVariance, Math.Sqrt(plainVariance));
        }

        var exponent = SpreadExponent(min, max);
        var (_, m2) = Welford(values, exponent, Math.ScaleB(min, -exponent));
        var scaledVariance = m2 / (values.Length - 1);
        return (
            MeanWithoutOverflow(values, largest),
            Math.ScaleB(scaledVariance, 2 * exponent),
            Math.ScaleB(Math.Sqrt(scaledVariance), exponent));
    }

    /// <summary>The largest magnitude, the smallest value and the largest value of a sample; the largest magnitude is
    /// NaN when a value is.</summary>
    /// <param name="values">The sample, not empty.</param>
    /// <returns>The three extremes.</returns>
    private static (double Largest, double Min, double Max) Extremes(ReadOnlySpan<double> values)
    {
        var largest = 0.0;
        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;
        foreach (var v in values)
        {
            largest = Math.Max(largest, Math.Abs(v));
            min = Math.Min(min, v);
            max = Math.Max(max, v);
        }

        return (largest, min, max);
    }

    /// <summary>Welford's running mean and sum of squared differences over the sample scaled by
    /// <c>2^-exponent</c> and shifted by <paramref name="reference"/>. A shift leaves the squared differences as they
    /// are, and centering on the smallest value keeps the running mean from rounding away a spread of a few units in
    /// the last place.</summary>
    /// <param name="values">The sample.</param>
    /// <param name="exponent">The power of two the values are divided by.</param>
    /// <param name="reference">The value subtracted from each scaled value.</param>
    /// <returns>The scaled, shifted mean and the scaled sum of squared differences from it.</returns>
    private static (double Mean, double M2) Welford(ReadOnlySpan<double> values, int exponent, double reference)
    {
        var mean = 0.0;
        var m2 = 0.0;
        var count = 0;
        foreach (var raw in values)
        {
            var v = Math.ScaleB(raw, -exponent) - reference;
            count++;
            var delta = v - mean;
            mean += delta / count;
            m2 += delta * (v - mean);
        }

        return (mean, m2);
    }

    /// <summary>
    /// The mean of a finite sample of at least two values: the compensated sum of a scaled copy
    /// (<see cref="CompensatedSum"/>) divided by the count. A sample whose largest magnitude is below 1 is scaled up
    /// to it, which is exact and keeps the division clear of the denormal range, where it would round early; one near
    /// the top of the range is shifted down by <see cref="OverflowShift"/>, just far enough to keep every partial sum
    /// finite; any other is not scaled. After a shift down the sum is restored before the division when it fits, so a
    /// tiny mean is not rounded away in the shifted copy, and the bits the shift rounded away are added back.
    /// </summary>
    /// <param name="values">The sample.</param>
    /// <param name="largest">The largest magnitude in it, finite.</param>
    /// <returns>The mean.</returns>
    private static double MeanWithoutOverflow(ReadOnlySpan<double> values, double largest)
    {
        var count = values.Length;
        var shift = largest is > 0 and < 1 ? Math.ILogB(largest) : OverflowShift(largest, count);
        var (sum, compensation, residual) = CompensatedSum(values, shift);
        var scaledSum = sum + compensation;
        if (shift <= 0)
            return Math.ScaleB(Divide(sum, compensation, count), shift);

        var total = Math.ScaleB(scaledSum, shift) + residual;
        return double.IsFinite(total)
            ? total / count
            : Math.ScaleB(scaledSum / count, shift) + (residual / count);
    }

    /// <summary>
    /// A compensated sum divided by the count with one rounding in effect: the quotient of the sum, then the exact
    /// remainder of that division (a fused multiply-add) and the compensation divided by the count and added to it.
    /// Dividing the rounded total instead rounds twice, which turns the mean of <c>[0.1, 0.2, 0.3]</c> into
    /// <c>0.19999999999999998</c>.
    /// </summary>
    /// <param name="sum">The compensated sum's running sum.</param>
    /// <param name="compensation">Its compensation.</param>
    /// <param name="count">The number of values.</param>
    /// <returns>The quotient.</returns>
    private static double Divide(double sum, double compensation, int count)
    {
        var quotient = sum / count;
        var remainder = Math.FusedMultiplyAdd(-quotient, count, sum);
        return quotient + ((remainder + compensation) / count);
    }

    /// <summary>
    /// Neumaier's compensated sum of the sample scaled by <c>2^-shift</c>: the low-order part each addition rounds
    /// away is collected in the compensation, so a small value between two larger ones that cancel still counts,
    /// whichever comes first; the compensation is one term, so with several levels of cancellation a value can still be
    /// lost. A shift down also rounds away the low bits of a value near the bottom of the range; those are kept in the
    /// residual, exactly, at the original scale. A shift up (a negative <paramref name="shift"/>) is exact.
    /// </summary>
    /// <param name="values">The sample, finite.</param>
    /// <param name="shift">The power of two the values are divided by.</param>
    /// <returns>The scaled sum, its compensation, and the residual at the original scale.</returns>
    private static (double Sum, double Compensation, double Residual) CompensatedSum(ReadOnlySpan<double> values, int shift)
    {
        var sum = 0.0;
        var compensation = 0.0;
        var residual = 0.0;
        foreach (var raw in values)
        {
            var v = Math.ScaleB(raw, -shift);
            residual += raw - Math.ScaleB(v, shift);
            var next = sum + v;
            compensation += Math.Abs(sum) >= Math.Abs(v) ? (sum - next) + v : (v - next) + sum;
            sum = next;
        }

        return (sum, compensation, residual);
    }

    /// <summary>
    /// The smallest power-of-two shift that keeps every partial sum and every running difference of
    /// <paramref name="count"/> values of magnitude up to <paramref name="largest"/> finite: zero unless the values
    /// come near the top of the range of a <see cref="double"/>.
    /// </summary>
    /// <param name="largest">The largest magnitude, finite and not negative.</param>
    /// <param name="count">The number of values, at least one when <paramref name="largest"/> is positive.</param>
    /// <returns>The shift, at least zero.</returns>
    private static int OverflowShift(double largest, int count)
        => largest > 0 ? Math.Max(0, Math.ILogB(largest) + int.Log2(count - 1) + 1 - 1021) : 0;

    /// <summary>
    /// The power-of-two exponent of the spread of a finite sample (its largest value minus its smallest), by which
    /// the squares are scaled: a difference of the scaled values is at most 2, and the values themselves stay below
    /// <c>2^55</c> (a non-zero spread is at least half the spacing of doubles at the largest magnitude), so no square
    /// overflows or underflows, for huge, tiny and nearly equal values alike. Zero for a sample of equal values.
    /// </summary>
    /// <param name="min">The smallest value, finite.</param>
    /// <param name="max">The largest value, finite.</param>
    /// <returns>The exponent.</returns>
    private static int SpreadExponent(double min, double max)
    {
        var spread = max - min;
        if (double.IsInfinity(spread))
            return Math.ILogB((max * 0.5) - (min * 0.5)) + 1;
        return spread > 0 ? Math.ILogB(spread) : 0;
    }

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

        return MathTolerance.IsApproximatelyEqual(Sum(values), expected, tolerance);
    }

    /// <summary>
    /// The sum of a sample. A sample with NaN or an infinity is summed plainly, so those carry into the sum. A finite
    /// sample is summed with compensation (<see cref="CompensatedSum"/>), so a small value between two large ones that
    /// cancel still counts, over a copy shifted down by <see cref="OverflowShift"/>, so no
    /// partial sum overflows (<c>[double.MaxValue, double.MaxValue, -double.MaxValue]</c> sums to
    /// <c>double.MaxValue</c>): the sum is infinite only when it does not fit in a <see cref="double"/>.
    /// </summary>
    /// <param name="values">The sample.</param>
    /// <returns>The sum; zero for the empty sample.</returns>
    private static double Sum(ReadOnlySpan<double> values)
    {
        var largest = 0.0;
        foreach (var v in values)
            largest = Math.Max(largest, Math.Abs(v));

        if (!double.IsFinite(largest))
        {
            var plain = 0.0;
            foreach (var v in values)
                plain += v;
            return plain;
        }

        var shift = OverflowShift(largest, values.Length);
        var (sum, compensation, residual) = CompensatedSum(values, shift);
        return Math.ScaleB(sum + compensation, shift) + residual;
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
