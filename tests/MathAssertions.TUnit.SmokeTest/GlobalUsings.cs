// The smoke-test project deliberately uses <ImplicitUsings>disable</ImplicitUsings> so a
// failure to wire up these usings, or a future change that breaks the auto-discovery of
// MathAssertions.TUnit's [GenerateAssertion]-emitted entry points, surfaces as a build
// failure here rather than silently passing in our own test project (which lives in the
// MathAssertions.TUnit.Tests namespace and gets parent-namespace visibility for free).

global using System.Numerics;                        // Vector3
global using System.Threading;                       // CancellationToken
global using System.Threading.Tasks;                 // Task
