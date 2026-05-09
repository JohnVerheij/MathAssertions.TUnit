using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the computed properties of the eight 3D primitives (<see cref="Sphere"/>,
/// <see cref="AxisAlignedBox"/>, <see cref="OrientedBox"/>, <see cref="Ray3D"/>,
/// <see cref="LineSegment3D"/>, <see cref="Triangle3D"/>, <see cref="Capsule"/>,
/// <see cref="Cylinder"/>) plus the property-predicate methods on
/// <see cref="MathAssertions.Geometry3D.Properties"/>.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class Geometry3DPrimitivesTests
{
    private const double Tol = 1e-6;

    // ----- Sphere -----

    [Test]
    public async Task Sphere_Volume_UnitSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var s = new Sphere(Vector3.Zero, 1f);
        // Unit sphere volume = 4/3 * pi.
        await Assert.That((double)s.Volume).IsEqualTo(4.0 / 3.0 * Math.PI).Within(Tol);
    }

    [Test]
    public async Task Sphere_SurfaceArea_UnitSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var s = new Sphere(Vector3.Zero, 1f);
        // Unit sphere surface = 4 * pi.
        await Assert.That((double)s.SurfaceArea).IsEqualTo(4.0 * Math.PI).Within(Tol);
    }

    [Test]
    public async Task Sphere_Volume_RadiusTwo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var s = new Sphere(new Vector3(1, 2, 3), 2f);
        // 4/3 * pi * 8 = 32/3 * pi.
        await Assert.That((double)s.Volume).IsEqualTo(32.0 / 3.0 * Math.PI).Within(Tol);
    }

    [Test]
    public async Task Sphere_RecordEquality(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Sphere(Vector3.Zero, 1f);
        var b = new Sphere(Vector3.Zero, 1f);
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a == b).IsTrue();
    }

    // ----- AxisAlignedBox -----

    [Test]
    public async Task AxisAlignedBox_Center(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(new Vector3(0, 0, 0), new Vector3(2, 4, 6));
        await Assert.That(MathTolerance.IsApproximatelyEqual(box.Center, new Vector3(1, 2, 3), Tol)).IsTrue();
    }

    [Test]
    public async Task AxisAlignedBox_Size(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(new Vector3(-1, -2, -3), new Vector3(1, 2, 3));
        await Assert.That(MathTolerance.IsApproximatelyEqual(box.Size, new Vector3(2, 4, 6), Tol)).IsTrue();
    }

    [Test]
    public async Task AxisAlignedBox_HalfExtents(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(new Vector3(-1, -2, -3), new Vector3(1, 2, 3));
        await Assert.That(MathTolerance.IsApproximatelyEqual(box.HalfExtents, new Vector3(1, 2, 3), Tol)).IsTrue();
    }

    [Test]
    public async Task AxisAlignedBox_Volume_UnitCube(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        await Assert.That((double)box.Volume).IsEqualTo(1.0).Within(Tol);
    }

    [Test]
    public async Task AxisAlignedBox_Volume_TwoByThreeByFour(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(Vector3.Zero, new Vector3(2, 3, 4));
        await Assert.That((double)box.Volume).IsEqualTo(24.0).Within(Tol);
    }

    // ----- OrientedBox -----

    [Test]
    public async Task OrientedBox_RecordEquality(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new OrientedBox(Vector3.Zero, Vector3.One, Quaternion.Identity);
        var b = new OrientedBox(Vector3.Zero, Vector3.One, Quaternion.Identity);
        await Assert.That(a).IsEqualTo(b);
    }

    // ----- Ray3D -----

    [Test]
    public async Task Ray3D_RecordEquality(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Ray3D(Vector3.Zero, Vector3.UnitX);
        var b = new Ray3D(Vector3.Zero, Vector3.UnitX);
        await Assert.That(a).IsEqualTo(b);
    }

    // ----- LineSegment3D -----

    [Test]
    public async Task LineSegment3D_Direction(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var seg = new LineSegment3D(new Vector3(1, 2, 3), new Vector3(4, 6, 8));
        await Assert.That(MathTolerance.IsApproximatelyEqual(seg.Direction, new Vector3(3, 4, 5), Tol)).IsTrue();
    }

    [Test]
    public async Task LineSegment3D_Length(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // |(3, 4, 5)| = sqrt(50).
        var seg = new LineSegment3D(new Vector3(1, 2, 3), new Vector3(4, 6, 8));
        await Assert.That((double)seg.Length).IsEqualTo(Math.Sqrt(50.0)).Within(Tol);
    }

    [Test]
    public async Task LineSegment3D_LengthOfZeroSegment(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var seg = new LineSegment3D(Vector3.UnitX, Vector3.UnitX);
        await Assert.That((double)seg.Length).IsEqualTo(0.0).Within(Tol);
    }

    // ----- Triangle3D -----

    [Test]
    public async Task Triangle3D_Area_RightTriangleInXYPlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Vertices (0,0), (3,0), (0,4); area = 6.
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 4, 0));
        await Assert.That((double)t.Area).IsEqualTo(6.0).Within(Tol);
    }

    [Test]
    public async Task Triangle3D_Centroid(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(MathTolerance.IsApproximatelyEqual(t.Centroid, new Vector3(1, 1, 0), Tol)).IsTrue();
    }

    [Test]
    public async Task Triangle3D_Normal_XYPlaneIsZUp(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Right-hand rule on (B-A) x (C-A) for a CCW triangle in the XY plane gives +Z.
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);
        await Assert.That(MathTolerance.IsApproximatelyEqual(t.Normal, Vector3.UnitZ, Tol)).IsTrue();
    }

    [Test]
    public async Task Triangle3D_Area_DegenerateIsZero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Three collinear vertices.
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, new Vector3(2, 0, 0));
        await Assert.That((double)t.Area).IsEqualTo(0.0).Within(Tol);
    }

    // ----- Capsule -----

    [Test]
    public async Task Capsule_RecordEquality(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Capsule(Vector3.Zero, Vector3.UnitY, 0.5f);
        var b = new Capsule(Vector3.Zero, Vector3.UnitY, 0.5f);
        await Assert.That(a).IsEqualTo(b);
    }

    // ----- Cylinder -----

    [Test]
    public async Task Cylinder_RecordEquality(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Cylinder(Vector3.Zero, Vector3.UnitY, 0.5f);
        var b = new Cylinder(Vector3.Zero, Vector3.UnitY, 0.5f);
        await Assert.That(a).IsEqualTo(b);
    }

    // ----- Properties.IsDegenerate -----

    [Test]
    public async Task IsDegenerate_NonDegenerateTriangle_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);
        await Assert.That(t.IsDegenerate(Tol)).IsFalse();
    }

    [Test]
    public async Task IsDegenerate_CollinearTriangle_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, new Vector3(2, 0, 0));
        await Assert.That(t.IsDegenerate(Tol)).IsTrue();
    }

    [Test]
    public async Task IsDegenerate_CoincidentVertices_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, Vector3.Zero, Vector3.Zero);
        await Assert.That(t.IsDegenerate(Tol)).IsTrue();
    }

    [Test]
    public async Task IsDegenerate_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);
        await Assert.That(() => t.IsDegenerate(-1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- Properties.IsCollinear -----

    [Test]
    public async Task IsCollinear_FewerThanThreePoints_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Properties.IsCollinear(ReadOnlySpan<Vector3>.Empty, Tol)).IsTrue();
        ReadOnlySpan<Vector3> single = [Vector3.UnitX];
        await Assert.That(Properties.IsCollinear(single, Tol)).IsTrue();
        ReadOnlySpan<Vector3> two = [Vector3.UnitX, Vector3.UnitY];
        await Assert.That(Properties.IsCollinear(two, Tol)).IsTrue();
    }

    [Test]
    public async Task IsCollinear_ThreeCollinearPoints_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> points = [Vector3.Zero, Vector3.UnitX, new(2, 0, 0)];
        await Assert.That(Properties.IsCollinear(points, Tol)).IsTrue();
    }

    [Test]
    public async Task IsCollinear_ThreeNonCollinearPoints_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> points = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];
        await Assert.That(Properties.IsCollinear(points, Tol)).IsFalse();
    }

    [Test]
    public async Task IsCollinear_AllPointsCoincide_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pins the leading-coincident-points fix: a naive impl that locks in
        // points[1] - points[0] would treat all-zero direction as a valid line and
        // return true even if the rest of the points are scattered. Here all points
        // do coincide, so true is correct.
        ReadOnlySpan<Vector3> points = [Vector3.UnitX, Vector3.UnitX, Vector3.UnitX];
        await Assert.That(Properties.IsCollinear(points, Tol)).IsTrue();
    }

    [Test]
    public async Task IsCollinear_LeadingCoincidentThenScattered_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // First two points coincide, third introduces a direction, fourth deviates from
        // that direction. The fix must skip the zero leading direction and use the
        // first non-trivial one.
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.Zero,
            Vector3.UnitX,
            Vector3.UnitY,
        ];
        await Assert.That(Properties.IsCollinear(points, Tol)).IsFalse();
    }

    [Test]
    public async Task IsCollinear_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => Properties.IsCollinear(ReadOnlySpan<Vector3>.Empty, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IsCollinear_LongBaselineSmallPerpendicular_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pins the dimensionally-correct cross-product-over-direction-length check. The
        // baseline from p0 to p1 is length 1000; p2 deviates by only 0.001 perpendicular
        // to that baseline, well below tolerance 0.01. A naive impl that compares
        // |direction × delta| to tolerance directly would reject this (the cross-product
        // magnitude is |direction|·perpDist = 1000·0.001 = 1.0, which appears far above
        // 0.01) even though the true perpendicular distance is in tolerance.
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            new(1000, 0, 0),
            new(500, 0.001f, 0),
        ];
        await Assert.That(Properties.IsCollinear(points, 0.01)).IsTrue();
    }

    // ----- Properties.AreCoplanar -----

    [Test]
    public async Task AreCoplanar_FewerThanFourPoints_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Properties.AreCoplanar(ReadOnlySpan<Vector3>.Empty, Tol)).IsTrue();
        ReadOnlySpan<Vector3> three = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];
        await Assert.That(Properties.AreCoplanar(three, Tol)).IsTrue();
    }

    [Test]
    public async Task AreCoplanar_FourPointsInXYPlane_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.UnitX,
            Vector3.UnitY,
            new(1, 1, 0),
        ];
        await Assert.That(Properties.AreCoplanar(points, Tol)).IsTrue();
    }

    [Test]
    public async Task AreCoplanar_FourthPointAboveXYPlane_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.UnitX,
            Vector3.UnitY,
            new(1, 1, 5),
        ];
        await Assert.That(Properties.AreCoplanar(points, Tol)).IsFalse();
    }

    [Test]
    public async Task AreCoplanar_AllCollinearPoints_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // A line lies on infinitely many planes; every collinear point set is trivially
        // coplanar. Pins the no-non-collinear-triple fallback.
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.UnitX,
            new(2, 0, 0),
            new(3, 0, 0),
            new(4, 0, 0),
        ];
        await Assert.That(Properties.AreCoplanar(points, Tol)).IsTrue();
    }

    [Test]
    public async Task AreCoplanar_LeadingCollinearTriple_StillFindsNonCollinearLater(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // First three points are collinear along X, then a fourth point off the X axis.
        // A naive impl that locks in the first triple's degenerate cross would return
        // true (NaN normal makes every dot-product check vacuously fail "> tolerance").
        // The corrected impl scans for a non-collinear pair and uses it.
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.UnitX,
            new(2, 0, 0),
            Vector3.UnitY,
        ];
        await Assert.That(Properties.AreCoplanar(points, Tol)).IsTrue();
    }

    [Test]
    public async Task AreCoplanar_LeadingCollinearTriple_DetectsOutOfPlaneFifth(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Same leading-collinear scenario, but the fifth point is out of the XY plane.
        ReadOnlySpan<Vector3> points =
        [
            Vector3.Zero,
            Vector3.UnitX,
            new(2, 0, 0),
            Vector3.UnitY,
            new(0, 1, 5),
        ];
        await Assert.That(Properties.AreCoplanar(points, Tol)).IsFalse();
    }

    [Test]
    public async Task AreCoplanar_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => Properties.AreCoplanar(ReadOnlySpan<Vector3>.Empty, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AreCoplanar_NaNTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => Properties.AreCoplanar(ReadOnlySpan<Vector3>.Empty, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }
}
