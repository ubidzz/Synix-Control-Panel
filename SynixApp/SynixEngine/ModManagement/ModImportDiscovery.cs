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
using System.IO.Compression;
using Synix_Control_Panel.SynixApp.Database;

namespace Synix_Control_Panel.SynixEngine.ModManagement;

internal sealed record ModImportChoice(ModSystemProfile Profile, ModInstallTarget Target, string Selection,
	bool Ready, bool RememberLocation = false)
{
	public string Label => LocalizationManager.TranslateKnownText(Target.DisplayName) + " — " +
		(Target.CanManageIds ? Target.ProviderName : Target.RelativePath);
}
internal sealed record ModImportAnalysis(string PackageSha256, IReadOnlyList<string> Files,
	IReadOnlyList<ModImportChoice> Choices, IReadOnlyList<ArkModPackageInfo> ProviderMods);

// Reading is independent of the selected inventory tab. No destination is created,
// no profile is saved and no downloaded code is executed during discovery.
internal static class ModImportDiscovery
{
	internal static ModInstallTarget SnapshotTarget { get; } = new()
	{
		PackageLayout = ModPackageLayout.FolderTree, AllowArchives = true, AllowFolderImport = true,
		PreserveArchiveContents = true, RelativePath = "ImportPreview",
		AllowedExtensions = [".uplugin", ".mod", ".jar", ".dll", ".cs", ".lua", ".js", ".py", ".so",
			".asi", ".smx", ".amxx", ".vpk", ".pak", ".bsp", ".json", ".xml", ".yaml", ".yml",
			".toml", ".cfg", ".ini", ".txt", ".dat", ".gma", ".tmod", ".mpk", ".pbo",
			".rfcmp", ".rfmod", ".as", ".pas", ".jbeam", ".dae", ".dds", ".zip"]
	};

