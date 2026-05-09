namespace Smoke.Consumer;

/// <summary>
/// Smoke tests proving that an external consumer can adopt MathAssertions.TUnit purely via
/// the README's recommended GlobalUsings.cs snippet, with no extra
/// <c>using MathAssertions.TUnit;</c> directive at every call site and no other wiring. The
/// test class lives in <c>Smoke.Consumer</c> deliberately: MathAssertions.TUnit's own test
/// project is in the <c>MathAssertions.TUnit.Tests</c> namespace, which inherits
/// parent-namespace visibility into <c>MathAssertions.TUnit</c>. That inheritance would mask
/// any future namespace-resolution bug in the source-generated entry points. By placing this
/// file in a namespace with NO parent relationship to MathAssertions.TUnit, this project is
/// the canonical regression coverage for the resolution-pathway bug class.
/// </summary>
[Category("ConsumerSurface")]
[Timeout(10_000)]
internal sealed class ConsumerSurfaceSmokeTests
{
    /// <summary>
    /// Pins that <c>IsApproximatelyEqualTo</c> on Vector3 resolves cleanly for an external
    /// consumer, source-generator-emitted entry point in <c>TUnit.Assertions.Extensions</c>;
    /// auto-imports alongside <c>Assert.That</c>.
    /// </summary>
    [Test]
    public async Task Vector3IsApproximatelyEqualToResolvesAndPassesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1, 2, 3);
        var b = new Vector3(1.0001f, 2.0001f, 3.0001f);

        await Assert.That(a).IsApproximatelyEqualTo(b, tolerance: 0.001);
    }

    /// <summary>
    /// Pins that the precision-preserving widen-to-double cast on the Vector3 overload is
    /// honored end-to-end through the source-generated wrapper. A tolerance of 1e-9 is below
    /// float epsilon; if the chain accidentally narrowed tolerance to float anywhere along
    /// the way, this test would fail.
    /// </summary>
    [Test]
    public async Task Vector3IsApproximatelyEqualToTightDoubleToleranceResolvesAndPassesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Vector3(1.0f, 1.0f, 1.0f);

        await Assert.That(a).IsApproximatelyEqualTo(a, tolerance: 1e-9);
    }

    // ----- One representative call from each adapter file added in 0.1.0. The selection
    // is deliberately small: the goal is to verify resolution, not exhaustive behaviour
    // (the in-tree MathAssertions.TUnit.Tests project covers behaviour against the source
    // tree). If a future package-resolution regression dropped or renamed any of these
    // entry points, this project would fail to compile or fail at the matching test.

    [Test]
    public async Task ScalarIsApproximatelyEqualToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(D(1.0)).IsApproximatelyEqualTo(1.0001, 0.001);
    }

    [Test]
    public async Task QuaternionIsRotationallyEquivalentToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var q = Quaternion.Normalize(new Quaternion(0.1f, 0.2f, 0.3f, 0.9f));
        var negQ = new Quaternion(-q.X, -q.Y, -q.Z, -q.W);
        await Assert.That(q).IsRotationallyEquivalentTo(negQ, 1e-6);
    }

    [Test]
    public async Task MatrixIsIdentityResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var m = Matrix4x4.Identity;
        await Assert.That(m).IsIdentity(1e-6);
    }

    [Test]
    public async Task PlaneIsGeometricallyEquivalentToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var p = new Plane(0, 1, 0, -5);
        var flipped = new Plane(-p.Normal, -p.D);
        await Assert.That(p).IsGeometricallyEquivalentTo(flipped, 1e-6);
    }

    [Test]
    public async Task ComplexIsApproximatelyEqualToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = new Complex(1, 2);
        var b = new Complex(1.0001, 2.0001);
        await Assert.That(a).IsApproximatelyEqualTo(b, 0.001);
    }

    [Test]
    public async Task DoubleArrayIsApproximatelyEqualToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(ArrayHappy).IsApproximatelyEqualTo(ArrayHappyShifted, 0.001);
    }

    [Test]
    public async Task SequenceIsMonotonicallyIncreasingResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(Ascending).IsMonotonicallyIncreasing();
    }

    [Test]
    public async Task StatisticsHasMeanApproximatelyResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(StatsReference).HasMeanApproximately(3.0, 1e-6);
    }

    [Test]
    public async Task LinearAlgebraIsOrthogonalToResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var v = Vector3.UnitX;
        await Assert.That(v).IsOrthogonalTo(Vector3.UnitY, 1e-6);
    }

    [Test]
    public async Task NumberTheoryIsPrimeResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Assert.That(L(17L)).IsPrime();
    }

    [Test]
    public async Task Geometry3DSphereContainsPointResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sphere = new MathAssertions.Geometry3D.Sphere(Vector3.Zero, 1f);
        var point = new Vector3(0.3f, 0.3f, 0.3f);
        await Assert.That(sphere).ContainsPoint(point);
    }

    [Test]
    public async Task Geometry3DRayIntersectsSphereResolvesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var ray = new MathAssertions.Geometry3D.Ray3D(new Vector3(-3, 0, 0), Vector3.UnitX);
        var sphere = new MathAssertions.Geometry3D.Sphere(Vector3.Zero, 1f);
        await Assert.That(ray).IntersectsSphere(sphere);
    }

    // Reusable arrays + literal-hide helpers (CA1861 + TUnitAssertions0005 mitigations).
    private static readonly double[] StatsReference = [1.0, 2.0, 3.0, 4.0, 5.0];
    private static readonly double[] Ascending = [1.0, 2.0, 3.0];
    private static readonly double[] ArrayHappy = [1.0, 2.0, 3.0];
    private static readonly double[] ArrayHappyShifted = [1.0001, 2.0001, 3.0001];

    private static double D(double v) => v;
    private static long L(long v) => v;
}
