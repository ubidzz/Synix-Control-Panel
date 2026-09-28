# Built-in Game Definition Library

Each supported game has its own folder and `.game.json` definition. Synix embeds these files into its assembly during compilation, so the release does not load game definitions, plugins, scripts, or DLLs from the user's computer.

## Adding a game

1. Open **Settings > Development > Built-in Game Definitions** in a development build.
2. Use **Definition Builder** or copy `GameDefinition.template.json` as a starting layout.
3. Give the definition a unique lowercase `id`, unique nonnegative `catalogOrder`, and `definitionRevision`. Existing order values remain stable when definitions are removed.
4. Add the executable, Steam AppID, launch arguments, ports, maps, modes, probing behavior, and configuration information.
5. If Synix creates the configuration or manages a complete game-generated configuration after first start, add every complete game-provided template under the game's `Templates` folder and give each template its exact installed-server location.
6. Run **Validate Library**, build Synix, and run the automated tests.

The Definition Builder accepts one or many configuration files. Use **Add files** for every additional file, then edit its **Installed location** in the table. Locations are relative to the installed server folder; for example, `Saved\Config\WindowsServer\Game.ini`. Synix rejects missing files, duplicate destinations, duplicate embedded filenames, and paths that escape the server folder.

Most Steam applications leave **SteamCMD app configuration** blank. Shared GoldSrc AppID 90 packages use the exact allowlisted form `90 mod folder`, such as `90 mod cstrike`. Synix places this selection before `app_update` and rejects commands that do not match the safe format.

**Steam branch** (`steamBranch`) is optional. The Isle uses `public` for Legacy and `evrima` for Evrima with the same dedicated-server AppID 412680. Synix passes the selected branch to `app_update -beta` during both installation and updates. A branch is a single, case-sensitive name, not a command: only letters, digits, underscores, dots and hyphens are accepted (maximum 64 characters; no leading dot or hyphen). Password-protected branches are not supported by this field.

The download AppID is not necessarily the store game's ID, and it is not a unique game key. Keep shared IDs when Steam distributes several games through one dedicated-server package, and supply the required selector or branch. Terraria and Stormworks bundle their current dedicated-server executables with the owned game download; those entries deliberately use the game package and require Steam login. Do not substitute a player-client ID unless the package actually contains the required dedicated server.

See [Steam download audit](Steam-Download-Audit.md) and `steam-download-audit.json` for the 2026-09-29 catalog review, removed installer entries, evidence links and limitations. Review SteamDB depots as well as the application title: a server-named Tool can contain only a placeholder or Linux files. Metadata review is not a successful live installation test.

Keep `gameModes` friendly for the user. For games that show PVP/PVE but require another launch or configuration value, set `pvpValue` and `pveValue` to the exact values the server accepts, such as `False`/`True` or `0`/`1`. Set `booleanTrueValue` and `booleanFalseValue` to the exact boolean representation used by the game's configuration and RCON settings. Synix validates these as single safe values before they can reach a server process or configuration file.

Special hardware and launch behavior belongs in `runtimeRequirements` and `launchBehavior`, not in game-name checks. Definitions can require minimum system RAM, AVX2, hardware virtualization, Hyper-V, a supported Windows edition, .NET Framework 4.8/4.8.1, and allowlisted Microsoft Visual C++ x64 runtimes. They can also request an elevated launch, force a required server-manager window to remain visible, select external lifecycle tracking, disable generated launch-file export, and provide a ready message. These values are validated before the game enters the catalog. Synix checks declared prerequisites and occupied server ports before launch but does not silently install Windows components or runtimes.

Use `minimumServerPasswordLength` and `serverPasswordMustNotAppearInName` for game-enforced password rules. Synix applies these rules while editing a server and again immediately before launch, so an older saved entry cannot start with settings the game will reject.

Games that require a third-party online credential may set `requiresAuthenticationToken`, provide a user-facing `authenticationTokenLabel` and HTTPS `authenticationTokenHelpUrl`, and place `{auth_token}` in their launch arguments. Synix then shows the token field only for that game, protects the saved value with the current Windows user account, and redacts it from Synix previews and logs. A user-requested batch export intentionally contains the usable token and passwords in readable text so the batch file can launch without Synix; the export flow warns the user before writing it.

