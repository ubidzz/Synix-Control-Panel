# Synix interface languages

Synix ships compiled, offline catalogs for English, French, German, Spanish,
Simplified Chinese, Traditional Chinese, Japanese, Korean, Italian, Brazilian
Portuguese, Polish, Russian, Turkish, Dutch, Swedish, Czech, Ukrainian,
Indonesian, Vietnamese, Danish, Finnish, Norwegian Bokmål, Greek, Hungarian,
Romanian, Hindi, Thai, Bulgarian, and Croatian (29 language
options). Every selectable language must contain every entry in the neutral
English catalog; partial preview catalogs
are no longer used for these languages.

The language picker displays each language's own name. Regional aliases use
the corresponding available catalog; generic Norwegian (`no` / `no-NO`)
selects Bokmål. Nynorsk (`nn` / `nn-NO`) is a different language and is not
advertised as supported.

Coverage includes the dashboard, setup and review, settings, game workspaces,
mod and plugin management, configuration editing, validation and error
messages, tooltips, accessibility labels, first-start guidance, the offline
Help Center, and built-in game-support descriptions. The Help Center searches
both the selected language and the English source, so exact commands and
technical terms remain searchable.

## Files and maintenance

- `Strings.resx` contains the English source and remains the safe runtime
  fallback for unknown or future resources.
- `Strings.<culture>.resx` contains each translation. These catalogs are
  discovered at build time, including for the single-EXE release.
- `SemanticResources.<culture>.ps1` and
  `OperationalTranslations.<culture>.ps1` maintain named resources and
  dynamic interface fragments for the original language generators.
- `GenerateEnglishResources.ps1` refreshes extracted text while preserving
  named resources and `Content.*` entries. The French, German, and Spanish
  generators also preserve catalog entries outside their translation tables.

Use `LocalizationManager.Get` or a named binding for new interface text.
For Synix-authored offline articles and catalog descriptions, `GetContent`
uses a `Content.*` key derived from the first 20 hexadecimal SHA-256 digits of
the English UTF-8 text, with line endings normalized to LF. Add that key and
its translations to every catalog when adding or changing the source. Use
`FormatContent` for templates and pass variable values as arguments, rather
than building a different source key for each server.

Edit the catalogs directly for languages without a generator. PowerShell
files are development tools only: users do not need PowerShell, an internet
connection, an AI model, or a translation service to change languages.
Run the maintenance generators with PowerShell 7 on Windows so their UTF-8
source text is read correctly.

## Data and technical syntax

Translate Synix's wording, not user data. Keep game names and identifiers,
passwords, map and mode values, configuration keys and values, console
commands, launch arguments, paths, filenames, ports, tokens, protocol names,
and original game-server output unchanged. Technical support exports retain
their original data for diagnosis; their surrounding interface is localized.
`SetDataText` protects labels that display user values even when a value
happens to be an interface word such as "Save".

Preserve placeholders, URLs, command examples, file-filter separators, and
paragraph breaks. Language changes must not modify saved settings or silently
reset the selected game, help topic, search text, or editable input.

## Validation and translation review

`CompleteLanguageCoverageTests`, `LocalizationTests`, and
`AdditionalLanguageTests` check all selectable languages, not just the
original three translations. They cover exact-culture resource completeness,
placeholder preservation, offline article coverage and technical examples,
live switching, aliases, unchanged user input, fallback behavior, and layout.
The source guard also includes the Help windows and visible tooltip text.

The expanded catalogs include machine-assisted translations. Structural tests
and sample reviews do not replace native-speaker review of terminology and
meaning, particularly for security, networking, licensing, and recovery
instructions. Corrections should update the relevant catalog entry without
altering technical syntax or the English source to hide a missing translation.
