using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;
using MathAssertions.TUnit;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// End-to-end smoke tests for the 0.1.0 adapter-expansion surface. Each adapter method
/// gets a happy-path call through <c>Assert.That(value).Method(...)</c> to verify the
/// <c>[GenerateAssertion]</c> source generator wired the wrapper correctly. Underlying
/// behaviour is exhaustively covered by the core unit tests; these tests focus on the
/// fluent-call surface and the array-overload null guards (M-4).
/// </summary>
[Category("Smoke")]
[Timeout(10_000)]
internal sealed class AdapterExpansionTests
{
    private const double Tol = 1e-6;

    // Reusable arrays lifted to fields per CA1861 and to defeat the
    // TUnitAssertions0005 "Assert.That on constant" heuristic.
    private static readonly double[] StatsReference = [1.0, 2.0, 3.0, 4.0, 5.0];
    private static readonly double[] AscendingDoubles = [1.0, 2.0, 3.0];
    private static readonly double[] DescendingDoubles = [3.0, 2.0, 1.0];
    private static readonly double[] BoundedDoubles = [1.0, 5.0, 10.0];
    private static readonly double[] ArithmeticProgression = [1.0, 3.0, 5.0, 7.0];
    private static readonly double[] GeometricProgression = [1.0, 2.0, 4.0, 8.0];
    private static readonly double[] ConvergingDoubles = [10.0, 5.0, 2.5, 1.001];
    private static readonly double[] CauchyConvergingDoubles = [10.0, 5.0, 2.0, 1.0001, 1.0];
    private static readonly double[] DoubleArrayHappy = [1.0, 2.0, 3.0];
    private static readonly double[] DoubleArrayHappyShifted = [1.0001, 2.0001, 3.0001];
    private static readonly float[] FloatArrayHappy = [1.0f, 2.0f, 3.0f];
    private static readonly float[] FloatArrayHappyShifted = [1.0001f, 2.0001f, 3.0001f];
    private static readonly int[] IntArrayHappy = [1, 2, 3];
    private static readonly int[] IntArrayLonger = [1, 2, 3, 4];
    private static readonly double[] UnsortedDoubles = [1.0, 3.0, 2.0];
    private static readonly double[] OneDouble = [1.0];
    private static readonly float[] OneFloat = [1.0f];

    private static readonly Vector3[] CollinearPoints =
    [
        Vector3.Zero,
        Vector3.UnitX,
        new(2, 0, 0),
    ];

    private static readonly Vector3[] CoplanarPoints =
    [
        Vector3.Zero,
        Vector3.UnitX,
        Vector3.UnitY,
        new(1, 1, 0),
    ];

    private static readonly Vector3[] StandardBasisR3 =
    [
        Vector3.UnitX,
        Vector3.UnitY,
        Vector3.UnitZ,
    ];

    private static readonly Vector3[] UnitTetrahedronHull =
    [
        Vector3.Zero, Vector3.UnitY, Vector3.UnitX,
        Vector3.Zero, Vector3.UnitX, Vector3.UnitZ,
        Vector3.Zero, Vector3.UnitZ, Vector3.UnitY,
        Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ,
    ];

    private static readonly Vector3[] BoundedCloud =
    [
        new(0.5f, 0.5f, 0.5f),
        new(0.1f, 0.9f, 0.5f),
    ];

    private static readonly Vector3[] InsideUnitSphereCloud =
    [
        new(0.3f, 0.3f, 0.3f),
        Vector3.Zero,
    ];

    private static readonly Vector3[] CentroidAtOriginCloud =
    [
        new(-1, -1, 0),
        new(1, 1, 0),
    ];

    private static readonly Vector3[] OnXZPlaneCloud =
    [
        Vector3.Zero,
        new(1, 0, 0),
    ];

    private static readonly Vector3[] OnUnitSphereCloud =
    [
        Vector3.UnitX,
        -Vector3.UnitX,
        Vector3.UnitY,
    ];

    // Helpers to defeat the TUnitAssertions0005 "Assert.That on constant" check by
    // hiding the value behind a non-inlineable method call.
    private static double D(double v) => v;
    private static float F(float v) => v;
    private static long L(long v) => v;

