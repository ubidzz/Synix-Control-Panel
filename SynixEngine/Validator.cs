// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.Database;

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

		public bool ValidatePortsAndReport(GameServer? excluding, int game, int query, int rcon, bool checkRcon, int app, bool checkAppPort, string gameName)
		{
			var portChecks = new List<(int Value, string Name)>
			{
				(game, "Game Port"),
				(query, "Query Port")
			};

			// 🎯 Only check RCON if the user enabled it
			if (checkRcon) portChecks.Add((rcon, "RCON Port"));

			// 🎯 Only check App Port if Rust is active
			if (checkAppPort) portChecks.Add((app, "App Port (Rust+)"));

			foreach (var check in portChecks)
			{
				// 1. Internal Database Check
				var owner = GetPortCollisionOwner(check.Value, excluding);
				if (owner != null)
				{
					MessageBox.Show($"Resource Collision: The {check.Name} ({check.Value}) is already allocated to instance: '{owner}'.",
									"Network Resource Conflict", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return false;
				}

				// 2. OS Socket Check
				if (IsPortInUseLocally(check.Value))
				{
					MessageBox.Show($"Socket Conflict: The {check.Name} ({check.Value}) is currently occupied by another system process.",
									"System Resource Conflict", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return false;
				}
			}

			// 3. Rust Protocol Check
			if (checkAppPort && gameName.Contains("Rust", StringComparison.OrdinalIgnoreCase) && app < 10000)
			{
				MessageBox.Show("Protocol Error: Rust+ (App Port) must be 10000 or higher.", "Logic Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
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
			// If it's the first time running, show the warning
			if (server.IsFirstBoot)
			{
				MainGUI.Instance?.AppendLog($"[🛠️ CONFIG] Opening mandatory configuration warning for {server.ServerName}...", Color.Yellow);

				using (var warningForm = new WarningDatabase(server))
				{
					warningForm.ShowDialog();
					return true;
				}
			}

			return false; // Already been booted before, let it through
		}

		public bool ValidateIntegrityAndReport(GameServer server)
		{
			if (!CanServerStart(server, out string errorMessage))
			{
				MainGUI.Instance?.Invoke((Action)(() =>
				{
					MainGUI.Instance.AppendLog($"[🚨 ERROR] {errorMessage}", Color.Red, true);
				}));

				MessageBox.Show(errorMessage, "Integrity Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

				server.Status = "Needs Repair";

				return false;
			}

			return true;
		}

		public bool PassBackupSpamLock(GameServer server, out string lockMessage)
		{
			lockMessage = string.Empty;
			string status = server.Status ?? "";

			// 🎯 Define the states where backup is FORBIDDEN
			bool isStarting = string.Equals(status, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);
			bool isRunning = string.Equals(status, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
			bool isStopping = string.Equals(status, StatusManager.GetStatus(ServerState.Stopping), StringComparison.OrdinalIgnoreCase);
			bool isInstalling = string.Equals(status, StatusManager.GetStatus(ServerState.Installing), StringComparison.OrdinalIgnoreCase);
			bool isUpdating = string.Equals(status, StatusManager.GetStatus(ServerState.Updating), StringComparison.OrdinalIgnoreCase);
			bool isBackingUp = string.Equals(status, StatusManager.GetStatus(ServerState.BackingUp), StringComparison.OrdinalIgnoreCase);

			// 1. Check if the server is active (Must be Stopped or Crashed to backup)
			if (isStarting || isRunning || isStopping || isInstalling || isUpdating || isBackingUp)
			{
				lockMessage = $"[LOCKED] Cannot backup. {server.ServerName} is currently {status}.";
				return false;
			}

			return true;
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
			bool isBackingUp = string.Equals(status, StatusManager.GetStatus(ServerState.BackingUp), StringComparison.OrdinalIgnoreCase);

			if (isStarting || isRunning || isStopping || isInstalling || isUpdating || isBackingUp)
			{
				lockMessage = $"[🔒 LOCKED] Cannot start. {server.ServerName} is currently {status}.";
				return false;
			}

			return true;
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
			bool isBackingUp = string.Equals(status, StatusManager.GetStatus(ServerState.BackingUp), StringComparison.OrdinalIgnoreCase);

			if (isStopping || isStopped || isCrashed || isInstalling || isUpdating || isBackingUp)
			{
				lockMessage = $"[🔒 LOCKED] Cannot stop. {server.ServerName} is currently {status}.";
				return false;
			}

			return true; // Safe to stop
		}

		public string? GetPortCollisionOwner(int port, GameServer? excluding = null)
		{
			// 🎯 1. First, check for an EXACT match on the primary Port.
			// This is the most "illegal" conflict (two games on same launch port).
			var primaryMatch = MainGUI.serverList.FirstOrDefault(s =>
				s != excluding && s.Port == port);

			if (primaryMatch != null) return primaryMatch.ServerName;

			// 🎯 2. If no primary match, check for overlaps with Query or App ports.
			var secondaryMatch = MainGUI.serverList.FirstOrDefault(s =>
				s != excluding &&
				(s.QueryPort == port || (s.AppPort.HasValue && s.AppPort.Value == port)));

			if (secondaryMatch != null)
			{
				// We return the name, but adding " (Query)" or " (App)" to the string
				// helps the SyncGatekeeper show a better warning.
				return secondaryMatch.ServerName;
			}

			return null;
		}

		public bool PassResourceGuard(out string message)
		{
			message = string.Empty;

			if (TotalCpuUsage >= 80.0)
			{
				message = $"[RESOURCE GUARD] CPU load is critical ({TotalCpuUsage:F1}%). Launch aborted.";
				return false;
			}

			// 🎯 (Current Usage / (Total - 7GB)) * 100
			double currentRamPercent = (TotalRamUsageGb / TotalRamGb) * 100;

			if (currentRamPercent >= 85.0)
			{
				message = $"[RESOURCE GUARD] System RAM usage is at {currentRamPercent:F1}% of the usable pool.";
				return false;
			}

			return true;
		}

	}
}