using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Axis-aligned bounding box (AABB) defined by minimum and maximum corners. The box is
/// well-formed when <c>Min &lt;= Max</c> component-wise; the computed properties remain
/// well-defined for inverted boxes (<c>Size</c> components turn negative, <c>Volume</c>
/// becomes signed) but containment and intersection methods on inverted boxes return
/// undefined results. Callers are responsible for constructing valid boxes.
/// </summary>
/// <param name="Min">Minimum corner.</param>
/// <param name="Max">Maximum corner.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct AxisAlignedBox(Vector3 Min, Vector3 Max)
{
    /// <summary>Geometric center of the box: <c>(Min + Max) / 2</c>.</summary>
    public Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>Component-wise size: <c>Max - Min</c>.</summary>
    public Vector3 Size => Max - Min;

    /// <summary>Component-wise half-extents: <c>Size / 2</c>.</summary>
    public Vector3 HalfExtents => Size * 0.5f;

    /// <summary>Volume of the box: <c>Size.X * Size.Y * Size.Z</c>. Signed for inverted boxes.</summary>
    public float Volume
    {
        get
        {
            var s = Size;
            return s.X * s.Y * s.Z;
        }
    }
}
