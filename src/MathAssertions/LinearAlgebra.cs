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
                if (!MathTolerance.IsApproximatelyEqual(m[row, col], m[col, row], tolerance))
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
    /// A translation shows up in the last row and column of <c>M * M^T</c>: its components,
    /// multiplied by the upper 3x3 block, off the diagonal, and <c>1</c> plus its squared length on
    /// the diagonal. The matrix stays orthogonal only while all of those are within the tolerance of
    /// the identity. A pure rotation is orthogonal; composed with a uniform scale <c>s</c> it is
    /// orthogonal only when <c>s^2</c> is <c>1</c> within the tolerance (the diagonal of
    /// <c>M * M^T</c> picks up <c>s^2</c>), so a scale of <c>-1</c> keeps it orthogonal. The check
    /// covers the whole 4x4 matrix and does not check the affine form (last column
    /// <c>0, 0, 0, 1</c>), so these rules hold for affine matrices: a 4D rotation that mixes W with
    /// X, Y or Z is orthogonal although its translation slots (<c>M41</c> to <c>M43</c>) are not zero.
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
    /// Returns <see langword="true"/> when the matrix is a proper rotation in the affine form of
    /// <see cref="Matrix4x4"/>: three checks that must all pass, each within tolerance. It is
    /// orthogonal (<c>M * M^T = I</c>, so it preserves lengths and angles), its last column is
    /// <c>0, 0, 0, 1</c> (<c>M14</c>, <c>M24</c> and <c>M34</c> within the tolerance of <c>0</c>,
    /// <c>M44</c> within the tolerance of <c>1</c>), <em>and</em> it has determinant <c>+1</c>. The
    /// determinant check rules out reflections (an improper orthogonal matrix has determinant
    /// <c>-1</c>) under any tolerance below <c>2</c>. A matrix with a translation, or with a scale
    /// factor other than <c>+1</c> or <c>-1</c>, fails unless the checks stay within the tolerance:
    /// <c>Matrix4x4.CreateScale(2)</c> passes the orthogonality check at a tolerance of <c>3</c>
    /// and fails the determinant check (<c>8</c>). A sign-flip scale such as
    /// <c>Matrix4x4.CreateScale(-1, -1, 1)</c> is a 180-degree rotation and passes. A 4D rotation
    /// that mixes W with X, Y or Z is orthogonal but fails the last-column check; use
    /// <see cref="IsOrthogonal(Matrix4x4, double)"/>, a pure 4x4 check, when that form is wanted.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Maximum allowed absolute deviation, applied to the
    /// orthogonality check (per element of <c>M * M^T - I</c>), the last column (per element) and the
    /// determinant (from <c>+1</c>). Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the matrix is a proper rotation within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsRotation(Matrix4x4 m, double tolerance)
        => IsOrthogonal(m, tolerance)
        && HasAffineLastColumn(m, tolerance)
        && HasDeterminantApproximately(m, 1.0, tolerance);

    /// <summary>
    /// Returns <see langword="true"/> when the last column of <paramref name="m"/> is <c>0, 0, 0, 1</c>
    /// within <paramref name="tolerance"/>: the form of an affine transform in the row-vector convention
    /// of <see cref="Matrix4x4"/>.
    /// </summary>
    /// <param name="m">Matrix to test.</param>
    /// <param name="tolerance">Maximum allowed absolute deviation per element, already validated.</param>
    /// <returns><see langword="true"/> if <c>M14</c>, <c>M24</c> and <c>M34</c> are within the tolerance of
    /// <c>0</c> and <c>M44</c> within the tolerance of <c>1</c>.</returns>
    private static bool HasAffineLastColumn(Matrix4x4 m, double tolerance)
        => MathTolerance.IsApproximatelyEqual(m.M14, 0.0, tolerance)
        && MathTolerance.IsApproximatelyEqual(m.M24, 0.0, tolerance)
        && MathTolerance.IsApproximatelyEqual(m.M34, 0.0, tolerance)
        && MathTolerance.IsApproximatelyEqual(m.M44, 1.0, tolerance);

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

        double determinant = m.GetDeterminant();
        return MathTolerance.IsApproximatelyEqual(determinant, expected, tolerance);
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

        double m11 = m.M11;
        double m22 = m.M22;
        double m33 = m.M33;
        double m44 = m.M44;
        var trace = m11 + m22 + m33 + m44;
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

        double determinant = m.GetDeterminant();
        return Math.Abs(determinant) > tolerance;
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

        // In double: the float products of (1e20, 1e20, 0) and (1e20, -1e20, 0) overflow to infinity and
        // the sum is NaN, although the vectors are orthogonal.
        double dot = DotInDouble(u, v);
        return MathTolerance.IsApproximatelyEqual(dot, 0.0, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the two vectors are parallel (or anti-parallel):
    /// the sine of the angle between their directions, <c>|u x v| / (|u| |v|)</c>, is within
    /// <paramref name="tolerance"/> of zero. The measure is scale-invariant, so parallelism
    /// depends only on direction for vectors longer than <paramref name="tolerance"/>. A zero (or
    /// shorter-than-tolerance) vector is treated as parallel to every other vector, both because <c>0 x v = 0</c> and to
    /// avoid dividing by a zero length.
    /// </summary>
    /// <remarks>
    /// The tolerance is an angular measure (the sine of the maximum permitted deviation from exactly
    /// parallel), not an absolute cross-product magnitude. For small angles <c>sin(theta) ~ theta</c>,
    /// so a tolerance of <c>1e-3</c> admits directions within roughly a milliradian of parallel
    /// regardless of how long the vectors are, as long as both are longer than the tolerance.
    /// The lengths and the cross product are computed in <see langword="double"/>, so no
    /// <see langword="float"/> vector can overflow them. A vector of length
    /// <c>&lt;= tolerance</c> counts as parallel to anything. A NaN component gives
    /// <see langword="false"/>, and so does an infinite one under a finite tolerance.
    /// </remarks>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <param name="tolerance">Maximum allowed sine of the angle between the directions. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the directions are parallel within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool AreParallel(Vector3 u, Vector3 v, double tolerance)
    {
        ValidateTolerance(tolerance);

        double uLength = LengthInDouble(u);
        double vLength = LengthInDouble(v);
        if (uLength <= tolerance || vLength <= tolerance)
            return true;

        double sineOfAngle = CrossLengthInDouble(u, v) / (uLength * vLength);
        return MathTolerance.IsApproximatelyEqual(sineOfAngle, 0.0, tolerance);
    }

    /// <summary>
    /// Length of <paramref name="v"/> computed in <see cref="double"/> precision, so a component up to
    /// <see cref="float.MaxValue"/> cannot overflow when squared.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <returns>The Euclidean length.</returns>
    private static double LengthInDouble(Vector3 v)
    {
        double x = v.X;
        double y = v.Y;
        double z = v.Z;
        return Math.Sqrt((x * x) + (y * y) + (z * z));
    }

    /// <summary>Length of the cross product of two vectors computed in <see cref="double"/> precision.</summary>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <returns>The length of <c>u x v</c>.</returns>
    private static double CrossLengthInDouble(Vector3 u, Vector3 v)
    {
        double ux = u.X;
        double uy = u.Y;
        double uz = u.Z;
        double vx = v.X;
        double vy = v.Y;
        double vz = v.Z;
        double cx = (uy * vz) - (uz * vy);
        double cy = (uz * vx) - (ux * vz);
        double cz = (ux * vy) - (uy * vx);
        return Math.Sqrt((cx * cx) + (cy * cy) + (cz * cz));
    }

    /// <summary>
    /// Returns the unsigned angle, in radians on <c>[0, pi]</c>, between two vectors, computed as
    /// <c>atan2(|u x v|, u . v)</c>. This form stays numerically accurate across the whole range,
    /// unlike <c>acos((u . v) / (|u| |v|))</c>, which loses precision near <c>0</c> and <c>pi</c>
    /// where the cosine flattens. A zero vector yields an angle of <c>0</c> by convention (the angle
    /// is otherwise undefined).
    /// </summary>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <returns>The angle between the vectors in radians, on <c>[0, pi]</c>; NaN when a component is NaN.</returns>
    /// <remarks>
    /// The cross product and the dot product are computed in <see langword="double"/>, so vectors of
    /// any <see langword="float"/> magnitude keep their angle: <c>(1e-30, 0, 0)</c> and <c>(0, 1e-30, 0)</c>
    /// are a right angle apart (the products underflow to zero in <see langword="float"/>, which reads
    /// as a zero vector and gives <c>0</c>), and vectors near <c>1e20</c> do not overflow.
    /// </remarks>
    public static double AngleBetween(Vector3 u, Vector3 v)
        => Math.Atan2(CrossLengthInDouble(u, v), DotInDouble(u, v));

    /// <summary>Dot product of two vectors computed in <see cref="double"/> precision.</summary>
    /// <param name="u">First vector.</param>
    /// <param name="v">Second vector.</param>
    /// <returns>The dot product <c>u . v</c>.</returns>
    private static double DotInDouble(Vector3 u, Vector3 v)
    {
        double ux = u.X;
        double uy = u.Y;
        double uz = u.Z;
        return (ux * v.X) + (uy * v.Y) + (uz * v.Z);
    }

    /// <summary>Scalar triple product <c>a . (b x c)</c> computed in <see cref="double"/> precision.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="c">Third vector.</param>
    /// <returns>The signed volume of the parallelepiped spanned by the three vectors.</returns>
    private static double TripleProductInDouble(Vector3 a, Vector3 b, Vector3 c)
    {
        double bx = b.X;
        double by = b.Y;
        double bz = b.Z;
        double cx = c.X;
        double cy = c.Y;
        double cz = c.Z;
        double crossX = (by * cz) - (bz * cy);
        double crossY = (bz * cx) - (bx * cz);
        double crossZ = (bx * cy) - (by * cx);
        double ax = a.X;
        double ay = a.Y;
        double az = a.Z;
        return (ax * crossX) + (ay * crossY) + (az * crossZ);
    }

    /// <summary>Returns <see langword="true"/> when every component of every vector is finite.</summary>
    /// <param name="vectors">Vectors to inspect.</param>
    /// <returns><see langword="true"/> if no component is NaN or infinite.</returns>
    private static bool AreAllFinite(ReadOnlySpan<Vector3> vectors)
    {
        foreach (var v in vectors)
        {
            if (!float.IsFinite(v.X) || !float.IsFinite(v.Y) || !float.IsFinite(v.Z))
                return false;
        }
        return true;
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
    /// <c>|v1 . (v2 x v3)| &gt; tolerance</c>. The absolute scalar triple product is the
    /// volume of the parallelepiped spanned by the three vectors and is non-zero iff the
    /// three are linearly independent.
    /// </para>
    /// <para>
    /// The length-1 case compares vector length directly against tolerance rather than
    /// length-squared against tolerance-squared, so the verdict is well-defined for
    /// extreme tolerance magnitudes (where squaring would underflow or overflow). The length and the
    /// triple product are computed in <see langword="double"/>, so a tiny or huge <see langword="float"/>
    /// vector does not underflow or overflow.
    /// </para>
    /// <para>
    /// A vector with a NaN or infinite component has no direction: the result is
    /// <see langword="false"/> for any count of vectors but zero.
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

        // A vector with a NaN or infinite component has no direction: the set is not independent (the pair
        // case would otherwise read "not parallel" off the NaN and report independence).
        if (!AreAllFinite(vectors))
            return false;

        switch (vectors.Length)
        {
            case 0:
                return true;
            case 1:
                return LengthInDouble(vectors[0]) > tolerance;
            case 2:
                return !AreParallel(vectors[0], vectors[1], tolerance);
            case 3:
                return Math.Abs(TripleProductInDouble(vectors[0], vectors[1], vectors[2])) > tolerance;
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
