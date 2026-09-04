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
using System.Text;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.Database.GameDefinitions
{
	internal enum GameDefinitionValidationLevel
	{
		Passed,
		Warning,
		Failed
	}

	internal sealed record GameDefinitionValidationItem(
		GameDefinitionValidationLevel Level,
		string Definition,
		string Details);

	internal sealed record GameDefinitionValidationReport(
		int DefinitionCount,
		int TemplateCount,
		int PostInstallActionCount,
		int ManagedSettingBindingCount,
		int DefinitionTestCount,
		IReadOnlyList<GameDefinitionValidationItem> Items)
	{
		public int FailedCount => Items.Count(item =>
			item.Level == GameDefinitionValidationLevel.Failed);
		public int WarningCount => Items.Count(item =>
			item.Level == GameDefinitionValidationLevel.Warning);
		public bool IsValid => FailedCount == 0;

		public string ToPlainText()
		{
			StringBuilder text = new();
			text.AppendLine("SYNIX GAME DEFINITION TEST REPORT");
			text.AppendLine(new string('=', 34));
			text.AppendLine($"Result: {(IsValid ? "VALID" : "FAILED")}");
			text.AppendLine($"Definitions: {DefinitionCount}");
			text.AppendLine($"Templates: {TemplateCount}");
			text.AppendLine($"Managed setting bindings: {ManagedSettingBindingCount}");
			text.AppendLine($"Safe post-install actions: {PostInstallActionCount}");
			text.AppendLine($"Definition tests completed: {DefinitionTestCount}");
			text.AppendLine($"Warnings: {WarningCount}  Failed: {FailedCount}");
			foreach (GameDefinitionValidationItem item in Items)
			{
				string marker = item.Level switch
				{
					GameDefinitionValidationLevel.Passed => "PASS",
					GameDefinitionValidationLevel.Warning => "WARN",
					_ => "FAIL"
				};
				text.AppendLine($"[{marker}] {item.Definition}");
				text.AppendLine($"       {item.Details}");
			}
			return text.ToString().TrimEnd();
		}
	}

	internal static class GameDefinitionValidator
	{
		internal static GameDefinitionValidationReport ValidateEmbeddedLibrary()
		{
			try
			{
				return BuildCatalogReport(
					TrustedGameDefinitionCatalog.Packages,
					[]);
			}
			catch (Exception exception)
			{
				return new GameDefinitionValidationReport(
					0,
					0,
					0,
					0,
					0,
					[new GameDefinitionValidationItem(
						GameDefinitionValidationLevel.Failed,
						"Embedded game library",
						exception.Message)]);
			}
		}

		internal static GameDefinitionValidationReport ValidateSourceDirectory(
			string projectDirectory)
		{
			string definitionsDirectory = Path.Combine(
				Path.GetFullPath(projectDirectory),
				"Database",
				"GameDefinitions");
			if (!Directory.Exists(definitionsDirectory))
			{
				return new GameDefinitionValidationReport(
					0,
					0,
					0,
					0,
					0,
					[new GameDefinitionValidationItem(
						GameDefinitionValidationLevel.Failed,
						"Game definition library",
						"Database\\GameDefinitions was not found.")]);
			}

			List<EmbeddedGamePackage> packages = [];
			List<GameDefinitionValidationItem> items = [];
			foreach (string file in Directory.EnumerateFiles(
				definitionsDirectory,
				"*.game.json",
				SearchOption.AllDirectories)
				.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
			{
				string displayName = Path.GetRelativePath(definitionsDirectory, file);
				try
				{
					string json = File.ReadAllText(file);
					ValidateExplicitRevisionProperties(json, displayName);
					string definitionDirectory = Path.GetDirectoryName(file)!;
					EmbeddedGamePackage package =
						TrustedGameDefinitionCatalog.ParsePackage(
							json,
							displayName,
							(_, templateFile) => ReadTemplate(
								definitionDirectory,
								templateFile));
					ValidateSourceLayout(file, package);
					packages.Add(package);
				}
				catch (Exception exception)
				{
					items.Add(new GameDefinitionValidationItem(
						GameDefinitionValidationLevel.Failed,
						displayName,
						exception.Message));
				}
			}

			return BuildCatalogReport(packages, items);
		}

		private static GameDefinitionValidationReport BuildCatalogReport(
			IReadOnlyList<EmbeddedGamePackage> packages,
			List<GameDefinitionValidationItem> items)
		{
			HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
			HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
			HashSet<int> orders = [];
			foreach (EmbeddedGamePackage package in packages)
			{
				GameInfo definition = package.Definition;
				if (!ids.Add(definition.DefinitionId))
					AddFailure(items, definition.Game, "The definition ID is duplicated.");
				if (!names.Add(definition.Game))
					AddFailure(items, definition.Game, "The game name is duplicated.");
				foreach (string alias in definition.Aliases)
				{
					if (!names.Add(alias))
						AddFailure(items, definition.Game, $"The alias '{alias}' is duplicated.");
				}
				if (definition.CatalogOrder < 0)
					AddFailure(items, definition.Game, "The catalog order cannot be negative.");
				else if (!orders.Add(definition.CatalogOrder))
					AddFailure(items, definition.Game, "The catalog order is duplicated.");
			}

			int templateCount = packages.Sum(package =>
				package.Configuration?.Templates.Count ?? 0);
			int actionCount = packages.Sum(package =>
				package.PostInstallActions.Count);
			(int managedBindingCount, int definitionTestCount) =
				RunDefinitionTests(packages, items);
			if (items.All(item => item.Level != GameDefinitionValidationLevel.Failed))
			{
				items.Add(new GameDefinitionValidationItem(
					GameDefinitionValidationLevel.Passed,
					"Game definition library",
					$"Validated {packages.Count} definitions, {templateCount} complete template(s), {managedBindingCount} managed setting binding(s), and {actionCount} allowlisted post-install action(s)."));
				items.Add(new GameDefinitionValidationItem(
					GameDefinitionValidationLevel.Passed,
					"Definition test runner",
					$"Completed {definitionTestCount} isolated definition and configuration test(s) without changing installed servers."));
			}

			return new GameDefinitionValidationReport(
				packages.Count,
				templateCount,
				actionCount,
				managedBindingCount,
				definitionTestCount,
				items);
		}

		private static (int ManagedBindingCount, int DefinitionTestCount)
			RunDefinitionTests(
				IReadOnlyList<EmbeddedGamePackage> packages,
				List<GameDefinitionValidationItem> items)
		{
			int managedBindingCount = 0;
			int testCount = packages.Count;
			foreach (EmbeddedGamePackage package in packages)
			{
				if (package.Configuration == null)
					continue;

				EmbeddedTemplateConfigurationDefinition definition = new(
					package.Definition.Game,
					package.Configuration);
				managedBindingCount += CountFlags(definition.SupportedInputs);
				testCount++;
				RunConfigurationTest(package, definition, items);
			}

			return (managedBindingCount, testCount);
		}

		private static void RunConfigurationTest(
			EmbeddedGamePackage package,
			EmbeddedTemplateConfigurationDefinition definition,
			List<GameDefinitionValidationItem> items)
		{
			string root = Path.Combine(
				Path.GetTempPath(),
				"Synix.DefinitionTests",
				Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(root);
			try
			{
				GameServer server = new()
				{
					Game = package.Definition.Game,
					InstallPath = root,
					ServerName = "Synix Definition Test",
					WorldName = "SynixWorld",
					WorldSeed = "12345",
					GameMode = "PVE",
					MaxPlayers = 12,
					Port = package.Definition.Port,
					QueryPort = package.Definition.QueryPort,
					AppPort = package.Definition.AppPort,
					RconPort = package.Definition.QueryPort + 1,
					EnableRcon = true
				};
				ConfigurationContext context = new(
					server,
					new SynixServerPasswords(
						"server-password",
						"admin-password",
						"rcon-password"),
					"synix-definition-test",
					"127.0.0.1",
					"203.0.113.10");
				ConfigurationApplyResult result = definition.ResetToTemplate(context);
				if (!result.Succeeded || !result.Complete)
				{
					AddFailure(items, package.Definition.Game, result.Message);
					return;
				}

				IReadOnlyList<string> paths = definition.ResolveConfigurationPaths(server);
				if (paths.Count != package.Configuration!.Templates.Count ||
					paths.Any(path => !File.Exists(path)))
				{
					AddFailure(
						items,
						package.Definition.Game,
						"The isolated template test did not create every declared configuration file.");
					return;
				}

				foreach (string path in paths)
				{
					_ = ConfigHandler.LoadConfig(path, package.Definition.Format);
					string content = File.ReadAllText(path);
					if (content.Contains("{ServerName}", StringComparison.Ordinal) ||
						content.Contains("{Port}", StringComparison.Ordinal) ||
						content.Contains("{Password}", StringComparison.Ordinal) ||
						content.Contains("{HasPassword}", StringComparison.Ordinal))
					{
						AddFailure(
							items,
							package.Definition.Game,
							"The isolated template test found an unresolved managed placeholder.");
						return;
					}
				}
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					package.Definition.Game,
					$"The isolated definition test failed: {exception.Message}");
			}
			finally
			{
				try
				{
					if (Directory.Exists(root))
						Directory.Delete(root, true);
				}
				catch (IOException suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
				}
catch (UnauthorizedAccessException suppressedException)
{
	Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
}
			}
		}

		private static int CountFlags(ManagedConfigurationInput inputs)
		{
			int count = 0;
			foreach (ManagedConfigurationInput value in Enum.GetValues<ManagedConfigurationInput>())
			{
				if (value != ManagedConfigurationInput.None && inputs.HasFlag(value))
					count++;
			}
			return count;
		}

		private static void ValidateExplicitRevisionProperties(
			string json,
			string displayName)
		{
			using JsonDocument document = JsonDocument.Parse(json);
			JsonElement root = document.RootElement;
			if (!root.TryGetProperty("definitionRevision", out _))
				throw new InvalidDataException($"{displayName} is missing definitionRevision.");

			if (!root.TryGetProperty("configuration", out JsonElement configuration) ||
				configuration.ValueKind == JsonValueKind.Null)
				return;
			if (configuration.ValueKind != JsonValueKind.Object)
				throw new InvalidDataException($"{displayName} has an invalid configuration object.");
			if (!configuration.TryGetProperty("revision", out _))
				throw new InvalidDataException($"{displayName} is missing configuration.revision.");
			if (configuration.TryGetProperty("templates", out JsonElement templates))
			{
				foreach (JsonElement template in templates.EnumerateArray())
				{
					if (!template.TryGetProperty("revision", out _))
						throw new InvalidDataException($"{displayName} contains a template without a revision.");
				}
			}
		}

		private static void ValidateSourceLayout(
			string file,
			EmbeddedGamePackage package)
		{
			string expectedFileName = package.Definition.DefinitionId + ".game.json";
			string? folderName = Path.GetFileName(Path.GetDirectoryName(file));
			if (!string.Equals(
				Path.GetFileName(file),
				expectedFileName,
				StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(
					folderName,
					package.Definition.DefinitionId,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The folder and definition filename must match the definition ID.");
			}
		}

		private static string? ReadTemplate(
			string definitionDirectory,
			string templateFile)
		{
			string templatesRoot = Path.GetFullPath(Path.Combine(
				definitionDirectory,
				"Templates"));
			string path = Path.GetFullPath(Path.Combine(
				templatesRoot,
				templateFile.Replace('/', Path.DirectorySeparatorChar)));
			if (!path.StartsWith(
				templatesRoot + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			return File.Exists(path) ? File.ReadAllText(path) : null;
		}

		private static void AddFailure(
			List<GameDefinitionValidationItem> items,
			string definition,
			string details)
		{
			items.Add(new GameDefinitionValidationItem(
				GameDefinitionValidationLevel.Failed,
				definition,
				details));
		}
	}
}
