using System.Xml;
using System.Xml.Linq;
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixEngine;

/// <summary>Inventory only: never scans a whole profile or guesses ownership from "Synix" in a filename.</summary>
internal sealed class SynixAppCleanup
{
	internal List<string> Folders { get; } = [];
	internal List<string> Files { get; } = [];
	internal List<string> PreferenceFolders { get; } = [];
	internal List<string> EmptyContainers { get; } = [];
	internal List<string> BackupRoots { get; } = [];
	internal List<string> RecoveryFolders { get; } = [];

	internal static SynixAppCleanup Discover(SynixUninstallPaths paths)
	{
		SynixAppCleanup inventory = new();
		inventory.Folders.Add(paths.LocalData);
		inventory.EmptyContainers.Add(paths.LocalData);
		// These are server recovery journals, not disposable app preferences.
		inventory.RecoveryFolders.Add(Path.Combine(paths.LocalData, "RestoreOperations"));
		inventory.BackupRoots.Add(Path.Combine(paths.Root, "BackupGames"));
		if (!string.IsNullOrWhiteSpace(paths.CustomBackups)) inventory.BackupRoots.Add(paths.CustomBackups);
		foreach (string settings in new[] { paths.UserSettings, paths.RoamingSettings }
			.OfType<string>().Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase))
		{
			// ConfigurationManager supplies the current version's folder. Only other
			// versions of THAT application identity are ours; sibling identities may
			// belong to a portable copy or another installed application.
			ModPathSafety.EnsureNoLinks(settings);
			string identity = Path.GetDirectoryName(settings)!;
			if (!Version.TryParse(Path.GetFileName(settings), out _)) throw SynixUninstallPlan.Unsafe(settings);
			foreach (string version in Directories(identity))
			{
				if (!Version.TryParse(Path.GetFileName(version), out _)) continue;
				ModPathSafety.EnsureNoLinks(version);
				inventory.Folders.Add(version);
				inventory.PreferenceFolders.Add(version);
				string config = Path.Combine(version, "user.config");
				ModPathSafety.EnsureNoLinks(config);
				if (!File.Exists(config)) continue;
				using XmlReader reader = XmlReader.Create(config, new XmlReaderSettings
				{
					DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 1024 * 1024
				});
				// Read only backup locations, never deserialize or log credentials.
				XElement? section = XDocument.Load(reader).Root?.Element("userSettings")?
					.Element("Synix_Control_Panel.Properties.Settings");
				foreach (XElement setting in section?.Elements("setting") ?? [])
				{
					string? location = setting.Element("value")?.Value;
					if ((string?)setting.Attribute("name") == "CustomBackupPath" && !string.IsNullOrWhiteSpace(location))
						inventory.BackupRoots.Add(location);
				}
			}
			inventory.EmptyContainers.Add(identity);
		}

		if (paths.Temp is not null)
		{
			ModPathSafety.EnsureNoLinks(paths.Temp);
			foreach (string entry in Entries(paths.Temp))
			{
				string name = Path.GetFileName(entry);
				if (IsOperationName(name, "SynixAddOn-", "") || IsOperationName(name, "SynixMinecraft-", "") ||
					IsOperationName(name, "synix-import-", ".stage"))
					inventory.Folders.Add(entry);
				else if (IsOperationName(name, ".synix-", ".zip.tmp") || IsOperationName(name, "synix-verify-", ".zip.tmp") ||
					IsOperationName(name, "synix-import-", ".zip.tmp"))
					inventory.Files.Add(entry);
			}
			string oxide = Path.Combine(paths.Temp, "Synix", "Oxide");
			foreach (string operation in Directories(oxide))
			{
				if (!Guid.TryParseExact(Path.GetFileName(operation), "N", out _)) continue;
				inventory.Folders.Add(operation);
				// Failed framework installs deliberately retain this rollback copy.
				inventory.RecoveryFolders.Add(Path.Combine(operation, "rollback"));
			}
			inventory.EmptyContainers.Add(oxide);
			inventory.EmptyContainers.Add(Path.Combine(paths.Temp, "Synix"));
		}
		return inventory;
	}

	internal static bool IsOperationName(string name, string prefix, string suffix) =>
		name.Length == prefix.Length + 32 + suffix.Length &&
		name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
		Guid.TryParseExact(name.Substring(prefix.Length, 32), "N", out _);

	internal static IEnumerable<string> Entries(string folder)
	{
		ModPathSafety.EnsureNoLinks(folder);
		if (File.Exists(folder)) throw SynixUninstallPlan.Unsafe(folder);
		// Exists can hide access failures. Only absence means an empty inventory.
		try { return Directory.GetFileSystemEntries(folder); }
		catch (DirectoryNotFoundException) { return []; }
	}

	private static IEnumerable<string> Directories(string folder) =>
		Entries(folder).Where(path => (File.GetAttributes(path) & FileAttributes.Directory) != 0);
}
