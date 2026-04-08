// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using System.Xml; // Added for XML parsing

namespace Synix_Control_Panel.ServerHandler
{
	public enum ConfigFormat { StandardINI, Palworld, XML, JSON }

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
				case ConfigFormat.Palworld: return LoadPalworld(path);
				case ConfigFormat.StandardINI: return LoadStandard(path);
				case ConfigFormat.JSON: return LoadJSON(path);
				case ConfigFormat.XML: return LoadXML(path);
				default: return new List<ConfigLine>();
			}
		}

		private static List<ConfigLine> LoadStandard(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			foreach (var line in File.ReadAllLines(path))
			{
				string trimmed = line.Trim();
				// Ignore comments and headers
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
					continue;

				var kv = trimmed.Split(new[] { '=' }, 2); // Split only on the FIRST equals sign
				if (kv.Length == 2)
				{
					settings.Add(new ConfigLine { Key = kv[0].Trim(), Value = kv[1].Trim() });
				}
			}
			return settings;
		}

		private static List<ConfigLine> LoadPalworld(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			foreach (var line in File.ReadAllLines(path))
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("OptionSettings=("))
				{
					string inner = trimmed.Replace("OptionSettings=(", "").TrimEnd(')');
					var parts = inner.Split(',');
					foreach (var part in parts)
					{
						var kv = part.Split('=');
						if (kv.Length == 2) settings.Add(new ConfigLine { Key = kv[0].Trim(), Value = kv[1].Trim() });
					}
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
				// Raw byte reader to bypass file locks and engine corruption
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

				// Dynamically flatten ANY JSON structure
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

		// Recursive function to dig through folders inside JSON files
		private static void FlattenJsonNode(JsonObject jsonObj, List<ConfigLine> settings)
		{
			foreach (var kvp in jsonObj)
			{
				if (kvp.Value is JsonObject innerObj)
				{
					FlattenJsonNode(innerObj, settings); // Dig deeper!
				}
				else if (kvp.Value is JsonArray)
				{
					continue; // Ignore raw arrays in the UI for now
				}
				else
				{
					string cleanValue = kvp.Value != null ? kvp.Value.GetValue<JsonElement>().ToString() : "";
					settings.Add(new ConfigLine { Key = kvp.Key, Value = cleanValue });
				}
			}
		}

		// XML PARSER
		private static List<ConfigLine> LoadXML(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				// Most game servers (like 7DTD) use: <property name="ServerName" value="My Server"/>
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
				case ConfigFormat.Palworld: SavePalworld(path, data); break;
				case ConfigFormat.StandardINI: SaveStandard(path, data); break;
				case ConfigFormat.JSON: SaveJSON(path, data); break;
				case ConfigFormat.XML: SaveXML(path, data); break;
			}
		}

		private static void SaveStandard(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			// Read all lines to PRESERVE comments and headers
			string[] originalLines = File.ReadAllLines(path);

			for (int i = 0; i < originalLines.Length; i++)
			{
				string trimmed = originalLines[i].Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
					continue; // Skip comments, don't delete them!

				var kv = trimmed.Split(new[] { '=' }, 2);
				if (kv.Length == 2)
				{
					string fileKey = kv[0].Trim();
					// Find if the UI changed this setting
					var matchingData = data.FirstOrDefault(d => d.Key == fileKey);
					if (matchingData != null)
					{
						// Inject the new value perfectly
						originalLines[i] = $"{fileKey}={matchingData.Value}";
					}
				}
			}

			// Overwrite file with the preserved lines + injected edits
			File.WriteAllLines(path, originalLines);
		}

		private static void SavePalworld(string path, List<ConfigLine> data)
		{
			if (!File.Exists(path)) return;

			string[] lines = File.ReadAllLines(path);
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Trim().StartsWith("OptionSettings=("))
				{
					string combined = string.Join(",", data.Select(c => $"{c.Key}={c.Value}"));
					lines[i] = $"OptionSettings=({combined})"; // Safely update just this line
					break;
				}
			}
			File.WriteAllLines(path, lines);
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
					// Dynamically inject values without destroying structure
					UpdateJsonNode(jsonObj, data);

					var options = new JsonSerializerOptions { WriteIndented = true };
					File.WriteAllText(path, jsonNode.ToJsonString(options));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving JSON: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Recursive function to find the exact node and update it safely
		private static void UpdateJsonNode(JsonObject jsonObj, List<ConfigLine> data)
		{
			foreach (var kvp in jsonObj.ToList()) // ToList prevents modification errors
			{
				if (kvp.Value is JsonObject innerObj)
				{
					UpdateJsonNode(innerObj, data); // Dig deeper!
				}
				else
				{
					var matchingData = data.FirstOrDefault(d => d.Key == kvp.Key);
					if (matchingData != null)
					{
						// Inject the proper data type so the game engine doesn't crash
						if (int.TryParse(matchingData.Value, out int intVal))
							jsonObj[kvp.Key] = intVal;
						else if (double.TryParse(matchingData.Value, out double dblVal))
							jsonObj[kvp.Key] = dblVal;
						else if (bool.TryParse(matchingData.Value, out bool boolVal))
							jsonObj[kvp.Key] = boolVal;
						else
							jsonObj[kvp.Key] = matchingData.Value; // String fallback
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
								// Inject the new value perfectly into the XML attribute
								node.Attributes["value"]!.Value = matchingData.Value;
							}
						}
					}
				}
				doc.Save(path); // Saves while perfectly preserving all XML tags and comments
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving XML: {ex.Message}", "XML Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}