    // ----- ScalarAssertions -----

    [Test]
    public async Task Scalar_Double_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(1.0)).IsApproximatelyEqualTo(1.0001, 0.001);
    }

    [Test]
    public async Task Scalar_Float_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(F(1.0f)).IsApproximatelyEqualTo(1.0001f, 0.001f);
    }

    [Test]
    public async Task Scalar_Double_IsCloseInUlpsTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = D(1.0);
        var b = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a) + 1);
        await Assert.That(a).IsCloseInUlpsTo(b, 1L);
    }

    [Test]
    public async Task Scalar_Float_IsCloseInUlpsTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = F(1.0f);
        var b = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(a) + 1);
        await Assert.That(a).IsCloseInUlpsTo(b, 1);
    }

    [Test]
    public async Task Scalar_IsRelativelyAndAbsolutelyCloseTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(1e9)).IsRelativelyAndAbsolutelyCloseTo(1e9 + 1e6, 1e-3, 1e-12);
    }

    // No IsFinite test here: TUnit's built-in DoubleAssertionExtensions.IsFinite and
    // SingleAssertionExtensions.IsFinite already cover that path. Adding our own would
    // collide with the generated extension from TUnit. The fluent surface for
    // IsFinite() therefore comes from TUnit core, not from MathAssertions.TUnit.

    [Test]
    public async Task Scalar_IsNonNegativeFinite(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(0.5)).IsNonNegativeFinite();
    }

    [Test]
    public async Task Scalar_IsProbability(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(0.5)).IsProbability();
    }

    [Test]
    public async Task Scalar_IsPercentage(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(50.0)).IsPercentage();
    }

    [Test]
    public async Task Scalar_HasRoundtripIdentity(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(0.5)).HasRoundtripIdentity(Math.Asin, Math.Sin, 1e-12);
    }

    // ----- VectorAssertions -----

    [Test]
    public async Task Vector2_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector2(1, 2);
        var b = new Vector2(1.0001f, 2.0001f);
        await Assert.That(a).IsApproximatelyEqualTo(b, 0.001);
    }

    [Test]
    public async Task Vector4_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector4(1, 2, 3, 4);
        var b = new Vector4(1.0001f, 2.0001f, 3.0001f, 4.0001f);
        await Assert.That(a).IsApproximatelyEqualTo(b, 0.001);
    }

    [Test]
    public async Task Vector3_HasMagnitudeApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = new Vector3(3, 4, 0);
        await Assert.That(v).HasMagnitudeApproximately(5.0, Tol);
    }

    [Test]
    public async Task Vector3_IsNormalized(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = Vector3.UnitX;
        await Assert.That(v).IsNormalized(Tol);
    }

    // ----- QuaternionAssertions -----

    [Test]
    public async Task Quaternion_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        await Assert.That(a).IsApproximatelyEqualTo(a, Tol);
    }

    [Test]
    public async Task Quaternion_IsRotationallyEquivalentTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.Normalize(new Quaternion(0.1f, 0.2f, 0.3f, 0.9f));
        var negQ = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);
        await Assert.That(q).IsRotationallyEquivalentTo(negQ, Tol);
    }

    [Test]
    public async Task Quaternion_IsIdentity(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.Identity;
        await Assert.That(q).IsIdentity(Tol);
    }

    [Test]
    public async Task Quaternion_IsNormalized(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.Identity;
        await Assert.That(q).IsNormalized(Tol);
    }

    // ----- MatrixAssertions -----

    [Test]
    public async Task Matrix_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsApproximatelyEqualTo(Matrix4x4.Identity, Tol);
    }

    [Test]
    public async Task Matrix_IsSymmetric(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsSymmetric(Tol);
    }

    [Test]
    public async Task Matrix_IsOrthogonal(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsOrthogonal(Tol);
    }

    [Test]
    public async Task Matrix_IsIdentity(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsIdentity(Tol);
    }

    [Test]
    public async Task Matrix_HasDeterminantApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).HasDeterminantApproximately(1.0, Tol);
    }

    [Test]
    public async Task Matrix_HasTraceApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).HasTraceApproximately(4.0, Tol);
    }

    [Test]
    public async Task Matrix_IsInvertible(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsInvertible(Tol);
    }

    // ----- PlaneAssertions -----

    [Test]
    public async Task Plane_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var p = new Plane(0, 1, 0, -5);
        await Assert.That(p).IsApproximatelyEqualTo(p, Tol);
    }

    [Test]
    public async Task Plane_IsGeometricallyEquivalentTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var p = new Plane(0, 1, 0, -5);
        var flipped = new Plane(-p.Normal, -p.D);
        await Assert.That(p).IsGeometricallyEquivalentTo(flipped, Tol);
    }

    // ----- ComplexAssertions -----

    [Test]
    public async Task Complex_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Complex(1, 2);
        var b = new Complex(1.0001, 2.0001);
        await Assert.That(a).IsApproximatelyEqualTo(b, 0.001);
    }

    // ----- ArrayAssertions -----

    [Test]
    public async Task DoubleArray_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(DoubleArrayHappy).IsApproximatelyEqualTo(DoubleArrayHappyShifted, 0.001);
    }

    [Test]
    public async Task FloatArray_IsApproximatelyEqualTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(FloatArrayHappy).IsApproximatelyEqualTo(FloatArrayHappyShifted, 0.001f);
    }

    [Test]
    public async Task DoubleArray_IsApproximatelyEqualTo_NullThrows(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            double[] nullArray = null!;
            return nullArray.IsApproximatelyEqualTo(OneDouble, 0.001);
        }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task FloatArray_IsApproximatelyEqualTo_NullThrows(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            float[] nullArray = null!;
            return nullArray.IsApproximatelyEqualTo(OneFloat, 0.001f);
        }).Throws<ArgumentNullException>();
    }

    // ----- SequencesAssertions -----

    [Test]
    public async Task Sequence_IsMonotonicallyIncreasing(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(AscendingDoubles).IsMonotonicallyIncreasing();
    }

    [Test]
    public async Task Sequence_IsMonotonicallyDecreasing(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(DescendingDoubles).IsMonotonicallyDecreasing();
    }

    [Test]
    public async Task Sequence_IsStrictlyMonotonicallyIncreasing(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(AscendingDoubles).IsStrictlyMonotonicallyIncreasing();
    }

    [Test]
    public async Task Sequence_IsStrictlyMonotonicallyDecreasing(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(DescendingDoubles).IsStrictlyMonotonicallyDecreasing();
    }

    [Test]
    public async Task Sequence_IsSorted(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(AscendingDoubles).IsSorted();
    }

    [Test]
    public async Task Sequence_IsBounded(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(BoundedDoubles).IsBounded(0.0, 10.0);
    }

    [Test]
    public async Task Sequence_IsArithmeticProgression(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(ArithmeticProgression).IsArithmeticProgression(Tol);
    }

    [Test]
    public async Task Sequence_IsGeometricProgression(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(GeometricProgression).IsGeometricProgression(Tol);
    }

    [Test]
    public async Task Sequence_ConvergesTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(ConvergingDoubles).ConvergesTo(1.0, 0.01);
    }

    [Test]
    public async Task Sequence_IsCauchyConvergent(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(CauchyConvergingDoubles).IsCauchyConvergent(0.001);
    }

    [Test]
    public async Task Sequence_HasLength(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(IntArrayHappy).HasLength(3);
    }

    [Test]
    public async Task Sequence_HasMinLength(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(IntArrayLonger).HasMinLength(3);
    }

    [Test]
    public async Task Sequence_NullArray_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            double[] nullArray = null!;
            return nullArray.IsMonotonicallyIncreasing();
        }).Throws<ArgumentNullException>();
    }

    // ----- StatisticsAssertions -----

    [Test]
    public async Task Statistics_HasMeanApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasMeanApproximately(3.0, Tol);
    }

    [Test]
    public async Task Statistics_HasVarianceApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasVarianceApproximately(2.5, Tol);
    }

    [Test]
    public async Task Statistics_HasStdDevApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasStdDevApproximately(Math.Sqrt(2.5), Tol);
    }

    [Test]
    public async Task Statistics_HasSumApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasSumApproximately(15.0, Tol);
    }

    [Test]
    public async Task Statistics_HasMedianApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasMedianApproximately(3.0, Tol);
    }

    [Test]
    public async Task Statistics_HasPercentileApproximately(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasPercentileApproximately(50.0, 3.0, Tol);
    }

    [Test]
    public async Task Statistics_IsWithinSigmasOfMean(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(3.0)).IsWithinSigmasOfMean(StatsReference, 0.0);
    }

    [Test]
    public async Task Statistics_AreAllWithinSigmasOfMean(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).AreAllWithinSigmasOfMean(3.0);
    }

    [Test]
    public async Task Statistics_NullArray_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            double[] nullArray = null!;
            return nullArray.HasMeanApproximately(0.0, Tol);
        }).Throws<ArgumentNullException>();
    }

    // ----- LinearAlgebraAssertions -----

    [Test]
    public async Task LinearAlgebra_Vector3_IsOrthogonalTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = Vector3.UnitX;
        await Assert.That(v).IsOrthogonalTo(Vector3.UnitY, Tol);
    }

    [Test]
    public async Task LinearAlgebra_Vector3_IsParallelTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = new Vector3(1, 0, 0);
        var other = new Vector3(2, 0, 0);
        await Assert.That(v).IsParallelTo(other, Tol);
    }

    [Test]
    public async Task LinearAlgebra_Vector3Array_AreLinearlyIndependent(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StandardBasisR3).AreLinearlyIndependent(Tol);
    }

    [Test]
    public async Task LinearAlgebra_NullArray_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            Vector3[] nullArray = null!;
            return nullArray.AreLinearlyIndependent(Tol);
        }).Throws<ArgumentNullException>();
    }

    // ----- NumberTheoryAssertions -----

    [Test]
    public async Task NumberTheory_IsDivisibleBy(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(6L)).IsDivisibleBy(2L);
    }

    [Test]
    public async Task NumberTheory_IsPrime(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(17L)).IsPrime();
    }

    [Test]
    public async Task NumberTheory_IsCoprimeWith(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(15L)).IsCoprimeWith(28L);
    }

    [Test]
    public async Task NumberTheory_IsPowerOf(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(8L)).IsPowerOf(2L);
    }

    [Test]
    public async Task NumberTheory_IsPerfectSquare(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(25L)).IsPerfectSquare();
    }

    [Test]
    public async Task NumberTheory_IsCongruentTo(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(10L)).IsCongruentTo(4L, 3L);
    }

    // ----- Geometry3DAssertions -----

    [Test]
    public async Task Geometry3D_Triangle_IsDegenerate(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, Vector3.UnitX, new Vector3(2, 0, 0));
        await Assert.That(t).IsDegenerate(Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3Array_IsCollinear(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(CollinearPoints).IsCollinear(Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3Array_AreCoplanar(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(CoplanarPoints).AreCoplanar(Tol);
    }

    [Test]
    public async Task Geometry3D_AABB_ContainsPoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        var point = new Vector3(0.5f, 0.5f, 0.5f);
        await Assert.That(box).ContainsPoint(point);
    }

    [Test]
    public async Task Geometry3D_Sphere_ContainsPoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 1f);
        var point = new Vector3(0.3f, 0.3f, 0.3f);
        await Assert.That(sphere).ContainsPoint(point);
    }

    [Test]
    public async Task Geometry3D_OrientedBox_ContainsPoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var obb = new OrientedBox(Vector3.Zero, Vector3.One, Quaternion.Identity);
        var point = new Vector3(0.5f, 0.5f, 0.5f);
        await Assert.That(obb).ContainsPoint(point);
    }

    [Test]
    public async Task Geometry3D_AABB_ContainsBox(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var outer = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        var inner = new AxisAlignedBox(new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.75f, 0.75f, 0.75f));
        await Assert.That(outer).ContainsBox(inner);
    }

    [Test]
    public async Task Geometry3D_AABB_ContainsSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(new Vector3(-2, -2, -2), new Vector3(2, 2, 2));
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(box).ContainsSphere(sphere);
    }

    [Test]
    public async Task Geometry3D_Vector3Array_ConvexHullContainsPoint(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var point = new Vector3(0.1f, 0.1f, 0.1f);
        await Assert.That(UnitTetrahedronHull).ConvexHullContainsPoint(point, Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3_HasDistanceFromPlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = new Vector3(2, 5, -1);
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(v).HasDistanceFromPlane(plane, 5.0, Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3_HasDistanceFromSegment(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = new Vector3(1, 3, 0);
        var seg = new LineSegment3D(Vector3.Zero, new Vector3(2, 0, 0));
        await Assert.That(v).HasDistanceFromSegment(seg, 3.0, Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3_HasDistanceFromTriangle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = new Vector3(1, 1, 5);
        var t = new Triangle3D(Vector3.Zero, new Vector3(3, 0, 0), new Vector3(0, 3, 0));
        await Assert.That(v).HasDistanceFromTriangle(t, 5.0, Tol);
    }

    [Test]
    public async Task Geometry3D_Sphere_IntersectsSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Sphere(Vector3.Zero, 1f);
        var b = new Sphere(new Vector3(1.5f, 0, 0), 1f);
        await Assert.That(a).IntersectsSphere(b);
    }

    [Test]
    public async Task Geometry3D_AABB_IntersectsBox(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        var b = new AxisAlignedBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2, 2, 2));
        await Assert.That(a).IntersectsBox(b);
    }

    [Test]
    public async Task Geometry3D_Ray_IntersectsPlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(0, 2, 0), -Vector3.UnitY);
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(ray).IntersectsPlane(plane);
    }

    [Test]
    public async Task Geometry3D_Ray_IntersectsSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new Ray3D(new Vector3(-3, 0, 0), Vector3.UnitX);
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(ray).IntersectsSphere(sphere);
    }

    [Test]
    public async Task Geometry3D_Ray_IntersectsTriangle(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var t = new Triangle3D(Vector3.Zero, new Vector3(2, 0, 0), new Vector3(0, 2, 0));
        var ray = new Ray3D(new Vector3(0.5f, 0.5f, 5), -Vector3.UnitZ);
        await Assert.That(ray).IntersectsTriangle(t);
    }

    [Test]
    public async Task Geometry3D_Ray_IntersectsBox(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        var ray = new Ray3D(new Vector3(-2, 0.5f, 0.5f), Vector3.UnitX);
        await Assert.That(ray).IntersectsBox(box);
    }

    [Test]
    public async Task Geometry3D_Pointcloud_IsBoundedByBox(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var box = new AxisAlignedBox(Vector3.Zero, Vector3.One);
        await Assert.That(BoundedCloud).IsBoundedByBox(box);
    }

    [Test]
    public async Task Geometry3D_Pointcloud_IsBoundedBySphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new Sphere(Vector3.Zero, 1f);
        await Assert.That(InsideUnitSphereCloud).IsBoundedBySphere(sphere);
    }

    [Test]
    public async Task Geometry3D_Pointcloud_HasCentroidAt(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(CentroidAtOriginCloud).HasCentroidAt(Vector3.Zero, Tol);
    }

    [Test]
    public async Task Geometry3D_Pointcloud_IsApproximatelyOnPlane(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var plane = new Plane(Vector3.UnitY, 0f);
        await Assert.That(OnXZPlaneCloud).IsApproximatelyOnPlane(plane, Tol);
    }

    [Test]
    public async Task Geometry3D_Pointcloud_IsApproximatelyOnSphere(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(OnUnitSphereCloud).IsApproximatelyOnSphere(Vector3.Zero, 1f, Tol);
    }

    [Test]
    public async Task Geometry3D_Vector3Array_NullThrows(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            Vector3[] nullArray = null!;
            return nullArray.IsCollinear(Tol);
        }).Throws<ArgumentNullException>();
    }

    // ----- Negative path: assertion fails when expected -----

    [Test]
    public async Task Sequence_IsMonotonicallyIncreasing_FailsForUnsorted(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(async () =>
        {
            await Assert.That(UnsortedDoubles).IsMonotonicallyIncreasing();
        }).Throws<AssertionException>();
    }

    [Test]
    public async Task NumberTheory_IsPrime_FailsForComposite(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(async () =>
        {
            await Assert.That(L(9L)).IsPrime();
        }).Throws<AssertionException>();
    }
}
