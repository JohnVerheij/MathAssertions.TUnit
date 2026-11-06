using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Half-line in 3-space starting at <see cref="Origin"/> and extending along
/// <see cref="Direction"/>. The direction is intended to be a unit vector but is not
/// validated. Ray-sphere intersection assumes a unit direction and gives wrong results
/// otherwise; ray-plane and ray-triangle report <c>t</c> in units of the direction length.
/// The parallel tests use an absolute epsilon of <c>1e-7</c>: ray-plane on
/// <c>|direction . normal|</c> (so a near-zero direction counts as parallel, exactly for a
/// unit-length normal), ray-triangle on <c>|direction . (edge2 x edge1)|</c> (the cutoff scales
/// with the triangle size), and ray-AABB per direction component (a near-zero component requires
/// the origin to lie inside that axis's slab, so a near-zero direction hits only when the origin
/// is inside the box).
/// </summary>
/// <param name="Origin">Starting point of the ray.</param>
/// <param name="Direction">Direction the ray extends in. Should be a unit vector for
/// <c>t</c> values returned from intersection methods to represent world-space distance.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Ray3D(Vector3 Origin, Vector3 Direction);
