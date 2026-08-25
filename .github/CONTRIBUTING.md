# Contributing to Synix Control Panel

Thank you for helping improve Synix. Useful bug reports, game-compatibility findings, documentation corrections, and focused pull requests are welcome.

## Before contributing

Synix is **source-available software for personal, non-commercial use**. It is not distributed under a general open-source license. Read [LICENSE.md](../LICENSE.md) before using, modifying, or contributing code.

Submitting a contribution does not grant permission to redistribute, rebrand, sell, mirror, or publish modified Synix builds. Contributions accepted into the official repository are governed by the contribution terms in the Synix license.

## Reporting a problem

1. Update to the latest stable Synix release when practical.
2. Search existing issues to avoid duplicate reports.
3. Choose the issue form that best matches the problem.
4. Include the Synix version, Windows version, affected server definition, and clear reproduction steps.
5. Remove passwords, tokens, webhooks, public IP addresses, private configuration, and secret-bearing commands.

Security vulnerabilities must follow the [security policy](SECURITY.md) and must not be posted publicly.

## Game compatibility reports

Synix contains many built-in game definitions, and not every definition is fully verified. A useful compatibility report identifies the exact stage that was tested:

- Installation
- Start
- Stop
- Monitoring
- Launch arguments
- Configuration generation or editing

When possible, link to an official server manual, developer wiki, or other authoritative source for required arguments and configuration fields.

## Pull requests

Keep each pull request focused on one problem or closely related set of changes.

- Explain what changed and why.
- Preserve Synix's local-first, low-dependency design.
- Do not introduce plugin loading, remote code execution, hidden commands, telemetry, advertising, or a hosted-service requirement.
- Do not commit build output, user data, server files, credentials, generated packages, or private paths.
- Keep passwords and other secrets out of logs, screenshots, tests, and examples.
- Update or add automated tests when behavior changes.
- Confirm that the solution builds before requesting review.
- Do not remove copyright or license notices.

The project owner may revise, decline, or delay a contribution when it conflicts with Synix's security, usability, licensing, or maintenance goals.

## Development requirements

- Windows 10 or Windows 11
- Visual Studio with .NET desktop development support, or the .NET 10 SDK
- The `Synix Control Panel.sln` solution

Run the automated test project before submitting a pull request:

```powershell
dotnet test "Synix Control Panel.sln" -c Release
```

## Community behavior

Be respectful, patient, and specific. Follow the [Synix Code of Conduct](CODE_OF_CONDUCT.md) when participating in issues, discussions, and pull requests.

