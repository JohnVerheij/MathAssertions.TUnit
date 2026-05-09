using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the containment and closest-point-distance predicates over the
/// <see cref="MathAssertions.Geometry3D"/> primitives.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class Geometry3DContainmentDistanceTests
{
    private const double Tol = 1e-6;
    private static readonly AxisAlignedBox UnitBox = new(Vector3.Zero, Vector3.One);
    private static readonly Sphere UnitSphere = new(Vector3.Zero, 1f);

    // ----- AxisAlignedBox.Contains(Vector3) -----

    [Test]
    public async Task AABBContains_PointInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitBox.Contains(new Vector3(0.5f, 0.5f, 0.5f))).IsTrue();
    }

    [Test]
    public async Task AABBContains_PointOnBoundary_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitBox.Contains(Vector3.Zero)).IsTrue();
        await Assert.That(UnitBox.Contains(Vector3.One)).IsTrue();
    }

    [Test]
    public async Task AABBContains_PointOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitBox.Contains(new Vector3(2, 0.5f, 0.5f))).IsFalse();
        await Assert.That(UnitBox.Contains(new Vector3(0.5f, -1, 0.5f))).IsFalse();
        await Assert.That(UnitBox.Contains(new Vector3(0.5f, 0.5f, 2))).IsFalse();
    }

    // ----- AxisAlignedBox.Contains(AxisAlignedBox) -----

    [Test]
    public async Task AABBContains_InnerBoxFullyInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var inner = new AxisAlignedBox(new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.75f, 0.75f, 0.75f));
        await Assert.That(UnitBox.Contains(inner)).IsTrue();
    }

    [Test]
    public async Task AABBContains_InnerBoxStraddlesBoundary_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var inner = new AxisAlignedBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2, 2, 2));
        await Assert.That(UnitBox.Contains(inner)).IsFalse();
    }

    [Test]
    public async Task AABBContains_InnerBoxMinOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Inner.Min is outside the outer box; short-circuits the && before checking Max.
        var inner = new AxisAlignedBox(new Vector3(-1, -1, -1), new Vector3(0.5f, 0.5f, 0.5f));
        await Assert.That(UnitBox.Contains(inner)).IsFalse();
    }

    // ----- AxisAlignedBox.Contains(Sphere) -----

    [Test]
    public async Task AABBContains_SphereWellInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(new Vector3(-2, -2, -2), new Vector3(2, 2, 2));
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(box.Contains(sphere)).IsTrue();
    }

    [Test]
    public async Task AABBContains_SphereStraddlesFace_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Sphere center at (1, 1, 1), radius 1, box is unit cube; sphere extends past every max face.
        var sphere = new Sphere(Vector3.One, 1f);
        await Assert.That(UnitBox.Contains(sphere)).IsFalse();
    }

    // ----- Sphere.Contains(Vector3) -----

    [Test]
    public async Task SphereContains_PointInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitSphere.Contains(new Vector3(0.3f, 0.3f, 0.3f))).IsTrue();
    }

    [Test]
    public async Task SphereContains_PointOnBoundary_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitSphere.Contains(Vector3.UnitX)).IsTrue();
    }

    [Test]
    public async Task SphereContains_PointOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(UnitSphere.Contains(new Vector3(2, 0, 0))).IsFalse();
    }

    // ----- OrientedBox.Contains(Vector3) -----

    [Test]
    public async Task OBBContains_AxisAlignedFallback_PointInside_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // OBB with identity orientation degenerates to AABB centered at origin.
        var obb = new OrientedBox(Vector3.Zero, Vector3.One, Quaternion.Identity);
        await Assert.That(obb.Contains(new Vector3(0.5f, 0.5f, 0.5f))).IsTrue();
        await Assert.That(obb.Contains(new Vector3(-0.5f, -0.5f, -0.5f))).IsTrue();
    }

    [Test]
    public async Task OBBContains_AxisAlignedFallback_PointOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var obb = new OrientedBox(Vector3.Zero, Vector3.One, Quaternion.Identity);
        await Assert.That(obb.Contains(new Vector3(2, 0, 0))).IsFalse();
    }

    [Test]
    public async Task OBBContains_RotatedFortyFiveDegrees(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Box rotated 45° around Z. A world point at (1.0, 0, 0) corresponds in local
        // frame to roughly (0.707, -0.707, 0); within the unit half-extents.
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 4);
        var obb = new OrientedBox(Vector3.Zero, Vector3.One, rotation);
        await Assert.That(obb.Contains(new Vector3(1, 0, 0))).IsTrue();
        // World point at (1.5, 0, 0) is outside even after rotation, since |x_local| > 1.
        await Assert.That(obb.Contains(new Vector3(1.5f, 0, 0))).IsFalse();
    }

    // ----- ConvexHullContains -----

    private static readonly Vector3[] UnitTetrahedronHull =
    [
        // Bottom face (z=0): outward normal -Z. CCW from below -> p0, p2, p1.
        new(0, 0, 0), new(0, 1, 0), new(1, 0, 0),
        // -Y face: p0, p1, p3 -> normal -Y.
        new(0, 0, 0), new(1, 0, 0), new(0, 0, 1),
        // -X face: p0, p3, p2 -> normal -X.
        new(0, 0, 0), new(0, 0, 1), new(0, 1, 0),
        // Slanted face opposite the origin: p1, p2, p3 -> normal (1,1,1).
        new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
    ];

    [Test]
    public async Task ConvexHullContains_PointInsideTetrahedron_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Interior point well inside the unit tetrahedron with vertices at the origin and
        // the three positive-axis unit points; tested against all four CCW-outward faces.
        await Assert.That(Containment.ConvexHullContains(UnitTetrahedronHull, new Vector3(0.1f, 0.1f, 0.1f), Tol)).IsTrue();
    }

    [Test]
    public async Task ConvexHullContains_PointOutside_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Far outside the slanted face; the slanted face's outward normal (1,1,1) sees
        // the query point at signed-positive deviation, breaking the half-space test.
        await Assert.That(Containment.ConvexHullContains(UnitTetrahedronHull, new Vector3(5, 5, 5), Tol)).IsFalse();
    }

    [Test]
    public async Task ConvexHullContains_PointBelowBottomFace_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Below z=0 is outside the bottom face's outward (-Z) half-space.
        await Assert.That(Containment.ConvexHullContains(UnitTetrahedronHull, new Vector3(0.2f, 0.2f, -0.5f), Tol)).IsFalse();
    }

    [Test]
    public async Task ConvexHullContains_MalformedTripleCount_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Containment.ConvexHullContains(ReadOnlySpan<Vector3>.Empty, Vector3.Zero, Tol)).IsFalse();
        ReadOnlySpan<Vector3> two = [Vector3.Zero, Vector3.UnitX];
        await Assert.That(Containment.ConvexHullContains(two, Vector3.Zero, Tol)).IsFalse();
        ReadOnlySpan<Vector3> four = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];
        await Assert.That(Containment.ConvexHullContains(four, Vector3.Zero, Tol)).IsFalse();
    }

    [Test]
    public async Task ConvexHullContains_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => Containment.ConvexHullContains(ReadOnlySpan<Vector3>.Empty, Vector3.Zero, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConvexHullContains_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => Containment.ConvexHullContains(ReadOnlySpan<Vector3>.Empty, Vector3.Zero, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConvexHullContains_DegenerateFaceTriangleSkipped_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // The valid tetrahedron faces classify (0.1, 0.1, 0.1) as inside. We add a
        // degenerate (collinear-vertex) "face" whose unnormalized cross product is zero.
        // Without the explicit skip, the normalized signed-distance computation would
        // divide by zero and produce NaN; the skip branch keeps the rest of the test
        // running normally.
        Vector3[] hull =
        [
            // Bottom face (-Z out)
            new(0, 0, 0), new(0, 1, 0), new(1, 0, 0),
            // -Y face
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1),
            // -X face
            new(0, 0, 0), new(0, 0, 1), new(0, 1, 0),
            // Slanted face
            new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
            // Degenerate triangle: three collinear vertices.
            new(0, 0, 0), new(1, 0, 0), new(2, 0, 0),
        ];
        await Assert.That(Containment.ConvexHullContains(hull, new Vector3(0.1f, 0.1f, 0.1f), Tol)).IsTrue();
    }

    [Test]
    public async Task ConvexHullContains_LargeFace_PerpendicularDistanceWithinTolerance_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pins the dimensionally-correct face-plane test. The hull is a 100x-scaled
        // tetrahedron, so the slanted face's outward-normal magnitude (twice the face
        // area) is sqrt(3) * 10000 ~= 17320. A test point 0.001 perpendicular OUTSIDE
        // the slanted face — well within tolerance 0.01 — produces a raw signed-dot
        // value around 17.32 (= 0.001 * 17320), which would falsely exceed the
        // tolerance if compared directly. Dividing by the normal length recovers the
        // true 0.001 perpendicular distance, which is correctly within tolerance.
        const float scale = 100f;
        var sx = new Vector3(scale, 0, 0);
        var sy = new Vector3(0, scale, 0);
        var sz = new Vector3(0, 0, scale);

        Vector3[] hull =
        [
            Vector3.Zero, sy, sx,
            Vector3.Zero, sx, sz,
            Vector3.Zero, sz, sy,
            sx, sy, sz,
        ];

        var centroidOfSlantedFace = (sx + sy + sz) / 3f;
        var outwardUnit = Vector3.Normalize(new Vector3(1, 1, 1));
        var pointJustOutside = centroidOfSlantedFace + (0.001f * outwardUnit);

        await Assert.That(Containment.ConvexHullContains(hull, pointJustOutside, 0.01)).IsTrue();
    }

    // ----- Distance: point-to-Plane -----

    [Test]
    public async Task DistanceFromPlane_PointAbovePlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Plane y = 0 with upward normal; point at y = 5 -> distance 5.
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(new Vector3(2, 5, -1).DistanceFrom(plane)).IsEqualTo(5.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromPlane_PointBelowPlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(new Vector3(0, -3, 0).DistanceFrom(plane)).IsEqualTo(3.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromPlane_PointOnPlane_Zero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(new Vector3(7, 0, 11).DistanceFrom(plane)).IsEqualTo(0.0).Within(Tol);
    }

    // ----- Distance: point-to-LineSegment -----

    [Test]
    public async Task DistanceFromSegment_PointAtMidpoint_Zero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var seg = new LineSegment3D(Vector3.Zero, new Vector3(2, 0, 0));
        await Assert.That(new Vector3(1, 0, 0).DistanceFrom(seg)).IsEqualTo(0.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromSegment_PointPerpendicularToInterior(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var seg = new LineSegment3D(Vector3.Zero, new Vector3(2, 0, 0));
        // Project to (1, 0, 0); distance is 3 (the y component).
        await Assert.That(new Vector3(1, 3, 0).DistanceFrom(seg)).IsEqualTo(3.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromSegment_PointBeyondStartEndpoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Projection parameter < 0; nearest point is the start endpoint.
        var seg = new LineSegment3D(Vector3.Zero, new Vector3(2, 0, 0));
        await Assert.That(new Vector3(-3, 0, 0).DistanceFrom(seg)).IsEqualTo(3.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromSegment_PointBeyondEndEndpoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Projection parameter > 1; nearest point is the end endpoint.
        var seg = new LineSegment3D(Vector3.Zero, new Vector3(2, 0, 0));
        await Assert.That(new Vector3(5, 0, 0).DistanceFrom(seg)).IsEqualTo(3.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromSegment_DegenerateSegment_ReturnsPointDistance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pins the bit-magnitude zero check: a segment whose endpoints coincide produces
        // ab.LengthSquared == 0. The naive `t = dot / 0` would yield NaN and contaminate
        // the answer; the explicit early return uses point-to-point distance instead.
        var seg = new LineSegment3D(Vector3.UnitX, Vector3.UnitX);
        await Assert.That(new Vector3(4, 0, 0).DistanceFrom(seg)).IsEqualTo(3.0).Within(Tol);
    }

    // ----- Distance: point-to-Triangle -----

    [Test]
    public async Task DistanceFromTriangle_PointAtCentroid_Zero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Right triangle in z=0 plane; query the centroid -> distance 0.
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(t.Centroid.DistanceFrom(t)).IsEqualTo(0.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointAboveTriangleInterior(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Triangle in z=0 plane; query at (1, 1, 5) projects to interior, distance 5.
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(new Vector3(1, 1, 5).DistanceFrom(t)).IsEqualTo(5.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestVertexA(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Query well outside the triangle on the A side.
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(new Vector3(-4, -4, 0).DistanceFrom(t)).IsEqualTo(Math.Sqrt(32.0)).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestVertexB(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        // Query right of B and below: closest is B.
        await Assert.That(new Vector3(7, -4, 0).DistanceFrom(t)).IsEqualTo(Math.Sqrt(32.0)).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestVertexC(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        // Query above C and left.
        await Assert.That(new Vector3(-4, 7, 0).DistanceFrom(t)).IsEqualTo(Math.Sqrt(32.0)).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestEdgeAB(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Triangle AB along X axis from (0,0) to (3,0); query (1.5, -2, 0) projects to
        // (1.5, 0, 0) on edge AB; distance = 2.
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(new Vector3(1.5f, -2, 0).DistanceFrom(t)).IsEqualTo(2.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestEdgeAC(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Triangle AC along Y axis from (0,0) to (0,3); query (-2, 1.5, 0).
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(new Vector3(-2, 1.5f, 0).DistanceFrom(t)).IsEqualTo(2.0).Within(Tol);
    }

    [Test]
    public async Task DistanceFromTriangle_PointNearestEdgeBC(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Edge BC from (3,0,0) to (0,3,0); midpoint is (1.5, 1.5, 0).
        // Query (1.5+t, 1.5+t, 0) for t along the outward normal (1,1,0)/sqrt(2).
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        // Pick query at (3, 3, 0); outward distance from (1.5, 1.5, 0) is sqrt(4.5).
        await Assert.That(new Vector3(3, 3, 0).DistanceFrom(t)).IsEqualTo(Math.Sqrt(4.5)).Within(Tol);
    }
}
