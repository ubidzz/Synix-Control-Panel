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
	public static partial class ConfigHandler
	{
		internal static bool EnsureStandardIniTupleValues(
			string path,
			string tupleKey,
			IReadOnlyDictionary<string, string> requiredValues)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException(LocalizationManager.Get(
					"Configuration.Editor.Error.FileNotFound"), path);
			if (string.IsNullOrWhiteSpace(tupleKey) ||
				tupleKey.IndexOfAny(['=', '(', ')', '\r', '\n']) >= 0)
			{
				throw new InvalidDataException(LocalizationManager.Get(
					"Configuration.Editor.Error.IniTupleKey"));
			}

			ConfigurationTextSnapshot snapshot = ConfigurationTextSnapshot.Read(path);
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
					throw new InvalidDataException(LocalizationManager.Get(
						"Configuration.Editor.Error.IniTupleValue"));
				}
			}

			string marker = tupleKey + "=(";
			int markerIndex = snapshot.Text.IndexOf(marker, StringComparison.Ordinal);
			if (markerIndex < 0 ||
				snapshot.Text.IndexOf(marker, markerIndex + marker.Length, StringComparison.Ordinal) >= 0)
			{
				throw new InvalidDataException(
					LocalizationManager.Get(
						"Configuration.Editor.Error.IniTupleCount",
						tupleKey));
			}

			int lineEnd = snapshot.Text.IndexOfAny(['\r', '\n'], markerIndex);
			if (lineEnd < 0)
				lineEnd = snapshot.Text.Length;
			int closingIndex = snapshot.Text.LastIndexOf(
				')',
				lineEnd - 1,
				lineEnd - markerIndex);
			if (closingIndex < markerIndex + marker.Length)
				throw new InvalidDataException(LocalizationManager.Get(
					"Configuration.Editor.Error.IniTupleIncomplete",
					tupleKey));

			string separator = closingIndex > markerIndex + marker.Length ? "," : string.Empty;
			string insertion = separator + string.Join(
				",",
				missing.Select(required => $"{required.Key}={required.Value}"));
			string updated = snapshot.Text.Insert(closingIndex, insertion);
			ConfigurationFileWriter.WriteAtomically(path, snapshot.Encode(updated));
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

	}
}
