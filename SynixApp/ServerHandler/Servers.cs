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
		static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);
		delegate bool ConsoleCtrlDelegate(uint CtrlType);

		const uint CTRL_C_EVENT = 0;
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

		public static async Task Start(GameServer server, Action<string, Color> logCallback, StartContext context = StartContext.Manual)
		{
			try
			{
				bool isSystemSafe = await Task.Run(() => IsSystemSafeToStart());
				if (!isSystemSafe) return;

				if (!Core.Instance.PassResourceGuard(out string guardMsg))
				{
					logCallback?.Invoke(guardMsg, Color.Orange);
					MessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (server.BackupOnStart && context != StartContext.CrashRecovery)
				{
					await Task.Run(() => Core.Instance.ExecuteBackup(server, context));
				}

				if (server.UpdateOnStart)
				{
					await Task.Run(() => Core.Instance.UpdateServerAndReport(server, "UPDATE", true));
				}

				// Probe coordination is runtime-only. A prior shutdown or serialized
				// value must never leave a newly launched server stuck at Starting.
				server.HasAnnouncedOnline = false;
				server.IsProbing = false;
				server.LastProbeTime = null;
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				ProcessStartInfo? psi = null;
				string finalArgs = "";
				bool isMinecraft = false;

				await Task.Run(() =>
				{
					var dbEntry = GameDatabase.GetGame(server.Game);
					if (dbEntry == null)
					{
						logCallback?.Invoke("[🚨 ERROR] Game template not found.", Color.Red);
						return;
					}

					string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);
					string binDir = Path.GetDirectoryName(fullExePath) ?? "";

					if (!File.Exists(fullExePath))
					{
						logCallback?.Invoke($"[🚨 ERROR] Executable missing: {fullExePath}", Color.Red);
						MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
						return;
					}

					isMinecraft = server.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
					if (isMinecraft)
					{
						PrepareMinecraftLauncher(fullExePath, logCallback);
					}

					string targetId = dbEntry.AppID;
					string invokedId = targetId;

					string rootAppIdPath = Path.Combine(server.InstallPath, "steam_appid.txt");
					string binAppIdPath = Path.Combine(binDir, "steam_appid.txt");
					string appidPath = rootAppIdPath;

					if (File.Exists(rootAppIdPath))
					{
						appidPath = rootAppIdPath;
					}
					else if (File.Exists(binAppIdPath))
					{
						appidPath = binAppIdPath;
					}
					else
					{
						try
						{
							var scanner = Directory.EnumerateFiles(server.InstallPath, "steam_appid.txt", new EnumerationOptions
							{
								RecurseSubdirectories = true,
								IgnoreInaccessible = true,
								MaxRecursionDepth = 15,
								AttributesToSkip = FileAttributes.ReparsePoint
							});

							appidPath = scanner.FirstOrDefault() ?? rootAppIdPath;
						}
						catch
						{
							appidPath = rootAppIdPath;
						}
					}

					if (File.Exists(appidPath))
					{
						try
						{
							string fileContent = File.ReadAllText(appidPath).Trim();

							fileContent = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "";

							if (!string.IsNullOrWhiteSpace(fileContent))
							{
								invokedId = fileContent;
							}
						}
						catch (Exception ex) { logCallback?.Invoke($"[⚠️ WARNING] File Read Error: {ex.Message}", Color.OrangeRed); }
					}

					// Calculate RAM for the launch argument WITHOUT overwriting the saved variable
					int ramToUse = server.MaxRam;
					if (isMinecraft)
					{
						ramToUse = server.MaxRam * 1024;
					}

					string cleanIdentity = Core.Instance.GetSafeName(server.ServerName);

					string args = dbEntry.RequiredArgs
						.Replace("{app_port}", server.AppPort?.ToString() ?? "0")
						.Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
						.Replace("{map}", server.WorldName)
						.Replace("{steamAppID}", invokedId)
						.Replace("{appid}", targetId)
						.Replace("{port}", server.Port.ToString())
						.Replace("{query}", server.QueryPort.ToString())
						.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
						.Replace("{pass}", server.Password ?? "")
						.Replace("{adminpass}", server.AdminPassword ?? "")
						.Replace("{ServerName}", server.ServerName)
						.Replace("{InstallPath}", server.InstallPath)
						.Replace("{world_size}", server.WorldSize.ToString())
						.Replace("{Identity}", cleanIdentity)
						.Replace("{ram}", ramToUse.ToString());

					// Modern Forge uses its generated win_args.txt instead of
					// "-jar server.jar". Start.bat supplies the Forge argument file,
					// while Synix still owns the selected heap size and optional args.
					if (isMinecraft &&
						MinecraftMetadataService.NormalizeLoader(server.MinecraftLoader)
							.Equals(MinecraftMetadataService.ForgeLoader, StringComparison.OrdinalIgnoreCase))
					{
						args = $"-Xmx{ramToUse}M -Xms{ramToUse}M";
					}

					if (args.Contains("{rcon}"))
					{
						string formattedRcon = "";

						if (server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax))
						{
							formattedRcon = dbEntry.RconSyntax
								.Replace("{rcon_port}", server.RconPort.ToString())
								.Replace("{rcon_pass}", server.RconPassword ?? "");

							if (string.Equals(server.Game, "Rust", StringComparison.OrdinalIgnoreCase))
							{
								formattedRcon += " +rcon.web 1";
							}
						}

						args = args.Replace("{rcon}", formattedRcon);
					}

					if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
					{
						bool usesBooleanMode =
							server.Game.Equals("ARK: Survival Evolved", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("ARK: Survival Ascended", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("PixARK", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("Atlas", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("Rust", StringComparison.OrdinalIgnoreCase);

						string translatedMode = server.GameMode;

						if (usesBooleanMode)
						{
							if (server.GameMode.Equals("PVE", StringComparison.OrdinalIgnoreCase))
								translatedMode = "True";
							else if (server.GameMode.Equals("PVP", StringComparison.OrdinalIgnoreCase))
								translatedMode = "False";
						}

						args = args.Replace("{mode}", translatedMode);
					}

					if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
					{
						if (!IsGameServerConfigSafe(server.ExtraArgs))
						{
							logCallback?.Invoke("[🚨 SECURITY] Illegal characters detected in the extra arguments. Aborting startup.", Color.Red);
							MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
							return;
						}

						args = $"{args} \"{server.ExtraArgs.Trim()}\"";
					}

					args = args.Replace("  ", " ").Trim();
					/*
					if (!IsStringSafe(args))
					{
						logCallback?.Invoke("[🚨 SECURITY] Illegal characters detected. Aborting startup.", Color.Red);
						MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
						return;
					}*/

					finalArgs = args;
					bool hideWindow = !Properties.Settings.Default.ShowServerWindow;

					psi = new ProcessStartInfo
					{
						FileName = fullExePath,
						Arguments = finalArgs,
						WorkingDirectory = binDir,
						UseShellExecute = false,
						CreateNoWindow = hideWindow,
						// Hidden Minecraft servers need a pipe so Synix can send the
						// native "stop" command. Visible servers keep their console input
						// so administrators can still type commands in the server window.
						RedirectStandardInput = isMinecraft && hideWindow,
					};

					if (server.Game == "Dune: Awakening")
					{
						psi.UseShellExecute = true;
						psi.Verb = "runas";
					}
					else
					{
						psi.EnvironmentVariables["SteamAppId"] = invokedId;
						psi.EnvironmentVariables["SteamGameId"] = invokedId;
					}
				});

				if (psi == null) return;

				// Scrub passwords from the UI log
				string safeLogArgs = finalArgs;
				if (!string.IsNullOrWhiteSpace(server.Password)) safeLogArgs = safeLogArgs.Replace(server.Password, "********");
				if (!string.IsNullOrWhiteSpace(server.AdminPassword)) safeLogArgs = safeLogArgs.Replace(server.AdminPassword, "********");
				if (!string.IsNullOrWhiteSpace(server.RconPassword)) safeLogArgs = safeLogArgs.Replace(server.RconPassword, "********");

				logCallback?.Invoke($"[ARGUMENT] {safeLogArgs}", Color.Cyan);

				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					if (isMinecraft && psi.RedirectStandardInput)
					{
						proc.StandardInput.AutoFlush = true;
					}

					server.RunningProcess = proc;
					server.PID = proc.Id;

					server.StartTime = DateTime.Now;

					_ = Core.Instance.SendDiscordAlert(server, "SERVER STARTING", $"{server.ServerName} process has been initiated.", Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) =>
					{
						try
						{
							if (IsStoppingStatus(server.Status))
							{
								return;
							}

							if (server.Status == StatusManager.GetStatus(ServerState.Running))
							{
								await Core.Instance.ExecuteStartSequence(server, "WATCHDOG");
							}
							else
							{
								FinalizeStoppedState(server);
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
			catch (Exception ex) { logCallback?.Invoke($"[🚨 CRITICAL ERROR] {ex.Message}", Color.Red); }
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
				// Starting remains possible, but Synix will have to use its verified
				// process-tree fallback if this legacy launcher cannot be migrated.
				logCallback?.Invoke($"[⚠️ MINECRAFT] Could not update Start.bat for graceful shutdown: {ex.Message}", Color.OrangeRed);
			}
		}

		public static async Task<bool> Stop(GameServer server, Action<string, Color> logCallback, bool isManual = true)
		{
			Dictionary<int, DateTime?> trackedProcesses = [];
			int targetPid = 0;

			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopping);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				targetPid = GetInitialTargetPid(server);
				if (targetPid > 0)
				{
					TrackProcessTree(targetPid, trackedProcesses);
				}

				TrackInstallDirectoryProcesses(server, trackedProcesses);
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

				if (isManual)
				{
					_ = Core.Instance.SendDiscordAlert(server, "MANUAL SHUTDOWN",
						"A shutdown command was issued via the Synix Control Panel.", Color.Orange);
				}

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...", Color.Aqua);

				bool isMinecraft = server.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
				bool signalSent = isMinecraft
					? await TrySendMinecraftStopCommand(server, targetPid, logCallback)
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

				await ForceTerminateProcesses(liveProcesses, targetPid, trackedProcesses, logCallback);
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

		private static async Task<bool> TrySendMinecraftStopCommand(
			GameServer server,
			int targetPid,
			Action<string, Color> logCallback)
		{
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

				// StandardInput itself is the reliable test. Reading StartInfo on a
				// process restored with Process.GetProcessById throws because Synix did
				// not create that Process object.
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
				// Console ownership can disappear while Windows Terminal or cmd.exe is
				// closing. The caller will use the verified process-tree fallback.
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
				// The Process object can be disposed between checks. Fall back to the saved PID.
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

				// Give Windows time to dispatch the control event before detaching.
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
			DateTime deadline = DateTime.UtcNow.Add(timeout);

			while (true)
			{
				RefreshTrackedProcesses(server, targetPid, trackedProcesses);
				List<int> liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
				if (liveProcesses.Count == 0 || DateTime.UtcNow >= deadline)
				{
					return liveProcesses;
				}

				await Task.Delay(500);
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
						// Access can disappear while the process list is being inspected.
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
				// The process exited between the snapshot and this lookup.
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
				// PID verification will still work if Windows denies StartTime access.
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
					// Continue looking for another verified live process.
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
					// A disposed process object will be replaced below.
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
				// Keep the verified PID even if Windows denies reopening the Process object.
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
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			server.RunningProcess?.Dispose();
			server.RunningProcess = null;
			MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
		}
	}
}
