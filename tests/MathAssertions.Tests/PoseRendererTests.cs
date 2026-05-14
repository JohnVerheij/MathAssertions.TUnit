using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Render;

namespace MathAssertions.Tests;

/// <summary>Pins the rendered format produced by <see cref="PoseRenderer"/>. Each test fixes
/// one corner of the contract: the two-line full-pose shape, the position-only and
/// orientation-only single-line overloads, the fixed six-digit component format, the
/// general-format tolerance, LF line endings, and verbatim (non-canonicalized) quaternion
/// signs. Snapshot consumers downstream rely on the exact byte shape of these outputs;
/// changing them is a breaking change.</summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class PoseRendererTests
{
    /// <summary>The literal LF byte the renderer emits to terminate each line. Hardcoded here
    /// so these tests assert against the same cross-platform-deterministic byte sequence the
    /// renderer produces (see <c>PoseRenderer</c> XML docs for the rationale).</summary>
    private const string Lf = "\n";

    /// <summary>The full-pose overload renders a <c>pos:</c> line followed by a <c>quat:</c>
    /// line, each LF-terminated, with the tolerance echoed into both.</summary>
    [Test]
    public async Task Render_FullPose_RendersPosThenQuatLines(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(
            new Vector3(1.5f, -2.25f, 0.125f),
            new Quaternion(0.5f, -0.5f, 0.5f, 0.5f),
            tolerance: 0.001);

        var expected = "pos: (1.500000, -2.250000, 0.125000) tol=0.001" + Lf
                     + "quat: (0.500000, -0.500000, 0.500000, 0.500000) tol=0.001" + Lf;

        await Assert.That(rendered).IsEqualTo(expected);
    }

    /// <summary>The position-only overload renders just the <c>pos:</c> line, with no
    /// <c>quat:</c> line.</summary>
    [Test]
    public async Task Render_PositionOnly_RendersOnlyPosLine(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(new Vector3(1.5f, -2.25f, 0.125f), tolerance: 0.001);

        await Assert.That(rendered).IsEqualTo("pos: (1.500000, -2.250000, 0.125000) tol=0.001" + Lf);
        await Assert.That(rendered).DoesNotContain("quat:");
    }

    /// <summary>The orientation-only overload renders just the <c>quat:</c> line, with no
    /// <c>pos:</c> line.</summary>
    [Test]
    public async Task Render_OrientationOnly_RendersOnlyQuatLine(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(new Quaternion(0.5f, -0.5f, 0.5f, 0.5f), tolerance: 0.001);

        await Assert.That(rendered).IsEqualTo("quat: (0.500000, -0.500000, 0.500000, 0.500000) tol=0.001" + Lf);
        await Assert.That(rendered).DoesNotContain("pos:");
    }

    /// <summary>Components render with six trailing fractional digits even for whole numbers:
    /// proves the fixed <c>F6</c> format is in effect (the <c>G</c> general format would
    /// render <c>2</c>, not <c>2.000000</c>).</summary>
    [Test]
    public async Task Render_Components_UseSixFractionalDigits(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(new Vector3(2f, 2f, 2f), tolerance: 0.001);

        await Assert.That(rendered).IsEqualTo("pos: (2.000000, 2.000000, 2.000000) tol=0.001" + Lf);
    }

    /// <summary>A quaternion and its negation are the same rotation but render differently:
    /// the renderer never canonicalizes the sign, so a regression that flips a quaternion
    /// sign surfaces as a snapshot diff.</summary>
    [Test]
    public async Task Render_QuaternionSign_NotCanonicalized(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var q = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
        var negated = -q;

        var renderedQ = PoseRenderer.Render(q, tolerance: 0.001);
        var renderedNegated = PoseRenderer.Render(negated, tolerance: 0.001);

        await Assert.That(renderedQ).IsNotEqualTo(renderedNegated);
        await Assert.That(renderedQ).IsEqualTo("quat: (0.500000, 0.500000, 0.500000, 0.500000) tol=0.001" + Lf);
        await Assert.That(renderedNegated).IsEqualTo("quat: (-0.500000, -0.500000, -0.500000, -0.500000) tol=0.001" + Lf);
    }

    /// <summary>Lines are terminated with the literal LF byte, never
    /// <c>Environment.NewLine</c>: a baseline committed on one OS stays byte-stable for test
    /// runs on every other.</summary>
    [Test]
    public async Task Render_UsesLfLineEndings(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(Vector3.One, Quaternion.Identity, tolerance: 0.001);

        await Assert.That(rendered).DoesNotContain("\r");
    }

    /// <summary>The tolerance renders with the invariant-culture <c>G</c> general format,
    /// which switches to scientific notation for small magnitudes.</summary>
    [Test]
    public async Task Render_Tolerance_UsesInvariantGeneralFormat(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rendered = PoseRenderer.Render(Vector3.Zero, tolerance: 1e-6);

        await Assert.That(rendered).IsEqualTo("pos: (0.000000, 0.000000, 0.000000) tol=1E-06" + Lf);
    }
}
