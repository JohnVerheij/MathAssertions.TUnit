using System;
using System.Numerics;
using MathAssertions.Geometry3D;
using TUnit.Assertions.Attributes;

namespace MathAssertions.TUnit;

/// <summary>
/// Fluent assertions over the <see cref="MathAssertions.Geometry3D"/> primitives:
/// triangle / point-set property predicates, containment, closest-point distance,
/// intersection, and pointcloud aggregates.
/// </summary>
/// <remarks>
/// Distance-from helpers call the static
/// <see cref="MathAssertions.Geometry3D.Distance"/> form rather than the
/// extension-method form because TUnit's <c>[GenerateAssertion]</c> source generator
/// inlines expression bodies into a generated assertion class without copying the
/// <see langword="using"/> directives that would be needed to resolve the extension call.
/// </remarks>
public static class Geometry3DAssertions
{
    // ----- Properties -----

    /// <summary>Asserts the triangle's area is at most <paramref name="tolerance"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be a degenerate triangle within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool IsDegenerate(this Triangle3D value, double tolerance)
        => Properties.IsDegenerate(value, tolerance);

    /// <summary>Asserts every point in the array is collinear within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be collinear within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool IsCollinear(this Vector3[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Properties.IsCollinear(value, tolerance);
    }

    /// <summary>Asserts every point in the array lies on a single plane within tolerance.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be coplanar within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool AreCoplanar(this Vector3[] value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Properties.AreCoplanar(value, tolerance);
    }

    // ----- Containment -----

    /// <summary>Asserts the AABB contains <paramref name="point"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to contain point {point}", InlineMethodBody = true)]
    public static bool ContainsPoint(this AxisAlignedBox value, Vector3 point)
        => Containment.Contains(value, point);

    /// <summary>Asserts the sphere contains <paramref name="point"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to contain point {point}", InlineMethodBody = true)]
    public static bool ContainsPoint(this Sphere value, Vector3 point)
        => Containment.Contains(value, point);

    /// <summary>Asserts the oriented box contains <paramref name="point"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to contain point {point}", InlineMethodBody = true)]
    public static bool ContainsPoint(this OrientedBox value, Vector3 point)
        => Containment.Contains(value, point);

    /// <summary>Asserts the outer box contains <paramref name="inner"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to contain box {inner}", InlineMethodBody = true)]
    public static bool ContainsBox(this AxisAlignedBox value, AxisAlignedBox inner)
        => Containment.Contains(value, inner);

    /// <summary>Asserts the box contains <paramref name="sphere"/> entirely.</summary>
    [GenerateAssertion(ExpectationMessage = "to contain sphere {sphere}", InlineMethodBody = true)]
    public static bool ContainsSphere(this AxisAlignedBox value, Sphere sphere)
        => Containment.Contains(value, sphere);

    /// <summary>
    /// Asserts <paramref name="point"/> lies inside the convex hull described by
    /// <paramref name="value"/>'s flat triples-of-three vertex layout.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to contain point {point} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool ConvexHullContainsPoint(this Vector3[] value, Vector3 point, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Containment.ConvexHullContains(value, point, tolerance);
    }

    // ----- Distance (predicate-style) -----

    /// <summary>
    /// Asserts the perpendicular distance from <paramref name="value"/> to
    /// <paramref name="plane"/> is approximately <paramref name="expected"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be at distance {expected} from {plane} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasDistanceFromPlane(this Vector3 value, Plane plane, double expected, double tolerance)
        => MathTolerance.IsApproximatelyEqual(Distance.DistanceFrom(value, plane), expected, tolerance);

    /// <summary>
    /// Asserts the closest-point distance from <paramref name="value"/> to
    /// <paramref name="segment"/> is approximately <paramref name="expected"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be at distance {expected} from {segment} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasDistanceFromSegment(
        this Vector3 value,
        LineSegment3D segment,
        double expected,
        double tolerance)
        => MathTolerance.IsApproximatelyEqual(Distance.DistanceFrom(value, segment), expected, tolerance);

    /// <summary>
    /// Asserts the closest-point distance from <paramref name="value"/> to
    /// <paramref name="triangle"/> is approximately <paramref name="expected"/>.
    /// </summary>
    [GenerateAssertion(
        ExpectationMessage = "to be at distance {expected} from {triangle} within tolerance {tolerance}",
        InlineMethodBody = true)]
    public static bool HasDistanceFromTriangle(
        this Vector3 value,
        Triangle3D triangle,
        double expected,
        double tolerance)
        => MathTolerance.IsApproximatelyEqual(Distance.DistanceFrom(value, triangle), expected, tolerance);

    // ----- Intersection -----

    /// <summary>Asserts two spheres intersect (boundary contact counts).</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect sphere {other}", InlineMethodBody = true)]
    public static bool IntersectsSphere(this Sphere value, Sphere other)
        => Intersection.Intersects(value, other);

    /// <summary>Asserts the ray hits <paramref name="sphere"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect sphere {sphere}", InlineMethodBody = true)]
    public static bool IntersectsSphere(this Ray3D value, Sphere sphere)
        => Intersection.Intersects(value, sphere);

    /// <summary>Asserts two boxes intersect (boundary contact counts).</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect box {other}", InlineMethodBody = true)]
    public static bool IntersectsBox(this AxisAlignedBox value, AxisAlignedBox other)
        => Intersection.Intersects(value, other);

    /// <summary>Asserts the ray hits <paramref name="box"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect box {box}", InlineMethodBody = true)]
    public static bool IntersectsBox(this Ray3D value, AxisAlignedBox box)
        => Intersection.Intersects(value, box);

    /// <summary>Asserts the ray hits <paramref name="plane"/> in front of its origin.</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect plane {plane}", InlineMethodBody = true)]
    public static bool IntersectsPlane(this Ray3D value, Plane plane)
        => Intersection.Intersects(value, plane);

    /// <summary>Asserts the ray hits <paramref name="triangle"/>.</summary>
    [GenerateAssertion(ExpectationMessage = "to intersect triangle {triangle}", InlineMethodBody = true)]
    public static bool IntersectsTriangle(this Ray3D value, Triangle3D triangle)
        => Intersection.Intersects(value, triangle);

    // ----- Pointcloud -----

    /// <summary>Asserts every point in the cloud lies inside <paramref name="box"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be bounded by box {box}",
        InlineMethodBody = false)]
    public static bool IsBoundedByBox(this Vector3[] value, AxisAlignedBox box)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Pointcloud.IsBoundedBy(value, box);
    }

    /// <summary>Asserts every point in the cloud lies inside <paramref name="sphere"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to be bounded by sphere {sphere}",
        InlineMethodBody = false)]
    public static bool IsBoundedBySphere(this Vector3[] value, Sphere sphere)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Pointcloud.IsBoundedBy(value, sphere);
    }

    /// <summary>Asserts the cloud's centroid is approximately <paramref name="expected"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to have centroid approximately {expected} within tolerance {tolerance}",
        InlineMethodBody = false)]
    public static bool HasCentroidAt(this Vector3[] value, Vector3 expected, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Pointcloud.HasCentroidAt(value, expected, tolerance);
    }

    /// <summary>Asserts every point in the cloud lies within <paramref name="maxResidual"/> of <paramref name="plane"/>.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to lie approximately on plane {plane} within residual {maxResidual}",
        InlineMethodBody = false)]
    public static bool IsApproximatelyOnPlane(this Vector3[] value, Plane plane, double maxResidual)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Pointcloud.IsApproximatelyOnPlane(value, plane, maxResidual);
    }

    /// <summary>Asserts every point in the cloud lies within <paramref name="maxResidual"/> of the sphere surface.</summary>
    [GenerateAssertion(
        ExpectationMessage = "to lie approximately on a sphere centered at {center} with radius {radius} within residual {maxResidual}",
        InlineMethodBody = false)]
    public static bool IsApproximatelyOnSphere(
        this Vector3[] value,
        Vector3 center,
        float radius,
        double maxResidual)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Pointcloud.IsApproximatelyOnSphere(value, center, radius, maxResidual);
    }
}
