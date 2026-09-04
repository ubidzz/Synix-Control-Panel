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
						throw new InvalidDataException(LocalizationManager.Get(
							"Configuration.Editor.Error.XmlCommentUnterminated"));
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
						throw new InvalidDataException(LocalizationManager.Get(
							"Configuration.Editor.Error.XmlCDataUnterminated"));
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
						throw new InvalidDataException(LocalizationManager.Get(
							"Configuration.Editor.Error.XmlDeclarationUnterminated"));
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
						LocalizationManager.Get(
							"Configuration.Editor.Error.XmlAttributeQuote",
							name));
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

			throw new InvalidDataException(LocalizationManager.Get(
				"Configuration.Editor.Error.XmlTagClose"));
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

			throw new InvalidDataException(LocalizationManager.Get(
				"Configuration.Editor.Error.XmlDeclarationClose"));
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
	}
}
