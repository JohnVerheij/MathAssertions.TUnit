using System.Threading;
using System.Threading.Tasks;
using MathAssertions;
using MathAssertions.TUnit;
using PublicApiGenerator;
using SnapshotAssertions.TUnit;

namespace MathAssertions.TUnit.SnapshotTests;

/// <summary>
/// Pins the public API surface of both shipped packages (<c>MathAssertions</c> and
/// <c>MathAssertions.TUnit</c>) using <c>SnapshotAssertions.TUnit</c>'s
/// <c>MatchesSnapshot()</c> chain. Any change to a public type, member, signature, attribute,
/// or visibility produces a diff against the corresponding <c>.expected.txt</c> file under
/// <c>Snapshots/</c> and fails the test until the snapshot is explicitly re-accepted (write
/// the new content to the expected path, or run with <c>SNAPSHOT_ACCEPT=1</c> to auto-write).
/// </summary>
/// <remarks>
/// <para>
/// Stronger than ApiCompat's per-version baseline check because these snapshots fire on every
/// PR, not just at pack time.
/// </para>
/// <para>
/// Cross-package dogfooding: this project consumes <c>SnapshotAssertions.TUnit</c> as a
/// downstream user of the family would, demonstrating that the family's snapshot helper is
/// suitable for the package's own public-API surface checks.
/// </para>
/// </remarks>
[Category("Smoke")]
[Timeout(10_000)]
internal sealed class PublicApiTests
{
    /// <summary>
    /// Pins the public surface of the framework-agnostic <c>MathAssertions</c> assembly:
    /// <c>MathTolerance</c> and any future BCL-only helpers.
    /// </summary>
    [Test]
    public async Task MathAssertionsPublicApiHasNotChangedAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var assembly = typeof(MathTolerance).Assembly;
        // Normalize line endings so the snapshot baseline survives both Linux CI (LF native)
        // and Windows local dev (CRLF native). Without this, PublicApiGenerator emits the
        // platform's native EOL while the committed .expected.txt baseline is always LF
        // (per .gitattributes), and Windows local runs would diff against the CI-accepted
        // baseline.
        var publicApi = assembly.GeneratePublicApi().ReplaceLineEndings("\n");

        await Assert.That(publicApi).MatchesSnapshot();
    }

    /// <summary>
    /// Pins the public surface of the TUnit adapter assembly: the <c>VectorAssertions</c>
    /// fluent extension class plus any future <c>[GenerateAssertion]</c>-emitted entry points.
    /// </summary>
    [Test]
    public async Task MathAssertionsTUnitPublicApiHasNotChangedAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var assembly = typeof(VectorAssertions).Assembly;
        // Normalize line endings so the snapshot baseline survives both Linux CI (LF native)
        // and Windows local dev (CRLF native). Without this, PublicApiGenerator emits the
        // platform's native EOL while the committed .expected.txt baseline is always LF
        // (per .gitattributes), and Windows local runs would diff against the CI-accepted
        // baseline.
        var publicApi = assembly.GeneratePublicApi().ReplaceLineEndings("\n");

        await Assert.That(publicApi).MatchesSnapshot();
    }
}
