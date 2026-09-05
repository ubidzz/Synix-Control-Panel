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
using Microsoft.Win32;
using Synix_Control_Panel.SynixEngine;
using System.Management;
using System.Runtime.Intrinsics.X86;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	internal enum GamePrerequisiteState
	{
		Passed,
		Warning,
		Failed
	}

	internal sealed record GamePrerequisiteItem(
		GamePrerequisiteState State,
		string Name,
		string Message);

	internal sealed record GamePrerequisiteSnapshot(
		double? SystemMemoryGb,
		bool Avx2Supported,
		bool? HardwareVirtualizationEnabled,
		string VirtualizationTechnology,
		bool? HypervisorPresent,
		bool? WindowsProfessionalOrHigher,
		int? DotNetFrameworkRelease,
		bool VisualCppRegistryReadable,
		IReadOnlySet<VisualCppRedistributableRequirement>
			InstalledVisualCppRedistributables);

	internal sealed class GamePrerequisiteReport
	{
		internal GamePrerequisiteReport(IReadOnlyList<GamePrerequisiteItem> items) =>
			Items = items;

		internal IReadOnlyList<GamePrerequisiteItem> Items { get; }
		internal bool CanStart => Items.All(item =>
			item.State != GamePrerequisiteState.Failed);
		internal GamePrerequisiteItem? FirstFailure => Items.FirstOrDefault(item =>
			item.State == GamePrerequisiteState.Failed);

		internal string ToDisplayText() => string.Join(
			Environment.NewLine,
			Items
				.Where(item => item.State != GamePrerequisiteState.Passed)
				.Select(item => $"• {item.Message}"));
	}

	internal static class GamePrerequisiteChecker
	{
		private const int DotNetFramework48Release = 528040;
		private const int DotNetFramework481Release = 533320;
		private static readonly object SnapshotLock = new();
		private static DateTime _snapshotTimeUtc;
		private static GamePrerequisiteSnapshot? _cachedSnapshot;

		internal static GamePrerequisiteReport CheckCurrentSystem(
			GameInfo definition,
			GameServer? server = null,
			Func<int, string?>? getPortOwner = null,
			Func<int, bool>? isPortInUse = null)
		{
			ArgumentNullException.ThrowIfNull(definition);
			GamePrerequisiteSnapshot snapshot = GetCurrentSnapshot();
			List<GamePrerequisiteItem> items = [.. Evaluate(definition, snapshot).Items];
			if (server != null && getPortOwner != null && isPortInUse != null)
			{
				items.AddRange(EvaluatePorts(
					definition,
					server,
					getPortOwner,
					isPortInUse));
			}
			return new GamePrerequisiteReport(items);
		}

		internal static GamePrerequisiteReport Evaluate(
			GameInfo definition,
			GamePrerequisiteSnapshot snapshot)
		{
			ArgumentNullException.ThrowIfNull(definition);
			ArgumentNullException.ThrowIfNull(snapshot);
			GameRuntimeRequirements requirements = definition.RuntimeRequirements;
			List<GamePrerequisiteItem> items = [];

			if (requirements.MinimumSystemMemoryGb > 0)
			{
				if (snapshot.SystemMemoryGb is double installedMemory)
				{
					AddResult(
						items,
						installedMemory >= requirements.MinimumSystemMemoryGb,
						LocalizationManager.Get(
							"Prerequisite.SystemMemory.Name"),
						LocalizationManager.Get(
							"Prerequisite.SystemMemory.Required",
							definition.Game,
							requirements.MinimumSystemMemoryGb,
							installedMemory));
				}
				else
				{
					AddUnknown(
						items,
						LocalizationManager.Get(
							"Prerequisite.SystemMemory.Name"),
						LocalizationManager.Get(
							"Prerequisite.SystemMemory.Unknown"));
				}
			}

			if (requirements.RequiresAvx2)
			{
				AddResult(
					items,
					snapshot.Avx2Supported,
					LocalizationManager.Get("Prerequisite.Avx2.Name"),
					LocalizationManager.Get(
						"Prerequisite.Avx2.Required",
						definition.Game));
			}

			EvaluateBooleanRequirement(
				items,
				requirements.RequiresHardwareVirtualization,
				snapshot.HardwareVirtualizationEnabled,
				LocalizationManager.Get(
					"Prerequisite.Virtualization.Name"),
				LocalizationManager.Get(
					"Prerequisite.Virtualization.Required",
					definition.Game,
					snapshot.VirtualizationTechnology));
			EvaluateBooleanRequirement(
				items,
				requirements.RequiresHyperV,
				snapshot.HypervisorPresent,
				LocalizationManager.Get("Prerequisite.HyperV.Name"),
				LocalizationManager.Get(
					"Prerequisite.HyperV.Required",
					definition.Game));
			EvaluateBooleanRequirement(
				items,
				requirements.RequiresWindowsProfessionalOrHigher,
				snapshot.WindowsProfessionalOrHigher,
				LocalizationManager.Get(
					"Prerequisite.WindowsEdition.Name"),
				LocalizationManager.Get(
					"Prerequisite.WindowsEdition.Required",
					definition.Game));

			if (requirements.MinimumDotNetFramework != DotNetFrameworkRequirement.None)
			{
				int requiredRelease = GetRequiredDotNetRelease(
					requirements.MinimumDotNetFramework);
				string label = GetDotNetFrameworkLabel(
					requirements.MinimumDotNetFramework);
				if (snapshot.DotNetFrameworkRelease is int installedRelease)
				{
					AddResult(
						items,
						installedRelease >= requiredRelease,
						label,
						LocalizationManager.Get(
							"Prerequisite.DotNet.Required",
							definition.Game,
							label));
				}
				else
				{
					AddUnknown(
						items,
						label,
						LocalizationManager.Get(
							"Prerequisite.Runtime.Unknown",
							label));
				}
			}

			foreach (VisualCppRedistributableRequirement runtime in
				requirements.VisualCppRedistributables)
			{
				string label = GetVisualCppLabel(runtime);
				if (!snapshot.VisualCppRegistryReadable)
				{
					AddUnknown(
						items,
						label,
						LocalizationManager.Get(
							"Prerequisite.Runtime.Unknown",
							label));
					continue;
				}

				AddResult(
					items,
					snapshot.InstalledVisualCppRedistributables.Contains(runtime),
					label,
					LocalizationManager.Get(
						"Prerequisite.VisualCpp.Required",
						definition.Game,
						label));
			}

			return new GamePrerequisiteReport(items);
		}

		internal static string GetDotNetFrameworkLabel(
			DotNetFrameworkRequirement requirement) => requirement switch
		{
			DotNetFrameworkRequirement.NetFramework48 => ".NET Framework 4.8",
			DotNetFrameworkRequirement.NetFramework481 => ".NET Framework 4.8.1",
			_ => LocalizationManager.Get("Prerequisite.DotNet.None")
		};

		internal static string GetVisualCppLabel(
			VisualCppRedistributableRequirement requirement) => requirement switch
		{
			VisualCppRedistributableRequirement.VisualCpp2013X64 =>
				LocalizationManager.Get("Prerequisite.VisualCpp.2013X64"),
			VisualCppRedistributableRequirement.VisualCpp2015To2022X64 =>
				LocalizationManager.Get("Prerequisite.VisualCpp.2015To2022X64"),
			_ => requirement.ToString()
		};

		private static IReadOnlyList<GamePrerequisiteItem> EvaluatePorts(
			GameInfo definition,
			GameServer server,
			Func<int, string?> getPortOwner,
			Func<int, bool> isPortInUse)
		{
			IReadOnlyList<(int Port, string Name)> candidates =
				Core.GetRequiredServerPorts(server, definition);

			List<GamePrerequisiteItem> items = [];
			foreach ((int port, string name) in candidates
				.Where(candidate => candidate.Port is > 0 and <= 65535)
				.GroupBy(candidate => candidate.Port)
				.Select(group => group.First()))
			{
				string? owner = getPortOwner(port);
				if (!string.IsNullOrWhiteSpace(owner))
				{
					items.Add(new GamePrerequisiteItem(
						GamePrerequisiteState.Failed,
						LocalizationManager.Get(
							"Prerequisite.Port.Name",
							port),
						LocalizationManager.Get(
							"Prerequisite.Port.Assigned",
							name,
							port,
							owner)));
					continue;
				}

				if (isPortInUse(port))
				{
					items.Add(new GamePrerequisiteItem(
						GamePrerequisiteState.Failed,
						LocalizationManager.Get(
							"Prerequisite.Port.Name",
							port),
						LocalizationManager.Get(
							"Prerequisite.Port.InUse",
							name,
							port)));
				}
			}
			return items;
		}

		private static GamePrerequisiteSnapshot GetCurrentSnapshot()
		{
			lock (SnapshotLock)
			{
				if (_cachedSnapshot != null &&
					DateTime.UtcNow - _snapshotTimeUtc < TimeSpan.FromSeconds(30))
				{
					return _cachedSnapshot;
				}

				_cachedSnapshot = CaptureCurrentSystem();
				_snapshotTimeUtc = DateTime.UtcNow;
				return _cachedSnapshot;
			}
		}

		private static GamePrerequisiteSnapshot CaptureCurrentSystem()
		{
			(double? memory, bool? hypervisor, bool? windowsPro) =
				ReadComputerSystem();
			(bool? virtualization, string technology) = ReadVirtualization();
			IReadOnlySet<VisualCppRedistributableRequirement> runtimes =
				ReadVisualCppRedistributables(out bool runtimeRegistryReadable);

			return new GamePrerequisiteSnapshot(
				memory,
				Avx2.IsSupported,
				virtualization,
				technology,
				hypervisor,
				windowsPro,
				ReadDotNetFrameworkRelease(),
				runtimeRegistryReadable,
				runtimes);
		}

		private static (double? Memory, bool? Hypervisor, bool? WindowsPro)
			ReadComputerSystem()
		{
			double? memory = null;
			bool? hypervisor = null;
			bool? windowsPro = null;
			try
			{
				using ManagementObjectSearcher computerSearch = new(
					"SELECT TotalPhysicalMemory, HypervisorPresent FROM Win32_ComputerSystem");
				using ManagementObjectCollection computers = computerSearch.Get();
				foreach (ManagementObject computer in computers)
				{
					if (computer["TotalPhysicalMemory"] != null)
					{
						memory = Convert.ToUInt64(computer["TotalPhysicalMemory"]) /
							(1024d * 1024d * 1024d);
					}
					if (computer["HypervisorPresent"] != null)
						hypervisor = Convert.ToBoolean(computer["HypervisorPresent"]);
					break;
				}

				using ManagementObjectSearcher osSearch = new(
					"SELECT Caption FROM Win32_OperatingSystem");
				using ManagementObjectCollection operatingSystems = osSearch.Get();
				foreach (ManagementObject operatingSystem in operatingSystems)
				{
					string caption = Convert.ToString(operatingSystem["Caption"]) ?? string.Empty;
					windowsPro = !caption.Contains("Home", StringComparison.OrdinalIgnoreCase);
					break;
				}
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			return (memory, hypervisor, windowsPro);
		}

		private static (bool? Enabled, string Technology) ReadVirtualization()
		{
			try
			{
				using ManagementObjectSearcher searcher = new(
					"SELECT VirtualizationFirmwareEnabled, Manufacturer FROM Win32_Processor");
				using ManagementObjectCollection processors = searcher.Get();
				foreach (ManagementObject processor in processors)
				{
					string manufacturer = Convert.ToString(processor["Manufacturer"]) ?? string.Empty;
					string technology = manufacturer.Contains("Intel", StringComparison.OrdinalIgnoreCase)
						? "Intel VT-x"
						: manufacturer.Contains("AMD", StringComparison.OrdinalIgnoreCase)
							? "AMD-V (SVM)"
							: LocalizationManager.Get("Prerequisite.Virtualization.Generic");
					bool? enabled = processor["VirtualizationFirmwareEnabled"] == null
						? null
						: Convert.ToBoolean(processor["VirtualizationFirmwareEnabled"]);
					return (enabled, technology);
				}
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			return (
				null,
				LocalizationManager.Get("Prerequisite.Virtualization.Generic"));
		}

		private static int? ReadDotNetFrameworkRelease()
		{
			try
			{
				using RegistryKey baseKey = RegistryKey.OpenBaseKey(
					RegistryHive.LocalMachine,
					RegistryView.Registry32);
				using RegistryKey? key = baseKey.OpenSubKey(
					@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full");
				return key?.GetValue("Release") is object value
					? Convert.ToInt32(value)
					: 0;
			}
			catch (Exception suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(
					suppressedException,
					"ReadDotNetFrameworkRelease");
				return null;
			}
		}

		private static IReadOnlySet<VisualCppRedistributableRequirement>
			ReadVisualCppRedistributables(out bool registryReadable)
		{
			HashSet<VisualCppRedistributableRequirement> installed = [];
			try
			{
				using RegistryKey baseKey = RegistryKey.OpenBaseKey(
					RegistryHive.LocalMachine,
					RegistryView.Registry64);
				if (IsRegistryRuntimeInstalled(
					baseKey,
					@"SOFTWARE\Microsoft\VisualStudio\12.0\VC\Runtimes\x64"))
				{
					installed.Add(VisualCppRedistributableRequirement.VisualCpp2013X64);
				}
				if (IsRegistryRuntimeInstalled(
					baseKey,
					@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"))
				{
					installed.Add(VisualCppRedistributableRequirement.VisualCpp2015To2022X64);
				}
				registryReadable = true;
			}
			catch
			{
				registryReadable = false;
			}
			return installed;
		}

		private static bool IsRegistryRuntimeInstalled(
			RegistryKey baseKey,
			string path)
		{
			using RegistryKey? key = baseKey.OpenSubKey(path);
			object? value = key?.GetValue("Installed");
			return value != null && Convert.ToInt32(value) == 1;
		}

		private static int GetRequiredDotNetRelease(
			DotNetFrameworkRequirement requirement) => requirement switch
		{
			DotNetFrameworkRequirement.NetFramework48 => DotNetFramework48Release,
			DotNetFrameworkRequirement.NetFramework481 => DotNetFramework481Release,
			_ => 0
		};

		private static void EvaluateBooleanRequirement(
			ICollection<GamePrerequisiteItem> items,
			bool required,
			bool? available,
			string name,
			string failureMessage)
		{
			if (!required)
				return;
			if (available.HasValue)
				AddResult(items, available.Value, name, failureMessage);
			else
				AddUnknown(
					items,
					name,
					LocalizationManager.Get(
						"Prerequisite.Requirement.Unknown",
						name));
		}

		private static void AddResult(
			ICollection<GamePrerequisiteItem> items,
			bool passed,
			string name,
			string failureMessage) => items.Add(new GamePrerequisiteItem(
			passed ? GamePrerequisiteState.Passed : GamePrerequisiteState.Failed,
			name,
			passed
				? LocalizationManager.Get(
					"Prerequisite.Requirement.Available",
					name)
				: failureMessage));

		private static void AddUnknown(
			ICollection<GamePrerequisiteItem> items,
			string name,
			string message) => items.Add(new GamePrerequisiteItem(
			GamePrerequisiteState.Warning,
			name,
			message));
	}
}
