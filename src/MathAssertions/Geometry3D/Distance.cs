using System;
using System.Numerics;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Closest-point distance functions from a <see cref="Vector3"/> to selected geometry
/// primitives. Implemented as extension methods so call sites read naturally
/// (<c>point.DistanceFrom(plane)</c>, <c>point.DistanceFrom(segment)</c>).
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
        double signed = Vector3.Dot(point, plane.Normal) + plane.D;
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
        var ab = seg.End - seg.Start;
        var abDotAb = Vector3.Dot(ab, ab);

        // Degenerate segment: both endpoints coincide, so |ab|^2 is +0 or -0. Detected
        // via bit-magnitude rather than `== 0f` to keep the operator-== floating-point
        // analyzer flag (S1244) silent on the value path. The same pattern
        // MathTolerance.IsCloseInUlps uses for its zero-magnitude short-circuit.
        const int MagnitudeMask = 0x7FFF_FFFF;
        if ((BitConverter.SingleToInt32Bits(abDotAb) & MagnitudeMask) is 0)
        {
            return Vector3.Distance(point, seg.Start);
        }

        var t = Vector3.Dot(point - seg.Start, ab) / abDotAb;
        t = Math.Clamp(t, 0f, 1f);
        var closest = seg.Start + (t * ab);
        return Vector3.Distance(point, closest);
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
    /// <see cref="float.NaN"/> when the projection lands in the interior path (the
    /// underlying division by zero produces NaN), but degenerate inputs that exit
    /// through the vertex or edge branches return a finite distance. Callers needing a
    /// well-defined verdict on degenerate geometry should guard with
    /// <see cref="Properties.IsDegenerate(Triangle3D, double)"/>.
    /// </remarks>
    /// <param name="point">Point to measure from.</param>
    /// <param name="triangle">Triangle to measure to.</param>
    /// <returns>The unsigned closest-point distance.</returns>
    public static double DistanceFrom(this Vector3 point, Triangle3D triangle)
        => Vector3.Distance(point, ClosestPointOn(triangle, point));

    /// <summary>
    /// Closest point of <paramref name="triangle"/> to <paramref name="point"/>: the Voronoi region
    /// classification of Ericson, <i>Real-Time Collision Detection</i>, §5.1.5, in the order vertex A, vertex B,
    /// edge AB, vertex C, edge AC, edge BC, interior.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point to measure from.</param>
    /// <returns>The point of the triangle nearest to <paramref name="point"/>.</returns>
    private static Vector3 ClosestPointOn(Triangle3D triangle, Vector3 point)
    {
        var ab = triangle.B - triangle.A;
        var ac = triangle.C - triangle.A;
        var ap = point - triangle.A;
        var d1 = Vector3.Dot(ab, ap);
        var d2 = Vector3.Dot(ac, ap);
        if (IsInVertexARegion(d1, d2))
            return triangle.A;

        var bp = point - triangle.B;
        var d3 = Vector3.Dot(ab, bp);
        var d4 = Vector3.Dot(ac, bp);
        if (IsInVertexBRegion(d3, d4))
            return triangle.B;

        var vc = (d1 * d4) - (d3 * d2);
        if (IsInEdgeRegion(vc, d1, d3))
            return triangle.A + (d1 / (d1 - d3) * ab);

        var cp = point - triangle.C;
        var d5 = Vector3.Dot(ab, cp);
        var d6 = Vector3.Dot(ac, cp);
        if (IsInVertexCRegion(d5, d6))
            return triangle.C;

        var vb = (d5 * d2) - (d1 * d6);
        if (IsInEdgeRegion(vb, d2, d6))
            return triangle.A + (d2 / (d2 - d6) * ac);

        var va = (d3 * d6) - (d5 * d4);
        if (IsInEdgeBcRegion(va, d3, d4, d5, d6))
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return triangle.B + (w * (triangle.C - triangle.B));
        }

        // Projection lies inside the triangle; reconstruct the foot of the perpendicular.
        var denom = 1f / (va + vb + vc);
        var vBary = vb * denom;
        var wBary = vc * denom;
        return triangle.A + (vBary * ab) + (wBary * ac);
    }

    /// <summary>Whether the projection falls in the Voronoi region of vertex A.</summary>
    /// <param name="d1">Dot of AB with the vector from A to the point.</param>
    /// <param name="d2">Dot of AC with the vector from A to the point.</param>
    /// <returns><see langword="true"/> when both dots are non-positive.</returns>
    private static bool IsInVertexARegion(float d1, float d2) => d1 <= 0 && d2 <= 0;

    /// <summary>Whether the projection falls in the Voronoi region of vertex B.</summary>
    /// <param name="d3">Dot of AB with the vector from B to the point.</param>
    /// <param name="d4">Dot of AC with the vector from B to the point.</param>
    /// <returns><see langword="true"/> when <paramref name="d3"/> is non-negative and <paramref name="d4"/> does not exceed it.</returns>
    private static bool IsInVertexBRegion(float d3, float d4) => d3 >= 0 && d4 <= d3;

    /// <summary>Whether the projection falls in the Voronoi region of vertex C.</summary>
    /// <param name="d5">Dot of AB with the vector from C to the point.</param>
    /// <param name="d6">Dot of AC with the vector from C to the point.</param>
    /// <returns><see langword="true"/> when <paramref name="d6"/> is non-negative and <paramref name="d5"/> does not exceed it.</returns>
    private static bool IsInVertexCRegion(float d5, float d6) => d6 >= 0 && d5 <= d6;

    /// <summary>Whether the projection falls in the Voronoi region of edge AB or edge AC.</summary>
    /// <param name="signedArea">The barycentric area term of the opposite vertex.</param>
    /// <param name="dotAtStart">Dot of the edge with the vector from its start vertex to the point.</param>
    /// <param name="dotAtEnd">Dot of the edge with the vector from the other vertex to the point.</param>
    /// <returns><see langword="true"/> when the point projects onto the edge.</returns>
    private static bool IsInEdgeRegion(float signedArea, float dotAtStart, float dotAtEnd)
        => signedArea <= 0 && dotAtStart >= 0 && dotAtEnd <= 0;

    /// <summary>Whether the projection falls in the Voronoi region of edge BC.</summary>
    /// <param name="va">The barycentric area term of vertex A.</param>
    /// <param name="d3">Dot of AB with the vector from B to the point.</param>
    /// <param name="d4">Dot of AC with the vector from B to the point.</param>
    /// <param name="d5">Dot of AB with the vector from C to the point.</param>
    /// <param name="d6">Dot of AC with the vector from C to the point.</param>
    /// <returns><see langword="true"/> when the point projects onto edge BC.</returns>
    private static bool IsInEdgeBcRegion(float va, float d3, float d4, float d5, float d6)
        => va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0;
}
