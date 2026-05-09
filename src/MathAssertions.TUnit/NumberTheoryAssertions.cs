using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="MathAssertions.NumberTheory"/> assertions over <see cref="long"/>
/// integers. The non-predicate helpers (<c>GreatestCommonDivisor</c>,
/// <c>LeastCommonMultiple</c>) are not exposed as fluent assertions in 0.1.0; consumers
/// can call them statically and apply <see cref="ScalarAssertions"/> on the result.
/// </summary>
public static class NumberTheoryAssertions
{
    /// <summary>Asserts <paramref name="value"/> is exactly divisible by <paramref name="divisor"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be divisible by {divisor}",
        InlineMethodBody = true)]
    public static bool IsDivisibleBy(this long value, long divisor)
        => NumberTheory.IsDivisibleBy(value, divisor);

    /// <summary>Asserts <paramref name="value"/> is prime.</summary>
    [GenerateAssertion(ExpectationMessage = "to be prime", InlineMethodBody = true)]
    public static bool IsPrime(this long value) => NumberTheory.IsPrime(value);

    /// <summary>Asserts <paramref name="value"/> and <paramref name="other"/> share no common divisor other than 1.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be coprime with {other}",
        InlineMethodBody = true)]
    public static bool IsCoprimeWith(this long value, long other)
        => NumberTheory.AreCoprime(value, other);

    /// <summary>
    /// Asserts <paramref name="value"/> is a non-negative integer power of
    /// <paramref name="baseValue"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a power of {baseValue}",
        InlineMethodBody = true)]
    public static bool IsPowerOf(this long value, long baseValue)
        => NumberTheory.IsPowerOf(value, baseValue);

    /// <summary>Asserts <paramref name="value"/> is a non-negative perfect square.</summary>
    [GenerateAssertion(ExpectationMessage = "to be a perfect square", InlineMethodBody = true)]
    public static bool IsPerfectSquare(this long value) => NumberTheory.IsPerfectSquare(value);

    /// <summary>
    /// Asserts <paramref name="value"/> is congruent to <paramref name="other"/> modulo
    /// <paramref name="modulus"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be congruent to {other} modulo {modulus}",
        InlineMethodBody = true)]
    public static bool IsCongruentTo(this long value, long other, long modulus)
        => NumberTheory.IsCongruent(value, other, modulus);
}
