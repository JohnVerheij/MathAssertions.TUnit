using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Finite line segment from <see cref="Start"/> to <see cref="End"/>.
/// </summary>
/// <param name="Start">Starting endpoint of the segment.</param>
/// <param name="End">Ending endpoint of the segment.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct LineSegment3D(Vector3 Start, Vector3 End)
{
    /// <summary>
    /// Unnormalized direction from <see cref="Start"/> to <see cref="End"/>:
    /// <c>End - Start</c>. Magnitude equals <see cref="Length"/>.
    /// </summary>
    public Vector3 Direction => End - Start;

    /// <summary>Length of the segment.</summary>
    public float Length => Direction.Length();
}
