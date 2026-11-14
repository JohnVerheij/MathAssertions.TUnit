using System;
using System.Numerics;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>The table of <see cref="AdapterCase"/> rows and the sample values the rows share.</summary>
internal static class AdapterCases
{
    // Samples are static fields because Assert.That(...) refuses a constant subject.
    private static double D1 => 1.0;
    private static double D2 => 2.0;
    private static double Tol => 0.1;
    private static float F1 => 1f;
    private static float F2 => 2f;
    private static float FTol => 0.1f;
    private static readonly Vector2 V2A = new(1f, 2f);
    private static readonly Vector2 V2B = new(1f, 3f);
    private static readonly Vector3 V3A = new(1f, 2f, 3f);
    private static readonly Vector3 V3B = new(1f, 2f, 4f);
    private static readonly Vector3 Origin = Vector3.Zero;
    private static readonly Vector3 Above = new(0f, 0f, 5f);
    private static readonly Vector3 Aside = new(0f, 5f, 0f);
    private static readonly Vector3 Far = new(5f, 0f, 0f);
    private static readonly Vector4 V4A = new(1f, 2f, 3f, 4f);
    private static readonly Vector4 V4B = new(1f, 2f, 3f, 5f);
    private static readonly Quaternion Identity = Quaternion.Identity;
    private static readonly Quaternion Turned = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f);
    private static readonly Quaternion NotUnit = new(2f, 0f, 0f, 0f);
    private static readonly Matrix4x4 MIdentity = Matrix4x4.Identity;
    private static readonly Matrix4x4 MScaled = Matrix4x4.CreateScale(2f);
    private static readonly Matrix4x4 MTranslated = Matrix4x4.CreateTranslation(1f, 0f, 0f);
    private static readonly Matrix4x4 MAsymmetric = Matrix4x4.CreateTranslation(1f, 2f, 3f);
    private static readonly Matrix4x4 MZero = Matrix4x4.Multiply(Matrix4x4.Identity, 0f);
    private static readonly Complex C1 = new(1.0, 2.0);
    private static readonly Complex C2 = new(1.0, 3.0);
    private static readonly Plane FloorPlane = new(Vector3.UnitZ, 0f);
    private static readonly Plane RaisedPlane = new(Vector3.UnitZ, 1f);
    private static readonly Triangle3D Flat = new(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);
    private static readonly LineSegment3D Segment = new(Vector3.Zero, Vector3.UnitX);
    private static readonly AxisAlignedBox UnitBox = new(Vector3.Zero, Vector3.One);
    private static readonly AxisAlignedBox FarBox = new(new Vector3(8f, 8f, 8f), new Vector3(9f, 9f, 9f));
    private static readonly AxisAlignedBox BigBox = new(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2f, 2f, 2f));
    private static readonly Sphere UnitSphere = new(Vector3.Zero, 1f);
    private static readonly Sphere FarSphere = new(new Vector3(9f, 0f, 0f), 1f);
    private static readonly Sphere MiddleSphere = new(new Vector3(0.5f, 0.5f, 0.5f), 1f);
    private static readonly OrientedBox Oriented = new(Vector3.Zero, Vector3.One, Quaternion.Identity);
    private static readonly Ray3D MissingRay = new(new Vector3(0f, 5f, 0f), Vector3.UnitX);
    private static readonly Ray3D HittingRay = new(new Vector3(-5f, 0f, 0f), Vector3.UnitX);
    private static readonly Ray3D FlatRay = new(new Vector3(0f, 0f, 1f), Vector3.UnitX);
    private static readonly Vector3[] LShape = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];
    private static readonly Vector3[] Line = [Vector3.Zero, Vector3.UnitX, new(2f, 0f, 0f)];
    private static readonly Vector3[] Tetrahedron = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];
    private static readonly Vector3[] Flat4 = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY, new(1f, 1f, 0f)];
    private static readonly Vector3[] Dependent = [Vector3.UnitX, new(2f, 0f, 0f)];
    private static readonly Vector3[] Independent = [Vector3.UnitX, Vector3.UnitY];
    private static readonly Vector3[] CloudOnPlane = [Vector3.Zero, Vector3.UnitX];
    private static readonly Vector3[] CloudOffPlane = [Vector3.Zero, Above];
    private static readonly Vector3[] CloudOnSphere = [Vector3.UnitX, -Vector3.UnitX];
    private static readonly Vector3[] CloudOffSphere = [Vector3.UnitX, new(3f, 0f, 0f)];
    private static readonly Vector3[] InsideUnitBox = [new(0.5f, 0.5f, 0.5f)];
    private static readonly Vector3[] OutsideUnitBox = [new(0.5f, 0.5f, 0.5f), new(2f, 0f, 0f)];
    private static readonly Vector3[] InsideUnitSphere = [new(0.5f, 0f, 0f)];
    private static readonly Vector3[] OutsideUnitSphere = [new(0.5f, 0f, 0f), new(3f, 0f, 0f)];
    private static readonly double[] Ascending = [1.0, 2.0, 3.0];
    private static readonly double[] Descending = [3.0, 2.0, 1.0];
    private static readonly double[] Flat3 = [2.0, 2.0, 2.0];
    private static readonly double[] Arithmetic = [1.0, 2.0, 3.0];
    private static readonly double[] NotArithmetic = [1.0, 2.0, 4.0];
    private static readonly double[] Geometric = [1.0, 2.0, 4.0];
    private static readonly double[] NotGeometric = [1.0, 2.0, 5.0];
    private static readonly double[] Settling = [5.0, 1.001, 1.0001];
    private static readonly double[] Diverging = [1.0, 2.0, 9.0];
    private static readonly double[] SingleValue = [4.0];
    private static readonly double[] Sample = [1.0, 2.0, 3.0];
    private static readonly double[] Outlier = [1.0, 2.0, 100.0];
    private static readonly float[] FloatsA = [1f, 2f];
    private static readonly float[] FloatsB = [1f, 3f];
    private static readonly double[] DoublesA = [1.0, 2.0];
    private static readonly double[] DoublesB = [1.0, 3.0];
    private static long Seven => 7;
    private static long Nine => 9;
    private static long Six => 6;
    private static long Eight => 8;
    private static long Two => 2;
    private static long Three => 3;
    private static long Five => 5;
    private static readonly (Vector3 Position, Quaternion Orientation) PoseHome = (Vector3.Zero, Quaternion.Identity);
    private static readonly (Vector3 Position, Quaternion Orientation) PoseMoved = (Vector3.UnitX, Quaternion.Identity);

    /// <summary>Every row of the table.</summary>
    public static readonly AdapterCase[] All =
    [
        .. Rows1(),
        .. Rows2(),
        .. Rows3(),
        .. Rows4(),
        .. Rows5(),
        .. Rows6(),
        .. Rows7(),
        .. Rows8(),
        .. Rows9(),
        .. Rows10(),
        .. Rows11(),
        .. Rows12(),
    ];

    /// <summary>Finds a row by its name.</summary>
    /// <param name="name">The unique name of the overload.</param>
    /// <returns>The row.</returns>
    public static AdapterCase Find(string name)
        => Array.Find(All, row => string.Equals(row.Name, name, StringComparison.Ordinal))
            ?? throw new ArgumentException("No such case: " + name, nameof(name));

    private static AdapterCase[] Rows1() =>
    [
        new("Array.IsApproximatelyEqualTo(double[])",
            async () => await Assert.That(DoublesA).IsApproximatelyEqualTo(DoublesB, Tol),
            () => ArrayAssertions.IsApproximatelyEqualTo(DoublesA, DoublesA, Tol).IsPassed,
            () => !ArrayAssertions.IsApproximatelyEqualTo(DoublesA, DoublesB, Tol).IsPassed),
        new("Array.IsApproximatelyEqualTo(float[])",
            async () => await Assert.That(FloatsA).IsApproximatelyEqualTo(FloatsB, FTol),
            () => ArrayAssertions.IsApproximatelyEqualTo(FloatsA, FloatsA, FTol).IsPassed,
            () => !ArrayAssertions.IsApproximatelyEqualTo(FloatsA, FloatsB, FTol).IsPassed),
        new("Complex.IsApproximatelyEqualTo",
            async () => await Assert.That(C1).IsApproximatelyEqualTo(C2, Tol),
            () => ComplexAssertions.IsApproximatelyEqualTo(C1, C1, Tol).IsPassed,
            () => !ComplexAssertions.IsApproximatelyEqualTo(C1, C2, Tol).IsPassed),
        new("Geometry.IsDegenerate",
            async () => await Assert.That(Flat).IsDegenerate(Tol),
            () => Geometry3DAssertions.IsDegenerate(new Triangle3D(Vector3.Zero, Vector3.UnitX, new Vector3(2f, 0f, 0f)), Tol),
            () => !Geometry3DAssertions.IsDegenerate(Flat, Tol)),
        new("Geometry.IsCollinear",
            async () => await Assert.That(LShape).IsCollinear(Tol),
            () => Geometry3DAssertions.IsCollinear(Line, Tol),
            () => !Geometry3DAssertions.IsCollinear(LShape, Tol)),
        new("Geometry.AreCoplanar",
            async () => await Assert.That(Tetrahedron).AreCoplanar(Tol),
            () => Geometry3DAssertions.AreCoplanar(Flat4, Tol),
            () => !Geometry3DAssertions.AreCoplanar(Tetrahedron, Tol)),
        new("Geometry.ContainsPoint(AxisAlignedBox)",
            async () => await Assert.That(UnitBox).ContainsPoint(Far),
            () => Geometry3DAssertions.ContainsPoint(UnitBox, new Vector3(0.5f, 0.5f, 0.5f)),
            () => !Geometry3DAssertions.ContainsPoint(UnitBox, Far)),
        new("Geometry.ContainsPoint(Sphere)",
            async () => await Assert.That(UnitSphere).ContainsPoint(Far),
            () => Geometry3DAssertions.ContainsPoint(UnitSphere, Origin),
            () => !Geometry3DAssertions.ContainsPoint(UnitSphere, Far)),
    ];

    private static AdapterCase[] Rows2() =>
    [
        new("Geometry.ContainsPoint(OrientedBox)",
            async () => await Assert.That(Oriented).ContainsPoint(Far),
            () => Geometry3DAssertions.ContainsPoint(Oriented, Origin),
            () => !Geometry3DAssertions.ContainsPoint(Oriented, Far)),
        new("Geometry.ContainsBox",
            async () => await Assert.That(UnitBox).ContainsBox(BigBox),
            () => Geometry3DAssertions.ContainsBox(UnitBox, UnitBox),
            () => !Geometry3DAssertions.ContainsBox(UnitBox, BigBox)),
        new("Geometry.ContainsSphere",
            async () => await Assert.That(UnitBox).ContainsSphere(MiddleSphere),
            () => Geometry3DAssertions.ContainsSphere(BigBox, new Sphere(new Vector3(1f, 1f, 1f), 0.25f)),
            () => !Geometry3DAssertions.ContainsSphere(UnitBox, MiddleSphere)),
        new("Geometry.ConvexHullContainsPoint",
            async () => await Assert.That(LShape).ConvexHullContainsPoint(Above, Tol),
            () => Geometry3DAssertions.ConvexHullContainsPoint(LShape, new Vector3(0.2f, 0.2f, -1f), Tol),
            () => !Geometry3DAssertions.ConvexHullContainsPoint(LShape, Above, Tol)),
        new("Geometry.HasDistanceFromPlane",
            async () => await Assert.That(Above).HasDistanceFromPlane(FloorPlane, D1, Tol),
            () => Geometry3DAssertions.HasDistanceFromPlane(Above, FloorPlane, 5.0, Tol),
            () => !Geometry3DAssertions.HasDistanceFromPlane(Above, FloorPlane, D1, Tol)),
        new("Geometry.HasDistanceFromSegment",
            async () => await Assert.That(Aside).HasDistanceFromSegment(Segment, D1, Tol),
            () => Geometry3DAssertions.HasDistanceFromSegment(Aside, Segment, 5.0, Tol),
            () => !Geometry3DAssertions.HasDistanceFromSegment(Aside, Segment, D1, Tol)),
        new("Geometry.HasDistanceFromTriangle",
            async () => await Assert.That(Above).HasDistanceFromTriangle(Flat, D1, Tol),
            () => Geometry3DAssertions.HasDistanceFromTriangle(Above, Flat, 5.0, Tol),
            () => !Geometry3DAssertions.HasDistanceFromTriangle(Above, Flat, D1, Tol)),
        new("Geometry.IntersectsSphere(Sphere)",
            async () => await Assert.That(UnitSphere).IntersectsSphere(FarSphere),
            () => Geometry3DAssertions.IntersectsSphere(UnitSphere, UnitSphere),
            () => !Geometry3DAssertions.IntersectsSphere(UnitSphere, FarSphere)),
    ];

    private static AdapterCase[] Rows3() =>
    [
        new("Geometry.IntersectsSphere(Ray3D)",
            async () => await Assert.That(MissingRay).IntersectsSphere(UnitSphere),
            () => Geometry3DAssertions.IntersectsSphere(HittingRay, UnitSphere),
            () => !Geometry3DAssertions.IntersectsSphere(MissingRay, UnitSphere)),
        new("Geometry.IntersectsBox(AxisAlignedBox)",
            async () => await Assert.That(UnitBox).IntersectsBox(FarBox),
            () => Geometry3DAssertions.IntersectsBox(UnitBox, BigBox),
            () => !Geometry3DAssertions.IntersectsBox(UnitBox, FarBox)),
        new("Geometry.IntersectsBox(Ray3D)",
            async () => await Assert.That(MissingRay).IntersectsBox(UnitBox),
            () => Geometry3DAssertions.IntersectsBox(HittingRay, UnitBox),
            () => !Geometry3DAssertions.IntersectsBox(MissingRay, UnitBox)),
        new("Geometry.IntersectsPlane",
            async () => await Assert.That(FlatRay).IntersectsPlane(FloorPlane),
            () => Geometry3DAssertions.IntersectsPlane(new Ray3D(Above, -Vector3.UnitZ), FloorPlane),
            () => !Geometry3DAssertions.IntersectsPlane(FlatRay, FloorPlane)),
        new("Geometry.IntersectsTriangle",
            async () => await Assert.That(MissingRay).IntersectsTriangle(Flat),
            () => Geometry3DAssertions.IntersectsTriangle(new Ray3D(new Vector3(0.2f, 0.2f, 1f), -Vector3.UnitZ), Flat),
            () => !Geometry3DAssertions.IntersectsTriangle(MissingRay, Flat)),
        new("Geometry.IsBoundedByBox",
            async () => await Assert.That(OutsideUnitBox).IsBoundedByBox(UnitBox),
            () => Geometry3DAssertions.IsBoundedByBox(InsideUnitBox, UnitBox),
            () => !Geometry3DAssertions.IsBoundedByBox(OutsideUnitBox, UnitBox)),
        new("Geometry.IsBoundedBySphere",
            async () => await Assert.That(OutsideUnitSphere).IsBoundedBySphere(UnitSphere),
            () => Geometry3DAssertions.IsBoundedBySphere(InsideUnitSphere, UnitSphere),
            () => !Geometry3DAssertions.IsBoundedBySphere(OutsideUnitSphere, UnitSphere)),
        new("Geometry.HasCentroidAt",
            async () => await Assert.That(CloudOnPlane).HasCentroidAt(Far, Tol),
            () => Geometry3DAssertions.HasCentroidAt(CloudOnPlane, new Vector3(0.5f, 0f, 0f), Tol),
            () => !Geometry3DAssertions.HasCentroidAt(CloudOnPlane, Far, Tol)),
    ];

    private static AdapterCase[] Rows4() =>
    [
        new("Geometry.IsApproximatelyOnPlane",
            async () => await Assert.That(CloudOffPlane).IsApproximatelyOnPlane(FloorPlane, Tol),
            () => Geometry3DAssertions.IsApproximatelyOnPlane(CloudOnPlane, FloorPlane, Tol),
            () => !Geometry3DAssertions.IsApproximatelyOnPlane(CloudOffPlane, FloorPlane, Tol)),
        new("Geometry.IsApproximatelyOnSphere",
            async () => await Assert.That(CloudOffSphere).IsApproximatelyOnSphere(Origin, F1, Tol),
            () => Geometry3DAssertions.IsApproximatelyOnSphere(CloudOnSphere, Origin, F1, Tol),
            () => !Geometry3DAssertions.IsApproximatelyOnSphere(CloudOffSphere, Origin, F1, Tol)),
        new("LinearAlgebra.IsOrthogonalTo",
            async () => await Assert.That(Vector3.UnitX).IsOrthogonalTo(new Vector3(1f, 1f, 0f), Tol),
            () => LinearAlgebraAssertions.IsOrthogonalTo(Vector3.UnitX, Vector3.UnitY, Tol),
            () => !LinearAlgebraAssertions.IsOrthogonalTo(Vector3.UnitX, new Vector3(1f, 1f, 0f), Tol)),
        new("LinearAlgebra.IsParallelTo",
            async () => await Assert.That(Vector3.UnitX).IsParallelTo(Vector3.UnitY, Tol),
            () => LinearAlgebraAssertions.IsParallelTo(Vector3.UnitX, new Vector3(2f, 0f, 0f), Tol),
            () => !LinearAlgebraAssertions.IsParallelTo(Vector3.UnitX, Vector3.UnitY, Tol)),
        new("LinearAlgebra.HasAngleBetweenApproximately",
            async () => await Assert.That(Vector3.UnitX).HasAngleBetweenApproximately(Vector3.UnitY, D1, Tol),
            () => LinearAlgebraAssertions.HasAngleBetweenApproximately(Vector3.UnitX, Vector3.UnitY, Math.PI / 2.0, Tol),
            () => !LinearAlgebraAssertions.HasAngleBetweenApproximately(Vector3.UnitX, Vector3.UnitY, D1, Tol)),
        new("LinearAlgebra.AreLinearlyIndependent",
            async () => await Assert.That(Dependent).AreLinearlyIndependent(Tol),
            () => LinearAlgebraAssertions.AreLinearlyIndependent(Independent, Tol),
            () => !LinearAlgebraAssertions.AreLinearlyIndependent(Dependent, Tol)),
        new("Matrix.IsApproximatelyEqualTo",
            async () => await Assert.That(MIdentity).IsApproximatelyEqualTo(MTranslated, Tol),
            () => MatrixAssertions.IsApproximatelyEqualTo(MIdentity, MIdentity, Tol).IsPassed,
            () => !MatrixAssertions.IsApproximatelyEqualTo(MIdentity, MTranslated, Tol).IsPassed),
        new("Matrix.IsSymmetric",
            async () => await Assert.That(MAsymmetric).IsSymmetric(Tol),
            () => MatrixAssertions.IsSymmetric(MIdentity, Tol),
            () => !MatrixAssertions.IsSymmetric(MAsymmetric, Tol)),
    ];

    private static AdapterCase[] Rows5() =>
    [
        new("Matrix.IsOrthogonal",
            async () => await Assert.That(MScaled).IsOrthogonal(Tol),
            () => MatrixAssertions.IsOrthogonal(MIdentity, Tol),
            () => !MatrixAssertions.IsOrthogonal(MScaled, Tol)),
        new("Matrix.IsRotation",
            async () => await Assert.That(MScaled).IsRotation(Tol),
            () => MatrixAssertions.IsRotation(MIdentity, Tol),
            () => !MatrixAssertions.IsRotation(MScaled, Tol)),
        new("Matrix.IsIdentity",
            async () => await Assert.That(MScaled).IsIdentity(Tol),
            () => MatrixAssertions.IsIdentity(MIdentity, Tol),
            () => !MatrixAssertions.IsIdentity(MScaled, Tol)),
        new("Matrix.HasDeterminantApproximately",
            async () => await Assert.That(MIdentity).HasDeterminantApproximately(D2, Tol),
            () => MatrixAssertions.HasDeterminantApproximately(MIdentity, D1, Tol),
            () => !MatrixAssertions.HasDeterminantApproximately(MIdentity, D2, Tol)),
        new("Matrix.HasTraceApproximately",
            async () => await Assert.That(MIdentity).HasTraceApproximately(D2, Tol),
            () => MatrixAssertions.HasTraceApproximately(MIdentity, 4.0, Tol),
            () => !MatrixAssertions.HasTraceApproximately(MIdentity, D2, Tol)),
        new("Matrix.IsInvertible",
            async () => await Assert.That(MZero).IsInvertible(Tol),
            () => MatrixAssertions.IsInvertible(MIdentity, Tol),
            () => !MatrixAssertions.IsInvertible(MZero, Tol)),
        new("NumberTheory.IsDivisibleBy",
            async () => await Assert.That(Seven).IsDivisibleBy(Two),
            () => NumberTheoryAssertions.IsDivisibleBy(Six, Two),
            () => !NumberTheoryAssertions.IsDivisibleBy(Seven, Two)),
        new("NumberTheory.IsPrime",
            async () => await Assert.That(Nine).IsPrime(),
            () => NumberTheoryAssertions.IsPrime(Seven),
            () => !NumberTheoryAssertions.IsPrime(Nine)),
    ];

    private static AdapterCase[] Rows6() =>
    [
        new("NumberTheory.IsCoprimeWith",
            async () => await Assert.That(Six).IsCoprimeWith(Nine),
            () => NumberTheoryAssertions.IsCoprimeWith(Eight, Nine),
            () => !NumberTheoryAssertions.IsCoprimeWith(Six, Nine)),
        new("NumberTheory.IsPowerOf",
            async () => await Assert.That(Six).IsPowerOf(Two),
            () => NumberTheoryAssertions.IsPowerOf(Eight, Two),
            () => !NumberTheoryAssertions.IsPowerOf(Six, Two)),
        new("NumberTheory.IsPerfectSquare",
            async () => await Assert.That(Eight).IsPerfectSquare(),
            () => NumberTheoryAssertions.IsPerfectSquare(Nine),
            () => !NumberTheoryAssertions.IsPerfectSquare(Eight)),
        new("NumberTheory.IsCongruentTo",
            async () => await Assert.That(Seven).IsCongruentTo(Three, Five),
            () => NumberTheoryAssertions.IsCongruentTo(Seven, Two, Five),
            () => !NumberTheoryAssertions.IsCongruentTo(Seven, Three, Five)),
        new("Plane.IsApproximatelyEqualTo",
            async () => await Assert.That(FloorPlane).IsApproximatelyEqualTo(RaisedPlane, Tol),
            () => PlaneAssertions.IsApproximatelyEqualTo(FloorPlane, FloorPlane, Tol).IsPassed,
            () => !PlaneAssertions.IsApproximatelyEqualTo(FloorPlane, RaisedPlane, Tol).IsPassed),
        new("Plane.IsGeometricallyEquivalentTo",
            async () => await Assert.That(FloorPlane).IsGeometricallyEquivalentTo(RaisedPlane, Tol),
            () => PlaneAssertions.IsGeometricallyEquivalentTo(FloorPlane, new Plane(-Vector3.UnitZ, 0f), Tol).IsPassed,
            () => !PlaneAssertions.IsGeometricallyEquivalentTo(FloorPlane, RaisedPlane, Tol).IsPassed),
        new("Pose.IsPoseApproximatelyEqualTo",
            async () => await Assert.That(PoseHome).IsPoseApproximatelyEqualTo(PoseMoved, Tol, D1),
            () => PoseAssertions.IsPoseApproximatelyEqualTo(PoseHome, PoseHome, Tol, D1).IsPassed,
            () => !PoseAssertions.IsPoseApproximatelyEqualTo(PoseHome, PoseMoved, Tol, D1).IsPassed),
        new("Pose.IsRigidTransformApproximatelyEqualTo",
            async () => await Assert.That(MIdentity).IsRigidTransformApproximatelyEqualTo(MTranslated, Tol, D1),
            () => PoseAssertions.IsRigidTransformApproximatelyEqualTo(MIdentity, MIdentity, Tol, D1).IsPassed,
            () => !PoseAssertions.IsRigidTransformApproximatelyEqualTo(MIdentity, MTranslated, Tol, D1).IsPassed),
    ];

    private static AdapterCase[] Rows7() =>
    [
        new("Quaternion.IsApproximatelyEqualTo",
            async () => await Assert.That(Identity).IsApproximatelyEqualTo(Turned, Tol),
            () => QuaternionAssertions.IsApproximatelyEqualTo(Identity, Identity, Tol).IsPassed,
            () => !QuaternionAssertions.IsApproximatelyEqualTo(Identity, Turned, Tol).IsPassed),
        new("Quaternion.IsRotationallyEquivalentTo",
            async () => await Assert.That(Identity).IsRotationallyEquivalentTo(Turned, Tol),
            () => QuaternionAssertions.IsRotationallyEquivalentTo(Identity, new Quaternion(0f, 0f, 0f, -1f), Tol).IsPassed,
            () => !QuaternionAssertions.IsRotationallyEquivalentTo(Identity, Turned, Tol).IsPassed),
        new("Quaternion.HasAxisAngleApproximately",
            async () => await Assert.That(Turned).HasAxisAngleApproximately(Vector3.UnitZ, 90.0, 1e-3),
            () => QuaternionAssertions.HasAxisAngleApproximately(Turned, Vector3.UnitZ, 1.0 * 180.0 / Math.PI, 1e-6).IsPassed,
            () => !QuaternionAssertions.HasAxisAngleApproximately(Turned, Vector3.UnitZ, 90.0, 1e-3).IsPassed),
        new("Quaternion.IsIdentity",
            async () => await Assert.That(Turned).IsIdentity(Tol),
            () => QuaternionAssertions.IsIdentity(Identity, Tol),
            () => !QuaternionAssertions.IsIdentity(Turned, Tol)),
        new("Quaternion.IsNormalized",
            async () => await Assert.That(NotUnit).IsNormalized(Tol),
            () => QuaternionAssertions.IsNormalized(Identity, Tol),
            () => !QuaternionAssertions.IsNormalized(NotUnit, Tol)),
        new("Scalar.IsApproximatelyEqualTo(double)",
            async () => await Assert.That(D1).IsApproximatelyEqualTo(D2, Tol),
            () => ScalarAssertions.IsApproximatelyEqualTo(D1, D1, Tol),
            () => !ScalarAssertions.IsApproximatelyEqualTo(D1, D2, Tol)),
        new("Scalar.IsApproximatelyEqualTo(float)",
            async () => await Assert.That(F1).IsApproximatelyEqualTo(F2, FTol),
            () => ScalarAssertions.IsApproximatelyEqualTo(F1, F1, FTol),
            () => !ScalarAssertions.IsApproximatelyEqualTo(F1, F2, FTol)),
        new("Scalar.IsAngleApproximatelyEqualTo(double)",
            async () => await Assert.That(350.0 + D1 - D1).IsAngleApproximatelyEqualTo(5.0 + D1 - D1, Tol),
            () => ScalarAssertions.IsAngleApproximatelyEqualTo(359.95, 0.0, Tol).IsPassed,
            () => !ScalarAssertions.IsAngleApproximatelyEqualTo(350.0, 5.0, Tol).IsPassed),
    ];

    private static AdapterCase[] Rows8() =>
    [
        new("Scalar.IsAngleApproximatelyEqualTo(float)",
            async () => await Assert.That(350f + F1 - F1).IsAngleApproximatelyEqualTo(5f + F1 - F1, FTol),
            () => ScalarAssertions.IsAngleApproximatelyEqualTo(359.95f, 0f, FTol).IsPassed,
            () => !ScalarAssertions.IsAngleApproximatelyEqualTo(350f, 5f, FTol).IsPassed),
        new("Scalar.IsCloseInUlpsTo(double)",
            async () => await Assert.That(D1).IsCloseInUlpsTo(D2, 1L),
            () => ScalarAssertions.IsCloseInUlpsTo(D1, Math.BitIncrement(D1), 1L),
            () => !ScalarAssertions.IsCloseInUlpsTo(D1, D2, 1L)),
        new("Scalar.IsCloseInUlpsTo(float)",
            async () => await Assert.That(F1).IsCloseInUlpsTo(F2, 1),
            () => ScalarAssertions.IsCloseInUlpsTo(F1, MathF.BitIncrement(F1), 1),
            () => !ScalarAssertions.IsCloseInUlpsTo(F1, F2, 1)),
        new("Scalar.IsRelativelyAndAbsolutelyCloseTo",
            async () => await Assert.That(D1).IsRelativelyAndAbsolutelyCloseTo(D2, 0.1, 0.01),
            () => ScalarAssertions.IsRelativelyAndAbsolutelyCloseTo(100.0, 101.0, 0.1, 0.01),
            () => !ScalarAssertions.IsRelativelyAndAbsolutelyCloseTo(D1, D2, 0.1, 0.01)),
        new("Scalar.IsNonNegativeFinite",
            async () => await Assert.That(-D1).IsNonNegativeFinite(),
            () => ScalarAssertions.IsNonNegativeFinite(D1),
            () => !ScalarAssertions.IsNonNegativeFinite(-D1)),
        new("Scalar.IsProbability",
            async () => await Assert.That(D2).IsProbability(),
            () => ScalarAssertions.IsProbability(0.5),
            () => !ScalarAssertions.IsProbability(D2)),
        new("Scalar.IsPercentage",
            async () => await Assert.That(D2 * 100.0).IsPercentage(),
            () => ScalarAssertions.IsPercentage(50.0),
            () => !ScalarAssertions.IsPercentage(200.0)),
        new("Scalar.HasRoundtripIdentity",
            async () => await Assert.That(D1).HasRoundtripIdentity(static x => x * 2.0, static y => y * 3.0, Tol),
            () => ScalarAssertions.HasRoundtripIdentity(D1, static x => x * 2.0, static y => y / 2.0, Tol),
            () => !ScalarAssertions.HasRoundtripIdentity(D1, static x => x * 2.0, static y => y * 3.0, Tol)),
    ];

    private static AdapterCase[] Rows9() =>
    [
        new("Sequences.IsMonotonicallyIncreasing",
            async () => await Assert.That(Descending).IsMonotonicallyIncreasing(),
            () => SequencesAssertions.IsMonotonicallyIncreasing(Ascending),
            () => !SequencesAssertions.IsMonotonicallyIncreasing(Descending)),
        new("Sequences.IsMonotonicallyDecreasing",
            async () => await Assert.That(Ascending).IsMonotonicallyDecreasing(),
            () => SequencesAssertions.IsMonotonicallyDecreasing(Descending),
            () => !SequencesAssertions.IsMonotonicallyDecreasing(Ascending)),
        new("Sequences.IsStrictlyMonotonicallyIncreasing",
            async () => await Assert.That(Flat3).IsStrictlyMonotonicallyIncreasing(),
            () => SequencesAssertions.IsStrictlyMonotonicallyIncreasing(Ascending),
            () => !SequencesAssertions.IsStrictlyMonotonicallyIncreasing(Flat3)),
        new("Sequences.IsStrictlyMonotonicallyDecreasing",
            async () => await Assert.That(Flat3).IsStrictlyMonotonicallyDecreasing(),
            () => SequencesAssertions.IsStrictlyMonotonicallyDecreasing(Descending),
            () => !SequencesAssertions.IsStrictlyMonotonicallyDecreasing(Flat3)),
        new("Sequences.IsSorted",
            async () => await Assert.That(Descending).IsSorted(),
            () => SequencesAssertions.IsSorted(Ascending),
            () => !SequencesAssertions.IsSorted(Descending)),
        new("Sequences.IsBounded",
            async () => await Assert.That(Outlier).IsBounded(0.0, 10.0),
            () => SequencesAssertions.IsBounded(Ascending, 0.0, 10.0),
            () => !SequencesAssertions.IsBounded(Outlier, 0.0, 10.0)),
        new("Sequences.IsArithmeticProgression",
            async () => await Assert.That(NotArithmetic).IsArithmeticProgression(Tol),
            () => SequencesAssertions.IsArithmeticProgression(Arithmetic, Tol),
            () => !SequencesAssertions.IsArithmeticProgression(NotArithmetic, Tol)),
        new("Sequences.IsGeometricProgression",
            async () => await Assert.That(NotGeometric).IsGeometricProgression(Tol),
            () => SequencesAssertions.IsGeometricProgression(Geometric, Tol),
            () => !SequencesAssertions.IsGeometricProgression(NotGeometric, Tol)),
    ];

    private static AdapterCase[] Rows10() =>
    [
        new("Sequences.ConvergesTo",
            async () => await Assert.That(Diverging).ConvergesTo(D1, Tol),
            () => SequencesAssertions.ConvergesTo(Settling, 1.0, 0.001),
            () => !SequencesAssertions.ConvergesTo(Diverging, D1, Tol)),
        new("Sequences.IsCauchyConvergent",
            async () => await Assert.That(Diverging).IsCauchyConvergent(Tol),
            () => SequencesAssertions.IsCauchyConvergent(Settling, 0.001),
            () => !SequencesAssertions.IsCauchyConvergent(Diverging, Tol)),
        new("Sequences.HasLength",
            async () => await Assert.That(SingleValue).HasLength(2),
            () => SequencesAssertions.HasLength(SingleValue, 1),
            () => !SequencesAssertions.HasLength(SingleValue, 2)),
        new("Sequences.HasMinLength",
            async () => await Assert.That(SingleValue).HasMinLength(2),
            () => SequencesAssertions.HasMinLength(SingleValue, 1),
            () => !SequencesAssertions.HasMinLength(SingleValue, 2)),
        new("Statistics.HasMeanApproximately",
            async () => await Assert.That(Sample).HasMeanApproximately(D1, Tol),
            () => StatisticsAssertions.HasMeanApproximately(Sample, D2, Tol),
            () => !StatisticsAssertions.HasMeanApproximately(Sample, D1, Tol)),
        new("Statistics.HasVarianceApproximately",
            async () => await Assert.That(Sample).HasVarianceApproximately(D2, Tol),
            () => StatisticsAssertions.HasVarianceApproximately(Sample, D1, Tol),
            () => !StatisticsAssertions.HasVarianceApproximately(Sample, D2, Tol)),
        new("Statistics.HasStdDevApproximately",
            async () => await Assert.That(Sample).HasStdDevApproximately(D2, Tol),
            () => StatisticsAssertions.HasStdDevApproximately(Sample, D1, Tol),
            () => !StatisticsAssertions.HasStdDevApproximately(Sample, D2, Tol)),
        new("Statistics.HasSumApproximately",
            async () => await Assert.That(Sample).HasSumApproximately(D1, Tol),
            () => StatisticsAssertions.HasSumApproximately(Sample, 6.0, Tol),
            () => !StatisticsAssertions.HasSumApproximately(Sample, D1, Tol)),
    ];

    private static AdapterCase[] Rows11() =>
    [
        new("Statistics.HasMedianApproximately",
            async () => await Assert.That(Sample).HasMedianApproximately(D1, Tol),
            () => StatisticsAssertions.HasMedianApproximately(Sample, D2, Tol),
            () => !StatisticsAssertions.HasMedianApproximately(Sample, D1, Tol)),
        new("Statistics.HasPercentileApproximately",
            async () => await Assert.That(Sample).HasPercentileApproximately(50.0, D1, Tol),
            () => StatisticsAssertions.HasPercentileApproximately(Sample, 50.0, D2, Tol),
            () => !StatisticsAssertions.HasPercentileApproximately(Sample, 50.0, D1, Tol)),
        new("Statistics.IsWithinSigmasOfMean",
            async () => await Assert.That(D2 * 50.0).IsWithinSigmasOfMean(Sample, D1),
            () => StatisticsAssertions.IsWithinSigmasOfMean(D2, Sample, D1),
            () => !StatisticsAssertions.IsWithinSigmasOfMean(100.0, Sample, D1)),
        new("Statistics.AreAllWithinSigmasOfMean",
            async () => await Assert.That(Outlier).AreAllWithinSigmasOfMean(0.5),
            () => StatisticsAssertions.AreAllWithinSigmasOfMean(Sample, 2.0),
            () => !StatisticsAssertions.AreAllWithinSigmasOfMean(Outlier, 0.5)),
        new("Vector.IsApproximatelyEqualTo(Vector2)",
            async () => await Assert.That(V2A).IsApproximatelyEqualTo(V2B, Tol),
            () => VectorAssertions.IsApproximatelyEqualTo(V2A, V2A, Tol).IsPassed,
            () => !VectorAssertions.IsApproximatelyEqualTo(V2A, V2B, Tol).IsPassed),
        new("Vector.IsApproximatelyEqualTo(Vector3)",
            async () => await Assert.That(V3A).IsApproximatelyEqualTo(V3B, Tol),
            () => VectorAssertions.IsApproximatelyEqualTo(V3A, V3A, Tol).IsPassed,
            () => !VectorAssertions.IsApproximatelyEqualTo(V3A, V3B, Tol).IsPassed),
        new("Vector.IsApproximatelyEqualTo(Vector4)",
            async () => await Assert.That(V4A).IsApproximatelyEqualTo(V4B, Tol),
            () => VectorAssertions.IsApproximatelyEqualTo(V4A, V4A, Tol).IsPassed,
            () => !VectorAssertions.IsApproximatelyEqualTo(V4A, V4B, Tol).IsPassed),
        new("Vector.HasMagnitudeApproximately",
            async () => await Assert.That(new Vector3(3f, 4f, 0f) * F1).HasMagnitudeApproximately(D1, Tol),
            () => VectorAssertions.HasMagnitudeApproximately(new Vector3(3f, 4f, 0f), 5.0, Tol),
            () => !VectorAssertions.HasMagnitudeApproximately(new Vector3(3f, 4f, 0f), D1, Tol)),
    ];

    private static AdapterCase[] Rows12() =>
    [
        new("Vector.IsNormalized",
            async () => await Assert.That(new Vector3(3f, 4f, 0f) * F1).IsNormalized(Tol),
            () => VectorAssertions.IsNormalized(Vector3.UnitX, Tol),
            () => !VectorAssertions.IsNormalized(new Vector3(3f, 4f, 0f), Tol)),
    ];
}
