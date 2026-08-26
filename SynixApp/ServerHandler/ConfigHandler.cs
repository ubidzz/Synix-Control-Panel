// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using System.Globalization;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public enum ConfigFormat
	{
		StandardINI = 0,
		XML = 2,
		JSON = 3,
		Space = 4
	}

	public enum ConfigValueType
	{
		Text,
		Number,
		Boolean,
		Secret,
		Null
	}

	public sealed class ConfigLine
	{
		public string Id { get; set; } = string.Empty;
		public string Key { get; set; } = string.Empty;
		public string Path { get; set; } = string.Empty;
		public string Section { get; set; } = string.Empty;
		public string Value { get; set; } = string.Empty;
		public string OriginalValue { get; set; } = string.Empty;
		public bool HasOriginalValue { get; set; }
		public ConfigValueType Type { get; set; } = ConfigValueType.Text;
	}

	public static class ConfigHandler
	{
		private enum ScalarStyle
		{
			Raw,
			QuotedSingle,
			QuotedDouble,
			JsonString,
			JsonNumber,
			JsonBoolean,
			JsonNull,
			XmlAttribute,
			XmlText,
			XmlCData
		}

		private sealed class ParsedValue
		{
			public string Id { get; init; } = string.Empty;
			public string Key { get; init; } = string.Empty;
			public string Path { get; init; } = string.Empty;
			public string Section { get; init; } = string.Empty;
			public string Value { get; init; } = string.Empty;
			public string OriginalToken { get; init; } = string.Empty;
			public ConfigValueType Type { get; init; }
			public ScalarStyle Style { get; init; }
			public int Start { get; init; }
			public int Length { get; init; }
			public bool IsCompositeValue { get; init; }
		}

		private sealed class ParsedDocument
		{
			public List<ParsedValue> Values { get; } = new();
		}

		private sealed class Replacement
		{
			public int Start { get; init; }
			public int Length { get; init; }
			public string ExpectedOriginalToken { get; init; } = string.Empty;
			public string Value { get; init; } = string.Empty;
		}

		private sealed class IdentityBuilder
		{
			private readonly Dictionary<string, int> _occurrences =
				new(StringComparer.Ordinal);

			public string Create(string formatPrefix, string logicalPath)
			{
				_occurrences.TryGetValue(logicalPath, out int occurrence);
				_occurrences[logicalPath] = occurrence + 1;

				string encodedPath = Convert.ToBase64String(
					Encoding.UTF8.GetBytes(logicalPath));
				return $"{formatPrefix}:{encodedPath}:{occurrence}";
			}
		}

		private sealed class TextFileSnapshot
		{
			public string Text { get; private init; } = string.Empty;
			public Encoding TextEncoding { get; private init; } =
				new UTF8Encoding(false);
			public bool HasByteOrderMark { get; private init; }

			public static TextFileSnapshot Read(string path)
			{
				byte[] bytes;
				using (FileStream stream = new(
					path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite))
				{
					if (stream.Length > int.MaxValue)
					{
						throw new InvalidDataException(
							"The configuration file is too large to edit safely.");
					}

					bytes = new byte[(int)stream.Length];
					stream.ReadExactly(bytes);
				}

				Encoding encoding;
				int preambleLength;
				bool hasBom;

				if (StartsWith(bytes, new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
				{
					encoding = new UTF32Encoding(true, true, true);
					preambleLength = 4;
					hasBom = true;
				}
				else if (StartsWith(bytes, new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
				{
					encoding = new UTF32Encoding(false, true, true);
					preambleLength = 4;
					hasBom = true;
				}
				else if (StartsWith(bytes, new byte[] { 0xEF, 0xBB, 0xBF }))
				{
					encoding = new UTF8Encoding(true, true);
					preambleLength = 3;
					hasBom = true;
				}
				else if (StartsWith(bytes, new byte[] { 0xFE, 0xFF }))
				{
					encoding = new UnicodeEncoding(true, true, true);
					preambleLength = 2;
					hasBom = true;
				}
				else if (StartsWith(bytes, new byte[] { 0xFF, 0xFE }))
				{
					encoding = new UnicodeEncoding(false, true, true);
					preambleLength = 2;
					hasBom = true;
				}
				else
				{
					encoding = IsValidUtf8(bytes)
						? new UTF8Encoding(false, true)
						: CreateStrictLatin1Encoding();
					preambleLength = 0;
					hasBom = false;
				}

				return new TextFileSnapshot
				{
					Text = encoding.GetString(
						bytes,
						preambleLength,
						bytes.Length - preambleLength),
					TextEncoding = encoding,
					HasByteOrderMark = hasBom
				};
			}

			public byte[] Encode(string content)
			{
				byte[] contentBytes = TextEncoding.GetBytes(content);
				if (!HasByteOrderMark)
				{
					return contentBytes;
				}

				byte[] preamble = TextEncoding.GetPreamble();
				byte[] output = new byte[preamble.Length + contentBytes.Length];
				Buffer.BlockCopy(preamble, 0, output, 0, preamble.Length);
				Buffer.BlockCopy(
					contentBytes,
					0,
					output,
					preamble.Length,
					contentBytes.Length);
				return output;
			}

			private static bool StartsWith(byte[] source, byte[] prefix)
			{
				if (source.Length < prefix.Length)
				{
					return false;
				}

				for (int index = 0; index < prefix.Length; index++)
				{
					if (source[index] != prefix[index])
					{
						return false;
					}
				}

				return true;
			}

			private static bool IsValidUtf8(byte[] bytes)
			{
				try
				{
					_ = new UTF8Encoding(false, true).GetString(bytes);
					return true;
				}
				catch (DecoderFallbackException)
				{
					return false;
				}
			}

			private static Encoding CreateStrictLatin1Encoding()
			{
				Encoding encoding = (Encoding)Encoding.Latin1.Clone();
				encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
				encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
				return encoding;
			}
		}

		private sealed class XmlElementFrame
		{
			public string NumericPath { get; init; } = string.Empty;
			public string DisplayPath { get; init; } = string.Empty;
			public int ContentStart { get; init; }
			public int NextChildIndex { get; set; }
			public bool HasChildElements { get; set; }
			public bool HasExplicitTextEntry { get; set; }
			public bool ContainsOtherMarkup { get; set; }
		}

		private sealed class XmlAttributeToken
		{
			public string Name { get; init; } = string.Empty;
			public string Value { get; init; } = string.Empty;
			public int ValueStart { get; init; }
			public int ValueLength { get; init; }
		}

		public static List<ConfigLine> LoadConfig(string path, ConfigFormat format)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("A configuration path is required.", nameof(path));
			}

			if (!File.Exists(path))
			{
				return new List<ConfigLine>();
			}

			TextFileSnapshot snapshot = TextFileSnapshot.Read(path);
			return LoadConfigText(snapshot.Text, format);
		}

		internal static List<ConfigLine> LoadConfigText(
			string text,
			ConfigFormat format)
		{
			ArgumentNullException.ThrowIfNull(text);
			ParsedDocument document = ParseDocument(text, format);
			return document.Values.Select(value => new ConfigLine
			{
				Id = value.Id,
				Key = value.Key,
				Path = value.Path,
				Section = value.Section,
				Value = value.Value,
				OriginalValue = value.Value,
				HasOriginalValue = true,
				Type = value.Type
			}).ToList();
		}

		internal static bool HasRequiredStructure(
			string path,
			string template,
			ConfigFormat format)
		{
			if (!File.Exists(path))
			{
				return false;
			}

			string existingText = TextFileSnapshot.Read(path).Text;
			Dictionary<string, int> existingStructure =
				BuildStructureSignature(existingText, format);
			Dictionary<string, int> requiredStructure =
				BuildStructureSignature(template, format);

			foreach ((string key, int requiredCount) in requiredStructure)
			{
				if (!existingStructure.TryGetValue(key, out int existingCount) ||
					existingCount < requiredCount)
				{
					return false;
				}
			}

			return true;
		}

		public static string CreatePreview(
			string path,
			IReadOnlyCollection<ConfigLine> data,
			ConfigFormat format)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(
					"The configuration file could not be found.",
					path);
			}

			TextFileSnapshot snapshot = TextFileSnapshot.Read(path);
			return BuildUpdatedText(snapshot.Text, data, format);
		}

		public static void SaveConfig(
			string path,
			IReadOnlyCollection<ConfigLine> data,
			ConfigFormat format)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(
					"The configuration file could not be found.",
					path);
			}

			TextFileSnapshot snapshot = TextFileSnapshot.Read(path);
			string updatedText = BuildUpdatedText(snapshot.Text, data, format);
			if (string.Equals(updatedText, snapshot.Text, StringComparison.Ordinal))
			{
				return;
			}

			WriteAtomically(path, snapshot.Encode(updatedText));
		}

		internal static bool EnsureStandardIniTupleValues(
			string path,
			string tupleKey,
			IReadOnlyDictionary<string, string> requiredValues)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException("The configuration file could not be found.", path);
			if (string.IsNullOrWhiteSpace(tupleKey) ||
				tupleKey.IndexOfAny(['=', '(', ')', '\r', '\n']) >= 0)
			{
				throw new InvalidDataException("The INI tuple key is invalid.");
			}

			TextFileSnapshot snapshot = TextFileSnapshot.Read(path);
			ParsedDocument document = ParseDocument(snapshot.Text, ConfigFormat.StandardINI);
			List<KeyValuePair<string, string>> missing = requiredValues
				.Where(required => !document.Values.Any(value =>
					string.Equals(value.Key, required.Key, StringComparison.Ordinal)))
				.ToList();
			if (missing.Count == 0)
				return false;

			foreach ((string key, string value) in missing)
			{
				if (string.IsNullOrWhiteSpace(key) ||
					key.IndexOfAny(['=', ',', '(', ')', '\r', '\n']) >= 0 ||
					!IsSafeIniTupleValue(value))
				{
					throw new InvalidDataException("A requested INI tuple value is invalid.");
				}
			}

			string marker = tupleKey + "=(";
			int markerIndex = snapshot.Text.IndexOf(marker, StringComparison.Ordinal);
			if (markerIndex < 0 ||
				snapshot.Text.IndexOf(marker, markerIndex + marker.Length, StringComparison.Ordinal) >= 0)
			{
				throw new InvalidDataException(
					$"The configuration must contain exactly one {tupleKey} tuple.");
			}

			int lineEnd = snapshot.Text.IndexOfAny(['\r', '\n'], markerIndex);
			if (lineEnd < 0)
				lineEnd = snapshot.Text.Length;
			int closingIndex = snapshot.Text.LastIndexOf(
				')',
				lineEnd - 1,
				lineEnd - markerIndex);
			if (closingIndex < markerIndex + marker.Length)
				throw new InvalidDataException($"The {tupleKey} tuple is incomplete.");

			string separator = closingIndex > markerIndex + marker.Length ? "," : string.Empty;
			string insertion = separator + string.Join(
				",",
				missing.Select(required => $"{required.Key}={required.Value}"));
			string updated = snapshot.Text.Insert(closingIndex, insertion);
			WriteAtomically(path, snapshot.Encode(updated));
			return true;
		}

		private static bool IsSafeIniTupleValue(string value)
		{
			if (value.IndexOfAny(['=', '\r', '\n', '\0']) >= 0)
				return false;
			if (!value.Contains(',') && !value.Contains('(') && !value.Contains(')'))
				return true;
			if (value.Length < 3 || value[0] != '(' || value[^1] != ')')
				return false;

			string entries = value[1..^1];
			if (entries.Length == 0)
				return false;
			foreach (string entry in entries.Split(','))
			{
				if (entry.Length == 0 || entry.Any(character =>
					!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-'))
				{
					return false;
				}
			}

			return true;
		}

		public static string GetFormatDisplayName(ConfigFormat format)
		{
			return format switch
			{
				ConfigFormat.JSON => "JSON",
				ConfigFormat.XML => "XML",
				ConfigFormat.Space => "SPACE",
				_ => "INI"
			};
		}

		private static string BuildUpdatedText(
			string originalText,
			IReadOnlyCollection<ConfigLine> data,
			ConfigFormat format)
		{
			ParsedDocument originalDocument = ParseDocument(originalText, format);
			Dictionary<string, ConfigLine> updatesById = data
				.Where(item => !string.IsNullOrWhiteSpace(item.Id))
				.GroupBy(item => item.Id, StringComparer.Ordinal)
				.ToDictionary(group => group.Key, group => group.Last(), StringComparer.Ordinal);

			Dictionary<string, List<ConfigLine>> legacyUpdatesByKey = data
				.Where(item => string.IsNullOrWhiteSpace(item.Id))
				.GroupBy(item => item.Key, StringComparer.Ordinal)
				.ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

			List<Replacement> replacements = new();
			Dictionary<string, string> expectedValuesById =
				new(StringComparer.Ordinal);
			HashSet<string> unmatchedChangedIds = updatesById.Values
				.Where(UserChangedValue)
				.Select(item => item.Id)
				.ToHashSet(StringComparer.Ordinal);

			foreach (ParsedValue sourceValue in originalDocument.Values)
			{
				ConfigLine? updatedValue = null;
				bool matchedById = updatesById.TryGetValue(sourceValue.Id, out updatedValue);
				if (!matchedById &&
					legacyUpdatesByKey.TryGetValue(sourceValue.Key, out List<ConfigLine>? legacyMatches) &&
					legacyMatches.Count == 1)
				{
					updatedValue = legacyMatches[0];
				}

				if (updatedValue == null || !UserChangedValue(updatedValue))
				{
					continue;
				}

				if (matchedById &&
					(!string.Equals(sourceValue.Key, updatedValue.Key, StringComparison.Ordinal) ||
					 !string.Equals(sourceValue.Path, updatedValue.Path, StringComparison.Ordinal)))
				{
					throw new InvalidDataException(
						$"The location of '{updatedValue.Key}' changed after the editor opened. " +
						"Reload the configuration before saving.");
				}

				unmatchedChangedIds.Remove(sourceValue.Id);
				if (ValuesAreEquivalent(sourceValue, updatedValue.Value))
				{
					continue;
				}

				if (updatedValue.HasOriginalValue &&
					!ValuesAreEquivalent(sourceValue, updatedValue.OriginalValue))
				{
					throw new InvalidDataException(
						$"'{sourceValue.Key}' changed on disk after the editor opened. " +
						"Reload the file before saving so the newer value is not overwritten.");
				}

				string replacementValue = FormatReplacement(
					sourceValue,
					updatedValue.Value,
					format);
				replacements.Add(new Replacement
				{
					Start = sourceValue.Start,
					Length = sourceValue.Length,
					ExpectedOriginalToken = sourceValue.OriginalToken,
					Value = replacementValue
				});
				expectedValuesById[sourceValue.Id] = NormalizeExpectedValue(
					sourceValue,
					updatedValue.Value);
			}

			if (unmatchedChangedIds.Count > 0)
			{
				throw new InvalidDataException(
					"One or more edited settings no longer exist in the file. " +
					"Reload the configuration before saving.");
			}

			string updatedText = ApplyLexicalSpanReplacements(
				originalText,
				replacements);
			VerifyLexicalPreservation(originalText, updatedText, replacements);
			ValidateUpdatedDocument(
				originalDocument,
				updatedText,
				format,
				expectedValuesById);
			return updatedText;
		}

		private static bool ValuesAreEquivalent(ParsedValue source, string updatedValue)
		{
			if (source.Type == ConfigValueType.Boolean &&
				TryParseBoolean(source.Value, out bool sourceBoolean) &&
				TryParseBoolean(updatedValue, out bool updatedBoolean))
			{
				return sourceBoolean == updatedBoolean;
			}

			return string.Equals(source.Value, updatedValue, StringComparison.Ordinal);
		}

		private static bool UserChangedValue(ConfigLine value)
		{
			if (!value.HasOriginalValue)
			{
				return true;
			}

			if (value.Type == ConfigValueType.Boolean &&
				TryParseBoolean(value.OriginalValue, out bool originalBoolean) &&
				TryParseBoolean(value.Value, out bool currentBoolean))
			{
				return originalBoolean != currentBoolean;
			}

			return !string.Equals(
				value.OriginalValue,
				value.Value,
				StringComparison.Ordinal);
		}

		private static string NormalizeExpectedValue(
			ParsedValue source,
			string requestedValue)
		{
			string value = requestedValue ?? string.Empty;
			if (source.Type == ConfigValueType.Boolean &&
				TryParseBoolean(value, out bool booleanValue))
			{
				return booleanValue ? "True" : "False";
			}

			if (source.Type == ConfigValueType.Number)
			{
				return value.Trim();
			}

			return source.Style == ScalarStyle.JsonNull &&
				value.Equals("null", StringComparison.OrdinalIgnoreCase)
				? "null"
				: value;
		}

		private static string ApplyLexicalSpanReplacements(
			string originalText,
			List<Replacement> replacements)
		{
			if (replacements.Count == 0)
			{
				return originalText;
			}

			List<Replacement> ordered = replacements
				.OrderByDescending(item => item.Start)
				.ToList();
			int previousStart = originalText.Length + 1;
			StringBuilder builder = new(originalText);

			foreach (Replacement replacement in ordered)
			{
				if (replacement.Start < 0 ||
					replacement.Length < 0 ||
					replacement.Start + replacement.Length > originalText.Length)
				{
					throw new InvalidDataException(
						"A configuration value could not be mapped back to the source file.");
				}

				if (replacement.ExpectedOriginalToken.Length != replacement.Length ||
					!originalText.AsSpan(replacement.Start, replacement.Length)
						.SequenceEqual(replacement.ExpectedOriginalToken.AsSpan()))
				{
					throw new InvalidDataException(
						"A configuration value no longer matches its exact source span. " +
						"Nothing was saved; reload the file before trying again.");
				}

				if (replacement.Start + replacement.Length > previousStart)
				{
					throw new InvalidDataException(
						"Overlapping configuration values were detected. Nothing was saved.");
				}

				builder.Remove(replacement.Start, replacement.Length);
				builder.Insert(replacement.Start, replacement.Value);
				previousStart = replacement.Start;
			}

			return builder.ToString();
		}

		private static void VerifyLexicalPreservation(
			string originalText,
			string updatedText,
			IReadOnlyCollection<Replacement> replacements)
		{
			if (replacements.Count == 0)
			{
				if (!string.Equals(originalText, updatedText, StringComparison.Ordinal))
				{
					throw new InvalidDataException(
						"The configuration changed even though no value replacement was requested.");
				}
				return;
			}

			List<Replacement> ordered = replacements
				.OrderBy(item => item.Start)
				.ToList();
			int originalCursor = 0;
			int updatedCursor = 0;

			foreach (Replacement replacement in ordered)
			{
				int unchangedLength = replacement.Start - originalCursor;
				if (unchangedLength < 0 ||
					updatedCursor + unchangedLength > updatedText.Length ||
					!originalText.AsSpan(originalCursor, unchangedLength)
						.SequenceEqual(updatedText.AsSpan(updatedCursor, unchangedLength)))
				{
					throw new InvalidDataException(
						"Text outside an edited value span would be modified. " +
						"The save was cancelled.");
				}

				originalCursor = replacement.Start;
				updatedCursor += unchangedLength;
				if (updatedCursor + replacement.Value.Length > updatedText.Length ||
					!updatedText.AsSpan(updatedCursor, replacement.Value.Length)
						.SequenceEqual(replacement.Value.AsSpan()))
				{
					throw new InvalidDataException(
						"A replacement value was not written to its exact lexical span. " +
						"The save was cancelled.");
				}

				originalCursor += replacement.Length;
				updatedCursor += replacement.Value.Length;
			}

			int trailingLength = originalText.Length - originalCursor;
			if (trailingLength < 0 ||
				updatedCursor + trailingLength != updatedText.Length ||
				!originalText.AsSpan(originalCursor, trailingLength)
					.SequenceEqual(updatedText.AsSpan(updatedCursor, trailingLength)))
			{
				throw new InvalidDataException(
					"Trailing text outside the edited value spans would be modified. " +
					"The save was cancelled.");
			}
		}

		private static void ValidateUpdatedDocument(
			ParsedDocument originalDocument,
			string updatedText,
			ConfigFormat format,
			Dictionary<string, string> expectedValuesById)
		{
			if (format == ConfigFormat.XML)
			{

				XmlDocument xmlDocument = new() { PreserveWhitespace = true };
				xmlDocument.LoadXml(updatedText);
			}

			ParsedDocument reparsedDocument = ParseDocument(updatedText, format);
			if (originalDocument.Values.Count != reparsedDocument.Values.Count ||
				!originalDocument.Values.Select(value => value.Id).SequenceEqual(
					reparsedDocument.Values.Select(value => value.Id),
					StringComparer.Ordinal))
			{
				throw new InvalidDataException(
					"The replacement would change the configuration's setting structure. " +
					"The save was cancelled.");
			}

			Dictionary<string, ParsedValue> reparsedValues = reparsedDocument.Values
				.ToDictionary(value => value.Id, value => value, StringComparer.Ordinal);

			foreach ((string changedId, string expectedValue) in expectedValuesById)
			{
				if (!reparsedValues.TryGetValue(changedId, out ParsedValue? reparsedValue) ||
					!ValuesAreEquivalent(reparsedValue, expectedValue))
				{
					ParsedValue? source = originalDocument.Values.FirstOrDefault(
						value => value.Id == changedId);
					throw new InvalidDataException(
						$"The new value for '{source?.Key ?? "a setting"}' would change " +
						"the file structure. The save was cancelled.");
				}
			}
		}

		private static string FormatReplacement(
			ParsedValue source,
			string requestedValue,
			ConfigFormat format)
		{
			string value = requestedValue ?? string.Empty;
			if (source.OriginalToken.Contains('\r') ||
				source.OriginalToken.Contains('\n'))
			{
				throw new InvalidDataException(
					$"'{source.Key}' contains source line breaks and cannot be edited " +
					"without changing the file's original layout.");
			}

			if (value.Contains('\r') || value.Contains('\n'))
			{
				throw new InvalidDataException(
					$"'{source.Key}' cannot contain a line break.");
			}

			if (source.Type == ConfigValueType.Boolean)
			{
				if (!TryParseBoolean(value, out bool booleanValue))
				{
					throw new InvalidDataException(
						$"'{source.Key}' must be True or False.");
				}

				value = source.Style == ScalarStyle.JsonBoolean
					? (booleanValue ? "true" : "false")
					: PreserveBooleanCasing(source.OriginalToken, booleanValue);
			}
			else if (source.Type == ConfigValueType.Number)
			{
				if (!IsValidNumber(value, source.Style == ScalarStyle.JsonNumber))
				{
					throw new InvalidDataException(
						$"'{source.Key}' requires a valid numeric value.");
				}
				value = value.Trim();
			}

			return source.Style switch
			{
				ScalarStyle.JsonString => EncodeJsonStringToken(value),
				ScalarStyle.JsonNumber => value,
				ScalarStyle.JsonBoolean => value.ToLowerInvariant(),
				ScalarStyle.JsonNull => value.Equals("null", StringComparison.OrdinalIgnoreCase)
					? "null"
					: EncodeJsonStringToken(value),
				ScalarStyle.QuotedSingle => QuoteValue(value, '\''),
				ScalarStyle.QuotedDouble => QuoteValue(value, '"'),
				ScalarStyle.XmlAttribute => EscapeXml(value),
				ScalarStyle.XmlText => EscapeXml(value),
				ScalarStyle.XmlCData => value.Contains("]]>", StringComparison.Ordinal)
					? throw new InvalidDataException(
						$"'{source.Key}' cannot contain the XML CDATA terminator ']]>'.")
					: value,
				_ => FormatRawValue(source, value, format)
			};
		}

		private static string FormatRawValue(
			ParsedValue source,
			string value,
			ConfigFormat format)
		{
			if (format == ConfigFormat.StandardINI && NeedsIniQuotes(source, value))
			{
				return QuoteValue(value, '"');
			}

			return value;
		}

		private static bool NeedsIniQuotes(ParsedValue source, string value)
		{
			if (source.IsCompositeValue &&
				(value.Contains(',') || value.Contains('(') || value.Contains(')')))
			{
				return true;
			}

			string trimmed = value.Trim();
			if (trimmed.Length >= 2 && trimmed[0] == '(' && trimmed[^1] == ')')
			{
				return true;
			}

			for (int index = 1; index < value.Length; index++)
			{
				if (!char.IsWhiteSpace(value[index - 1]))
				{
					continue;
				}

				if (value[index] == ';' || value[index] == '#' ||
					(value[index] == '/' && index + 1 < value.Length && value[index + 1] == '/'))
				{
					return true;
				}
			}

			return false;
		}

		private static string QuoteValue(string value, char quote)
		{
			StringBuilder builder = new(value.Length + 2);
			builder.Append(quote);
			foreach (char character in value)
			{
				if (character == quote)
				{
					builder.Append('\\');
				}
				builder.Append(character);
			}
			builder.Append(quote);
			return builder.ToString();
		}

		private static string EncodeJsonStringToken(string value)
		{
			StringBuilder builder = new(value.Length + 2);
			builder.Append('"');
			for (int index = 0; index < value.Length; index++)
			{
				char character = value[index];
				switch (character)
				{
					case '"': builder.Append("\\\""); break;
					case '\\': builder.Append("\\\\"); break;
					case '\b': builder.Append("\\b"); break;
					case '\f': builder.Append("\\f"); break;
					case '\n': builder.Append("\\n"); break;
					case '\r': builder.Append("\\r"); break;
					case '\t': builder.Append("\\t"); break;
					default:
						if (character < ' ')
						{
							builder.Append("\\u");
							builder.Append(((int)character).ToString(
								"X4",
								CultureInfo.InvariantCulture));
						}
						else if (char.IsHighSurrogate(character))
						{
							if (index + 1 >= value.Length ||
								!char.IsLowSurrogate(value[index + 1]))
							{
								throw new InvalidDataException(
									"A JSON value contains an invalid Unicode surrogate.");
							}

							builder.Append(character);
							builder.Append(value[++index]);
						}
						else if (char.IsLowSurrogate(character))
						{
							throw new InvalidDataException(
								"A JSON value contains an invalid Unicode surrogate.");
						}
						else
						{
							builder.Append(character);
						}
						break;
				}
			}
			builder.Append('"');
			return builder.ToString();
		}

		private static string EscapeXml(string value)
		{
			return SecurityElement.Escape(value) ?? string.Empty;
		}

		private static bool IsValidNumber(string value, bool requireJsonNumber)
		{
			string trimmed = value.Trim();
			if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return !requireJsonNumber && long.TryParse(
					trimmed[2..],
					NumberStyles.HexNumber,
					CultureInfo.InvariantCulture,
					out _);
			}

			if (!double.TryParse(
				trimmed,
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out double number) ||
				!double.IsFinite(number))
			{
				return false;
			}

			if (!requireJsonNumber)
			{
				return true;
			}

			try
			{
				using JsonDocument document = JsonDocument.Parse(trimmed);
				return document.RootElement.ValueKind == JsonValueKind.Number;
			}
			catch (JsonException)
			{
				return false;
			}
		}

		private static string PreserveBooleanCasing(string originalValue, bool value)
		{
			string original = originalValue.Trim();
			if (original.Length >= 2 &&
				((original[0] == '"' && original[^1] == '"') ||
				 (original[0] == '\'' && original[^1] == '\'')))
			{
				original = original[1..^1];
			}
			if (original.All(character => !char.IsLetter(character) || char.IsUpper(character)))
			{
				return value ? "TRUE" : "FALSE";
			}

			if (original.Length > 0 && char.IsUpper(original[0]))
			{
				return value ? "True" : "False";
			}

			return value ? "true" : "false";
		}

		private static void WriteAtomically(string path, byte[] content)
		{
			string fullPath = Path.GetFullPath(path);
			string directory = Path.GetDirectoryName(fullPath)
				?? throw new InvalidOperationException("The config directory is unavailable.");
			string temporaryPath = Path.Combine(
				directory,
				$".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.synix.tmp");
			string backupPath = fullPath + ".synix.bak";

			try
			{
				File.WriteAllBytes(temporaryPath, content);
				try
				{
					File.Replace(temporaryPath, fullPath, backupPath, true);
				}
				catch (PlatformNotSupportedException)
				{
					ReplaceWithFallback(temporaryPath, fullPath, backupPath);
				}
				catch (IOException)
				{
					ReplaceWithFallback(temporaryPath, fullPath, backupPath);
				}
			}
			finally
			{
				if (File.Exists(temporaryPath))
				{
					File.Delete(temporaryPath);
				}
			}
		}

		private static void ReplaceWithFallback(
			string temporaryPath,
			string destinationPath,
			string backupPath)
		{
			if (!File.Exists(temporaryPath))
			{
				return;
			}

			File.Copy(destinationPath, backupPath, true);
			File.Move(temporaryPath, destinationPath, true);
		}

		private static ParsedDocument ParseDocument(string text, ConfigFormat format)
		{
			return format switch
			{
				ConfigFormat.StandardINI => ParseIniDocument(text),
				ConfigFormat.JSON => new JsonConfigScanner(text).Parse(),
				ConfigFormat.XML => ParseXmlDocument(text),
				ConfigFormat.Space => ParseSpaceDocument(text),
				_ => throw new NotSupportedException(
					$"The configuration format '{format}' is not supported.")
			};
		}

		private static Dictionary<string, int> BuildStructureSignature(
			string text,
			ConfigFormat format)
		{
			Dictionary<string, int> signature = new(StringComparer.Ordinal);
			if (format == ConfigFormat.JSON)
			{
				using JsonDocument document = JsonDocument.Parse(text);
				AddJsonStructure(document.RootElement, "$", signature);
				return signature;
			}

			ParsedDocument parsed = ParseDocument(text, format);
			foreach (ParsedValue value in parsed.Values)
			{
				AddStructureEntry(
					signature,
					value.Path);
			}

			return signature;
		}

		private static void AddJsonStructure(
			JsonElement element,
			string path,
			Dictionary<string, int> signature)
		{
			AddStructureEntry(signature, $"{path}\u001f{element.ValueKind}");
			if (element.ValueKind != JsonValueKind.Object)
			{
				return;
			}

			foreach (JsonProperty property in element.EnumerateObject())
			{
				string segment = property.Name
					.Replace("~", "~0", StringComparison.Ordinal)
					.Replace("/", "~1", StringComparison.Ordinal);
				AddJsonStructure(property.Value, $"{path}/{segment}", signature);
			}
		}

		private static void AddStructureEntry(
			Dictionary<string, int> signature,
			string key)
		{
			signature.TryGetValue(key, out int count);
			signature[key] = count + 1;
		}

		private static ParsedDocument ParseIniDocument(string text)
		{
			ParsedDocument document = new();
			IdentityBuilder identities = new();
			string section = string.Empty;
			int lineStart = 0;

			while (lineStart <= text.Length)
			{
				int lineEnd = FindLineEnd(text, lineStart);
				int contentStart = lineStart;
				int contentEnd = lineEnd;
				TrimRange(text, ref contentStart, ref contentEnd);

				if (contentStart < contentEnd)
				{
					char first = text[contentStart];
					if (first == '[')
					{
						int closingBracket = text.IndexOf(']', contentStart + 1);
						if (closingBracket >= 0 && closingBracket < contentEnd)
						{
							section = text.Substring(
								contentStart + 1,
								closingBracket - contentStart - 1).Trim();
						}
					}
					else if (!IsCommentStart(text, contentStart, contentEnd))
					{
						int equalsIndex = FindTopLevelCharacter(
							text,
							contentStart,
							contentEnd,
							'=');
						if (equalsIndex > contentStart)
						{
							int keyStart = contentStart;
							int keyEnd = equalsIndex;
							TrimRange(text, ref keyStart, ref keyEnd);
							string key = text.Substring(keyStart, keyEnd - keyStart);
							int valueStart = equalsIndex + 1;
							int valueEnd = FindIniValueEnd(text, valueStart, lineEnd);
							TrimRange(text, ref valueStart, ref valueEnd);

							if (valueStart < valueEnd &&
								IsWrappedContainer(text, valueStart, valueEnd, '(', ')'))
							{
								int previousCount = document.Values.Count;
								ParseIniComposite(
									text,
									valueStart + 1,
									valueEnd - 1,
									key,
									section,
									document,
									identities);

								if (document.Values.Count == previousCount)
								{
									AddIniValue(
										text,
										valueStart,
										valueEnd,
										key,
										key,
										section,
										false,
										document,
										identities);
								}
							}
							else if (valueStart <= valueEnd)
							{
								AddIniValue(
									text,
									valueStart,
									valueEnd,
									key,
									key,
									section,
									false,
									document,
									identities);
							}
						}
					}
				}

				if (lineEnd >= text.Length)
				{
					break;
				}

				lineStart = lineEnd + 1;
				if (text[lineEnd] == '\r' && lineStart < text.Length && text[lineStart] == '\n')
				{
					lineStart++;
				}
			}

			return document;
		}

		private static void ParseIniComposite(
			string text,
			int start,
			int end,
			string parentPath,
			string section,
			ParsedDocument document,
			IdentityBuilder identities)
		{
			int segmentStart = start;
			int segmentIndex = 0;
			char quote = '\0';
			int roundDepth = 0;
			int squareDepth = 0;
			int braceDepth = 0;

			for (int index = start; index <= end; index++)
			{
				bool atEnd = index == end;
				char character = atEnd ? ',' : text[index];

				if (!atEnd && quote != '\0')
				{
					if (character == '\\' && index + 1 < end)
					{
						index++;
						continue;
					}
					if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (!atEnd && (character == '"' || character == '\''))
				{
					quote = character;
					continue;
				}

				if (!atEnd)
				{
					switch (character)
					{
						case '(': roundDepth++; break;
						case ')': roundDepth--; break;
						case '[': squareDepth++; break;
						case ']': squareDepth--; break;
						case '{': braceDepth++; break;
						case '}': braceDepth--; break;
					}
				}

				if (character == ',' &&
					roundDepth == 0 &&
					squareDepth == 0 &&
					braceDepth == 0)
				{
					ParseIniCompositeSegment(
						text,
						segmentStart,
						index,
						segmentIndex++,
						parentPath,
						section,
						document,
						identities);
					segmentStart = index + 1;
				}
			}
		}

		private static void ParseIniCompositeSegment(
			string text,
			int start,
			int end,
			int segmentIndex,
			string parentPath,
			string section,
			ParsedDocument document,
			IdentityBuilder identities)
		{
			TrimRange(text, ref start, ref end);
			if (start >= end)
			{
				return;
			}

			int equalsIndex = FindTopLevelCharacter(text, start, end, '=');
			string key;
			string valuePath;
			int valueStart;

			if (equalsIndex > start)
			{
				int keyStart = start;
				int keyEnd = equalsIndex;
				TrimRange(text, ref keyStart, ref keyEnd);
				key = text.Substring(keyStart, keyEnd - keyStart);
				valuePath = parentPath + "." + key;
				valueStart = equalsIndex + 1;
			}
			else
			{
				key = $"[{segmentIndex}]";
				valuePath = parentPath + key;
				valueStart = start;
			}

			int valueEnd = end;
			TrimRange(text, ref valueStart, ref valueEnd);
			if (valueStart >= valueEnd)
			{
				return;
			}

			if (IsWrappedContainer(text, valueStart, valueEnd, '(', ')'))
			{
				if (FindTopLevelCharacter(text, valueStart + 1, valueEnd - 1, '=') < 0)
				{
					AddIniValue(
						text,
						valueStart,
						valueEnd,
						key,
						valuePath,
						section,
						true,
						document,
						identities);
					return;
				}

				int previousCount = document.Values.Count;
				ParseIniComposite(
					text,
					valueStart + 1,
					valueEnd - 1,
					valuePath,
					section,
					document,
					identities);

				if (document.Values.Count > previousCount)
				{
					return;
				}
			}

			AddIniValue(
				text,
				valueStart,
				valueEnd,
				key,
				valuePath,
				section,
				true,
				document,
				identities);
		}

		private static void AddIniValue(
			string text,
			int start,
			int end,
			string key,
			string keyPath,
			string section,
			bool isCompositeValue,
			ParsedDocument document,
			IdentityBuilder identities)
		{
			ScalarStyle style = ScalarStyle.Raw;
			string token = text.Substring(start, end - start);
			string decodedValue = token;

			if (token.Length >= 2 && token[0] == token[^1] && token[0] == '"')
			{
				style = ScalarStyle.QuotedDouble;
				decodedValue = DecodeQuotedValue(token[1..^1], '"');
			}
			else if (token.Length >= 2 && token[0] == token[^1] && token[0] == '\'')
			{
				style = ScalarStyle.QuotedSingle;
				decodedValue = DecodeQuotedValue(token[1..^1], '\'');
			}

			string displayPath = string.IsNullOrWhiteSpace(section)
				? keyPath
				: $"[{section}] / {keyPath}";
			string logicalPath = section + "\u001f" + keyPath;
			ConfigValueType type = DetectValueType(keyPath, decodedValue);
			if (type == ConfigValueType.Boolean && TryParseBoolean(decodedValue, out bool booleanValue))
			{
				decodedValue = booleanValue ? "True" : "False";
			}

			document.Values.Add(new ParsedValue
			{
				Id = identities.Create("ini", logicalPath),
				Key = key,
				Path = displayPath,
				Section = section,
				Value = decodedValue,
				OriginalToken = token,
				Type = type,
				Style = style,
				Start = start,
				Length = end - start,
				IsCompositeValue = isCompositeValue
			});
		}

		private static ParsedDocument ParseSpaceDocument(string text)
		{
			ParsedDocument document = new();
			IdentityBuilder identities = new();
			int lineStart = 0;

			while (lineStart <= text.Length)
			{
				int lineEnd = FindLineEnd(text, lineStart);
				int start = lineStart;
				int end = lineEnd;
				TrimRange(text, ref start, ref end);

				if (start < end && !IsCommentStart(text, start, end))
				{
					int keyEnd = start;
					while (keyEnd < end && !char.IsWhiteSpace(text[keyEnd]))
					{
						keyEnd++;
					}

					if (keyEnd < end)
					{
						string key = text.Substring(start, keyEnd - start);
						int valueStart = keyEnd;
						int valueEnd = FindIniValueEnd(text, valueStart, lineEnd);
						TrimRange(text, ref valueStart, ref valueEnd);
						if (valueStart < valueEnd)
						{
							AddSpaceValue(
								text,
								valueStart,
								valueEnd,
								key,
								document,
								identities);
						}
					}
				}

				if (lineEnd >= text.Length)
				{
					break;
				}

				lineStart = lineEnd + 1;
				if (text[lineEnd] == '\r' && lineStart < text.Length && text[lineStart] == '\n')
				{
					lineStart++;
				}
			}

			return document;
		}

		private static void AddSpaceValue(
			string text,
			int start,
			int end,
			string key,
			ParsedDocument document,
			IdentityBuilder identities)
		{
			string token = text.Substring(start, end - start);
			ScalarStyle style = ScalarStyle.Raw;
			string decodedValue = token;

			if (token.Length >= 2 && token[0] == token[^1] && token[0] == '"')
			{
				style = ScalarStyle.QuotedDouble;
				decodedValue = DecodeQuotedValue(token[1..^1], '"');
			}
			else if (token.Length >= 2 && token[0] == token[^1] && token[0] == '\'')
			{
				style = ScalarStyle.QuotedSingle;
				decodedValue = DecodeQuotedValue(token[1..^1], '\'');
			}

			ConfigValueType type = DetectValueType(key, decodedValue);
			if (type == ConfigValueType.Boolean && TryParseBoolean(decodedValue, out bool booleanValue))
			{
				decodedValue = booleanValue ? "True" : "False";
			}

			document.Values.Add(new ParsedValue
			{
				Id = identities.Create("space", key),
				Key = key,
				Path = key,
				Value = decodedValue,
				OriginalToken = token,
				Type = type,
				Style = style,
				Start = start,
				Length = end - start
			});
		}

		private sealed class JsonConfigScanner
		{
			private readonly string _text;
			private readonly ParsedDocument _document = new();
			private readonly IdentityBuilder _identities = new();
			private int _index;

			public JsonConfigScanner(string text)
			{
				_text = text;
			}

			public ParsedDocument Parse()
			{
				SkipTrivia();
				if (_index >= _text.Length)
				{
					return _document;
				}

				ParseValue(string.Empty, string.Empty, string.Empty);
				SkipTrivia();
				if (_index != _text.Length)
				{
					throw new InvalidDataException(
						$"Unexpected JSON content at character {_index}.");
				}

				return _document;
			}

			private void ParseValue(string pointer, string displayPath, string key)
			{
				SkipTrivia();
				if (_index >= _text.Length)
				{
					throw new InvalidDataException("The JSON value ended unexpectedly.");
				}

				char character = _text[_index];
				switch (character)
				{
					case '{': ParseObject(pointer, displayPath); return;
					case '[': ParseArray(pointer, displayPath); return;
					case '"': AddStringValue(pointer, displayPath, key); return;
					case 't': ReadLiteral("true", pointer, displayPath, key, ScalarStyle.JsonBoolean); return;
					case 'f': ReadLiteral("false", pointer, displayPath, key, ScalarStyle.JsonBoolean); return;
					case 'n': ReadLiteral("null", pointer, displayPath, key, ScalarStyle.JsonNull); return;
					default:
						if (character == '-' || char.IsDigit(character))
						{
							AddNumberValue(pointer, displayPath, key);
							return;
						}
						break;
				}

				throw new InvalidDataException(
					$"Unsupported JSON value at character {_index}.");
			}

			private void ParseObject(string pointer, string displayPath)
			{
				_index++;
				SkipTrivia();
				if (TryConsume('}'))
				{
					return;
				}

				while (true)
				{
					SkipTrivia();
					JsonStringToken property = ReadStringToken();
					SkipTrivia();
					Expect(':');

					string childPointer = pointer + "/" + EscapeJsonPointer(property.Decoded);
					string childDisplayPath = string.IsNullOrWhiteSpace(displayPath)
						? property.Decoded
						: displayPath + "." + property.Decoded;
					ParseValue(childPointer, childDisplayPath, property.Decoded);
					SkipTrivia();

					if (TryConsume('}'))
					{
						return;
					}

					Expect(',');
					SkipTrivia();
					if (TryConsume('}'))
					{
						return;
					}
				}
			}

			private void ParseArray(string pointer, string displayPath)
			{
				_index++;
				SkipTrivia();
				if (TryConsume(']'))
				{
					return;
				}

				int itemIndex = 0;
				while (true)
				{
					string childPointer = pointer + "/" + itemIndex;
					string childDisplayPath = string.IsNullOrWhiteSpace(displayPath)
						? $"[{itemIndex}]"
						: $"{displayPath}[{itemIndex}]";
					ParseValue(childPointer, childDisplayPath, $"[{itemIndex}]");
					itemIndex++;
					SkipTrivia();

					if (TryConsume(']'))
					{
						return;
					}

					Expect(',');
					SkipTrivia();
					if (TryConsume(']'))
					{
						return;
					}
				}
			}

			private void AddStringValue(string pointer, string displayPath, string key)
			{
				JsonStringToken token = ReadStringToken();
				AddValue(
					pointer,
					displayPath,
					key,
					token.Decoded,
					token.Raw,
					token.Start,
					token.Length,
					ScalarStyle.JsonString);
			}

			private void AddNumberValue(string pointer, string displayPath, string key)
			{
				int start = _index;
				while (_index < _text.Length)
				{
					char character = _text[_index];
					if (char.IsWhiteSpace(character) ||
						character == ',' ||
						character == ']' ||
						character == '}' ||
						(character == '/' && _index + 1 < _text.Length &&
							(_text[_index + 1] == '/' || _text[_index + 1] == '*')))
					{
						break;
					}

					_index++;
				}

				string token = _text.Substring(start, _index - start);
				if (!IsValidNumber(token, true))
				{
					throw new InvalidDataException(
						$"Invalid JSON number at character {start}.");
				}

				AddValue(
					pointer,
					displayPath,
					key,
					token,
					token,
					start,
					token.Length,
					ScalarStyle.JsonNumber);
			}

			private void ReadLiteral(
				string literal,
				string pointer,
				string displayPath,
				string key,
				ScalarStyle style)
			{
				int start = _index;
				if (_index + literal.Length > _text.Length ||
					!_text.AsSpan(_index, literal.Length).SequenceEqual(literal.AsSpan()))
				{
					throw new InvalidDataException(
						$"Invalid JSON literal at character {_index}.");
				}

				_index += literal.Length;
				string value = style == ScalarStyle.JsonBoolean
					? (literal == "true" ? "True" : "False")
					: literal;
				AddValue(
					pointer,
					displayPath,
					key,
					value,
					literal,
					start,
					literal.Length,
					style);
			}

			private void AddValue(
				string pointer,
				string displayPath,
				string key,
				string value,
				string originalToken,
				int start,
				int length,
				ScalarStyle style)
			{
				string effectiveKey = string.IsNullOrWhiteSpace(displayPath)
					? key
					: displayPath;
				string section = GetParentPath(displayPath);
				ConfigValueType type = style switch
				{
					ScalarStyle.JsonBoolean => ConfigValueType.Boolean,
					ScalarStyle.JsonNumber => ConfigValueType.Number,
					ScalarStyle.JsonNull => ConfigValueType.Null,
					ScalarStyle.JsonString => IsSensitiveKey(effectiveKey)
						? ConfigValueType.Secret
						: ConfigValueType.Text,
					_ => DetectValueType(effectiveKey, value)
				};

				_document.Values.Add(new ParsedValue
				{
					Id = _identities.Create("json", pointer),
					Key = effectiveKey,
					Path = effectiveKey,
					Section = section,
					Value = value,
					OriginalToken = originalToken,
					Type = type,
					Style = style,
					Start = start,
					Length = length
				});
			}

			private JsonStringToken ReadStringToken()
			{
				if (_index >= _text.Length || _text[_index] != '"')
				{
					throw new InvalidDataException(
						$"Expected a JSON string at character {_index}.");
				}

				int start = _index++;
				bool escaped = false;
				while (_index < _text.Length)
				{
					char character = _text[_index++];
					if (escaped)
					{
						escaped = false;
						continue;
					}
					if (character == '\\')
					{
						escaped = true;
						continue;
					}
					if (character == '"')
					{
						string raw = _text.Substring(start, _index - start);
						string decoded = JsonSerializer.Deserialize<string>(raw)
							?? string.Empty;
						return new JsonStringToken(start, raw.Length, raw, decoded);
					}
				}

				throw new InvalidDataException(
					$"Unterminated JSON string at character {start}.");
			}

			private void SkipTrivia()
			{
				while (_index < _text.Length)
				{
					if (char.IsWhiteSpace(_text[_index]))
					{
						_index++;
						continue;
					}

					if (_index + 1 < _text.Length &&
						_text[_index] == '/' &&
						_text[_index + 1] == '/')
					{
						_index += 2;
						while (_index < _text.Length &&
							_text[_index] != '\r' &&
							_text[_index] != '\n')
						{
							_index++;
						}
						continue;
					}

					if (_index + 1 < _text.Length &&
						_text[_index] == '/' &&
						_text[_index + 1] == '*')
					{
						int commentEnd = _text.IndexOf("*/", _index + 2, StringComparison.Ordinal);
						if (commentEnd < 0)
						{
							throw new InvalidDataException("An unterminated JSON comment was found.");
						}
						_index = commentEnd + 2;
						continue;
					}

					break;
				}
			}

			private bool TryConsume(char character)
			{
				if (_index < _text.Length && _text[_index] == character)
				{
					_index++;
					return true;
				}
				return false;
			}

			private void Expect(char character)
			{
				if (!TryConsume(character))
				{
					throw new InvalidDataException(
						$"Expected '{character}' at JSON character {_index}.");
				}
			}

			private static string EscapeJsonPointer(string value)
			{
				return value.Replace("~", "~0").Replace("/", "~1");
			}

			private readonly record struct JsonStringToken(
				int Start,
				int Length,
				string Raw,
				string Decoded);
		}

		private static ParsedDocument ParseXmlDocument(string text)
		{
			ParsedDocument document = new();
			if (string.IsNullOrWhiteSpace(text))
			{
				return document;
			}

			XmlDocument validator = new() { PreserveWhitespace = true };
			validator.LoadXml(text);

			IdentityBuilder identities = new();
			Stack<XmlElementFrame> frames = new();
			int rootIndex = 0;
			int index = 0;

			while (index < text.Length)
			{
				int tagStart = text.IndexOf('<', index);
				if (tagStart < 0)
				{
					break;
				}

				if (text.AsSpan(tagStart).StartsWith("<!--".AsSpan(), StringComparison.Ordinal))
				{
					int commentEnd = text.IndexOf("-->", tagStart + 4, StringComparison.Ordinal);
					if (commentEnd < 0)
					{
						throw new InvalidDataException("An unterminated XML comment was found.");
					}
					if (frames.Count > 0)
					{
						frames.Peek().ContainsOtherMarkup = true;
					}
					index = commentEnd + 3;
					continue;
				}

				if (text.AsSpan(tagStart).StartsWith("<![CDATA[".AsSpan(), StringComparison.Ordinal))
				{
					int cdataStart = tagStart + 9;
					int cdataEnd = text.IndexOf("]]>", cdataStart, StringComparison.Ordinal);
					if (cdataEnd < 0)
					{
						throw new InvalidDataException("An unterminated XML CDATA section was found.");
					}

					if (frames.Count > 0)
					{
						XmlElementFrame frame = frames.Peek();
						string key = GetLeafName(frame.DisplayPath);
						string value = text.Substring(cdataStart, cdataEnd - cdataStart);
						document.Values.Add(CreateXmlValue(
							identities,
							frame.NumericPath + "#cdata",
							key,
							frame.DisplayPath,
							GetParentPath(frame.DisplayPath),
							value,
							value,
							ScalarStyle.XmlCData,
							cdataStart,
							cdataEnd - cdataStart));
						frame.HasExplicitTextEntry = true;
					}

					index = cdataEnd + 3;
					continue;
				}

				if (tagStart + 1 < text.Length && text[tagStart + 1] == '?')
				{
					int processingEnd = text.IndexOf("?>", tagStart + 2, StringComparison.Ordinal);
					if (processingEnd < 0)
					{
						throw new InvalidDataException("An unterminated XML declaration was found.");
					}
					if (frames.Count > 0)
					{
						frames.Peek().ContainsOtherMarkup = true;
					}
					index = processingEnd + 2;
					continue;
				}

				if (tagStart + 1 < text.Length && text[tagStart + 1] == '!')
				{
					int declarationEnd = FindXmlDeclarationEnd(text, tagStart + 2);
					if (frames.Count > 0)
					{
						frames.Peek().ContainsOtherMarkup = true;
					}
					index = declarationEnd + 1;
					continue;
				}

				if (tagStart + 1 < text.Length && text[tagStart + 1] == '/')
				{
					int closingEnd = FindXmlTagEnd(text, tagStart + 2);
					if (frames.Count > 0)
					{
						XmlElementFrame frame = frames.Pop();
						if (!frame.HasChildElements &&
							!frame.HasExplicitTextEntry &&
							!frame.ContainsOtherMarkup)
						{
							int valueStart = frame.ContentStart;
							int valueEnd = tagStart;
							TrimRange(text, ref valueStart, ref valueEnd);
							string token = text.Substring(valueStart, valueEnd - valueStart);
							string value = WebUtility.HtmlDecode(token);
							document.Values.Add(CreateXmlValue(
								identities,
								frame.NumericPath + "#text",
								GetLeafName(frame.DisplayPath),
								frame.DisplayPath,
								GetParentPath(frame.DisplayPath),
								value,
								token,
								ScalarStyle.XmlText,
								valueStart,
								valueEnd - valueStart));
						}
					}
					index = closingEnd + 1;
					continue;
				}

				int tagEnd = FindXmlTagEnd(text, tagStart + 1);
				int nameStart = tagStart + 1;
				while (nameStart < tagEnd && char.IsWhiteSpace(text[nameStart]))
				{
					nameStart++;
				}
				int nameEnd = nameStart;
				while (nameEnd < tagEnd &&
					!char.IsWhiteSpace(text[nameEnd]) &&
					text[nameEnd] != '/' &&
					text[nameEnd] != '>')
				{
					nameEnd++;
				}

				string elementName = text.Substring(nameStart, nameEnd - nameStart);
				bool selfClosing = IsSelfClosingXmlTag(text, tagStart, tagEnd);
				int childIndex;
				string numericPath;
				string displayPath;

				if (frames.Count > 0)
				{
					XmlElementFrame parent = frames.Peek();
					parent.HasChildElements = true;
					childIndex = parent.NextChildIndex++;
					numericPath = parent.NumericPath + "/" + childIndex;
					displayPath = parent.DisplayPath + "." + elementName;
				}
				else
				{
					childIndex = rootIndex++;
					numericPath = "/" + childIndex;
					displayPath = elementName;
				}

				List<XmlAttributeToken> attributes = ParseXmlAttributes(
					text,
					nameEnd,
					tagEnd);
				XmlAttributeToken? nameAttribute = attributes.FirstOrDefault(
					attribute => attribute.Name.Equals("name", StringComparison.OrdinalIgnoreCase));
				XmlAttributeToken? valueAttribute = attributes.FirstOrDefault(
					attribute => attribute.Name.Equals("value", StringComparison.OrdinalIgnoreCase));

				if (nameAttribute != null && valueAttribute != null)
				{
					string settingName = nameAttribute.Value;
					document.Values.Add(CreateXmlValue(
						identities,
						numericPath + "@" + valueAttribute.Name,
						settingName,
						displayPath + "." + settingName,
						displayPath,
						valueAttribute.Value,
						text.Substring(valueAttribute.ValueStart, valueAttribute.ValueLength),
						ScalarStyle.XmlAttribute,
						valueAttribute.ValueStart,
						valueAttribute.ValueLength));
				}

				foreach (XmlAttributeToken attribute in attributes)
				{
					if (attribute.Name.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase) ||
						(nameAttribute != null && valueAttribute != null &&
							(attribute == nameAttribute || attribute == valueAttribute)))
					{
						continue;
					}

					string attributePath = displayPath + ".@" + attribute.Name;
					document.Values.Add(CreateXmlValue(
						identities,
						numericPath + "@" + attribute.Name,
						attributePath,
						attributePath,
						displayPath,
						attribute.Value,
						text.Substring(attribute.ValueStart, attribute.ValueLength),
						ScalarStyle.XmlAttribute,
						attribute.ValueStart,
						attribute.ValueLength));
				}

				if (!selfClosing)
				{
					frames.Push(new XmlElementFrame
					{
						NumericPath = numericPath,
						DisplayPath = displayPath,
						ContentStart = tagEnd + 1
					});
				}

				index = tagEnd + 1;
			}

			return document;
		}

		private static ParsedValue CreateXmlValue(
			IdentityBuilder identities,
			string logicalPath,
			string key,
			string path,
			string section,
			string value,
			string originalToken,
			ScalarStyle style,
			int start,
			int length)
		{
			ConfigValueType type = DetectValueType(key, value);
			if (type == ConfigValueType.Boolean && TryParseBoolean(value, out bool booleanValue))
			{
				value = booleanValue ? "True" : "False";
			}

			return new ParsedValue
			{
				Id = identities.Create("xml", logicalPath),
				Key = key,
				Path = path,
				Section = section,
				Value = value,
				OriginalToken = originalToken,
				Type = type,
				Style = style,
				Start = start,
				Length = length
			};
		}

		private static List<XmlAttributeToken> ParseXmlAttributes(
			string text,
			int start,
			int tagEnd)
		{
			List<XmlAttributeToken> attributes = new();
			int index = start;

			while (index < tagEnd)
			{
				while (index < tagEnd && char.IsWhiteSpace(text[index]))
				{
					index++;
				}
				if (index >= tagEnd || text[index] == '/')
				{
					break;
				}

				int nameStart = index;
				while (index < tagEnd &&
					!char.IsWhiteSpace(text[index]) &&
					text[index] != '=' &&
					text[index] != '/' &&
					text[index] != '>')
				{
					index++;
				}
				string name = text.Substring(nameStart, index - nameStart);

				while (index < tagEnd && char.IsWhiteSpace(text[index]))
				{
					index++;
				}
				if (index >= tagEnd || text[index] != '=')
				{
					continue;
				}
				index++;
				while (index < tagEnd && char.IsWhiteSpace(text[index]))
				{
					index++;
				}
				if (index >= tagEnd || (text[index] != '"' && text[index] != '\''))
				{
					continue;
				}

				char quote = text[index++];
				int valueStart = index;
				while (index < tagEnd && text[index] != quote)
				{
					index++;
				}
				if (index >= tagEnd)
				{
					throw new InvalidDataException(
						$"The XML attribute '{name}' is missing its closing quote.");
				}

				string rawValue = text.Substring(valueStart, index - valueStart);
				attributes.Add(new XmlAttributeToken
				{
					Name = name,
					Value = WebUtility.HtmlDecode(rawValue),
					ValueStart = valueStart,
					ValueLength = index - valueStart
				});
				index++;
			}

			return attributes;
		}

		private static int FindXmlTagEnd(string text, int start)
		{
			char quote = '\0';
			for (int index = start; index < text.Length; index++)
			{
				char character = text[index];
				if (quote != '\0')
				{
					if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (character == '"' || character == '\'')
				{
					quote = character;
				}
				else if (character == '>')
				{
					return index;
				}
			}

			throw new InvalidDataException("An XML tag is missing its closing '>'.");
		}

		private static int FindXmlDeclarationEnd(string text, int start)
		{
			char quote = '\0';
			int bracketDepth = 0;
			for (int index = start; index < text.Length; index++)
			{
				char character = text[index];
				if (quote != '\0')
				{
					if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (character == '"' || character == '\'')
				{
					quote = character;
				}
				else if (character == '[')
				{
					bracketDepth++;
				}
				else if (character == ']')
				{
					bracketDepth = Math.Max(0, bracketDepth - 1);
				}
				else if (character == '>' && bracketDepth == 0)
				{
					return index;
				}
			}

			throw new InvalidDataException("An XML declaration is missing its closing '>'.");
		}

		private static bool IsSelfClosingXmlTag(string text, int tagStart, int tagEnd)
		{
			int index = tagEnd - 1;
			while (index > tagStart && char.IsWhiteSpace(text[index]))
			{
				index--;
			}
			return index > tagStart && text[index] == '/';
		}

		private static int FindLineEnd(string text, int start)
		{
			int carriageReturn = text.IndexOf('\r', start);
			int lineFeed = text.IndexOf('\n', start);
			if (carriageReturn < 0)
			{
				return lineFeed < 0 ? text.Length : lineFeed;
			}
			if (lineFeed < 0)
			{
				return carriageReturn;
			}
			return Math.Min(carriageReturn, lineFeed);
		}

		private static int FindIniValueEnd(string text, int start, int lineEnd)
		{
			char quote = '\0';
			int roundDepth = 0;
			int squareDepth = 0;
			int braceDepth = 0;

			for (int index = start; index < lineEnd; index++)
			{
				char character = text[index];
				if (quote != '\0')
				{
					if (character == '\\' && index + 1 < lineEnd)
					{
						index++;
					}
					else if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (character == '"' || character == '\'')
				{
					quote = character;
					continue;
				}

				switch (character)
				{
					case '(': roundDepth++; break;
					case ')': roundDepth--; break;
					case '[': squareDepth++; break;
					case ']': squareDepth--; break;
					case '{': braceDepth++; break;
					case '}': braceDepth--; break;
				}

				if (roundDepth == 0 && squareDepth == 0 && braceDepth == 0 &&
					IsInlineComment(text, index, start, lineEnd))
				{
					return index;
				}
			}

			return lineEnd;
		}

		private static bool IsInlineComment(
			string text,
			int index,
			int valueStart,
			int lineEnd)
		{
			if (index <= valueStart || !char.IsWhiteSpace(text[index - 1]))
			{
				return false;
			}

			return text[index] == ';' ||
				text[index] == '#' ||
				(text[index] == '/' && index + 1 < lineEnd && text[index + 1] == '/');
		}

		private static int FindTopLevelCharacter(
			string text,
			int start,
			int end,
			char target)
		{
			char quote = '\0';
			int roundDepth = 0;
			int squareDepth = 0;
			int braceDepth = 0;

			for (int index = start; index < end; index++)
			{
				char character = text[index];
				if (quote != '\0')
				{
					if (character == '\\' && index + 1 < end)
					{
						index++;
					}
					else if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (character == '"' || character == '\'')
				{
					quote = character;
					continue;
				}

				if (character == target &&
					roundDepth == 0 &&
					squareDepth == 0 &&
					braceDepth == 0)
				{
					return index;
				}

				switch (character)
				{
					case '(': roundDepth++; break;
					case ')': roundDepth--; break;
					case '[': squareDepth++; break;
					case ']': squareDepth--; break;
					case '{': braceDepth++; break;
					case '}': braceDepth--; break;
				}
			}

			return -1;
		}

		private static bool IsWrappedContainer(
			string text,
			int start,
			int end,
			char opening,
			char closing)
		{
			if (end - start < 2 || text[start] != opening || text[end - 1] != closing)
			{
				return false;
			}

			char quote = '\0';
			int depth = 0;
			for (int index = start; index < end; index++)
			{
				char character = text[index];
				if (quote != '\0')
				{
					if (character == '\\' && index + 1 < end)
					{
						index++;
					}
					else if (character == quote)
					{
						quote = '\0';
					}
					continue;
				}

				if (character == '"' || character == '\'')
				{
					quote = character;
				}
				else if (character == opening)
				{
					depth++;
				}
				else if (character == closing)
				{
					depth--;
					if (depth == 0 && index != end - 1)
					{
						return false;
					}
				}
			}

			return depth == 0 && quote == '\0';
		}

		private static bool IsCommentStart(string text, int start, int end)
		{
			return text[start] == ';' ||
				text[start] == '#' ||
				(text[start] == '/' && start + 1 < end && text[start + 1] == '/');
		}

		private static void TrimRange(string text, ref int start, ref int end)
		{
			while (start < end && char.IsWhiteSpace(text[start]))
			{
				start++;
			}
			while (end > start && char.IsWhiteSpace(text[end - 1]))
			{
				end--;
			}
		}

		private static string DecodeQuotedValue(string value, char quote)
		{
			StringBuilder builder = new(value.Length);
			for (int index = 0; index < value.Length; index++)
			{
				char character = value[index];
				if (character == '\\' && index + 1 < value.Length &&
					value[index + 1] == quote)
				{
					builder.Append(value[++index]);
				}
				else
				{
					builder.Append(character);
				}
			}
			return builder.ToString();
		}

		private static ConfigValueType DetectValueType(string key, string value)
		{
			if (IsSensitiveKey(key))
			{
				return ConfigValueType.Secret;
			}

			if (TryParseBoolean(value, out _))
			{
				return ConfigValueType.Boolean;
			}

			if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
			{
				return ConfigValueType.Null;
			}

			if (IsValidNumber(value, false))
			{
				return ConfigValueType.Number;
			}

			return ConfigValueType.Text;
		}

		private static bool TryParseBoolean(string value, out bool result)
		{
			return bool.TryParse(value.Trim(), out result);
		}

		private static bool IsSensitiveKey(string key)
		{
			string normalized = key.Replace("_", string.Empty)
				.Replace("-", string.Empty)
				.Replace(".", string.Empty)
				.ToLowerInvariant();

			return normalized.Contains("password") ||
				normalized.Contains("passwd") ||
				normalized.Contains("secret") ||
				normalized.Contains("token") ||
				normalized.Contains("apikey") ||
				normalized.Contains("webhook");
		}

		private static string GetParentPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			int dotIndex = path.LastIndexOf('.');
			int arrayIndex = path.LastIndexOf('[');
			int splitIndex = Math.Max(dotIndex, arrayIndex);
			return splitIndex > 0 ? path[..splitIndex] : string.Empty;
		}

		private static string GetLeafName(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			int dotIndex = path.LastIndexOf('.');
			return dotIndex >= 0 && dotIndex + 1 < path.Length
				? path[(dotIndex + 1)..]
				: path;
		}
	}
}
