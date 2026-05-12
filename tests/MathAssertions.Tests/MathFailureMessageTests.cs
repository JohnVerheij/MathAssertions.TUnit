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

    /// <summary>Pins the modulo-360 shortest-arc rendering of the delta angle: 350 vs 10
    /// degrees renders as 20 (the geometric distance in SO(3)), not as 340 (linear
    /// arithmetic). The full-message-text check (rather than just <c>Contains("20")</c>)
    /// guards against an accidental "340" or "-340" leak from a regression to the
    /// linear-arithmetic form.</summary>
    [Test]
    public async Task AxisAngleApproximately_DeltaAngleWrapsModulo360_RendersShortestArc(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Quaternion actual = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 18.0)); // ~10 degrees
        string message = MathFailureMessage.AxisAngleApproximately(
            actual,
            expectedAxis: Vector3.UnitY,
            expectedAngleDegrees: 350.0,
            extractedAxis: Vector3.UnitY,
            extractedAngleDegrees: 10.0,
            tolerance: 1e-6);

        await Assert.That(message).Contains("delta angle:        20 degrees");
        await Assert.That(message).DoesNotContain("340");
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

    [Test]
    public async Task Vector2_SingleAxisExceeded_NamesAxisAndDelta(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector2(1.0f, 2.5f);
        var expected = new Vector2(1.0f, 2.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("component-wise");
        await Assert.That(message).Contains("delta:");
        await Assert.That(message).Contains("Y (");
        await Assert.That(message).DoesNotContain("X (");
    }

    [Test]
    public async Task Vector4_MultiAxisExceeded_NamesAllAxes(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector4(1.5f, 2.5f, 3.5f, 4.5f);
        var expected = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("X (");
        await Assert.That(message).Contains("Y (");
        await Assert.That(message).Contains("Z (");
        await Assert.That(message).Contains("W (");
    }

    [Test]
    public async Task Complex_RealAndImaginaryDeltasShown(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new System.Numerics.Complex(1.5, 2.5);
        var expected = new System.Numerics.Complex(1.0, 2.0);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("Real=");
        await Assert.That(message).Contains("Imaginary=");
        await Assert.That(message).Contains("Real (");
        await Assert.That(message).Contains("Imaginary (");
    }

    [Test]
    public async Task FloatArray_FirstFailingIndexNamed(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        float[] actual = [1.0f, 2.0f, 3.0f, 4.5f, 5.0f];
        float[] expected = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6f);

        await Assert.That(message).Contains("first mismatch at index 3");
    }

    [Test]
    public async Task FloatArray_LengthMismatch_FlaggedExplicitly(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        float[] actual = [1.0f, 2.0f, 3.0f];
        float[] expected = [1.0f, 2.0f];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6f);

        await Assert.That(message).Contains("length mismatch");
    }

    /// <summary>Pins the equal-special-value handling: a NaN component pair (NaN vs NaN) is
    /// equal under <see cref="MathTolerance.IsApproximatelyEqual(double, double, double)"/>
    /// and must therefore NOT appear in the rendered <c>exceeded:</c> list. The prior
    /// implementation used a delta-only inference (<c>delta &gt; tolerance || double.IsNaN(delta)</c>)
    /// that incorrectly flagged this case because <c>NaN - NaN</c> is <c>NaN</c>.</summary>
    [Test]
    public async Task Vector3_NaNVsNaN_NotFlaggedInExceeded_OnlyOtherAxis(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(float.NaN, 1.0f, 3.5f);
        var expected = new Vector3(float.NaN, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        // X is NaN/NaN -> equal under the predicate -> not in exceeded list.
        // Y and Z are finite mismatches -> in exceeded list.
        await Assert.That(message).Contains("Y (");
        await Assert.That(message).Contains("Z (");
        await Assert.That(message).DoesNotContain("X (");
    }

    /// <summary>Pins the equal-special-value handling: same-sign-infinity pair (+inf vs +inf)
    /// is equal under the predicate and must NOT appear in the rendered <c>exceeded:</c> list.
    /// The subtraction <c>+inf - +inf</c> is <c>NaN</c>, which the prior delta-only inference
    /// would have flagged.</summary>
    [Test]
    public async Task Vector3_SameSignInfinityPair_NotFlaggedInExceeded(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(float.PositiveInfinity, 1.0f, 3.0f);
        var expected = new Vector3(float.PositiveInfinity, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("Y (");
        await Assert.That(message).DoesNotContain("X (");
    }

    /// <summary>Pins the equal-special-value handling at the matrix-cell level. A NaN/NaN
    /// cell pair must NOT appear in the rendered <c>exceeded:</c> list. The matrix branch
    /// has its own inline classification; this test guards against the two branches drifting
    /// from <see cref="AppendExceeded"/> semantics.</summary>
    [Test]
    public async Task Matrix4x4_NaNCellPair_NotFlaggedAsExceeded(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Matrix4x4 actual = Matrix4x4.Identity;
        Matrix4x4 expected = Matrix4x4.Identity;
        actual.M23 = float.NaN;
        expected.M23 = float.NaN;
        actual.M12 = 0.5f; // a real off-diagonal mismatch so the renderer still produces an exceeded entry
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("[0,1]");
        await Assert.That(message).DoesNotContain("[1,2]");
    }

    [Test]
    public async Task Matrix4x4_AllCellsEqual_RendersFallbackNoteOnExceeded(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Matrix4x4 actual = Matrix4x4.Identity;
        Matrix4x4 expected = Matrix4x4.Identity;
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("(none on individual cell;");
    }

    [Test]
    public async Task Vector3_AllAxesEqual_RendersFallbackNoteOnExceeded(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actual = new Vector3(1.0f, 2.0f, 3.0f);
        var expected = new Vector3(1.0f, 2.0f, 3.0f);
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("(none on individual component;");
    }

    [Test]
    public async Task DoubleArray_AllElementsEqual_RendersTraceForNanInfinityCheck(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        double[] actual = [1.0, 2.0, 3.0];
        double[] expected = [1.0, 2.0, 3.0];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6);

        await Assert.That(message).Contains("no element exceeded tolerance");
    }

    [Test]
    public async Task FloatArray_AllElementsEqual_RendersTraceForNanInfinityCheck(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        float[] actual = [1.0f, 2.0f, 3.0f];
        float[] expected = [1.0f, 2.0f, 3.0f];
        string message = MathFailureMessage.ApproximatelyEqual(actual, expected, tolerance: 1e-6f);

        await Assert.That(message).Contains("no element exceeded tolerance");
    }
}
