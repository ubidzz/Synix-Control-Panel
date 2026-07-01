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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml; // Added for XML parsing

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public enum ConfigFormat { StandardINI, Palworld, XML, JSON, Space }

	public class ConfigLine
	{
		public string Key { get; set; } = "";
		public string Value { get; set; } = "";
	}

	public static class ConfigHandler
	{
		// ==========================================
		// 1. MASTER LOAD ROUTER
		// ==========================================
		public static List<ConfigLine> LoadConfig(string path, ConfigFormat format)
		{
			switch (format)
			{
				case ConfigFormat.StandardINI: return LoadStandard(path);
				case ConfigFormat.JSON: return LoadJSON(path);
				case ConfigFormat.XML: return LoadXML(path);
				case ConfigFormat.Space: return LoadSpace(path);
				default: return new List<ConfigLine>();
			}
		}

		private static List<ConfigLine> LoadSpace(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			foreach (var line in File.ReadAllLines(path))
			{
				string trimmed = line.Trim();
				// Skip comments and empty lines
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
					continue;

				// Use Split by space, but only into 2 parts
				var parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 2)
				{
					settings.Add(new ConfigLine
					{
						Key = parts[0].Trim(),
						Value = parts[1].Trim().Trim('"') // This removes the quotes for the UI
					});
				}
			}
			return settings;
		}

		private static void SaveSpace(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			string[] originalLines = File.ReadAllLines(path);

			for (int i = 0; i < originalLines.Length; i++)
			{
				string trimmed = originalLines[i].Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
					continue;

				int firstSpace = trimmed.IndexOf(' ');
				if (firstSpace > 0)
				{
					string fileKey = trimmed.Substring(0, firstSpace).Trim();
					var matchingData = data.FirstOrDefault(d => d.Key == fileKey);

					if (matchingData != null)
					{
						originalLines[i] = $"{fileKey} \"{matchingData.Value}\"";
					}
				}
			}
			File.WriteAllLines(path, originalLines);
		}

		private static List<ConfigLine> LoadStandard(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			foreach (var line in File.ReadAllLines(path))
			{
				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
					continue;

				var kv = trimmed.Split(new[] { '=' }, 2);
				if (kv.Length == 2)
				{
					settings.Add(new ConfigLine { Key = kv[0].Trim(), Value = kv[1].Trim() });
				}
			}
			return settings;
		}

		private static List<ConfigLine> LoadJSON(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			try
			{
				byte[] rawBytes;
				using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					rawBytes = new byte[fs.Length];
					fs.Read(rawBytes, 0, (int)fs.Length);
				}

				string jsonString = System.Text.Encoding.UTF8.GetString(rawBytes);
				jsonString = jsonString.Replace("\0", "").Replace("\uFEFF", "").Replace("\uFFFE", "");

				int firstBracket = jsonString.IndexOf('{');
				int lastBracket = jsonString.LastIndexOf('}');
				if (firstBracket >= 0 && lastBracket > firstBracket)
				{
					jsonString = jsonString.Substring(firstBracket, lastBracket - firstBracket + 1);
				}

				if (string.IsNullOrWhiteSpace(jsonString) || !jsonString.StartsWith("{"))
					return settings;

				var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true });

				if (jsonNode is JsonObject jsonObj)
				{
					FlattenJsonNode(jsonObj, settings);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error reading JSON: {ex.Message}", "JSON Parser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return settings;
		}

		private static void FlattenJsonNode(JsonObject jsonObj, List<ConfigLine> settings)
		{
			foreach (var kvp in jsonObj)
			{
				if (kvp.Value is JsonObject innerObj)
				{
					FlattenJsonNode(innerObj, settings);
				}
				else if (kvp.Value is JsonArray)
				{
					continue;
				}
				else
				{
					string cleanValue = kvp.Value != null ? kvp.Value.GetValue<JsonElement>().ToString() : "";
					settings.Add(new ConfigLine { Key = kvp.Key, Value = cleanValue });
				}
			}
		}

		private static List<ConfigLine> LoadXML(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				XmlNodeList? properties = doc.SelectNodes("//property");
				if (properties != null)
				{
					foreach (XmlNode node in properties)
					{
						if (node.Attributes?["name"] != null && node.Attributes["value"] != null)
						{
							settings.Add(new ConfigLine
							{
								Key = node.Attributes["name"]!.Value,
								Value = node.Attributes["value"]!.Value
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error reading XML: {ex.Message}", "XML Parser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return settings;
		}

		// ==========================================
		// 2. MASTER SAVE ROUTER (NON-DESTRUCTIVE)
		// ==========================================
		public static void SaveConfig(string path, List<ConfigLine> data, ConfigFormat format)
		{
			switch (format)
			{
				case ConfigFormat.StandardINI: SaveStandard(path, data); break;
				case ConfigFormat.JSON: SaveJSON(path, data); break;
				case ConfigFormat.XML: SaveXML(path, data); break;
				case ConfigFormat.Space: SaveSpace(path, data); break;
			}
		}

		private static void SaveStandard(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			string[] originalLines = File.ReadAllLines(path);

			for (int i = 0; i < originalLines.Length; i++)
			{
				string trimmed = originalLines[i].Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
					continue;

				var kv = trimmed.Split(new[] { '=' }, 2);
				if (kv.Length == 2)
				{
					string fileKey = kv[0].Trim();
					var matchingData = data.FirstOrDefault(d => d.Key == fileKey);
					if (matchingData != null)
					{
						originalLines[i] = $"{fileKey}={matchingData.Value}";
					}
				}
			}

			File.WriteAllLines(path, originalLines);
		}

		private static void SaveJSON(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			try
			{
				string jsonString = File.ReadAllText(path).Replace("\0", "").Replace("\uFEFF", "");

				int firstBracket = jsonString.IndexOf('{');
				if (firstBracket >= 0) jsonString = jsonString.Substring(firstBracket);

				var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true });

				if (jsonNode is JsonObject jsonObj)
				{
					UpdateJsonNode(jsonObj, data);

					var options = new JsonSerializerOptions
					{
						WriteIndented = true,
						NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
					};

					File.WriteAllText(path, jsonNode.ToJsonString(options));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving JSON: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static void UpdateJsonNode(JsonObject jsonObj, List<ConfigLine> data)
		{
			foreach (var kvp in jsonObj.ToList())
			{
				if (kvp.Value is JsonObject innerObj)
				{
					UpdateJsonNode(innerObj, data);
				}
				else
				{
					var matchingData = data.FirstOrDefault(d => d.Key == kvp.Key);
					if (matchingData != null)
					{
						if (int.TryParse(matchingData.Value, out int intVal))
							jsonObj[kvp.Key] = intVal;
						else if (double.TryParse(matchingData.Value, out double dblVal))
							jsonObj[kvp.Key] = dblVal;
						else if (bool.TryParse(matchingData.Value, out bool boolVal))
							jsonObj[kvp.Key] = boolVal;
						else
							jsonObj[kvp.Key] = matchingData.Value;
					}
				}
			}
		}

		private static void SaveXML(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				XmlNodeList? properties = doc.SelectNodes("//property");
				if (properties != null)
				{
					foreach (XmlNode node in properties)
					{
						if (node.Attributes?["name"] != null && node.Attributes["value"] != null)
						{
							string fileKey = node.Attributes["name"]!.Value;
							var matchingData = data.FirstOrDefault(d => d.Key == fileKey);

							if (matchingData != null)
							{
								node.Attributes["value"]!.Value = matchingData.Value;
							}
						}
					}
				}
				doc.Save(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving XML: {ex.Message}", "XML Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
