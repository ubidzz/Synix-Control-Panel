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
using Synix_Control_Panel.SynixApp.Database;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static readonly Regex SafeRegex = new Regex(@"^[a-zA-Z0-9\s\-+:\""\\/._=?,]*$", RegexOptions.Compiled);

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

			if (checkRcon) portChecks.Add((rcon, "RCON Port"));

			if (checkAppPort) portChecks.Add((app, "App Port (Rust+)"));

			foreach (var check in portChecks)
			{
				var owner = GetPortCollisionOwner(check.Value, excluding);
				if (owner != null)
				{
					MessageBox.Show($"Resource Collision: The {check.Name} ({check.Value}) is already allocated to instance: '{owner}'.",
									"Network Resource Conflict", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return false;
				}

				if (IsPortInUseLocally(check.Value))
				{
					MessageBox.Show($"Socket Conflict: The {check.Name} ({check.Value}) is currently occupied by another system process.",
									"System Resource Conflict", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return false;
				}
			}

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
			if (server.IsFirstBoot)
			{
				DialogResult result = DialogResult.Cancel;

				if (MainGUI.Instance != null && MainGUI.Instance.InvokeRequired)
				{
					MainGUI.Instance?.AppendLog($"[🛠️ CONFIG] Opening mandatory configuration warning for {server.ServerName}...", Color.Yellow);
					MainGUI.Instance.Invoke((Action)(() =>
					{
						using (var warningForm = new Synix_Control_Panel.Database.WarningDatabase(server))
						{
							result = warningForm.ShowDialog(MainGUI.Instance);
						}
					}));
				}
				else
				{
					using (var warningForm = new Synix_Control_Panel.Database.WarningDatabase(server))
					{
						result = warningForm.ShowDialog(MainGUI.Instance);
					}
				}

				return result != DialogResult.OK;
			}

			return false;
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
			GameServer? owner = MainGUI.serverList.FirstOrDefault(server =>
			{
				if (server == excluding)
					return false;

				if (server.Port == port)
					return true;

				GameInfo? gameData = GameDatabase.GetGame(server.Game);
				string requiredArgs = gameData?.RequiredArgs ?? "";
				string rconSyntax = gameData?.RconSyntax ?? "";

				bool usesQueryPort = requiredArgs.Contains(
					"{query}",
					StringComparison.OrdinalIgnoreCase);

				if (usesQueryPort &&
					server.QueryPort > 0 &&
					server.QueryPort == port)
				{
					return true;
				}

				bool usesAppPort = requiredArgs.Contains(
					"{app_port}",
					StringComparison.OrdinalIgnoreCase);

				if (usesAppPort &&
					server.AppPort.HasValue &&
					server.AppPort.Value > 0 &&
					server.AppPort.Value == port)
				{
					return true;
				}

				bool usesRconPort =
					requiredArgs.Contains("{rcon_port}", StringComparison.OrdinalIgnoreCase) ||
					requiredArgs.Contains("{rcon}", StringComparison.OrdinalIgnoreCase) ||
					rconSyntax.Contains("{rcon_port}", StringComparison.OrdinalIgnoreCase);

				return server.EnableRcon &&
					usesRconPort &&
					server.RconPort > 0 &&
					server.RconPort == port;
			});

			return owner?.ServerName;
		}

		public bool PassResourceGuard(out string message)
		{
			message = string.Empty;

			if (TotalCpuUsage >= 80.0)
			{
				message = $"[RESOURCE GUARD] CPU load is critical ({TotalCpuUsage:F1}%). Launch aborted.";
				return false;
			}

			double currentRamPercent = (TotalRamUsageGb / TotalRamGb) * 100;

			if (currentRamPercent >= 85.0)
			{
				message = $"[RESOURCE GUARD] System RAM usage is at {currentRamPercent:F1}% of the usable pool.";
				return false;
			}

			return true;
		}

		public static bool IsStringSafe(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return true;

			if (input.Contains("..")) return false;

			return SafeRegex.IsMatch(input);
		}

		public static bool IsGameServerConfigSafe(object obj)
		{
			if (obj == null) return false;

			if (obj is string directString)
			{
				return IsStringSafe(directString);
			}

			PropertyInfo[] properties = obj.GetType().GetProperties();

			foreach (var prop in properties)
			{
				if (prop.PropertyType == typeof(string))
				{
					string value = (string)prop.GetValue(obj);

					if (!IsStringSafe(value))
					{
						Core.Instance.Log($"[🚨 SECURITY] Illegal characters found in property: {prop.Name}");
						return false;
					}
				}
			}
			return true;
		}

		public static int GetSystemJavaVersion()
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "java",
					Arguments = "-version",
					RedirectStandardError = true, // Java prints version info to the Error stream, not Output
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using Process proc = Process.Start(psi);
				string output = proc.StandardError.ReadToEnd();
				proc.WaitForExit();

				// Older Java 8 formats like: java version "1.8.0_xxx"
				if (output.Contains("version \"1.8")) return 8;
				if (output.Contains("version \"1.7")) return 7;

				// Modern Java 9+ formats like: openjdk version "21.0.2"
				int startIndex = output.IndexOf("version \"") + 9;
				if (startIndex > 8)
				{
					int endIndex = output.IndexOf('.', startIndex);
					if (endIndex == -1) endIndex = output.IndexOf('"', startIndex);

					if (endIndex > startIndex)
					{
						string versionStr = output.Substring(startIndex, endIndex - startIndex);
						if (int.TryParse(versionStr, out int version)) return version;
					}
				}
			}
			catch
			{
				// Triggers if Java is completely missing or not added to Windows PATH
			}
			return 0;
		}
	}
}