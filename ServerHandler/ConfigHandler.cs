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

namespace Synix_Control_Panel.ServerHandler
{
	public enum ConfigFormat { StandardINI, Palworld, XML, JSON }

	public class ConfigLine
	{
		public string RawLine { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
		public bool IsSetting => !string.IsNullOrEmpty(Key);
		public bool IsHeader => RawLine != null && RawLine.Trim().StartsWith("[");
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
				case ConfigFormat.Palworld:
					return LoadPalworld(path);
				case ConfigFormat.StandardINI:
					return LoadStandard(path);
				case ConfigFormat.JSON:
					return LoadJSON(path);
				default:
					return new List<ConfigLine>();
			}
		}

		private static List<ConfigLine> LoadPalworld(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			var lines = File.ReadAllLines(path);
			foreach (var line in lines)
			{
				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";")) continue;

				if (trimmed.StartsWith("OptionSettings=("))
				{
					string inner = trimmed.Replace("OptionSettings=(", "").TrimEnd(')');
					var parts = inner.Split(',');
					foreach (var part in parts)
					{
						var kv = part.Split('=');
						if (kv.Length == 2)
						{
							settings.Add(new ConfigLine { Key = kv[0].Trim(), Value = kv[1].Trim() });
						}
					}
				}
			}
			return settings;
		}

		private static List<ConfigLine> LoadStandard(string path)
		{
			var settings = new List<ConfigLine>();
			if (!File.Exists(path)) return settings;

			foreach (var line in File.ReadAllLines(path))
			{
				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("[") || trimmed.StartsWith(";")) continue;

				var kv = trimmed.Split('=', 2);
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

			if (!File.Exists(path))
			{
				MessageBox.Show($"PATH ERROR: Synix cannot find the file at:\n\n{path}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return settings;
			}

			try
			{
				// 1. Force read the file ignoring all Windows locks
				string jsonString = "";
				using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var textReader = new StreamReader(fileStream))
				{
					jsonString = textReader.ReadToEnd();
				}

				// Diagnostic Check #1: Did we actually get text?
				if (jsonString.Length < 10)
				{
					MessageBox.Show($"FILE IS EMPTY: Synix found the file, but it only contains {jsonString.Length} characters.\n\nContents: '{jsonString}'\n\nAre you sure you are looking at the right folder?", "Empty File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return settings;
				}

				// Clean up invisible garbage
				jsonString = jsonString.Replace("\0", "").Replace("\uFEFF", "");

				// Slice brackets
				int firstBracket = jsonString.IndexOf('{');
				int lastBracket = jsonString.LastIndexOf('}');

				if (firstBracket >= 0 && lastBracket > firstBracket)
				{
					jsonString = jsonString.Substring(firstBracket, lastBracket - firstBracket + 1);
				}

				// Parse JSON
				var docOptions = new JsonDocumentOptions { AllowTrailingCommas = true };
				var jsonNode = JsonNode.Parse(jsonString, documentOptions: docOptions);

				if (jsonNode is JsonObject jsonObj)
				{
					// Dive into Soulmask's "0" folder
					if (jsonObj.ContainsKey("0") && jsonObj["0"] is JsonObject innerObj)
					{
						foreach (var kvp in innerObj)
						{
							string cleanValue = kvp.Value != null ? kvp.Value.GetValue<JsonElement>().ToString() : "";
							settings.Add(new ConfigLine { Key = kvp.Key, Value = cleanValue });
						}
						// Diagnostic Check #2: Success!
						MessageBox.Show($"SUCCESS: Successfully read {settings.Count} settings from the '0' object!", "Parsed Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						// Standard JSON
						foreach (var kvp in jsonObj)
						{
							string cleanValue = kvp.Value != null ? kvp.Value.GetValue<JsonElement>().ToString() : "";
							settings.Add(new ConfigLine { Key = kvp.Key, Value = cleanValue });
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"CRASH IN LOADJSON:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Major Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return settings;
		}

		// ==========================================
		// 2. MASTER SAVE ROUTER
		// ==========================================
		public static void SaveConfig(string path, List<ConfigLine> data, ConfigFormat format)
		{
			var lines = new List<string>();

			switch (format)
			{
				case ConfigFormat.Palworld:
					lines.Add("[/Script/Pal.PalGameWorldSettings]");
					string combined = string.Join(",", data.Select(c => $"{c.Key}={c.Value}"));
					lines.Add($"OptionSettings=({combined})");
					File.WriteAllLines(path, lines);
					break;

				case ConfigFormat.StandardINI:
					foreach (var line in data)
					{
						lines.Add($"{line.Key}={line.Value}");
					}
					File.WriteAllLines(path, lines);
					break;

				case ConfigFormat.JSON:
					SaveJSON(path, data);
					break;
			}
		}

		private static void SaveJSON(string path, List<ConfigLine> data)
		{
			try
			{
				string jsonString = "{\"0\":{}}";

				if (File.Exists(path))
				{
					using (var reader = new StreamReader(path, detectEncodingFromByteOrderMarks: true))
					{
						jsonString = reader.ReadToEnd().Replace("\0", "");

						// THE ULTIMATE FIX FOR SAVING
						int firstBracket = jsonString.IndexOf('{');
						if (firstBracket > 0)
						{
							jsonString = jsonString.Substring(firstBracket);
						}
					}

					if (string.IsNullOrWhiteSpace(jsonString))
					{
						jsonString = "{\"0\":{}}";
					}
				}

				var docOptions = new JsonDocumentOptions { AllowTrailingCommas = true };
				var jsonNode = JsonNode.Parse(jsonString, documentOptions: docOptions) as JsonObject ?? new JsonObject();

				JsonObject targetNode = jsonNode;

				// SOULMASK FIX
				if (jsonNode.ContainsKey("0") && jsonNode["0"] is JsonObject innerObj)
				{
					targetNode = innerObj;
				}

				foreach (var line in data)
				{
					if (int.TryParse(line.Value, out int intVal))
						targetNode[line.Key] = intVal;
					else if (double.TryParse(line.Value, out double dblVal))
						targetNode[line.Key] = dblVal;
					else if (bool.TryParse(line.Value, out bool boolVal))
						targetNode[line.Key] = boolVal;
					else
						targetNode[line.Key] = line.Value;
				}

				var options = new JsonSerializerOptions { WriteIndented = true };
				File.WriteAllText(path, jsonNode.ToJsonString(options));
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving JSON: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}