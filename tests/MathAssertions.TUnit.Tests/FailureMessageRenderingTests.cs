using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.TUnit;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// End-to-end coverage of the rich failure-message renderers (in <c>MathFailureMessage</c>)
/// invoked when a fluent-chain assertion fails. Each renderer is exercised via the chain
/// rather than called directly so the test surface matches the consumer-observable path,
/// and so the coverage-instrumented assemblies see the renderer code being reached through
/// the production wiring.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class FailureMessageRenderingTests
{
    [Test]
    public async Task Vector2_IsApproximatelyEqualTo_Fails_SurfacesAxisDelta(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1.5f, 2.0f);
        var b = new Vector2(1.0f, 2.0f);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("delta:");
        await Assert.That(ex.Message).Contains("X (");
    }

    [Test]
    public async Task Vector4_IsApproximatelyEqualTo_Fails_NamesEveryComponent(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector4(1.5f, 2.5f, 3.5f, 4.5f);
        var b = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("X (");
        await Assert.That(ex.Message).Contains("Y (");
        await Assert.That(ex.Message).Contains("Z (");
        await Assert.That(ex.Message).Contains("W (");
    }

    [Test]
    public async Task Quaternion_IsApproximatelyEqualTo_Fails_NamesXyzwComponents(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Quaternion(0.1f, 0.2f, 0.3f, 0.95f);
        var b = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("X (");
        await Assert.That(ex.Message).Contains("W (");
    }

    [Test]
    public async Task Matrix4x4_IsApproximatelyEqualTo_Fails_NamesCellInRowColForm(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Matrix4x4 a = Matrix4x4.Identity;
        a.M23 = 0.5f;
        Matrix4x4 b = Matrix4x4.Identity;
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("[1,2]");
    }

    [Test]
    public async Task Plane_IsApproximatelyEqualTo_Fails_SurfacesNormalAndDelta(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(new Vector3(1.0f, 0.1f, 0.0f), 2.5f);
        var b = new Plane(new Vector3(1.0f, 0.0f, 0.0f), 2.0f);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("Normal.Y (");
        await Assert.That(ex.Message).Contains("D (");
    }

    [Test]
    public async Task Complex_IsApproximatelyEqualTo_Fails_NamesRealAndImaginary(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new System.Numerics.Complex(1.5, 2.5);
        var b = new System.Numerics.Complex(1.0, 2.0);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("Real (");
        await Assert.That(ex.Message).Contains("Imaginary (");
    }

    [Test]
    public async Task DoubleArray_IsApproximatelyEqualTo_Fails_NamesFirstFailingIndex(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] a = [1.0, 2.0, 3.0, 4.5, 5.0];
        double[] b = [1.0, 2.0, 3.0, 4.0, 5.0];
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("first mismatch at index 3");
    }

    [Test]
    public async Task DoubleArray_IsApproximatelyEqualTo_LengthMismatch_FailsExplicitly(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        double[] a = [1.0, 2.0, 3.0];
        double[] b = [1.0, 2.0];
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("length mismatch");
    }

    [Test]
    public async Task FloatArray_IsApproximatelyEqualTo_Fails_NamesFirstFailingIndex(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        float[] a = [1.0f, 2.0f, 3.0f, 4.5f, 5.0f];
        float[] b = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f];
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6f);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("first mismatch at index 3");
    }

    [Test]
    public async Task FloatArray_IsApproximatelyEqualTo_LengthMismatch_FailsExplicitly(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        float[] a = [1.0f, 2.0f, 3.0f];
        float[] b = [1.0f, 2.0f];
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 1e-6f);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("length mismatch");
    }

    [Test]
    public async Task Quaternion_IsRotationallyEquivalentTo_Fails_SurfacesDotAndThreshold(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Quaternion a = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(System.Math.PI / 4.0));
        Quaternion b = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(System.Math.PI / 4.0));
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsRotationallyEquivalentTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("dot");
        await Assert.That(ex.Message).Contains("threshold");
    }

    [Test]
    public async Task Plane_IsGeometricallyEquivalentTo_Fails_RendersBothCandidates(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Plane(new Vector3(0.5f, 0.5f, 0.0f), 1.0f);
        var b = new Plane(new Vector3(1.0f, 0.0f, 0.0f), 2.0f);
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(a).IsGeometricallyEquivalentTo(b, tolerance: 1e-6);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("delta vs expected:");
        await Assert.That(ex.Message).Contains("delta vs sign-flipped:");
    }
}
