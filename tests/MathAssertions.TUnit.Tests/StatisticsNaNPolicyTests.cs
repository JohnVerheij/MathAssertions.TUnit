using System;
using System.Threading;
using System.Threading.Tasks;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins what each of the <see cref="Statistics"/> methods does with a NaN in the sample, which the documentation
/// states: the moments (mean, variance, standard deviation, sum) are poisoned by a NaN, so they compare equal
/// only to an expected NaN; median and percentile sort NaN first and read positions, so they are poisoned only
/// when a NaN lands on a position they read; the sigma checks fail.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class StatisticsNaNPolicyTests
{
    [Test]
    public async Task MeanAndVariance_NaNInSample_IsNaN(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [1.0, double.NaN, 3.0];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(double.IsNaN(mean)).IsTrue();
        await Assert.That(double.IsNaN(variance)).IsTrue();
    }

    [Test]
    public async Task MeanAndVariance_SingleNaN_ReturnsItAsTheMeanWithZeroVariance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [double.NaN];
        var (mean, variance) = Statistics.MeanAndVariance(values);
        await Assert.That(double.IsNaN(mean)).IsTrue();
        await Assert.That(variance).IsEqualTo(0.0);
    }

    [Test]
    public async Task MomentChecks_NaNInSample_MatchOnlyAnExpectedNaN(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [1.0, double.NaN, 3.0];
        await Assert.That(Statistics.HasMeanApproximately(values, 2.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasMeanApproximately(values, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasVarianceApproximately(values, 1.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasVarianceApproximately(values, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasStdDevApproximately(values, 1.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasStdDevApproximately(values, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(values, 4.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasSumApproximately(values, double.NaN, 0.0)).IsTrue();
    }

    [Test]
    public async Task HasSumApproximately_Empty_SumsToZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Statistics.HasSumApproximately(ReadOnlySpan<double>.Empty, 0.0, 0.0)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(ReadOnlySpan<double>.Empty, double.NaN, 0.0)).IsFalse();
    }

    [Test]
    public async Task HasMedianApproximately_NaNSortsFirstAndIsReadOnlyWhenItLandsOnTheMiddle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Sorted: NaN, 1, 2. The middle position holds 1.
        double[] odd = [2.0, double.NaN, 1.0];
        // Sorted: NaN, 1, 2, 3. The middle positions hold 1 and 2.
        double[] even = [3.0, 2.0, double.NaN, 1.0];
        // Sorted: NaN, NaN, 5. The middle position holds NaN.
        double[] mostlyNaN = [double.NaN, 5.0, double.NaN];
        // Sorted: NaN, NaN, 1, 2. The middle positions hold NaN and 1.
        double[] evenWithNaNInTheMiddle = [double.NaN, 2.0, double.NaN, 1.0];
        await Assert.That(Statistics.HasMedianApproximately(odd, 1.0, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(even, 1.5, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(mostlyNaN, 5.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasMedianApproximately(mostlyNaN, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(evenWithNaNInTheMiddle, 1.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasMedianApproximately(evenWithNaNInTheMiddle, double.NaN, 0.0)).IsTrue();
    }

    [Test]
    public async Task HasPercentileApproximately_NaNSortsFirst_PoisonsOnlyTheRanksThatTouchIt(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Sorted: NaN, 1, 2. Rank 0 is the NaN; rank 0.5 interpolates it; rank 1 and above read 1 and 2 only.
        double[] values = [2.0, double.NaN, 1.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 0.0, 1.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasPercentileApproximately(values, 0.0, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 25.0, 1.0, 1e9)).IsFalse();
        await Assert.That(Statistics.HasPercentileApproximately(values, 25.0, double.NaN, 0.0)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 50.0, 1.0, 0.0)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 100.0, 2.0, 0.0)).IsTrue();
    }

    [Test]
    public async Task SigmaChecks_NaNValueOrSample_Fail(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] clean = [1.0, 2.0, 3.0];
        double[] withNaN = [1.0, double.NaN, 3.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(double.NaN, clean, 100.0)).IsFalse();
        await Assert.That(Statistics.IsWithinSigmasOfMean(2.0, withNaN, 100.0)).IsFalse();
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(withNaN, 100.0)).IsFalse();
    }

    [Test]
    public async Task SigmaChecks_InfiniteSigmasOverAConstantSample_Fail(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The threshold is sigmas * standard deviation = infinity * 0 = NaN, and nothing is within NaN.
        double[] constant = [2.0, 2.0, 2.0];
        await Assert.That(Statistics.IsWithinSigmasOfMean(2.0, constant, double.PositiveInfinity)).IsFalse();
        await Assert.That(Statistics.AreAllWithinSigmasOfMean(constant, double.PositiveInfinity)).IsFalse();
        await Assert.That(Statistics.IsWithinSigmasOfMean(2.0, constant, 0.0)).IsTrue();
    }

    [Test]
    public async Task ToleranceUnits_VarianceIsInSquaredUnitsAndStdDevInTheUnitOfTheValues(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Values in meters: the standard deviation is 1 m and the variance 1 m^2; scaling by 1000 (millimeters)
        // multiplies the standard deviation by 1000 and the variance by 1,000,000, and so must the tolerance.
        double[] meters = [1.0, 2.0, 3.0];
        double[] millimeters = [1000.0, 2000.0, 3000.0];
        await Assert.That(Statistics.HasStdDevApproximately(meters, 1.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasStdDevApproximately(millimeters, 1000.0, 1e-6)).IsTrue();
        await Assert.That(Statistics.HasVarianceApproximately(meters, 1.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasVarianceApproximately(millimeters, 1_000_000.0, 1e-3)).IsTrue();
        await Assert.That(Statistics.HasMeanApproximately(millimeters, 2000.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasSumApproximately(millimeters, 6000.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasMedianApproximately(millimeters, 2000.0, 1e-9)).IsTrue();
    }

    [Test]
    public async Task PercentileScale_IsZeroToOneHundred(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] values = [10.0, 20.0, 30.0, 40.0, 50.0];
        await Assert.That(Statistics.HasPercentileApproximately(values, 50.0, 30.0, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 100.0, 50.0, 1e-9)).IsTrue();
        // 0.5 is the half-percentile, not the median: a fraction scale of 0 to 1 is not accepted as such.
        await Assert.That(Statistics.HasPercentileApproximately(values, 0.5, 10.2, 1e-9)).IsTrue();
        await Assert.That(Statistics.HasPercentileApproximately(values, 0.5, 30.0, 1e-9)).IsFalse();
    }
}
