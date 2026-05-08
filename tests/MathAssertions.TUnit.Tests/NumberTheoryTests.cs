using System;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the semantics of <see cref="NumberTheory"/>. Reference values follow the standard
/// integer-arithmetic textbooks; primality samples cover both small composites that the
/// loop exits on and edge cases of the wheel-of-six trial-division.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class NumberTheoryTests
{
    // ----- IsDivisibleBy -----

    [Test]
    [Arguments(6L, 2L, true)]
    [Arguments(6L, 3L, true)]
    [Arguments(7L, 2L, false)]
    [Arguments(0L, 5L, true)]
    [Arguments(-12L, 4L, true)]
    [Arguments(-12L, -4L, true)]
    [Arguments(15L, -5L, true)]
    public async Task IsDivisibleBy_HappyPath(long value, long divisor, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsDivisibleBy(value, divisor)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsDivisibleBy_ZeroDivisor_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsDivisibleBy(10L, 0L))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsPrime -----

    [Test]
    [Arguments(0L, false)]
    [Arguments(1L, false)]
    [Arguments(2L, true)]
    [Arguments(3L, true)]
    [Arguments(4L, false)]
    [Arguments(5L, true)]
    [Arguments(7L, true)]
    [Arguments(9L, false)]
    [Arguments(17L, true)]
    [Arguments(25L, false)]
    [Arguments(49L, false)]
    [Arguments(100L, false)]
    [Arguments(101L, true)]
    [Arguments(-7L, false)]
    public async Task IsPrime(long value, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsPrime(value)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsPrime_LargeMersennePrime_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 2^31 - 1 = 2147483647, the largest 32-bit Mersenne prime.
        await Assert.That(NumberTheory.IsPrime(2147483647L)).IsTrue();
    }

    [Test]
    public async Task IsPrime_LargeComposite_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 2147483647 + 2 = 2147483649 = 3 * 715827883. Composite, exits early via mod-3 check.
        await Assert.That(NumberTheory.IsPrime(2147483649L)).IsFalse();
    }

    // ----- AreCoprime -----

    [Test]
    [Arguments(15L, 28L, true)]
    [Arguments(12L, 8L, false)]
    [Arguments(7L, 11L, true)]
    [Arguments(0L, 5L, false)]   // gcd(0, 5) = 5, not coprime
    [Arguments(1L, 100L, true)]   // 1 is coprime to anything
    public async Task AreCoprime(long a, long b, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.AreCoprime(a, b)).IsEqualTo(expected);
    }

    // ----- GreatestCommonDivisor -----

    [Test]
    [Arguments(12L, 8L, 4L)]
    [Arguments(17L, 13L, 1L)]
    [Arguments(0L, 5L, 5L)]
    [Arguments(5L, 0L, 5L)]
    [Arguments(0L, 0L, 0L)]
    [Arguments(-12L, 8L, 4L)]
    [Arguments(12L, -8L, 4L)]
    [Arguments(-12L, -8L, 4L)]
    [Arguments(48L, 18L, 6L)]
    public async Task GreatestCommonDivisor(long a, long b, long expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.GreatestCommonDivisor(a, b)).IsEqualTo(expected);
    }

    [Test]
    public async Task GreatestCommonDivisor_MinValueAsA_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.GreatestCommonDivisor(long.MinValue, 4L))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task GreatestCommonDivisor_MinValueAsB_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.GreatestCommonDivisor(4L, long.MinValue))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- LeastCommonMultiple -----

    [Test]
    [Arguments(4L, 6L, 12L)]
    [Arguments(3L, 5L, 15L)]
    [Arguments(0L, 7L, 0L)]
    [Arguments(7L, 0L, 0L)]
    [Arguments(-4L, 6L, 12L)]
    [Arguments(4L, -6L, 12L)]
    [Arguments(2L, 3L, 6L)]
    public async Task LeastCommonMultiple(long a, long b, long expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.LeastCommonMultiple(a, b)).IsEqualTo(expected);
    }

    [Test]
    public async Task LeastCommonMultiple_MinValueAsA_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.LeastCommonMultiple(long.MinValue, 4L))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsPowerOf -----

    [Test]
    [Arguments(8L, 2L, true)]
    [Arguments(9L, 2L, false)]
    [Arguments(27L, 3L, true)]
    [Arguments(81L, 3L, true)]
    [Arguments(82L, 3L, false)]
    [Arguments(1L, 2L, true)]    // 2^0 = 1 by convention
    [Arguments(1L, 5L, true)]    // 5^0 = 1
    [Arguments(0L, 2L, false)]   // 0 is not a power of any base
    [Arguments(-8L, 2L, false)]  // negatives never
    [Arguments(2L, 2L, true)]
    [Arguments(1024L, 2L, true)]  // 2^10
    public async Task IsPowerOf(long value, long baseValue, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsPowerOf(value, baseValue)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsPowerOf_BaseZero_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsPowerOf(8L, 0L))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsPowerOf_BaseOne_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsPowerOf(8L, 1L))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsPowerOf_NegativeBase_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsPowerOf(8L, -2L))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsPerfectSquare -----

    [Test]
    [Arguments(0L, true)]
    [Arguments(1L, true)]
    [Arguments(4L, true)]
    [Arguments(5L, false)]
    [Arguments(9L, true)]
    [Arguments(10L, false)]
    [Arguments(25L, true)]
    [Arguments(100L, true)]
    [Arguments(99L, false)]
    [Arguments(-4L, false)]
    public async Task IsPerfectSquare(long value, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsPerfectSquare(value)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsPerfectSquare_LargePerfectSquare_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 3037000499^2 = 9223372030926249001; the largest perfect square that fits in long.
        await Assert.That(NumberTheory.IsPerfectSquare(9223372030926249001L)).IsTrue();
    }

    [Test]
    public async Task IsPerfectSquare_LongMaxValue_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // long.MaxValue is just above the largest perfect square that fits. The
        // overflow-safe bound check must skip the (sqrt+1)² candidate (which would
        // overflow) and rely on sqrt² alone, returning false correctly.
        await Assert.That(NumberTheory.IsPerfectSquare(long.MaxValue)).IsFalse();
    }

    [Test]
    public async Task IsPerfectSquare_OneLessThanLargePerfectSquare_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 3037000499^2 - 1 is not a perfect square; tests the sqrt-rounding branch
        // in the lower-magnitude regime where (sqrt+1)² is computed safely.
        await Assert.That(NumberTheory.IsPerfectSquare(9223372030926249000L)).IsFalse();
    }

    [Test]
    public async Task IsPerfectSquare_NextLargerSquareDetected_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 1000003^2 = 1000006000009, a value where Math.Sqrt could conceivably round to
        // 1000002 due to float precision, and the +1 check must catch it.
        await Assert.That(NumberTheory.IsPerfectSquare(1000006000009L)).IsTrue();
    }

    // ----- IsCongruent -----

    [Test]
    [Arguments(10L, 4L, 3L, true)]   // 10 ≡ 4 (mod 3)
    [Arguments(10L, 5L, 3L, false)]
    [Arguments(7L, 7L, 5L, true)]    // a ≡ a (mod m)
    [Arguments(0L, 6L, 3L, true)]    // 0 ≡ 6 (mod 3)
    [Arguments(-1L, 2L, 3L, true)]   // -1 ≡ 2 (mod 3) under canonical residue
    [Arguments(-7L, 5L, 6L, true)]   // -7 ≡ 5 (mod 6)
    [Arguments(100L, 0L, 100L, true)] // 100 ≡ 0 (mod 100)
    public async Task IsCongruent(long a, long b, long modulus, bool expected, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(NumberTheory.IsCongruent(a, b, modulus)).IsEqualTo(expected);
    }

    [Test]
    public async Task IsCongruent_StraddlingSignedRange_NoOverflow(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // A naive (a - b) % modulus implementation overflows long for these inputs:
        // long.MaxValue - long.MinValue exceeds long.MaxValue. The canonical-residue form
        // never subtracts, so the verdict comes out correct.
        // long.MaxValue mod 10 = 7; long.MinValue mod 10 = -8 → +2 canonical → 2.
        // Not congruent, expect false.
        await Assert.That(NumberTheory.IsCongruent(long.MaxValue, long.MinValue, 10L)).IsFalse();
    }

    [Test]
    public async Task IsCongruent_ZeroModulus_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsCongruent(10L, 4L, 0L))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsCongruent_NegativeModulus_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => NumberTheory.IsCongruent(10L, 4L, -3L))
            .Throws<ArgumentOutOfRangeException>();
    }
}
