using System;
using System.Numerics;

namespace MathAssertions;

/// <summary>
/// Tolerance-comparison helpers for floating-point primitives and a single
/// <see cref="System.Numerics.Vector3"/> overload. NaN-aware, infinity-aware,
/// allocation-free. Callable from any test framework or production code.
/// </summary>
/// <remarks>
/// <para>
/// The <c>MathAssertions.TUnit</c> adapter delegates to these helpers from its
/// <c>[GenerateAssertion]</c> extensions; consumers' own <c>[GenerateAssertion]</c>
/// extensions on private types call them directly to inherit the same semantics.
/// </para>
/// <para>
/// The 0.0.1 surface is deliberately narrow: scalar plus a single Vector3 component-wise
/// comparison. The wider surface for Vector2/4, Quaternion, Matrix4x4, Plane, Complex,
/// span overloads, ULP-distance equality, relative+absolute combined tolerance, and
/// <c>IsRotationallyEquivalent</c> ships in 0.1.0.
/// </para>
/// </remarks>
public static class MathTolerance
{
    /// <summary>NaN-aware, infinity-aware tolerance comparison for two doubles.</summary>
    /// <remarks>
    /// Both NaN: returns <see langword="true"/>. One NaN: returns <see langword="false"/>.
    /// Same-sign infinity: returns <see langword="true"/>. Opposite-sign infinity: returns
    /// <see langword="false"/>. Both finite: returns <c>Math.Abs(a - b) &lt;= tolerance</c>.
    /// Mirrors TUnit's <c>IsCloseTo</c> primitive semantics.
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if approximately equal under the rules above.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(double a, double b, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;
        if (double.IsPositiveInfinity(a) && double.IsPositiveInfinity(b))
            return true;
        if (double.IsNegativeInfinity(a) && double.IsNegativeInfinity(b))
            return true;
        if (!double.IsFinite(a) || !double.IsFinite(b))
            return false;

        return Math.Abs(a - b) <= tolerance;
    }

    /// <summary>
    /// NaN-aware, infinity-aware tolerance comparison for two floats. Same semantics as the
    /// double overload; the float-typed signature lets callers stay in single precision when
    /// that is the natural value type.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="tolerance">Maximum allowed absolute difference. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(float a, float b, float tolerance)
    {
        ValidateTolerance(tolerance);

        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        if (float.IsNaN(a) || float.IsNaN(b))
            return false;
        if (float.IsPositiveInfinity(a) && float.IsPositiveInfinity(b))
            return true;
        if (float.IsNegativeInfinity(a) && float.IsNegativeInfinity(b))
            return true;
        if (!float.IsFinite(a) || !float.IsFinite(b))
            return false;

        return Math.Abs(a - b) <= tolerance;
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Vector3"/> values.
    /// </summary>
    /// <remarks>
    /// Component values widen to <see cref="double"/> before the per-axis comparison so the
    /// caller's <c>double</c> tolerance is honored at full precision. Casting the tolerance
    /// down to <c>float</c> instead would discard up to 22 bits of mantissa for tight
    /// tolerances such as <c>1e-9</c> and produce surprising near-equal-on-every-component
    /// results; the fix is a load-bearing semantic for the rest of the assertion family.
    /// </remarks>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns>
    /// <see langword="true"/> if every component is approximately equal under
    /// <see cref="IsApproximatelyEqual(double, double, double)"/> semantics.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Vector3 a, Vector3 b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual((double)a.X, (double)b.X, tolerance)
            && IsApproximatelyEqual((double)a.Y, (double)b.Y, tolerance)
            && IsApproximatelyEqual((double)a.Z, (double)b.Z, tolerance);
    }

    private static void ValidateTolerance(double tolerance)
    {
        if (double.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");
        }
    }

    private static void ValidateTolerance(float tolerance)
    {
        if (float.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");
        }
    }
}
