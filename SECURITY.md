# Security Policy

## Supported versions

Only the latest published version of `MathAssertions.TUnit` receives fixes. Earlier versions are not supported.

| Version | Supported |
|---------|-----------|
| latest  | ✅        |
| older   | ❌        |

## Reporting a vulnerability

If you discover a security vulnerability, **please do not open a public GitHub issue.** Instead, report it privately via [GitHub's private security reporting](https://github.com/JohnVerheij/MathAssertions.TUnit/security/advisories/new).

Reports are acknowledged within seven days. After a fix is prepared, a coordinated disclosure timeline is agreed with the reporter before public release.

## Scope

This package is a TUnit-targeting test-only library. Realistic attack surface is small: it reads a `TimeSpan` budget and a `TimeProvider`, then compares those against the wall-clock duration captured by TUnit's evaluator. Issues that may qualify:

- Unbounded memory or CPU consumption from a crafted assertion chain
- Information disclosure through assertion failure messages that escapes intended scope
- Supply-chain concerns about the package itself

Issues that do not qualify:

- Bugs in dependent packages (TUnit); report those upstream
- Issues in test-runner integration that are TUnit-side
