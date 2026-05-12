using System;

namespace MathAssertions;

/// <summary>
/// Exact integer-arithmetic predicates over <see cref="long"/>: divisibility, primality,
/// greatest common divisor and least common multiple, coprimality, power-of-base check,
/// perfect square, modular congruence. All checks are exact (no floating-point tolerance);
/// the methods throw <see cref="ArgumentOutOfRangeException"/> for the inputs where the
/// underlying mathematical operation is undefined (zero divisor, base &lt;= 1, modulus
/// &lt;= 0).
/// </summary>
/// <remarks>
/// 0.1.0 Cluster 6. Sits alongside the floating-point clusters in the same package; the
/// <c>MathAssertions.TUnit</c> adapter delegates to these helpers from its
/// <c>[GenerateAssertion]</c> extensions.
/// </remarks>
public static class NumberTheory
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is exactly divisible
    /// by <paramref name="divisor"/>.
    /// </summary>
    /// <param name="value">Dividend.</param>
    /// <param name="divisor">Divisor. Must not be zero.</param>
    /// <returns><see langword="true"/> if <c>value % divisor == 0</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is zero.</exception>
    public static bool IsDivisibleBy(long value, long divisor)
    {
        if (divisor == 0)
            throw new ArgumentOutOfRangeException(nameof(divisor), "divisor cannot be zero.");
        return value % divisor == 0;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is prime. Values less
    /// than two return <see langword="false"/>; <c>2</c> and <c>3</c> are special-cased
    /// to <see langword="true"/>; otherwise the wheel-of-six trial-division loop tests
    /// candidates of the form <c>6k +/- 1</c>.
    /// </summary>
    /// <remarks>
    /// The loop bound is written <c>i &lt;= value / i</c> rather than
    /// <c>i * i &lt;= value</c> so the candidate-square computation never overflows when
    /// <paramref name="value"/> approaches <see cref="long.MaxValue"/>. Algebraically
    /// equivalent for the positive values reached here.
    /// </remarks>
    /// <param name="value">Value to test.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is prime.</returns>
    public static bool IsPrime(long value)
    {
        if (value < 2)
            return false;
        if (value == 2 || value == 3)
            return true;
        if (value % 2 == 0 || value % 3 == 0)
            return false;

        for (var i = 5L; i <= value / i; i += 6)
        {
            if (value % i == 0 || value % (i + 2) == 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="a"/> and <paramref name="b"/>
    /// are coprime, that is <c>gcd(a, b) == 1</c>.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns><see langword="true"/> if the two values share no common divisor other than 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either argument is <see cref="long.MinValue"/>.</exception>
    public static bool AreCoprime(long a, long b) => GreatestCommonDivisor(a, b) == 1;

    /// <summary>
    /// Returns the greatest common divisor of <paramref name="a"/> and <paramref name="b"/>
    /// computed by the Euclidean algorithm. The result is non-negative.
    /// <c>gcd(0, 0) == 0</c> by convention; <c>gcd(0, b) == |b|</c> for any
    /// <paramref name="b"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="long.MinValue"/> is rejected: its absolute value does not fit in
    /// <see cref="long"/>, so the standard Euclidean implementation produces a wrong-sign
    /// result for some input pairs. Callers needing GCD across the full <see cref="long"/>
    /// range should widen to <see cref="System.Numerics.BigInteger"/>.
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns>The greatest common divisor.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either argument is <see cref="long.MinValue"/>.</exception>
    public static long GreatestCommonDivisor(long a, long b)
    {
        if (a == long.MinValue)
            throw new ArgumentOutOfRangeException(nameof(a), "long.MinValue is not supported; its absolute value does not fit in long.");
        if (b == long.MinValue)
            throw new ArgumentOutOfRangeException(nameof(b), "long.MinValue is not supported; its absolute value does not fit in long.");

        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }

    /// <summary>
    /// Returns the least common multiple of <paramref name="a"/> and <paramref name="b"/>.
    /// <c>lcm(0, b) == 0</c> for any <paramref name="b"/> by convention. The result is
    /// non-negative.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Computed via <c>|a / gcd(a, b) * b|</c>, dividing first to keep the intermediate
    /// as small as possible. The final multiplication runs in a <see langword="checked"/> context;
    /// when the LCM does not fit in <see cref="long"/> (large coprime inputs near the
    /// signed range, for example <c>lcm(long.MaxValue, 2)</c>) an
    /// <see cref="OverflowException"/> is thrown rather than the wrapped negative result
    /// the unchecked computation would silently produce. Callers needing LCM across the
    /// full <see cref="long"/> range should widen to <see cref="System.Numerics.BigInteger"/>.
    /// </para>
    /// <para>
    /// <see cref="long.MinValue"/> is rejected at entry for the same reason
    /// <see cref="GreatestCommonDivisor(long, long)"/> rejects it. The check happens
    /// before the <c>lcm(0, b) == 0</c> fast-path so the contract holds even when one
    /// argument is zero and the other is <see cref="long.MinValue"/>.
    /// </para>
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns>The least common multiple.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either argument is <see cref="long.MinValue"/>.</exception>
    /// <exception cref="OverflowException">The least common multiple does not fit in <see cref="long"/>.</exception>
    public static long LeastCommonMultiple(long a, long b)
    {
        if (a == long.MinValue)
            throw new ArgumentOutOfRangeException(nameof(a), "long.MinValue is not supported; its absolute value does not fit in long.");
        if (b == long.MinValue)
            throw new ArgumentOutOfRangeException(nameof(b), "long.MinValue is not supported; its absolute value does not fit in long.");
        if (a == 0 || b == 0)
            return 0;

        var gcd = GreatestCommonDivisor(a, b);
        return checked(Math.Abs(a / gcd * b));
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is a non-negative
    /// integer power of <paramref name="baseValue"/>; that is, when there exists a
    /// non-negative integer <c>k</c> such that <c><paramref name="baseValue"/>^k ==
    /// <paramref name="value"/></c>. By convention <c>baseValue^0 == 1</c>, so
    /// <see cref="IsPowerOf"/> returns <see langword="true"/> for
    /// <paramref name="value"/> <c>== 1</c> and any valid base.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <param name="baseValue">Base. Must be greater than 1; <c>0</c>, <c>1</c>, and
    /// negative bases are rejected because the resulting power sequence is degenerate or
    /// non-monotonic.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is a power of
    /// <paramref name="baseValue"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="baseValue"/> is less than or equal to 1.</exception>
    public static bool IsPowerOf(long value, long baseValue)
    {
        if (baseValue <= 1)
            throw new ArgumentOutOfRangeException(nameof(baseValue), "baseValue must be greater than 1.");
        if (value <= 0)
            return false;

        while (value > 1)
        {
            if (value % baseValue != 0)
                return false;
            value /= baseValue;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is a non-negative
    /// perfect square. <c>0</c> and <c>1</c> are perfect squares; negative values are not.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Math.Sqrt(double)"/> truncated to <see cref="long"/> to obtain a
    /// candidate, then checks both the candidate and its successor against
    /// <paramref name="value"/> to accommodate the rounding error <c>Math.Sqrt</c> can
    /// introduce for very large inputs. The successor check is skipped when squaring
    /// would overflow <see cref="long"/>; in that regime the candidate is the only
    /// admissible answer.
    /// </remarks>
    /// <param name="value">Value to test.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is a non-negative perfect square.</returns>
    public static bool IsPerfectSquare(long value)
    {
        if (value < 0)
            return false;

        var sqrt = long.CreateTruncating(Math.Sqrt(value));
        if (sqrt * sqrt == value)
            return true;

        // floor(sqrt(long.MaxValue)) = 3037000499. Beyond this, (sqrt + 1) * (sqrt + 1)
        // overflows; the original sqrt² check (above) is then the only candidate.
        const long MaxSafeSqrt = 3037000499L;
        if (sqrt >= MaxSafeSqrt)
            return false;
        return (sqrt + 1) * (sqrt + 1) == value;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="a"/> and <paramref name="b"/>
    /// are congruent modulo <paramref name="modulus"/>: <c>a ≡ b (mod modulus)</c>.
    /// </summary>
    /// <remarks>
    /// Implemented by comparing the canonical non-negative residues of <paramref name="a"/>
    /// and <paramref name="b"/> rather than by checking <c>(a - b) % modulus == 0</c>:
    /// the subtraction would overflow <see cref="long"/> for inputs straddling the signed
    /// range (for example <see cref="long.MaxValue"/> and a negative value). Working
    /// through the residues keeps every intermediate within the input range.
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="modulus">Modulus. Must be positive.</param>
    /// <returns><see langword="true"/> if the two values share the same residue modulo
    /// <paramref name="modulus"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="modulus"/> is less than or equal to zero.</exception>
    public static bool IsCongruent(long a, long b, long modulus)
    {
        if (modulus <= 0)
            throw new ArgumentOutOfRangeException(nameof(modulus), "modulus must be positive.");
        return CanonicalMod(a, modulus) == CanonicalMod(b, modulus);
    }

    /// <summary>
    /// Returns the mathematical (non-negative) modulo of <paramref name="value"/> by
    /// <paramref name="modulus"/>. The C# <c>%</c> operator returns a result with the
    /// sign of the dividend; this helper normalizes the result into <c>[0, modulus)</c>.
    /// </summary>
    private static long CanonicalMod(long value, long modulus)
    {
        var r = value % modulus;
        return r < 0 ? r + modulus : r;
    }
}
