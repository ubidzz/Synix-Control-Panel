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
		// Changed from void to bool!
		public static bool PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) || !Directory.Exists(server.InstallPath))
				return false;

			try
			{
				// Now it returns true if a fix was applied, or false if it wasn't
				switch (server.Game)
				{
					case "Soulmask": return FixSoulmask(server.InstallPath);
					case "StarRupture": return FixStarRupture(server.InstallPath);
					case "Sons Of The Forest": return FixSonsOfTheForest(server.InstallPath);
					case "Palworld": return FixPalworld(server.InstallPath);
					case "Enshrouded": return FixEnshrouded(server.InstallPath);
					default: return false; // No fixes needed for this game
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		// --------------------------------------------------------
		// INDIVIDUAL GAME FIXES BELOW
		// --------------------------------------------------------

		private static bool FixSoulmask(string installPath)
		{
			bool filesCopied = false;
			string[] dlls = { "steamclient64.dll", "tier0_s64.dll", "vstdlib_s64.dll" };
			string targetDir = Path.Combine(installPath, @"WS\Binaries\Win64");

			foreach (string dll in dlls)
			{
				string sourcePath = Path.Combine(installPath, dll);

				if (File.Exists(sourcePath) && !File.Exists(Path.Combine(targetDir, dll)))
				{
					if (FileHandler.Copy(sourcePath, targetDir, dll, false))
					{
						filesCopied = true; // We actually copied something!
					}
				}
			}
			return filesCopied;
		}

		private static bool FixStarRupture(string installPath)
		{
			if (!File.Exists(Path.Combine(installPath, "DSSetings.txt")))
			{
				string content = @"{
				  ""SessionName"": ""SESSIONNAME"",
				  ""SaveGameInterval"": ""300"",
				  ""StartNewGame"": ""true"",
				  ""LoadSavedGame"": ""false"",
				  ""SaveGameName"": ""AutoSave0.sav""
				}";
				return FileHandler.Create(installPath, "DSSetings.txt", content);
			}
			return false;
		}

		private static bool FixSonsOfTheForest(string installPath)
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
				return FileHandler.Create(configDir, "dedicatedserver.cfg", content);
			}
			return false;
		}

		private static bool FixPalworld(string installPath)
		{
			string configDir = Path.Combine(installPath, @"Pal\Saved\Config\WindowsServer");

			if (!File.Exists(Path.Combine(configDir, "PalWorldSettings.ini")))
			{
				string content = @"[/Script/Pal.PalGameWorldSettings]
					OptionSettings=(ServerName=""Default Palworld Server"",ServerPassword="""",ServerPlayerMaxNum=32,bEnableInvaderEnemy=True,bEnableAimAssistPad=True,bEnableAimAssistKeyboard=False,DropItemMaxNum=3000,BaseCampMaxNum=128,BaseCampWorkerMaxNum=15,DropItemAliveMaxHours=1.000000,bAutoResetGuildNoOnlinePlayers=False,AutoResetGuildTimeNoOnlinePlayers=72.000000,GuildPlayerMaxNum=20,PalEggDefaultHatchingTime=72.000000,WorkSpeedRate=1.000000,bIsMultiplay=True,bIsPvP=False,bCanPickupOtherGuildDeathPenaltyDrop=False,bEnableNonLoginPenalty=True,bEnableFastTravel=True,bIsStartLocationSelectByMap=True,bExistPlayerAfterLogout=False,bEnableDefenseOtherGuildPlayer=False,CoopPlayerMaxNum=4,ServerPlayerMaxNum=32,ServerName=""Default Palworld Server"",ServerDescription="""",ServerPassword="""",AdminPassword="""",PublicPort=8211,PublicIP="""",RCONEnabled=False,RCONPort=25575,Region="""",bUseAuth=True,BanListURL=""https://api.palworldgame.com/api/banlist.txt"")";

				return FileHandler.Create(configDir, "PalWorldSettings.ini", content);
			}
			return false;
		}

		private static bool FixEnshrouded(string installPath)
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
				return FileHandler.Create(installPath, "enshrouded_server.json", content);
			}
			return false;
		}
	}
}