`controlCapabilities` selects the built-in controller family for lifecycle,
console, configuration, and player operations. Ordinary games should keep the
`Standard`, `None`, `Generic`, and `QueryProtocol` defaults. Minecraft declares
its four Minecraft controllers in its definition. This keeps controller
selection out of normal game-name switches while preserving a fixed, compiled,
security-reviewed implementation. A definition cannot name a class, assembly,
script, or arbitrary controller, and incompatible controller combinations are
rejected during catalog validation.

Complete templates may use either `SynixTemplate` or `GameGenerated`. `SynixTemplate` writes the complete file before first start. `GameGenerated` is for an official complete configuration captured after the server creates it; Synix then uses that built-in copy for managed values, validation, repair, and full reset. In both modes, placeholders automatically expose matching common fields in Server Settings without another game-name switch.

Use `{HasPassword}` when a configuration has a separate password-enabled field. It expands to the definition's `booleanTrueValue` when `{Password}` is not empty and `booleanFalseValue` when it is empty.

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
- Set `crossplayDisablesPlayerTracking` when enabling Crossplay switches the game away from its query protocol. Synix then disables player counts and player management only while that server's Crossplay switch is on.

## Safe post-install actions

Definitions may declare only these built-in actions:

- `CopySteamRuntimeFiles` copies the fixed allowlist `steamclient64.dll`, `tier0_s64.dll`, and `vstdlib_s64.dll` from Synix's SteamCMD folder.
- `EnsureDirectory` creates a relative directory inside the selected server installation.

All targets must remain inside the server folder. Definitions cannot run PowerShell, batch files, command prompts, arbitrary programs, downloaded code, plugins, or custom C# handlers.

## Validation

The development validator checks every source definition for:

- Strict JSON fields and supported schema versions.
- Explicit definition and template revisions.
- Unique IDs, game names, aliases, and nonnegative catalog order values.
- Safe relative executable, configuration, template, and post-install paths.
- Existing complete template files and supported placeholders.
- HTTPS-only download and icon addresses.
- Allowlisted declarative post-install actions.
- Declarative hardware and Windows runtime requirements, visible/elevated/external launch behavior, and supported server frameworks.
- Valid built-in control capabilities and compatible controller combinations.

The same validation is included in the Release Readiness Checker, so an invalid game definition blocks a release.

## International server titles and passwords

`game-text-support.json` records a separate online search for every current built-in game. It is embedded into the app and available from **General → Title characters** and **Security → Name & password character support**. The details window also lets users select any other game in the catalog.

- Title, join-password, administrator-password and RCON-password evidence is independent. Translated game menus, a UTF-8 configuration file or a shared engine are not sufficient evidence for all four fields.
- `Documented` is backed by a developer source; `Conditional` explains version, encoding or client limitations; `Restricted` describes a known constraint; `NotApplicable` is scoped to the built-in feature, not plugins. Every non-unverified record requires a source and an explanatory note.
- `Unverified` means the individual online search did not establish support. It does **not** impose an ASCII-only input filter or claim that the game cannot use international text. Missing/new game records also remain unverified.
- `searchedOn` and `searchQuery` record research coverage, not live-server testing. Detailed evidence notes are maintained in English; user-facing labels are localized. Recheck sources when game versions change.
- Synix preserves complete Unicode input without normalizing passwords. Malformed surrogate characters are rejected with a field-only error. Existing native-format, token and game-specific validation remains in effect.
- Exported Windows launch files use UTF-8 and select that code page before any names, paths or passwords are read. Managed Java Minecraft properties use Unicode escapes compatible with both legacy Latin-1 and modern UTF-8 readers; Bedrock keeps its native text format.
- Tests cover catalog coverage/evidence validation, international inputs, protected-password storage, ARK configuration synchronization, Java/Bedrock properties, UTF-8 RCON packet lengths, launch arguments, batch export and the localized guidance layout. These are Synix tests, not certification that every game/client accepts every script or emoji.

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
