using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Closest-point distance functions from a <see cref="Vector3"/> to selected geometry
/// primitives. Implemented as extension methods so call sites read naturally
/// (<c>point.DistanceFrom(plane)</c>, <c>point.DistanceFrom(segment)</c>). Every distance is
/// computed in <see cref="double"/>, so a tiny or huge <see cref="float"/> primitive neither
/// underflows nor overflows on the way.
/// </summary>
public static class Distance
{
    /// <summary>
    /// Perpendicular distance from <paramref name="point"/> to <paramref name="plane"/>.
    /// Assumes the plane normal is unit-length (the standard <see cref="Plane"/>
    /// convention from <c>Plane.CreateFromVertices</c>); otherwise the result is scaled
    /// by the normal's magnitude.
    /// </summary>
    /// <param name="point">Point to measure from.</param>
    /// <param name="plane">Plane to measure to.</param>
    /// <returns>The unsigned perpendicular distance.</returns>
    public static double DistanceFrom(this Vector3 point, Plane plane)
    {
        double signed = Vec.Dot(Vec.From(point), Vec.From(plane.Normal)) + plane.D;
        return Math.Abs(signed);
    }

    /// <summary>
    /// Closest-point distance from <paramref name="point"/> to the finite line segment
    /// <paramref name="seg"/>. The closest point is either an interior projection (when
    /// the projection parameter is in <c>[0, 1]</c>) or the nearer endpoint (otherwise).
    /// </summary>
    /// <remarks>Reference: Ericson, <i>Real-Time Collision Detection</i>, §5.1.2.</remarks>
    /// <param name="point">Point to measure from.</param>
    /// <param name="seg">Segment to measure to.</param>
    /// <returns>The unsigned closest-point distance.</returns>
    public static double DistanceFrom(this Vector3 point, LineSegment3D seg)
    {
        var start = Vec.From(seg.Start);
        var p = Vec.From(point);
        var ab = Vec.From(seg.End) - start;
        var abDotAb = Vec.Dot(ab, ab);

        // Degenerate segment: both endpoints coincide, so |ab|^2 is +0 or -0. Detected
        // via bit-magnitude rather than `== 0` to keep the operator-== floating-point
        // analyzer flag (S1244) silent on the value path. The same pattern
        // MathTolerance.IsCloseInUlps uses for its zero-magnitude short-circuit.
        const long MagnitudeMask = 0x7FFF_FFFF_FFFF_FFFFL;
        if ((BitConverter.DoubleToInt64Bits(abDotAb) & MagnitudeMask) is 0L)
        {
            return p.DistanceTo(start);
        }

        var t = Vec.Dot(p - start, ab) / abDotAb;
        t = Math.Clamp(t, 0.0, 1.0);
        var closest = start + (t * ab);
        return p.DistanceTo(closest);
    }

    /// <summary>
    /// Closest-point distance from <paramref name="point"/> to <paramref name="triangle"/>.
    /// The closest point is on the triangle's interior, on one of its three edges, or at
    /// one of its three vertices; the implementation classifies which Voronoi region
    /// contains <paramref name="point"/>'s projection and returns the corresponding
    /// distance.
    /// </summary>
    /// <remarks>
    /// Reference: Ericson, <i>Real-Time Collision Detection</i>, §5.1.5. The barycentric
    /// classification is the standard one; for degenerate triangles the result may be
    /// <see cref="double.NaN"/> when the projection lands in the interior path (the
    /// underlying division by zero produces NaN), but degenerate inputs that exit
    /// through the vertex or edge branches return a finite distance. Callers needing a
    /// well-defined verdict on degenerate geometry should guard with
    /// <see cref="Properties.IsDegenerate(Triangle3D, double)"/>.
    /// </remarks>
    /// <param name="point">Point to measure from.</param>
    /// <param name="triangle">Triangle to measure to.</param>
    /// <returns>The unsigned closest-point distance.</returns>
    public static double DistanceFrom(this Vector3 point, Triangle3D triangle)
    {
        var p = Vec.From(point);
        return p.DistanceTo(ClosestPointOn(triangle, p));
    }

    /// <summary>Euclidean distance between two points, computed in <see cref="double"/>.</summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>The distance between the two points.</returns>
    internal static double DistanceInDouble(Vector3 a, Vector3 b) => Vec.From(a).DistanceTo(Vec.From(b));

    /// <summary>
    /// Squared Euclidean distance between two points, computed in <see cref="double"/>: a float squared
    /// distance overflows beyond about <c>1.8e19</c> and underflows below about <c>1e-23</c>.
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>The squared distance between the two points.</returns>
    internal static double SquaredDistanceInDouble(Vector3 a, Vector3 b)
    {
        var d = Vec.From(a) - Vec.From(b);
        return Vec.Dot(d, d);
    }