	internal static ModImportAnalysis Read(GameServer server, string source, CancellationToken cancellationToken = default)
	{
		ModPathSafety.EnsureNoLinks(source);
		using FileStream sourceLock = new(source, FileMode.Open, FileAccess.Read, FileShare.Read);
		// This preliminary preview validates archive paths and captures the bytes reviewed.
		ModPackagePreview read = UniversalModPackage.Preview(server, SnapshotTarget, source, cancellationToken: cancellationToken);
		string[] files = read.Files.Select(file => file.Source.Replace('\\', '/')).ToArray();
		IReadOnlyList<ModSystemProfile> profiles = ModSystemCatalog.GetProfiles(server);
		bool minecraft = GameDatabase.IsMinecraft(server.Game);
		HashSet<string> javaLayouts = minecraft ? ReadJavaLayouts(source, cancellationToken) : [];
		List<ModImportChoice> choices = [];
		IReadOnlyList<ArkModPackageInfo> mods = [];
		bool arkPackage = files.Any(path => Path.GetExtension(path).ToLowerInvariant() is ".uplugin" or ".mod");
		if (arkPackage && server.Game is ArkModPackageReader.Ascended or ArkModPackageReader.Evolved)
		{
			// The edition-specific reader rejects mismatched, incomplete and mixed packages.
			mods = ArkModPackageReader.Read(source, server.Game, cancellationToken);
			foreach (ModSystemProfile profile in profiles.Where(profile => !profile.UserConfigured))
				foreach (ModInstallTarget target in profile.Targets.Where(target => ArkModPackageReader.Supports(server.Game, target)))
					choices.Add(new(profile, target, "", true));
			return new(read.PackageSha256, files, choices, mods);
		}
		foreach (ModSystemProfile profile in profiles.Where(profile => profile.SupportLevel == ModSystemSupportLevel.Managed))
		{
			ModSystemDetection? detection = ModSystemCatalog.Detect(server, profile);
			foreach (ModInstallTarget target in profile.Targets.Where(target => target.CanImport))
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (profile.Id == "minecraft-addons" && javaLayouts.Count > 0 &&
					(target.Id == "plugins" ? !javaLayouts.Contains("Plugin") : javaLayouts.All(value => value == "Plugin"))) continue;
				if (!files.Any(path => target.AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))) continue;
				if (target.RequiredArchiveFileName.Length > 0 && !files.Any(path =>
					Path.GetFileName(path).Equals(target.RequiredArchiveFileName, StringComparison.OrdinalIgnoreCase))) continue;
				if (target.PackageLayout == ModPackageLayout.EmpyrionScenario && !files.Any(path =>
					Path.GetFileName(path).Equals("gameoptions.yaml", StringComparison.OrdinalIgnoreCase))) continue;
				if (Path.GetExtension(source).Equals(".zip", StringComparison.OrdinalIgnoreCase) ? !target.AllowArchives : target.ArchiveOnly) continue;
				bool ready = detection?.FrameworkDetected == true;
				if (!profile.UserConfigured && target.FrameworkNames.Count > 0)
				{
					string loader = minecraft ? server.MinecraftLoader : server.ServerFramework;
					ready = string.IsNullOrWhiteSpace(loader) ? detection?.ActiveTargets.Contains(target) == true :
						target.FrameworkNames.Contains(loader, StringComparer.OrdinalIgnoreCase);
					if (profile.Id == "minecraft-addons" && javaLayouts.Count > 0)
						ready &= javaLayouts.All(value => value == "Plugin" ? target.Id == "plugins" : value.Equals(loader, StringComparison.OrdinalIgnoreCase));
				}
				choices.Add(new(profile, target, SuggestSelection(files, target), ready));
			}
		}
		return new(read.PackageSha256, files, choices.OrderByDescending(choice => choice.Ready).ToArray(), mods);
	}

	private static HashSet<string> ReadJavaLayouts(string source, CancellationToken cancellation)
	{
		HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
		void Inspect(ZipArchive jar)
		{
			if (jar.Entries.Count > 50000) throw ModPackageFiles.Error("Size");
			if (jar.GetEntry("plugin.yml") != null || jar.GetEntry("paper-plugin.yml") != null) result.Add("Plugin");
			if (jar.GetEntry("fabric.mod.json") != null) result.Add("Fabric");
			if (jar.GetEntry("quilt.mod.json") != null) result.Add("Quilt");
			if (jar.GetEntry("META-INF/mods.toml") != null) result.Add("Forge");
			if (jar.GetEntry("META-INF/neoforge.mods.toml") != null) result.Add("NeoForge");
		}
		if (Path.GetExtension(source).Equals(".jar", StringComparison.OrdinalIgnoreCase))
		{
			using ZipArchive jar = ZipFile.OpenRead(source); Inspect(jar);
		}
		else if (Path.GetExtension(source).Equals(".zip", StringComparison.OrdinalIgnoreCase))
		{
			using ZipArchive zip = ZipFile.OpenRead(source);
			long total = 0;
			foreach (ZipArchiveEntry entry in zip.Entries.Where(entry => entry.FullName.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)))
			{
				cancellation.ThrowIfCancellationRequested();
				if (entry.Length > 64 * 1024 * 1024 || (total = checked(total + entry.Length)) > 256 * 1024 * 1024) throw ModPackageFiles.Error("Size");
				using MemoryStream bytes = new();
				using Stream input = entry.Open();
				ModPackageFiles.CopyExactBounded(input, bytes, entry.Length, cancellation);
				bytes.Position = 0;
				using ZipArchive jar = new(bytes, ZipArchiveMode.Read); Inspect(jar);
			}
		}
		return result;
	}

	internal static string SuggestSelection(IReadOnlyList<string> files, ModInstallTarget target)
	{
		if (target.PackageLayout is ModPackageLayout.EmpyrionMod or ModPackageLayout.EmpyrionScenario) return "";
		string destination = target.RelativePath.Replace('\\', '/').Trim('/') + "/";
		HashSet<string> roots = new(StringComparer.OrdinalIgnoreCase);
		foreach (string file in files)
		{
			string padded = "/" + file;
			int at = padded.IndexOf("/" + destination, StringComparison.OrdinalIgnoreCase);
			if (at >= 0) roots.Add(padded.Substring(1, at + destination.Length));
		}
		if (roots.Count == 1 && files.Where(path => target.AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
			.All(path => path.StartsWith(roots.Single(), StringComparison.OrdinalIgnoreCase))) return roots.Single();
		if (target.PackageLayout == ModPackageLayout.FolderTree) return ""; // Unknown wrappers may be the actual mod folder.
		string[] primary = files.Where(path => target.AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)).ToArray();
		if (target.RequiredArchiveFileName.Length > 0)
			primary = files.Where(path => Path.GetFileName(path).Equals(target.RequiredArchiveFileName, StringComparison.OrdinalIgnoreCase))
				.Select(path => Parent(path).TrimEnd('/')).Select(Parent).ToArray();
		else primary = primary.Select(Parent).ToArray();
		return CommonRoot(primary);
	}

	internal static ModImportChoice ChooseFolder(GameServer server, ModImportAnalysis analysis, string folder)
	{
		string relative = Path.GetRelativePath(server.InstallPath, folder).Replace('\\', '/');
		string[] codeTypes = [".uplugin", ".mod", ".jar", ".dll", ".cs", ".lua", ".js", ".py", ".so", ".asi", ".smx", ".amxx", ".vpk", ".pak",
			".gma", ".tmod", ".mpk", ".pbo", ".rfcmp", ".rfmod", ".as", ".pas"];
		string[] extensions = analysis.Files.Select(Path.GetExtension).OfType<string>().Select(value => value.ToLowerInvariant())
			.Where(value => SnapshotTarget.AllowedExtensions.Contains(value)).Distinct().ToArray();
		string[] main = extensions.Where(value => codeTypes.Contains(value)).ToArray();
		if (main.Length == 0) main = extensions;
		ModInstallTarget target = UniversalModImports.CreateTarget(server, Path.GetFileName(folder.TrimEnd('\\', '/')),
			relative, string.Join(',', main), ModContentKind.Mod);
		return new(UniversalModImports.Profile(server, [target]), target, SuggestSelection(analysis.Files, target), true, true);
	}

	private static string Parent(string path) => path.Contains('/') ? path[..(path.LastIndexOf('/') + 1)] : "";
	private static string CommonRoot(string[] parents)
	{
		if (parents.Length == 0) return "";
		string prefix = parents[0];
		while (prefix.Length > 0 && parents.Any(path => !path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			prefix = Parent(prefix.TrimEnd('/'));
		return prefix;
	}
}
