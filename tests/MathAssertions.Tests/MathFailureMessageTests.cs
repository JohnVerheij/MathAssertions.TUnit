using System;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.Tests;

/// <summary>
/// Pins the rich failure-message rendering shape from <see cref="MathFailureMessage"/>.
/// The exact text format is documented as not stable in the README; these tests pin the
/// load-bearing markers (component names, the <c>delta:</c> and <c>exceeded:</c> section
/// headers, the <c>(value > tolerance)</c> exceeded-component shape) rather than full
/// string equality, so the format can evolve without rewriting every test.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class MathFailureMessageTests
{
    [Test]
    public async Task Vector3_SingleAxisExceeded_NamesAxisAndDelta(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(1.0f, 2.0f, 3.001f);
        var expected = new Vector3(1.0f, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("component-wise");
        await Assert.That(message).Contains("actual:");
        await Assert.That(message).Contains("delta:");
        await Assert.That(message).Contains("exceeded:");
        await Assert.That(message).Contains("Z (");
        await Assert.That(message).DoesNotContain("X (");
        await Assert.That(message).DoesNotContain("Y (");
    }

    [Test]
    public async Task Vector3_MultiAxisExceeded_NamesAllAxes(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(1.5f, 2.5f, 3.5f);
        var expected = new Vector3(1.0f, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("X (");
        await Assert.That(message).Contains("Y (");
        await Assert.That(message).Contains("Z (");
    }

    [Test]
    public async Task Vector3_NaNComponent_FlaggedInExceeded(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(1.0f, float.NaN, 3.0f);
        var expected = new Vector3(1.0f, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("Y (");
        await Assert.That(message).Contains("NaN");
    }

    [Test]
    public async Task Quaternion_NamesXyzwComponents(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Quaternion(0.1f, 0.2f, 0.3f, 0.95f);
        var expected = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("X (");
        await Assert.That(message).Contains("Y (");
        await Assert.That(message).Contains("Z (");
        await Assert.That(message).Contains("W (");
    }

    [Test]
    public async Task Matrix4x4_OffDiagonalExceeded_NamesCellInRowColForm(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Matrix4x4 actual = Matrix4x4.Identity;
        actual.M23 = 0.5f;
        Matrix4x4 expected = Matrix4x4.Identity;
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("[1,2]");
    }

    [Test]
    public async Task Plane_NormalAxisAndDistanceDeltasShown(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Plane(new Vector3(1.0f, 0.1f, 0.0f), 2.5f);
        var expected = new Plane(new Vector3(1.0f, 0.0f, 0.0f), 2.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("Normal.Y (");
        await Assert.That(message).Contains("D (");
    }

    [Test]
    public async Task DoubleArray_FirstFailingIndexNamed(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        double[] actual = [1.0, 2.0, 3.0, 4.5, 5.0];
        double[] expected = [1.0, 2.0, 3.0, 4.0, 5.0];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("first mismatch at index 3");
    }

    [Test]
    public async Task DoubleArray_LengthMismatch_FlaggedExplicitly(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        double[] actual = [1.0, 2.0, 3.0];
        double[] expected = [1.0, 2.0];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("length mismatch");
    }

    [Test]
    public async Task RotationallyEquivalent_RendersDotAndThreshold(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion actual = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 4.0));
        Quaternion expected = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 4.0));
        string message = MathFailureMessage.RotationallyEquivalent(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("dot");
        await Assert.That(message).Contains("threshold");
        await Assert.That(message).Contains("normalized");
    }

    [Test]
    public async Task GeometricallyEquivalent_RendersBothCandidates(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Plane(new Vector3(0.5f, 0.5f, 0.0f), 1.0f);
        var expected = new Plane(new Vector3(1.0f, 0.0f, 0.0f), 2.0f);
        string message = MathFailureMessage.GeometricallyEquivalent(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("delta vs expected:");
        await Assert.That(message).Contains("delta vs sign-flipped:");
    }

    [Test]
    public async Task AxisAngleApproximately_RendersExtractedAxisAndAngle(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion actual = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 2.0));
        string message = MathFailureMessage.AxisAngleApproximately(
            actual,
            expectedAxis: Vector3.UnitX,
            expectedAngleDegrees: 90.0,
            extractedAxis: Vector3.UnitY,
            extractedAngleDegrees: 90.0,
            tolerance: 1e-6);

        await Assert.That(message).Contains("extracted axis:");
        await Assert.That(message).Contains("extracted angle:");
        await Assert.That(message).Contains("delta angle:");
    }

    [Test]
    public async Task InvariantCulture_RendersFloatsWithDotDecimalSeparator(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-NL"); // uses ',' as decimal separator
            var actual = new Vector3(1.5f, 2.0f, 3.0f);
            var expected = new Vector3(1.0f, 2.0f, 3.0f);
            string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 0.001);

            await Assert.That(message).Contains("0.5"); // dot not comma
            await Assert.That(message).DoesNotContain("0,5");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
