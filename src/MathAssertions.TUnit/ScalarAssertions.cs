using System;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent scalar (<see cref="double"/> / <see cref="float"/>) assertions delegating to
/// <see cref="MathAssertions.MathTolerance"/>. The wrappers exist so consumers can write
/// <c>await Assert.That(value).IsApproximatelyEqualTo(expected, tolerance)</c> rather than
/// <c>await Assert.That(MathTolerance.IsApproximatelyEqual(value, expected, tolerance)).IsTrue()</c>.
/// </summary>
public static class ScalarAssertions
{
    /// <summary>NaN- and infinity-aware tolerance comparison for <see cref="double"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this double value, double expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>NaN- and infinity-aware tolerance comparison for <see cref="float"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this float value, float expected, float tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>
    /// Angle comparison in degrees on the circle that wraps every <c>360</c> degrees, for
    /// <see cref="double"/>: <c>359</c> and <c>1</c> are <c>2</c> degrees apart and <c>180</c> equals
    /// <c>-180</c>. A NaN or infinite angle never matches. The failure shows the wrapped difference.
    /// See <see cref="MathTolerance.AngleDifferenceDegrees(double, double)"/>.
    /// </summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsAngleApproximatelyEqualTo(this double value, double expected, double toleranceDegrees)
        => MathTolerance.IsAngleApproximatelyEqual(value, expected, toleranceDegrees)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.AngleApproximatelyEqual(value, expected, toleranceDegrees));

    /// <summary>
    /// Angle comparison in degrees on the circle that wraps every <c>360</c> degrees, for
    /// <see cref="float"/>. Same rules as the <see cref="double"/> overload.
    /// </summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsAngleApproximatelyEqualTo(this float value, float expected, float toleranceDegrees)
        => MathTolerance.IsAngleApproximatelyEqual(value, expected, toleranceDegrees)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.AngleApproximatelyEqual(value, expected, toleranceDegrees));

    /// <summary>ULP-distance equality for <see cref="double"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be within {ulpDistance} ULPs of {expected}",
        InlineMethodBody = true)]
    public static bool IsCloseInUlpsTo(this double value, double expected, long ulpDistance)
        => MathTolerance.IsCloseInUlps(value, expected, ulpDistance);

    /// <summary>ULP-distance equality for <see cref="float"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be within {ulpDistance} ULPs of {expected}",
        InlineMethodBody = true)]
    public static bool IsCloseInUlpsTo(this float value, float expected, int ulpDistance)
        => MathTolerance.IsCloseInUlps(value, expected, ulpDistance);

    /// <summary>Combined relative + absolute tolerance equality for <see cref="double"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be within relative tolerance {relativeTolerance} or absolute tolerance {absoluteTolerance} of {expected}",
        InlineMethodBody = true)]
    public static bool IsRelativelyAndAbsolutelyCloseTo(
        this double value,
        double expected,
        double relativeTolerance,
        double absoluteTolerance)
        => MathTolerance.IsRelativelyAndAbsolutelyClose(value, expected, relativeTolerance, absoluteTolerance);

    // IsFinite() is already provided by TUnit's built-in DoubleAssertionExtensions and
    // SingleAssertionExtensions on IAssertionSource<double>/<float>; consumers reach
    // those directly via Assert.That(value).IsFinite(). MathTolerance.IsFinite remains
    // available as a static predicate from the core package for non-fluent contexts.

    /// <summary>Asserts the value is finite and non-negative.</summary>
    [GenerateAssertion(ExpectationMessage = "to be a finite non-negative value", InlineMethodBody = true)]
    public static bool IsNonNegativeFinite(this double value) => MathTolerance.IsNonNegativeFinite(value);

    /// <summary>Asserts the value is finite and in the closed interval <c>[0, 1]</c>.</summary>
    [GenerateAssertion(ExpectationMessage = "to be a probability (finite, in [0, 1])", InlineMethodBody = true)]
    public static bool IsProbability(this double value) => MathTolerance.IsProbability(value);

    /// <summary>Asserts the value is finite and in the closed interval <c>[0, 100]</c>.</summary>
    [GenerateAssertion(ExpectationMessage = "to be a percentage (finite, in [0, 100])", InlineMethodBody = true)]
    public static bool IsPercentage(this double value) => MathTolerance.IsPercentage(value);

    /// <summary>
    /// Asserts the roundtrip <c>inverse(forward(value))</c> recovers <paramref name="value"/>
    /// within <paramref name="tolerance"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to roundtrip through forward/inverse within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasRoundtripIdentity(
        this double value,
        Func<double, double> forward,
        Func<double, double> inverse,
        double tolerance)
        => MathTolerance.HasRoundtripIdentity(value, forward, inverse, tolerance);
}
