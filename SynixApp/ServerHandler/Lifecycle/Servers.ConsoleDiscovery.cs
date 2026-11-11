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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

internal sealed record ServerConsoleConnection(int HostProcessId);

public static partial class Servers
{
	[DllImport("ntdll.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern int NtQueryInformationProcess(SafeProcessHandle process, int informationClass,
		out nuint information, uint informationLength, out uint returnLength);

	/// <summary>
	/// Reads the OS console-host association with query-only access. Does not attach
	/// Synix to the console, touch its input, or add control/shutdown permissions.
	/// </summary>
	internal static ServerConsoleConnection? ReadConsoleConnection(ServerProcessIdentity identity)
	{
		if (identity.ProcessId <= 0 || identity.ProcessId == Environment.ProcessId ||
			!identity.StartTimeUtc.HasValue)
			return null;

		try
		{
			using Process target = Process.GetProcessById(identity.ProcessId);
			if (target.HasExited || target.StartTime.ToUniversalTime() != identity.StartTimeUtc.Value ||
				!string.Equals(TryGetProcessImagePath(target.Id), identity.ExecutablePath, StringComparison.OrdinalIgnoreCase))
				return null;
			using SafeProcessHandle handle = OpenProcess(ProcessQueryLimitedInformation, false, target.Id);
			// ProcessConsoleHostProcess (49) returns an ULONG_PTR. Its low two bits
			// are OS flags, not part of the PID. This optional native information class
			// can be unavailable: callers also support verified parent/child discovery.
			if (handle.IsInvalid || NtQueryInformationProcess(handle, 49, out nuint host,
				(uint)IntPtr.Size, out uint returned) < 0 || returned != IntPtr.Size || target.HasExited)
				return null;
			host &= ~(nuint)3;
			return host is > 0 and <= int.MaxValue ? new ServerConsoleConnection((int)host) : null;
		}
		catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or
			System.ComponentModel.Win32Exception or EntryPointNotFoundException or DllNotFoundException)
		{
			return null; // The process may exit or deny access while being sampled.
		}
	}
}
