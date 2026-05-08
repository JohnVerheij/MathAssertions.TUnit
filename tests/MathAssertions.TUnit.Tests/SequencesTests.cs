using System;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the semantics of <see cref="Sequences"/>. Covers monotonicity (strict and non-strict),
/// sortedness, boundedness, arithmetic and geometric progressions, two convergence checks,
/// and the length predicates.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class SequencesTests
{
    // ----- IsMonotonicallyIncreasing -----

    [Test]
    public async Task IsMonotonicallyIncreasing_StrictlyAscending_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0];
        await Assert.That(Sequences.IsMonotonicallyIncreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsMonotonicallyIncreasing_AdjacentEqual_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 2.0, 3.0];
        await Assert.That(Sequences.IsMonotonicallyIncreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsMonotonicallyIncreasing_DecreasingPair_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 3.0, 2.0];
        await Assert.That(Sequences.IsMonotonicallyIncreasing(values)).IsFalse();
    }

    [Test]
    public async Task IsMonotonicallyIncreasing_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsMonotonicallyIncreasing(ReadOnlySpan<double>.Empty)).IsTrue();
    }

    [Test]
    public async Task IsMonotonicallyIncreasing_Single_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [42.0];
        await Assert.That(Sequences.IsMonotonicallyIncreasing(values)).IsTrue();
    }

    // ----- IsMonotonicallyDecreasing -----

    [Test]
    public async Task IsMonotonicallyDecreasing_StrictlyDescending_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 2.0, 1.0];
        await Assert.That(Sequences.IsMonotonicallyDecreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsMonotonicallyDecreasing_AdjacentEqual_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 2.0, 2.0, 1.0];
        await Assert.That(Sequences.IsMonotonicallyDecreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsMonotonicallyDecreasing_IncreasingPair_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 1.0, 2.0];
        await Assert.That(Sequences.IsMonotonicallyDecreasing(values)).IsFalse();
    }

    // ----- IsStrictlyMonotonicallyIncreasing -----

    [Test]
    public async Task IsStrictlyMonotonicallyIncreasing_StrictlyAscending_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0];
        await Assert.That(Sequences.IsStrictlyMonotonicallyIncreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsStrictlyMonotonicallyIncreasing_AdjacentEqual_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 2.0, 3.0];
        await Assert.That(Sequences.IsStrictlyMonotonicallyIncreasing(values)).IsFalse();
    }

    [Test]
    public async Task IsStrictlyMonotonicallyIncreasing_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsStrictlyMonotonicallyIncreasing(ReadOnlySpan<double>.Empty)).IsTrue();
    }

    // ----- IsStrictlyMonotonicallyDecreasing -----

    [Test]
    public async Task IsStrictlyMonotonicallyDecreasing_StrictlyDescending_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 2.0, 1.0];
        await Assert.That(Sequences.IsStrictlyMonotonicallyDecreasing(values)).IsTrue();
    }

    [Test]
    public async Task IsStrictlyMonotonicallyDecreasing_AdjacentEqual_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 2.0, 2.0, 1.0];
        await Assert.That(Sequences.IsStrictlyMonotonicallyDecreasing(values)).IsFalse();
    }

    // ----- IsSorted -----

    [Test]
    public async Task IsSorted_AscendingSequence_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 2.0, 3.0];
        await Assert.That(Sequences.IsSorted(values)).IsTrue();
    }

    [Test]
    public async Task IsSorted_Unsorted_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [3.0, 1.0, 2.0];
        await Assert.That(Sequences.IsSorted(values)).IsFalse();
    }

    // ----- IsBounded -----

    [Test]
    public async Task IsBounded_AllInsideRange_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 5.0, 10.0];
        await Assert.That(Sequences.IsBounded(values, 0.0, 10.0)).IsTrue();
    }

    [Test]
    public async Task IsBounded_BoundariesIncluded_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [0.0, 5.0, 10.0];
        await Assert.That(Sequences.IsBounded(values, 0.0, 10.0)).IsTrue();
    }

    [Test]
    public async Task IsBounded_OneAboveMax_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 5.0, 11.0];
        await Assert.That(Sequences.IsBounded(values, 0.0, 10.0)).IsFalse();
    }

    [Test]
    public async Task IsBounded_OneBelowMin_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [-1.0, 5.0, 10.0];
        await Assert.That(Sequences.IsBounded(values, 0.0, 10.0)).IsFalse();
    }

    [Test]
    public async Task IsBounded_NaNValue_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, double.NaN, 10.0];
        await Assert.That(Sequences.IsBounded(values, 0.0, 10.0)).IsFalse();
    }

    [Test]
    public async Task IsBounded_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsBounded(ReadOnlySpan<double>.Empty, 0.0, 10.0)).IsTrue();
    }

    [Test]
    public async Task IsBounded_MaxLessThanMin_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Sequences.IsBounded(values, 10.0, 0.0);
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task IsBounded_NaNMin_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // NaN bounds are rejected at entry. Without this guard, every comparison against
        // NaN returns false under IEEE 754 and any sequence (including the empty span)
        // would pass the bound check vacuously, silently inverting the method's contract.
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Sequences.IsBounded(values, double.NaN, 10.0);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsBounded_NaNMax_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Sequences.IsBounded(values, 0.0, double.NaN);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsBounded_NaNMin_ThrowsEvenOnEmptySpan(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Validation must happen before the loop so the caller sees the failure even when
        // the sequence is empty (no value would have triggered an in-loop check).
        await Assert.That(() => Sequences.IsBounded(ReadOnlySpan<double>.Empty, double.NaN, 10.0))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsBounded_NaNMax_ThrowsEvenOnEmptySpan(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Symmetric to the NaN-min variant; pins both validation legs against
        // ordering regressions that might let one fire after the loop while the other
        // fires before it.
        await Assert.That(() => Sequences.IsBounded(ReadOnlySpan<double>.Empty, 0.0, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsArithmeticProgression -----

    [Test]
    public async Task IsArithmeticProgression_CommonDifference_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 3.0, 5.0, 7.0];
        await Assert.That(Sequences.IsArithmeticProgression(values, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsArithmeticProgression_NegativeDifference_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [10.0, 7.0, 4.0, 1.0];
        await Assert.That(Sequences.IsArithmeticProgression(values, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsArithmeticProgression_BrokenStep_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 3.0, 5.0, 99.0];
        await Assert.That(Sequences.IsArithmeticProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsArithmeticProgression_ShortSequence_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsArithmeticProgression(ReadOnlySpan<double>.Empty, 1e-9)).IsTrue();
        ReadOnlySpan<double> single = [5.0];
        await Assert.That(Sequences.IsArithmeticProgression(single, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsArithmeticProgression_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Sequences.IsArithmeticProgression(values, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsGeometricProgression -----

    [Test]
    public async Task IsGeometricProgression_CommonRatio_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 4.0, 8.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsGeometricProgression_FractionalRatio_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [16.0, 8.0, 4.0, 2.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsGeometricProgression_BrokenRatio_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 4.0, 99.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsGeometricProgression_LeadingZero_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [0.0, 2.0, 4.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsGeometricProgression_LeadingNegativeZero_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // -0 has a different bit pattern from +0 but the same magnitude. The bit-magnitude
        // mask used internally must reject both signs of zero.
        ReadOnlySpan<double> values = [-0.0, 2.0, 4.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsGeometricProgression_InteriorZero_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 0.0, 8.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsGeometricProgression_InteriorZeroAtDivisorPosition_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // values[1] is zero, so the loop's first iteration (i=2) hits the divisor-zero
        // short-circuit before computing any ratio. Distinct from InteriorZero_False, which
        // exits via the tolerance-mismatch path on the same iteration.
        ReadOnlySpan<double> values = [1.0, 0.0, 5.0];
        await Assert.That(Sequences.IsGeometricProgression(values, 1e-9)).IsFalse();
    }

    [Test]
    public async Task IsGeometricProgression_ShortSequence_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsGeometricProgression(ReadOnlySpan<double>.Empty, 1e-9)).IsTrue();
        ReadOnlySpan<double> single = [5.0];
        await Assert.That(Sequences.IsGeometricProgression(single, 1e-9)).IsTrue();
    }

    [Test]
    public async Task IsGeometricProgression_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Sequences.IsGeometricProgression(values, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- ConvergesTo -----

    [Test]
    public async Task ConvergesTo_LastValueAtLimit_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [10.0, 5.0, 2.5, 1.001];
        await Assert.That(Sequences.ConvergesTo(values, 1.0, 0.01)).IsTrue();
    }

    [Test]
    public async Task ConvergesTo_LastValueAwayFromLimit_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [10.0, 5.0, 2.5, 99.0];
        await Assert.That(Sequences.ConvergesTo(values, 1.0, 0.01)).IsFalse();
    }

    [Test]
    public async Task ConvergesTo_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.ConvergesTo(ReadOnlySpan<double>.Empty, 1.0, 0.01)).IsFalse();
    }

    [Test]
    public async Task ConvergesTo_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Sequences.ConvergesTo(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConvergesTo_NegativeTolerance_ThrowsEvenOnEmptySpan(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pins "tolerance is validated before the empty-span early-return." Without the
        // ordering, an empty span with an invalid tolerance would silently return false
        // and the caller would never see the validation failure.
        await Assert.That(() => Sequences.ConvergesTo(ReadOnlySpan<double>.Empty, 1.0, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsCauchyConvergent -----

    [Test]
    public async Task IsCauchyConvergent_TightTail_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [10.0, 5.0, 2.0, 1.0001, 1.0];
        await Assert.That(Sequences.IsCauchyConvergent(values, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsCauchyConvergent_LooseTail_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 99.0];
        await Assert.That(Sequences.IsCauchyConvergent(values, 0.001)).IsFalse();
    }

    [Test]
    public async Task IsCauchyConvergent_ShortSequence_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Sequences.IsCauchyConvergent(ReadOnlySpan<double>.Empty, 0.001)).IsTrue();
        ReadOnlySpan<double> single = [5.0];
        await Assert.That(Sequences.IsCauchyConvergent(single, 0.001)).IsTrue();
    }

    [Test]
    public async Task IsCauchyConvergent_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Sequences.IsCauchyConvergent(values, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- ValidateTolerance NaN branch -----

    [Test]
    public async Task IsArithmeticProgression_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Sequences.IsArithmeticProgression(values, double.NaN);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasLength / HasMinLength -----

    [Test]
    public async Task HasLength_ExactMatch_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<int> values = [1, 2, 3];
        await Assert.That(Sequences.HasLength(values, 3)).IsTrue();
    }

    [Test]
    public async Task HasLength_Mismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<int> values = [1, 2, 3];
        await Assert.That(Sequences.HasLength(values, 5)).IsFalse();
    }

    [Test]
    public async Task HasMinLength_AtMinimum_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<int> values = [1, 2, 3];
        await Assert.That(Sequences.HasMinLength(values, 3)).IsTrue();
    }

    [Test]
    public async Task HasMinLength_AboveMinimum_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<int> values = [1, 2, 3, 4];
        await Assert.That(Sequences.HasMinLength(values, 3)).IsTrue();
    }

    [Test]
    public async Task HasMinLength_BelowMinimum_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<int> values = [1, 2];
        await Assert.That(Sequences.HasMinLength(values, 3)).IsFalse();
    }

    [Test]
    public async Task HasLength_NegativeExpected_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Span.Length is non-negative; a negative expected is always a caller bug, not a
        // useful "no value will match" sentinel. Failing fast surfaces the typo.
        await Assert.That(() =>
        {
            ReadOnlySpan<int> values = [1];
            return Sequences.HasLength(values, -1);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasMinLength_NegativeExpected_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Without the guard, HasMinLength(values, -1) is trivially true for any span
        // (Length >= -1 always), masking caller-side typos.
        await Assert.That(() =>
        {
            ReadOnlySpan<int> values = [1];
            return Sequences.HasMinLength(values, -1);
        }).Throws<ArgumentOutOfRangeException>();
    }
}
