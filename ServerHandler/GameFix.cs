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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Synix_Control_Panel.ServerHandler
{
	public static class GameFix
	{
		public static void PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) || !Directory.Exists(server.InstallPath))
				return;

			try
			{
				// We use a switch statement to look at the name of the game and apply the specific fix
				switch (server.Game)
				{
					case "Soulmask":
						FixSoulmask(server.InstallPath);
						break;

					case "StarRupture":
						FixStarRupture(server.InstallPath);
						break;

					case "Sons Of The Forest":
						FixSonsOfTheForest(server.InstallPath);
						break;

					case "Palworld":
						FixPalworld(server.InstallPath);
						break;

					case "Enshrouded":
						FixEnshrouded(server.InstallPath);
						break;
				}
			}
			catch (Exception)
			{
				// Optional: Log the error so your app doesn't crash if a file is locked
			}
		}

		// --------------------------------------------------------
		// INDIVIDUAL GAME FIXES BELOW
		// --------------------------------------------------------

		private static void FixSoulmask(string installPath)
		{
			string[] dlls = { "steamclient64.dll", "tier0_s64.dll", "vstdlib_s64.dll" };
			string targetDir = Path.Combine(installPath, @"WS\Binaries\Win64");

			foreach (string dll in dlls)
			{
				string sourcePath = Path.Combine(installPath, dll);

				// Ensure the file exists before we try to copy it, and only copy if it's missing in the destination
				if (File.Exists(sourcePath) && !File.Exists(Path.Combine(targetDir, dll)))
				{
					// USING YOUR NEW COPY UTILITY!
					FileHandler.Copy(sourcePath, targetDir, dll, false);
				}
			}
		}

		private static void FixStarRupture(string installPath)
		{
			// Only create it if it doesn't exist so we don't overwrite user settings
			if (!File.Exists(Path.Combine(installPath, "DSSetings.txt")))
			{
				string content = @"{
				  ""SessionName"": ""SESSIONNAME"",
				  ""SaveGameInterval"": ""300"",
				  ""StartNewGame"": ""true"",
				  ""LoadSavedGame"": ""false"",
				  ""SaveGameName"": ""AutoSave0.sav""
				}";
				// USING YOUR NEW CREATE UTILITY!
				FileHandler.Create(installPath, "DSSetings.txt", content);
			}
		}

		private static void FixSonsOfTheForest(string installPath)
		{
			string configDir = Path.Combine(installPath, "config");

			if (!File.Exists(Path.Combine(configDir, "dedicatedserver.cfg")))
			{
				string content = @"{
				  ""IpAddress"": ""0.0.0.0"",
				  ""GamePort"": 8766,
				  ""QueryPort"": 27016,
				  ""ServerName"": ""SOTF Server"",
				  ""MaxPlayers"": 8,
				  ""Password"": """",
				  ""ServerPlayMode"": ""Normal""
				}";
				// USING YOUR NEW CREATE UTILITY!
				FileHandler.Create(configDir, "dedicatedserver.cfg", content);
			}
		}

		private static void FixPalworld(string installPath)
		{
			string configDir = Path.Combine(installPath, @"Pal\Saved\Config\WindowsServer");

			if (!File.Exists(Path.Combine(configDir, "PalWorldSettings.ini")))
			{
				string content = @"[/Script/Pal.PalGameWorldSettings]
					OptionSettings=(ServerName=""Default Palworld Server"",ServerPassword="""",ServerPlayerMaxNum=32,bEnableInvaderEnemy=True,bEnableAimAssistPad=True,bEnableAimAssistKeyboard=False,DropItemMaxNum=3000,BaseCampMaxNum=128,BaseCampWorkerMaxNum=15,DropItemAliveMaxHours=1.000000,bAutoResetGuildNoOnlinePlayers=False,AutoResetGuildTimeNoOnlinePlayers=72.000000,GuildPlayerMaxNum=20,PalEggDefaultHatchingTime=72.000000,WorkSpeedRate=1.000000,bIsMultiplay=True,bIsPvP=False,bCanPickupOtherGuildDeathPenaltyDrop=False,bEnableNonLoginPenalty=True,bEnableFastTravel=True,bIsStartLocationSelectByMap=True,bExistPlayerAfterLogout=False,bEnableDefenseOtherGuildPlayer=False,CoopPlayerMaxNum=4,ServerPlayerMaxNum=32,ServerName=""Default Palworld Server"",ServerDescription="""",ServerPassword="""",AdminPassword="""",PublicPort=8211,PublicIP="""",RCONEnabled=False,RCONPort=25575,Region="""",bUseAuth=True,BanListURL=""https://api.palworldgame.com/api/banlist.txt"")";

				// USING YOUR NEW CREATE UTILITY!
				FileHandler.Create(configDir, "PalWorldSettings.ini", content);
			}
		}

		private static void FixEnshrouded(string installPath)
		{
			if (!File.Exists(Path.Combine(installPath, "enshrouded_server.json")))
			{
				string content = @"{
				  ""name"": ""Enshrouded Server"",
				  ""password"": """",
				  ""saveDirectory"": ""./savegame"",
				  ""logDirectory"": ""./logs"",
				  ""ip"": ""0.0.0.0"",
				  ""gamePort"": 15636,
				  ""queryPort"": 15637,
				  ""slotCount"": 16
				}";
				// USING YOUR NEW CREATE UTILITY!
				FileHandler.Create(installPath, "enshrouded_server.json", content);
			}
		}
	}
}
