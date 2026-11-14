using System;
using System.Collections.Generic;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// The pinned failure text of every generated assertion, by overload name: what TUnit prints before the call-site
/// line, rendered with the invariant culture. Regenerate a row only when a message is changed on purpose.
/// </summary>
internal static class AdapterFailureTexts
{
    /// <summary>The expected texts.</summary>
    public static readonly Dictionary<string, string> Expected = new(StringComparer.Ordinal)
    {
        ["Array.IsApproximatelyEqualTo(double[])"] =
            "Expected to satisfy IsApproximatelyEqualTo(System.Double[], 0.1)\n"
            + "but to be approximately equal element-wise within tolerance 0.1 (length 2)\n"
            + "  actual length: 2\n"
            + "  first mismatch at index 1: actual=2, expected=3, delta=1",
        ["Array.IsApproximatelyEqualTo(float[])"] =
            "Expected to satisfy IsApproximatelyEqualTo(System.Single[], 0.1)\n"
            + "but to be approximately equal element-wise within tolerance 0.10000000149011612 (length 2)\n"
            + "  actual length: 2\n"
            + "  first mismatch at index 1: actual=2, expected=3, delta=1",
        ["Complex.IsApproximatelyEqualTo"] =
            "Expected to satisfy IsApproximatelyEqualTo(<1; 3>, 0.1)\n"
            + "but to be approximately equal to <1; 3> component-wise within tolerance 0.1\n"
            + "  actual:   <1; 2>\n"
            + "  delta:    Real=0, Imaginary=1\n"
            + "  exceeded: Imaginary (1 > 0.1)",
        ["Geometry.IsDegenerate"] =
            "Expected to be a degenerate triangle within tolerance 0.1\n"
            + "but found Triangle3D { A = <0, 0, 0>, B = <1, 0, 0>, C = <0, 1, 0>, Normal = <0, 0, 1>, Centroid = <0.33333334, 0.33333334, 0>, Area = 0.5 }",
        ["Geometry.IsCollinear"] =
            "Expected to be collinear within tolerance 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.AreCoplanar"] =
            "Expected to be coplanar within tolerance 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.ContainsPoint(AxisAlignedBox)"] =
            "Expected to contain point <5, 0, 0>\n"
            + "but found AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }",
        ["Geometry.ContainsPoint(Sphere)"] =
            "Expected to contain point <5, 0, 0>\n"
            + "but found Sphere { Center = <0, 0, 0>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }",
        ["Geometry.ContainsPoint(OrientedBox)"] =
            "Expected to contain point <5, 0, 0>\n"
            + "but found OrientedBox { Center = <0, 0, 0>, HalfExtents = <1, 1, 1>, Orientation = {X:0 Y:0 Z:0 W:1} }",
        ["Geometry.ContainsBox"] =
            "Expected to contain box AxisAlignedBox { Min = <0.5, 0.5, 0.5>, Max = <2, 2, 2>, Center = <1.25, 1.25, 1.25>, Size = <1.5, 1.5, 1.5>, HalfExtents = <0.75, 0.75, 0.75>, Volume = 3.375 }\n"
            + "but found AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }",
        ["Geometry.ContainsSphere"] =
            "Expected to contain sphere Sphere { Center = <0.5, 0.5, 0.5>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }\n"
            + "but found AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }",
        ["Geometry.ConvexHullContainsPoint"] =
            "Expected to contain point <0, 0, 5> within tolerance 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.HasDistanceFromPlane"] =
            "Expected to be at distance 1 from {Normal:<0, 0, 1> D:0} within tolerance 0.1\n"
            + "but found <0, 0, 5>",
        ["Geometry.HasDistanceFromSegment"] =
            "Expected to be at distance 1 from LineSegment3D { Start = <0, 0, 0>, End = <1, 0, 0>, Direction = <1, 0, 0>, Length = 1 } within tolerance 0.1\n"
            + "but found <0, 5, 0>",
        ["Geometry.HasDistanceFromTriangle"] =
            "Expected to be at distance 1 from Triangle3D { A = <0, 0, 0>, B = <1, 0, 0>, C = <0, 1, 0>, Normal = <0, 0, 1>, Centroid = <0.33333334, 0.33333334, 0>, Area = 0.5 } within tolerance 0.1\n"
            + "but found <0, 0, 5>",
        ["Geometry.IntersectsSphere(Sphere)"] =
            "Expected to intersect sphere Sphere { Center = <9, 0, 0>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }\n"
            + "but found Sphere { Center = <0, 0, 0>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }",
        ["Geometry.IntersectsSphere(Ray3D)"] =
            "Expected to intersect sphere Sphere { Center = <0, 0, 0>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }\n"
            + "but found Ray3D { Origin = <0, 5, 0>, Direction = <1, 0, 0> }",
        ["Geometry.IntersectsBox(AxisAlignedBox)"] =
            "Expected to intersect box AxisAlignedBox { Min = <8, 8, 8>, Max = <9, 9, 9>, Center = <8.5, 8.5, 8.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }\n"
            + "but found AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }",
        ["Geometry.IntersectsBox(Ray3D)"] =
            "Expected to intersect box AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }\n"
            + "but found Ray3D { Origin = <0, 5, 0>, Direction = <1, 0, 0> }",
        ["Geometry.IntersectsPlane"] =
            "Expected to intersect plane {Normal:<0, 0, 1> D:0}\n"
            + "but found Ray3D { Origin = <0, 0, 1>, Direction = <1, 0, 0> }",
        ["Geometry.IntersectsTriangle"] =
            "Expected to intersect triangle Triangle3D { A = <0, 0, 0>, B = <1, 0, 0>, C = <0, 1, 0>, Normal = <0, 0, 1>, Centroid = <0.33333334, 0.33333334, 0>, Area = 0.5 }\n"
            + "but found Ray3D { Origin = <0, 5, 0>, Direction = <1, 0, 0> }",
        ["Geometry.IsBoundedByBox"] =
            "Expected to be bounded by box AxisAlignedBox { Min = <0, 0, 0>, Max = <1, 1, 1>, Center = <0.5, 0.5, 0.5>, Size = <1, 1, 1>, HalfExtents = <0.5, 0.5, 0.5>, Volume = 1 }\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.IsBoundedBySphere"] =
            "Expected to be bounded by sphere Sphere { Center = <0, 0, 0>, Radius = 1, Volume = 4.1887903, SurfaceArea = 12.566371 }\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.HasCentroidAt"] =
            "Expected to have centroid approximately <5, 0, 0> within tolerance 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.IsApproximatelyOnPlane"] =
            "Expected to lie approximately on plane {Normal:<0, 0, 1> D:0} within residual 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Geometry.IsApproximatelyOnSphere"] =
            "Expected to lie approximately on a sphere centered at <0, 0, 0> with radius 1 within residual 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["LinearAlgebra.IsOrthogonalTo"] =
            "Expected to be orthogonal to <1, 1, 0> within tolerance 0.1\n"
            + "but found <1, 0, 0>",
        ["LinearAlgebra.IsParallelTo"] =
            "Expected to be parallel to <0, 1, 0> within tolerance 0.1\n"
            + "but found <1, 0, 0>",
        ["LinearAlgebra.HasAngleBetweenApproximately"] =
            "Expected to have an angle of 1 rad to <0, 1, 0> within tolerance 0.1\n"
            + "but found <1, 0, 0>",
        ["LinearAlgebra.AreLinearlyIndependent"] =
            "Expected to be linearly independent within tolerance 0.1\n"
            + "but found System.Numerics.Vector3[]",
        ["Matrix.IsApproximatelyEqualTo"] =
            "Expected to satisfy IsApproximatelyEqualTo({ {M11:1 M12:0 M13:0 M14:0} {M21:0 M22:1 M23:0 M24:0} {M31:0 M32:0 M33:1 M34:0} {M41:1 M42:0 M43:0 M44:1} }, 0.1)\n"
            + "but to be approximately equal to matrix { { M11:1 M12:0 M13:0 M14:0 } { M21:0 M22:1 M23:0 M24:0 } { M31:0 M32:0 M33:1 M34:0 } { M41:1 M42:0 M43:0 M44:1 } } component-wise within tolerance 0.1\n"
            + "  actual:   { { M11:1 M12:0 M13:0 M14:0 } { M21:0 M22:1 M23:0 M24:0 } { M31:0 M32:0 M33:1 M34:0 } { M41:0 M42:0 M43:0 M44:1 } }\n"
            + "  exceeded: [3,0] (1 > 0.1)",
        ["Matrix.IsSymmetric"] =
            "Expected to be symmetric within tolerance 0.1\n"
            + "but found { {M11:1 M12:0 M13:0 M14:0} {M21:0 M22:1 M23:0 M24:0} {M31:0 M32:0 M33:1 M34:0} {M41:1 M42:2 M43:3 M44:1} }",
        ["Matrix.IsOrthogonal"] =
            "Expected to be orthogonal within tolerance 0.1\n"
            + "but found { {M11:2 M12:0 M13:0 M14:0} {M21:0 M22:2 M23:0 M24:0} {M31:0 M32:0 M33:2 M34:0} {M41:0 M42:0 M43:0 M44:1} }",
        ["Matrix.IsRotation"] =
            "Expected to be a proper rotation within tolerance 0.1\n"
            + "but found { {M11:2 M12:0 M13:0 M14:0} {M21:0 M22:2 M23:0 M24:0} {M31:0 M32:0 M33:2 M34:0} {M41:0 M42:0 M43:0 M44:1} }",
        ["Matrix.IsIdentity"] =
            "Expected to be the identity matrix within tolerance 0.1\n"
            + "but found { {M11:2 M12:0 M13:0 M14:0} {M21:0 M22:2 M23:0 M24:0} {M31:0 M32:0 M33:2 M34:0} {M41:0 M42:0 M43:0 M44:1} }",
        ["Matrix.HasDeterminantApproximately"] =
            "Expected to have determinant approximately 2 within tolerance 0.1\n"
            + "but found { {M11:1 M12:0 M13:0 M14:0} {M21:0 M22:1 M23:0 M24:0} {M31:0 M32:0 M33:1 M34:0} {M41:0 M42:0 M43:0 M44:1} }",
        ["Matrix.HasTraceApproximately"] =
            "Expected to have trace approximately 2 within tolerance 0.1\n"
            + "but found { {M11:1 M12:0 M13:0 M14:0} {M21:0 M22:1 M23:0 M24:0} {M31:0 M32:0 M33:1 M34:0} {M41:0 M42:0 M43:0 M44:1} }",
        ["Matrix.IsInvertible"] =
            "Expected to be invertible (|det| > tolerance 0.1)\n"
            + "but found { {M11:0 M12:0 M13:0 M14:0} {M21:0 M22:0 M23:0 M24:0} {M31:0 M32:0 M33:0 M34:0} {M41:0 M42:0 M43:0 M44:0} }",
        ["NumberTheory.IsDivisibleBy"] =
            "Expected to be divisible by 2\n"
            + "but found 7",
        ["NumberTheory.IsPrime"] =
            "Expected to be prime\n"
            + "but found 9",
        ["NumberTheory.IsCoprimeWith"] =
            "Expected to be coprime with 9\n"
            + "but found 6",
        ["NumberTheory.IsPowerOf"] =
            "Expected to be a power of 2\n"
            + "but found 6",
        ["NumberTheory.IsPerfectSquare"] =
            "Expected to be a perfect square\n"
            + "but found 8",
        ["NumberTheory.IsCongruentTo"] =
            "Expected to be congruent to 3 modulo 5\n"
            + "but found 7",
        ["Plane.IsApproximatelyEqualTo"] =
            "Expected to satisfy IsApproximatelyEqualTo({Normal:<0, 0, 1> D:1}, 0.1)\n"
            + "but to be approximately equal to { Normal:<0, 0, 1> D:1 } component-wise within tolerance 0.1\n"
            + "  actual:   { Normal:<0, 0, 1> D:0 }\n"
            + "  delta:    Normal=(0, 0, 0), D=1\n"
            + "  exceeded: D (1 > 0.1)",
        ["Plane.IsGeometricallyEquivalentTo"] =
            "Expected to satisfy IsGeometricallyEquivalentTo({Normal:<0, 0, 1> D:1}, 0.1)\n"
            + "but to be geometrically equivalent to { Normal:<0, 0, 1> D:1 } within tolerance 0.1\n"
            + "  actual:   { Normal:<0, 0, 1> D:0 }\n"
            + "  delta vs expected:        Normal=(0, 0, 0), D=1\n"
            + "  delta vs sign-flipped:    Normal=(0, 0, 2), D=1",
        ["Pose.IsPoseApproximatelyEqualTo"] =
            "Expected to satisfy IsPoseApproximatelyEqualTo((<1, 0, 0>, {X:0 Y:0 Z:0 W:1}), 0.1, 1)\n"
            + "but to be the same pose within position tolerance 0.1 and rotation tolerance 1 degrees\n"
            + "  expected: pos <1, 0, 0> quat {X:0 Y:0 Z:0 W:1}\n"
            + "  actual:   pos <0, 0, 0> quat {X:0 Y:0 Z:0 W:1}\n"
            + "  position: delta=1 (tolerance 0.1) EXCEEDED\n"
            + "  rotation: delta=0 degrees (tolerance 1 degrees) OK",
        ["Pose.IsRigidTransformApproximatelyEqualTo"] =
            "Expected to satisfy IsRigidTransformApproximatelyEqualTo({ {M11:1 M12:0 M13:0 M14:0} {M21:0 M22:1 M23:0 M24:0} {M31:0 M32:0 M33:1 M34:0} {M41:1 M42:0 M43:0 M44:1} }, 0.1, 1)\n"
            + "but to be the same pose within position tolerance 0.1 and rotation tolerance 1 degrees\n"
            + "  expected: pos <1, 0, 0> quat {X:0 Y:0 Z:0 W:1}\n"
            + "  actual:   pos <0, 0, 0> quat {X:0 Y:0 Z:0 W:1}\n"
            + "  position: delta=1 (tolerance 0.1) EXCEEDED\n"
            + "  rotation: delta=0 degrees (tolerance 1 degrees) OK",
        ["Quaternion.IsApproximatelyEqualTo"] =
            "Expected to satisfy IsApproximatelyEqualTo({X:0 Y:0 Z:0.47942555 W:0.87758255}, 0.1)\n"
            + "but to be approximately equal to {X:0 Y:0 Z:0.4794255495071411 W:0.8775825500488281} component-wise within tolerance 0.1\n"
            + "  actual:   {X:0 Y:0 Z:0 W:1}\n"
            + "  delta:    (0, 0, 0.4794255495071411, 0.12241744995117188)\n"
            + "  exceeded: Z (0.4794255495071411 > 0.1), W (0.12241744995117188 > 0.1)",
        ["Quaternion.IsRotationallyEquivalentTo"] =
            "Expected to satisfy IsRotationallyEquivalentTo({X:0 Y:0 Z:0.47942555 W:0.87758255}, 0.1)\n"
            + "but to be rotationally equivalent to {X:0 Y:0 Z:0.4794255495071411 W:0.8775825500488281} within tolerance 0.1\n"
            + "  actual:        {X:0 Y:0 Z:0 W:1}\n"
            + "  |dot(an, en)|: 0.8775825500488281 (threshold: 0.9 = 1 - 0.1)\n"
            + "  normalized:    actual={X:0 Y:0 Z:0 W:1}, expected={X:0 Y:0 Z:0.4794255495071411 W:0.8775825500488281}",
        ["Quaternion.HasAxisAngleApproximately"] =
            "Expected to satisfy HasAxisAngleApproximately(<0, 0, 1>, 90, 0.001)\n"
            + "but to be a rotation of approximately 90 degrees around <0, 0, 1> within tolerance 0.001\n"
            + "  actual quaternion:  {X:0 Y:0 Z:0.4794255495071411 W:0.8775825500488281}\n"
            + "  extracted axis:     <0, 0, 1>\n"
            + "  extracted angle:    57.29578126007273 degrees\n"
            + "  delta angle:        32.70421873992727 degrees",
        ["Quaternion.IsIdentity"] =
            "Expected to be the identity quaternion within tolerance 0.1\n"
            + "but found {X:0 Y:0 Z:0.47942555 W:0.87758255}",
        ["Quaternion.IsNormalized"] =
            "Expected to be a unit quaternion within tolerance 0.1\n"
            + "but found {X:2 Y:0 Z:0 W:0}",
        ["Scalar.IsApproximatelyEqualTo(double)"] =
            "Expected to be approximately equal to 2 within tolerance 0.1\n"
            + "but found 1",
        ["Scalar.IsApproximatelyEqualTo(float)"] =
            "Expected to be approximately equal to 2 within tolerance 0.1\n"
            + "but found 1",
        ["Scalar.IsAngleApproximatelyEqualTo(double)"] =
            "Expected to satisfy IsAngleApproximatelyEqualTo(5, 0.1)\n"
            + "but the wrapped difference is 15 degrees, outside the tolerance of 0.1 degrees\n"
            + "  actual:   350 degrees\n"
            + "  expected: 5 degrees (angles compared modulo 360)",
        ["Scalar.IsAngleApproximatelyEqualTo(float)"] =
            "Expected to satisfy IsAngleApproximatelyEqualTo(5, 0.1)\n"
            + "but the wrapped difference is 15 degrees, outside the tolerance of 0.1 degrees\n"
            + "  actual:   350 degrees\n"
            + "  expected: 5 degrees (angles compared modulo 360)",
        ["Scalar.IsCloseInUlpsTo(double)"] =
            "Expected to be within 1 ULPs of 2\n"
            + "but found 1",
        ["Scalar.IsCloseInUlpsTo(float)"] =
            "Expected to be within 1 ULPs of 2\n"
            + "but found 1",
        ["Scalar.IsRelativelyAndAbsolutelyCloseTo"] =
            "Expected to be within relative tolerance 0.1 or absolute tolerance 0.01 of 2\n"
            + "but found 1",
        ["Scalar.IsNonNegativeFinite"] =
            "Expected to be a finite non-negative value\n"
            + "but found -1",
        ["Scalar.IsProbability"] =
            "Expected to be a probability (finite, in [0, 1])\n"
            + "but found 2",
        ["Scalar.IsPercentage"] =
            "Expected to be a percentage (finite, in [0, 100])\n"
            + "but found 200",
        ["Scalar.HasRoundtripIdentity"] =
            "Expected to roundtrip through forward/inverse within tolerance 0.1\n"
            + "but found 1",
        ["Sequences.IsMonotonicallyIncreasing"] =
            "Expected to be monotonically increasing\n"
            + "but found System.Double[]",
        ["Sequences.IsMonotonicallyDecreasing"] =
            "Expected to be monotonically decreasing\n"
            + "but found System.Double[]",
        ["Sequences.IsStrictlyMonotonicallyIncreasing"] =
            "Expected to be strictly monotonically increasing\n"
            + "but found System.Double[]",
        ["Sequences.IsStrictlyMonotonicallyDecreasing"] =
            "Expected to be strictly monotonically decreasing\n"
            + "but found System.Double[]",
        ["Sequences.IsSorted"] =
            "Expected to be sorted ascending\n"
            + "but found System.Double[]",
        ["Sequences.IsBounded"] =
            "Expected to be bounded by [0, 10]\n"
            + "but found System.Double[]",
        ["Sequences.IsArithmeticProgression"] =
            "Expected to be an arithmetic progression within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Sequences.IsGeometricProgression"] =
            "Expected to be a geometric progression within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Sequences.ConvergesTo"] =
            "Expected to converge to 1 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Sequences.IsCauchyConvergent"] =
            "Expected to be Cauchy convergent within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Sequences.HasLength"] =
            "Expected to have length 2\n"
            + "but found System.Double[]",
        ["Sequences.HasMinLength"] =
            "Expected to have at least 2 elements\n"
            + "but found System.Double[]",
        ["Statistics.HasMeanApproximately"] =
            "Expected to have mean approximately 1 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.HasVarianceApproximately"] =
            "Expected to have variance approximately 2 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.HasStdDevApproximately"] =
            "Expected to have standard deviation approximately 2 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.HasSumApproximately"] =
            "Expected to have sum approximately 1 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.HasMedianApproximately"] =
            "Expected to have median approximately 1 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.HasPercentileApproximately"] =
            "Expected to have 50-percentile approximately 1 within tolerance 0.1\n"
            + "but found System.Double[]",
        ["Statistics.IsWithinSigmasOfMean"] =
            "Expected to lie within 1 sigmas of the sample mean\n"
            + "but found 100",
        ["Statistics.AreAllWithinSigmasOfMean"] =
            "Expected to have every element within 0.5 sigmas of its own mean\n"
            + "but found System.Double[]",
        ["Vector.IsApproximatelyEqualTo(Vector2)"] =
            "Expected to satisfy IsApproximatelyEqualTo(<1, 3>, 0.1)\n"
            + "but to be approximately equal to <1, 3> component-wise within tolerance 0.1\n"
            + "  actual:   <1, 2>\n"
            + "  delta:    (0, 1)\n"
            + "  exceeded: Y (1 > 0.1)",
        ["Vector.IsApproximatelyEqualTo(Vector3)"] =
            "Expected to satisfy IsApproximatelyEqualTo(<1, 2, 4>, 0.1)\n"
            + "but to be approximately equal to <1, 2, 4> component-wise within tolerance 0.1\n"
            + "  actual:   <1, 2, 3>\n"
            + "  delta:    (0, 0, 1)\n"
            + "  exceeded: Z (1 > 0.1)",
        ["Vector.IsApproximatelyEqualTo(Vector4)"] =
            "Expected to satisfy IsApproximatelyEqualTo(<1, 2, 3, 5>, 0.1)\n"
            + "but to be approximately equal to <1, 2, 3, 5> component-wise within tolerance 0.1\n"
            + "  actual:   <1, 2, 3, 4>\n"
            + "  delta:    (0, 0, 0, 1)\n"
            + "  exceeded: W (1 > 0.1)",
        ["Vector.HasMagnitudeApproximately"] =
            "Expected to have magnitude approximately 1 within tolerance 0.1\n"
            + "but found <3, 4, 0>",
        ["Vector.IsNormalized"] =
            "Expected to be a unit (normalized) vector within tolerance 0.1\n"
            + "but found <3, 4, 0>",
    };
}
