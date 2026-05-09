using System;
using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="MathAssertions.LinearAlgebra"/> assertions for
/// <see cref="Vector3"/> pair properties and <see cref="Vector3"/>-array linear
/// independence. Matrix-side invariants live on
/// <see cref="MatrixAssertions"/>.
/// </summary>
public static class LinearAlgebraAssertions
{
    /// <summary>Asserts <paramref name="value"/> and <paramref name="other"/> are orthogonal within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be orthogonal to {other} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsOrthogonalTo(this Vector3 value, Vector3 other, double tolerance)
        => LinearAlgebra.AreOrthogonal(value, other, tolerance);

    /// <summary>Asserts <paramref name="value"/> and <paramref name="other"/> are parallel within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be parallel to {other} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsParallelTo(this Vector3 value, Vector3 other, double tolerance)
        => LinearAlgebra.AreParallel(value, other, tolerance);

    /// <summary>
    /// Asserts the supplied vectors are linearly independent in <c>R^3</c> within
    /// tolerance. Spans of four or more vectors are always dependent.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be linearly independent within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool AreLinearlyIndependent(this Vector3[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return LinearAlgebra.AreLinearlyIndependent((ReadOnlySpan<Vector3>)value, tolerance);
    }
}
