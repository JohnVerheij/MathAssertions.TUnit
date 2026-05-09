using System;
using System.Numerics;

namespace MathAssertions.Geometry3D;

/// <summary>
/// Aggregate predicates over a <see cref="ReadOnlySpan{T}"/> of <see cref="Vector3"/>:
/// boundedness, centroid, and "approximately on a known surface" checks. Each helper is
/// an extension method for natural call-site syntax (<c>cloud.IsBoundedBy(box)</c>,
/// <c>cloud.HasCentroidAt(expected, tolerance)</c>).
/// </summary>
public static class Pointcloud
{
    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="cloud"/> lies
    /// inside (or on the boundary of) <paramref name="box"/>. The empty cloud is
    /// vacuously bounded.
    /// </summary>
    public static bool IsBoundedBy(this ReadOnlySpan<Vector3> cloud, AxisAlignedBox box)
    {
        foreach (var p in cloud)
        {
            if (!box.Contains(p))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="cloud"/> lies
    /// inside (or on the boundary of) <paramref name="sphere"/>. The empty cloud is
    /// vacuously bounded.
    /// </summary>
    public static bool IsBoundedBy(this ReadOnlySpan<Vector3> cloud, Sphere sphere)
    {
        foreach (var p in cloud)
        {
            if (!sphere.Contains(p))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the arithmetic centroid of
    /// <paramref name="cloud"/> is within <paramref name="tolerance"/> of
    /// <paramref name="expected"/> component-wise. The empty cloud has no centroid and
    /// returns <see langword="false"/>.
    /// </summary>
    /// <param name="cloud">Point cloud.</param>
    /// <param name="expected">Expected centroid.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component. Must
    /// be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the centroid matches within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasCentroidAt(this ReadOnlySpan<Vector3> cloud, Vector3 expected, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (cloud.Length == 0)
            return false;

        var sum = Vector3.Zero;
        foreach (var p in cloud)
        {
            sum += p;
        }
        var centroid = sum / cloud.Length;
        return MathTolerance.IsApproximatelyEqual(centroid, expected, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="cloud"/> lies
    /// within <paramref name="maxResidual"/> perpendicular distance of
    /// <paramref name="plane"/>. The empty cloud is vacuously on the plane.
    /// </summary>
    /// <param name="cloud">Point cloud.</param>
    /// <param name="plane">Reference plane (assumes unit-length normal, the standard
    /// <see cref="Plane.CreateFromVertices(Vector3, Vector3, Vector3)"/> convention).</param>
    /// <param name="maxResidual">Maximum allowed perpendicular distance from the plane.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if every point is within the residual.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxResidual"/> is NaN or negative.</exception>
    public static bool IsApproximatelyOnPlane(
        this ReadOnlySpan<Vector3> cloud,
        Plane plane,
        double maxResidual)
    {
        ValidateTolerance(maxResidual, nameof(maxResidual));

        foreach (var p in cloud)
        {
            if (p.DistanceFrom(plane) > maxResidual)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when every point in <paramref name="cloud"/> lies
    /// within <paramref name="maxResidual"/> radial distance of the sphere defined by
    /// <paramref name="center"/> and <paramref name="radius"/>: the absolute difference
    /// between point-to-center distance and the radius is bounded by the residual. The
    /// empty cloud is vacuously on the sphere.
    /// </summary>
    /// <param name="cloud">Point cloud.</param>
    /// <param name="center">Sphere center.</param>
    /// <param name="radius">Sphere radius.</param>
    /// <param name="maxResidual">Maximum allowed radial deviation. Must be non-negative
    /// and not NaN.</param>
    /// <returns><see langword="true"/> if every point is within the residual band.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxResidual"/> is NaN or negative.</exception>
    public static bool IsApproximatelyOnSphere(
        this ReadOnlySpan<Vector3> cloud,
        Vector3 center,
        float radius,
        double maxResidual)
    {
        ValidateTolerance(maxResidual, nameof(maxResidual));

        foreach (var p in cloud)
        {
            var residual = Math.Abs((double)Vector3.Distance(p, center) - radius);
            if (residual > maxResidual)
                return false;
        }
        return true;
    }

    private static void ValidateTolerance(double tolerance, string paramName = "tolerance")
    {
        if (double.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(paramName, "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Tolerance cannot be negative.");
        }
    }
}
