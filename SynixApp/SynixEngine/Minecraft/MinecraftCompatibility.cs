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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal sealed record MinecraftDependency(string Id, string Range);
internal sealed record MinecraftAddOn(string File, string Id, string Version, string Loader,
	bool ClientOnly, IReadOnlyList<MinecraftDependency> Dependencies, bool Bundled = false);
internal sealed record MinecraftFinding(string Area, string Detail, bool DefiniteProblem = false);
internal sealed record MinecraftCompatibilityReport(IReadOnlyList<MinecraftAddOn> AddOns, IReadOnlyList<MinecraftFinding> Findings);

internal static class MinecraftCompatibility
{
	private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

	internal static MinecraftCompatibilityReport Scan(GameServer server, IReadOnlyList<MinecraftFileChange>? changes = null)
	{
		Dictionary<string, string> files = new(StringComparer.OrdinalIgnoreCase);
		foreach (string folder in MinecraftPluginRuntime.IsPluginRuntime(server.MinecraftLoader) ? new[] { "plugins" } :
			server.MinecraftLoader == "Vanilla" ? Array.Empty<string>() : new[] { "mods" })
		{
			string root = ModPathSafety.Resolve(server.InstallPath, folder);
			if (!Directory.Exists(root)) continue;
			foreach (string file in Directory.EnumerateFiles(root, "*.jar").Take(5001))
			{
				if (files.Count >= 5000) throw MinecraftContentTransactions.Error("Size");
				ModPathSafety.EnsureNoLinks(file);
				files[folder + "/" + Path.GetFileName(file)] = file;
			}
		}
		foreach (MinecraftFileChange change in changes ?? [])
		{
			if (!change.RelativePath.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)) continue;
			if (change.Source == null) files.Remove(change.RelativePath);
			else files[change.RelativePath] = change.Source;
		}
		List<MinecraftAddOn> addons = [];
		List<MinecraftFinding> findings = [];
		foreach ((string relative, string path) in files)
		{
			try
			{
				ModPathSafety.EnsureNoLinks(path);
				using ZipArchive zip = ZipFile.OpenRead(path);
				IReadOnlyList<MinecraftAddOn> metadata = ReadJar(zip, relative, server.MinecraftLoader, 0, new ReadBudget());
				if (metadata.Any(item => string.IsNullOrWhiteSpace(item.Id) || item.Id.Length > 200 || item.Id.Any(char.IsControl) ||
					string.IsNullOrWhiteSpace(item.Version) || item.Version.Length > 200 || item.Dependencies.Count > 5000 ||
					item.Dependencies.Any(dependency => string.IsNullOrWhiteSpace(dependency.Id) || dependency.Id.Length > 200 || dependency.Range.Length > 4096)))
					throw MinecraftContentTransactions.Error("Layout");
				addons.AddRange(metadata);
			}
			catch (Exception exception) when (exception is IOException or InvalidDataException or JsonException or FormatException or KeyNotFoundException or InvalidOperationException or RegexMatchTimeoutException)
			{
				findings.Add(new(relative, Text("MetadataUnknown")));
			}
		}
		findings.AddRange(Evaluate(server, addons));
		return new(addons, findings);
	}

	internal static IReadOnlyList<MinecraftFinding> Evaluate(GameServer server, IReadOnlyList<MinecraftAddOn> addons)
	{
		List<MinecraftFinding> findings = [];
		Dictionary<string, string> versions = new(StringComparer.OrdinalIgnoreCase)
		{
			["minecraft"] = server.GameVersion, ["java"] = server.RequiredJavaVersion.ToString(),
			[server.MinecraftLoader == "Fabric" ? "fabricloader" : server.MinecraftLoader.ToLowerInvariant()] = server.MinecraftLoaderVersion
		};
		foreach (MinecraftAddOn addon in addons)
			versions.TryAdd(addon.Id, addon.Version);
		foreach (IGrouping<string, MinecraftAddOn> duplicates in addons.Where(item => !item.Bundled)
			.GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
			findings.Add(new(duplicates.Key, Text("Duplicate"), true));
		foreach (MinecraftAddOn addon in addons)
		{
			if (addon.ClientOnly) findings.Add(new(addon.File, Text("ClientOnly"), true));
			bool loaderMatches = addon.Loader == "Plugin" ? MinecraftPluginRuntime.IsPluginRuntime(server.MinecraftLoader) :
				addon.Loader.Equals(server.MinecraftLoader, StringComparison.OrdinalIgnoreCase);
			if (!loaderMatches) findings.Add(new(addon.File, Text("WrongLoader"), true));
			foreach (MinecraftDependency dependency in addon.Dependencies)
			{
				if (!versions.TryGetValue(dependency.Id, out string? version))
					findings.Add(new(addon.File, Text("DependencyMissing", dependency.Id)));
				else
				{
					bool? matches = MatchesVersion(version, dependency.Range);
					if (matches != true) findings.Add(new(addon.File,
						Text(matches == false ? "DependencyVersion" : "DependencyReview", dependency.Id, dependency.Range, version)));
				}
			}
		}
		return findings;
	}

	private sealed class ReadBudget { internal long Bytes; internal int Count; }

	private static IReadOnlyList<MinecraftAddOn> ReadJar(ZipArchive zip, string file, string selectedLoader, int depth, ReadBudget budget)
	{
		if (zip.Entries.Count > 50000 || depth > 3 || ++budget.Count > 256) throw MinecraftContentTransactions.Error("Size");
		ZipArchiveEntry? fabric = zip.GetEntry("fabric.mod.json");
		ZipArchiveEntry? toml = zip.GetEntry(selectedLoader == "NeoForge" ? "META-INF/neoforge.mods.toml" : "META-INF/mods.toml")
			?? zip.GetEntry("META-INF/neoforge.mods.toml") ?? zip.GetEntry("META-INF/mods.toml");
		List<MinecraftAddOn> result = [];
		if (fabric != null && (selectedLoader == "Fabric" || toml == null))
		{
			using JsonDocument document = JsonDocument.Parse(MinecraftStaging.ReadSmall(fabric));
			JsonElement root = document.RootElement;
			List<MinecraftDependency> depends = [];
			if (root.TryGetProperty("depends", out JsonElement dependencies))
				foreach (JsonProperty dependency in dependencies.EnumerateObject())
					depends.Add(new(dependency.Name, dependency.Value.ValueKind == JsonValueKind.Array
						? string.Join(" || ", dependency.Value.EnumerateArray().Select(value => value.GetString())) : dependency.Value.GetString() ?? ""));
			string version = root.GetProperty("version").GetString() ?? "?";
			result.Add(new(file, root.GetProperty("id").GetString()!, version, "Fabric",
				root.TryGetProperty("environment", out JsonElement environment) && environment.GetString() == "client", depends, depth > 0));
			if (root.TryGetProperty("provides", out JsonElement provides))
				foreach (JsonElement alias in provides.EnumerateArray())
					result.Add(new(file, alias.GetString()!, version, "Fabric", false, [], true));
			if (root.TryGetProperty("jars", out JsonElement jars))
			{
				if (jars.GetArrayLength() > 256) throw MinecraftContentTransactions.Error("Size");
				foreach (JsonElement jar in jars.EnumerateArray())
				{
					string nestedName = jar.GetProperty("file").GetString() ?? "";
					if (!ModPathSafety.IsSafeRelativePath(nestedName)) throw MinecraftContentTransactions.Error("Layout");
					ZipArchiveEntry? nested = zip.GetEntry(nestedName);
					if (nested == null || nested.Length > 32 * 1024 * 1024) continue;
					if ((budget.Bytes += nested.Length) > 128 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
					using Stream input = nested.Open();
					using MemoryStream memory = new();
					ModPackageFiles.CopyExactBounded(input, memory, nested.Length);
					memory.Position = 0;
					using ZipArchive child = new(memory, ZipArchiveMode.Read);
					result.AddRange(ReadJar(child, file + "!" + nestedName, selectedLoader, depth + 1, budget));
				}
			}
			return result;
		}
		if (toml != null)
		{
			string text = MinecraftStaging.ReadSmall(toml);
			string loader = toml.FullName.EndsWith("neoforge.mods.toml", StringComparison.Ordinal) ? "NeoForge" : "Forge";
			List<MinecraftDependency> depends = [];
			foreach (Match section in Regex.Matches(text, @"(?ms)^\s*\[\[dependencies\.[^\]]+\]\](.*?)(?=^\s*\[|\z)", RegexOptions.None, RegexTimeout))
			{
				string body = section.Groups[1].Value;
				if (TomlValue(body, "side") == "CLIENT" || TomlValue(body, "type") is "optional" or "incompatible" or "discouraged" ||
					Regex.IsMatch(body, @"(?m)^\s*mandatory\s*=\s*false\b", RegexOptions.None, RegexTimeout)) continue;
				string id = TomlValue(body, "modId");
				if (id.Length > 0) depends.Add(new(id, TomlValue(body, "versionRange")));
			}
			foreach (Match section in Regex.Matches(text, @"(?ms)^\s*\[\[mods\]\](.*?)(?=^\s*\[|\z)", RegexOptions.None, RegexTimeout))
				result.Add(new(file, TomlValue(section.Groups[1].Value, "modId"),
					TomlValue(section.Groups[1].Value, "version"), loader, false, depends, depth > 0));
			if (result.Count > 0 && result.All(item => item.Id.Length > 0)) return result;
		}
		ZipArchiveEntry? plugin = zip.GetEntry("plugin.yml") ?? zip.GetEntry("paper-plugin.yml");
		if (plugin != null)
		{
			string text = MinecraftStaging.ReadSmall(plugin);
			// Use Synix's non-executing, format-aware YAML reader, including block lists.
			List<ConfigLine> values = ConfigHandler.LoadConfigText(text, ConfigFormat.YAML);
			string id = values.FirstOrDefault(value => value.Key == "name")?.Value ?? Path.GetFileNameWithoutExtension(file);
			string version = values.FirstOrDefault(value => value.Key == "version")?.Value ?? "?";
			List<MinecraftDependency> dependencies = [];
			foreach (Match match in Regex.Matches(text, @"(?m)^depend:\s*\[([^\]]*)\]", RegexOptions.None, RegexTimeout))
				dependencies.AddRange(match.Groups[1].Value.Split(',').Select(value => new MinecraftDependency(value.Trim().Trim('\'', '"'), "*")));
			Match block = Regex.Match(text, @"(?m)^depend:\s*\r?\n((?:[ \t]*-[^\n]*\n?)+)", RegexOptions.None, RegexTimeout);
			if (block.Success)
				dependencies.AddRange(block.Groups[1].Value.Split('\n', StringSplitOptions.RemoveEmptyEntries)
					.Select(value => new MinecraftDependency(value.Trim().TrimStart('-').Trim().Trim('\'', '"'), "*")));
			return [new(file, id, version, "Plugin", false, dependencies, depth > 0)];
		}
		throw MinecraftContentTransactions.Error("Layout");
	}

	private static string TomlValue(string text, string key)
	{
		Match value = Regex.Match(text, "(?m)^\\s*" + Regex.Escape(key) + "\\s*=\\s*[\\\"']([^\\\"'\\r\\n]*)[\\\"']", RegexOptions.None, RegexTimeout);
		return value.Success ? value.Groups[1].Value : "";
	}

	// Unknown/complex ranges remain 'review', never a compatibility pass.
	internal static bool? MatchesVersion(string version, string range)
	{
		if (range is "" or "*") return true;
		if (range.Contains("||", StringComparison.Ordinal))
		{
			bool?[] alternatives = range.Split("||").Select(part => MatchesVersion(version, part.Trim())).ToArray();
			return alternatives.Contains(true) ? true : alternatives.Contains(null) ? null : false;
		}
		if (range == version) return true;
		if (!TryVersion(version, out Version? current)) return null;
		if (range.StartsWith('[') || range.StartsWith('('))
		{
			if (!(range.EndsWith(']') || range.EndsWith(')'))) return null;
			string[] edges = range[1..^1].Split(',');
			if (edges.Length == 1) return TryVersion(edges[0], out Version? exact) ? current == exact : null;
			if (edges.Length != 2) return null;
			if (edges[0].Length > 0)
			{
				if (!TryVersion(edges[0], out Version? lower)) return null;
				if (current < lower || (current == lower && range[0] == '(')) return false;
			}
			if (edges[1].Length > 0)
			{
				if (!TryVersion(edges[1], out Version? upper)) return null;
				if (current > upper || (current == upper && range[^1] == ')')) return false;
			}
			return true;
		}
		bool unknown = false;
		foreach (string predicate in range.Split(' ', StringSplitOptions.RemoveEmptyEntries))
		{
			Match match = Regex.Match(predicate, @"^(>=|<=|>|<|=)?(\d+(?:\.\d+){0,3})(\.\*)?$", RegexOptions.None, RegexTimeout);
			if (!match.Success || !TryVersion(match.Groups[2].Value, out Version? target)) { unknown = true; continue; }
			bool satisfied = match.Groups[3].Success ? version == match.Groups[2].Value || version.StartsWith(match.Groups[2].Value + ".", StringComparison.Ordinal) :
				match.Groups[1].Value switch { ">=" => current >= target, "<=" => current <= target, ">" => current > target, "<" => current < target, _ => current == target };
			if (!satisfied) return false;
		}
		return unknown ? null : true;
	}

	private static bool TryVersion(string value, out Version? version)
	{
		version = null;
		if (!Regex.IsMatch(value, @"^\d+(\.\d+){0,3}$", RegexOptions.None, RegexTimeout)) return false;
		string[] parts = value.Split('.');
		return Version.TryParse(string.Join(".", parts.Concat(Enumerable.Repeat("0", 4 - parts.Length))), out version);
	}

	private static string Text(string key, params object[] args) => LocalizationManager.Get("MinecraftWorkspace.Check." + key, args);
}

internal static class MinecraftLogDiagnostics
{
	internal static IReadOnlyList<MinecraftFinding> Analyze(string log)
	{
		List<MinecraftFinding> result = [];
		foreach ((string key, string[] patterns) in new (string, string[])[]
		{
			("Java", ["UnsupportedClassVersionError", "class file version"]),
			("Memory", ["OutOfMemoryError", "Could not reserve enough space"]),
			("Dependency", ["Missing mandatory dependencies", "Incompatible mod set", "Missing or unsupported mandatory dependencies"]),
			("Port", ["FAILED TO BIND TO PORT", "Address already in use"]),
			("Eula", ["agree to the EULA"]),
			("Tick", ["Can't keep up!", "A single server tick took"]),
			("Mixin", ["MixinApplyError", "MixinTransformerError"]),
			("World", ["Failed to load level.dat", "Failed to read chunk", "Exception reading region"])
		})
			if (patterns.Any(pattern => log.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
				result.Add(new(LocalizationManager.Get("MinecraftWorkspace.Diagnostics"), LocalizationManager.Get("MinecraftWorkspace.Log." + key)));
		return result;
	}
}
