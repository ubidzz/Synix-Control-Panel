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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Synix_Control_Panel.Database;
using Synix_Control_Panel.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		public bool CanServerStart(GameServer server, out string errorMessage)
		{
			var dbEntry = GameDatabase.GetGame(server.Game);
			if (dbEntry == null)
			{
				errorMessage = "Game not found in database.";
				return false;
			}

			string fullPath = Path.Combine(server.InstallPath, dbEntry.ExeName);
			if (!File.Exists(fullPath))
			{
				errorMessage = "The game files are missing! Please run 'Update' to fix the server.";
				return false;
			}

			errorMessage = "";
			return true;
		}

		// Include these to ensure Validator.cs is complete
		public bool ValidateNameAndReport(string name, string game, GameServer? excluding = null)
		{
			bool exists = MainGUI.serverList.Any(s =>
				s.Game.Equals(game, StringComparison.OrdinalIgnoreCase) &&
				s.ServerName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
				s != excluding);

			if (exists)
			{
				MessageBox.Show($"You already have a {game} server named '{name}'.",
								"Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			return true;
		}

		public bool ValidatePortsAndReport(int p, int q, int r, bool rconActive, GameServer? excluding = null)
		{
			if (p == q)
			{
				MessageBox.Show("Game and Query ports must be different.", "Port Conflict");
				return false;
			}

			foreach (var s in MainGUI.serverList)
			{
				if (s == excluding) continue;
				if (s.Port == p || s.QueryPort == q)
				{
					MessageBox.Show($"Port conflict with '{s.ServerName}'.", "Conflict");
					return false;
				}
			}
			return true;
		}

		public bool ValidateFolderAndReport(string path, bool isEditMode)
		{
			if (!isEditMode && Directory.Exists(path))
			{
				// Check if the folder is empty
				if (Directory.EnumerateFileSystemEntries(path).Any())
				{
					var result = MessageBox.Show("This folder isn't empty. Installing here might overwrite files. Continue?",
											   "Folder Not Empty", MessageBoxButtons.YesNo);
					return result == DialogResult.Yes;
				}
			}
			return true;
		}

		public bool ShouldBlockForConfig(GameServer server)
		{
			if (server.NeedsConfigWarning && server.IsFirstBoot)
			{
				// The AI handles the popup creation and display
				using (var warningForm = new WarningDatabase(server))
				{
					warningForm.ShowDialog();

					// Return true to tell the GUI "Stop what you are doing!"
					return true;
				}
			}

			// Everything is fine, allow the server to start
			return false;
		}

		public bool ValidateIntegrityAndReport(GameServer server)
		{
			// 1. Run the internal technical check
			if (!CanServerStart(server, out string errorMessage))
			{
				// 2. The AI logs the error to the MainGUI thread
				MainGUI.Instance?.Invoke((Action)(() => {
					MainGUI.Instance.AppendLog($"[ERROR] {errorMessage}", Color.Red, true);
				}));

				// 3. The AI shows the popup
				MessageBox.Show(errorMessage, "Integrity Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// 4. The AI marks the server as broken
				server.Status = "Needs Repair";

				return false; // Stop the launch
			}

			return true; // Files are good!
		}

		public bool PassStartSpamLock(GameServer server, out string lockMessage)
		{
			lockMessage = string.Empty;
			string status = server.Status ?? "";

			bool isStarting = string.Equals(status, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);
			bool isRunning = string.Equals(status, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
			bool isStopping = string.Equals(status, StatusManager.GetStatus(ServerState.Stopping), StringComparison.OrdinalIgnoreCase);
			bool isInstalling = string.Equals(status, StatusManager.GetStatus(ServerState.Installing), StringComparison.OrdinalIgnoreCase);
			bool isUpdating = string.Equals(status, StatusManager.GetStatus(ServerState.Updating), StringComparison.OrdinalIgnoreCase);

			if (isStarting || isRunning || isStopping || isInstalling || isUpdating)
			{
				lockMessage = $"[LOCKED] Cannot start. {server.ServerName} is currently {status}.";
				return false; // Lock is triggered
			}

			return true; // Safe to start
		}

		public bool PassStopSpamLock(GameServer server, out string lockMessage)
		{
			lockMessage = string.Empty;
			string status = server.Status ?? "";

			bool isStopping = string.Equals(status, StatusManager.GetStatus(ServerState.Stopping), StringComparison.OrdinalIgnoreCase);
			bool isStopped = string.Equals(status, StatusManager.GetStatus(ServerState.Stopped), StringComparison.OrdinalIgnoreCase);
			bool isCrashed = string.Equals(status, StatusManager.GetStatus(ServerState.Crashed), StringComparison.OrdinalIgnoreCase);
			bool isInstalling = string.Equals(status, StatusManager.GetStatus(ServerState.Installing), StringComparison.OrdinalIgnoreCase);
			bool isUpdating = string.Equals(status, StatusManager.GetStatus(ServerState.Updating), StringComparison.OrdinalIgnoreCase);

			if (isStopping || isStopped || isCrashed || isInstalling || isUpdating)
			{
				lockMessage = $"[LOCKED] Cannot stop. {server.ServerName} is currently {status}.";
				return false; // Lock is triggered
			}

			return true; // Safe to stop
		}
	}
}