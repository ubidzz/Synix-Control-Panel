# Built-in Game Definition Library

Each supported game has its own folder and `.game.json` definition. Synix embeds these files into its assembly during compilation, so the release does not load game definitions, plugins, scripts, or DLLs from the user's computer.

## Adding a game

1. Create a folder using a unique lowercase game ID.
2. Add `<game-id>.game.json` using an existing definition as the starting layout.
3. Give the definition a unique `id` and `catalogOrder`.
4. Add the executable, Steam AppID, launch arguments, ports, maps, modes, probing behavior, and configuration information.
5. If Synix must create the configuration file, add the complete template under the game's `Templates` folder and reference it from the definition.
6. Build Synix and run the automated tests.

## Security boundaries

- Definitions and templates are trusted source files compiled into Synix.
- Synix does not scan user folders for additional definitions.
- Definitions cannot name a plugin, assembly, type, script, or command handler.
- Unknown JSON properties are rejected.
- Executable and configuration paths must be relative and cannot escape the server folder.
- Download and icon URLs must use HTTPS.
- Only configuration handlers compiled into the Synix assembly can be discovered.
- Changing the library requires rebuilding Synix.

