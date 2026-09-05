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
						LocalizationManager.Get(
							"Configuration.Editor.Error.JsonUnexpectedContent",
							_index));
				}

				return _document;
			}

			private void ParseValue(string pointer, string displayPath, string key)
			{
				SkipTrivia();
				if (_index >= _text.Length)
				{
					throw new InvalidDataException(LocalizationManager.Get(
						"Configuration.Editor.Error.JsonUnexpectedEnd"));
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
					LocalizationManager.Get(
						"Configuration.Editor.Error.JsonUnsupportedValue",
						_index));
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
						LocalizationManager.Get(
							"Configuration.Editor.Error.JsonInvalidNumber",
							start));
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
						LocalizationManager.Get(
							"Configuration.Editor.Error.JsonInvalidLiteral",
							_index));
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
						LocalizationManager.Get(
							"Configuration.Editor.Error.JsonStringExpected",
							_index));
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
					LocalizationManager.Get(
						"Configuration.Editor.Error.JsonStringUnterminated",
						start));
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
							throw new InvalidDataException(LocalizationManager.Get(
								"Configuration.Editor.Error.JsonCommentUnterminated"));
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
						LocalizationManager.Get(
							"Configuration.Editor.Error.JsonCharacterExpected",
							character,
							_index));
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
	}
}
