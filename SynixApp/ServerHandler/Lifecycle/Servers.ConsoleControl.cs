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
	public static partial class Servers
	{
		#region Win32 API for Console Control
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

		#endregion

		private static readonly SemaphoreSlim _consoleLock = new SemaphoreSlim(1, 1);

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
						LocalizationManager.Get("Minecraft.Activity.StopThroughManagement"),
						Color.Aqua);
					return true;
				}

				MinecraftRconResult rcon = await MinecraftRconClient.ExecuteCommandAsync(
					server,
					"stop");
				if (rcon.Succeeded)
				{
					logCallback?.Invoke(
						LocalizationManager.Get("Minecraft.Activity.StopThroughRcon"),
						Color.Aqua);
					return true;
				}
			}

			if (TryWriteRedirectedInput(server, "stop"))
			{
				logCallback?.Invoke(LocalizationManager.Get("Minecraft.Activity.StopThroughPipe"), Color.Aqua);
				return true;
			}

			if (targetPid > 0 && await TryWriteConsoleCommand(targetPid, "stop\r"))
			{
				logCallback?.Invoke(LocalizationManager.Get("Minecraft.Activity.StopThroughConsole"), Color.Aqua);
				return true;
			}

			logCallback?.Invoke(
				LocalizationManager.Get("Minecraft.Activity.ConsoleFallback"),
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
			catch (ObjectDisposedException ex)
			{
				ApplicationLogService.WriteSuppressedException(ex);
				return false;
			}
			catch (InvalidOperationException ex)
			{
				ApplicationLogService.WriteSuppressedException(ex);
				return false;
			}
			catch (IOException ex)
			{
				ApplicationLogService.WriteSuppressedException(ex);
				return false;
			}
			catch (Exception ex)
			{
				ApplicationLogService.WriteSuppressedException(ex);
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
			catch (Exception ex)
			{
				ApplicationLogService.WriteSuppressedException(ex);
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

		internal static async Task<(bool Succeeded, string Message)> SendMinecraftCommandAsync(
			GameServer server,
			string command)
		{
			ArgumentNullException.ThrowIfNull(server);
			string normalized = command?.Trim() ?? string.Empty;
			if (!GameCapabilityResolver.UsesMinecraftConsole(server))
				return (false, LocalizationManager.Get("Minecraft.Command.ConsoleOnly"));
			if (normalized.Length == 0)
				return (false, LocalizationManager.Get("Minecraft.Command.Required"));
			if (normalized.Length > 512 || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
				return (false, LocalizationManager.Get("Minecraft.Command.Unsafe"));
			if (normalized.Equals("stop", StringComparison.OrdinalIgnoreCase))
			{
				bool stopped = await Stop(
					server,
					(message, color) => Core.Instance.Log(message, color));
				return stopped
					? (true, LocalizationManager.Get("Minecraft.Command.StopSucceeded"))
					: (false, LocalizationManager.Get("Minecraft.Command.StopFailed"));
			}

			if (TryWriteRedirectedInput(server, normalized))
			{
				MinecraftConsoleHub.Publish(server, $"> {normalized}", false);
				return (true, LocalizationManager.Get("Minecraft.Command.SentThroughPipe"));
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
					return (true, LocalizationManager.Get("Minecraft.Command.SentThroughRcon"));
				}
			}

			int targetPid = GetInitialTargetPid(server);
			if (targetPid > 0 && await TryWriteConsoleCommand(targetPid, normalized + "\r"))
			{
				MinecraftConsoleHub.Publish(server, $"> {normalized}", false);
				return (true, LocalizationManager.Get("Minecraft.Command.SentThroughConsole"));
			}

			return (
				false,
				LocalizationManager.Get("Minecraft.Command.ChannelUnavailable"));
		}
	}
}
