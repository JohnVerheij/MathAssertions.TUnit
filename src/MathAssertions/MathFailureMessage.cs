using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace MathAssertions;

/// <summary>
/// Renders rich per-component / per-element failure-message text for the compound
/// tolerance comparisons in <see cref="MathTolerance"/>. The boolean predicates in
/// <see cref="MathTolerance"/> remain the authoritative source of truth for pass / fail;
/// these helpers produce the human-readable explanation surfaced in the assertion
/// exception when the predicate returns <see langword="false"/>.
/// </summary>
/// <remarks>
/// <para>The rendered shape pins three sections under a leading expectation line:
/// <c>actual:</c>, <c>delta:</c>, and <c>exceeded:</c>. The <c>actual</c> section
/// echoes the value (so the failure stays readable in isolation from the auto-prefix),
/// <c>delta</c> shows the absolute per-axis difference, and <c>exceeded</c> names the
/// components whose delta crossed tolerance.</para>
/// <para>Numeric formatting uses <see cref="CultureInfo.InvariantCulture"/> with the
/// <c>G</c> general format so the rendered text is stable across locales and round-trips
/// cleanly through CI log scrapers.</para>
/// <para>Failure-message text is explicitly documented as not stable in the package
/// README; these helpers may evolve. The boolean comparison contract on
/// <see cref="MathTolerance"/> is the stable surface.</para>
/// <para>The type is <see langword="internal"/>; the
/// <c>[GenerateAssertion(InlineMethodBody=true)]</c> shape places the inlined call into
/// the sibling <c>MathAssertions.TUnit</c> assembly, which has access via the
/// <c>[InternalsVisibleTo]</c> attribute on this assembly. Not part of the public API
/// surface.</para>
/// </remarks>
[SuppressMessage(
    "MeziantouAnalyzer",
    "MA0182:Unused internal type",
    Justification = "Consumed cross-assembly by MathAssertions.TUnit's generated [GenerateAssertion(InlineMethodBody=true)] classes via [InternalsVisibleTo]; the analyzer only inspects this assembly's compilation.")]
