using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Assertions.Exceptions;

namespace MathAssertions.TUnit.Tests;

/// <summary>
/// Pins the full failure text of every generated assertion of the adapter, one test per overload, so a change
/// to an expectation message or to a rendered failure (delta lines, wrapped difference, per-element rows) shows
/// up as a diff. The text is the assertion's own message: the part before the call-site line that TUnit appends.
/// </summary>
[Category("Smoke")]
[Timeout(5_000)]
internal sealed class AdapterFailureTextTests
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
    public async Task FailureText_IsPinned(string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // TUnit formats the placeholders of its own text with the current culture (0,1 on a Dutch machine). The
        // change stays inside this test's async flow, so parallel tests are not affected.
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var row = AdapterCases.Find(name);
        AssertionException? ex = await Assert.That(row.Fail).Throws<AssertionException>();
        var text = OwnMessage(ex!.Message);

        await Assert.That(text).IsEqualTo(AdapterFailureTexts.Expected.TryGetValue(name, out var expected) ? expected : "<not pinned>");
    }

    /// <summary>Removes the call-site line TUnit appends and normalizes line endings to LF.</summary>
    /// <param name="message">The exception message.</param>
    /// <returns>The assertion's own message.</returns>
    private static string OwnMessage(string message)
    {
        var text = message.ReplaceLineEndings("\n");
        var callSite = text.LastIndexOf("\n\nat Assert.", StringComparison.Ordinal);
        return callSite < 0 ? text : text[..callSite];
    }
}
