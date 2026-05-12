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

    /// <summary>Exercises the identity-rotation branch inside
    /// <c>MathTolerance.ExtractAxisAngle</c>: when the actual quaternion normalizes to (or
    /// near) the identity, <c>|xyz|</c> is zero and the extracted axis is reported as
    /// <see cref="Vector3.Zero"/>. The chain fails (expected angle is 90 degrees, actual is
    /// the identity), so the renderer fires and the identity-branch early-return is taken.</summary>
    [Test]
    public async Task Quaternion_HasAxisAngleApproximately_ActualIsIdentity_RendersZeroExtractedAxis(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(Quaternion.Identity).HasAxisAngleApproximately(Vector3.UnitX, expectedAngleDegrees: 90.0, tolerance: 1e-4);
        }).Throws<AssertionException>();

        await Assert.That(ex!.Message).Contains("extracted axis:");
        await Assert.That(ex.Message).Contains("extracted angle:");
    }

    /// <summary>Exercises the sign-flip alignment branch inside
    /// <c>MathTolerance.ExtractAxisAngle</c>: when the extracted axis points in the opposite
    /// direction from the normalized expected axis, the renderer flips the extracted axis and
    /// negates the extracted angle so the displayed pair is sign-aligned with the caller's
    /// supplied axis. Triggered by asking for an expected rotation around <c>-UnitY</c> while
    /// the actual rotation is around <c>+UnitY</c> — both encode the SAME physical rotation
    /// (so the predicate is happy after <see cref="System.Math"/>-level sign correction), but
    /// to drive the chain into the FAILURE renderer we use a different angle on top.</summary>
    [Test]
    public async Task Quaternion_HasAxisAngleApproximately_ExtractedAxisOppositeExpected_AlignsForDisplay(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // actual rotates 60 degrees around +UnitY; we ask for 30 degrees around -UnitY.
        // The sign-flip path runs (dot(extracted=+UnitY, expected=-UnitY) < 0 → flip both).
        // The chain fails because 60 != 30 within tolerance, so the renderer fires.
        Quaternion actual = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(System.Math.PI / 3.0));
        AssertionException? ex = await Assert.That(async () =>
        {
            await Assert.That(actual).HasAxisAngleApproximately(-Vector3.UnitY, expectedAngleDegrees: 30.0, tolerance: 1e-4);
        }).Throws<AssertionException>();

        // Render is informational; the message must surface the standard fields.
        await Assert.That(ex!.Message).Contains("extracted axis:");
        await Assert.That(ex.Message).Contains("extracted angle:");
        await Assert.That(ex.Message).Contains("delta angle:");
    }
}
