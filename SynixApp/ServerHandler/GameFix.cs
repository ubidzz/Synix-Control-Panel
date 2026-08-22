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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static class GameFix
	{
		private static readonly IReadOnlyDictionary<string, ConfigurationDefinition> ConfigurationIndex =
			CreateConfigurationIndex();

		public static bool ManualConfigWasCreated { get; set; }

		internal static bool ManagedConfigurationsEnabled =>
			ShouldUseManagedConfigurations(
				Core.IsOfficialRelease,
				Properties.Settings.Default.DisablePremadeConfigurationsForDevelopment);

		internal static bool ShouldUseManagedConfigurations(
			bool isOfficialRelease,
			bool disabledForDevelopment)
		{
			return isOfficialRelease || !disabledForDevelopment;
		}

		internal static bool TryGetConfiguration(
			string gameName,
			out ConfigurationDefinition? definition)
		{
			return ConfigurationIndex.TryGetValue(
				GameDatabase.GetCanonicalGameName(gameName),
				out definition);
		}

		internal static ManagedConfigurationInput GetManagedConfigurationInputs(
			string gameName)
		{
			if (!ManagedConfigurationsEnabled ||
				!TryGetConfiguration(gameName, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return ManagedConfigurationInput.None;
			}

			return definition.SupportedInputs;
		}

		internal static bool NeedsManagedConfiguration(GameServer server)
		{
			if (!ManagedConfigurationsEnabled)
			{
				return false;
			}

			if (!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return false;
			}

			if (server.ManagedConfigurationVersion < definition.SchemaVersion)
			{
				return true;
			}

			try
			{
				return !definition.ConfigurationFileExists(server);
			}
			catch
			{
				return true;
			}
		}

		internal static async Task<ConfigurationApplyResult> ApplyManagedConfiguration(
			GameServer server)
		{
			if (!ManagedConfigurationsEnabled)
			{
				return new ConfigurationApplyResult(
					true,
					true,
					false,
					false,
					"Premade game configurations are disabled for this development build.");
			}

			if (!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return new ConfigurationApplyResult(
					true,
					true,
					false,
					false,
					"This game does not have a managed configuration definition.");
			}

			string localIp = string.Empty;
			string publicIp = string.Empty;
			bool needsNetworkAddresses = definition.RequiresNetworkAddresses;
			if (needsNetworkAddresses)
			{
				try
				{
					needsNetworkAddresses = !definition.ConfigurationFileExists(server);
				}
				catch
				{
				}
			}

			if (needsNetworkAddresses)
			{
				localIp = await Core.Instance.GetLocalIP();
				publicIp = await Core.Instance.GetPublicIP();
			}

			SynixServerPasswords passwords = default;
			if (definition.UsesConfigurationFile)
			{
				try
				{
					passwords = Core.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					return ConfigurationApplyResult.Failure(
						"Synix could not unlock the saved passwords. Re-enter them in Server Settings before applying the game configuration.");
				}
			}

			ConfigurationContext context = new(
				server,
				passwords,
				Core.Instance.GetSafeName(server.ServerName),
				localIp,
				publicIp);
			ConfigurationApplyResult result = definition.Apply(context);
			if (result.Succeeded && result.Complete)
			{
				server.ManagedConfigurationVersion = definition.SchemaVersion;
			}

			return result;
		}

		public static async Task<bool> PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
			{
				return false;
			}

			bool applied = false;
			if (server.Game == "Dune: Awakening" || server.Game == "Minecraft")
			{
				ManualConfigWasCreated = true;
				applied = true;
			}

			try
			{
				switch (server.Game)
				{
					case "StarRupture":
						if (CopySteamDLLs(server.InstallPath, @"StarRupture\Binaries\Win64")) applied = true; break;
					case "Soulmask":
						if (CopySteamDLLs(server.InstallPath, @"WS\Binaries\Win64")) applied = true; break;
					case "Palworld":
						if (CopySteamDLLs(server.InstallPath, @"Pal\Binaries\Win64")) applied = true; break;
					case "ARK: Survival Evolved":
					case "ARK: Survival Ascended":
					case "ARK: Survival Ascended (Scorched Earth)":
					case "PixARK":
					case "Atlas":
					case "The Stomping Land":
					case "Dirty Bomb":
						if (CopySteamDLLs(server.InstallPath, @"ShooterGame\Binaries\Win64")) applied = true; break;
					case "Foundry":
						if (CopySteamDLLs(server.InstallPath, string.Empty)) applied = true; break;
					case "ASTRONEER":
						if (CopySteamDLLs(server.InstallPath, @"Astro\Binaries\Win64")) applied = true; break;
					case "Abiotic Factor":
						if (CopySteamDLLs(server.InstallPath, @"AbioticFactor\Binaries\Win64")) applied = true; break;
					case "BATTALION: Legacy":
						if (CopySteamDLLs(server.InstallPath, @"Battalion\Binaries\Win64")) applied = true; break;
					case "Icarus":
						if (CopySteamDLLs(server.InstallPath, @"Icarus\Binaries\Win64")) applied = true; break;
					case "The Front":
						if (CopySteamDLLs(server.InstallPath, @"ProjectWar\Binaries\Win64")) applied = true; break;
					case "Smalland: Survive the Wilds":
						if (CopySteamDLLs(server.InstallPath, @"SMALLAND\Binaries\Win64")) applied = true; break;
					case "Conan Exiles":
					case "Conan Exiles (TestLive)":
						if (CopySteamDLLs(server.InstallPath, @"ConanSandbox\Binaries\Win64")) applied = true; break;
					case "Mordhau":
						if (CopySteamDLLs(server.InstallPath, @"Mordhau\Binaries\Win64")) applied = true; break;
					case "Satisfactory":
						if (CopySteamDLLs(server.InstallPath, @"FactoryGame\Binaries\Win64")) applied = true; break;
					case "Insurgency: Sandstorm":
						if (CopySteamDLLs(server.InstallPath, @"Insurgency\Binaries\Win64")) applied = true; break;
					case "Myth of Empires":
						if (CopySteamDLLs(server.InstallPath, @"MOE\Binaries\Win64")) applied = true; break;
					case "SCUM":
						if (CopySteamDLLs(server.InstallPath, @"SCUM\Binaries\Win64")) applied = true; break;
					case "Hell Let Loose":
						if (CopySteamDLLs(server.InstallPath, @"HLL\Binaries\Win64")) applied = true; break;
					case "Nightingale":
						if (CopySteamDLLs(server.InstallPath, @"NWX\Binaries\Win64")) applied = true; break;
					case "DeadPoly":
						if (CopySteamDLLs(server.InstallPath, @"DeadPoly\Binaries\Win64")) applied = true; break;
					case "Bellwright":
						if (CopySteamDLLs(server.InstallPath, @"Bellwright\Binaries\Win64")) applied = true; break;
					case "The Isle":
					case "The Isle (Evrima)":
					case "The Isle (Legacy)":
						if (CopySteamDLLs(server.InstallPath, @"TheIsle\Binaries\Win64")) applied = true; break;
					case "Grounded":
						if (CopySteamDLLs(server.InstallPath, @"Maine\Binaries\Win64")) applied = true; break;
					case "Day of Dragons":
						if (CopySteamDLLs(server.InstallPath, @"Dragons\Binaries\Win64")) applied = true; break;
					case "Return to Moria":
						if (CopySteamDLLs(server.InstallPath, @"Moria\Binaries\Win64")) applied = true; break;
					case "Citadel: Forged with Fire":
						if (CopySteamDLLs(server.InstallPath, @"Citadel\Binaries\Win64")) applied = true; break;
					case "Outlaws of the Old West":
						if (CopySteamDLLs(server.InstallPath, @"Outlaws\Binaries\Win64")) applied = true; break;
					case "Primal Carnage: Extinction":
						if (CopySteamDLLs(server.InstallPath, @"PrimalCarnage\Binaries\Win64")) applied = true; break;
					case "Ranch Simulator":
						if (CopySteamDLLs(server.InstallPath, @"Ranch_Simulator\Binaries\Win64")) applied = true; break;
					case "Memories of Mars":
						if (CopySteamDLLs(server.InstallPath, @"MemoriesOfMars\Binaries\Win64")) applied = true; break;
					case "Deadside":
						if (CopySteamDLLs(server.InstallPath, @"DeadsideServer\Binaries\Win64")) applied = true; break;
					case "Dune: Awakening":
						if (CopySteamDLLs(server.InstallPath, string.Empty)) applied = true; break;
					case "Last Oasis":
						if (CopySteamDLLs(server.InstallPath, @"OasisServer\Binaries\Win64")) applied = true; break;
					case "Dark and Light":
						if (CopySteamDLLs(server.InstallPath, @"DNL\Binaries\Win64")) applied = true; break;
					case "SCP: 5K":
						if (CopySteamDLLs(server.InstallPath, @"Pandemic\Binaries\Win64")) applied = true; break;
					case "GROUND BRANCH CTE":
						if (CopySteamDLLs(server.InstallPath, @"GroundBranch\Binaries\Win64")) applied = true; break;
					case "Desynced":
						if (CopySteamDLLs(server.InstallPath, @"Desynced\Binaries\Win64")) applied = true; break;
					case "HYPERCHARGE: Unboxed":
						if (CopySteamDLLs(server.InstallPath, @"Unboxed\Binaries\Win64")) applied = true; break;
					case "Dysterra":
						if (CopySteamDLLs(server.InstallPath, @"Dysterra\Binaries\Win64")) applied = true; break;
					case "D.A.T.A":
						if (CopySteamDLLs(server.InstallPath, @"WindowsServer\ABYSS421\Binaries\Win64")) applied = true; break;
					case "Days of War":
						if (CopySteamDLLs(server.InstallPath, @"DaysOfWar\Binaries\Win64")) applied = true; break;
					case "Angels Fall First":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Right to Rule":
						if (CopySteamDLLs(server.InstallPath, @"RightToRule\Binaries\Win64")) applied = true; break;
					case "HELL'S NEW WORLD":
						if (CopySteamDLLs(server.InstallPath, @"WindowsServer\HellsNewWorld\Binaries\Win64")) applied = true; break;
					case "Gray Zone Warfare":
						if (CopySteamDLLs(server.InstallPath, @"GZW\Binaries\Win64")) applied = true; break;
					case "HumanitZ":
						if (CopySteamDLLs(server.InstallPath, @"HumanitZ\Binaries\Win64")) applied = true; break;
					case "VoidTrain":
						if (CopySteamDLLs(server.InstallPath, @"VoidTrain\Binaries\Win64")) applied = true; break;
					case "Pavlov VR":
						if (CopySteamDLLs(server.InstallPath, @"Pavlov\Binaries\Win64")) applied = true; break;
					case "Longvinter":
						if (CopySteamDLLs(server.InstallPath, @"Longvinter\Binaries\Win64")) applied = true; break;
					case "Ground Branch":
						if (CopySteamDLLs(server.InstallPath, @"GroundBranch\Binaries\Win64")) applied = true; break;
					case "Beasts of Bermuda":
						if (CopySteamDLLs(server.InstallPath, @"BeastsOfBermuda\Binaries\Win64")) applied = true; break;
					case "The Mean Greens - Plastic Warfare":
						if (CopySteamDLLs(server.InstallPath, @"MeanGreens\Binaries\Win64")) applied = true; break;
					case "Operation: Harsh Doorstop":
						if (CopySteamDLLs(server.InstallPath, @"HarshDoorstop\Binaries\Win64")) applied = true; break;
					case "America's Army: Proving Grounds":
						if (CopySteamDLLs(server.InstallPath, @"AAGame\Binaries\Win64")) applied = true; break;
					case "Monday Night Combat":
						if (CopySteamDLLs(server.InstallPath, @"MNC\Binaries\Win64")) applied = true; break;
					case "Chivalry 2":
						if (CopySteamDLLs(server.InstallPath, @"TBL\Binaries\Win64")) applied = true; break;
					case "Depth":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Primal Carnage":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win32")) applied = true; break;
					case "Toxikk":
					case "Sanctum 2":
					case "Sanctum":
					case "The Haunted: Hell's Reach":
					case "Chivalry: Medieval Warfare":
					case "Orion: Prelude":
						if (CopySteamDLLs(server.InstallPath, @"UDKGame\Binaries\Win64")) applied = true; break;
					case "Beyond the Wire":
						if (CopySteamDLLs(server.InstallPath, @"BeyondTheWire\Binaries\Win64")) applied = true; break;
					case "Mortal Online 2":
						if (CopySteamDLLs(server.InstallPath, @"MortalOnline2\Binaries\Win64")) applied = true; break;
					case "XERA: Survival":
						if (CopySteamDLLs(server.InstallPath, @"Xera\Binaries\Win64")) applied = true; break;
					case "Desolate":
						if (CopySteamDLLs(server.InstallPath, @"Desolate\Binaries\Win64")) applied = true; break;
					case "Fragmented":
						if (CopySteamDLLs(server.InstallPath, @"Fragmented\Binaries\Win64")) applied = true; break;
					case "GRAV":
						if (CopySteamDLLs(server.InstallPath, @"CAG\Binaries\Win64")) applied = true; break;
					case "Eden Star":
						if (CopySteamDLLs(server.InstallPath, @"EdenGame\Binaries\Win64")) applied = true; break;
					case "Rokh":
						if (CopySteamDLLs(server.InstallPath, @"Rokh\Binaries\Win64")) applied = true; break;
					case "Outpost Zero":
						if (CopySteamDLLs(server.InstallPath, @"OutpostZero\Binaries\Win64")) applied = true; break;
					case "Rend":
						if (CopySteamDLLs(server.InstallPath, @"Rend\Binaries\Win64")) applied = true; break;
					case "Night of the Dead":
						if (CopySteamDLLs(server.InstallPath, @"LF\Binaries\Win64")) applied = true; break;
					case "Tower Unite":
						if (CopySteamDLLs(server.InstallPath, @"TowerUnite\Binaries\Win64")) applied = true; break;
					case "Witch It":
						if (CopySteamDLLs(server.InstallPath, @"WitchIt\Binaries\Win64")) applied = true; break;
					case "Shattered Skies":
						if (CopySteamDLLs(server.InstallPath, @"ShatteredSkies\Binaries\Win64")) applied = true; break;
					case "Ready or Not":
						if (CopySteamDLLs(server.InstallPath, @"ReadyOrNot\Binaries\Win64")) applied = true; break;
					case "No One Survived":
						if (CopySteamDLLs(server.InstallPath, @"NoOneSurvived\Binaries\Win64")) applied = true; break;
					case "Killing Floor 2":
					case "Rising Storm 2: Vietnam":
					case "Red Orchestra 2: Heroes of Stalingrad":
					case "Unreal Tournament 3":
					case "Viscera Cleanup Detail":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Windrose":
						if (CopySteamDLLs(server.InstallPath, @"R5\Binaries\Win64")) applied = true; break;
					case "Subsistence":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
				}

				if (TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
					definition != null)
				{
					ConfigurationApplyResult result = await ApplyManagedConfiguration(server);
					if (!result.Succeeded)
					{
						Core.Instance.Log($"[CONFIG ERROR] {result.Message}", Color.Red);
					}
					else
					{
						if (result.Created)
						{
							ManualConfigWasCreated = true;
						}

						if (result.Changed)
						{
							applied = true;
						}

						if (!result.Complete)
						{
							Core.Instance.Log($"[CONFIG WARNING] {result.Message}", Color.Orange);
						}
					}
				}
			}
			catch (Exception)
			{
				return false;
			}

			return applied;
		}

		private static IReadOnlyDictionary<string, ConfigurationDefinition> CreateConfigurationIndex()
		{
			ConfigurationDefinition[] definitions =
			[
				new SevenDaysToDieConfiguration(),
				new SoulmaskConfiguration(),
				new PalworldConfiguration(),
				new RustConfiguration(),
				new MinecraftConfiguration(),
				new StarRuptureConfiguration(),
				new SubsistenceConfiguration(),
				new WindroseConfiguration(),
				new AskaConfiguration(),
				new JustCause3MultiplayerConfiguration(),
				new SonsOfTheForestConfiguration(),
				new EnshroudedConfiguration(),
				new LongvinterConfiguration(),
				new GroundBranchConfiguration(),
				new HoldfastConfiguration(),
				new VRisingConfiguration(),
				new OutOfReachConfiguration(),
				new Ns2CombatConfiguration(),
				new JustCause2MultiplayerConfiguration(),
				new BeyondTheWireConfiguration(),
				new ColonySurvivalConfiguration(),
				new CoreKeeperConfiguration(),
				new FactorioConfiguration(),
				new EcoConfiguration(),
				new ProjectCars2Configuration(),
				new AssettoCorsaCompetizioneConfiguration(),
				new RFactor2Configuration(),
				new SurviveTheNightsConfiguration(),
				new FoundryConfiguration(),
				new HumanitZConfiguration(),
				new AstroneerConfiguration(),
				new DayZConfiguration(),
				new Arma3Configuration(),
				new ArmaReforgerConfiguration(),
				new BannerlordConfiguration(),
				new DysterraConfiguration(),
				new SeriousSam2017Configuration(),
				new SeriousSamHdConfiguration(),
				new WreckfestConfiguration()
			];

			Dictionary<string, ConfigurationDefinition> index =
				new(StringComparer.OrdinalIgnoreCase);
			foreach (ConfigurationDefinition definition in definitions)
			{
				index.Add(definition.GameName, definition);
				foreach (string alias in definition.Aliases)
				{
					index.Add(alias, definition);
				}
			}

			return index;
		}

		private static bool CopySteamDLLs(string installPath, string binariesDirectory)
		{
			bool filesCopied = false;
			string[] dlls = ["steamclient64.dll", "tier0_s64.dll", "vstdlib_s64.dll"];
			string targetDirectory = Path.Combine(installPath, binariesDirectory);
			string steamCmdPath = Core.SteamCmdPath;

			if (!Directory.Exists(targetDirectory))
			{
				Directory.CreateDirectory(targetDirectory);
			}

			foreach (string dll in dlls)
			{
				string sourcePath = Path.Combine(steamCmdPath, dll);
				if (File.Exists(sourcePath) &&
					!File.Exists(Path.Combine(targetDirectory, dll)) &&
					FileHandler.Copy(sourcePath, targetDirectory, dll, false))
				{
					filesCopied = true;
				}
			}

			return filesCopied;
		}
	}
}
