using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.TUnit;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// End-to-end tests for the <c>[GenerateAssertion]</c>-emitted Vector3 fluent extension.
/// One happy path, one failing case, plus a tight-tolerance precision check that fails
/// if the source generator or wrapper accidentally narrows the tolerance to float.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class AdapterAssertionsTests
{
    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_Happy(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);
        await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
    }

    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_Fails(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1, 2, 99);
        await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
        }).Throws<AssertionException>();
    }

    [Test]
    public async Task Vector3_IsApproximatelyEqualTo_TightDoubleTolerance_Passes(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1.0f, 1.0f, 1.0f);
        await Assert.That(a).IsApproximatelyEqualTo(a, tolerance: 1e-9);
    }
}
