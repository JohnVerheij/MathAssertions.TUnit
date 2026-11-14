using System;
using System.Threading.Tasks;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// One row per generated assertion of the adapter: a failing fluent call (whose message is pinned in full by
/// <see cref="AdapterFailureTextTests"/>), a direct call of the declared static method that must accept
/// good input and a direct call that must reject bad input (the declared methods of the inlined assertions are
/// only reached this way, and are part of the public surface).
/// </summary>
/// <param name="Name">Unique name of the assertion overload.</param>
/// <param name="Fail">A fluent assertion that fails.</param>
/// <param name="DirectAccepts">Calls the declared method with input it accepts; returns whether it did.</param>
/// <param name="DirectRejects">Calls the declared method with input it rejects; returns whether it did.</param>
internal sealed record AdapterCase(string Name, Func<Task> Fail, Func<bool> DirectAccepts, Func<bool> DirectRejects);
