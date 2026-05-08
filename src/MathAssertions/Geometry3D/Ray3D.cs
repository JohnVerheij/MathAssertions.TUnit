using System.Numerics;
using System.Runtime.InteropServices;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Half-line in 3-space starting at <see cref="Origin"/> and extending along
/// <see cref="Direction"/>. The direction is intended to be a unit vector but is not
/// validated; non-unit directions parameterize the same geometric ray with rescaled
/// <c>t</c> values from intersection methods.
/// </summary>
/// <param name="Origin">Starting point of the ray.</param>
/// <param name="Direction">Direction the ray extends in. Should be a unit vector for
/// <c>t</c> values returned from intersection methods to represent world-space distance.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Ray3D(Vector3 Origin, Vector3 Direction);
