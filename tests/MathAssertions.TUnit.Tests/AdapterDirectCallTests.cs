using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Calls the declared static method behind every generated assertion directly. The fluent form reaches the
/// copy of the body that the generator inlines; the declared method is public surface of its own, and
/// these calls pin that it accepts good input and rejects bad input like the fluent form does.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class AdapterDirectCallTests
{
    /// <summary>The case table as test data, one test per overload.</summary>
    /// <returns>One name per row.</returns>
    public static IEnumerable<string> CaseNames()
    {
        foreach (var row in AdapterCases.All)
            yield return row.Name;
    }

    [Test]
    [MethodDataSource(nameof(CaseNames))]
    public async Task DeclaredMethod_AcceptsGoodInput(string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var row = AdapterCases.Find(name);
        await Assert.That(row.DirectAccepts()).IsTrue();
    }

    [Test]
    [MethodDataSource(nameof(CaseNames))]
    public async Task DeclaredMethod_RejectsBadInput(string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var row = AdapterCases.Find(name);
        await Assert.That(row.DirectRejects()).IsTrue();
    }
}
