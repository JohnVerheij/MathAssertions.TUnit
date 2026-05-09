using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the intersection predicates and pointcloud aggregate predicates over the
/// <see cref="MathAssertions.Geometry3D"/> primitives.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class Geometry3DIntersectionPointcloudTests
{
    private const double Tol = 1e-6;
    private static readonly AxisAlignedBox UnitBox = new(Vector3.Zero, Vector3.One);

    // ----- Sphere-Sphere -----

    [Test]
    public async Task Sphere_Sphere_Overlap_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Sphere(Vector3.Zero, 1f);
        var b = new Sphere(new Vector3(1.5f, 0, 0), 1f);
        await Assert.That(Intersection.Intersects(a, b)).IsTrue();
    }

    [Test]
    public async Task Sphere_Sphere_Tangent_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Sphere(Vector3.Zero, 1f);
        var b = new Sphere(new Vector3(2, 0, 0), 1f);
        await Assert.That(Intersection.Intersects(a, b)).IsTrue();
    }

    [Test]
    public async Task Sphere_Sphere_Disjoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Sphere(Vector3.Zero, 1f);
        var b = new Sphere(new Vector3(5, 0, 0), 1f);
        await Assert.That(Intersection.Intersects(a, b)).IsFalse();
    }

    // ----- AABB-AABB -----

    [Test]
    public async Task AABB_AABB_Overlap_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var b = new AxisAlignedBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2, 2, 2));
        await Assert.That(Intersection.Intersects(UnitBox, b)).IsTrue();
    }

    [Test]
    public async Task AABB_AABB_BoundaryContact_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Boxes touching at a face: intersection includes the boundary.
        var b = new AxisAlignedBox(new Vector3(1, 0, 0), new Vector3(2, 1, 1));
        await Assert.That(Intersection.Intersects(UnitBox, b)).IsTrue();
    }

    [Test]
    public async Task AABB_AABB_Disjoint_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var b = new AxisAlignedBox(new Vector3(2, 2, 2), new Vector3(3, 3, 3));
        await Assert.That(Intersection.Intersects(UnitBox, b)).IsFalse();
    }

    // ----- Ray-Plane -----

    [Test]
    public async Task Ray_Plane_FrontalHit_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray pointing at the y=0 plane from above; t=2.
        var ray = new Ray3D(new Vector3(0, 2, 0), new Vector3(0, -1, 0));
        var plane = new Plane(Vector3.UnitY, 0f);
        var hit = Intersection.Intersects(ray, plane, out var t);
        await Assert.That(hit).IsTrue();
        await Assert.That((double)t).IsEqualTo(2.0).Within(Tol);
    }

    [Test]
    public async Task Ray_Plane_PointingAway_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray on the y=0 plane pointing up away from the +Y normal: hits at t < 0
        // (the plane is behind the ray's start), method returns false.
        var ray = new Ray3D(new Vector3(0, 1, 0), Vector3.UnitY);
        var plane = new Plane(Vector3.UnitY, 0f);
        var hit = Intersection.Intersects(ray, plane, out _);
        await Assert.That(hit).IsFalse();
    }

    [Test]
    public async Task Ray_Plane_Parallel_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray parallel to y=0 plane; never crosses.
        var ray = new Ray3D(new Vector3(0, 1, 0), Vector3.UnitX);
        var plane = new Plane(Vector3.UnitY, 0f);
        var hit = Intersection.Intersects(ray, plane, out var t);
        await Assert.That(hit).IsFalse();
        await Assert.That((double)t).IsEqualTo(0.0).Within(Tol);
    }

    [Test]
    public async Task Ray_Plane_BooleanShorthand(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(0, 2, 0), new Vector3(0, -1, 0));
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(Intersection.Intersects(ray, plane)).IsTrue();
    }

    // ----- Ray-Sphere -----

    [Test]
    public async Task Ray_Sphere_DirectHit_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(-3, 0, 0), Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(Intersection.Intersects(ray, sphere)).IsTrue();
    }

    [Test]
    public async Task Ray_Sphere_Tangent_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray on the y=1 line aimed at +X grazes a unit sphere at (0, 1, 0).
        var ray = new Ray3D(new Vector3(-3, 1, 0), Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(Intersection.Intersects(ray, sphere)).IsTrue();
    }

    [Test]
    public async Task Ray_Sphere_Miss_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(-3, 5, 0), Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(Intersection.Intersects(ray, sphere)).IsFalse();
    }

    [Test]
    public async Task Ray_Sphere_PointingAwayFromBehind_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray origin past the sphere, pointing away: misses.
        var ray = new Ray3D(new Vector3(5, 0, 0), Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(Intersection.Intersects(ray, sphere)).IsFalse();
    }

    [Test]
    public async Task Ray_Sphere_StartsInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray origin at the sphere center: any direction hits the sphere.
        var ray = new Ray3D(Vector3.Zero, Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(Intersection.Intersects(ray, sphere)).IsTrue();
    }

    // ----- Ray-Triangle (Möller-Trumbore) -----

    [Test]
    public async Task Ray_Triangle_HitsInterior_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        // Ray from (0.5, 0.5, 5) pointing -Z hits the triangle at (0.5, 0.5, 0).
        var ray = new Ray3D(new Vector3(0.5f, 0.5f, 5), -Vector3.UnitZ);
        var hit = Intersection.Intersects(ray, t, out var tHit);
        await Assert.That(hit).IsTrue();
        await Assert.That((double)tHit).IsEqualTo(5.0).Within(Tol);
    }

    [Test]
    public async Task Ray_Triangle_MissesOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(5, 5, 5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_BehindOrigin_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        // Ray origin past the triangle plane, pointing away from it.
        var ray = new Ray3D(new Vector3(0.5f, 0.5f, -5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_Parallel_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(0.5f, 0.5f, 5), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_VBeyondOne_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray hits the triangle's plane but at a point where u+v > 1 (outside the
        // triangle past the BC edge). Pins the v-range short-circuit.
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(2.5f, 2.5f, 5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_UNegative_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray hits the triangle's plane but at a point where u < 0 (left of edge AC).
        // Pins the negative-u branch of the u-range short-circuit.
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(-1, 0.5f, 5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_VNegative_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray hits the triangle's plane but at a point where v < 0 (below edge AB).
        // Pins the negative-v branch of the v-range short-circuit, distinct from the
        // u+v>1 branch.
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(0.5f, -1, 5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsFalse();
    }

    [Test]
    public async Task Ray_Triangle_BooleanShorthand(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(0.5f, 0.5f, 5), -Vector3.UnitZ);
        await Assert.That(Intersection.Intersects(ray, t)).IsTrue();
    }

    // ----- Ray-AABB -----

    [Test]
    public async Task Ray_AABB_DirectHit_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(-2, 0.5f, 0.5f), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsTrue();
    }

    [Test]
    public async Task Ray_AABB_Miss_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(-2, 5, 5), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsFalse();
    }

    [Test]
    public async Task Ray_AABB_ParallelAxis_OriginAboveMax_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Y origin = 5 > Y max = 1. Pins the origin > max branch of the parallel-axis check.
        var ray = new Ray3D(new Vector3(-2, 5, 0.5f), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsFalse();
    }

    [Test]
    public async Task Ray_AABB_ParallelAxis_OriginBelowMin_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Y origin = -5 < Y min = 0. Pins the origin < min branch of the parallel-axis
        // check, distinct from the origin > max branch.
        var ray = new Ray3D(new Vector3(-2, -5, 0.5f), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsFalse();
    }

    [Test]
    public async Task Ray_AABB_SlabsInconsistent_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray enters the X slab at t=2 and exits at t=3, but the Y slab limits the
        // intersection to t in [4, 5]. tMin > tMax fires inside the slab loop and
        // short-circuits to false. Distinct from the parallel-axis miss path.
        var ray = new Ray3D(new Vector3(-2, 5, 5), new Vector3(1, -1, -1));
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsFalse();
    }

    [Test]
    public async Task Ray_AABB_PointingAway_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray origin past the box pointing further away.
        var ray = new Ray3D(new Vector3(5, 0.5f, 0.5f), Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsFalse();
    }

    [Test]
    public async Task Ray_AABB_NegativeDirectionFlipsT(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Ray pointing in -X with origin to the right of the box: t1 > t2 in the slab
        // test, exercising the swap branch.
        var ray = new Ray3D(new Vector3(2, 0.5f, 0.5f), -Vector3.UnitX);
        await Assert.That(Intersection.Intersects(ray, UnitBox)).IsTrue();
    }

    // ----- Pointcloud.IsBoundedBy(AABB / Sphere) -----

    [Test]
    public async Task Cloud_IsBoundedByAABB_AllInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> cloud = [new(0.1f, 0.1f, 0.1f), new(0.5f, 0.5f, 0.5f), new(0.9f, 0.9f, 0.9f)];
        await Assert.That(cloud.IsBoundedBy(UnitBox)).IsTrue();
    }

    [Test]
    public async Task Cloud_IsBoundedByAABB_OneOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> cloud = [new(0.5f, 0.5f, 0.5f), new(2, 2, 2)];
        await Assert.That(cloud.IsBoundedBy(UnitBox)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsBoundedByAABB_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(ReadOnlySpan<Vector3>.Empty.IsBoundedBy(UnitBox)).IsTrue();
    }

    [Test]
    public async Task Cloud_IsBoundedBySphere_AllInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 1f);
        ReadOnlySpan<Vector3> cloud = [new(0.3f, 0.3f, 0.3f), Vector3.Zero, new(-0.5f, 0, 0)];
        await Assert.That(cloud.IsBoundedBy(sphere)).IsTrue();
    }

    [Test]
    public async Task Cloud_IsBoundedBySphere_OneOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 1f);
        ReadOnlySpan<Vector3> cloud = [Vector3.Zero, new(5, 0, 0)];
        await Assert.That(cloud.IsBoundedBy(sphere)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsBoundedBySphere_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(ReadOnlySpan<Vector3>.Empty.IsBoundedBy(sphere)).IsTrue();
    }

    // ----- Pointcloud.HasCentroidAt -----

    [Test]
    public async Task Cloud_HasCentroidAt_OriginCenteredCube_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> cloud =
        [
            new(-1, -1, -1), new(1, -1, -1), new(-1, 1, -1), new(1, 1, -1),
            new(-1, -1, 1), new(1, -1, 1), new(-1, 1, 1), new(1, 1, 1),
        ];
        await Assert.That(cloud.HasCentroidAt(Vector3.Zero, Tol)).IsTrue();
    }

    [Test]
    public async Task Cloud_HasCentroidAt_Mismatch_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> cloud = [Vector3.Zero, Vector3.UnitX];
        await Assert.That(cloud.HasCentroidAt(new Vector3(99, 99, 99), Tol)).IsFalse();
    }

    [Test]
    public async Task Cloud_HasCentroidAt_Empty_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(ReadOnlySpan<Vector3>.Empty.HasCentroidAt(Vector3.Zero, Tol)).IsFalse();
    }

    [Test]
    public async Task Cloud_HasCentroidAt_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => ReadOnlySpan<Vector3>.Empty.HasCentroidAt(Vector3.Zero, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Cloud_HasCentroidAt_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => ReadOnlySpan<Vector3>.Empty.HasCentroidAt(Vector3.Zero, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Pointcloud.IsApproximatelyOnPlane -----

    [Test]
    public async Task Cloud_IsApproximatelyOnPlane_AllOnPlane_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        ReadOnlySpan<Vector3> cloud = [Vector3.Zero, new(1, 0, 0), new(0, 0, 1)];
        await Assert.That(cloud.IsApproximatelyOnPlane(plane, Tol)).IsTrue();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnPlane_OneOff_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        ReadOnlySpan<Vector3> cloud = [Vector3.Zero, new(0, 5, 0)];
        await Assert.That(cloud.IsApproximatelyOnPlane(plane, Tol)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnPlane_NegativeResidual_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(() => ReadOnlySpan<Vector3>.Empty.IsApproximatelyOnPlane(plane, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Pointcloud.IsApproximatelyOnSphere -----

    [Test]
    public async Task Cloud_IsApproximatelyOnSphere_AllOnSurface_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Six points at unit distance from origin along ±axes.
        ReadOnlySpan<Vector3> cloud =
        [
            Vector3.UnitX, -Vector3.UnitX,
            Vector3.UnitY, -Vector3.UnitY,
            Vector3.UnitZ, -Vector3.UnitZ,
        ];
        await Assert.That(cloud.IsApproximatelyOnSphere(Vector3.Zero, 1f, Tol)).IsTrue();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnSphere_OneOff_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> cloud = [Vector3.UnitX, new(0.5f, 0, 0)];
        await Assert.That(cloud.IsApproximatelyOnSphere(Vector3.Zero, 1f, Tol)).IsFalse();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnSphere_NegativeResidual_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => ReadOnlySpan<Vector3>.Empty.IsApproximatelyOnSphere(Vector3.Zero, 1f, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Cloud_IsApproximatelyOnSphere_NaNResidual_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => ReadOnlySpan<Vector3>.Empty.IsApproximatelyOnSphere(Vector3.Zero, 1f, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }
}
