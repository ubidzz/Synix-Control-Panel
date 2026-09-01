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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static class Servers
	{
		#region Win32 API for Graceful Shutdown
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool FreeConsole();

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindowAsync(IntPtr windowHandle, int command);

		[DllImport("kernel32.dll")]
		static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);
		delegate bool ConsoleCtrlDelegate(uint CtrlType);

		const uint CTRL_C_EVENT = 0;
		private const int SW_HIDE = 0;
		private const int STD_INPUT_HANDLE = -10;
		private const ushort KEY_EVENT = 0x0001;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct KeyEventRecord
		{
			public int KeyDown;
			public ushort RepeatCount;
			public ushort VirtualKeyCode;
			public ushort VirtualScanCode;
			public char UnicodeChar;
			public uint ControlKeyState;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct InputRecord
		{
			[FieldOffset(0)]
			public ushort EventType;
			[FieldOffset(4)]
			public KeyEventRecord KeyEvent;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int standardHandle);

		[DllImport("kernel32.dll", EntryPoint = "WriteConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool WriteConsoleInput(
			IntPtr consoleInput,
			InputRecord[] buffer,
			uint numberOfEvents,
			out uint numberOfEventsWritten);

		private const uint TH32CS_SNAPPROCESS = 0x00000002;
		private const int MAX_PATH = 260;
		private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct ProcessEntry32
		{
			public uint Size;
			public uint UsageCount;
			public uint ProcessId;
			public IntPtr DefaultHeapId;
			public uint ModuleId;
			public uint ThreadCount;
			public uint ParentProcessId;
			public int BasePriority;
			public uint Flags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string ExeFile;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 entry);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 entry);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);
		#endregion
		private static readonly SemaphoreSlim _consoleLock = new SemaphoreSlim(1, 1);
		private static readonly object _serverProcessRegistryLock = new();
		private static readonly TimeSpan _processDiscoveryInterval = TimeSpan.FromSeconds(5);

		public static async Task Start(GameServer server, Action<string, Color> logCallback, StartContext context = StartContext.Manual)
		{
			ArgumentNullException.ThrowIfNull(logCallback);
			try
			{
				GameInfo? selectedDefinition = GameDatabase.GetGame(server.Game);
				if (selectedDefinition == null)
				{
					logCallback?.Invoke(
						$"[START ERROR] The built-in definition for '{server.Game}' could not be loaded.",
						Color.Red);
					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.ConfigurationWarning,
						"START CONFIGURATION ERROR",
						$"The built-in definition for {server.Game} could not be loaded.",
						Color.Red);
					return;
				}

				GamePrerequisiteReport prerequisites = await Task.Run(() =>
					GamePrerequisiteChecker.CheckCurrentSystem(
						selectedDefinition,
						server,
						port => Core.Instance.GetPortCollisionOwner(
							port,
							server,
							activeOnly: true),
						Core.Instance.IsPortInUseLocally));
				foreach (GamePrerequisiteItem warning in prerequisites.Items.Where(item =>
					item.State == GamePrerequisiteState.Warning))
				{
					logCallback?.Invoke($"[REQUIREMENT WARNING] {warning.Message}", Color.Orange);
				}
				if (!prerequisites.CanStart)
				{
					string message = prerequisites.ToDisplayText();
					logCallback?.Invoke($"[START BLOCKED] {message}", Color.Red);
					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.MonitoringWarning,
						"START BLOCKED",
						message,
						Color.Red);
					if (context == StartContext.Manual)
					{
						MessageBox.Show(
							message,
							"Server Requirements Not Met",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
					}
					return;
				}

				SynixServerPasswords launchPasswords;
				try
				{
					launchPasswords = Core
						.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					logCallback?.Invoke(
						"[🚨 ERROR] Synix could not unlock this server's passwords. Open Server Settings, re-enter them, and save before starting the server.",
						Color.Red);
					return;
				}

				bool isSystemSafe = await Task.Run(() => IsSystemSafeToStart());
				if (!isSystemSafe) return;

				if (!Core.Instance.PassResourceGuard(out string guardMsg))
				{
					logCallback?.Invoke(guardMsg, Color.Orange);
					if (context == StartContext.Manual && !Core.IsBackgroundServiceMode)
						MessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				bool maintenanceBackup = context == StartContext.Scheduled &&
					server.SmartMaintenanceEnabled &&
					server.MaintenanceBackupBeforeRestart;
				if ((server.BackupOnStart || maintenanceBackup) &&
					context != StartContext.CrashRecovery)
				{
					await Task.Run(() => Core.Instance.ExecuteBackup(server, context));
				}

				bool maintenanceUpdate = context == StartContext.Scheduled &&
					server.SmartMaintenanceEnabled &&
					server.MaintenanceUpdateBeforeRestart;
				if (server.UpdateOnStart || maintenanceUpdate)
				{
					await Task.Run(() => Core.Instance.UpdateServerAndReport(server, "UPDATE", true));
				}
				if (OxideRuntimeManager.IsEnabled(server, selectedDefinition) &&
					string.Equals(
						server.ServerFrameworkVersion,
						OxideRuntimeManager.FailedVersion,
						StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke(
						"[OXIDE ERROR] Start blocked because Oxide was not installed successfully. Run Update or Validate to retry.",
						Color.Red);
					return;
				}
				if (OxideRuntimeManager.RequiresVanillaRestore(server, selectedDefinition))
				{
					logCallback?.Invoke(
						"[OXIDE] Start blocked because Rust was changed to Vanilla. Run Update or Validate to restore the official files first.",
						Color.Orange);
					return;
				}

				string launchPublicIp = string.Empty;
				if (selectedDefinition.RequiredArgs.Contains(
					"{PublicIP}",
					StringComparison.Ordinal))
				{
					launchPublicIp = (await Core.Instance.GetPublicIP()).Trim();
					if (string.IsNullOrWhiteSpace(launchPublicIp))
					{
						logCallback?.Invoke(
							"[PUBLIC LISTING] Synix could not detect the current public address. The server will use its own automatic address detection.",
							Color.Orange);
					}
				}

				server.HasAnnouncedOnline = false;
				server.IsProbing = false;
				server.LastProbeTime = null;
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				ProcessStartInfo? psi = null;
				string finalArgs = "";
				bool isMinecraft = false;
				bool hideWindow = selectedDefinition != null &&
					GameLaunchCommandBuilder.ShouldHideServerWindow(
						selectedDefinition,
						Properties.Settings.Default.ShowServerWindow);

				await Task.Run(() =>
				{
					var dbEntry = GameDatabase.GetGame(server.Game);
					if (dbEntry == null)
					{
						logCallback?.Invoke("[🚨 ERROR] Game template not found.", Color.Red);
						return;
					}

					string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(server, dbEntry);
					string binDir = Path.GetDirectoryName(fullExePath) ?? "";

					if (!File.Exists(fullExePath))
					{
						logCallback?.Invoke($"[🚨 ERROR] Executable missing: {fullExePath}", Color.Red);
						_ = Core.Instance.SendDiscordNotification(
							server,
							DiscordNotificationEvent.ConfigurationWarning,
							"SERVER FILE MISSING",
							"The configured server launch file is missing. Run Update or Validate before starting.",
							Color.Red);
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						Core.Instance.UpdateGridStatus();
						return;
					}

					isMinecraft = GameDatabase.IsMinecraft(server.Game);
					if (MinecraftControlProfile.IsJava(server))
					{
						PrepareMinecraftLauncher(fullExePath, logCallback!);
					}

					string invokedId = GameLaunchCommandBuilder.ResolveInvokedAppId(
						server,
						dbEntry,
						fullExePath);

					string cleanIdentity = Core.Instance.GetSafeName(server.ServerName);
					if (!GameLaunchCommandBuilder.TryBuildArguments(
						server,
						dbEntry,
						invokedId,
						launchPasswords,
						launchPublicIp,
						out string args,
						out string argumentError))
					{
						logCallback?.Invoke(
							$"[🚨 SECURITY] {argumentError} Startup was blocked.",
							Color.Red);
						_ = Core.Instance.SendDiscordNotification(
							server,
							DiscordNotificationEvent.SecurityWarning,
							"UNSAFE STARTUP BLOCKED",
							argumentError,
							Color.Red);
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						Core.Instance.UpdateGridStatus();
						return;
					}

					finalArgs = args;
					if (server.Game.Equals("Arma Reforger", StringComparison.OrdinalIgnoreCase))
					{
						string profilePath = Path.Combine(
							server.InstallPath,
							"profiles",
							cleanIdentity);
						Directory.CreateDirectory(profilePath);
						logCallback?.Invoke(
							$"[ARMA REFORGER] Profile and crash logs: {profilePath}",
							Color.Cyan);
					}

					psi = GameLaunchCommandBuilder.CreateProcessStartInfo(
						fullExePath,
						finalArgs,
						binDir,
						dbEntry.LaunchBehavior.RunElevated,
						hideWindow,
						isMinecraft && hideWindow,
						isMinecraft && hideWindow);

					if (!psi.UseShellExecute)
					{
						psi.EnvironmentVariables["SteamAppId"] = invokedId;
						psi.EnvironmentVariables["SteamGameId"] = invokedId;
					}
				});

				if (psi == null) return;

				string safeLogArgs = "[arguments unavailable]";
				if (selectedDefinition != null)
				{
					string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(
						server,
						selectedDefinition);
					string invokedId = GameLaunchCommandBuilder.ResolveInvokedAppId(
						server,
						selectedDefinition,
						fullExePath);
					if (!GameLaunchCommandBuilder.TryBuildArguments(
						server,
						selectedDefinition,
						invokedId,
						GameLaunchCommandBuilder.CreateRedactedPasswords(launchPasswords),
						launchPublicIp,
						out safeLogArgs,
						out _))
					{
						safeLogArgs = "[arguments passed securely; preview unavailable]";
					}
					else if (!string.IsNullOrWhiteSpace(launchPublicIp))
					{
						safeLogArgs = safeLogArgs.Replace(
							launchPublicIp,
							"[PUBLIC IP]",
							StringComparison.Ordinal);
					}
				}

				logCallback?.Invoke($"[ARGUMENT] {safeLogArgs}", Color.Cyan);

				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					if (isMinecraft && psi.RedirectStandardInput)
					{
						proc.StandardInput.AutoFlush = true;
					}
					if (isMinecraft && psi.RedirectStandardOutput)
					{
						MinecraftConsoleHub.Attach(server, proc);
					}

					server.RunningProcess = proc;
					server.PID = proc.Id;
					server.LastProcessDiscoveryUtc = DateTime.MinValue;
					RefreshServerProcessRegistry(server, forceDiscovery: true);
					_ = CaptureSpawnedServerProcesses(server, logCallback!);
					if (hideWindow && !psi.UseShellExecute)
					{
						_ = HideServerWindowAfterLaunch(proc);
					}

					server.StartTime = DateTime.Now;

					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.ServerStarting,
						"SERVER STARTING",
						$"{server.ServerName} process has been initiated.",
						Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) =>
					{
						try
						{
							if (IsStoppingStatus(server.Status))
							{
								return;
							}

							if (ReconcileActiveServerProcesses(server, forceDiscovery: true))
							{
								logCallback?.Invoke(
									$"[PROCESS TRACKING] The launcher exited, but {server.ServerProcesses.Count} verified server process(es) remain active: {FormatProcessRegistry(server.ServerProcesses)}",
									Color.Cyan);
								FileHandler.SaveServers();
								return;
							}

							if (server.Status == StatusManager.GetStatus(ServerState.Running))
							{
								await Core.Instance.ExecuteStartSequence(server, "WATCHDOG");
							}
							else
							{
								FinalizeStoppedState(server);
								await Core.Instance.SynchronizeFirstGeneratedConfiguration(server);
								FileHandler.SaveServers();
							}
						}
						catch (Exception ex)
						{
							logCallback?.Invoke($"[🚨 CRASH HANDLER ERROR] {ex.Message}", Color.Red);
							FinalizeStoppedState(server);
						}
					};
					FileHandler.SaveServers();
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[🚨 CRITICAL ERROR] {ex.Message}", Color.Red);
				_ = Core.Instance.SendDiscordNotification(
					server,
					DiscordNotificationEvent.MonitoringWarning,
					"START FAILED",
					ex.Message,
					Color.Red);
			}
		}

		private static void PrepareMinecraftLauncher(string launcherPath, Action<string, Color> logCallback)
		{
			try
			{
				string original = File.ReadAllText(launcherPath);
				string updated = original
					.Replace(" %* <NUL", " %*", StringComparison.OrdinalIgnoreCase)
					.Replace("if %errorlevel% neq 0 pause", "exit /b %errorlevel%", StringComparison.OrdinalIgnoreCase);

				if (!string.Equals(original, updated, StringComparison.Ordinal))
				{
					File.WriteAllText(launcherPath, updated);
					logCallback?.Invoke("[MINECRAFT] Updated the legacy launcher so clean console shutdown commands are accepted.", Color.Cyan);
				}
			}
			catch (Exception ex)
			{

				logCallback?.Invoke($"[⚠️ MINECRAFT] Could not update Start.bat for graceful shutdown: {ex.Message}", Color.OrangeRed);
			}
		}

		public static async Task<bool> Stop(GameServer server, Action<string, Color> logCallback, bool isManual = true)
		{
			ArgumentNullException.ThrowIfNull(logCallback);
			Dictionary<int, DateTime?> trackedProcesses = [];
			int targetPid = 0;

			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopping);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
				TrackSavedServerProcesses(server, trackedProcesses);

				targetPid = GetInitialTargetPid(server);
				if (targetPid > 0)
				{
					TrackProcessTree(targetPid, trackedProcesses);
				}

				TrackInstallDirectoryProcesses(server, trackedProcesses);
				SynchronizeServerProcessRegistry(server, trackedProcesses);
				List<int> liveProcesses = GetLiveTrackedProcesses(trackedProcesses);

				if (targetPid <= 0 || !liveProcesses.Contains(targetPid))
				{
					targetPid = SelectPrimaryProcess(server, liveProcesses, 0);
					if (targetPid > 0)
					{
						TrackProcessTree(targetPid, trackedProcesses);
						liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
					}
				}

				if (liveProcesses.Count == 0)
				{
					logCallback?.Invoke($"[SHUTDOWN] No live process remains for {server.ServerName}. State reconciled.", Color.Lime);
					FinalizeStoppedState(server);
					return true;
				}

				logCallback?.Invoke(
					$"[SHUTDOWN] Tracking {liveProcesses.Count} process(es) for {server.ServerName}: {FormatProcessRegistry(server.ServerProcesses)}",
					Color.Aqua);

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...", Color.Aqua);

				bool isMinecraft = GameDatabase.IsMinecraft(server.Game);
				bool signalSent = isMinecraft
					? await TrySendMinecraftStopCommand(server, targetPid, logCallback!)
					: targetPid > 0 && await TrySendConsoleShutdownSignal(targetPid, server);
				TimeSpan gracefulTimeout = isMinecraft
					? TimeSpan.FromSeconds(60)
					: TimeSpan.FromSeconds(25);

				if (signalSent)
				{
					liveProcesses = await WaitForServerProcessesToExit(server, targetPid, trackedProcesses, gracefulTimeout);
				}
				else
				{
					RefreshTrackedProcesses(server, targetPid, trackedProcesses);
					liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
				}

				if (liveProcesses.Count == 0)
				{
					logCallback?.Invoke($"[SYNIX] {server.ServerName} saved and closed cleanly.", Color.Lime);
					FinalizeStoppedState(server);
					return true;
				}

				logCallback?.Invoke(
					$"[🛡️ WATCHDOG] {server.ServerName} did not close cleanly. Forcing {liveProcesses.Count} process(es) to stop...",
					Color.Violet);

				await ForceTerminateProcesses(liveProcesses, targetPid, trackedProcesses, logCallback!);
				liveProcesses = await WaitForServerProcessesToExit(server, targetPid, trackedProcesses, TimeSpan.FromSeconds(10));

				if (liveProcesses.Count > 0)
				{
					RestoreLiveServerState(server, liveProcesses, targetPid);
					logCallback?.Invoke(
						$"[🚨 STOP FAILED] {server.ServerName} still has a live process (PID: {string.Join(", ", liveProcesses)}). Status was not changed to Stopped.",
						Color.Red);
					return false;
				}

				FinalizeStoppedState(server);
				logCallback?.Invoke($"[🛡️ WATCHDOG] {server.ServerName} was force-closed and verified stopped.", Color.Violet);
				return true;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[🚨 ERROR] Failed to stop {server.ServerName}: {ex.Message}", Color.Red);

				RefreshTrackedProcesses(server, targetPid, trackedProcesses);
				List<int> liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
				if (liveProcesses.Count == 0)
				{
					FinalizeStoppedState(server);
					return true;
				}

				RestoreLiveServerState(server, liveProcesses, targetPid);
				return false;
			}
		}

		private static Task HideServerWindowAfterLaunch(Process process)
		{
			return Task.Run(async () =>
			{
				DateTime? firstHiddenAt = null;
				for (int attempt = 0; attempt < 60; attempt++)
				{
					try
					{
						if (Properties.Settings.Default.ShowServerWindow || process.HasExited)
							return;

						if (await TryHideServerWindow(process).ConfigureAwait(false))
						{
							firstHiddenAt ??= DateTime.UtcNow;
						}

						if (firstHiddenAt.HasValue &&
							DateTime.UtcNow - firstHiddenAt.Value >= TimeSpan.FromSeconds(3))
						{
							return;
						}
					}
					catch
					{
						return;
					}

					await Task.Delay(250).ConfigureAwait(false);
				}
			});
		}

		private static async Task<bool> TryHideServerWindow(Process process)
		{
			bool hidden = false;
			try
			{
				process.Refresh();
				IntPtr mainWindow = process.MainWindowHandle;
				if (mainWindow != IntPtr.Zero)
				{
					ShowWindowAsync(mainWindow, SW_HIDE);
					hidden = true;
				}
			}
			catch
			{
			}

			await _consoleLock.WaitAsync().ConfigureAwait(false);
			bool attached = false;
			try
			{
				attached = AttachConsole((uint)process.Id);
				if (!attached)
					return hidden;

				IntPtr consoleWindow = GetConsoleWindow();
				if (consoleWindow == IntPtr.Zero)
					return hidden;

				ShowWindowAsync(consoleWindow, SW_HIDE);
				return true;
			}
			finally
			{
				if (attached)
					FreeConsole();
				_consoleLock.Release();
			}
		}

		private static async Task<bool> TrySendMinecraftStopCommand(
			GameServer server,
			int targetPid,
			Action<string, Color> logCallback)
		{
			if (MinecraftControlProfile.IsJava(server))
			{
				MinecraftManagementResult<bool> management =
					await MinecraftManagementClient.StopAsync(server);
				if (management.Succeeded)
				{
					logCallback?.Invoke(
						"[MINECRAFT] Requested a clean stop through Minecraft's local management service.",
						Color.Aqua);
					return true;
				}

				MinecraftRconResult rcon = await MinecraftRconClient.ExecuteCommandAsync(
					server,
					"stop");
				if (rcon.Succeeded)
				{
					logCallback?.Invoke(
						"[MINECRAFT] Sent the native 'stop' command through local Minecraft RCON.",
						Color.Aqua);
					return true;
				}
			}

			if (TryWriteRedirectedInput(server, "stop"))
			{
				logCallback?.Invoke("[MINECRAFT] Sent the native 'stop' command through Synix's managed console pipe.", Color.Aqua);
				return true;
			}

			if (targetPid > 0 && await TryWriteConsoleCommand(targetPid, "stop\r"))
			{
				logCallback?.Invoke("[MINECRAFT] Sent the native 'stop' command to the visible server console.", Color.Aqua);
				return true;
			}

			logCallback?.Invoke(
				"[⚠️ MINECRAFT] The original console input channel is unavailable. Synix will use the verified process-tree fallback.",
				Color.OrangeRed);
			return false;
		}

		private static bool TryWriteRedirectedInput(GameServer server, string command)
		{
			try
			{
				Process? process = server.RunningProcess;
				if (process == null || process.HasExited)
				{
					return false;
				}

				process.StandardInput.WriteLine(command);
				process.StandardInput.Flush();
				return true;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
			catch (IOException)
			{
				return false;
			}
			catch
			{
				return false;
			}
		}

		private static async Task<bool> TryWriteConsoleCommand(int targetPid, string command)
		{
			await _consoleLock.WaitAsync();
			bool attached = false;

			try
			{
				attached = AttachConsole((uint)targetPid);
				if (!attached)
				{
					return false;
				}

				IntPtr inputHandle = GetStdHandle(STD_INPUT_HANDLE);
				if (inputHandle == IntPtr.Zero || inputHandle == InvalidHandleValue)
				{
					return false;
				}

				InputRecord[] inputRecords = CreateConsoleInputRecords(command);
				return inputRecords.Length > 0 &&
					WriteConsoleInput(inputHandle, inputRecords, (uint)inputRecords.Length, out uint written) &&
					written == (uint)inputRecords.Length;
			}
			catch
			{

				return false;
			}
			finally
			{
				if (attached)
				{
					FreeConsole();
				}

				_consoleLock.Release();
			}
		}

		private static InputRecord[] CreateConsoleInputRecords(string command)
		{
			List<InputRecord> records = new List<InputRecord>(command.Length * 2);
			foreach (char character in command)
			{
				records.Add(CreateConsoleInputRecord(character, true));
				records.Add(CreateConsoleInputRecord(character, false));
			}

			return records.ToArray();
		}

		private static InputRecord CreateConsoleInputRecord(char character, bool keyDown)
		{
			ushort virtualKey = character == '\r'
				? (ushort)Keys.Enter
				: (ushort)char.ToUpperInvariant(character);

			return new InputRecord
			{
				EventType = KEY_EVENT,
				KeyEvent = new KeyEventRecord
				{
					KeyDown = keyDown ? 1 : 0,
					RepeatCount = 1,
					VirtualKeyCode = virtualKey,
					VirtualScanCode = 0,
					UnicodeChar = character,
					ControlKeyState = 0
				}
			};
		}

		private static bool IsStoppingStatus(string? status)
		{
			return status?.StartsWith(
				StatusManager.GetStatus(ServerState.Stopping),
				StringComparison.OrdinalIgnoreCase) == true;
		}

		private static int GetInitialTargetPid(GameServer server)
		{
			try
			{
				if (server.RunningProcess != null && !server.RunningProcess.HasExited)
				{
					return server.RunningProcess.Id;
				}
			}
			catch
			{

			}

			int savedPid = server.PID.GetValueOrDefault();
			return savedPid > 0 && IsExpectedServerProcess(server, savedPid) ? savedPid : 0;
		}

		private static bool IsExpectedServerProcess(GameServer server, int processId)
		{
			if (processId <= 0 || processId == Environment.ProcessId)
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				if (process.HasExited)
				{
					return false;
				}

				GameInfo? game = GameDatabase.GetGame(server.Game);
				string configuredExe = game?.ExeName ?? string.Empty;
				string expectedName = Path.GetFileNameWithoutExtension(configuredExe);
				bool launchesScript = configuredExe.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
					configuredExe.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);

				if ((!string.IsNullOrWhiteSpace(expectedName) &&
					 process.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase)) ||
					(launchesScript && process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}

				string? imagePath = TryGetProcessImagePath(process);
				return imagePath != null && IsPathInsideDirectory(imagePath, server.InstallPath);
			}
			catch
			{
				return false;
			}
		}

		private static async Task<bool> TrySendConsoleShutdownSignal(int targetPid, GameServer server)
		{
			await _consoleLock.WaitAsync();
			bool attached = false;
			bool ignoreHandlerInstalled = false;

			try
			{
				attached = AttachConsole((uint)targetPid);
				if (!attached)
				{
					return false;
				}

				ignoreHandlerInstalled = SetConsoleCtrlHandler(null, true);
				bool signalSent = GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);

				TryWriteRedirectedInput(server, "Y");

				await Task.Delay(200);
				return signalSent;
			}
			finally
			{
				if (attached)
				{
					FreeConsole();
				}

				if (ignoreHandlerInstalled)
				{
					SetConsoleCtrlHandler(null, false);
				}

				_consoleLock.Release();
			}
		}

		private static async Task<List<int>> WaitForServerProcessesToExit(
			GameServer server,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses,
			TimeSpan timeout)
		{
			return await WaitForStableProcessExit(
				() =>
				{
					RefreshTrackedProcesses(server, targetPid, trackedProcesses);
					SynchronizeServerProcessRegistry(server, trackedProcesses);
					return GetLiveTrackedProcesses(trackedProcesses);
				},
				timeout,
				TimeSpan.FromSeconds(3),
				TimeSpan.FromMilliseconds(500));
		}

		internal static async Task<(bool Succeeded, string Message)> SendMinecraftCommandAsync(
			GameServer server,
			string command)
		{
			ArgumentNullException.ThrowIfNull(server);
			string normalized = command?.Trim() ?? string.Empty;
			if (!GameDatabase.IsMinecraft(server.Game))
				return (false, "This console is available only for Minecraft servers.");
			if (normalized.Length == 0)
				return (false, "Enter a Minecraft server command.");
			if (normalized.Length > 512 || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
				return (false, "The command is too long or contains an unsafe line break.");
			if (normalized.Equals("stop", StringComparison.OrdinalIgnoreCase))
			{
				bool stopped = await Stop(
					server,
					(message, color) => Core.Instance.Log(message, color));
				return stopped
					? (true, "Minecraft saved and stopped through Synix's verified shutdown workflow.")
					: (false, "Minecraft did not stop cleanly. Check Activity & Diagnostics for details.");
			}

			if (TryWriteRedirectedInput(server, normalized))
			{
				MinecraftConsoleHub.Publish(server, $"> {normalized}", false);
				return (true, "Command sent through Synix's managed server console.");
			}

			if (MinecraftControlProfile.IsJava(server))
			{
				MinecraftRconResult rcon = await MinecraftRconClient.ExecuteCommandAsync(
					server,
					normalized);
				if (rcon.Succeeded)
				{
					MinecraftConsoleHub.Publish(server, $"> {normalized}", false);
					if (!string.IsNullOrWhiteSpace(rcon.Response))
						MinecraftConsoleHub.Publish(server, rcon.Response, false);
					return (true, "Command sent through local Minecraft RCON.");
				}
			}

			int targetPid = GetInitialTargetPid(server);
			if (targetPid > 0 && await TryWriteConsoleCommand(targetPid, normalized + "\r"))
			{
				MinecraftConsoleHub.Publish(server, $"> {normalized}", false);
				return (true, "Command sent to the visible Minecraft console.");
			}

			return (
				false,
				"The Minecraft command channel is unavailable. Start this server from Synix with hidden server windows enabled, or enable local RCON for Java Edition.");
		}

		internal static async Task<List<int>> WaitForStableProcessExit(
			Func<List<int>> getLiveProcesses,
			TimeSpan timeout,
			TimeSpan quietPeriod,
			TimeSpan pollInterval)
		{
			const int minimumConsecutiveEmptySamples = 3;
			ArgumentNullException.ThrowIfNull(getLiveProcesses);
			if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
			if (quietPeriod < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(quietPeriod));
			if (pollInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollInterval));

			DateTime deadline = DateTime.UtcNow.Add(timeout);
			DateTime? quietSince = null;
			int consecutiveEmptySamples = 0;

			while (true)
			{
				List<int> liveProcesses = getLiveProcesses();
				DateTime now = DateTime.UtcNow;

				if (liveProcesses.Count > 0)
				{
					quietSince = null;
					consecutiveEmptySamples = 0;
					if (now >= deadline)
					{
						return liveProcesses;
					}
				}
				else
				{
					quietSince ??= now;
					consecutiveEmptySamples++;
					if (consecutiveEmptySamples >= minimumConsecutiveEmptySamples &&
						now - quietSince.Value >= quietPeriod)
					{
						return liveProcesses;
					}
				}

				await Task.Delay(pollInterval);
			}
		}

		private static void RefreshTrackedProcesses(
			GameServer server,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			if (targetPid > 0 && IsTrackedProcessAlive(targetPid, trackedProcesses))
			{
				TrackProcessTree(targetPid, trackedProcesses);
			}

			TrackInstallDirectoryProcesses(server, trackedProcesses);
		}

		internal static IReadOnlyList<ServerProcessIdentity> RefreshServerProcessRegistry(
			GameServer server,
			bool forceDiscovery = false)
		{
			ArgumentNullException.ThrowIfNull(server);
			Dictionary<int, DateTime?> trackedProcesses = [];
			TrackSavedServerProcesses(server, trackedProcesses);

			int targetPid = GetInitialTargetPid(server);
			if (targetPid > 0)
			{
				TrackProcessTree(targetPid, trackedProcesses);
			}

			DateTime now = DateTime.UtcNow;
			if (forceDiscovery ||
				trackedProcesses.Count == 0 ||
				now - server.LastProcessDiscoveryUtc >= _processDiscoveryInterval)
			{
				TrackInstallDirectoryProcesses(server, trackedProcesses);
				server.LastProcessDiscoveryUtc = now;
			}

			SynchronizeServerProcessRegistry(server, trackedProcesses);
			lock (_serverProcessRegistryLock)
			{
				return server.ServerProcesses.ToArray();
			}
		}

		internal static bool ReconcileActiveServerProcesses(
			GameServer server,
			bool forceDiscovery = false)
		{
			IReadOnlyList<ServerProcessIdentity> processes =
				RefreshServerProcessRegistry(server, forceDiscovery);
			if (processes.Count == 0)
			{
				return false;
			}

			int primaryPid = SelectPrimaryProcess(
				server,
				processes.Select(process => process.ProcessId).ToArray(),
				server.PID.GetValueOrDefault());
			if (primaryPid <= 0)
			{
				return false;
			}

			try
			{
				bool alreadyBound = server.RunningProcess != null &&
					!server.RunningProcess.HasExited &&
					server.RunningProcess.Id == primaryPid;
				if (!alreadyBound)
				{
					Process replacement = Process.GetProcessById(primaryPid);
					server.RunningProcess?.Dispose();
					server.RunningProcess = replacement;
				}

				server.PID = primaryPid;
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static async Task CaptureSpawnedServerProcesses(
			GameServer server,
			Action<string, Color> logCallback)
		{
			int previousCount = 0;
			for (int attempt = 0; attempt < 10; attempt++)
			{
				await Task.Delay(1000).ConfigureAwait(false);
				if (IsStoppingStatus(server.Status) ||
					server.Status == StatusManager.GetStatus(ServerState.Stopped))
				{
					return;
				}

				IReadOnlyList<ServerProcessIdentity> processes =
					RefreshServerProcessRegistry(server, forceDiscovery: true);
				if (processes.Count > previousCount)
				{
					previousCount = processes.Count;
					logCallback?.Invoke(
						$"[PROCESS TRACKING] Registered {processes.Count} server process(es): {FormatProcessRegistry(processes)}",
						Color.Cyan);
				}
			}

			FileHandler.SaveServers();
		}

		private static void TrackSavedServerProcesses(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			ServerProcessIdentity[] savedProcesses;
			lock (_serverProcessRegistryLock)
			{
				savedProcesses = (server.ServerProcesses ?? []).ToArray();
			}

			foreach (ServerProcessIdentity identity in savedProcesses)
			{
				if (IsSavedServerProcessAlive(server, identity))
				{
					trackedProcesses[identity.ProcessId] = identity.StartTimeUtc;
				}
			}
		}

		private static bool IsSavedServerProcessAlive(
			GameServer server,
			ServerProcessIdentity identity)
		{
			if (identity.ProcessId <= 0 ||
				identity.ProcessId == Environment.ProcessId ||
				string.IsNullOrWhiteSpace(identity.ExecutablePath) ||
				!IsPathInsideDirectory(identity.ExecutablePath, server.InstallPath))
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(identity.ProcessId);
				if (process.HasExited)
				{
					return false;
				}

				if (identity.StartTimeUtc.HasValue &&
					process.StartTime.ToUniversalTime() != identity.StartTimeUtc.Value)
				{
					return false;
				}

				string? actualPath = TryGetProcessImagePath(process);
				if (!string.IsNullOrWhiteSpace(actualPath))
				{
					return string.Equals(
						Path.GetFullPath(actualPath),
						Path.GetFullPath(identity.ExecutablePath),
						StringComparison.OrdinalIgnoreCase);
				}

				return process.ProcessName.Equals(
					Path.GetFileNameWithoutExtension(identity.ExecutablePath),
					StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static void SynchronizeServerProcessRegistry(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			HashSet<int> verifiedLaunchProcessTree = GetVerifiedLaunchProcessTreeIds(server);
			Dictionary<int, ServerProcessIdentity> existing;
			lock (_serverProcessRegistryLock)
			{
				existing = (server.ServerProcesses ?? [])
					.Where(process => process.ProcessId > 0)
					.GroupBy(process => process.ProcessId)
					.ToDictionary(group => group.Key, group => group.First());
			}

			List<ServerProcessIdentity> liveIdentities = [];
			foreach (int processId in GetLiveTrackedProcesses(trackedProcesses))
			{
				try
				{
					using Process process = Process.GetProcessById(processId);
					string? executablePath = TryGetProcessImagePath(process);
					if (string.IsNullOrWhiteSpace(executablePath) &&
						existing.TryGetValue(processId, out ServerProcessIdentity? savedIdentity))
					{
						executablePath = savedIdentity.ExecutablePath;
					}

					bool isInstalledServerExecutable = !string.IsNullOrWhiteSpace(executablePath) &&
						IsPathInsideDirectory(executablePath, server.InstallPath);
					bool isVerifiedLaunchProcess = verifiedLaunchProcessTree.Contains(processId);
					if (string.IsNullOrWhiteSpace(executablePath) ||
						(!isInstalledServerExecutable && !isVerifiedLaunchProcess))
					{
						continue;
					}

					DateTime? startTimeUtc = null;
					try
					{
						startTimeUtc = process.StartTime.ToUniversalTime();
					}
					catch
					{
						if (existing.TryGetValue(processId, out ServerProcessIdentity? recoveredIdentity))
						{
							startTimeUtc = recoveredIdentity.StartTimeUtc;
						}
					}

					liveIdentities.Add(new ServerProcessIdentity
					{
						ProcessId = processId,
						ExecutablePath = Path.GetFullPath(executablePath),
						StartTimeUtc = startTimeUtc
					});
				}
				catch
				{
				}
			}

			lock (_serverProcessRegistryLock)
			{
				server.ServerProcesses = liveIdentities
					.OrderBy(process => process.ProcessId)
					.ToList();
			}
		}

		private static HashSet<int> GetVerifiedLaunchProcessTreeIds(GameServer server)
		{
			try
			{
				GameInfo? game = GameDatabase.GetGame(server.Game);
				if (game == null)
				{
					return [];
				}

				string launchPath = GameLaunchCommandBuilder.ResolveExecutablePath(server, game);
				string extension = Path.GetExtension(launchPath);
				if (!extension.Equals(".bat", StringComparison.OrdinalIgnoreCase) &&
					!extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase))
				{
					return [];
				}

				Process? launchProcess = server.RunningProcess;
				if (launchProcess == null ||
					launchProcess.HasExited ||
					server.PID.GetValueOrDefault() != launchProcess.Id)
				{
					return [];
				}

				return GetProcessTreeIds(launchProcess.Id);
			}
			catch
			{
				return [];
			}
		}

		private static string FormatProcessRegistry(
			IEnumerable<ServerProcessIdentity> processes)
		{
			string result = string.Join(
				", ",
				processes.Select(process =>
					$"{Path.GetFileName(process.ExecutablePath)} (PID {process.ProcessId})"));
			return string.IsNullOrWhiteSpace(result) ? "none" : result;
		}

		private static void TrackProcessTree(int rootPid, Dictionary<int, DateTime?> trackedProcesses)
		{
			foreach (int processId in GetProcessTreeIds(rootPid))
			{
				TrackProcess(processId, trackedProcesses);
			}
		}

		private static HashSet<int> GetProcessTreeIds(int rootPid)
		{
			HashSet<int> processTree = [];
			if (rootPid <= 0 || rootPid == Environment.ProcessId)
			{
				return processTree;
			}

			processTree.Add(rootPid);
			IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
			if (snapshot == InvalidHandleValue)
			{
				return processTree;
			}

			try
			{
				List<(int ProcessId, int ParentProcessId)> allProcesses = [];
				ProcessEntry32 entry = new ProcessEntry32
				{
					Size = (uint)Marshal.SizeOf<ProcessEntry32>()
				};

				if (Process32First(snapshot, ref entry))
				{
					do
					{
						allProcesses.Add(((int)entry.ProcessId, (int)entry.ParentProcessId));
						entry.Size = (uint)Marshal.SizeOf<ProcessEntry32>();
					}
					while (Process32Next(snapshot, ref entry));
				}

				Queue<int> pendingParents = new Queue<int>();
				pendingParents.Enqueue(rootPid);
				while (pendingParents.Count > 0)
				{
					int parentId = pendingParents.Dequeue();
					foreach ((int processId, int parentProcessId) in allProcesses)
					{
						if (parentProcessId == parentId && processTree.Add(processId))
						{
							pendingParents.Enqueue(processId);
						}
					}
				}
			}
			finally
			{
				CloseHandle(snapshot);
			}

			return processTree;
		}

		private static void TrackInstallDirectoryProcesses(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				return;
			}

			Process[] processes = Process.GetProcesses();
			try
			{
				foreach (Process process in processes)
				{
					try
					{
						if (process.Id == Environment.ProcessId || process.HasExited)
						{
							continue;
						}

						string? imagePath = TryGetProcessImagePath(process);
						if (imagePath != null && IsPathInsideDirectory(imagePath, server.InstallPath))
						{
							TrackProcess(process, trackedProcesses);
						}
					}
					catch
					{

					}
				}
			}
			finally
			{
				foreach (Process process in processes)
				{
					process.Dispose();
				}
			}
		}

		private static string? TryGetProcessImagePath(Process process)
		{
			try
			{
				return process.MainModule?.FileName;
			}
			catch
			{
				return null;
			}
		}

		private static bool IsPathInsideDirectory(string filePath, string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directoryPath))
			{
				return false;
			}

			try
			{
				string normalizedDirectory = Path.GetFullPath(directoryPath)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
				string normalizedFile = Path.GetFullPath(filePath);
				return normalizedFile.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static void TrackProcess(int processId, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (processId <= 0 || processId == Environment.ProcessId || trackedProcesses.ContainsKey(processId))
			{
				return;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				TrackProcess(process, trackedProcesses);
			}
			catch
			{

			}
		}

		private static void TrackProcess(Process process, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (process.Id <= 0 || process.Id == Environment.ProcessId || trackedProcesses.ContainsKey(process.Id) || process.HasExited)
			{
				return;
			}

			DateTime? startTime = null;
			try
			{
				startTime = process.StartTime.ToUniversalTime();
			}
			catch
			{

			}

			trackedProcesses[process.Id] = startTime;
		}

		private static bool IsTrackedProcessAlive(int processId, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (!trackedProcesses.TryGetValue(processId, out DateTime? expectedStartTime))
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				if (process.HasExited)
				{
					return false;
				}

				if (expectedStartTime.HasValue)
				{
					try
					{
						return process.StartTime.ToUniversalTime() == expectedStartTime.Value;
					}
					catch
					{
						return false;
					}
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		private static List<int> GetLiveTrackedProcesses(Dictionary<int, DateTime?> trackedProcesses)
		{
			List<int> liveProcesses = [];
			foreach (int processId in trackedProcesses.Keys.ToArray())
			{
				if (IsTrackedProcessAlive(processId, trackedProcesses))
				{
					liveProcesses.Add(processId);
				}
				else
				{
					trackedProcesses.Remove(processId);
				}
			}

			return liveProcesses;
		}

		private static async Task ForceTerminateProcesses(
			List<int> liveProcesses,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses,
			Action<string, Color> logCallback)
		{
			IEnumerable<int> orderedProcesses = liveProcesses
				.OrderBy(processId => processId == targetPid ? 0 : 1)
				.ThenBy(processId => processId);

			foreach (int processId in orderedProcesses)
			{
				if (!IsTrackedProcessAlive(processId, trackedProcesses))
				{
					continue;
				}

				try
				{
					using Process process = Process.GetProcessById(processId);
					process.Kill(entireProcessTree: true);
				}
				catch (Exception ex)
				{
					logCallback?.Invoke($"[⚠️ STOP] Direct process-tree kill failed for PID {processId}: {ex.Message}", Color.OrangeRed);
				}
			}

			await Task.Delay(300);

			foreach (int processId in GetLiveTrackedProcesses(trackedProcesses))
			{
				ProcessStartInfo killInfo = new ProcessStartInfo
				{
					FileName = "taskkill.exe",
					CreateNoWindow = true,
					UseShellExecute = false
				};
				killInfo.ArgumentList.Add("/F");
				killInfo.ArgumentList.Add("/T");
				killInfo.ArgumentList.Add("/PID");
				killInfo.ArgumentList.Add(processId.ToString());

				try
				{
					using Process? killProcess = Process.Start(killInfo);
					if (killProcess != null)
					{
						await killProcess.WaitForExitAsync();
						if (killProcess.ExitCode != 0 && IsTrackedProcessAlive(processId, trackedProcesses))
						{
							logCallback?.Invoke($"[⚠️ STOP] taskkill returned exit code {killProcess.ExitCode} for PID {processId}.", Color.OrangeRed);
						}
					}
				}
				catch (Exception ex)
				{
					logCallback?.Invoke($"[⚠️ STOP] taskkill failed for PID {processId}: {ex.Message}", Color.OrangeRed);
				}
			}
		}

		private static int SelectPrimaryProcess(GameServer server, IReadOnlyCollection<int> liveProcesses, int preferredPid)
		{
			if (preferredPid > 0 && liveProcesses.Contains(preferredPid))
			{
				return preferredPid;
			}

			GameInfo? game = GameDatabase.GetGame(server.Game);
			string configuredExe = game?.ExeName ?? string.Empty;
			string expectedName = Path.GetFileNameWithoutExtension(configuredExe);
			bool launchesScript = configuredExe.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
				configuredExe.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);

			foreach (int processId in liveProcesses)
			{
				try
				{
					using Process process = Process.GetProcessById(processId);
					if ((!string.IsNullOrWhiteSpace(expectedName) &&
						 process.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase)) ||
						(launchesScript && process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase)))
					{
						return processId;
					}
				}
				catch
				{

				}
			}

			return liveProcesses.FirstOrDefault();
		}

		private static void RestoreLiveServerState(GameServer server, IReadOnlyCollection<int> liveProcesses, int preferredPid)
		{
			int survivingPid = SelectPrimaryProcess(server, liveProcesses, preferredPid);
			if (survivingPid <= 0)
			{
				return;
			}

			Process? survivingProcess = null;
			try
			{
				survivingProcess = Process.GetProcessById(survivingPid);
				if (survivingProcess.HasExited)
				{
					survivingProcess.Dispose();
					survivingProcess = null;
					return;
				}

				bool alreadyBound = false;
				try
				{
					alreadyBound = server.RunningProcess != null && server.RunningProcess.Id == survivingPid;
				}
				catch
				{

				}

				if (!alreadyBound)
				{
					server.RunningProcess?.Dispose();
					server.RunningProcess = survivingProcess;
					survivingProcess = null;
				}
			}
			catch
			{

			}
			finally
			{
				survivingProcess?.Dispose();
			}

			server.PID = survivingPid;
			server.Status = StatusManager.GetStatus(ServerState.Running);
			MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
		}

		private static void FinalizeStoppedState(GameServer server)
		{
			MinecraftConsoleHub.NotifyStopped(server);
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			lock (_serverProcessRegistryLock)
			{
				server.ServerProcesses = [];
			}
			server.LastProcessDiscoveryUtc = DateTime.MinValue;
			server.HasAnnouncedOnline = false;
			server.IsProbing = false;
			server.LastProbeTime = null;
			server.RunningProcess?.Dispose();
			server.RunningProcess = null;
			MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
		}
	}
}
