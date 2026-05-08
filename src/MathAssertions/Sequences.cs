using System;

namespace MathAssertions;

/// <summary>
/// Sequence-property checks for <see cref="ReadOnlySpan{T}"/> of <see cref="double"/>:
/// monotonicity, sortedness, boundedness, arithmetic and geometric progression,
/// convergence, and length predicates. NaN-aware where it applies, allocation-free
/// where it can be, callable from any test framework or production code.
/// </summary>
/// <remarks>
/// 0.1.0 Cluster 3. Sits alongside <see cref="MathTolerance"/> in the same package; the
/// <c>MathAssertions.TUnit</c> adapter delegates to these helpers from its
/// <c>[GenerateAssertion]</c> extensions.
/// </remarks>
public static class Sequences
{
    /// <summary>
    /// Returns <see langword="true"/> when every value is greater than or equal to the
    /// previous value (non-strict monotonicity; adjacent equal values are allowed).
    /// Empty and single-element spans are vacuously monotonic.
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <returns><see langword="true"/> if non-decreasing.</returns>
    public static bool IsMonotonicallyIncreasing(ReadOnlySpan<double> values)
    {
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] < values[i - 1])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every value is less than or equal to the
    /// previous value (non-strict monotonicity).
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <returns><see langword="true"/> if non-increasing.</returns>
    public static bool IsMonotonicallyDecreasing(ReadOnlySpan<double> values)
    {
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] > values[i - 1])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every value is strictly greater than the
    /// previous value (adjacent equal values fail).
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <returns><see langword="true"/> if strictly increasing.</returns>
    public static bool IsStrictlyMonotonicallyIncreasing(ReadOnlySpan<double> values)
    {
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] <= values[i - 1])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every value is strictly less than the
    /// previous value (adjacent equal values fail).
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <returns><see langword="true"/> if strictly decreasing.</returns>
    public static bool IsStrictlyMonotonicallyDecreasing(ReadOnlySpan<double> values)
    {
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] >= values[i - 1])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the sequence is sorted in ascending order.
    /// Convenience alias for <see cref="IsMonotonicallyIncreasing(ReadOnlySpan{double})"/>.
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <returns><see langword="true"/> if sorted ascending.</returns>
    public static bool IsSorted(ReadOnlySpan<double> values) => IsMonotonicallyIncreasing(values);

    /// <summary>
    /// Returns <see langword="true"/> when every value is in the closed interval
    /// <c>[<paramref name="min"/>, <paramref name="max"/>]</c>. NaN values fail the bound
    /// check (no NaN is "within" any range). An empty span is vacuously bounded.
    /// </summary>
    /// <remarks>
    /// NaN bounds are rejected at entry. IEEE 754 makes every comparison against NaN
    /// return <see langword="false"/>, which would let any sequence pass the bound check
    /// vacuously and silently invert the method's contract. Validation happens before the
    /// loop so the caller sees the failure even on empty input.
    /// </remarks>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="min">Lower bound, inclusive. Must not be NaN.</param>
    /// <param name="max">Upper bound, inclusive. Must not be NaN, and must satisfy
    /// <c>max &gt;= min</c>.</param>
    /// <returns><see langword="true"/> if every value is in <c>[min, max]</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="min"/> or
    /// <paramref name="max"/> is NaN.</exception>
    /// <exception cref="ArgumentException"><paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public static bool IsBounded(ReadOnlySpan<double> values, double min, double max)
    {
        if (double.IsNaN(min))
            throw new ArgumentOutOfRangeException(nameof(min), "min cannot be NaN.");
        if (double.IsNaN(max))
            throw new ArgumentOutOfRangeException(nameof(max), "max cannot be NaN.");
        if (max < min)
            throw new ArgumentException("max must be greater than or equal to min.", nameof(max));

        foreach (var v in values)
        {
            if (double.IsNaN(v))
                return false;
            if (v < min || v > max)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when adjacent differences are equal within
    /// <paramref name="tolerance"/>; that is, the sequence is an arithmetic progression
    /// with a common difference. Empty and single-element spans are vacuously progressions.
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="tolerance">Maximum allowed deviation between adjacent differences.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the sequence has a (tolerance-stable) common difference.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsArithmeticProgression(ReadOnlySpan<double> values, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return true;

        var d = values[1] - values[0];
        for (var i = 2; i < values.Length; i++)
        {
            if (!MathTolerance.IsApproximatelyEqual(values[i] - values[i - 1], d, tolerance))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when adjacent ratios are equal within
    /// <paramref name="tolerance"/>; that is, the sequence is a geometric progression
    /// with a common ratio. Returns <see langword="false"/> when any divisor is zero
    /// (the ratio is undefined). Empty and single-element spans are vacuously progressions.
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="tolerance">Maximum allowed deviation between adjacent ratios. Must
    /// be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the sequence has a (tolerance-stable) common ratio.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsGeometricProgression(ReadOnlySpan<double> values, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return true;

        if (IsZeroMagnitude(values[0]))
            return false;

        var r = values[1] / values[0];
        for (var i = 2; i < values.Length; i++)
        {
            if (IsZeroMagnitude(values[i - 1]))
                return false;
            if (!MathTolerance.IsApproximatelyEqual(values[i] / values[i - 1], r, tolerance))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the last value of the sequence is within
    /// <paramref name="tolerance"/> of <paramref name="limit"/>. The empty-span case
    /// returns <see langword="false"/> because no value has been observed.
    /// </summary>
    /// <remarks>
    /// This is the practical convergence check used in test contexts: the sequence
    /// represents iterations of an algorithm and the last term is the latest estimate.
    /// For the formal Cauchy criterion (no presumed limit), see
    /// <see cref="IsCauchyConvergent(ReadOnlySpan{double}, double)"/>.
    /// </remarks>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="limit">Expected limit value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference between the last value
    /// and <paramref name="limit"/>. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the last value is within tolerance of the limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool ConvergesTo(ReadOnlySpan<double> values, double limit, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length == 0)
            return false;
        return MathTolerance.IsApproximatelyEqual(values[^1], limit, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the last two values of the sequence are within
    /// <paramref name="tolerance"/> of each other. A single-step approximation of the
    /// Cauchy criterion: in the formal definition, the sequence is Cauchy when
    /// <c>|a_m - a_n|</c> is arbitrarily small for all sufficiently large
    /// <c>m</c>, <c>n</c>. The two-tail check is the practical proxy for "the iteration
    /// has stopped meaningfully changing"; sufficient for unit tests of convergent
    /// algorithms, not a substitute for analytic proof. Empty and single-element spans
    /// return <see langword="true"/> vacuously.
    /// </summary>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="tolerance">Maximum allowed absolute difference between the last two
    /// values. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the last two values are within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsCauchyConvergent(ReadOnlySpan<double> values, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (values.Length < 2)
            return true;
        return MathTolerance.IsApproximatelyEqual(values[^1], values[^2], tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the span has exactly <paramref name="expected"/>
    /// elements.
    /// </summary>
    /// <remarks>
    /// Negative <paramref name="expected"/> is rejected at entry. <see cref="ReadOnlySpan{T}.Length"/>
    /// is non-negative, so a negative argument is always a caller bug rather than a useful
    /// "no value will match" sentinel; failing fast surfaces the typo at the call site.
    /// </remarks>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="expected">Required length. Must be non-negative.</param>
    /// <returns><see langword="true"/> if <c>values.Length == expected</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expected"/> is negative.</exception>
    public static bool HasLength<T>(ReadOnlySpan<T> values, int expected)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expected);
        return values.Length == expected;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the span has at least <paramref name="expected"/>
    /// elements.
    /// </summary>
    /// <remarks>
    /// Negative <paramref name="expected"/> is rejected at entry. <see cref="ReadOnlySpan{T}.Length"/>
    /// is non-negative, so any negative threshold makes the result trivially
    /// <see langword="true"/> and would mask caller-side typos in the
    /// <c>HasMinLength(values, -1)</c> shape.
    /// </remarks>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="values">Sequence to inspect.</param>
    /// <param name="expected">Minimum required length. Must be non-negative.</param>
    /// <returns><see langword="true"/> if <c>values.Length &gt;= expected</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expected"/> is negative.</exception>
    public static bool HasMinLength<T>(ReadOnlySpan<T> values, int expected)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expected);
        return values.Length >= expected;
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

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is positive zero or
    /// negative zero. Done at the bit level via the magnitude mask so the operator-<c>==</c>
    /// floating-point analyzer flag (S1244) does not fire on the value path; the same
    /// pattern <see cref="MathTolerance.IsCloseInUlps(double, double, long)"/> uses.
    /// </summary>
    private static bool IsZeroMagnitude(double v)
    {
        const long MagnitudeMask = 0x7FFF_FFFF_FFFF_FFFFL;
        return (BitConverter.DoubleToInt64Bits(v) & MagnitudeMask) == 0L;
    }
}
