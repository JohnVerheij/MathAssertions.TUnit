using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Triangle defined by three vertices in 3-space. Vertex order determines the orientation
/// of the surface normal (right-hand rule on <c>(B - A) x (C - A)</c>).
/// </summary>
/// <param name="A">First vertex.</param>
/// <param name="B">Second vertex.</param>
/// <param name="C">Third vertex.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Triangle3D(Vector3 A, Vector3 B, Vector3 C)
{
    /// <summary>
    /// Unit-length surface normal computed by the right-hand rule on edges <c>AB</c> and
    /// <c>AC</c>. For degenerate triangles (collinear or coincident vertices), the
    /// underlying cross product is the zero vector and normalization yields <c>NaN</c>
    /// components; check <see cref="Properties.IsDegenerate(Triangle3D, double)"/> first
    /// when the input triangle's geometry is not pre-validated.
    /// </summary>
    public Vector3 Normal => Vector3.Normalize(Vector3.Cross(B - A, C - A));

    /// <summary>Centroid of the triangle: <c>(A + B + C) / 3</c>.</summary>
    public Vector3 Centroid => (A + B + C) / 3f;

    /// <summary>
    /// Triangle area: <c>|AB x AC| / 2</c>. Returns zero for degenerate
    /// (collinear or coincident-vertex) triangles.
    /// </summary>
    public float Area => Vector3.Cross(B - A, C - A).Length() * 0.5f;
}
