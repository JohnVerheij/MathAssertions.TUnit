using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Render;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Coverage-instrumentation exercise for <see cref="PoseRenderer"/>. The authoritative
/// contract tests live in <c>tests/MathAssertions.Tests/PoseRendererTests.cs</c> — that
/// project is framework-agnostic (no <c>MathAssertions.TUnit</c> reference), so the
/// renderer's framework-independence is structurally enforced. The CI coverage gate,
/// however, instruments only this project's test exe; the renderer's lines sit in
/// <c>MathAssertions.dll</c> and would show as uncovered without a touchpoint here. Every
/// overload of <see cref="PoseRenderer"/> — full pose, position-only, orientation-only —
/// is exercised once below so the production assembly's coverage rate reflects the actual
/// test depth.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class PoseRendererCoverageExercise
{
    /// <summary>Cross-platform-deterministic LF the renderer emits; see <c>PoseRenderer</c>
    /// XML docs for the rationale on hardcoding it.</summary>
    private const string Lf = "\n";

    [Test]
    public async Task ExercisesAllOverloads(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var position = new Vector3(1.5f, -2.25f, 0.125f);
        var orientation = new Quaternion(0.5f, -0.5f, 0.5f, 0.5f);

        // Full pose: pos line then quat line.
        await Assert.That(PoseRenderer.Render(position, orientation, tolerance: 0.001))
            .IsEqualTo(
                "pos: (1.500000, -2.250000, 0.125000) tol=0.001" + Lf
                + "quat: (0.500000, -0.500000, 0.500000, 0.500000) tol=0.001" + Lf);

        // Position-only overload.
        await Assert.That(PoseRenderer.Render(position, tolerance: 0.001))
            .IsEqualTo("pos: (1.500000, -2.250000, 0.125000) tol=0.001" + Lf);

        // Orientation-only overload.
        await Assert.That(PoseRenderer.Render(orientation, tolerance: 0.001))
            .IsEqualTo("quat: (0.500000, -0.500000, 0.500000, 0.500000) tol=0.001" + Lf);
    }
}
