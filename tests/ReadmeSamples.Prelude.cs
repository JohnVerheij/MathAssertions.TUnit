// What the README samples use without declaring it: the reader's own types and test data. CI compiles the samples
// with this file (assertions-ci, README code samples); it is not part of any project here.
global using System.Numerics;

namespace ReadmeSamples;

/// <summary>A generated protobuf position message of the reader (the protobuf pose sample).</summary>
public sealed class MyProtoPosition
{
    /// <summary>The X coordinate.</summary>
    public double X { get; set; }

    /// <summary>The Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>The Z coordinate.</summary>
    public double Z { get; set; }
}

/// <summary>A generated protobuf orientation message of the reader (the protobuf pose sample).</summary>
public sealed class MyProtoOrientation
{
    /// <summary>The X component.</summary>
    public double X { get; set; }

    /// <summary>The Y component.</summary>
    public double Y { get; set; }

    /// <summary>The Z component.</summary>
    public double Z { get; set; }

    /// <summary>The W component.</summary>
    public double W { get; set; }
}

/// <summary>A generated protobuf pose message of the reader, with its parser (the protobuf pose sample).</summary>
public sealed class MyProtoPose
{
    /// <summary>The generated parser.</summary>
    public static MyProtoPoseParser Parser { get; } = new();

    /// <summary>The position.</summary>
    public MyProtoPosition Position { get; set; } = new();

    /// <summary>The orientation.</summary>
    public MyProtoOrientation Orientation { get; set; } = new();
}

/// <summary>The generated parser of <see cref="MyProtoPose"/>.</summary>
public sealed class MyProtoPoseParser
{
    /// <summary>Parses a message.</summary>
    public MyProtoPose ParseFrom(byte[] data) => new();
}

/// <summary>A pose in another shape than System.Numerics (the "Projecting a non-System.Numerics pose" sample).</summary>
public sealed class ProtoPose
{
    /// <summary>The position.</summary>
    public MyProtoPosition Position { get; set; } = new();

    /// <summary>The orientation.</summary>
    public MyProtoOrientation Orientation { get; set; } = new();

    /// <summary>
    /// Lets that sample pass the prelude's <c>actual</c> and <c>expected</c>, which the other samples need as a
    /// <see cref="Vector3"/>; a reader's own type has no such conversion.
    /// </summary>
    public static implicit operator ProtoPose(Vector3 value) => new();
}

/// <summary>A domain type of the reader (the "extend the DSL to your own types" samples).</summary>
public sealed record MyPosition(float X, float Y, float Z)
{
    /// <summary>
    /// Lets the domain-type sample pass the prelude's <c>expectedPosition</c>, which the protobuf pose sample needs as
    /// a <see cref="Vector3"/>; a reader's own type has no such conversion.
    /// </summary>
    public static implicit operator MyPosition(Vector3 value) => new(value.X, value.Y, value.Z);

    /// <summary>The position as a <see cref="Vector3"/>.</summary>
    public Vector3 AsVector3() => new(X, Y, Z);
}

/// <summary>The test data the samples use without declaring it; every sample class derives from it.</summary>
public partial class Prelude
{
    /// <summary>A serialized protobuf pose.</summary>
    protected byte[] payload = [];

    /// <summary>The expected position.</summary>
    protected Vector3 expectedPosition = Vector3.Zero;

    /// <summary>The expected orientation.</summary>
    protected Quaternion expectedOrientation = Quaternion.Identity;

    /// <summary>The position under test in the domain-type sample.</summary>
    protected MyPosition actualPosition = new(0, 0, 0);

    /// <summary>The input of the reader's computation.</summary>
    protected object input = new();

    /// <summary>A received message.</summary>
    protected object message = new();

    /// <summary>A computed vector.</summary>
    protected Vector3 actual = Vector3.Zero;

    /// <summary>An expected vector.</summary>
    protected Vector3 expected = Vector3.Zero;

    /// <summary>A target position.</summary>
    protected Vector3 target = Vector3.UnitX;

    /// <summary>A per-component tolerance.</summary>
    protected float tolerance = 1e-3f;

    /// <summary>Two points.</summary>
    protected Vector3 a = Vector3.Zero, b = Vector3.Zero;

    /// <summary>Directions.</summary>
    protected Vector3 forward = Vector3.UnitY, axis = Vector3.UnitZ, direction = Vector3.UnitX;

    /// <summary>An orientation.</summary>
    protected Quaternion orientation = Quaternion.Identity;

    /// <summary>A yaw angle in degrees.</summary>
    protected float yawDegrees = 179.99f;

    /// <summary>The expected yaw angle in degrees.</summary>
    protected float expectedYawDegrees = -180f;

    /// <summary>Rigid transforms.</summary>
    protected Matrix4x4 actualTransform = Matrix4x4.Identity, expectedTransform = Matrix4x4.Identity;

    /// <summary>A Unix time in milliseconds.</summary>
    protected double epochMs = 1_700_000_000_000;

    /// <summary>The reader's trajectory solver.</summary>
    protected static Vector3 SolveTrajectory(object input) => Vector3.UnitX;

    /// <summary>The reader's orientation solver.</summary>
    protected static Quaternion SolveOrientation(object input) => Quaternion.Identity;

    /// <summary>The reader's pose computation.</summary>
    protected static (Vector3 Position, Quaternion Orientation) ComputePose(object input) => (Vector3.Zero, Quaternion.Identity);

    /// <summary>The reader's ground-plane fit.</summary>
    protected static Plane ComputeGroundPlane(object input) => new(Vector3.UnitY, 0);

    /// <summary>The reader's message decoding.</summary>
    protected static Quaternion ReadRotationFromMessage(object message) => Quaternion.Identity;
}
