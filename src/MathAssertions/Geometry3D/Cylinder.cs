using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Finite cylinder defined by two axis endpoints and a radius. Distinct from
/// <see cref="Capsule"/>: the cylinder's caps are flat disks, not hemispheres.
/// </summary>
/// <param name="Start">First endpoint of the central axis.</param>
/// <param name="End">Second endpoint of the central axis.</param>
/// <param name="Radius">Radius around the axis.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Cylinder(Vector3 Start, Vector3 End, float Radius);
