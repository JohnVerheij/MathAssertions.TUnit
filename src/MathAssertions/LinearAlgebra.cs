using System;
using System.Numerics;

namespace MathAssertions;

/// <summary>
/// Linear-algebra invariants over <see cref="Matrix4x4"/> and <see cref="Vector3"/>:
/// matrix symmetry, orthogonality, identity, determinant, trace, invertibility, plus
/// vector-pair orthogonality and parallelism and the
/// <see cref="AreLinearlyIndependent(ReadOnlySpan{Vector3}, double)"/> triple-product
/// check for sets of up to three vectors in <c>R^3</c>.
/// </summary>
/// <remarks>
/// 0.1.0 Cluster 5. Sits alongside <see cref="MathTolerance"/>, <see cref="Sequences"/>,
/// and <see cref="Statistics"/> in the same package; the <c>MathAssertions.TUnit</c>
/// adapter delegates to these helpers from its <c>[GenerateAssertion]</c> extensions.
/// </remarks>
public static class LinearAlgebra
{
    /// <summary>
    /// Returns <see langword="true"/> when the matrix is symmetric: every off-diagonal
    /// element equals its transpose partner within tolerance, that is
    /// <c>m[i, j] = m[j, i]</c> for all <c>i != j</c>.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Maximum allowed absolute difference between mirrored
    /// off-diagonal elements. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the matrix is symmetric within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsSymmetric(Matrix4x4 m, double tolerance)
    {
        ValidateTolerance(tolerance);

        for (var row = 0; row < 4; row++)
        {
            for (var col = row + 1; col < 4; col++)
            {
                if (!MathTolerance.IsApproximatelyEqual((double)m[row, col], (double)m[col, row], tolerance))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the matrix is orthogonal: <c>M * M^T = I</c>
    /// within tolerance. Orthogonal matrices preserve angles and lengths and have
    /// determinant <c>+/- 1</c>.
    /// </summary>
    /// <remarks>
    /// Translation matrices are not orthogonal because the translation column makes the
    /// product <c>M * M^T</c> deviate from identity in the off-diagonals. A pure rotation
    /// is orthogonal; rotations composed with uniform scaling are not (the diagonal of
    /// <c>M * M^T</c> picks up the squared scale).
    /// </remarks>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Maximum allowed absolute difference between
    /// <c>M * M^T</c> and the identity per element. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the matrix is orthogonal within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsOrthogonal(Matrix4x4 m, double tolerance)
    {
        ValidateTolerance(tolerance);

        var transposed = Matrix4x4.Transpose(m);
        var product = Matrix4x4.Multiply(m, transposed);
        return MathTolerance.IsApproximatelyEqual(product, Matrix4x4.Identity, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the matrix equals the
    /// <see cref="Matrix4x4.Identity"/> matrix element-wise within tolerance.
    /// Convenience wrapper over <see cref="MathTolerance.IsApproximatelyEqual(Matrix4x4, Matrix4x4, double)"/>
    /// with the identity as the second operand.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the matrix is approximately the identity.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsIdentity(Matrix4x4 m, double tolerance)
    {
        ValidateTolerance(tolerance);

        return MathTolerance.IsApproximatelyEqual(m, Matrix4x4.Identity, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the matrix determinant is within
    /// <paramref name="tolerance"/> of <paramref name="expected"/>. Delegates the
    /// computation to <see cref="Matrix4x4.GetDeterminant"/>.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="expected">Expected determinant value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the determinant is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasDeterminantApproximately(Matrix4x4 m, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        return MathTolerance.IsApproximatelyEqual((double)m.GetDeterminant(), expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the matrix trace (sum of diagonal elements)
    /// is within <paramref name="tolerance"/> of <paramref name="expected"/>.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="expected">Expected trace value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if the trace is approximately
    /// <paramref name="expected"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasTraceApproximately(Matrix4x4 m, double expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        var trace = (double)m.M11 + (double)m.M22 + (double)m.M33 + (double)m.M44;
        return MathTolerance.IsApproximatelyEqual(trace, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the absolute value of the determinant exceeds
    /// <paramref name="tolerance"/>. The threshold expresses "the matrix is far enough
    /// from singular to invert numerically"; choose a tolerance that reflects the
    /// expected condition number of the inputs.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Minimum acceptable absolute determinant for the matrix to
    /// be considered invertible. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if <c>|det(M)| &gt; tolerance</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsInvertible(Matrix4x4 m, double tolerance)
    {
        ValidateTolerance(tolerance);

        return Math.Abs((double)m.GetDeterminant()) > tolerance;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the two vectors are orthogonal:
    /// their dot product is within <paramref name="tolerance"/> of zero.
    /// </summary>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <param name="tolerance">Maximum allowed absolute deviation of the dot product
    /// from zero. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if <c>u . v ~ 0</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool AreOrthogonal(Vector3 u, Vector3 v, double tolerance)
    {
        ValidateTolerance(tolerance);

        return MathTolerance.IsApproximatelyEqual((double)Vector3.Dot(u, v), 0.0, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the two vectors are parallel (or anti-parallel):
    /// the magnitude of their cross product is within <paramref name="tolerance"/> of zero.
    /// A zero vector is treated as parallel to every other vector under this definition
    /// because <c>0 x v = 0</c> for any <c>v</c>.
    /// </summary>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <param name="tolerance">Maximum allowed absolute deviation of the cross-product
    /// magnitude from zero. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if <c>|u x v| ~ 0</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool AreParallel(Vector3 u, Vector3 v, double tolerance)
    {
        ValidateTolerance(tolerance);

        var cross = Vector3.Cross(u, v);
        return MathTolerance.IsApproximatelyEqual((double)cross.Length(), 0.0, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the supplied vectors are linearly independent
    /// in <c>R^3</c>. Up to three vectors can be linearly independent; spans of four or
    /// more vectors in <c>R^3</c> are always dependent and return <see langword="false"/>.
    /// The empty span is vacuously independent and returns <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Length-1: the single vector is independent iff its length exceeds tolerance.
    /// Length-2: equivalent to <see cref="AreParallel(Vector3, Vector3, double)"/>
    /// returning <see langword="false"/>.
    /// Length-3: triple-product test
    /// <c>|v1 . (v2 x v3)| &gt; tolerance</c> — the absolute scalar triple product is the
    /// volume of the parallelepiped spanned by the three vectors and is non-zero iff the
    /// three are linearly independent.
    /// </para>
    /// <para>
    /// The length-1 case compares vector length directly against tolerance rather than
    /// length-squared against tolerance-squared, so the verdict is well-defined for
    /// extreme tolerance magnitudes (where squaring would underflow or overflow).
    /// </para>
    /// </remarks>
    /// <param name="vectors">Vectors to test.</param>
    /// <param name="tolerance">Threshold for the length and triple-product checks. Must
    /// be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the vectors are linearly independent.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool AreLinearlyIndependent(ReadOnlySpan<Vector3> vectors, double tolerance)
    {
        ValidateTolerance(tolerance);

        switch (vectors.Length)
        {
            case 0:
                return true;
            case 1:
                return (double)vectors[0].Length() > tolerance;
            case 2:
                return !AreParallel(vectors[0], vectors[1], tolerance);
            case 3:
                {
                    var det = Vector3.Dot(vectors[0], Vector3.Cross(vectors[1], vectors[2]));
                    return Math.Abs((double)det) > tolerance;
                }
            default:
                return false;
        }
    }

    private static void ValidateTolerance(double tolerance)
    {
        if (double.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");
        }
    }
}
