using System.Numerics;
using MathAssertions;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent <see cref="Matrix4x4"/> assertions covering element-wise equality and the
/// matrix invariants from <see cref="LinearAlgebra"/>.
/// </summary>
/// <remarks>
/// v0.2.0 enriches the failure message for <see cref="IsApproximatelyEqualTo"/> with the
/// list of <c>[row, col]</c> cells whose absolute delta exceeded tolerance via
/// <see cref="MathFailureMessage"/>.
/// </remarks>
public static class MatrixAssertions
{
    /// <summary>Element-wise tolerance comparison across all sixteen elements.</summary>
    [GenerateAssertion(InlineMethodBody = true)]
    public static AssertionResult IsApproximatelyEqualTo(this Matrix4x4 value, Matrix4x4 expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(value, expected, tolerance)
            ? AssertionResult.Passed
            : AssertionResult.Failed(MathFailureMessage.ApproximatelyEqual(value, expected, tolerance));

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

    /// <summary>Asserts the matrix is a proper rotation (orthogonal with determinant <c>+1</c>)
    /// within tolerance. Reflections (determinant <c>-1</c>) are rejected, and so are affine matrices
    /// with a translation or a scale factor other than <c>+1</c> or <c>-1</c> beyond the tolerance;
    /// <c>Matrix4x4.CreateScale(-1, -1, 1)</c> is a 180-degree rotation and passes. The whole 4x4
    /// matrix is checked, not the affine form (last column <c>0, 0, 0, 1</c>): a 4D rotation that
    /// mixes W with X, Y or Z passes.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a proper rotation within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsRotation(this Matrix4x4 value, double tolerance)
        => LinearAlgebra.IsRotation(value, tolerance);

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
