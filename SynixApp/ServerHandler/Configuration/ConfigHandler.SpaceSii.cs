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

		private static ParsedDocument ParseSiiDocument(string text)
		{
			ParsedDocument document = new();
			IdentityBuilder identities = new();
			int lineStart = 0;

			while (lineStart <= text.Length)
			{
				int lineEnd = FindLineEnd(text, lineStart);
				int contentStart = lineStart;
				int contentEnd = lineEnd;
				TrimRange(text, ref contentStart, ref contentEnd);

				if (contentStart < contentEnd &&
					!IsCommentStart(text, contentStart, contentEnd) &&
					text[contentStart] is not '{' and not '}')
				{
					int colonIndex = FindTopLevelCharacter(
						text,
						contentStart,
						contentEnd,
						':');
					if (colonIndex > contentStart)
					{
						int keyStart = contentStart;
						int keyEnd = colonIndex;
						TrimRange(text, ref keyStart, ref keyEnd);
						string key = text.Substring(keyStart, keyEnd - keyStart);
						int valueStart = colonIndex + 1;
						int valueEnd = FindIniValueEnd(text, valueStart, lineEnd);
						TrimRange(text, ref valueStart, ref valueEnd);
						if (valueStart < valueEnd)
						{
							AddIniValue(
								text,
								valueStart,
								valueEnd,
								key,
								key,
								string.Empty,
								false,
								document,
								identities);
						}
					}
				}

				if (lineEnd >= text.Length)
					break;

				lineStart = lineEnd + 1;
				if (text[lineEnd] == '\r' &&
					lineStart < text.Length &&
					text[lineStart] == '\n')
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
	}
}
