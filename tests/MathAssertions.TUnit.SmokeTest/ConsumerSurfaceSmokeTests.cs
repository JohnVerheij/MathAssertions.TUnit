namespace Smoke.Consumer;

/// <summary>
/// Smoke tests proving that an external consumer can adopt MathAssertions.TUnit purely via
/// the README's recommended GlobalUsings.cs snippet, with no extra
/// <c>using MathAssertions.TUnit;</c> directive at every call site and no other wiring. The
/// test class lives in <c>Smoke.Consumer</c> deliberately: MathAssertions.TUnit's own test
/// project is in the <c>MathAssertions.TUnit.Tests</c> namespace, which inherits
/// parent-namespace visibility into <c>MathAssertions.TUnit</c>. That inheritance would mask
/// any future namespace-resolution bug in the source-generated entry points. By placing this
/// file in a namespace with NO parent relationship to MathAssertions.TUnit, this project is
/// the canonical regression coverage for the resolution-pathway bug class.
/// </summary>
[Category("ConsumerSurface")]
[Timeout(10_000)]
internal sealed class ConsumerSurfaceSmokeTests
{
    /// <summary>
    /// Pins that <c>IsApproximatelyEqualTo</c> on Vector3 resolves cleanly for an external
    /// consumer, source-generator-emitted entry point in <c>TUnit.Assertions.Extensions</c>;
    /// auto-imports alongside <c>Assert.That</c>.
    /// </summary>
    [Test]
    public async Task Vector3IsApproximatelyEqualToResolvesAndPassesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);

        await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
    }

    /// <summary>
    /// Pins that the precision-preserving widen-to-double cast on the Vector3 overload is
    /// honored end-to-end through the source-generated wrapper. A tolerance of 1e-9 is below
    /// float epsilon; if the chain accidentally narrowed tolerance to float anywhere along
    /// the way, this test would fail.
    /// </summary>
    [Test]
    public async Task Vector3IsApproximatelyEqualToTightDoubleToleranceResolvesAndPassesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1.0f, 1.0f, 1.0f);

        await Assert.That(a).IsApproximatelyEqualTo(a, tolerance: 1e-9);
    }
}
