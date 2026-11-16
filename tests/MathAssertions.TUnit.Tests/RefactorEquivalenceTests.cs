using System;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;
using MathAssertions.Geometry3D;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Property tests for the complexity refactor: the methods that were split into helpers must return exactly what
/// the code they replaced returned. The previous implementations are kept here as private copies and compared
/// against the shipped ones on edge values and on deterministic pseudo-random input.
/// </summary>
[Category("Smoke")]
[Timeout(30_000)]
internal sealed class RefactorEquivalenceTests
{
    private static readonly double[] DoubleEdges =
    [
        0.0, -0.0, 1.0, -1.0, 0.5, 2.0, 1e-9, -1e-9, 1e-300, -1e-300, 1e300, -1e300, double.Epsilon, -double.Epsilon,
        double.MaxValue, -double.MaxValue, double.MinValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity,
        1.0000000000000002, 0.9999999999999999, 4.94e-324, 2.2250738585072014E-308,
    ];

    private static readonly double[] DoubleTolerances = [0.0, 1e-12, 1e-6, 0.5, 1.0, 1e10, double.MaxValue, double.PositiveInfinity];

    private static readonly double[] FiniteDoubleTolerances = [0.0, 1e-12, 1e-6, 0.5, 1.0, 1e10, double.MaxValue];

    private static readonly float[] FloatEdges =
    [
        0f, -0f, 1f, -1f, 0.5f, 1e-9f, -1e-9f, 1e30f, -1e30f, float.Epsilon, -float.Epsilon, float.MaxValue, -float.MaxValue,
        float.NaN, float.PositiveInfinity, float.NegativeInfinity, 1.0000001f, 0.99999994f,
    ];

    private static readonly float[] FloatTolerances = [0f, 1e-6f, 0.5f, 1f, 1e10f, float.MaxValue, float.PositiveInfinity];

    private static readonly nint[] Shape1 = [(nint)1];
    private static readonly nint[] Shape4 = [(nint)4];

    // ----- IsApproximatelyEqual (double and float) -----

