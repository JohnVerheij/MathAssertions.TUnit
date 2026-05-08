using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Capsule defined by two axis endpoints and a radius. Geometrically, the capsule is the
/// set of points within <see cref="Radius"/> of the line segment from <see cref="Start"/>
/// to <see cref="End"/>: a cylinder of length <c>|End - Start|</c> with hemispherical caps.
/// </summary>
/// <param name="Start">First endpoint of the central axis.</param>
/// <param name="End">Second endpoint of the central axis.</param>
/// <param name="Radius">Radius around the axis.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Capsule(Vector3 Start, Vector3 End, float Radius);