internal static class MathFailureMessage
{
    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Vector2, Vector2, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Vector2 actual, Vector2 expected, double tolerance)
    {
        double ax = actual.X, ay = actual.Y;
        double ex = expected.X, ey = expected.Y;
        double dx = Math.Abs(ax - ex);
        double dy = Math.Abs(ay - ey);
        var sb = new StringBuilder();
        AppendHeader(sb, FormatVector(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatVector(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    ({Fmt(dx)}, {Fmt(dy)})").AppendLine();
        AppendExceeded(sb, tolerance, ("X", ax, ex), ("Y", ay, ey));
        return sb.ToString();
    }

    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Vector3, Vector3, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Vector3 actual, Vector3 expected, double tolerance)
    {
        double ax = actual.X, ay = actual.Y, az = actual.Z;
        double ex = expected.X, ey = expected.Y, ez = expected.Z;
        double dx = Math.Abs(ax - ex);
        double dy = Math.Abs(ay - ey);
        double dz = Math.Abs(az - ez);
        var sb = new StringBuilder();
        AppendHeader(sb, FormatVector(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatVector(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    ({Fmt(dx)}, {Fmt(dy)}, {Fmt(dz)})").AppendLine();
        AppendExceeded(sb, tolerance, ("X", ax, ex), ("Y", ay, ey), ("Z", az, ez));
        return sb.ToString();
    }

    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Vector4, Vector4, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Vector4 actual, Vector4 expected, double tolerance)
    {
        double ax = actual.X, ay = actual.Y, az = actual.Z, aw = actual.W;
        double ex = expected.X, ey = expected.Y, ez = expected.Z, ew = expected.W;
        double dx = Math.Abs(ax - ex);
        double dy = Math.Abs(ay - ey);
        double dz = Math.Abs(az - ez);
        double dw = Math.Abs(aw - ew);
        var sb = new StringBuilder();
        AppendHeader(sb, FormatVector(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatVector(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    ({Fmt(dx)}, {Fmt(dy)}, {Fmt(dz)}, {Fmt(dw)})").AppendLine();
        AppendExceeded(sb, tolerance, ("X", ax, ex), ("Y", ay, ey), ("Z", az, ez), ("W", aw, ew));
        return sb.ToString();
    }

    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Quaternion, Quaternion, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Quaternion actual, Quaternion expected, double tolerance)
    {
        double ax = actual.X, ay = actual.Y, az = actual.Z, aw = actual.W;
        double ex = expected.X, ey = expected.Y, ez = expected.Z, ew = expected.W;
        double dx = Math.Abs(ax - ex);
        double dy = Math.Abs(ay - ey);
        double dz = Math.Abs(az - ez);
        double dw = Math.Abs(aw - ew);
        var sb = new StringBuilder();
        AppendHeader(sb, FormatQuaternion(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatQuaternion(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    ({Fmt(dx)}, {Fmt(dy)}, {Fmt(dz)}, {Fmt(dw)})").AppendLine();
        AppendExceeded(sb, tolerance, ("X", ax, ex), ("Y", ay, ey), ("Z", az, ez), ("W", aw, ew));
        return sb.ToString();
    }

    /// <summary>Renders the per-cell failure for <see cref="MathTolerance.IsApproximatelyEqual(Matrix4x4, Matrix4x4, double)"/>.
    /// Cells are referenced in <c>[row, col]</c> 0-based form.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Matrix4x4 actual, Matrix4x4 expected, double tolerance)
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "matrix " + FormatMatrix(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatMatrix(actual)}").AppendLine();
        var exceeded = new StringBuilder();
        var first = true;
        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                double a = actual[row, col];
                double e = expected[row, col];
                // Delegate the exceeded classification to the predicate so equal-special-value
                // cells (NaN/NaN, same-sign infinity/infinity) are correctly treated as equal,
                // matching the AppendExceeded helper used by the vector / quaternion renderers.
                if (!MathTolerance.IsApproximatelyEqual(a, e, tolerance))
                {
                    if (!first)
                    {
                        exceeded.Append(", ");
                    }
                    double delta = Math.Abs(a - e);
                    exceeded.Append(CultureInfo.InvariantCulture, $"[{row},{col}] ({Fmt(delta)} > {Fmt(tolerance)})");
                    first = false;
                }
            }
        }
        sb.Append("  exceeded: ");
        sb.Append(exceeded.Length is 0 ? "(none on individual cell; NaN / infinity mismatch)" : exceeded.ToString());
        return sb.ToString();
    }

    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Plane, Plane, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Plane actual, Plane expected, double tolerance)
    {
        double anx = actual.Normal.X, any = actual.Normal.Y, anz = actual.Normal.Z, ad = actual.D;
        double enx = expected.Normal.X, eny = expected.Normal.Y, enz = expected.Normal.Z, ed = expected.D;
        double dnx = Math.Abs(anx - enx);
        double dny = Math.Abs(any - eny);
        double dnz = Math.Abs(anz - enz);
        double dd = Math.Abs(ad - ed);
        var sb = new StringBuilder();
        AppendHeader(sb, FormatPlane(expected), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatPlane(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    Normal=({Fmt(dnx)}, {Fmt(dny)}, {Fmt(dnz)}), D={Fmt(dd)}").AppendLine();
        AppendExceeded(sb, tolerance, ("Normal.X", anx, enx), ("Normal.Y", any, eny), ("Normal.Z", anz, enz), ("D", ad, ed));
        return sb.ToString();
    }

    /// <summary>Renders the per-component failure for <see cref="MathTolerance.IsApproximatelyEqual(Complex, Complex, double)"/>.</summary>
    /// <param name="actual">The value that failed the comparison.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text (multi-line, invariant-culture formatted).</returns>
    public static string ApproximatelyEqual(Complex actual, Complex expected, double tolerance)
    {
        double ar = actual.Real, ai = actual.Imaginary;
        double er = expected.Real, ei = expected.Imaginary;
        double dr = Math.Abs(ar - er);
        double di = Math.Abs(ai - ei);
        var sb = new StringBuilder();
        AppendHeader(sb, expected.ToString(CultureInfo.InvariantCulture), tolerance);
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {actual.ToString(CultureInfo.InvariantCulture)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta:    Real={Fmt(dr)}, Imaginary={Fmt(di)}").AppendLine();
        AppendExceeded(sb, tolerance, ("Real", ar, er), ("Imaginary", ai, ei));
        return sb.ToString();
    }

    /// <summary>Renders the first-failing-index failure for the <c>double[]</c> element-wise overload.</summary>
    /// <param name="actual">The array that failed the comparison.</param>
    /// <param name="expected">The expected array.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text. Length mismatch is flagged explicitly;
    /// otherwise the first index whose element fell outside tolerance is named.</returns>
    public static string ApproximatelyEqual(ReadOnlySpan<double> actual, ReadOnlySpan<double> expected, double tolerance)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"to be approximately equal element-wise within tolerance {Fmt(tolerance)} (length {expected.Length})").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  actual length: {actual.Length}").AppendLine();
        if (actual.Length != expected.Length)
        {
            sb.Append("  exceeded: length mismatch");
            return sb.ToString();
        }
        for (var i = 0; i < actual.Length; i++)
        {
            if (!MathTolerance.IsApproximatelyEqual(actual[i], expected[i], tolerance))
            {
                double delta = Math.Abs(actual[i] - expected[i]);
                sb.Append(CultureInfo.InvariantCulture, $"  first mismatch at index {i}: actual={Fmt(actual[i])}, expected={Fmt(expected[i])}, delta={Fmt(delta)}");
                return sb.ToString();
            }
        }
        sb.Append("  (no element exceeded tolerance; check NaN / infinity semantics)");
        return sb.ToString();
    }

    /// <summary>Renders the first-failing-index failure for the <c>float[]</c> element-wise overload.</summary>
    /// <param name="actual">The array that failed the comparison.</param>
    /// <param name="expected">The expected array.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text. Length mismatch is flagged explicitly;
    /// otherwise the first index whose element fell outside tolerance is named.</returns>
    public static string ApproximatelyEqual(ReadOnlySpan<float> actual, ReadOnlySpan<float> expected, float tolerance)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"to be approximately equal element-wise within tolerance {Fmt(tolerance)} (length {expected.Length})").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  actual length: {actual.Length}").AppendLine();
        if (actual.Length != expected.Length)
        {
            sb.Append("  exceeded: length mismatch");
            return sb.ToString();
        }
        for (var i = 0; i < actual.Length; i++)
        {
            if (!MathTolerance.IsApproximatelyEqual(actual[i], expected[i], tolerance))
            {
                double a = actual[i];
                double e = expected[i];
                double delta = Math.Abs(a - e);
                sb.Append(CultureInfo.InvariantCulture, $"  first mismatch at index {i}: actual={Fmt(a)}, expected={Fmt(e)}, delta={Fmt(delta)}");
                return sb.ToString();
            }
        }
        sb.Append("  (no element exceeded tolerance; check NaN / infinity semantics)");
        return sb.ToString();
    }

    /// <summary>Renders the failure for <see cref="MathTolerance.IsRotationallyEquivalent(Quaternion, Quaternion, double)"/>.
    /// Surfaces the absolute dot product (the load-bearing equivalence statistic), the threshold
    /// (<c>1 - tolerance</c>), and the two normalized quaternions, so the reader can see whether
    /// the non-equivalence came from a non-unit input or from a genuine rotational difference.</summary>
    /// <param name="actual">The quaternion that failed the rotational-equivalence check.</param>
    /// <param name="expected">The expected quaternion (same rotation, possibly different sign).</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text.</returns>
    public static string RotationallyEquivalent(Quaternion actual, Quaternion expected, double tolerance)
    {
        Quaternion an = Quaternion.Normalize(actual);
        Quaternion en = Quaternion.Normalize(expected);
        double dotF = Quaternion.Dot(an, en);
        double dot = Math.Abs(dotF);
        double threshold = 1.0 - tolerance;
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"to be rotationally equivalent to {FormatQuaternion(expected)} within tolerance {Fmt(tolerance)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  actual:        {FormatQuaternion(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  |dot(an, en)|: {Fmt(dot)} (threshold: {Fmt(threshold)} = 1 - {Fmt(tolerance)})").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  normalized:    actual={FormatQuaternion(an)}, expected={FormatQuaternion(en)}");
        return sb.ToString();
    }

    /// <summary>Renders the failure for <see cref="MathTolerance.IsGeometricallyEquivalent(Plane, Plane, double)"/>.
    /// Surfaces both candidate representations: the direct comparison and the sign-flipped one.</summary>
    /// <param name="actual">The plane that failed the geometric-equivalence check.</param>
    /// <param name="expected">The expected plane (same set of points, possibly opposite sign on <c>(n, d)</c>).</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text showing the deltas against both the direct and the sign-flipped form of <paramref name="expected"/>.</returns>
    public static string GeometricallyEquivalent(Plane actual, Plane expected, double tolerance)
    {
        var flipped = new Plane(-expected.Normal, -expected.D);
        double anx = actual.Normal.X, any = actual.Normal.Y, anz = actual.Normal.Z, ad = actual.D;
        double enx = expected.Normal.X, eny = expected.Normal.Y, enz = expected.Normal.Z, ed = expected.D;
        double fnx = flipped.Normal.X, fny = flipped.Normal.Y, fnz = flipped.Normal.Z, fd = flipped.D;
        double dnx = Math.Abs(anx - enx);
        double dny = Math.Abs(any - eny);
        double dnz = Math.Abs(anz - enz);
        double dd = Math.Abs(ad - ed);
        double fdnx = Math.Abs(anx - fnx);
        double fdny = Math.Abs(any - fny);
        double fdnz = Math.Abs(anz - fnz);
        double fdd = Math.Abs(ad - fd);
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"to be geometrically equivalent to {FormatPlane(expected)} within tolerance {Fmt(tolerance)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  actual:   {FormatPlane(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta vs expected:        Normal=({Fmt(dnx)}, {Fmt(dny)}, {Fmt(dnz)}), D={Fmt(dd)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  delta vs sign-flipped:    Normal=({Fmt(fdnx)}, {Fmt(fdny)}, {Fmt(fdnz)}), D={Fmt(fdd)}");
        return sb.ToString();
    }

    /// <summary>Renders the failure for <see cref="MathTolerance.HasAxisAngleApproximately(Quaternion, Vector3, double, double)"/>.
    /// Shows the extracted axis and angle (in degrees) alongside the expected ones. The delta
    /// angle is reported as the shorter of the two arcs between expected and extracted, so a
    /// 359 vs 1 degree comparison renders as 2 (the geometric distance in <c>SO(3)</c>), not
    /// 358 (the linear-arithmetic difference).</summary>
    /// <remarks>
    /// The predicate (<see cref="MathTolerance.HasAxisAngleApproximately"/>) and this renderer
    /// take independent paths to their results: the predicate routes through
    /// <see cref="MathTolerance.IsRotationallyEquivalent(Quaternion, Quaternion, double)"/>
    /// (a dot-product check that is robust at every angle and across the SO(3) double cover);
    /// the renderer reconstructs the extracted axis-angle pair via
    /// <see cref="MathTolerance.ExtractAxisAngle"/> for diagnostic display. Near numerical
    /// boundaries (180 degrees, near-identity rotations) the two paths can differ slightly:
    /// the predicate stays correct, but the rendered extracted angle may show a small residual
    /// where the dot-product test still cleared / failed by tolerance.
    /// </remarks>
    /// <param name="actual">The quaternion that failed the axis-angle check.</param>
    /// <param name="expectedAxis">The expected rotation axis as supplied by the caller (un-normalized).</param>
    /// <param name="expectedAngleDegrees">The expected rotation angle in degrees.</param>
    /// <param name="extractedAxis">The axis extracted from <paramref name="actual"/> by
    /// <see cref="MathTolerance.ExtractAxisAngle"/>, sign-aligned with the normalized
    /// <paramref name="expectedAxis"/>.</param>
    /// <param name="extractedAngleDegrees">The angle extracted from <paramref name="actual"/>, in
    /// degrees, sign-aligned with <paramref name="expectedAngleDegrees"/>. The value is unwrapped
    /// from <see cref="Math.Atan2(double, double)"/> output and lies in the open interval
    /// <c>(-360, 360)</c>.</param>
    /// <param name="tolerance">The tolerance the comparison was performed against.</param>
    /// <returns>The rendered failure-message text.</returns>
    public static string AxisAngleApproximately(Quaternion actual, Vector3 expectedAxis, double expectedAngleDegrees, Vector3 extractedAxis, double extractedAngleDegrees, double tolerance)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"to be a rotation of approximately {Fmt(expectedAngleDegrees)} degrees around {FormatVector(expectedAxis)} within tolerance {Fmt(tolerance)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  actual quaternion:  {FormatQuaternion(actual)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  extracted axis:     {FormatVector(extractedAxis)}").AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  extracted angle:    {Fmt(extractedAngleDegrees)} degrees").AppendLine();
        // Circular distance modulo 360: shorter of the two arcs. Without this, (359, 1)
        // would render as 358 instead of 2, which contradicts the predicate's SO(3) verdict.
        double rawDelta = Math.Abs(extractedAngleDegrees - expectedAngleDegrees) % 360.0;
        double dAngle = Math.Min(rawDelta, 360.0 - rawDelta);
        sb.Append(CultureInfo.InvariantCulture, $"  delta angle:        {Fmt(dAngle)} degrees");
        return sb.ToString();
    }

    private static void AppendHeader(StringBuilder sb, string expectedRendered, double tolerance)
    {
        sb.Append(CultureInfo.InvariantCulture, $"to be approximately equal to {expectedRendered} component-wise within tolerance {Fmt(tolerance)}").AppendLine();
    }

    private static void AppendExceeded(StringBuilder sb, double tolerance, params (string Name, double Actual, double Expected)[] components)
    {
        sb.Append("  exceeded: ");
        var first = true;
        foreach (var (name, actual, expected) in components)
        {
            // Delegate the exceeded classification to the predicate itself. The predicate
            // treats NaN/NaN and same-sign infinity/infinity as equal; a naive
            // `delta > tolerance || double.IsNaN(delta)` would incorrectly flag those
            // equal-special-value pairs as exceeded (their delta computes to NaN even
            // though the predicate verdict is equal).
            if (!MathTolerance.IsApproximatelyEqual(actual, expected, tolerance))
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                double delta = Math.Abs(actual - expected);
                sb.Append(CultureInfo.InvariantCulture, $"{name} ({Fmt(delta)} > {Fmt(tolerance)})");
                first = false;
            }
        }
        if (first)
        {
            sb.Append("(none on individual component; NaN / infinity mismatch)");
        }
    }

    private static string Fmt(double value) => value.ToString("G", CultureInfo.InvariantCulture);

    private static string FormatVector(Vector2 v) => v.ToString("G", CultureInfo.InvariantCulture);
    private static string FormatVector(Vector3 v) => v.ToString("G", CultureInfo.InvariantCulture);
    private static string FormatVector(Vector4 v) => v.ToString("G", CultureInfo.InvariantCulture);
    private static string FormatQuaternion(Quaternion q) =>
        $"{{X:{Fmt(q.X)} Y:{Fmt(q.Y)} Z:{Fmt(q.Z)} W:{Fmt(q.W)}}}";
    private static string FormatMatrix(Matrix4x4 m) =>
        $"{{ {{ M11:{Fmt(m.M11)} M12:{Fmt(m.M12)} M13:{Fmt(m.M13)} M14:{Fmt(m.M14)} }} {{ M21:{Fmt(m.M21)} M22:{Fmt(m.M22)} M23:{Fmt(m.M23)} M24:{Fmt(m.M24)} }} {{ M31:{Fmt(m.M31)} M32:{Fmt(m.M32)} M33:{Fmt(m.M33)} M34:{Fmt(m.M34)} }} {{ M41:{Fmt(m.M41)} M42:{Fmt(m.M42)} M43:{Fmt(m.M43)} M44:{Fmt(m.M44)} }} }}";
    private static string FormatPlane(Plane p) =>
        $"{{ Normal:{FormatVector(p.Normal)} D:{Fmt(p.D)} }}";
}
