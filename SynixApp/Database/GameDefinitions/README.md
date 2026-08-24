# Built-in Game Definition Library

Each supported game has its own folder and `.game.json` definition. Synix embeds these files into its assembly during compilation, so the release does not load game definitions, plugins, scripts, or DLLs from the user's computer.

## Adding a game

1. Open **Settings > Development > Built-in Game Definitions** in a development build.
2. Use **Definition Builder** or copy `GameDefinition.template.json` as a starting layout.
3. Give the definition a unique lowercase `id`, continuous `catalogOrder`, and `definitionRevision`.
4. Add the executable, Steam AppID, launch arguments, ports, maps, modes, probing behavior, and configuration information.
5. If Synix creates the configuration or manages a complete game-generated configuration after first start, add every complete game-provided template under the game's `Templates` folder and give each template its exact installed-server location.
6. Run **Validate Library**, build Synix, and run the automated tests.

The Definition Builder accepts one or many configuration files. Use **Add files** for every additional file, then edit its **Installed location** in the table. Locations are relative to the installed server folder; for example, `Saved\Config\WindowsServer\Game.ini`. Synix rejects missing files, duplicate destinations, duplicate embedded filenames, and paths that escape the server folder.

Most Steam applications leave **SteamCMD app configuration** blank. Shared GoldSrc AppID 90 packages use the exact allowlisted form `90 mod folder`, such as `90 mod cstrike`. Synix places this selection before `app_update` and rejects commands that do not match the safe format.

Keep `gameModes` friendly for the user. For games that show PVP/PVE but require another launch or configuration value, set `pvpValue` and `pveValue` to the exact values the server accepts, such as `False`/`True` or `0`/`1`. Set `booleanTrueValue` and `booleanFalseValue` to the exact boolean representation used by the game's configuration and RCON settings. Synix validates these as single safe values before they can reach a server process or configuration file.

Special hardware and launch behavior belongs in `runtimeRequirements` and `launchBehavior`, not in game-name checks. Definitions can require minimum system RAM, AVX2, hardware virtualization, Hyper-V, a supported Windows edition, .NET Framework 4.8/4.8.1, and allowlisted Microsoft Visual C++ x64 runtimes. They can also request an elevated launch, force a required server-manager window to remain visible, select external lifecycle tracking, disable generated launch-file export, and provide a ready message. These values are validated before the game enters the catalog. Synix checks declared prerequisites and occupied server ports before launch but does not silently install Windows components or runtimes.

Complete templates may use either `SynixTemplate` or `GameGenerated`. `SynixTemplate` writes the complete file before first start. `GameGenerated` is for an official complete configuration captured after the server creates it; Synix then uses that built-in copy for managed values, validation, repair, and full reset. In both modes, placeholders automatically expose matching common fields in Server Settings without another game-name switch.

`supportedServerFrameworks` exposes only frameworks with a fixed implementation compiled into Synix. Rust may list `Oxide`. Synix downloads only the official Windows Oxide.Rust release, requires GitHub's published SHA-256 digest, performs guarded extraction with rollback, and reapplies Oxide after Rust updates. Plugins are never installed or managed by Synix and remain the user's responsibility.

The builder is a development tool. It writes validated source files into this project but cannot add games to an already released Synix executable.

## Revisions and safe upgrades

- `definitionRevision` tracks changes to the whole game definition.
- `configuration.revision` tracks the current managed configuration layout.
- Every configuration template has its own `revision` and cannot be newer than the containing configuration revision.
- Raise the configuration revision only when the managed template layout changes.
- Before applying a newer template revision to an existing server, Synix creates a one-time `.synix.before-template-v<revision>.bak` copy.
- Synix updates only settings represented by its placeholders. Unrecognized game settings and user customizations remain in the live file.
- Missing required tags are reported for repair instead of silently replacing the user's configuration.

## Safe post-install actions

Definitions may declare only these built-in actions:

- `CopySteamRuntimeFiles` copies the fixed allowlist `steamclient64.dll`, `tier0_s64.dll`, and `vstdlib_s64.dll` from Synix's SteamCMD folder.
- `EnsureDirectory` creates a relative directory inside the selected server installation.

All targets must remain inside the server folder. Definitions cannot run PowerShell, batch files, command prompts, arbitrary programs, downloaded code, plugins, or custom C# handlers.

## Validation

The development validator checks every source definition for:

- Strict JSON fields and supported schema versions.
- Explicit definition and template revisions.
- Unique IDs, game names, aliases, and continuous catalog order.
- Safe relative executable, configuration, template, and post-install paths.
- Existing complete template files and supported placeholders.
- HTTPS-only download and icon addresses.
- Allowlisted declarative post-install actions.
- Declarative hardware and Windows runtime requirements, visible/elevated/external launch behavior, and supported server frameworks.

The same validation is included in the Release Readiness Checker, so an invalid game definition blocks a release.

## Security boundaries

- Definitions and templates are trusted source files compiled into Synix.
- Synix does not scan user folders for additional definitions.
- Definitions cannot name a plugin, assembly, type, script, or command handler.
- Unknown JSON properties are rejected.
- Executable and configuration paths must be relative and cannot escape the server folder.
- Download and icon URLs must use HTTPS.
- Only configuration handlers compiled into the Synix assembly can be discovered.
- Post-install actions are interpreted by a fixed allowlist and never execute definition-provided code.
- Changing the library requires rebuilding Synix.
