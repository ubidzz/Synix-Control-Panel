# Synix interface languages

Synix ships compiled, offline translation catalogs. The
[coverage inventory](../../docs/Language-Coverage.md) lists every current
catalog and outstanding Windows/.NET language or writing-system target.
Every selectable language must contain every entry in the neutral English
catalog; partial preview catalogs are not offered in the language picker.

The language picker displays each language's own name. Regional aliases use
the corresponding available catalog; generic Norwegian (`no` / `no-NO`)
selects Bokmål. Nynorsk (`nn` / `nn-NO`) requires its own catalog and never
silently resolves to Bokmål.

## Comprehensive language target

The target is every language exposed by Windows/.NET, with separate writing
systems where needed. This is a roadmap, not a claim that every target is
already translated. [The coverage inventory](../../docs/Language-Coverage.md)
lists the complete current catalogs and every outstanding language/script
target. Country variants may share a language catalog; they are not counted
as independently translated languages.

The app only offers catalogs with every English source key present and
nonblank. Preview catalogs, stale keys and English-only placeholder copies
are excluded. Unknown writing systems do not silently borrow a different
script: for example, Latin Hindi is not the default Hindi translation.
Explicit script catalogs keep their script code, not a guessed country code.

Run `dotnet run --project Tools/LanguageCoverage -- --check` from the
repository root to check all source catalogs, including files that the app
would exclude. Use `--markdown` to print an updated inventory. This tool is
offline, read-only and not shipped in the installed application. The culture
inventory depends on the Windows/.NET globalization data on the machine;
there is no hard-coded global language count.

Add translations in complete-language batches. Before exposing a new catalog:

1. Translate every source entry, including help, warnings, themes and
   accessibility labels, preserving the technical content described below.
2. Check placeholders and technical syntax and review the meaning of critical
   security, recovery, update and uninstall instructions.
3. Review fonts, long labels, navigation, dialogs and live language switching.
   Right-to-left languages additionally need bidirectional controls and
   mirroring without changing passwords, paths, commands or server data.
4. Regenerate the coverage inventory. Catalog completeness is a structural
   safeguard, not proof of translation accuracy or finished RTL layout.

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
`ExpandedLanguageBatchTests` additionally checks the new script-specific
catalog choices, diagnostic symbols, recovery instructions and technical tokens.

The expanded catalogs include machine-assisted translations. Structural tests
and sample reviews do not replace native-speaker review of terminology and
meaning, particularly for security, networking, licensing, and recovery
instructions. Corrections should update the relevant catalog entry without
altering technical syntax or the English source to hide a missing translation.

Native-speaker review is not a release prerequisite for a complete catalog.
Machine-assisted translations may ship while wording is refined through user
reports. Reports should include the selected language, the text shown, where
it appears, and the suggested wording. Never include passwords or other
private server data. Broken technical syntax, missing text, or an identified
reversal of a destructive-action or account-recovery warning must still be
corrected before that catalog is enabled.

