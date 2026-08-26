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
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class ArmaReforgerConfiguration : ConfigurationDefinition
	{
		private const string DefaultScenario =
			"{ECC61978EDCC2B5A}Missions/23_Campaign.conf";

		private static readonly JsonSerializerOptions TemplateJsonOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("bindPort", context => context.Server.Port.ToString()),
			new("publicPort", context => context.Server.Port.ToString()),
			new("a2s.port", context => context.Server.QueryPort.ToString()),
			new("game.name", context => context.Server.ServerName),
			new("game.password", context => NormalizeOptionalValue(context.Passwords.ServerPassword)),
			new("game.passwordAdmin", context => NormalizeAdminPassword(context.Passwords.AdminPassword)),
			new("game.maxPlayers", context => context.Server.MaxPlayers.ToString()),
			new("game.crossPlatform", context => context.Server.CrossplayEnabled.ToString().ToLowerInvariant())
		];

		public override string GameName => "Arma Reforger";
		public override int SchemaVersion => 4;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Crossplay;
		public override string RelativePath => @"configs\server.json";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override IReadOnlyList<ConfigurationValidationItem> Validate(
			ConfigurationContext context)
		{
			List<ConfigurationValidationItem> items = base.Validate(context).ToList();
			string path = ResolveFullPath(context.Server);
			if (!File.Exists(path))
				return items;

			try
			{
				JsonNode? parsed = JsonNode.Parse(File.ReadAllText(path));
				if (parsed is not JsonObject root)
				{
					items.Add(new ConfigurationValidationItem(
						ConfigurationValidationState.Failed,
						"rcon",
						"The root of server.json is not a JSON object."));
					return items;
				}

				bool matchesSavedValue;
				if (context.Server.EnableRcon)
				{
					JsonNode expected = JsonSerializer.SerializeToNode(
						CreateRconConfiguration(context),
						TemplateJsonOptions)!;
					matchesSavedValue = JsonNode.DeepEquals(root["rcon"], expected);
				}
				else
				{
					matchesSavedValue = !root.ContainsKey("rcon");
				}

				items.Add(new ConfigurationValidationItem(
					matchesSavedValue
						? ConfigurationValidationState.Passed
						: ConfigurationValidationState.Failed,
					"rcon",
					matchesSavedValue
						? "The RCON section matches the values saved in Synix."
						: "The RCON section does not match the enabled state, port, or password saved in Synix."));
			}
			catch (Exception exception)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					"rcon",
					$"Synix could not inspect the RCON section: {exception.Message}"));
			}

			return items;
		}

		public override string CreateTemplate(ConfigurationContext context)
		{
			object? rcon = context.Server.EnableRcon
				? CreateRconConfiguration(context)
				: null;

			object configuration = new
			{
				bindAddress = "0.0.0.0",
				bindPort = context.Server.Port,
				publicAddress = string.Empty,
				publicPort = context.Server.Port,
				a2s = new
				{
					address = "0.0.0.0",
					port = context.Server.QueryPort
				},
				rcon,
				game = new
				{
					name = context.Server.ServerName,
					password = NormalizeOptionalValue(context.Passwords.ServerPassword),
					passwordAdmin = NormalizeAdminPassword(context.Passwords.AdminPassword),
					admins = Array.Empty<string>(),
					scenarioId = DefaultScenario,
					maxPlayers = context.Server.MaxPlayers,
					visible = true,
					crossPlatform = context.Server.CrossplayEnabled,
					modsRequiredByDefault = true,
					gameProperties = new
					{
						serverMaxViewDistance = 1600,
						serverMinGrassDistance = 50,
						networkViewDistance = 1500,
						disableThirdPerson = false,
						fastValidation = true,
						battlEye = true,
						VONDisableUI = false,
						VONDisableDirectSpeechUI = false,
						VONCanTransmitCrossFaction = false
					},
					mods = Array.Empty<object>()
				},
				operating = new
				{
					lobbyPlayerSynchronise = true,
					disableCrashReporter = false,
					disableServerShutdown = false,
					disableAI = false,
					playerSaveTime = 120,
					aiLimit = -1,
					slotReservationTimeout = 60,
					joinQueue = new
					{
						maxSize = 0
					}
				}
			};

			return JsonSerializer.Serialize(configuration, TemplateJsonOptions);
		}

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			ConfigurationApplyResult result = base.Apply(context);
			if (!result.Succeeded)
			{
				return result;
			}

			try
			{
				bool rconChanged = SynchronizeRconConfiguration(context);
				if (!rconChanged)
				{
					return result;
				}

				return new ConfigurationApplyResult(
					true,
					result.Complete,
					true,
					result.Created,
					result.Complete
						? $"Updated and verified the {GameName} configuration."
						: result.Message);
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					$"The {GameName} RCON configuration could not be applied: {exception.Message}");
			}
		}

		private static object CreateRconConfiguration(ConfigurationContext context)
		{
			string password = NormalizeRconPassword(context.Passwords.RconPassword);
			int port = context.Server.RconPort > 0
				? context.Server.RconPort
				: 19999;

			return new
			{
				address = "0.0.0.0",
				port,
				password,
				maxClients = 16,
				permission = "admin",
				blacklist = Array.Empty<string>(),
				whitelist = Array.Empty<string>()
			};
		}

		private bool SynchronizeRconConfiguration(ConfigurationContext context)
		{
			string path = ResolveFullPath(context.Server);
			JsonNode? parsed = JsonNode.Parse(File.ReadAllText(path));
			if (parsed is not JsonObject root)
			{
				throw new InvalidDataException("The root of server.json must be a JSON object.");
			}

			bool changed;
			if (context.Server.EnableRcon)
			{
				JsonNode expected = JsonSerializer.SerializeToNode(
					CreateRconConfiguration(context),
					TemplateJsonOptions)!;
				changed = !JsonNode.DeepEquals(root["rcon"], expected);
				if (changed)
				{
					root["rcon"] = expected;
				}
			}
			else
			{
				changed = root.Remove("rcon");
			}

			if (changed)
			{
				WriteJsonSafely(path, root);
			}

			return changed;
		}

		private static void WriteJsonSafely(string path, JsonObject root)
		{
			string? directory = Path.GetDirectoryName(path);
			if (string.IsNullOrWhiteSpace(directory))
			{
				throw new InvalidOperationException("The configuration directory is unavailable.");
			}

			string temporaryPath = Path.Combine(
				directory,
				$".{Path.GetFileName(path)}.{Guid.NewGuid():N}.synix.tmp");
			try
			{
				string json = root.ToJsonString(TemplateJsonOptions);
				File.WriteAllText(temporaryPath, json, new UTF8Encoding(false, true));
				_ = JsonNode.Parse(File.ReadAllText(temporaryPath)) ??
					throw new InvalidDataException("The generated JSON could not be verified.");
				File.Move(temporaryPath, path, true);
			}
			finally
			{
				if (File.Exists(temporaryPath))
				{
					File.Delete(temporaryPath);
				}
			}
		}

		private static string NormalizeOptionalValue(string value)
		{
			return string.Equals(value, "Not Required", StringComparison.OrdinalIgnoreCase)
				? string.Empty
				: value;
		}

		private static string NormalizeAdminPassword(string value)
		{
			string password = NormalizeOptionalValue(value);
			if (password.Any(char.IsWhiteSpace))
			{
				throw new InvalidDataException(
					"Arma Reforger admin passwords cannot contain spaces.");
			}

			return password;
		}

		private static string NormalizeRconPassword(string value)
		{
			string password = NormalizeOptionalValue(value);
			if (password.Length < 3)
			{
				throw new InvalidDataException(
					"Arma Reforger RCON passwords must contain at least three characters.");
			}

			if (password.Any(char.IsWhiteSpace))
			{
				throw new InvalidDataException(
					"Arma Reforger RCON passwords cannot contain spaces.");
			}

			return password;
		}
	}
}
