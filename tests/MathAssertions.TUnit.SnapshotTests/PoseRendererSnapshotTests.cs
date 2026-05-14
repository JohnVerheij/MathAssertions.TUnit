using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Render;
using SnapshotAssertions.TUnit;

namespace MathAssertions.TUnit.SnapshotTests;

/// <summary>
/// End-to-end integration test for the canonical "render a pose, pin via snapshot" pattern
/// documented in the <c>README.md</c> cookbook. Exercises
/// <see cref="PoseRenderer.Render(System.Numerics.Vector3, System.Numerics.Quaternion, double)"/>
/// from <c>MathAssertions</c> paired with <c>MatchesSnapshot()</c> from
/// <c>SnapshotAssertions.TUnit</c> against a committed baseline.
/// </summary>
/// <remarks>
/// The two packages share no PackageReference: <c>MathAssertions.TUnit</c> does not depend on
/// <c>SnapshotAssertions.TUnit</c>. This test project adds both as consumer-side dependencies
/// to validate the pairing the same way a downstream consumer would. A baseline drift on
/// either side (renderer format change, snapshot framework change) surfaces here before it
/// reaches downstream consumers.
/// </remarks>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class PoseRendererSnapshotTests
{
    /// <summary>
    /// Pins the rendered text of a fixed grasp-pose-shaped position / orientation pair against
    /// the committed <c>RenderedPose.expected.txt</c> baseline. The baseline is the canonical
    /// shape consumers will see: a <c>pos:</c> line and a <c>quat:</c> line, invariant-culture
    /// <c>F6</c> components, the tolerance echoed into each line, LF line endings.
    /// </summary>
    [Test]
    public async Task PoseRendererProducesSnapshotMatchingBaseline(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var position = new Vector3(0.3f, 0.15f, 0.45f);
        var orientation = new Quaternion(0f, 0f, 0.7071068f, 0.7071068f);

        var rendered = PoseRenderer.Render(position, orientation, tolerance: 1e-4);

        await Assert.That(rendered).MatchesSnapshot("RenderedPose");
    }
}
