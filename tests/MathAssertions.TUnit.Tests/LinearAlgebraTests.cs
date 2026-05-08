using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the semantics of <see cref="LinearAlgebra"/>. Reference matrices: the identity
/// (symmetric, orthogonal, identity, det 1, trace 4, invertible), a translation matrix
/// (not orthogonal, still invertible because det != 0), a singular matrix (det 0, not
/// invertible), and a rotation matrix (orthogonal, det 1). Reference vectors: the three
/// standard basis vectors plus simple parallel and orthogonal pairs.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class LinearAlgebraTests
{
    private const double Tol = 1e-6;

    // ----- IsSymmetric -----

    [Test]
    public async Task IsSymmetric_Identity_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.IsSymmetric(Matrix4x4.Identity, Tol)).IsTrue();
    }

    [Test]
    public async Task IsSymmetric_DiagonalMatrix_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        m.M11 = 2f;
        m.M22 = 3f;
        m.M33 = 5f;
        m.M44 = 7f;
        await Assert.That(LinearAlgebra.IsSymmetric(m, Tol)).IsTrue();
    }

    [Test]
    public async Task IsSymmetric_AsymmetricOffDiagonal_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        m.M12 = 1f;  // M12 != M21
        await Assert.That(LinearAlgebra.IsSymmetric(m, Tol)).IsFalse();
    }

    [Test]
    public async Task IsSymmetric_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.IsSymmetric(Matrix4x4.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsOrthogonal -----

    [Test]
    public async Task IsOrthogonal_Identity_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // M = I, M^T = I, M * M^T = I -> orthogonal.
        await Assert.That(LinearAlgebra.IsOrthogonal(Matrix4x4.Identity, Tol)).IsTrue();
    }

    [Test]
    public async Task IsOrthogonal_PureRotation_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Rotation matrices are orthogonal: their inverse equals their transpose.
        var rotation = Matrix4x4.CreateRotationY(MathF.PI / 4);
        await Assert.That(LinearAlgebra.IsOrthogonal(rotation, Tol)).IsTrue();
    }

    [Test]
    public async Task IsOrthogonal_TranslationMatrix_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Translation matrices are not orthogonal: the translation column makes
        // M * M^T deviate from identity in the off-diagonals.
        var translation = Matrix4x4.CreateTranslation(new Vector3(1, 2, 3));
        await Assert.That(LinearAlgebra.IsOrthogonal(translation, Tol)).IsFalse();
    }

    [Test]
    public async Task IsOrthogonal_NonUniformScale_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Scaling is not orthogonal because M * M^T picks up the squared scale on the diagonal.
        var scale = Matrix4x4.CreateScale(2.0f);
        await Assert.That(LinearAlgebra.IsOrthogonal(scale, Tol)).IsFalse();
    }

    [Test]
    public async Task IsOrthogonal_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.IsOrthogonal(Matrix4x4.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsIdentity -----

    [Test]
    public async Task IsIdentity_Identity_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.IsIdentity(Matrix4x4.Identity, 0.0)).IsTrue();
    }

    [Test]
    public async Task IsIdentity_NotIdentity_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rotation = Matrix4x4.CreateRotationY(MathF.PI / 4);
        await Assert.That(LinearAlgebra.IsIdentity(rotation, Tol)).IsFalse();
    }

    [Test]
    public async Task IsIdentity_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.IsIdentity(Matrix4x4.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasDeterminantApproximately -----

    [Test]
    public async Task HasDeterminantApproximately_Identity_One(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.HasDeterminantApproximately(Matrix4x4.Identity, 1.0, Tol)).IsTrue();
    }

    [Test]
    public async Task HasDeterminantApproximately_PureRotation_One(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Pure rotations have determinant +1 (no reflection).
        var rotation = Matrix4x4.CreateRotationZ(MathF.PI / 3);
        await Assert.That(LinearAlgebra.HasDeterminantApproximately(rotation, 1.0, Tol)).IsTrue();
    }

    [Test]
    public async Task HasDeterminantApproximately_SingularMatrix_Zero(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // A matrix with a zero row has determinant zero.
        var singular = new Matrix4x4(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            0, 0, 0, 0);
        await Assert.That(LinearAlgebra.HasDeterminantApproximately(singular, 0.0, Tol)).IsTrue();
    }

    [Test]
    public async Task HasDeterminantApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.HasDeterminantApproximately(Matrix4x4.Identity, 1.0, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- HasTraceApproximately -----

    [Test]
    public async Task HasTraceApproximately_Identity_Four(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.HasTraceApproximately(Matrix4x4.Identity, 4.0, Tol)).IsTrue();
    }

    [Test]
    public async Task HasTraceApproximately_DiagonalMatrix_SumOfDiagonal(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        m.M11 = 2f;
        m.M22 = 3f;
        m.M33 = 5f;
        m.M44 = 7f;
        // 2 + 3 + 5 + 7 = 17.
        await Assert.That(LinearAlgebra.HasTraceApproximately(m, 17.0, Tol)).IsTrue();
    }

    [Test]
    public async Task HasTraceApproximately_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.HasTraceApproximately(Matrix4x4.Identity, 4.0, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- IsInvertible -----

    [Test]
    public async Task IsInvertible_Identity_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.IsInvertible(Matrix4x4.Identity, Tol)).IsTrue();
    }

    [Test]
    public async Task IsInvertible_TranslationMatrix_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Translation has det = 1, so it's invertible (and the inverse is the inverse translation).
        var translation = Matrix4x4.CreateTranslation(new Vector3(1, 2, 3));
        await Assert.That(LinearAlgebra.IsInvertible(translation, Tol)).IsTrue();
    }

    [Test]
    public async Task IsInvertible_SingularMatrix_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var singular = new Matrix4x4(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            0, 0, 0, 0);
        await Assert.That(LinearAlgebra.IsInvertible(singular, Tol)).IsFalse();
    }

    [Test]
    public async Task IsInvertible_ZeroMatrix_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var zero = new Matrix4x4();
        await Assert.That(LinearAlgebra.IsInvertible(zero, Tol)).IsFalse();
    }

    [Test]
    public async Task IsInvertible_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.IsInvertible(Matrix4x4.Identity, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- AreOrthogonal (Vector3, Vector3) -----

    [Test]
    public async Task AreOrthogonal_StandardBasisXY_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.AreOrthogonal(Vector3.UnitX, Vector3.UnitY, Tol)).IsTrue();
    }

    [Test]
    public async Task AreOrthogonal_ParallelVectors_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // u . u = 1 != 0 -> not orthogonal.
        await Assert.That(LinearAlgebra.AreOrthogonal(Vector3.UnitX, Vector3.UnitX, Tol)).IsFalse();
    }

    [Test]
    public async Task AreOrthogonal_OppositeDirections_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // u . -u = -1 != 0.
        await Assert.That(LinearAlgebra.AreOrthogonal(Vector3.UnitX, -Vector3.UnitX, Tol)).IsFalse();
    }

    [Test]
    public async Task AreOrthogonal_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.AreOrthogonal(Vector3.UnitX, Vector3.UnitY, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- AreParallel (Vector3, Vector3) -----

    [Test]
    public async Task AreParallel_SameDirection_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1, 0, 0), new Vector3(2, 0, 0), Tol)).IsTrue();
    }

    [Test]
    public async Task AreParallel_OppositeDirection_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Anti-parallel vectors have cross product zero too.
        await Assert.That(LinearAlgebra.AreParallel(new Vector3(1, 0, 0), new Vector3(-3, 0, 0), Tol)).IsTrue();
    }

    [Test]
    public async Task AreParallel_OrthogonalVectors_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.AreParallel(Vector3.UnitX, Vector3.UnitY, Tol)).IsFalse();
    }

    [Test]
    public async Task AreParallel_ZeroVectorIsParallelToAnything(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // 0 x v = 0 for any v, so the cross-product test treats the zero vector as parallel
        // to every other vector. Documented behavior; pinned here.
        await Assert.That(LinearAlgebra.AreParallel(Vector3.Zero, Vector3.UnitX, Tol)).IsTrue();
    }

    [Test]
    public async Task AreParallel_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() => LinearAlgebra.AreParallel(Vector3.UnitX, Vector3.UnitY, -1e-6))
            .Throws<ArgumentOutOfRangeException>();
    }

    // ----- AreLinearlyIndependent -----

    [Test]
    public async Task AreLinearlyIndependent_Empty_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(ReadOnlySpan<Vector3>.Empty, Tol)).IsTrue();
    }

    [Test]
    public async Task AreLinearlyIndependent_SingleNonZero_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> vectors = [Vector3.UnitX];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsTrue();
    }

    [Test]
    public async Task AreLinearlyIndependent_SingleZero_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> vectors = [Vector3.Zero];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsFalse();
    }

    [Test]
    public async Task AreLinearlyIndependent_TwoOrthogonal_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> vectors = [Vector3.UnitX, Vector3.UnitY];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsTrue();
    }

    [Test]
    public async Task AreLinearlyIndependent_TwoParallel_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> vectors = [new Vector3(1, 0, 0), new Vector3(2, 0, 0)];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsFalse();
    }

    [Test]
    public async Task AreLinearlyIndependent_StandardBasisR3_True(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReadOnlySpan<Vector3> vectors = [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsTrue();
    }

    [Test]
    public async Task AreLinearlyIndependent_ThreeCoplanar_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Third vector is the sum of the first two, so the three are coplanar in z=0.
        ReadOnlySpan<Vector3> vectors =
        [
            new(1, 0, 0),
            new(0, 1, 0),
            new(1, 1, 0),
        ];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsFalse();
    }

    [Test]
    public async Task AreLinearlyIndependent_FourVectorsInR3_False(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Four or more vectors in R^3 cannot be linearly independent.
        ReadOnlySpan<Vector3> vectors =
        [
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ,
            new(1, 1, 1),
        ];
        await Assert.That(LinearAlgebra.AreLinearlyIndependent(vectors, Tol)).IsFalse();
    }

    [Test]
    public async Task AreLinearlyIndependent_NegativeTolerance_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(() =>
        {
            ReadOnlySpan<Vector3> vectors = [Vector3.UnitX];
            return LinearAlgebra.AreLinearlyIndependent(vectors, -1e-6);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AreLinearlyIndependent_NaNToleranceOnEmpty_Throws(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Validation must run before the empty-span vacuous-true return (parity with
        // family-wide validation-order pattern from Sequences.ConvergesTo / Statistics).
        await Assert.That(() => LinearAlgebra.AreLinearlyIndependent(ReadOnlySpan<Vector3>.Empty, double.NaN))
            .Throws<ArgumentOutOfRangeException>();
    }
}
