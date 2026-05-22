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

		public bool PassSpamLock(GameServer server, out string lockMessage, string serverTrigger)
		{
			lockMessage = string.Empty;
			string status = server.Status ?? "";

			// 1. Define states ONCE
			bool isTransitioning = status == StatusManager.GetStatus(ServerState.Starting) ||
								   status == StatusManager.GetStatus(ServerState.Stopping) ||
								   status == StatusManager.GetStatus(ServerState.Installing) ||
								   status == StatusManager.GetStatus(ServerState.Updating) ||
								   status == StatusManager.GetStatus(ServerState.BackingUp) ||
								   status == StatusManager.GetStatus(ServerState.Export) ||
								   status == StatusManager.GetStatus(ServerState.Validating);

			bool isRunning = status == StatusManager.GetStatus(ServerState.Running);
			bool isStopped = status == StatusManager.GetStatus(ServerState.Stopped);
			bool isCrashed = status == StatusManager.GetStatus(ServerState.Crashed);

			// 2. Evaluate based on trigger
			bool isLocked = false;

			switch (serverTrigger)
			{
				case "Backup":
				case "Delete":
				case "EditConfig":
				case "Validate":
				case "Config":
				case "Update":
				case "Start":
				case "Export":
					isLocked = isTransitioning || isRunning;
					break;
				case "Restart":
					isLocked = isTransitioning;
					break;
				case "Stop":
					isLocked = isTransitioning || isStopped || isCrashed;
					break;
			}

			if (isLocked)
			{
				lockMessage = $"[🔒 LOCKED] Cannot {serverTrigger.ToLower()}. {server.ServerName} is currently {status}.";
				return false;
			}

			return true;
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