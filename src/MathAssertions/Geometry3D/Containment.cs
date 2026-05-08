using System;
using System.Numerics;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Containment predicates: which primitives lie wholly inside which others. Implemented
/// as extension methods so call sites read naturally
/// (<c>box.Contains(point)</c>, <c>sphere.Contains(point)</c>).
/// </summary>
public static class Containment
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="point"/> lies inside the
    /// closed box defined by <paramref name="box"/> (boundary included).
    /// </summary>
    /// <param name="box">Axis-aligned box to test against.</param>
    /// <param name="point">Point to test.</param>
    /// <returns><see langword="true"/> if every component of <paramref name="point"/>
    /// lies in the corresponding <c>[Min, Max]</c> interval.</returns>
    public static bool Contains(this AxisAlignedBox box, Vector3 point)
        => point.X >= box.Min.X && point.X <= box.Max.X
        && point.Y >= box.Min.Y && point.Y <= box.Max.Y
        && point.Z >= box.Min.Z && point.Z <= box.Max.Z;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="inner"/> lies wholly inside
    /// <paramref name="outer"/> (a box contains another box iff it contains the inner
    /// box's two extreme corners; AABBs are convex, so the two-corner test suffices).
    /// </summary>
    /// <param name="outer">Outer box.</param>
    /// <param name="inner">Inner box to test for containment.</param>
    /// <returns><see langword="true"/> if <paramref name="outer"/> contains both corners
    /// of <paramref name="inner"/>.</returns>
    public static bool Contains(this AxisAlignedBox outer, AxisAlignedBox inner)
        => outer.Contains(inner.Min) && outer.Contains(inner.Max);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="sphere"/> lies wholly inside
    /// <paramref name="box"/>: the sphere center is at least one radius from every box
    /// face. Negative radii are accepted (the predicate becomes a "shrunken-box-contains-
    /// center" check); callers are responsible for using positive radii.
    /// </summary>
    /// <param name="box">Outer axis-aligned box.</param>
    /// <param name="sphere">Inner sphere to test.</param>
    /// <returns><see langword="true"/> if every face of <paramref name="box"/> is at
    /// least <c>sphere.Radius</c> away from <paramref name="sphere"/>'s center.</returns>
    public static bool Contains(this AxisAlignedBox box, Sphere sphere)
        => box.Min.X + sphere.Radius <= sphere.Center.X
        && sphere.Center.X <= box.Max.X - sphere.Radius
        && box.Min.Y + sphere.Radius <= sphere.Center.Y
        && sphere.Center.Y <= box.Max.Y - sphere.Radius
        && box.Min.Z + sphere.Radius <= sphere.Center.Z
        && sphere.Center.Z <= box.Max.Z - sphere.Radius;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="point"/> lies inside or on
    /// the boundary of <paramref name="sphere"/>. Compared via squared distance so no
    /// square root is computed.
    /// </summary>
    /// <param name="sphere">Sphere to test against.</param>
    /// <param name="point">Point to test.</param>
    /// <returns><see langword="true"/> if <c>|point - center|^2 &lt;= radius^2</c>.</returns>
    public static bool Contains(this Sphere sphere, Vector3 point)
        => Vector3.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="point"/> lies inside the
    /// closed oriented box. The point is rotated into the box's local frame by the
    /// inverse orientation, then a half-extent test is applied component-wise.
    /// </summary>
    /// <param name="box">Oriented box to test against.</param>
    /// <param name="point">Point to test.</param>
    /// <returns><see langword="true"/> if the local-frame coordinates of
    /// <paramref name="point"/> all lie within the box's half-extents.</returns>
    public static bool Contains(this OrientedBox box, Vector3 point)
    {
        var inverseRotation = Quaternion.Inverse(box.Orientation);
        var localPoint = Vector3.Transform(point - box.Center, inverseRotation);
        return Math.Abs(localPoint.X) <= box.HalfExtents.X
            && Math.Abs(localPoint.Y) <= box.HalfExtents.Y
            && Math.Abs(localPoint.Z) <= box.HalfExtents.Z;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="point"/> lies inside the
    /// convex hull described by <paramref name="hullVertices"/>, an array of
    /// triangulation-ready triples: every three consecutive vertices form one face
    /// triangle. Each triangle must be wound counter-clockwise when viewed from outside
    /// the hull so that the cross-product face normal points outward; a point is inside
    /// the hull iff it lies on the negative side of every face plane (within tolerance).
    /// </summary>
    /// <remarks>
    /// <para>
    /// 0.1.0 implementation: tests half-space inclusion against each computed face
    /// triangle. Hulls supplied with arbitrary winding produce undefined containment.
    /// Automatic triangulation from a point cloud is deferred to 0.2.0.
    /// </para>
    /// <para>
    /// Spans whose length is not a positive multiple of three return
    /// <see langword="false"/>: the input is malformed and no half-space test is
    /// well-defined. The empty span returns <see langword="false"/> (no interior).
    /// </para>
    /// </remarks>
    /// <param name="hullVertices">Convex-hull face vertices in flat triples-of-three
    /// order; each triple is one CCW-outward face triangle.</param>
    /// <param name="point">Point to test.</param>
    /// <param name="tolerance">Maximum allowed signed deviation onto the positive (outward)
    /// side of any face plane. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if <paramref name="point"/> is inside the hull
    /// within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool ConvexHullContains(
        ReadOnlySpan<Vector3> hullVertices,
        Vector3 point,
        double tolerance)
    {
        ValidateTolerance(tolerance);

        if (hullVertices.Length < 3 || hullVertices.Length % 3 != 0)
            return false;

        for (var i = 0; i < hullVertices.Length; i += 3)
        {
            var a = hullVertices[i];
            var b = hullVertices[i + 1];
            var c = hullVertices[i + 2];
            var normal = Vector3.Cross(b - a, c - a);
            var signed = Vector3.Dot(point - a, normal);
            if ((double)signed > tolerance)
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
