using System;
using System.Numerics;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Boolean intersection predicates and parametric ray-intersection results for the
/// <see cref="Geometry3D"/> primitives plus <see cref="Plane"/>. Each ray-intersection
/// pair offers a boolean overload (for tests that only care whether the ray hits). Only
/// ray-plane and ray-triangle also have a parametric overload with an <c>out float t</c>,
/// the ray parameter of the hit point <c>origin + t * direction</c> (a distance only for a
/// unit direction); ray-sphere and ray-AABB are boolean only.
/// </summary>
public static class Intersection
{
    private const float ParallelEpsilon = 1e-7f;

    /// <summary>
    /// Returns <see langword="true"/> when the closed solid spheres
    /// <paramref name="a"/> and <paramref name="b"/> overlap (boundary contact counts as
    /// intersection). Compared via squared center distance to avoid the square root.
    /// </summary>
    public static bool Intersects(Sphere a, Sphere b)
    {
        var sumRadii = a.Radius + b.Radius;
        return Vector3.DistanceSquared(a.Center, b.Center) <= sumRadii * sumRadii;
    }

    /// <summary>
    /// Returns <see langword="true"/> when two axis-aligned boxes overlap on every axis
    /// (boundary contact counts as intersection).
    /// </summary>
    public static bool Intersects(AxisAlignedBox a, AxisAlignedBox b)
        => a.Min.X <= b.Max.X && a.Max.X >= b.Min.X
        && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y
        && a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="ray"/> hits
    /// <paramref name="plane"/> at a ray parameter <c>t &gt;= 0</c>, and outputs
    /// <paramref name="t"/>. A ray parallel to the plane
    /// (<c>|direction . normal| &lt; 1e-7</c>) returns <see langword="false"/> with <c>t == 0</c>.
    /// A plane behind the ray returns <see langword="false"/> with the computed, negative
    /// <c>t</c>.
    /// </summary>
    /// <param name="ray">Ray to test.</param>
    /// <param name="plane">Plane to test against.</param>
    /// <param name="t">Ray parameter of the hit on success: the hit point is
    /// <c>origin + t * direction</c>, so <c>t</c> is a distance only for a unit direction. When
    /// the method returns <see langword="false"/>: <c>0</c> if the ray is parallel to the plane,
    /// or the computed negative <c>t</c> if the plane is behind the ray.</param>
    /// <returns><see langword="true"/> if the ray crosses the plane in front of its origin.</returns>
    public static bool Intersects(Ray3D ray, Plane plane, out float t)
    {
        var denom = Vector3.Dot(ray.Direction, plane.Normal);
        if (Math.Abs(denom) < ParallelEpsilon)
        {
            t = 0;
            return false;
        }
        t = -(Vector3.Dot(ray.Origin, plane.Normal) + plane.D) / denom;
        return t >= 0;
    }

    /// <summary>Boolean shorthand for the <see cref="Intersects(Ray3D, Plane, out float)"/> overload.</summary>
    public static bool Intersects(Ray3D ray, Plane plane) => Intersects(ray, plane, out _);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="ray"/> intersects the closed
    /// solid <paramref name="sphere"/>. Geometric solution: project the sphere center
    /// onto the ray, then compare squared perpendicular distance to squared radius.
    /// </summary>
    /// <remarks>The ray direction must be a unit vector; the projection math assumes it.
    /// Reference: Akenine-Möller, Haines, Hoffman, <i>Real-Time Rendering</i> 4th ed., §22.6.</remarks>
    public static bool Intersects(Ray3D ray, Sphere sphere)
    {
        var l = sphere.Center - ray.Origin;
        var s = Vector3.Dot(l, ray.Direction);
        var l2 = Vector3.Dot(l, l);
        var r2 = sphere.Radius * sphere.Radius;
        if (s < 0 && l2 > r2)
            return false;
        var m2 = l2 - (s * s);
        return m2 <= r2;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="ray"/> hits
    /// <paramref name="triangle"/> at a ray parameter <c>t &gt; 1e-7</c> (a ray origin lying on
    /// the triangle does not count as a hit), and outputs <paramref name="t"/>, the ray parameter
    /// of the hit point <c>origin + t * direction</c>. The threshold applies to the parameter, not
    /// to a distance: for a non-unit direction the hit lies <c>t * |direction|</c> from the origin.
    /// Implemented via the Möller-Trumbore algorithm. <paramref name="t"/> is <c>0</c> when the
    /// method returns <see langword="false"/> because the ray is parallel to the triangle plane or
    /// misses the triangle; when the triangle lies behind the ray (or at the origin),
    /// <see langword="false"/> is returned with the computed <c>t</c>, which is at most <c>1e-7</c>.
    /// </summary>
    /// <remarks>Reference: Akenine-Möller, Haines, Hoffman, <i>Real-Time Rendering</i> 4th ed., §22.8.</remarks>
    public static bool Intersects(Ray3D ray, Triangle3D triangle, out float t)
    {
        var edge1 = triangle.B - triangle.A;
        var edge2 = triangle.C - triangle.A;
        var h = Vector3.Cross(ray.Direction, edge2);
        var a = Vector3.Dot(edge1, h);
        if (Math.Abs(a) < ParallelEpsilon)
        {
            t = 0;
            return false;
        }
        var f = 1.0f / a;
        var s = ray.Origin - triangle.A;
        var u = f * Vector3.Dot(s, h);
        if (u < 0 || u > 1)
        {
            t = 0;
            return false;
        }
        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(ray.Direction, q);
        if (v < 0 || u + v > 1)
        {
            t = 0;
            return false;
        }
        t = f * Vector3.Dot(edge2, q);
        return t > ParallelEpsilon;
    }

    /// <summary>Boolean shorthand for the <see cref="Intersects(Ray3D, Triangle3D, out float)"/> overload.</summary>
    public static bool Intersects(Ray3D ray, Triangle3D triangle) => Intersects(ray, triangle, out _);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="ray"/> intersects
    /// <paramref name="box"/>. Slab-test implementation.
    /// </summary>
    /// <remarks>Reference: Akenine-Möller, Haines, Hoffman, <i>Real-Time Rendering</i> 4th ed., §22.7.</remarks>
    public static bool Intersects(Ray3D ray, AxisAlignedBox box)
    {
        var tMin = float.NegativeInfinity;
        var tMax = float.PositiveInfinity;
        for (var i = 0; i < 3; i++)
        {
            var origin = ComponentAt(ray.Origin, i);
            var dir = ComponentAt(ray.Direction, i);
            var min = ComponentAt(box.Min, i);
            var max = ComponentAt(box.Max, i);
            if (Math.Abs(dir) < ParallelEpsilon)
            {
                // Negated bounds test, so a NaN origin fails instead of passing the slab.
                if (!(origin >= min && origin <= max))
                    return false;
            }
            else
            {
                var invD = 1.0f / dir;
                var t1 = (min - origin) * invD;
                var t2 = (max - origin) * invD;
                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax)
                    return false;
            }
        }
        return tMax >= 0;
    }

    private static float ComponentAt(Vector3 v, int axis) => axis switch
    {
        0 => v.X,
        1 => v.Y,
        _ => v.Z,
    };
}