The Azerbaijani, Bosnian, and Uzbek Cyrillic catalogs are derived from their
Latin-script translations using the corresponding
[Unicode CLDR transforms](https://github.com/unicode-org/cldr/tree/main/common/transforms),
with spelling adjustments for the target script. Latin-script Hindi is
derived from a protected Hindi translation using the Windows ICU
Devanagari-to-Latin transform. Technical names, paths, commands, and placeholders
are kept verbatim. These conversions run only during catalog preparation;
Synix does not ship a transliteration or translation model.

The Catalan, Slovak, and Slovenian catalogs started from offline,
machine-assisted drafts using the corresponding English-to-language
[Argos Translate 1.9 packages](https://github.com/argosopentech/argospm-index),
based on OPUS-MT by Jörg Tiedemann and Santhosh Thottingal (the package identifies
its source model as CC BY 4.0). The Latvian and Lithuanian catalogs use the
OPUS-MT transformer-big models for
[English–Latvian](https://huggingface.co/Helsinki-NLP/opus-mt-tc-big-en-lv) and
[English–Lithuanian](https://huggingface.co/Helsinki-NLP/opus-mt-tc-big-en-lt)
by the Helsinki-NLP team (CC BY 4.0), also run offline. The Estonian draft used
the English–Estonian 1.9 package by Aleksey Kutashov, trained with OPUS and the other corpora
credited in that package's README. Headings, technical tokens, key actions and
safety-sensitive passages were corrected separately. No translation model,
Python runtime, translation cache or online service is shipped with Synix.
This does not replace native-speaker review or establish an authoritative
legal translation; the original English licence remains unchanged.

The Malay and Filipino catalogs were drafted locally with machine assistance
from [MADLAD-400-3B-MT](https://huggingface.co/google/madlad400-3b-mt)
(Apache 2.0) and [NLLB-200 distilled 1.3B](https://huggingface.co/facebook/nllb-200-distilled-1.3B)
(CC BY-NC 4.0), respectively, then corrected for interface terminology,
diagnostic markers, technical examples, and safety-sensitive instructions.
The app bundles only static text resources, with no translation model or
online translation dependency. These translations are not certified and
still benefit from native-speaker review. Malay uses the Latin script;
Filipino uses the `fil` catalog and `fil-PH` selection culture.

The expanded language batch uses local machine-assisted drafts
from [MADLAD-400-3B-MT](https://huggingface.co/google/madlad400-3b-mt), through
[Nextcloud's CTranslate2 int8 conversion](https://huggingface.co/Nextcloud-AI/madlad400-3b-mt-ct2-int8)
(Apache 2.0, revision `aa32bbdeba7880eff2096ec044cb155a340a9400`) and
[NLLB-200 distilled 600M](https://huggingface.co/facebook/nllb-200-distilled-600M),
through [the CTranslate2 int8 conversion](https://huggingface.co/mydubber-org/nllb-200-distilled-600M-ct2-int8)
(CC BY-NC 4.0, revision `9e63a771e9ec358ef812d2ea149d97e16d5f8cee`). Further
catalogs and corrections use [OpenNMT's NLLB-200-3.3B int8 conversion](https://huggingface.co/OpenNMT/nllb-200-3.3B-ct2-int8)
(CC BY-NC 4.0, revision `28d998cc8548ad62f4f98cf8860928c3af99b97b`). Corrections
cover interface actions, untranslated fragments, translation artifacts and
critical pairing, recovery and deletion wording. Each catalog includes all
English source entries, including offline help. Community wording corrections
remain welcome; completeness and automated checks do not certify accuracy.
Only the translated text ships with Synix. Models, generators and caches
used for this batch are kept outside the application and repository.

Tifinagh and Adlam interface text uses Windows' Ebrima font to avoid missing-character
boxes. Tibetan and Dzongkha use Microsoft Himalaya at a larger readable size.
Language switching restores the previous interface fonts, including menu and
grid styles. Editable technical fields retain their fonts and contents. The
settings cards measure the selected fonts and reserve space for wrapped text.

Azerbaijani, Bosnian, Serbian, and Uzbek have separate Latin and Cyrillic
catalogs. Hindi has Devanagari and Latin-script catalogs. A script-specific
catalog does not claim support for a different writing system of the same
language.

Fula has separate Latin and Adlam catalogs. The Adlam text derives from the
Latin catalog using the [Library of Congress Adlam correspondences](https://www.loc.gov/catdir/cpso/romanization/adlam37.pdf),
including long vowels, consonant length and nasal combinations. Technical
identifiers, paths, numbers and placeholders retain their original spelling.

## Reading direction and resource loading

Right-to-left cultures set the reading direction of interface captions,
buttons, menus and read-only help text. Custom-drawn controls use matching
text flags. Editable technical values, console output, user-data labels and
scrolling containers retain their existing direction and geometry. This
supports right-to-left text without claiming that every window has a fully
mirrored layout. Community feedback can improve usability for complex scripts;
native-speaker review is not a prerequisite for adding a complete catalog.

Use `UseInterfaceTextDirection` for read-only rich text that contains app
instructions. Use `SetDataText` for user values. Switching the interface
language does not change the formatting culture used by game configuration.

For ordinary folder builds, catalog discovery reads embedded resource data
directly from satellite PE files without loading each satellite assembly.
Single-file builds use the bundled ResourceManager fallback. Discovery
releases resource sets used for completeness checks. The text lookup index
starts with English and adds languages when selected, retaining those
entries for live switching. Adding catalogs therefore does not keep every
translation table in the live text index at startup. Bind help titles to
their exact content keys so similar translated captions cannot select a
different article title.
