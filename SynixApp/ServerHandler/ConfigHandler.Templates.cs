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
		internal static bool HasRequiredStructure(
			string path,
			string template,
			ConfigFormat format)
		{
			if (!File.Exists(path))
			{
				return false;
			}

			string existingText = ConfigurationTextSnapshot.Read(path).Text;
			return HasRequiredStructureText(existingText, template, format);
		}

		internal static bool HasRequiredStructureText(
			string existingText,
			string template,
			ConfigFormat format)
		{
			ArgumentNullException.ThrowIfNull(existingText);
			ArgumentNullException.ThrowIfNull(template);
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
	}
}
