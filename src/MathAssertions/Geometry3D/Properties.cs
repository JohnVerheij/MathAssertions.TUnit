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
        double area = t.Area;
        return area <= tolerance;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="points"/> lies
    /// on a single line within <paramref name="tolerance"/>. Sequences with fewer than
    /// three points are vacuously collinear; sequences in which all points coincide are
    /// also collinear. A point with a NaN or infinite coordinate defines no line: the result is
    /// <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Picks the first non-trivial direction in the sequence (the first point distinct
    /// from <c>points[0]</c> within tolerance) and tests perpendicular distance from
    /// that direction for every point. Handling the leading-coincident-points case
    /// explicitly avoids the false-true result a naive implementation that locks in
    /// <c>points[1] - points[0]</c> would produce when those first two points coincide.
    /// </para>
    /// <para>
    /// The perpendicular-distance check is computed as
    /// <c>|direction × (p - p0)| / |direction|</c>: the cross-product magnitude alone is
    /// scaled by <c>|direction|</c>, so comparing it directly to a length-dimensioned
    /// tolerance would let a longer baseline silently widen the apparent tolerance.
    /// Dividing by the direction magnitude restores the dimensionally correct
    /// perpendicular distance from the point to the line through <c>p0</c>.
    /// </para>
    /// </remarks>
    /// <param name="points">Points to test.</param>
    /// <param name="tolerance">Maximum allowed perpendicular displacement from the line.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if all points are collinear within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsCollinear(ReadOnlySpan<Vector3> points, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (!AreAllFinite(points))
            return false;

        if (points.Length < 3)
            return true;

        var p0 = points[0];
        var direction = Vector3.Zero;
        var foundDirection = false;
        for (var i = 1; i < points.Length; i++)
        {
            var d = points[i] - p0;
            double dLength = d.Length();
            if (dLength > tolerance)
            {
                direction = d;
                foundDirection = true;
                break;
            }
        }
        if (!foundDirection)
            return true;

        // foundDirection guarantees |direction| > tolerance >= 0, so the divisor is non-zero.
        double dirLength = direction.Length();
        for (var i = 1; i < points.Length; i++)
        {
            var cross = Vector3.Cross(direction, points[i] - p0);
            double crossLength = cross.Length();
            var perpendicularDistance = crossLength / dirLength;
            if (perpendicularDistance > tolerance)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="points"/> lies
    /// on a single plane within <paramref name="tolerance"/>. Sequences with fewer than
    /// four points are vacuously coplanar; sequences in which every point is collinear
    /// are also coplanar (every line lies on infinitely many planes). A point with a NaN or
    /// infinite coordinate defines no plane: the result is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Searches the input for a non-collinear triple and uses it to construct the test
    /// plane. A triple counts as non-collinear when its third point lies farther than
    /// <paramref name="tolerance"/> (a length, like every other distance here) from the line
    /// through the first two, the same measure <see cref="IsCollinear(ReadOnlySpan{Vector3}, double)"/>
    /// uses; the cross product alone is an area and would let a small point set pass whatever
    /// its shape. If no such triple exists the entire set is collinear, which trivially
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

        if (!AreAllFinite(points))
            return false;
        if (points.Length < 4)
            return true;
        if (!TryFindPlaneNormal(points, tolerance, out var normalX, out var normalY, out var normalZ))
            return true;

        double ox = points[0].X;
        double oy = points[0].Y;
        double oz = points[0].Z;
        for (var i = 0; i < points.Length; i++)
        {
            double px = points[i].X;
            double py = points[i].Y;
            double pz = points[i].Z;
            double dot = ((px - ox) * normalX) + ((py - oy) * normalY) + ((pz - oz) * normalZ);
            if (Math.Abs(dot) > tolerance)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Looks for a triple <c>points[0]</c>, <c>points[i]</c>, <c>points[j]</c> that spans a plane: the
    /// third point lies farther than <paramref name="tolerance"/> from the line through the first two
    /// (the length of the cross product divided by the length of the edge, so the measure is a distance).
    /// An edge no longer than the tolerance (coincident or near-coincident points, as in IsCollinear) is skipped: its direction is noise. Computed in <see cref="double"/> precision.
    /// </summary>
    /// <param name="points">Finite points, at least three.</param>
    /// <param name="tolerance">Distance a point must exceed to count as off the line.</param>
    /// <param name="normalX">Receives the X component of the unit plane normal.</param>
    /// <param name="normalY">Receives the Y component of the unit plane normal.</param>
    /// <param name="normalZ">Receives the Z component of the unit plane normal.</param>
    /// <returns><see langword="true"/> if a spanning triple was found; <see langword="false"/> if every
    /// point lies on one line within <paramref name="tolerance"/>.</returns>
    private static bool TryFindPlaneNormal(
        ReadOnlySpan<Vector3> points,
        double tolerance,
        out double normalX,
        out double normalY,
        out double normalZ)
    {
        double ox = points[0].X;
        double oy = points[0].Y;
        double oz = points[0].Z;
        for (var i = 1; i < points.Length - 1; i++)
        {
            double ax = points[i].X;
            double ay = points[i].Y;
            double az = points[i].Z;
            double ux = ax - ox;
            double uy = ay - oy;
            double uz = az - oz;
            double edgeLength = Math.Sqrt((ux * ux) + (uy * uy) + (uz * uz));
            if (edgeLength <= tolerance)
                continue;

            for (var j = i + 1; j < points.Length; j++)
            {
                double jx = points[j].X;
                double jy = points[j].Y;
                double jz = points[j].Z;
                double vx = jx - ox;
                double vy = jy - oy;
                double vz = jz - oz;
                double cx = (uy * vz) - (uz * vy);
                double cy = (uz * vx) - (ux * vz);
                double cz = (ux * vy) - (uy * vx);
                double crossLength = Math.Sqrt((cx * cx) + (cy * cy) + (cz * cz));
                if (crossLength / edgeLength > tolerance)
                {
                    normalX = cx / crossLength;
                    normalY = cy / crossLength;
                    normalZ = cz / crossLength;
                    return true;
                }
            }
        }

        normalX = normalY = normalZ = 0.0;
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every coordinate of every point is finite (neither NaN nor
    /// infinite). Distances to such a point are NaN, and a NaN distance would otherwise pass a
    /// "greater than tolerance" test.
    /// </summary>
    /// <param name="points">Points to inspect.</param>
    /// <returns><see langword="true"/> if all coordinates are finite.</returns>
    private static bool AreAllFinite(ReadOnlySpan<Vector3> points)
    {
        foreach (var p in points)
        {
            if (!float.IsFinite(p.X) || !float.IsFinite(p.Y) || !float.IsFinite(p.Z))
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