    /// <summary>
    /// Closest point of <paramref name="triangle"/> to <paramref name="p"/>: the Voronoi region
    /// classification of Ericson, <i>Real-Time Collision Detection</i>, §5.1.5, in the order vertex A, vertex B,
    /// edge AB, vertex C, edge AC, edge BC, interior. Computed in <see cref="double"/>: the float products of
    /// a tiny triangle underflow to zero and divide to NaN.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="p">The point to measure from.</param>
    /// <returns>The point of the triangle nearest to <paramref name="p"/>.</returns>
    private static Vec ClosestPointOn(Triangle3D triangle, Vec p)
    {
        var a = Vec.From(triangle.A);
        var b = Vec.From(triangle.B);
        var c = Vec.From(triangle.C);
        var ab = b - a;
        var ac = c - a;
        var ap = p - a;
        var d1 = Vec.Dot(ab, ap);
        var d2 = Vec.Dot(ac, ap);
        if (IsInVertexARegion(d1, d2))
            return a;

        var bp = p - b;
        var d3 = Vec.Dot(ab, bp);
        var d4 = Vec.Dot(ac, bp);
        if (IsInVertexBRegion(d3, d4))
            return b;

        var vc = (d1 * d4) - (d3 * d2);
        if (IsInEdgeRegion(vc, d1, d3))
            return a + (d1 / (d1 - d3) * ab);

        var cp = p - c;
        var d5 = Vec.Dot(ab, cp);
        var d6 = Vec.Dot(ac, cp);
        if (IsInVertexCRegion(d5, d6))
            return c;

        var vb = (d5 * d2) - (d1 * d6);
        if (IsInEdgeRegion(vb, d2, d6))
            return a + (d2 / (d2 - d6) * ac);

        var va = (d3 * d6) - (d5 * d4);
        if (IsInEdgeBcRegion(va, d3, d4, d5, d6))
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + (w * (c - b));
        }

        // Projection lies inside the triangle; reconstruct the foot of the perpendicular.
        var denom = 1.0 / (va + vb + vc);
        var vBary = vb * denom;
        var wBary = vc * denom;
        return a + (vBary * ab) + (wBary * ac);
    }

    /// <summary>Whether the projection falls in the Voronoi region of vertex A.</summary>
    /// <param name="d1">Dot of AB with the vector from A to the point.</param>
    /// <param name="d2">Dot of AC with the vector from A to the point.</param>
    /// <returns><see langword="true"/> when both dots are non-positive.</returns>
    private static bool IsInVertexARegion(double d1, double d2) => d1 <= 0 && d2 <= 0;

    /// <summary>Whether the projection falls in the Voronoi region of vertex B.</summary>
    /// <param name="d3">Dot of AB with the vector from B to the point.</param>
    /// <param name="d4">Dot of AC with the vector from B to the point.</param>
    /// <returns><see langword="true"/> when <paramref name="d3"/> is non-negative and <paramref name="d4"/> does not exceed it.</returns>
    private static bool IsInVertexBRegion(double d3, double d4) => d3 >= 0 && d4 <= d3;

    /// <summary>Whether the projection falls in the Voronoi region of vertex C.</summary>
    /// <param name="d5">Dot of AB with the vector from C to the point.</param>
    /// <param name="d6">Dot of AC with the vector from C to the point.</param>
    /// <returns><see langword="true"/> when <paramref name="d6"/> is non-negative and <paramref name="d5"/> does not exceed it.</returns>
    private static bool IsInVertexCRegion(double d5, double d6) => d6 >= 0 && d5 <= d6;

    /// <summary>Whether the projection falls in the Voronoi region of edge AB or edge AC.</summary>
    /// <param name="signedArea">The barycentric area term of the opposite vertex.</param>
    /// <param name="dotAtStart">Dot of the edge with the vector from its start vertex to the point.</param>
    /// <param name="dotAtEnd">Dot of the edge with the vector from the other vertex to the point.</param>
    /// <returns><see langword="true"/> when the point projects onto the edge.</returns>
    private static bool IsInEdgeRegion(double signedArea, double dotAtStart, double dotAtEnd)
        => signedArea <= 0 && dotAtStart >= 0 && dotAtEnd <= 0;

    /// <summary>Whether the projection falls in the Voronoi region of edge BC.</summary>
    /// <param name="va">The barycentric area term of vertex A.</param>
    /// <param name="d3">Dot of AB with the vector from B to the point.</param>
    /// <param name="d4">Dot of AC with the vector from B to the point.</param>
    /// <param name="d5">Dot of AB with the vector from C to the point.</param>
    /// <param name="d6">Dot of AC with the vector from C to the point.</param>
    /// <returns><see langword="true"/> when the point projects onto edge BC.</returns>
    private static bool IsInEdgeBcRegion(double va, double d3, double d4, double d5, double d6)
        => va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0;

    /// <summary>A point or vector in <see cref="double"/> precision, for the distance arithmetic.</summary>
    /// <param name="X">The X component.</param>
    /// <param name="Y">The Y component.</param>
    /// <param name="Z">The Z component.</param>
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Vec(double X, double Y, double Z)
    {
        /// <summary>Widens a <see cref="Vector3"/> to double precision.</summary>
        /// <param name="v">The vector.</param>
        /// <returns>The same components in double precision.</returns>
        public static Vec From(Vector3 v) => new(v.X, v.Y, v.Z);

        /// <summary>Component-wise sum.</summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>The sum.</returns>
        public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        /// <summary>Component-wise difference.</summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>The difference <c>a - b</c>.</returns>
        public static Vec operator -(Vec a, Vec b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /// <summary>Scales a vector.</summary>
        /// <param name="s">The factor.</param>
        /// <param name="v">The vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vec operator *(double s, Vec v) => new(s * v.X, s * v.Y, s * v.Z);

        /// <summary>Dot product.</summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>The dot product <c>a . b</c>.</returns>
        public static double Dot(Vec a, Vec b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

        /// <summary>Euclidean distance to another point.</summary>
        /// <param name="other">The other point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceTo(Vec other)
        {
            var d = this - other;
            return Math.Sqrt(Dot(d, d));
        }
    }
}
