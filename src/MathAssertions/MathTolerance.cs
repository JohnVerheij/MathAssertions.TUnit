using System;
using System.Numerics;
using System.Numerics.Tensors;

namespace MathAssertions;

/// <summary>
/// Tolerance-comparison helpers for floating-point primitives and selected
/// <see cref="System.Numerics"/> types. NaN-aware, infinity-aware, allocation-free.
/// Callable from any test framework or production code.
/// </summary>
/// <remarks>
/// <para>
/// The <c>MathAssertions.TUnit</c> adapter delegates to these helpers from its
/// <c>[GenerateAssertion]</c> extensions; consumers' own <c>[GenerateAssertion]</c>
/// extensions on private types call them directly to inherit the same semantics.
/// </para>
/// <para>
/// 0.1.0 Cluster 1 covers the tolerance primitives: NaN- and infinity-aware absolute
/// comparison for <see cref="double"/>/<see cref="float"/>, ULP-distance equality,
/// combined relative+absolute tolerance, finiteness predicates, probability/percentage
/// range checks, and an invertible-transformation roundtrip-identity helper. 0.1.0
/// Cluster 2 adds <see cref="System.Numerics"/> compounds: <see cref="Vector2"/>,
/// <see cref="Vector3"/>, <see cref="Vector4"/>, <see cref="Quaternion"/> (component-wise
/// plus rotational equivalence), <see cref="Matrix4x4"/>, <see cref="Plane"/>
/// (component-wise plus geometric equivalence under the <c>(n, d) = (-n, -d)</c>
/// sign-flip symmetry), <see cref="Complex"/>, <see cref="ReadOnlySpan{T}"/> for
/// <see cref="double"/> and <see cref="float"/>, and a generic
/// <see cref="ReadOnlyTensorSpan{T}"/> overload for any
/// <see cref="System.Numerics.INumber{TSelf}"/>.
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
    /// Component-wise tolerance comparison for two <see cref="Vector2"/> values.
    /// Components widen to <see cref="double"/> before comparing against the
    /// caller's <c>double</c> tolerance; see the <see cref="Vector3"/> overload
    /// for the full precision-preservation rationale.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns>
    /// <see langword="true"/> if every component is approximately equal under
    /// <see cref="IsApproximatelyEqual(double, double, double)"/> semantics.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Vector2 a, Vector2 b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual((double)a.X, (double)b.X, tolerance)
            && IsApproximatelyEqual((double)a.Y, (double)b.Y, tolerance);
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

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Vector4"/> values. Components
    /// widen to <see cref="double"/> before comparing against the caller's <c>double</c>
    /// tolerance; see the <see cref="Vector3"/> overload for the full precision-preservation
    /// rationale.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns>
    /// <see langword="true"/> if every component is approximately equal under
    /// <see cref="IsApproximatelyEqual(double, double, double)"/> semantics.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Vector4 a, Vector4 b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual((double)a.X, (double)b.X, tolerance)
            && IsApproximatelyEqual((double)a.Y, (double)b.Y, tolerance)
            && IsApproximatelyEqual((double)a.Z, (double)b.Z, tolerance)
            && IsApproximatelyEqual((double)a.W, (double)b.W, tolerance);
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Quaternion"/> values across
    /// X, Y, Z, W. Distinguishes the quaternion <c>q</c> from <c>-q</c>; for rotational
    /// equivalence (where <c>q</c> and <c>-q</c> represent the same rotation), use
    /// <see cref="IsRotationallyEquivalent(Quaternion, Quaternion, double)"/> instead.
    /// </summary>
    /// <param name="a">First quaternion.</param>
    /// <param name="b">Second quaternion.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns><see langword="true"/> if every component is approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Quaternion a, Quaternion b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual((double)a.X, (double)b.X, tolerance)
            && IsApproximatelyEqual((double)a.Y, (double)b.Y, tolerance)
            && IsApproximatelyEqual((double)a.Z, (double)b.Z, tolerance)
            && IsApproximatelyEqual((double)a.W, (double)b.W, tolerance);
    }

    /// <summary>
    /// Element-wise tolerance comparison for two <see cref="Matrix4x4"/> values across all
    /// sixteen elements. Elements widen to <see cref="double"/> before comparing against
    /// the caller's <c>double</c> tolerance.
    /// </summary>
    /// <param name="a">First matrix.</param>
    /// <param name="b">Second matrix.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element.</param>
    /// <returns><see langword="true"/> if every element is approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Matrix4x4 a, Matrix4x4 b, double tolerance)
    {
        ValidateTolerance(tolerance);

        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                if (!IsApproximatelyEqual((double)a[row, col], (double)b[row, col], tolerance))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Plane"/> values across
    /// <see cref="Plane.Normal"/> and <see cref="Plane.D"/>. Distinguishes a plane from
    /// its sign-flipped representation; for geometric equivalence (where <c>(n, d)</c>
    /// and <c>(-n, -d)</c> describe the same point set), use
    /// <see cref="IsGeometricallyEquivalent(Plane, Plane, double)"/> instead.
    /// </summary>
    /// <param name="a">First plane.</param>
    /// <param name="b">Second plane.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns><see langword="true"/> if every component is approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Plane a, Plane b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual(a.Normal, b.Normal, tolerance)
            && IsApproximatelyEqual((double)a.D, (double)b.D, tolerance);
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Complex"/> values across
    /// real and imaginary parts.
    /// </summary>
    /// <param name="a">First complex number.</param>
    /// <param name="b">Second complex number.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component.</param>
    /// <returns><see langword="true"/> if both real and imaginary parts are approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(Complex a, Complex b, double tolerance)
    {
        ValidateTolerance(tolerance);

        return IsApproximatelyEqual(a.Real, b.Real, tolerance)
            && IsApproximatelyEqual(a.Imaginary, b.Imaginary, tolerance);
    }

    /// <summary>
    /// Element-wise tolerance comparison for two <see cref="double"/> spans. A length
    /// mismatch returns <see langword="false"/> rather than throwing; explicit length
    /// validation belongs at higher layers (for example the array-overload extensions in
    /// the TUnit adapter, which throw on null inputs and surface length-mismatch as a
    /// dedicated assertion failure).
    /// </summary>
    /// <param name="a">First span.</param>
    /// <param name="b">Second span.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element.</param>
    /// <returns><see langword="true"/> if both spans have the same length and every
    /// element pair is approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(ReadOnlySpan<double> a, ReadOnlySpan<double> b, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (a.Length != b.Length)
            return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (!IsApproximatelyEqual(a[i], b[i], tolerance))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Element-wise tolerance comparison for two <see cref="float"/> spans. See the
    /// <see cref="double"/> span overload for length-mismatch semantics.
    /// </summary>
    /// <param name="a">First span.</param>
    /// <param name="b">Second span.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element.</param>
    /// <returns><see langword="true"/> if both spans have the same length and every
    /// element pair is approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual(ReadOnlySpan<float> a, ReadOnlySpan<float> b, float tolerance)
    {
        ValidateTolerance(tolerance);

        if (a.Length != b.Length)
            return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (!IsApproximatelyEqual(a[i], b[i], tolerance))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Element-wise tolerance comparison for two <see cref="ReadOnlyTensorSpan{T}"/>
    /// values for any <see cref="INumber{TSelf}"/>. Shape mismatch (different rank or
    /// different per-dimension length) returns <see langword="false"/>; only when both
    /// shapes match are the elements iterated and compared.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NaN handling matches the floating-point scalar overloads: both NaN at the same
    /// position counts as equal; one NaN counts as unequal. For integer <typeparamref name="T"/>
    /// the NaN branches are unreachable because <see cref="INumberBase{T}.IsNaN(T)"/>
    /// returns <see langword="false"/> for non-floating-point representations.
    /// </para>
    /// <para>
    /// Iteration uses the tensor span's own enumerator so strided (non-dense) shapes
    /// produced by tensor slicing work correctly.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Element type, any <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="a">First tensor span.</param>
    /// <param name="b">Second tensor span.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per element. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> when shapes match and every element pair is
    /// approximately equal.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsApproximatelyEqual<T>(
        ReadOnlyTensorSpan<T> a,
        ReadOnlyTensorSpan<T> b,
        T tolerance)
        where T : INumber<T>
    {
        if (T.IsNaN(tolerance))
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        if (tolerance < T.Zero)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");

        var aLengths = a.Lengths;
        var bLengths = b.Lengths;
        if (aLengths.Length != bLengths.Length)
            return false;
        for (var i = 0; i < aLengths.Length; i++)
        {
            if (aLengths[i] != bLengths[i])
                return false;
        }

        var ae = a.GetEnumerator();
        var be = b.GetEnumerator();
        while (ae.MoveNext() && be.MoveNext())
        {
            var av = ae.Current;
            var bv = be.Current;
            if (T.IsNaN(av) && T.IsNaN(bv))
                continue;
            if (T.IsNaN(av) || T.IsNaN(bv))
                return false;
            if (T.Abs(av - bv) > tolerance)
                return false;
        }
        return true;
    }

    /// <summary>
    /// ULP-distance equality for two doubles. Returns <see langword="true"/> when the two
    /// values are within <paramref name="ulpDistance"/> representable doubles of each other
    /// under IEEE 754.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ULP ("unit in the last place") distance is the count of representable floats between
    /// two values. It is the natural metric for "as close as floating-point can express,"
    /// scaling with the magnitude of the values without the caller picking an absolute
    /// tolerance. Values with opposite signs are never within any finite ULP distance and
    /// always compare as <see langword="false"/>; positive and negative zero are treated
    /// as equal.
    /// </para>
    /// <para>
    /// Reference: Goldberg, <i>What Every Computer Scientist Should Know About
    /// Floating-Point Arithmetic</i>, ACM Computing Surveys, 1991.
    /// </para>
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="ulpDistance">Maximum allowed number of representable doubles between
    /// <paramref name="a"/> and <paramref name="b"/>. Must be non-negative.</param>
    /// <returns><see langword="true"/> if both NaN, both zero (regardless of sign), or
    /// within <paramref name="ulpDistance"/> ULPs and same-signed.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ulpDistance"/> is negative.</exception>
    public static bool IsCloseInUlps(double a, double b, long ulpDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ulpDistance);

        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;

        var aBits = BitConverter.DoubleToInt64Bits(a);
        var bBits = BitConverter.DoubleToInt64Bits(b);

        // +0 and -0 are equal under IEEE 754 even though their bit patterns differ
        // by 2^63. The cheap test "value is zero" is "the magnitude bits are zero,"
        // which masking off the sign bit captures for both signs at once.
        const long MagnitudeMask = 0x7FFF_FFFF_FFFF_FFFFL;
        if ((aBits & MagnitudeMask) == 0 && (bBits & MagnitudeMask) == 0)
            return true;

        if (aBits == bBits)
            return true;

        if ((aBits < 0) != (bBits < 0))
            return false;

        return Math.Abs(aBits - bBits) <= ulpDistance;
    }

    /// <summary>
    /// ULP-distance equality for two floats. Returns <see langword="true"/> when the two
    /// values are within <paramref name="ulpDistance"/> representable floats of each other
    /// under IEEE 754. See the double overload for full semantics.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="ulpDistance">Maximum allowed number of representable floats between
    /// <paramref name="a"/> and <paramref name="b"/>. Must be non-negative.</param>
    /// <returns><see langword="true"/> if both NaN, both zero (regardless of sign), or
    /// within <paramref name="ulpDistance"/> ULPs and same-signed.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ulpDistance"/> is negative.</exception>
    public static bool IsCloseInUlps(float a, float b, int ulpDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ulpDistance);

        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        if (float.IsNaN(a) || float.IsNaN(b))
            return false;

        var aBits = BitConverter.SingleToInt32Bits(a);
        var bBits = BitConverter.SingleToInt32Bits(b);

        const int MagnitudeMask = 0x7FFF_FFFF;
        if ((aBits & MagnitudeMask) == 0 && (bBits & MagnitudeMask) == 0)
            return true;

        if (aBits == bBits)
            return true;

        if ((aBits < 0) != (bBits < 0))
            return false;

        return Math.Abs(aBits - bBits) <= ulpDistance;
    }

    /// <summary>
    /// Combined relative + absolute tolerance check. Returns <see langword="true"/> when
    /// <c>|a - b| &lt;= max(absoluteTolerance, relativeTolerance * max(|a|, |b|))</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The textbook combined-tolerance check from Knuth, <i>The Art of Computer
    /// Programming</i>, Vol. 2. The absolute term dominates near zero where the relative
    /// term collapses; the relative term dominates at large magnitudes where a fixed
    /// absolute tolerance would be unreasonably tight or unreasonably loose.
    /// </para>
    /// <para>
    /// Both NaN compare equal; one NaN compares unequal. When either operand is infinite,
    /// the values compare equal only when they have the same bit representation (same sign
    /// of infinity); a finite and an infinite value are unequal regardless of either
    /// tolerance.
    /// </para>
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="relativeTolerance">Relative tolerance, applied against the larger of
    /// <c>|a|</c> and <c>|b|</c>. Must be non-negative and not NaN.</param>
    /// <param name="absoluteTolerance">Absolute tolerance, the floor used near zero. Must
    /// be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if approximately equal under the combined rule.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either tolerance is NaN or negative.</exception>
    public static bool IsRelativelyAndAbsolutelyClose(
        double a,
        double b,
        double relativeTolerance,
        double absoluteTolerance)
    {
        ValidateTolerance(relativeTolerance, nameof(relativeTolerance));
        ValidateTolerance(absoluteTolerance, nameof(absoluteTolerance));

        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;
        if (!double.IsFinite(a) || !double.IsFinite(b))
            return BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);

        var diff = Math.Abs(a - b);
        var scale = Math.Max(Math.Abs(a), Math.Abs(b));
        return diff <= Math.Max(absoluteTolerance, relativeTolerance * scale);
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is finite (neither NaN nor
    /// infinite). Mirrors <see cref="double.IsFinite(double)"/> as a one-stop predicate
    /// alongside the other tolerance helpers, useful in fluent assertion chains where the
    /// caller wants a single namespace.
    /// </summary>
    /// <param name="v">Value to check.</param>
    /// <returns><see langword="true"/> if <paramref name="v"/> is finite.</returns>
    public static bool IsFinite(double v) => double.IsFinite(v);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is finite (neither NaN nor
    /// infinite). Mirrors <see cref="float.IsFinite(float)"/>.
    /// </summary>
    /// <param name="v">Value to check.</param>
    /// <returns><see langword="true"/> if <paramref name="v"/> is finite.</returns>
    public static bool IsFinite(float v) => float.IsFinite(v);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is finite and non-negative
    /// (zero is allowed). The natural domain check for magnitudes, distances, durations,
    /// counts cast to <see cref="double"/>, and similar non-negative quantities.
    /// </summary>
    /// <param name="v">Value to check.</param>
    /// <returns><see langword="true"/> if <paramref name="v"/> is finite and <c>&gt;= 0</c>.</returns>
    public static bool IsNonNegativeFinite(double v) => double.IsFinite(v) && v >= 0.0;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is finite and in the closed
    /// interval <c>[0, 1]</c>. The standard probability-domain check.
    /// </summary>
    /// <param name="v">Value to check.</param>
    /// <returns><see langword="true"/> if <paramref name="v"/> is finite and <c>0 &lt;= v &lt;= 1</c>.</returns>
    public static bool IsProbability(double v) => double.IsFinite(v) && v >= 0.0 && v <= 1.0;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="v"/> is finite and in the closed
    /// interval <c>[0, 100]</c>. The standard percentage-domain check.
    /// </summary>
    /// <param name="v">Value to check.</param>
    /// <returns><see langword="true"/> if <paramref name="v"/> is finite and <c>0 &lt;= v &lt;= 100</c>.</returns>
    public static bool IsPercentage(double v) => double.IsFinite(v) && v >= 0.0 && v <= 100.0;

    /// <summary>
    /// Returns <see langword="true"/> when <c>inverse(forward(x))</c> equals <paramref name="x"/>
    /// within <paramref name="tolerance"/>. Verifies the roundtrip identity for invertible
    /// transformations such as <c>Sin/Asin</c>, <c>Log/Exp</c>, encode/decode pairs, or
    /// degree/radian conversions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The double overload covers the dominant case. Consumers needing the same check on
    /// other types (vectors, quaternions, custom domain types) compose their own predicate
    /// out of the type-specific <c>IsApproximatelyEqual</c> overload, typically inside a
    /// <c>[GenerateAssertion]</c> extension on their own type. Keeping the core surface
    /// double-only avoids locking a generic equality shape into the public API at 0.1.0.
    /// </para>
    /// <para>
    /// NaN-tolerant equality is delegated to
    /// <see cref="IsApproximatelyEqual(double, double, double)"/>; both NaN counts as equal,
    /// which is appropriate for transformations whose domain includes NaN as a meaningful
    /// "no value" sentinel.
    /// </para>
    /// </remarks>
    /// <param name="x">Value to roundtrip.</param>
    /// <param name="forward">Forward transformation, applied to <paramref name="x"/>.</param>
    /// <param name="inverse">Inverse transformation, applied to the forward result.</param>
    /// <param name="tolerance">Maximum allowed absolute difference between <paramref name="x"/>
    /// and <c>inverse(forward(x))</c>. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the roundtrip recovers <paramref name="x"/> within tolerance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="forward"/> or <paramref name="inverse"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool HasRoundtripIdentity(
        double x,
        Func<double, double> forward,
        Func<double, double> inverse,
        double tolerance)
    {
        ArgumentNullException.ThrowIfNull(forward);
        ArgumentNullException.ThrowIfNull(inverse);
        ValidateTolerance(tolerance);

        var roundtripped = inverse(forward(x));
        return IsApproximatelyEqual(x, roundtripped, tolerance);
    }

    /// <summary>
    /// Returns <see langword="true"/> when two quaternions represent the same rotation
    /// within tolerance, treating <c>q</c> and <c>-q</c> as equivalent (the well-known
    /// double-cover of <c>SO(3)</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementation normalizes both inputs first so non-unit quaternions still produce
    /// the correct verdict; the dot-product test
    /// <c>|q1 . q2| &gt;= 1 - tolerance</c> identifies the same-rotation condition robustly.
    /// </para>
    /// <para>
    /// Reference: Hanson, <i>Visualizing Quaternions</i>, §4.6.
    /// </para>
    /// </remarks>
    /// <param name="a">First quaternion.</param>
    /// <param name="b">Second quaternion.</param>
    /// <param name="tolerance">Maximum allowed deviation of <c>|dot|</c> from <c>1</c>.
    /// Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the two quaternions encode the same rotation
    /// within the requested tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsRotationallyEquivalent(Quaternion a, Quaternion b, double tolerance)
    {
        ValidateTolerance(tolerance);

        var aNormalized = Quaternion.Normalize(a);
        var bNormalized = Quaternion.Normalize(b);
        var dot = Math.Abs((double)Quaternion.Dot(aNormalized, bNormalized));
        return dot >= 1.0 - tolerance;
    }

    /// <summary>
    /// Returns <see langword="true"/> when two planes describe the same set of points in
    /// 3-space within tolerance. A plane <c>(n, d)</c> and its sign-flipped counterpart
    /// <c>(-n, -d)</c> describe the same geometric plane; this method treats them as
    /// equivalent, while <see cref="IsApproximatelyEqual(Plane, Plane, double)"/> does not.
    /// </summary>
    /// <param name="a">First plane.</param>
    /// <param name="b">Second plane.</param>
    /// <param name="tolerance">Maximum allowed absolute difference per component for either
    /// the matching or the flipped representation. Must be non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the two planes describe the same point set
    /// within the requested tolerance under either sign convention.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    public static bool IsGeometricallyEquivalent(Plane a, Plane b, double tolerance)
    {
        ValidateTolerance(tolerance);

        if (IsApproximatelyEqual(a, b, tolerance))
            return true;

        var flipped = new Plane(-b.Normal, -b.D);
        return IsApproximatelyEqual(a, flipped, tolerance);
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

    private static void ValidateTolerance(float tolerance, string paramName = "tolerance")
    {
        if (float.IsNaN(tolerance))
        {
            throw new ArgumentOutOfRangeException(paramName, "Tolerance cannot be NaN.");
        }
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Tolerance cannot be negative.");
        }
    }
}
