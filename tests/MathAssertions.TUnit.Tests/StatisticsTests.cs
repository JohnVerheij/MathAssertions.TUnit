using System;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the semantics of <see cref="Statistics"/>. Reference dataset <c>[1, 2, 3, 4, 5]</c>
/// has known mean <c>3.0</c>, unbiased sample variance <c>2.5</c>, sample standard
/// deviation <c>~1.5811</c>, sum <c>15</c>, median <c>3.0</c>; reused throughout to keep
/// expected values verifiable from a single textbook reference.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class StatisticsTests
{
    // ----- MeanAndVariance -----

    [Test]
    public async Task MeanAndVariance_ReferenceDataset_ReturnsKnownMoments(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(mean).IsEqualTo(3.0).Within(1e-12);
        await Assert.That(variance).IsEqualTo(2.5).Within(1e-12);
    }

    [Test]
    public async Task MeanAndVariance_Empty_ReturnsNaNNaN(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var (mean, variance) = Statistics.MeanAndVariance(ReadOnlySpan<double>.Empty);
        await Assert.That(double.IsNaN(mean)).IsTrue();
        await Assert.That(double.IsNaN(variance)).IsTrue();
    }

    [Test]
    public async Task MeanAndVariance_Single_ReturnsValueAndZeroVariance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [42.0];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(mean).IsEqualTo(42.0).Within(1e-12);
        await Assert.That(variance).IsEqualTo(0.0).Within(1e-12);
    }

    [Test]
    public async Task MeanAndVariance_AllEqual_ZeroVariance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [7.0, 7.0, 7.0, 7.0];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(mean).IsEqualTo(7.0).Within(1e-12);
        await Assert.That(variance).IsEqualTo(0.0).Within(1e-12);
    }

    [Test]
    public async Task MeanAndVariance_NegativeValues_HandledCorrectly(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // [-2, -1, 0, 1, 2]: mean = 0, variance = 2.5 (sum of squares 10, divided by N-1=4)
        ReadOnlySpan<double> values = [-2.0, -1.0, 0.0, 1.0, 2.0];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(mean).IsEqualTo(0.0).Within(1e-12);
        await Assert.That(variance).IsEqualTo(2.5).Within(1e-12);
    }

    // ----- HasMeanApproximately -----

    [Test]
    public async Task HasMeanApproximately_ExpectedMatches_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasMeanApproximately(values, 3.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasMeanApproximately_ExpectedDoesNotMatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasMeanApproximately(values, 99.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasMeanApproximately_Single_UsesSoleValueAsMean(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [42.0];
        await Assert.That(Statistics.HasMeanApproximately(values, 42.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasMeanApproximately_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.HasMeanApproximately(ReadOnlySpan<double>.Empty, 0.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasMeanApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasMeanApproximately(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasMeanApproximately_NaNTolerance_ThrowsEvenOnEmpty(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Tolerance must be validated before the empty-span early-return; same parity
        // pattern as Sequences.ConvergesTo.
        await Assert.That(() => Statistics.HasMeanApproximately(ReadOnlySpan<double>.Empty, 0.0, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasVarianceApproximately -----

    [Test]
    public async Task HasVarianceApproximately_ExpectedMatches_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasVarianceApproximately(values, 2.5, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasVarianceApproximately_ExpectedDoesNotMatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasVarianceApproximately(values, 99.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasVarianceApproximately_AllEqual_ZeroVariance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [5.0, 5.0, 5.0];
        await Assert.That(Statistics.HasVarianceApproximately(values, 0.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasVarianceApproximately_Single_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Sample variance is undefined for one observation under the unbiased N-1 convention.
        ReadOnlySpan<double> values = [42.0];
        await Assert.That(Statistics.HasVarianceApproximately(values, 0.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasVarianceApproximately_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.HasVarianceApproximately(ReadOnlySpan<double>.Empty, 0.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasVarianceApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Statistics.HasVarianceApproximately(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasStdDevApproximately -----

    [Test]
    public async Task HasStdDevApproximately_ExpectedMatches_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        // sqrt(2.5) ~= 1.5811388300841898
        await Assert.That(Statistics.HasStdDevApproximately(values, Math.Sqrt(2.5), 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasStdDevApproximately_ExpectedDoesNotMatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasStdDevApproximately(values, 99.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasStdDevApproximately_Single_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [42.0];
        await Assert.That(Statistics.HasStdDevApproximately(values, 0.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasStdDevApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Statistics.HasStdDevApproximately(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasSumApproximately -----

    [Test]
    public async Task HasSumApproximately_ExpectedMatches_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasSumApproximately(values, 15.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasSumApproximately_NegativeValues(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [-2.0, -1.0, 0.0, 1.0, 2.0];
        await Assert.That(Statistics.HasSumApproximately(values, 0.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasSumApproximately_Empty_TreatsSumAsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Identity element for addition: empty sum is 0.
        await Assert.That(Statistics.HasSumApproximately(ReadOnlySpan<double>.Empty, 0.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(ReadOnlySpan<double>.Empty, 1.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasSumApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasSumApproximately(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasMedianApproximately -----

    [Test]
    public async Task HasMedianApproximately_OddLength_TakesMiddleValue(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasMedianApproximately(values, 3.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasMedianApproximately_EvenLength_AveragesTwoMiddleValues(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0];
        await Assert.That(Statistics.HasMedianApproximately(values, 2.5, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasMedianApproximately_Unsorted_StillCorrect(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Implementation must sort a copy; same input as the odd-length test, shuffled.
        ReadOnlySpan<double> values = [5.0, 1.0, 3.0, 2.0, 4.0];
        await Assert.That(Statistics.HasMedianApproximately(values, 3.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasMedianApproximately_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.HasMedianApproximately(ReadOnlySpan<double>.Empty, 0.0, 1e-9)).IsFalse();
    }

    [Test]
    public async Task HasMedianApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasMedianApproximately(values, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasPercentileApproximately -----

    [Test]
    public async Task HasPercentileApproximately_FiftiethEqualsMedian(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 50.0, 3.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_ZerothIsMin(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 0.0, 1.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_HundredthIsMax(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 100.0, 5.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_LinearInterpolationBetweenRanks(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Length 5 -> rank for 25th percentile is 0.25 * 4 = 1.0 -> values[1] = 2.0 exactly.
        // Pick 30th percentile to exercise the interpolation branch:
        // rank = 0.3 * 4 = 1.2 -> values[1] + 0.2 * (values[2] - values[1]) = 2 + 0.2 * 1 = 2.2.
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 30.0, 2.2, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_Unsorted_StillCorrect(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [5.0, 1.0, 3.0, 2.0, 4.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 50.0, 3.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.HasPercentileApproximately(ReadOnlySpan<double>.Empty, 50.0, 0.0, 1e-9))
            .IsFalse();
    }

    [Test]
    public async Task HasPercentileApproximately_NegativePercentile_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasPercentileApproximately(values, -1.0, 1.0, 1e-9);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasPercentileApproximately_PercentileAbove100_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasPercentileApproximately(values, 101.0, 1.0, 1e-9);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasPercentileApproximately_NaNPercentile_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasPercentileApproximately(values, double.NaN, 1.0, 1e-9);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task HasPercentileApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0];
            return Statistics.HasPercentileApproximately(values, 50.0, 1.0, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsWithinSigmasOfMean -----

    [Test]
    public async Task IsWithinSigmasOfMean_ValueAtMean_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> sample = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(3.0, sample, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsWithinSigmasOfMean_ValueWithinOneSigma_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // mean=3, stddev=sqrt(2.5)~=1.581. value=4 -> |4-3|=1 <= 1*stddev. Within 1 sigma.
        ReadOnlySpan<double> sample = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(4.0, sample, 1.0)).IsTrue();
    }

    [Test]
    public async Task IsWithinSigmasOfMean_OutlierBeyondTwoSigma_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 2 sigma envelope ~ [3 - 3.16, 3 + 3.16] = [-0.16, 6.16]. value=10 is well outside.
        ReadOnlySpan<double> sample = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(10.0, sample, 2.0)).IsFalse();
    }

    [Test]
    public async Task IsWithinSigmasOfMean_Single_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> sample = [42.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(42.0, sample, 1.0)).IsFalse();
    }

    [Test]
    public async Task IsWithinSigmasOfMean_NegativeSigmas_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> sample = [1.0, 2.0];
            return Statistics.IsWithinSigmasOfMean(1.0, sample, -1.0);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsWithinSigmasOfMean_NaNSigmas_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> sample = [1.0, 2.0];
            return Statistics.IsWithinSigmasOfMean(1.0, sample, double.NaN);
        }).Throws<ArgumentOutOfRangeException>();
    }

    // ----- AreAllWithinSigmasOfMean -----

    [Test]
    public async Task AreAllWithinSigmasOfMean_TightDistribution_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 3-sigma envelope swallows the entire reference distribution; the sample's max
        // absolute deviation from its own mean is 2 (values 1 and 5 vs mean 3), well
        // within 3 * sqrt(2.5) ~= 4.74.
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 3.0)).IsTrue();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_OutlierAtZeroSigmas_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // sigmas=0 means every value must equal the mean exactly. The reference dataset
        // has dispersion, so this fails on the first non-mean value.
        ReadOnlySpan<double> values = [1.0, 2.0, 3.0, 4.0, 5.0];
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 0.0)).IsFalse();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_AllEqualAtZeroSigmas_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // All values equal -> stddev = 0 -> every value at the mean -> 0-sigma envelope holds.
        ReadOnlySpan<double> values = [7.0, 7.0, 7.0, 7.0];
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 0.0)).IsTrue();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_Single_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<double> values = [42.0];
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(values, 1.0)).IsFalse();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(ReadOnlySpan<double>.Empty, 1.0)).IsFalse();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_NegativeSigmas_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<double> values = [1.0, 2.0];
            return Statistics.AreAllWithinSigmasOfMean(values, -1.0);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AreAllWithinSigmasOfMean_NaNSigmas_ThrowsEvenOnEmpty(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Validation must run before the empty-span early-return (parity with the family-wide
        // ConvergesTo / IsBounded pattern surfaced in Cluster 3).
        await Assert.That(() => Statistics.AreAllWithinSigmasOfMean(ReadOnlySpan<double>.Empty, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }
}
