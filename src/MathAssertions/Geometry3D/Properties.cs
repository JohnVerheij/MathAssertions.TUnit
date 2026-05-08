using System;
using System.Numerics;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Geometric-property predicates over <see cref="Geometry3D"/> primitives and point sets:
/// triangle degeneracy, point-set collinearity, and point-set coplanarity.
/// </summary>
public static class Properties
{
    /// <summary>
    /// Returns <see langword="true"/> when the triangle's area is at most
    /// <paramref name="tolerance"/>, indicating collinear or coincident vertices.
    /// </summary>
    /// <param name="t">Triangle to test.</param>
    /// <param name="tolerance">Maximum area considered degenerate. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if <c>t.Area &lt;= tolerance</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsDegenerate(this Triangle3D t, double tolerance)
    {
        ValidateTolerance(tolerance);
        return (double)t.Area <= tolerance;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="points"/> lies
    /// on a single line within <paramref name="tolerance"/>. Sequences with fewer than
    /// three points are vacuously collinear; sequences in which all points coincide are
    /// also collinear.
    /// </summary>
    /// <remarks>
    /// Picks the first non-trivial direction in the sequence (the first point distinct
    /// from <c>points[0]</c> within tolerance) and tests cross-product magnitude against
    /// that direction for every point. Handling the leading-coincident-points case
    /// explicitly avoids the false-true result a naive implementation that locks in
    /// <c>points[1] - points[0]</c> would produce when those first two points coincide.
    /// </remarks>
    /// <param name="points">Points to test.</param>
    /// <param name="tolerance">Maximum allowed perpendicular displacement from the line.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if all points are collinear within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsCollinear(ReadOnlySpan<Vector3> points, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (points.Length < 3)
            return true;

        var p0 = points[0];
        var direction = Vector3.Zero;
        var foundDirection = false;
        for (var i = 1; i < points.Length; i++)
        {
            var d = points[i] - p0;
            if ((double)d.Length() > tolerance)
            {
                direction = d;
                foundDirection = true;
                break;
            }
        }
        if (!foundDirection)
            return true;

        for (var i = 1; i < points.Length; i++)
        {
            var cross = Vector3.Cross(direction, points[i] - p0);
            if ((double)cross.Length() > tolerance)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="points"/> lies
    /// on a single plane within <paramref name="tolerance"/>. Sequences with fewer than
    /// four points are vacuously coplanar; sequences in which every point is collinear
    /// are also coplanar (every line lies on infinitely many planes).
    /// </summary>
    /// <remarks>
    /// Searches the input for a non-collinear triple and uses it to construct the test
    /// plane. If no such triple exists the entire set is collinear, which trivially
    /// satisfies coplanarity. The search is at worst quadratic in the number of points
    /// when most pairs are collinear with <c>points[0]</c>; for typical test inputs the
    /// non-collinear triple is found in the first few iterations.
    /// </remarks>
    /// <param name="points">Points to test.</param>
    /// <param name="tolerance">Maximum allowed perpendicular displacement from the plane.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if all points are coplanar within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool AreCoplanar(ReadOnlySpan<Vector3> points, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (points.Length < 4)
            return true;

        var p0 = points[0];
        var normal = Vector3.Zero;
        var foundNormal = false;
        for (var i = 1; i < points.Length - 1 && !foundNormal; i++)
        {
            for (var j = i + 1; j < points.Length; j++)
            {
                var n = Vector3.Cross(points[i] - p0, points[j] - p0);
                if ((double)n.Length() > tolerance)
                {
                    normal = Vector3.Normalize(n);
                    foundNormal = true;
                    break;
                }
            }
        }
        if (!foundNormal)
            return true;

        for (var i = 0; i < points.Length; i++)
        {
            var d = Math.Abs((double)Vector3.Dot(points[i] - p0, normal));
            if (d > tolerance)
                return false;
        }
        return true;
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
