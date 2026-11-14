using System;
using System.Numerics;
using System.Numerics.Tensors;

namespace MathAssertions;

/// <summary>
/// Tolerance-comparison helpers for floating-point primitives and selected
/// <see cref="System.Numerics"/> types. The scalar and component-wise comparisons are NaN-aware and
/// infinity-aware. Allocation-free.
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

        if (double.IsNaN(a) || double.IsNaN(b))
            return double.IsNaN(a) && double.IsNaN(b);
        if (double.IsInfinity(a) || double.IsInfinity(b))
            return double.IsInfinity(a) && double.IsInfinity(b) && double.IsNegative(a) == double.IsNegative(b);

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

        if (float.IsNaN(a) || float.IsNaN(b))
            return float.IsNaN(a) && float.IsNaN(b);
        if (float.IsInfinity(a) || float.IsInfinity(b))
            return float.IsInfinity(a) && float.IsInfinity(b) && float.IsNegative(a) == float.IsNegative(b);

        return Math.Abs(a - b) <= tolerance;
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Vector2"/> values.
    /// Components widen to <see cref="double"/> before comparing against the
    /// caller's <see langword="double"/> tolerance; see the <see cref="Vector3"/> overload
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

        return IsApproximatelyEqual(a.X, b.X, tolerance)
            && IsApproximatelyEqual(a.Y, b.Y, tolerance);
    }

    /// <summary>Component-wise tolerance comparison for two <see cref="Vector3"/> values.</summary>
    /// <remarks>
    /// Component values widen to <see cref="double"/> before the per-axis comparison so the
    /// caller's <see langword="double"/> tolerance is kept as is and each component difference is
    /// computed in <see langword="double"/>, so tight tolerances such as <c>1e-9</c> are not lost
    /// to <see langword="float"/> rounding. This is a load-bearing semantic for the rest of the
    /// assertion family.
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

        return IsApproximatelyEqual(a.X, b.X, tolerance)
            && IsApproximatelyEqual(a.Y, b.Y, tolerance)
            && IsApproximatelyEqual(a.Z, b.Z, tolerance);
    }

    /// <summary>
    /// Component-wise tolerance comparison for two <see cref="Vector4"/> values. Components
    /// widen to <see cref="double"/> before comparing against the caller's <see langword="double"/>
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

        return IsApproximatelyEqual(a.X, b.X, tolerance)
            && IsApproximatelyEqual(a.Y, b.Y, tolerance)
            && IsApproximatelyEqual(a.Z, b.Z, tolerance)
            && IsApproximatelyEqual(a.W, b.W, tolerance);
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

        return IsApproximatelyEqual(a.X, b.X, tolerance)
            && IsApproximatelyEqual(a.Y, b.Y, tolerance)
            && IsApproximatelyEqual(a.Z, b.Z, tolerance)
            && IsApproximatelyEqual(a.W, b.W, tolerance);
    }

    /// <summary>
    /// Element-wise tolerance comparison for two <see cref="Matrix4x4"/> values across all
    /// sixteen elements. Elements widen to <see cref="double"/> before comparing against
    /// the caller's <see langword="double"/> tolerance.
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
                if (!IsApproximatelyEqual(a[row, col], b[row, col], tolerance))
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
            && IsApproximatelyEqual(a.D, b.D, tolerance);
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
    /// Infinity handling matches the scalar overloads under every tolerance, including positive
    /// infinity: an infinity equals only the infinity of the same sign, decided before any
    /// subtraction. The difference of two finite elements is formed as larger minus smaller in
    /// checked arithmetic, so unsigned types do not wrap and signed integers do not overflow:
    /// a difference beyond the range of <typeparamref name="T"/> (for example
    /// <see cref="int.MaxValue"/> against <see cref="int.MinValue"/>) is larger than any
    /// tolerance and compares unequal.
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
        ValidateTensorTolerance(tolerance);

        if (!HaveSameShape(a, b))
            return false;

        try
        {
            var ae = a.GetEnumerator();
            var be = b.GetEnumerator();
            while (ae.MoveNext() && be.MoveNext())
            {
                if (!AreElementsApproximatelyEqual(ae.Current, be.Current, tolerance))
                    return false;
            }
        }
        catch (OverflowException)
        {
            // Only an integer element type throws: the pair differs by more than T can represent, which is
            // more than any tolerance of T. Floating-point types overflow to infinity instead and never throw.
            return false;
        }
        return true;
    }

    /// <summary>Validates the tolerance of the tensor overload: not NaN and not negative.</summary>
    /// <typeparam name="T">Element type, any <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="tolerance">The tolerance to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    private static void ValidateTensorTolerance<T>(T tolerance)
        where T : INumber<T>
    {
        if (T.IsNaN(tolerance))
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be NaN.");
        if (tolerance < T.Zero)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative.");
    }

    /// <summary>Returns <see langword="true"/> when two tensor spans have the same rank and the same length in every dimension.</summary>
    /// <typeparam name="T">Element type, any <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="a">First tensor span.</param>
    /// <param name="b">Second tensor span.</param>
    /// <returns><see langword="true"/> if the shapes are equal.</returns>
    private static bool HaveSameShape<T>(ReadOnlyTensorSpan<T> a, ReadOnlyTensorSpan<T> b)
        where T : INumber<T>
    {
        var aLengths = a.Lengths;
        var bLengths = b.Lengths;
        if (aLengths.Length != bLengths.Length)
            return false;
        for (var i = 0; i < aLengths.Length; i++)
        {
            if (aLengths[i] != bLengths[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Compares one element pair of the tensor overload by the scalar overloads' rules: NaN matches only NaN,
    /// an infinity matches only the same infinity (before any subtraction, under every tolerance), and finite
    /// values match when the larger minus the smaller is at most the tolerance. The subtraction is checked so
    /// an integer difference beyond the range of <typeparamref name="T"/> throws instead of wrapping.
    /// </summary>
    /// <typeparam name="T">Element type, any <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="av">Element of the first tensor.</param>
    /// <param name="bv">Element of the second tensor.</param>
    /// <param name="tolerance">Maximum allowed difference, already validated.</param>
    /// <returns><see langword="true"/> if the two elements are approximately equal.</returns>
    /// <exception cref="OverflowException">The difference of two integers exceeds the range of <typeparamref name="T"/>.</exception>
    private static bool AreElementsApproximatelyEqual<T>(T av, T bv, T tolerance)
        where T : INumber<T>
    {
        if (T.IsNaN(av) || T.IsNaN(bv))
            return T.IsNaN(av) && T.IsNaN(bv);
        if (T.IsInfinity(av) || T.IsInfinity(bv))
            return av == bv;

        var larger = av >= bv ? av : bv;
        var smaller = av >= bv ? bv : av;
        return checked(larger - smaller) <= tolerance;
    }

    /// <summary>
    /// ULP-distance equality for two doubles. Returns <see langword="true"/> when the two
    /// values are within <paramref name="ulpDistance"/> representable doubles of each other
    /// under IEEE 754.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ULP ("unit in the last place") distance is the number of steps between two values on the
    /// ordered list of representable floating-point numbers; adjacent representable values are
    /// at distance 1. It is the natural metric for "as close as floating-point can express,"
    /// scaling with the magnitude of the values without the caller picking an absolute
    /// tolerance. Values with opposite signs always compare as <see langword="false"/>, even
    /// tiny ones such as <see cref="double.Epsilon"/> and its negation, because the comparison
    /// is only made between same-signed values. Positive and negative zero are treated as
    /// equal.
    /// </para>
    /// <para>
    /// Reference: Goldberg, <i>What Every Computer Scientist Should Know About
    /// Floating-Point Arithmetic</i>, ACM Computing Surveys, 1991.
    /// </para>
    /// </remarks>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="ulpDistance">Maximum allowed distance in representable doubles, counted as
    /// steps between adjacent representable values (adjacent values are at distance 1).
    /// Must be non-negative.</param>
    /// <returns><see langword="true"/> if both NaN, both zero (regardless of sign), or
    /// same-signed and within <paramref name="ulpDistance"/> ULPs; <see langword="false"/> for
    /// values of opposite sign (other than the two zeros).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ulpDistance"/> is negative.</exception>
    public static bool IsCloseInUlps(double a, double b, long ulpDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ulpDistance);

        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;

        return AreBitPatternsWithinUlps(
            BitConverter.DoubleToInt64Bits(a),
            BitConverter.DoubleToInt64Bits(b),
            ulpDistance,
            0x7FFF_FFFF_FFFF_FFFFL);
    }

    /// <summary>
    /// ULP-distance equality for two floats. Returns <see langword="true"/> when the two
    /// values are within <paramref name="ulpDistance"/> representable floats of each other
    /// under IEEE 754. See the double overload for full semantics.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="ulpDistance">Maximum allowed distance in representable floats, counted as
    /// steps between adjacent representable values (adjacent values are at distance 1).
    /// Must be non-negative.</param>
    /// <returns><see langword="true"/> if both NaN, both zero (regardless of sign), or
    /// same-signed and within <paramref name="ulpDistance"/> ULPs; <see langword="false"/> for
    /// values of opposite sign (other than the two zeros).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ulpDistance"/> is negative.</exception>
    public static bool IsCloseInUlps(float a, float b, int ulpDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ulpDistance);

        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        if (float.IsNaN(a) || float.IsNaN(b))
            return false;

        return AreBitPatternsWithinUlps(
            BitConverter.SingleToInt32Bits(a),
            BitConverter.SingleToInt32Bits(b),
            ulpDistance,
            0x7FFF_FFFFL);
    }

    /// <summary>
    /// Decides ULP-distance equality for two non-NaN values from their IEEE 754 bit patterns, widened to
    /// <see cref="long"/> with the sign preserved: two zeros of either sign are equal (their patterns differ
    /// only in the sign bit), identical patterns are equal, values of opposite sign are not, and same-signed
    /// values are equal when their patterns, which are ordered like the values, are at most
    /// <paramref name="ulpDistance"/> apart.
    /// </summary>
    /// <param name="aBits">Bit pattern of the first value.</param>
    /// <param name="bBits">Bit pattern of the second value.</param>
    /// <param name="ulpDistance">Maximum allowed distance in representable values. Non-negative.</param>
    /// <param name="magnitudeMask">Mask of every bit except the sign bit of the value's width.</param>
    /// <returns><see langword="true"/> if the two values are within <paramref name="ulpDistance"/> ULPs.</returns>
    private static bool AreBitPatternsWithinUlps(long aBits, long bBits, long ulpDistance, long magnitudeMask)
    {
        // +0 and -0 are equal under IEEE 754 even though their bit patterns differ by the sign bit. The
        // cheap test "value is zero" is "the magnitude bits are zero", which masking off the sign bit
        // captures for both signs at once.
        if ((aBits & magnitudeMask) == 0 && (bBits & magnitudeMask) == 0)
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
    /// Implementation normalizes both inputs first (in <see cref="double"/> precision, so a
    /// non-unit quaternion compared with itself or a scaled copy passes a tolerance such as <c>1e-12</c>);
    /// the dot-product test <c>|q1 . q2| &gt;= 1 - tolerance</c> identifies the same-rotation
    /// condition robustly.
    /// </para>
    /// <para>
    /// A quaternion that cannot be normalized (squared magnitude zero or not finite, for example
    /// <c>default(Quaternion)</c>) names no rotation. Like
    /// <see cref="IsPoseApproximatelyEqual(Vector3, Quaternion, Vector3, Quaternion, double, double)"/>,
    /// the comparison then falls back to a componentwise check against a tight fixed tolerance
    /// of <c>1e-6</c> that ignores <paramref name="tolerance"/>: the zero quaternion matches
    /// itself and any quaternion within <c>1e-6</c> of zero in every component (noise, not a
    /// rotation), and no unit-length rotation. A quaternion with a NaN or infinite component never
    /// matches, not even itself.
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

        if (!TryNormalizeToDouble(a, out double ax, out double ay, out double az, out double aw)
            || !TryNormalizeToDouble(b, out double bx, out double by, out double bz, out double bw))
        {
            return DegenerateOrientationsMatch(a, b);
        }

        double dot = Math.Abs((ax * bx) + (ay * by) + (az * bz) + (aw * bw));
        return dot >= 1.0 - tolerance;
    }

    /// <summary>
    /// Returns the geodesic angle between two rotations, in degrees: the magnitude of the single
    /// rotation that carries one orientation onto the other. The SO(3) double cover is handled by
    /// taking the absolute value of the relative quaternion's real part, so a quaternion and its
    /// negation yield a zero angle. Both inputs are normalized internally; the result lies in
    /// <c>[0, 180]</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The angle is computed from the relative rotation <c>r = a * conj(b)</c> as
    /// <c>theta = 2 * atan2(‖imag(r)‖, |real(r)|)</c>. This <c>atan2</c> form is numerically
    /// stable across the whole <c>0°-180°</c> range: unlike <c>2 * acos(|dot|)</c>, it does not
    /// sit on the infinite-slope shoulder of <c>acos</c> near <c>dot = 1</c>, where the residual
    /// float error of normalizing a non-unit quaternion would otherwise be amplified into a
    /// spurious non-zero angle (so <c>d(q, q)</c> measured as non-zero for nearly every real
    /// quaternion). The relative quaternion is formed in <see cref="double"/> precision after a
    /// double-precision normalization, so the metric is symmetric and a quaternion compared with
    /// itself yields zero or a value within about <c>1e-14</c> degrees (float/double rounding).
    /// </para>
    /// <para>
    /// A quaternion that is not normalizable (squared magnitude is zero or not finite, e.g.
    /// <c>default(Quaternion)</c> = <c>(0, 0, 0, 0)</c>) has no defined rotation axis, so no
    /// geodesic angle exists; this method returns <see cref="double.NaN"/> in that case. Callers
    /// that need a defined verdict for the zero quaternion (such as
    /// <see cref="IsPoseApproximatelyEqual(Vector3, Quaternion, Vector3, Quaternion, double, double)"/>)
    /// fall back to a componentwise comparison.
    /// </para>
    /// </remarks>
    /// <param name="a">First rotation.</param>
    /// <param name="b">Second rotation.</param>
    /// <returns>The geodesic angle between the two rotations in degrees, in <c>[0, 180]</c>;
    /// or <see cref="double.NaN"/> when either input is not normalizable.</returns>
    public static double RotationAngleDegrees(Quaternion a, Quaternion b)
    {
        return TryRotationAngleDegrees(a, b, out double angle) ? angle : double.NaN;
    }

    /// <summary>
    /// Returns the shorter arc between two angles given in degrees, on the circle that wraps every
    /// <c>360</c> degrees: the result lies in <c>[0, 180]</c>, so <c>359</c> and <c>1</c> are
    /// <c>2</c> degrees apart, and <c>180</c> and <c>-180</c> are <c>0</c> degrees apart.
    /// </summary>
    /// <remarks>
    /// Each angle is first reduced to <c>[-180, 180]</c> (the IEEE remainder of a division by
    /// <c>360</c>, an exact operation), so huge angles such as <c>1e7</c> degrees keep their precision
    /// and a difference of two huge angles of opposite sign cannot overflow. Two angles on both sides
    /// of <c>180</c> are compared after the negative one moves up a full turn, so their difference is
    /// formed near zero, not near <c>360</c> where a tiny difference would round to a full turn.
    /// A NaN or infinite angle has no position on the circle: the result is <see cref="double.NaN"/>.
    /// </remarks>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <returns>The wrapped difference in degrees, in <c>[0, 180]</c>; <see cref="double.NaN"/>
    /// when either angle is NaN or infinite.</returns>
    public static double AngleDifferenceDegrees(double a, double b)
    {
        if (!double.IsFinite(a) || !double.IsFinite(b))
            return double.NaN;

        var reducedA = Math.IEEERemainder(a, 360.0);
        var reducedB = Math.IEEERemainder(b, 360.0);
        if (reducedA - reducedB > 180.0)
            reducedB += 360.0;
        else if (reducedB - reducedA > 180.0)
            reducedA += 360.0;
        return Math.Abs(reducedA - reducedB);
    }

    /// <summary>
    /// Returns the shorter arc between two angles given in degrees as a <see cref="float"/>: the
    /// double-precision result of <see cref="AngleDifferenceDegrees(double, double)"/>, rounded once to
    /// single precision, so a caller that works in single precision stays there.
    /// </summary>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <returns>The wrapped difference in degrees, in <c>[0, 180]</c>; <see cref="float.NaN"/>
    /// when either angle is NaN or infinite.</returns>
    public static float AngleDifferenceDegrees(float a, float b)
    {
        double widenedA = a;
        double widenedB = b;
        return float.CreateTruncating(AngleDifferenceDegrees(widenedA, widenedB));
    }

    /// <summary>
    /// Returns <see langword="true"/> when two angles in degrees are within
    /// <paramref name="toleranceDegrees"/> of each other on the circle that wraps every <c>360</c>
    /// degrees: <see cref="AngleDifferenceDegrees(double, double)"/> is at most the tolerance.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="IsApproximatelyEqual(double, double, double)"/>, which compares the plain
    /// difference, <c>179</c> and <c>-179</c> are <c>2</c> degrees apart here and <c>180</c> equals
    /// <c>-180</c>. A NaN or infinite angle never matches, not even NaN against NaN: an angle that is
    /// not a number has no position on the circle.
    /// </remarks>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <param name="toleranceDegrees">Maximum allowed wrapped difference in degrees. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the wrapped difference is at most
    /// <paramref name="toleranceDegrees"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="toleranceDegrees"/> is NaN or negative.</exception>
    public static bool IsAngleApproximatelyEqual(double a, double b, double toleranceDegrees)
    {
        ValidateTolerance(toleranceDegrees, nameof(toleranceDegrees));

        return AngleDifferenceDegrees(a, b) <= toleranceDegrees;
    }

    /// <summary>
    /// Returns <see langword="true"/> when two angles in degrees are within
    /// <paramref name="toleranceDegrees"/> of each other on the circle that wraps every <c>360</c>
    /// degrees. Same rules as the double overload; the difference is rounded to single precision
    /// before it is compared with the tolerance.
    /// </summary>
    /// <param name="a">First angle in degrees.</param>
    /// <param name="b">Second angle in degrees.</param>
    /// <param name="toleranceDegrees">Maximum allowed wrapped difference in degrees. Must be
    /// non-negative and not NaN.</param>
    /// <returns><see langword="true"/> if the wrapped difference is at most
    /// <paramref name="toleranceDegrees"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="toleranceDegrees"/> is NaN or negative.</exception>
    public static bool IsAngleApproximatelyEqual(float a, float b, float toleranceDegrees)
    {
        ValidateTolerance(toleranceDegrees, nameof(toleranceDegrees));

        return AngleDifferenceDegrees(a, b) <= toleranceDegrees;
    }

    /// <summary>
    /// Computes the geodesic angle between two rotations in degrees using the numerically-stable
    /// <c>2 * atan2(‖imag(r)‖, |real(r)|)</c> form on the relative quaternion <c>r = a * conj(b)</c>,
    /// reporting via <paramref name="angleDegrees"/>. Returns <see langword="false"/> when either
    /// input is not normalizable (squared magnitude is zero or not finite), in which case no geodesic
    /// angle is defined.
    /// </summary>
    /// <param name="a">First rotation.</param>
    /// <param name="b">Second rotation.</param>
    /// <param name="angleDegrees">Receives the geodesic angle in degrees, in <c>[0, 180]</c>, when
    /// the method returns <see langword="true"/>; otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> when both inputs are normalizable and an angle was computed;
    /// <see langword="false"/> when either input is not normalizable.</returns>
    internal static bool TryRotationAngleDegrees(Quaternion a, Quaternion b, out double angleDegrees)
    {
        if (!TryNormalizeToDouble(a, out double ax, out double ay, out double az, out double aw)
            || !TryNormalizeToDouble(b, out double bx, out double by, out double bz, out double bw))
        {
            angleDegrees = 0.0;
            return false;
        }

        // Relative rotation r = a * conj(b), with conj(b) = (-bx, -by, -bz, bw), formed in double
        // precision. The real part is the cosine of the half-angle; the imaginary magnitude is its
        // sine. atan2 of the two is stable over the full 0-180 degree range.
        double rw = (aw * bw) + (ax * bx) + (ay * by) + (az * bz);
        double rx = (-aw * bx) + (ax * bw) - (ay * bz) + (az * by);
        double ry = (-aw * by) + (ax * bz) + (ay * bw) - (az * bx);
        double rz = (-aw * bz) - (ax * by) + (ay * bx) + (az * bw);

        double imaginaryNorm = Math.Sqrt((rx * rx) + (ry * ry) + (rz * rz));
        // |real(r)| collapses the SO(3) double cover: q and -q yield the same angle.
        double halfAngle = Math.Atan2(imaginaryNorm, Math.Abs(rw));
        angleDegrees = halfAngle * 2.0 * (180.0 / Math.PI);
        return true;
    }

    /// <summary>
    /// Normalizes <paramref name="q"/> to unit magnitude in <see cref="double"/> precision,
    /// reporting the components through the <see langword="out"/> parameters. Returns <see langword="false"/>
    /// when the squared magnitude is zero or not finite (the quaternion has no
    /// defined direction and cannot be normalized).
    /// </summary>
    /// <param name="q">The quaternion to normalize.</param>
    /// <param name="x">Receives the normalized X component.</param>
    /// <param name="y">Receives the normalized Y component.</param>
    /// <param name="z">Receives the normalized Z component.</param>
    /// <param name="w">Receives the normalized W component.</param>
    /// <returns><see langword="true"/> when normalization succeeded; otherwise <see langword="false"/>.</returns>
    private static bool TryNormalizeToDouble(Quaternion q, out double x, out double y, out double z, out double w)
    {
        double qx = q.X, qy = q.Y, qz = q.Z, qw = q.W;
        double lengthSquared = (qx * qx) + (qy * qy) + (qz * qz) + (qw * qw);
        if (!double.IsFinite(lengthSquared) || lengthSquared <= 0.0)
        {
            x = y = z = w = 0.0;
            return false;
        }

        double inverseLength = 1.0 / Math.Sqrt(lengthSquared);
        x = qx * inverseLength;
        y = qy * inverseLength;
        z = qz * inverseLength;
        w = qw * inverseLength;
        return true;
    }

    /// <summary>
    /// Returns the Euclidean distance between two positions, computed in <see cref="double"/>
    /// precision (components widen from <see cref="float"/> before the subtraction to avoid
    /// catastrophic cancellation near large coordinates).
    /// </summary>
    /// <param name="a">First position.</param>
    /// <param name="b">Second position.</param>
    /// <returns>The Euclidean distance between the two positions.</returns>
    public static double PositionDistance(Vector3 a, Vector3 b)
    {
        double ax = a.X, ay = a.Y, az = a.Z;
        double bx = b.X, by = b.Y, bz = b.Z;
        double dx = ax - bx;
        double dy = ay - by;
        double dz = az - bz;
        return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }

    /// <summary>Component-space tolerance for the degenerate-orientation fallback in
    /// <see cref="IsPoseApproximatelyEqual"/>. Tight and fixed (not the caller's degrees value, which
    /// would be far too permissive) so a non-normalizable quaternion only matches a near-identical one,
    /// never a real rotation.</summary>
    private const double DegenerateOrientationComponentTolerance = 1e-6;

    /// <summary>
    /// Fallback comparison for orientations of which at least one cannot be normalized: both must be finite
    /// (a NaN or infinite component names no rotation and never matches, not even itself), and then they match
    /// componentwise within <see cref="DegenerateOrientationComponentTolerance"/>, so the zero quaternion matches
    /// itself and any quaternion within that tolerance of zero, and no unit-length rotation.
    /// </summary>
    /// <param name="a">First quaternion.</param>
    /// <param name="b">Second quaternion.</param>
    /// <returns><see langword="true"/> if both are finite and componentwise equal within the fixed tolerance.</returns>
    private static bool DegenerateOrientationsMatch(Quaternion a, Quaternion b)
        => IsFiniteQuaternion(a)
        && IsFiniteQuaternion(b)
        && IsApproximatelyEqual(a, b, DegenerateOrientationComponentTolerance);

    /// <summary>
    /// Returns <see langword="true"/> when every component is finite. The components widen to
    /// <see cref="double"/> before they are summed, so the sum of finite floats cannot overflow and is
    /// non-finite only when a component is NaN or infinite.
    /// </summary>
    /// <param name="q">The quaternion to inspect.</param>
    /// <returns><see langword="true"/> if no component is NaN or infinite.</returns>
    private static bool IsFiniteQuaternion(Quaternion q)
    {
        double sum = q.X;
        sum += q.Y;
        sum += q.Z;
        sum += q.W;
        return double.IsFinite(sum);
    }

    /// <summary>
    /// Returns <see langword="true"/> when two rigid poses (a position and an orientation) match
    /// within separate tolerances: the Euclidean <see cref="PositionDistance(Vector3, Vector3)"/>
    /// is at most <paramref name="positionTolerance"/> AND the geodesic
    /// <see cref="RotationAngleDegrees(Quaternion, Quaternion)"/> is at most
    /// <paramref name="rotationToleranceDegrees"/>. Position and orientation carry different units
    /// (typically metres vs degrees), so a single shared tolerance would be wrong; the two are
    /// supplied and evaluated independently.
    /// </summary>
    /// <param name="actualPosition">The actual pose translation.</param>
    /// <param name="actualOrientation">The actual pose rotation.</param>
    /// <param name="expectedPosition">The expected pose translation.</param>
    /// <param name="expectedOrientation">The expected pose rotation.</param>
    /// <param name="positionTolerance">Maximum allowed Euclidean position distance. Non-negative, not NaN.</param>
    /// <param name="rotationToleranceDegrees">Maximum allowed geodesic rotation angle in degrees. Non-negative, not NaN.</param>
    /// <returns><see langword="true"/> when both the position and the orientation are within their tolerances.</returns>
    /// <remarks>
    /// <para>
    /// Degenerate orientation: a quaternion that is not normalizable (squared magnitude is zero or not
    /// finite, e.g. <c>default(Quaternion)</c> = <c>(0, 0, 0, 0)</c>) has no defined rotation axis,
    /// so the geodesic angle from
    /// <see cref="RotationAngleDegrees(Quaternion, Quaternion)"/> is undefined. Rather than fail a
    /// legitimate default-pose round-trip, the orientation half falls back to a componentwise
    /// comparison against a tight fixed tolerance
    /// (<see cref="DegenerateOrientationComponentTolerance"/>), independent of
    /// <paramref name="rotationToleranceDegrees"/>. Reusing the degrees value would be far too
    /// permissive (a tolerance of 1 or more would accept a zero quaternion as equal to a real
    /// rotation). So <c>(0, 0, 0, 0)</c> compared with itself passes, while <c>(0, 0, 0, 0)</c>
    /// compared with any real rotation fails on the component deltas regardless of the angular tolerance.
    /// </para>
    /// <para>
    /// A position with a NaN coordinate, or two positions whose distance is not a number (for example
    /// two infinite coordinates), never matches, not even NaN against NaN.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Either tolerance is NaN or negative.</exception>
    public static bool IsPoseApproximatelyEqual(
        Vector3 actualPosition,
        Quaternion actualOrientation,
        Vector3 expectedPosition,
        Quaternion expectedOrientation,
        double positionTolerance,
        double rotationToleranceDegrees)
    {
        ValidateTolerance(positionTolerance, nameof(positionTolerance));
        ValidateTolerance(rotationToleranceDegrees, nameof(rotationToleranceDegrees));
        // Written as a negated <=, so a NaN distance (a NaN or infinite coordinate) fails instead of passing.
        if (!(PositionDistance(actualPosition, expectedPosition) <= positionTolerance))
        {
            return false;
        }

        // When either orientation is not normalizable, no geodesic angle is defined; fall back to a
        // componentwise comparison against a tight fixed tolerance (not the degrees value, which would
        // accept a zero quaternion as equal to a real rotation once the angular tolerance reaches 1).
        if (TryRotationAngleDegrees(actualOrientation, expectedOrientation, out double angleDegrees))
        {
            return angleDegrees <= rotationToleranceDegrees;
        }

        return DegenerateOrientationsMatch(actualOrientation, expectedOrientation);
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

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/>, viewed as a rotation in
    /// axis-angle form, is rotationally equivalent (within <paramref name="tolerance"/>) to the
    /// rotation <paramref name="expectedAngleDegrees"/> degrees around <paramref name="expectedAxis"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The comparison is performed on the rotational-equivalence metric, not on axis-component
    /// or angle-magnitude differences. The expected axis-angle pair is materialized as a unit
    /// quaternion in <see cref="double"/> precision (the axis is normalized in double, so even an
    /// axis such as <c>(1e30, 0, 0)</c> or <c>(1e-30, 0, 0)</c> keeps its direction); that
    /// quaternion is then compared to <paramref name="value"/> with the same test as
    /// <see cref="IsRotationallyEquivalent(Quaternion, Quaternion, double)"/>, which evaluates
    /// <c>|dot(unit_a, unit_b)| &gt;= 1 - tolerance</c>. Going through the dot-product metric
    /// handles every edge case the per-component extraction would otherwise stumble on
    /// uniformly: the SO(3) <c>q</c> vs <c>-q</c> double cover; the 180-degree boundary where
    /// <c>(axis, +180)</c>, <c>(axis, -180)</c>, and <c>(-axis, +-180)</c> all encode the same
    /// rotation; non-unit inputs via the internal normalization both inputs receive.
    /// </para>
    /// <para>
    /// Identity-rotation edge case: when <paramref name="expectedAngleDegrees"/> is approximately
    /// zero the expected quaternion is approximately the identity, and the comparison reduces
    /// to <paramref name="value"/> being approximately the identity rotation. The expected
    /// axis must still have a direction, so a deliberate zero-axis input is rejected.
    /// </para>
    /// <para>
    /// A <paramref name="value"/> that cannot be normalized (for example <c>default(Quaternion)</c>)
    /// names no rotation: it is compared componentwise, at the fixed tolerance of
    /// <c>1e-6</c> (a NaN or infinite component never matches) described for
    /// <see cref="IsRotationallyEquivalent(Quaternion, Quaternion, double)"/>, with the expected
    /// rotation, so it never matches a real rotation.
    /// </para>
    /// <para>
    /// Tolerance calibration: <paramref name="tolerance"/> is a dot-product tolerance. For small
    /// dot-product tolerances <c>t</c>, the corresponding angular tolerance between the two
    /// rotations is approximately <c>2 * arccos(1 - t)</c> radians, which for small <c>t</c>
    /// linearises to roughly <c>2 * sqrt(2 * t)</c> radians; for example <c>t = 1e-4</c> admits
    /// rotations that differ by up to ~1.62 degrees. Callers used to angle-difference tolerance
    /// should pick <paramref name="tolerance"/> accordingly.
    /// </para>
    /// <para>
    /// Reference: Hanson, <i>Visualizing Quaternions</i>, §4.6.
    /// </para>
    /// </remarks>
    /// <param name="value">The rotation under test.</param>
    /// <param name="expectedAxis">The expected rotation axis. Normalized internally in double
    /// precision; must have finite components and a length greater than zero.</param>
    /// <param name="expectedAngleDegrees">The expected rotation magnitude in degrees.</param>
    /// <param name="tolerance">Dot-product tolerance on the unit-quaternion forms of
    /// <paramref name="value"/> and the materialised expected rotation: the comparison passes
    /// when <c>|dot(unit_value, unit_expected)| &gt;= 1 - tolerance</c>. Must be non-negative
    /// and not NaN. See the calibration note in the remarks for the relationship to an
    /// equivalent angular tolerance.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is rotationally equivalent
    /// to the requested axis-angle rotation within tolerance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is NaN or negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="expectedAxis"/> has a NaN or infinite
    /// component, or has zero length (no axis is defined).</exception>
    public static bool HasAxisAngleApproximately(
        Quaternion value,
        Vector3 expectedAxis,
        double expectedAngleDegrees,
        double tolerance)
    {
        ValidateTolerance(tolerance);
        NormalizeAxisToDoubleOrThrow(expectedAxis, out double axisX, out double axisY, out double axisZ);

        // The expected rotation as a unit quaternion, built in double precision from the unit axis.
        double halfAngleRadians = expectedAngleDegrees * (Math.PI / 360.0);
        double sine = Math.Sin(halfAngleRadians);
        double ex = axisX * sine;
        double ey = axisY * sine;
        double ez = axisZ * sine;
        double ew = Math.Cos(halfAngleRadians);

        // Use the rotational-equivalence formulation: two rotations are equal iff the absolute
        // dot product of their unit-quaternion forms is >= 1 - tolerance. This handles every
        // edge case the per-component axis-angle comparison would otherwise stumble on
        // (180-degree boundary where (axis, +180) ≡ (axis, -180) ≡ (-axis, ±180); the q vs -q
        // double cover; non-unit inputs via internal normalization).
        if (!TryNormalizeToDouble(value, out double vx, out double vy, out double vz, out double vw))
        {
            var expectedQuaternion = new Quaternion(
                float.CreateTruncating(ex), float.CreateTruncating(ey), float.CreateTruncating(ez), float.CreateTruncating(ew));
            return DegenerateOrientationsMatch(value, expectedQuaternion);
        }

        double dot = Math.Abs((vx * ex) + (vy * ey) + (vz * ez) + (vw * ew));
        return dot >= 1.0 - tolerance;
    }

    /// <summary>
    /// Extracts the axis-angle representation of <paramref name="value"/>, applying the SO(3)
    /// double-cover sign flip when needed so the extracted axis aligns with
    /// <paramref name="expectedAxis"/>. Used only by
    /// <see cref="MathFailureMessage.AxisAngleApproximately"/> for diagnostic display in the
    /// failure message; the <see cref="HasAxisAngleApproximately"/> predicate itself routes
    /// through <see cref="IsRotationallyEquivalent(Quaternion, Quaternion, double)"/>, which
    /// is a dot-product test on unit-quaternion forms and does not depend on this extraction.
    /// The split is deliberate: dot-product is robust at every angle and across the SO(3)
    /// double cover, while extracted axis / angle pairs are easier for humans to read in a
    /// failure message. Near numerical boundaries (180 degrees, near-identity rotations) the
    /// two paths can disagree by a residual within tolerance; the predicate's verdict is
    /// authoritative and the extracted pair is informational.
    /// </summary>
    /// <param name="value">The rotation under test.</param>
    /// <param name="expectedAxis">The expected rotation axis (un-normalized).</param>
    /// <param name="extractedAxis">Receives the (sign-corrected) extracted axis. Set to
    /// <see cref="Vector3.Zero"/> when the rotation is the identity.</param>
    /// <param name="extractedAngleDegrees">Receives the (sign-corrected) extracted angle in
    /// degrees. Set to <c>0</c> when the rotation is the identity.</param>
    /// <param name="normalizedExpectedAxis">Receives the normalized <paramref name="expectedAxis"/>.</param>
    /// <param name="isIdentityRotation"><see langword="true"/> when <paramref name="value"/>
    /// normalizes to (or near) the identity quaternion. When <see langword="true"/>,
    /// <paramref name="extractedAxis"/> is <see cref="Vector3.Zero"/> and
    /// <paramref name="extractedAngleDegrees"/> is <c>0</c>.</param>
    /// <remarks>
    /// When <paramref name="isIdentityRotation"/> is <see langword="false"/>, the extracted
    /// angle lies in <c>[-180, 180]</c> degrees. The sign-alignment step (axis flip when the
    /// dot product with <paramref name="normalizedExpectedAxis"/> is negative) negates the
    /// angle alongside the axis so the extracted pair compares directly against the caller's
    /// expected pair.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="expectedAxis"/> has a NaN or infinite
    /// component, or has zero length (no axis is defined).</exception>
    internal static void ExtractAxisAngle(
        Quaternion value,
        Vector3 expectedAxis,
        out Vector3 extractedAxis,
        out double extractedAngleDegrees,
        out Vector3 normalizedExpectedAxis,
        out bool isIdentityRotation)
    {
        NormalizeAxisToDoubleOrThrow(expectedAxis, out double axisX, out double axisY, out double axisZ);
        normalizedExpectedAxis = new Vector3(
            float.CreateTruncating(axisX), float.CreateTruncating(axisY), float.CreateTruncating(axisZ));

        if (!TryNormalizeToDouble(value, out double nx, out double ny, out double nz, out double nw))
        {
            // A quaternion with no direction has no axis and no angle to report.
            extractedAxis = Vector3.Zero;
            extractedAngleDegrees = double.NaN;
            isIdentityRotation = false;
            return;
        }

        double xyzLen = Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
        if (xyzLen < 1e-12)
        {
            extractedAxis = Vector3.Zero;
            extractedAngleDegrees = 0.0;
            isIdentityRotation = true;
            return;
        }

        double ux = nx / xyzLen;
        double uy = ny / xyzLen;
        double uz = nz / xyzLen;
        double angleRadians = 2.0 * Math.Atan2(xyzLen, Math.Abs(nw));
        if (nw < 0)
        {
            angleRadians = -angleRadians;
        }
        extractedAxis = new Vector3(float.CreateTruncating(ux), float.CreateTruncating(uy), float.CreateTruncating(uz));
        extractedAngleDegrees = angleRadians * (180.0 / Math.PI);

        if (((ux * axisX) + (uy * axisY) + (uz * axisZ)) < 0)
        {
            extractedAxis = -extractedAxis;
            extractedAngleDegrees = -extractedAngleDegrees;
        }
        isIdentityRotation = false;
    }

    /// <summary>
    /// Normalizes <paramref name="expectedAxis"/> to unit length in <see cref="double"/> precision. The
    /// squared length of any finite <see cref="float"/> axis, from the smallest denormal to
    /// <see cref="float.MaxValue"/>, is representable in double, so only a non-finite component or an
    /// exactly zero axis has no direction.
    /// </summary>
    /// <param name="expectedAxis">The axis to normalize. The parameter is named like the public callers'
    /// own parameter so the thrown exception's parameter name matches the public API.</param>
    /// <param name="x">Receives the normalized X component.</param>
    /// <param name="y">Receives the normalized Y component.</param>
    /// <param name="z">Receives the normalized Z component.</param>
    /// <exception cref="ArgumentException">A component is NaN or infinite, or the axis has zero length.</exception>
    private static void NormalizeAxisToDoubleOrThrow(Vector3 expectedAxis, out double x, out double y, out double z)
    {
        double ax = expectedAxis.X;
        double ay = expectedAxis.Y;
        double az = expectedAxis.Z;
        double lengthSquared = (ax * ax) + (ay * ay) + (az * az);
        if (!double.IsFinite(lengthSquared))
        {
            throw new ArgumentException(
                "expectedAxis must have finite components (a NaN or infinite component leaves the rotation axis undefined).",
                nameof(expectedAxis));
        }

        if (lengthSquared <= 0.0)
        {
            throw new ArgumentException(
                "expectedAxis has zero length (a zero-length axis has no defined direction).",
                nameof(expectedAxis));
        }

        double inverseLength = 1.0 / Math.Sqrt(lengthSquared);
        x = ax * inverseLength;
        y = ay * inverseLength;
        z = az * inverseLength;
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
