using System.Numerics;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Matrix4x4"/> assertions covering element-wise equality and the
/// matrix invariants from <see cref="MathAssertions.LinearAlgebra"/>.
/// </summary>
public static class MatrixAssertions
{
    /// <summary>Element-wise tolerance comparison across all sixteen elements.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be approximately equal to {expected} element-wise within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsApproximatelyEqualTo(this Matrix4x4 value, Matrix4x4 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance);

    /// <summary>Asserts the matrix is symmetric (each off-diagonal pair mirror-equal) within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be symmetric within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsSymmetric(this Matrix4x4 value, double tolerance)
        => LinearAlgebra.IsSymmetric(value, tolerance);

    /// <summary>Asserts the matrix is orthogonal (<c>M*M^T = I</c>) within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be orthogonal within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsOrthogonal(this Matrix4x4 value, double tolerance)
        => LinearAlgebra.IsOrthogonal(value, tolerance);

    /// <summary>Asserts the matrix is the identity matrix within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be the identity matrix within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsIdentity(this Matrix4x4 value, double tolerance)
        => LinearAlgebra.IsIdentity(value, tolerance);

    /// <summary>Asserts the matrix determinant is approximately <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have determinant approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasDeterminantApproximately(this Matrix4x4 value, double expected, double tolerance)
        => LinearAlgebra.HasDeterminantApproximately(value, expected, tolerance);

    /// <summary>Asserts the matrix trace is approximately <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have trace approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasTraceApproximately(this Matrix4x4 value, double expected, double tolerance)
        => LinearAlgebra.HasTraceApproximately(value, expected, tolerance);

    /// <summary>Asserts the absolute determinant exceeds <paramref name="tolerance"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be invertible (|det| > tolerance {tolerance})",
        InlineMethodBody = true)]
    public static bool IsInvertible(this Matrix4x4 value, double tolerance)
        => LinearAlgebra.IsInvertible(value, tolerance);
}
