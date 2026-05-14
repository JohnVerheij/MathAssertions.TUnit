using System.Globalization;
using System.Numerics;
using System.Text;

namespace MathAssertions.Render;

/// <summary>
/// Pure renderer that converts a 3D pose — a <see cref="Vector3"/> position and / or a
/// <see cref="Quaternion"/> orientation — into deterministic, snapshot-friendly text.
/// Position renders as <c>pos: (x, y, z) tol=t</c> and orientation as
/// <c>quat: (x, y, z, w) tol=t</c>, each on its own LF-terminated line; the full-pose
/// overload emits both.
/// </summary>
/// <remarks>
/// <para>
/// <b>Pairs naturally with snapshot assertions.</b> Render the pose once, then pin the
/// result against a baseline:
/// </para>
/// <code>
/// var rendered = PoseRenderer.Render(position, orientation, tolerance: 1e-4);
/// await Assert.That(rendered).MatchesSnapshot();
/// </code>
/// <para>
/// The <c>MatchesSnapshot()</c> extension above lives in the sibling
/// <c>SnapshotAssertions.TUnit</c> package; this package does not depend on it. The
/// two-line composition is deliberate: it lets consumers reach for the renderer without
/// committing to a specific snapshot framework, and lets the SnapshotAssertions package
/// stay an opt-in pairing rather than a transitive dependency.
/// </para>
/// <para>
/// <b>Orientation is rendered verbatim — the sign is not canonicalized.</b> A quaternion
/// and its negation (<c>q</c> and <c>-q</c>) represent the same rotation but render
/// differently. That is intentional: pinning the raw components means a regression that
/// flips a quaternion sign surfaces as a snapshot diff. Callers asserting rotational
/// equivalence rather than component identity should use the <c>IsRotationallyEquivalentTo</c>
/// assertion instead of a rendered snapshot.
/// </para>
/// <para>
/// <b>The tolerance is recorded, not applied.</b> The renderer performs no comparison; it
/// echoes the supplied tolerance into the output so the snapshot baseline records the
/// asserted precision. A tolerance silently loosened during a refactor then surfaces as a
/// snapshot diff.
/// </para>
/// <para>
/// <b>Deterministic formatting.</b> Components use the invariant-culture <c>F6</c> fixed
/// format — six fractional digits, enough to distinguish typical calibration / grasp-pose
/// outputs without recording floating-point noise that would make the snapshot fragile
/// across platforms. The tolerance uses the invariant-culture <c>G</c> general format.
/// Lines are terminated with the literal LF byte (<c>'\n'</c>), never
/// <see cref="System.Environment.NewLine"/>, so a snapshot committed on one OS stays
/// byte-stable for test runs on every other.
/// </para>
/// </remarks>
public static class PoseRenderer
{
    /// <summary>Renders a position and orientation pair as a two-line pose snapshot: a
    /// <c>pos:</c> line followed by a <c>quat:</c> line.</summary>
    /// <param name="position">The pose translation (typically metres).</param>
    /// <param name="orientation">The pose rotation, rendered verbatim (sign not canonicalized).</param>
    /// <param name="tolerance">The asserted tolerance, echoed into both lines.</param>
    /// <returns>A deterministic two-line, LF-terminated text rendering of the pose.</returns>
    public static string Render(Vector3 position, Quaternion orientation, double tolerance)
    {
        var sb = new StringBuilder(capacity: 160);
        AppendPosition(sb, position, tolerance);
        AppendOrientation(sb, orientation, tolerance);
        return sb.ToString();
    }

    /// <summary>Renders a position alone as a single <c>pos:</c> line.</summary>
    /// <param name="position">The pose translation (typically metres).</param>
    /// <param name="tolerance">The asserted tolerance, echoed into the line.</param>
    /// <returns>A deterministic single-line, LF-terminated text rendering of the position.</returns>
    public static string Render(Vector3 position, double tolerance)
    {
        var sb = new StringBuilder(capacity: 80);
        AppendPosition(sb, position, tolerance);
        return sb.ToString();
    }

    /// <summary>Renders an orientation alone as a single <c>quat:</c> line.</summary>
    /// <param name="orientation">The pose rotation, rendered verbatim (sign not canonicalized).</param>
    /// <param name="tolerance">The asserted tolerance, echoed into the line.</param>
    /// <returns>A deterministic single-line, LF-terminated text rendering of the orientation.</returns>
    public static string Render(Quaternion orientation, double tolerance)
    {
        var sb = new StringBuilder(capacity: 96);
        AppendOrientation(sb, orientation, tolerance);
        return sb.ToString();
    }

    private static void AppendPosition(StringBuilder sb, Vector3 position, double tolerance)
    {
        sb.Append(
            CultureInfo.InvariantCulture,
            $"pos: ({position.X:F6}, {position.Y:F6}, {position.Z:F6}) tol={tolerance:G}\n");
    }

    private static void AppendOrientation(StringBuilder sb, Quaternion orientation, double tolerance)
    {
        sb.Append(
            CultureInfo.InvariantCulture,
            $"quat: ({orientation.X:F6}, {orientation.Y:F6}, {orientation.Z:F6}, {orientation.W:F6}) tol={tolerance:G}\n");
    }
}