    [Test]
    public async Task IsApproximatelyEqual_Double_MatchesThePreviousImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var mismatches = 0;
        var checkedPairs = 0;
        foreach (var (a, b) in DoublePairs(new SplitMix(1)))
        {
            foreach (var tolerance in DoubleTolerances)
            {
                checkedPairs++;
                if (MathTolerance.IsApproximatelyEqual(a, b, tolerance) != OldIsApproximatelyEqual(a, b, tolerance))
                    mismatches++;
            }
        }
        await Assert.That(checkedPairs).IsGreaterThan(10_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    [Test]
    public async Task IsApproximatelyEqual_Float_MatchesThePreviousImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var mismatches = 0;
        var checkedPairs = 0;
        foreach (var (a, b) in FloatPairs(new SplitMix(2)))
        {
            foreach (var tolerance in FloatTolerances)
            {
                checkedPairs++;
                if (MathTolerance.IsApproximatelyEqual(a, b, tolerance) != OldIsApproximatelyEqual(a, b, tolerance))
                    mismatches++;
            }
        }
        await Assert.That(checkedPairs).IsGreaterThan(10_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- IsCloseInUlps (double and float) -----

    [Test]
    public async Task IsCloseInUlps_Double_MatchesThePreviousImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        long[] distances = [0, 1, 2, 3, 16, 1_000_000, long.MaxValue];
        var mismatches = 0;
        var checkedPairs = 0;
        foreach (var (a, b) in DoublePairs(new SplitMix(3)))
        {
            foreach (var distance in distances)
            {
                checkedPairs++;
                if (MathTolerance.IsCloseInUlps(a, b, distance) != OldIsCloseInUlps(a, b, distance))
                    mismatches++;
            }
        }
        // Adjacent values on both sides of zero and of the exponent boundaries, and the zero pair.
        foreach (var start in new[] { 0.0, -0.0, 1.0, -1.0, double.Epsilon, -double.Epsilon, double.MaxValue, 2.0 })
        {
            var next = Math.BitIncrement(start);
            var previous = Math.BitDecrement(start);
            foreach (var distance in distances)
            {
                checkedPairs += 2;
                if (MathTolerance.IsCloseInUlps(start, next, distance) != OldIsCloseInUlps(start, next, distance))
                    mismatches++;
                if (MathTolerance.IsCloseInUlps(start, previous, distance) != OldIsCloseInUlps(start, previous, distance))
                    mismatches++;
            }
        }
        await Assert.That(checkedPairs).IsGreaterThan(10_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    [Test]
    public async Task IsCloseInUlps_Float_MatchesThePreviousImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        int[] distances = [0, 1, 2, 3, 16, 1_000_000, int.MaxValue];
        var mismatches = 0;
        var checkedPairs = 0;
        foreach (var (a, b) in FloatPairs(new SplitMix(4)))
        {
            foreach (var distance in distances)
            {
                checkedPairs++;
                if (MathTolerance.IsCloseInUlps(a, b, distance) != OldIsCloseInUlps(a, b, distance))
                    mismatches++;
            }
        }
        foreach (var start in new[] { 0f, -0f, 1f, -1f, float.Epsilon, -float.Epsilon, float.MaxValue, 2f })
        {
            var next = MathF.BitIncrement(start);
            var previous = MathF.BitDecrement(start);
            foreach (var distance in distances)
            {
                checkedPairs += 2;
                if (MathTolerance.IsCloseInUlps(start, next, distance) != OldIsCloseInUlps(start, next, distance))
                    mismatches++;
                if (MathTolerance.IsCloseInUlps(start, previous, distance) != OldIsCloseInUlps(start, previous, distance))
                    mismatches++;
            }
        }
        await Assert.That(checkedPairs).IsGreaterThan(10_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- Tensor overload -----

    [Test]
    public async Task IsApproximatelyEqual_TensorDouble_MatchesThePreviousImplementationUnderFiniteTolerance(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var mismatches = 0;
        var checkedPairs = 0;
        foreach (var (a, b) in DoublePairs(new SplitMix(5)))
        {
            // Under a tolerance of positive infinity the previous tensor code differed from the scalar rules; the
            // rule change is pinned separately. Every other tolerance must agree element for element.
            foreach (var tolerance in FiniteDoubleTolerances)
            {
                checkedPairs++;
                var left = new[] { a };
                var right = new[] { b };
                var shipped = MathTolerance.IsApproximatelyEqual(
                    new ReadOnlyTensorSpan<double>(left, Shape1, default), new ReadOnlyTensorSpan<double>(right, Shape1, default), tolerance);
                var previous = OldTensorIsApproximatelyEqual(
                    new ReadOnlyTensorSpan<double>(left, Shape1, default), new ReadOnlyTensorSpan<double>(right, Shape1, default), tolerance);
                if (shipped != previous)
                    mismatches++;
            }
        }
        await Assert.That(checkedPairs).IsGreaterThan(10_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    [Test]
    public async Task IsApproximatelyEqual_TensorInt_MatchesThePreviousImplementationWithoutOverflow(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var random = new SplitMix(6);
        var mismatches = 0;
        for (var n = 0; n < 5_000; n++)
        {
            // Values and tolerances small enough that the old subtraction cannot overflow.
            var left = new[] { random.NextInt(-1000, 1000), random.NextInt(-1000, 1000), random.NextInt(-5, 5), 0 };
            var right = new[] { random.NextInt(-1000, 1000), random.NextInt(-1000, 1000), random.NextInt(-5, 5), 0 };
            var tolerance = random.NextInt(0, 300);
            var shipped = MathTolerance.IsApproximatelyEqual(
                new ReadOnlyTensorSpan<int>(left, Shape4, default), new ReadOnlyTensorSpan<int>(right, Shape4, default), tolerance);
            var previous = OldTensorIsApproximatelyEqual(
                new ReadOnlyTensorSpan<int>(left, Shape4, default), new ReadOnlyTensorSpan<int>(right, Shape4, default), tolerance);
            if (shipped != previous)
                mismatches++;
        }
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- IsPrime -----

    [Test]
    public async Task IsPrime_MatchesThePreviousImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var mismatches = 0;
        for (long value = -10; value <= 60_000; value++)
        {
            if (NumberTheory.IsPrime(value) != OldIsPrime(value))
                mismatches++;
        }

        var random = new SplitMix(7);
        for (var n = 0; n < 400; n++)
        {
            var value = random.NextLong(60_000, 1_000_000_000_000L);
            if (NumberTheory.IsPrime(value) != OldIsPrime(value))
                mismatches++;
        }

        // Large primes and the largest values, which have a small factor and so return at once.
        long[] large = [1_000_000_007L, 2_147_483_647L, 4_294_967_311L, 999_999_999_989L, long.MaxValue, long.MaxValue - 1, long.MaxValue - 2, long.MinValue];
        mismatches += large.Count(value => NumberTheory.IsPrime(value) != OldIsPrime(value));
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- DistanceFrom(Vector3, Triangle3D) -----

    [Test]
    public async Task DistanceFrom_Triangle_AgreesWithThePreviousFloatImplementation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var random = new SplitMix(8);
        var mismatches = 0;
        var checkedCases = 0;
        for (var n = 0; n < 20_000; n++)
        {
            var scale = ScaleFor(n);
            var a = random.NextVector(scale);
            var b = n % 7 == 0 ? a : random.NextVector(scale);
            var c = n % 11 == 0 ? Vector3.Lerp(a, b, 0.5f) : random.NextVector(scale);
            var triangle = new Triangle3D(a, b, c);
            var point = n % 5 == 0 ? Vector3.Lerp(a, c, random.NextFloat()) : random.NextVector(scale * 2f);
            checkedCases++;
            // The shipped code computes in double, the previous code in float: the distances agree to float
            // precision (a fraction of a thousandth of the scale). A near-degenerate triangle (the collinear
            // third of the cases) is ill-conditioned: the float result there is rounding noise, so it is not compared.
            var actual = point.DistanceFrom(triangle);
            var previous = OldDistanceFrom(point, triangle);
            var nearDegenerate = Properties.IsDegenerate(triangle, 1e-5 * scale * scale);
            if (!nearDegenerate && !AgreesToFloatPrecision(actual, previous, scale))
                mismatches++;
        }

        // Every region of a fixed triangle: the three vertices, the three edges and the interior, from above and below.
        var fixedTriangle = new Triangle3D(Vector3.Zero, new Vector3(4f, 0f, 0f), new Vector3(0f, 3f, 0f));
        Vector3[] probes =
        [
            new(-1f, -1f, 0f), new(5f, -1f, 0f), new(-1f, 4f, 0f), new(2f, -1f, 1f), new(-1f, 1.5f, 1f), new(3f, 3f, -1f),
            new(1f, 1f, 2f), new(1f, 1f, -2f), new(0f, 0f, 0f), new(4f, 0f, 0f), new(0f, 3f, 0f), new(2f, 0f, 0f),
            new(float.NaN, 0f, 0f),
        ];
        foreach (var probe in probes)
        {
            checkedCases++;
            if (!AgreesToFloatPrecision(probe.DistanceFrom(fixedTriangle), OldDistanceFrom(probe, fixedTriangle), 4f))
                mismatches++;
        }
        await Assert.That(checkedCases).IsGreaterThan(20_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- AreCoplanar: unchanged where the area test and the length test agree -----

    [Test]
    public async Task AreCoplanar_AgreesWithThePreviousImplementationWhereThePreviousOneWasRight(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var random = new SplitMix(9);
        var mismatches = 0;
        var coplanarCases = 0;
        var offPlaneCases = 0;
        for (var n = 0; n < 4_000; n++)
        {
            var count = 4 + (n % 6);
            var points = new Vector3[count];
            var onPlane = n % 2 == 0;
            for (var i = 0; i < count; i++)
            {
                // Well separated points (coordinates between 1 and 11) so the cross product is large next to the tolerance.
                var p = random.NextVector(10f);
                points[i] = new Vector3(p.X + 11f, p.Y + 11f, onPlane ? 3f : p.Z + 11f);
            }
            var shipped = Properties.AreCoplanar(points, 1e-3);
            var previous = OldAreCoplanar(points, 1e-3);
            if (shipped != previous || shipped != onPlane)
                mismatches++;
            if (onPlane)
                coplanarCases++;
            else
                offPlaneCases++;
        }
        await Assert.That(coplanarCases).IsGreaterThan(1_000);
        await Assert.That(offPlaneCases).IsGreaterThan(1_000);
        await Assert.That(mismatches).IsEqualTo(0);
    }

    // ----- Helpers: input generators -----

    private static (double A, double B)[] DoublePairs(SplitMix random)
    {
        var pairs = new System.Collections.Generic.List<(double, double)>();
        foreach (var a in DoubleEdges)
        {
            foreach (var b in DoubleEdges)
                pairs.Add((a, b));
        }
        for (var n = 0; n < 600; n++)
        {
            var a = random.NextDoubleFromBits();
            pairs.Add((a, random.NextDoubleFromBits()));
            pairs.Add((a, Math.BitIncrement(a)));
            pairs.Add((a, a + (random.NextFloat() * 1e-9)));
            pairs.Add((a, -a));
        }
        return [.. pairs];
    }

    private static (float A, float B)[] FloatPairs(SplitMix random)
    {
        var pairs = new System.Collections.Generic.List<(float, float)>();
        foreach (var a in FloatEdges)
        {
            foreach (var b in FloatEdges)
                pairs.Add((a, b));
        }
        for (var n = 0; n < 600; n++)
        {
            var a = random.NextFloatFromBits();
            pairs.Add((a, random.NextFloatFromBits()));
            pairs.Add((a, MathF.BitIncrement(a)));
            pairs.Add((a, a + (random.NextFloat() * 1e-6f)));
            pairs.Add((a, -a));
        }
        return [.. pairs];
    }

    private static float ScaleFor(int n)
    {
        float[] scales = [1e-3f, 1f, 1e3f];
        return scales[n % scales.Length];
    }

    private static bool AgreesToFloatPrecision(double actual, double expected, float scale)
        => (double.IsNaN(actual) && double.IsNaN(expected))
            || Math.Abs(actual - expected) <= 1e-3 * scale;

    /// <summary>A small deterministic generator (SplitMix64), so every run sees the same inputs.</summary>
    private sealed class SplitMix(ulong seed)
    {
        private ulong _state = seed;

        public ulong Next()
        {
            _state += 0x9E3779B97F4A7C15UL;
            var z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public double NextDoubleFromBits() => BitConverter.UInt64BitsToDouble(Next());

        public float NextFloatFromBits() => BitConverter.UInt32BitsToSingle((uint)(Next() >> 32));

        public float NextFloat() => (Next() >> 40) / (float)(1 << 24);

        public int NextInt(int minInclusive, int maxExclusive) => minInclusive + (int)(Next() % (ulong)(maxExclusive - minInclusive));

        public long NextLong(long minInclusive, long maxExclusive) => minInclusive + (long)(Next() % (ulong)(maxExclusive - minInclusive));

        public Vector3 NextVector(float scale)
            => new(((NextFloat() * 2f) - 1f) * scale, ((NextFloat() * 2f) - 1f) * scale, ((NextFloat() * 2f) - 1f) * scale);
    }

    // ----- The previous implementations, verbatim -----

    private static bool OldIsApproximatelyEqual(double a, double b, double tolerance)
    {
        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;
        if (double.IsPositiveInfinity(a) && double.IsPositiveInfinity(b))
            return true;
        if (double.IsNegativeInfinity(a) && double.IsNegativeInfinity(b))
            return true;
        if (!double.IsFinite(a) || !double.IsFinite(b))
            return false;

        return Math.Abs(a - b) <= tolerance;
    }

    private static bool OldIsApproximatelyEqual(float a, float b, float tolerance)
    {
        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        if (float.IsNaN(a) || float.IsNaN(b))
            return false;
        if (float.IsPositiveInfinity(a) && float.IsPositiveInfinity(b))
            return true;
        if (float.IsNegativeInfinity(a) && float.IsNegativeInfinity(b))
            return true;
        if (!float.IsFinite(a) || !float.IsFinite(b))
            return false;

        // The difference is formed in double since the sweep: a float subtraction rounds, and the rounded
        // difference could meet the tolerance when the exact one is larger (see CorrectnessFixTests).
        double widenedA = a;
        return Math.Abs(widenedA - b) <= tolerance;
    }

    private static bool OldIsCloseInUlps(double a, double b, long ulpDistance)
    {
        if (double.IsNaN(a) && double.IsNaN(b))
            return true;
        if (double.IsNaN(a) || double.IsNaN(b))
            return false;

        var aBits = BitConverter.DoubleToInt64Bits(a);
        var bBits = BitConverter.DoubleToInt64Bits(b);

        const long MagnitudeMask = 0x7FFF_FFFF_FFFF_FFFFL;
        if ((aBits & MagnitudeMask) == 0 && (bBits & MagnitudeMask) == 0)
            return true;

        if (aBits == bBits)
            return true;

        if ((aBits < 0) != (bBits < 0))
            return false;

        return Math.Abs(aBits - bBits) <= ulpDistance;
    }

    private static bool OldIsCloseInUlps(float a, float b, int ulpDistance)
    {
        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        if (float.IsNaN(a) || float.IsNaN(b))
            return false;

        var aBits = BitConverter.SingleToInt32Bits(a);
        var bBits = BitConverter.SingleToInt32Bits(b);

        const int MagnitudeMask = 0x7FFF_FFFF;
        if ((aBits & MagnitudeMask) is 0 && (bBits & MagnitudeMask) is 0)
            return true;

        if (aBits == bBits)
            return true;

        if ((aBits < 0) != (bBits < 0))
            return false;

        return Math.Abs(aBits - bBits) <= ulpDistance;
    }

    private static bool OldTensorIsApproximatelyEqual<T>(ReadOnlyTensorSpan<T> a, ReadOnlyTensorSpan<T> b, T tolerance)
        where T : INumber<T>
    {
        var aLengths = a.Lengths;
        var bLengths = b.Lengths;
        if (aLengths.Length != bLengths.Length)
            return false;
        for (var i = 0; i < aLengths.Length; i++)
        {
            if (aLengths[i] != bLengths[i])
                return false;
        }

        var ae = a.GetEnumerator();
        var be = b.GetEnumerator();
        while (ae.MoveNext() && be.MoveNext())
        {
            var av = ae.Current;
            var bv = be.Current;
            if (T.IsNaN(av) && T.IsNaN(bv))
                continue;
            if (T.IsNaN(av) || T.IsNaN(bv))
                return false;
            if (T.Abs(av - bv) > tolerance)
                return false;
        }
        return true;
    }

    private static bool OldIsPrime(long value)
    {
        if (value < 2)
            return false;
        if (value == 2 || value == 3)
            return true;
        if (value % 2 == 0 || value % 3 == 0)
            return false;

        for (var i = 5L; i <= value / i; i += 6)
        {
            if (value % i == 0 || value % (i + 2) == 0)
                return false;
        }
        return true;
    }

    private static double OldDistanceFrom(Vector3 point, Triangle3D triangle)
    {
        var ab = triangle.B - triangle.A;
        var ac = triangle.C - triangle.A;
        var ap = point - triangle.A;
        var d1 = Vector3.Dot(ab, ap);
        var d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0)
            return Vector3.Distance(point, triangle.A);

        var bp = point - triangle.B;
        var d3 = Vector3.Dot(ab, bp);
        var d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3)
            return Vector3.Distance(point, triangle.B);

        var vc = (d1 * d4) - (d3 * d2);
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            var v = d1 / (d1 - d3);
            return Vector3.Distance(point, triangle.A + (v * ab));
        }

        var cp = point - triangle.C;
        var d5 = Vector3.Dot(ab, cp);
        var d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6)
            return Vector3.Distance(point, triangle.C);

        var vb = (d5 * d2) - (d1 * d6);
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            var w = d2 / (d2 - d6);
            return Vector3.Distance(point, triangle.A + (w * ac));
        }

        var va = (d3 * d6) - (d5 * d4);
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return Vector3.Distance(point, triangle.B + (w * (triangle.C - triangle.B)));
        }

        var denom = 1f / (va + vb + vc);
        var vBary = vb * denom;
        var wBary = vc * denom;
        var closest = triangle.A + (vBary * ab) + (wBary * ac);
        return Vector3.Distance(point, closest);
    }

    private static bool OldAreCoplanar(ReadOnlySpan<Vector3> points, double tolerance)
    {
        if (points.Length < 4)
            return true;

        var p0 = points[0];
        var normal = Vector3.Zero;
        var foundNormal = false;
        for (var i = 1; i < points.Length - 1 && !foundNormal; i++)
        {
            for (var j = i + 1; j < points.Length; j++)
            {
                var n = Vector3.Cross(points[i] - p0, points[j] - p0);
                double nLength = n.Length();
                if (nLength > tolerance)
                {
                    normal = Vector3.Normalize(n);
                    foundNormal = true;
                    break;
                }
            }
        }
        if (!foundNormal)
            return true;

        for (var i = 0; i < points.Length; i++)
        {
            double dot = Vector3.Dot(points[i] - p0, normal);
            var d = Math.Abs(dot);
            if (d > tolerance)
                return false;
        }
        return true;
    }
}
