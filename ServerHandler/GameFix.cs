/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System;
using System.IO;
using Synix_Control_Panel.FileFolderHandler; // Points to your FolderHandler utility

namespace Synix_Control_Panel.ServerHandler
{
	public static class GameFix
	{
		public static bool PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) || !Directory.Exists(server.InstallPath))
				return false;

			bool applied = false;

			try
			{
				// --- PASS 1: STEAM DLL COPIES ---
				switch (server.Game)
				{
					case "Soulmask":
						if (CopySteamDLLs(server.InstallPath, @"WS\Binaries\Win64")) applied = true; break;
					case "StarRupture":
						if (CopySteamDLLs(server.InstallPath, @"StarRupture\Binaries\Win64")) applied = true; break;
					case "Palworld":
						if (CopySteamDLLs(server.InstallPath, @"Pal\Binaries\Win64")) applied = true; break;
					case "ARK: Survival Ascended":
					case "ARK: Survival Evolved":
					case "Atlas":
					case "PixARK":
						if (CopySteamDLLs(server.InstallPath, @"ShooterGame\Binaries\Win64")) applied = true; break;
					case "Astroneer":
						if (CopySteamDLLs(server.InstallPath, @"Astro\Binaries\Win64")) applied = true; break;
					case "Abiotic Factor":
						if (CopySteamDLLs(server.InstallPath, @"AbioticFactor\Binaries\Win64")) applied = true; break;
					case "Icarus":
						if (CopySteamDLLs(server.InstallPath, @"Icarus\Binaries\Win64")) applied = true; break;
					case "Killing Floor 2":
					case "Rising Storm 2: Vietnam":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "The Front":
						if (CopySteamDLLs(server.InstallPath, @"ProjectWar\Binaries\Win64")) applied = true; break;
					case "Smalland: Survive the Wilds":
						if (CopySteamDLLs(server.InstallPath, @"SMALLAND\Binaries\Win64")) applied = true; break;
					case "Mordhau":
						if (CopySteamDLLs(server.InstallPath, @"Mordhau\Binaries\Win64")) applied = true; break;
					case "The Isle":
						if (CopySteamDLLs(server.InstallPath, @"TheIsle\Binaries\Win64")) applied = true; break;
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
					case "Grounded":
						if (CopySteamDLLs(server.InstallPath, @"Maine\Binaries\Win64")) applied = true; break;
					case "Gray Zone Warfare":
						if (CopySteamDLLs(server.InstallPath, @"GZW\Binaries\Win64")) applied = true; break;
					case "HumanitZ":
						if (CopySteamDLLs(server.InstallPath, @"HumanitZ\Binaries\Win64")) applied = true; break;
					case "Pavlov VR":
						if (CopySteamDLLs(server.InstallPath, @"Pavlov\Binaries\Win64")) applied = true; break;
					case "Ready or Not":
						if (CopySteamDLLs(server.InstallPath, @"ReadyOrNot\Binaries\Win64")) applied = true; break;
				}

				// --- PASS 2: DYNAMIC CONFIG FILE CREATION ---
				switch (server.Game)
				{
					case "StarRupture":
						string srJson = @"{ ""SessionName"": ""{ServerName}"", ""SaveGameInterval"": ""300"", ""StartNewGame"": ""true"", ""LoadSavedGame"": ""false"", ""SaveGameName"": ""AutoSave0.sav"" }";
						if (CreateGameConfig(server, "DSSettings.txt", srJson)) applied = true;
						break;

					case "ASKA":
						string askaJson = @"{ ""ServerName"": ""{ServerName}"", ""Password"": """", ""MaxPlayers"": 16 }";
						if (CreateGameConfig(server, "server_settings.json", askaJson)) applied = true;
						break;

					case "Raft Dedicated Server":
						string raftJson = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": 8 }";
						if (CreateGameConfig(server, "server_config.json", raftJson)) applied = true;
						break;

					case "Sons Of The Forest":
						string sotfCfg = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": 8, ""ServerPlayMode"": ""Normal"" }";
						if (CreateGameConfig(server, @"userdata\dedicated_server.cfg", sotfCfg)) applied = true;
						break;

					case "Palworld":
						string palIni = @"[/Script/Pal.PalGameWorldSettings] 
							OptionSettings=(ServerName=""{ServerName}"")";
						if (CreateGameConfig(server, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini", palIni)) applied = true;
						break;

					case "Enshrouded":
						string enshroudedJson = @"{ ""name"": ""{ServerName}"", ""slotCount"": 16 }";
						if (CreateGameConfig(server, "enshrouded_server.json", enshroudedJson)) applied = true;
						break;
				}
			}
			catch (Exception)
			{
				return false;
			}

			return applied;
		}

		// --------------------------------------------------------
		// UNIFIED UTILITY FUNCTIONS
		// --------------------------------------------------------

		/// <summary>
		/// Unified function to create game configuration files with dynamic tag replacement.
		/// It correctly combines server.InstallPath with the relative file path.
		/// </summary>
		private static bool CreateGameConfig(GameServer server, string relativeFilePath, string contentTemplate)
		{
			// The full path to the file
			string fullFilePath = Path.Combine(server.InstallPath, relativeFilePath);

			// Gate creation so we don't overwrite existing user data
			if (!File.Exists(fullFilePath))
			{
				// Inject the dynamic server name from your GUI
				string finalContent = contentTemplate.Replace("{ServerName}", server.ServerName);

				// identifies the directory part of the path (e.g., 'userdata' or 'Pal\Saved\...')
				string targetFolder = Path.GetDirectoryName(fullFilePath);

				// Path.GetFileName extracts just the filename (e.g., 'dedicated_server.cfg')
				string fileName = Path.GetFileName(fullFilePath);

				// Uses your FileHandler.Create which calls FolderHandler.Create internally
				return FileHandler.Create(targetFolder, fileName, finalContent);
			}

			return false;
		}

		/// <summary>
		/// Copies required Steam DLLs to the target engine's binary directory.
		/// </summary>
		private static bool CopySteamDLLs(string installPath, string BinariesDir)
		{
			bool filesCopied = false;
			string[] dlls = { "steamclient64.dll", "tier0_s64.dll", "vstdlib_s64.dll" };
			string targetDir = Path.Combine(installPath, BinariesDir);

			foreach (string dll in dlls)
			{
				string sourcePath = Path.Combine(installPath, dll);

				if (File.Exists(sourcePath) && !File.Exists(Path.Combine(targetDir, dll)))
				{
					// Uses your FileHandler.Copy utility
					if (FileHandler.Copy(sourcePath, targetDir, dll, false))
					{
						filesCopied = true;
					}
				}
			}
			return filesCopied;
		}
	}
}