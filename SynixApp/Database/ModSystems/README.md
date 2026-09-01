# Synix mod-system profiles

The Mod & Plugin Manager is intentionally data-driven. It does not contain a database of individual mods, and adding support for a game should not require changing the manager UI or package engine.

## How the system stays maintainable

- A **profile** describes one game/framework combination.
- A **target** describes a folder, the file types allowed there, and whether Synix may import files.
- Synix scans the real server folders every time the manager opens or refreshes.
- Synix records only the files it installed. Files added by a user, a provider, or another tool remain visible but are never claimed as Synix-managed.
- Every replaced file receives a rollback copy before Synix changes it.
- Local packages pass a security gate before installation: strict type rules, safe archive paths, size limits, symbolic-link rejection, file-signature checks, SHA-256 binding, capability warnings for readable source plugins, and a Microsoft Defender custom scan when available.
- A security review is not a sandbox. Mods execute with the game server's Windows permissions, so users must still trust the source and should run servers as a standard Windows user.
- Steam Workshop and other provider-managed systems can use `ConfigurationIds` when the game documents configuration keys for an ordered ID list. Synix manages those small settings while the provider still owns downloads and updates.
- Provider ID systems use `ArgumentIds` when the game itself accepts an ordered ID list. For example, ARK: Survival Ascended uses CurseForge IDs in `-mods=` while ARK owns the download and update.
- ARK: Survival Evolved uses `ConfigurationIds`: Synix keeps `ActiveMods` and `[ModInstaller] ModIDS` synchronized and adds `-automanagedmods`; Steam/ARK install and update the Workshop content.
- Unknown games get a read-only fallback when Synix finds common folders such as `plugins`, `mods`, `oxide/plugins`, or `BepInEx/plugins`.
- Provider-installed content such as Workshop or CurseForge downloads cannot be pre-scanned by Synix. Profiles must say that clearly instead of showing a false safe status.

## Updating support without changing C#

Profiles compiled with Synix live in this folder and end with `.modsystem.json`. A newer profile can also be placed in:

`C:\Synix\SynixData\ModSystems`

External profiles are validated with the same strict rules as built-in profiles. An external profile with the same `id` replaces the built-in profile the next time the manager opens, so support rules can be updated independently of the main UI code.

## Schema version 1

The root object contains:

- `schemaVersion`: must be `1`.
- `profiles`: one or more profile objects.

A profile contains:

- `id`: stable lowercase identifier. Keep it unchanged across updates.
- `displayName`: friendly framework name shown to users.
- `description`: one plain-English sentence explaining the ownership model.
- `supportLevel`: `Managed` or `DetectedOnly`. Managed profiles can copy verified local packages or manage provider IDs while the provider owns downloads.
- `gameNames`: exact Synix catalog game names.
- `frameworkName`: framework name Synix can compare with the server settings; may be blank.
- `frameworkMarkers`: relative files or folders that prove the framework exists.
- `catalogUrl`: optional HTTPS page that users can open themselves. It is not a list of individual mods.
- `restartRequired`: whether the user should restart after a change.
- `targets`: one or more target objects.

A target contains:

- `id`: stable identifier unique inside the profile.
- `displayName`: friendly label such as `Loader mods`.
- `kind`: `Mod` or `Plugin`.
- `mode`: `FileImport`, `ArgumentIds`, `ConfigurationIds`, or `DetectionOnly`.
- `providerName`: provider or framework shown to users, such as `Steam Workshop`, `CurseForge`, or `uMod`.
- `relativePath`: folder below the server installation. Absolute paths and `..` are rejected. Leave blank for provider-ID modes.
- `allowedExtensions`: extensions Synix may scan or import.
- `markerPaths`: relative paths that make this target active.
- `frameworkNames`: server loader/framework names that make this target active.
- `allowArchives`: permits safe ZIP extraction; other archive formats are not accepted.
- `scanDirectories`: also show first-level folders as provider-managed items.
- `recursive`: scan supported files below subfolders.
- `argumentName`: for `ArgumentIds`, the exact launch option such as `-mods`.
- `maximumIds`: safety limit for a provider ID list.
- `requiredArguments`: optional safe launch flags that are added when a `ConfigurationIds` list is not empty.
- `idStores`: for `ConfigurationIds`, one or more INI destinations with a relative path, section, key, and storage style.

INI ID storage styles are:

- `Csv`: one ordered comma-separated value such as `ActiveMods=111,222`.
- `RepeatedKey`: one line per ID such as repeated `ModIDS=111` entries.

## Adding a new profile safely

1. Confirm the game’s official or framework documentation identifies the add-on folder.
2. Begin with `DetectedOnly` and verify discovery on a disposable test server.
3. List the smallest possible set of allowed file extensions.
4. Change to `Managed` only when copying those files or managing provider IDs is the documented installation method.
5. Use an ID mode only when the game documents the exact argument or configuration keys. The provider must remain responsible for downloading and updating content.
6. Add tests for matching, discovery, safe imports, rollback, and unsafe paths.

Never add an individual mod to this file. Provider integrations should consume provider metadata at runtime and hand a verified local package to the same package engine.
