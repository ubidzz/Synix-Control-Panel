# Synix interface languages

All interface-language files are kept in this folder.

- `Strings.resx` is the neutral English catalog and the runtime fallback.
- `Strings.fr.resx` is the compiled French catalog.
- `Strings.de.resx` is the compiled German catalog.
- `Strings.es.resx` is the compiled Spanish catalog.
- `GenerateEnglishResources.ps1` refreshes the English catalog from visible
  control text and keeps the named resources used by dynamic text and lists.
- `GenerateFrenchResources.ps1` contains the French translations in one place
  and rebuilds `Strings.fr.resx`.
- `GenerateGermanResources.ps1` and `GenerateSpanishResources.ps1` keep the
  German and Spanish translations organized in the same folder.
- `SemanticResources.<culture>.ps1` keeps the named text used by dynamic
  windows, dialogs, validation summaries, and accessibility labels in one
  reviewable file per language.
- `OperationalTranslations.<culture>.ps1` contains changing status text and
  dialog wording that is assembled around server names, ports, paths, counts,
  and other values that must remain unchanged.
- `OperationalTranslations.en.ps1` pins the matching English source fragments
  so resource regeneration cannot drop translations used by runtime reports.

Additional compiled `Strings.<culture>.resx` catalogs are discovered at
build time and appear in the language selector automatically, including in
Synix's single-EXE release. No C# language registration is required. Each
catalog should contain the same resource keys as `Strings.resx`; untranslated
entries safely fall back to English.

Run the English generator first, followed by each language generator, after
adding or changing visible text. Localization tests fail when a named runtime
resource or visible interface string is missing from any supported language.
English, French, German, and Spanish cover the shared operational interface,
including specialist, developer, warning, server-management, activity, and
validation text. The Help Center and technical text-log/support-report contents
intentionally remain in English.

Only Synix interface text is translated. Game identifiers, map and mode values,
console commands, configuration keys and values, launch arguments, ports,
tokens, file names, protocol identifiers, and output-matching markers remain
unchanged so a language change cannot alter how a game server operates.
