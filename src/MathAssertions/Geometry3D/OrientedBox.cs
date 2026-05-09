using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Oriented bounding box (OBB), defined by a center, three half-extents along the box's
/// local axes, and a quaternion rotation that maps local-axis space to world space. The
/// half-extents are the distances from the center to the box face along each local axis;
/// negative half-extents are accepted but produce undefined containment results.
/// </summary>
/// <param name="Center">World-space center of the box.</param>
/// <param name="HalfExtents">Half-extents along the box's local X, Y, Z axes.</param>
/// <param name="Orientation">Rotation from local to world space.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct OrientedBox(Vector3 Center, Vector3 HalfExtents, Quaternion Orientation);
