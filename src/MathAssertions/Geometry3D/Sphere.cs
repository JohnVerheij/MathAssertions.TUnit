using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Sphere defined by center and radius. The radius is intended to be non-negative; a
/// negative radius produces signed volume and signed surface area (the formulas remain
/// well-defined as polynomials in the radius) and is left to the caller to validate.
/// </summary>
/// <param name="Center">Center of the sphere in 3-space.</param>
/// <param name="Radius">Radius of the sphere.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Sphere(Vector3 Center, float Radius)
{
    /// <summary>Volume of the sphere: <c>4 / 3 * pi * r^3</c>.</summary>
    public float Volume => 4f / 3f * float.Pi * Radius * Radius * Radius;

    /// <summary>Surface area of the sphere: <c>4 * pi * r^2</c>.</summary>
    public float SurfaceArea => 4f * float.Pi * Radius * Radius;
}
