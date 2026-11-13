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
}
