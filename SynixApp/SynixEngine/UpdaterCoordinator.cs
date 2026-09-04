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
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#if SYNIX_STABLE_RELEASE
namespace Synix_Control_Panel.SynixEngine
{
	internal enum SynixUpdateApplyMode
	{
		Standalone,
		Msi,
		WinGet
	}

	internal sealed class SynixUpdateRequest
	{
		public int FormatVersion { get; set; } = 1;
		public SynixUpdateApplyMode Mode { get; set; }
		public int ParentProcessId { get; set; }
		[JsonIgnore]
		public string OperationDirectory { get; set; } = string.Empty;
		[JsonIgnore]
		public string PayloadPath { get; set; } = string.Empty;
		[JsonIgnore]
		public string DestinationPath { get; set; } = string.Empty;
		public string ExpectedSha256 { get; set; } = string.Empty;
		public string PreviousVersion { get; set; } = string.Empty;
		public string NewVersion { get; set; } = string.Empty;
		[JsonIgnore]
		public string ReadyMarkerPath { get; set; } = string.Empty;
		[JsonIgnore]
		public string StartupSuccessMarkerPath { get; set; } = string.Empty;
	}

	public sealed record SynixPreparedUpdate(
		string HelperPath,
		string RequestPath,
		string ReadyMarkerPath,
		Version NewVersion);

	public partial class Core
	{
		public const string ApplyUpdateArgument = "--synix-apply-update";
		public const string UpdateStartedArgument = "--synix-update-started";
		public const string UpdateRolledBackArgument = "--synix-update-rolled-back";

		private const int RequestFormatVersion = 2;
		private const int ParentExitTimeoutSeconds = 60;
		private const int UpdatedStartupTimeoutSeconds = 90;
		private const int InstallerTimeoutMinutes = 15;
		private const string LegacyUpdaterSetupUninstallKey =
			@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{D3E8B790-86E8-4485-B827-7A743AB72BDB}_is1";

		public static async Task<SynixPreparedUpdate> PrepareUpdateAsync(
			SynixUpdateCheckResult check,
			IProgress<SynixUpdateDownloadProgress>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(check);
			if (!check.CanInstall || check.Release is null || check.Asset is null)
				throw new InvalidOperationException("This Synix update is not ready to install.");

			string updaterRoot = GetUpdaterRoot();
			Directory.CreateDirectory(updaterRoot);
			CleanupOldOperations(updaterRoot);

			long helperSize = new FileInfo(check.Installation.ExecutablePath).Length;
			long requiredBytes = checked(check.Asset.Size * 2 + helperSize + 16L * 1024 * 1024);
			EnsureFreeSpace(updaterRoot, requiredBytes);
			if (check.Installation.Kind == SynixInstallationKind.Standalone)
			{
				string destinationDirectory = Path.GetDirectoryName(
					check.Installation.ExecutablePath) ?? string.Empty;
				EnsureFolderCanBeUpdated(destinationDirectory, check.Asset.Size);
			}

			string operationDirectory = Path.Combine(
				updaterRoot,
				Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(operationDirectory);

			string payloadFileName = check.Installation.Kind ==
				SynixInstallationKind.Standalone
					? "SynixUpdatePayload.exe"
					: "SynixUpdatePayload.msi";
			string payloadPath = Path.Combine(operationDirectory, payloadFileName);
			string helperPath = Path.Combine(operationDirectory, "SynixUpdater.exe");
			string requestPath = Path.Combine(operationDirectory, "update-request.json");
			string readyMarkerPath = Path.Combine(operationDirectory, "helper-ready.marker");

			try
			{
				await DownloadUpdateAssetAsync(
					check.Asset,
					payloadPath,
					progress,
					cancellationToken);

				File.Copy(
					check.Installation.ExecutablePath,
					helperPath,
					overwrite: false);

				SynixUpdateRequest request = new()
				{
					FormatVersion = RequestFormatVersion,
					Mode = check.Installation.Kind switch
					{
						SynixInstallationKind.Standalone => SynixUpdateApplyMode.Standalone,
						SynixInstallationKind.WinGet => SynixUpdateApplyMode.WinGet,
						_ => SynixUpdateApplyMode.Msi
					},
					ParentProcessId = Environment.ProcessId,
					ExpectedSha256 = check.Asset.Sha256,
					PreviousVersion = check.CurrentVersion.ToString(3),
					NewVersion = check.Release.Version.ToString(3)
				};

				File.WriteAllText(
					requestPath,
					JsonSerializer.Serialize(
						request,
						new JsonSerializerOptions { WriteIndented = true }),
					Encoding.UTF8);

				return new SynixPreparedUpdate(
					helperPath,
					requestPath,
					readyMarkerPath,
					check.Release.Version);
			}
			catch
			{
				TryDeleteUpdateDirectory(operationDirectory);
				throw;
			}
		}

		public static void LaunchPreparedUpdate(SynixPreparedUpdate preparedUpdate)
		{
			ArgumentNullException.ThrowIfNull(preparedUpdate);
			ProcessStartInfo startInfo = new(preparedUpdate.HelperPath)
			{
				UseShellExecute = true,
				WorkingDirectory = Path.GetDirectoryName(preparedUpdate.HelperPath)
			};
			startInfo.ArgumentList.Add(ApplyUpdateArgument);
			startInfo.ArgumentList.Add(preparedUpdate.RequestPath);

			Process helper = Process.Start(startInfo) ??
				throw new InvalidOperationException("Synix could not start the update helper.");
			using (helper)
			{
				DateTime deadline = DateTime.UtcNow.AddSeconds(10);
				while (DateTime.UtcNow < deadline)
				{
					if (File.Exists(preparedUpdate.ReadyMarkerPath))
						return;
					if (helper.HasExited)
						break;
					Thread.Sleep(50);
				}
			}

			throw new InvalidOperationException(
				"The Synix update helper could not prepare the update safely.");
		}

		public static bool TryRunUpdateHelper(string[] args)
		{
			if (args.Length != 2 ||
				!string.Equals(args[0], ApplyUpdateArgument, StringComparison.Ordinal))
			{
				return false;
			}

			try
			{
				RunUpdateHelper(args[1]);
			}
			catch (Exception exception)
			{
				TryWriteHelperFailure(args[1], exception);
				LocalizedMessageBox.Show(
					"Synix could not finish the automatic update. Synix attempted to restore the previous program when possible.\n\n" +
					"If Synix does not reopen, run your existing Synix program or download the latest release from GitHub. Your C:\\Synix server data was not changed.",
					"Synix Update Did Not Complete",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			return true;
		}

		public static string? GetStartupSuccessMarker(string[] args)
		{
			return GetArgumentValue(args, UpdateStartedArgument);
		}

		public static string? GetRollbackVersion(string[] args)
		{
			string? version = GetArgumentValue(args, UpdateRolledBackArgument);
			return version is { Length: > 0 and <= 32 } ? version : null;
		}

		public static void MarkStartupSuccessful(string markerPath)
		{
			if (!IsPathInsideUpdaterRoot(markerPath))
				return;

			try
			{
				File.WriteAllText(markerPath, DateTimeOffset.UtcNow.ToString("O"));
				string? operationDirectory = Path.GetDirectoryName(
					Path.GetFullPath(markerPath));
				if (operationDirectory is not null)
				{
					_ = Task.Run(async () =>
						{
							string readyMarker = Path.Combine(
								operationDirectory,
								"helper-ready.marker");
							for (int attempt = 0; attempt < 30; attempt++)
							{
								await Task.Delay(TimeSpan.FromSeconds(1));
								if (File.Exists(readyMarker))
									continue;

								TryDeleteUpdateDirectory(operationDirectory);
								if (!Directory.Exists(operationDirectory))
									return;
							}
						});
				}
			}
			catch
			{
			}
		}

		public static void CleanupStaleOperations()
		{
			try
			{
				string updaterRoot = GetUpdaterRoot();
				if (Directory.Exists(updaterRoot))
					CleanupOldOperations(updaterRoot);
			}
			catch
			{
			}
		}

		private static void RunUpdateHelper(string requestPath)
		{
			SynixUpdateRequest request = ReadAndValidateRequest(requestPath);
			using Process parent = Process.GetProcessById(request.ParentProcessId);
			string? parentExecutable = TryGetProcessExecutable(parent);
			string? helperExecutable = Environment.ProcessPath;
			if (parentExecutable is null || helperExecutable is null ||
				!IsPathInsideOperation(
					helperExecutable,
					request.OperationDirectory) ||
				!FilesMatch(helperExecutable, parentExecutable))
			{
				throw new InvalidDataException(
					"The update request does not match the running Synix process.");
			}
			request.DestinationPath = Path.GetFullPath(parentExecutable);

			File.WriteAllText(request.ReadyMarkerPath, "ready");
			if (!parent.WaitForExit(TimeSpan.FromSeconds(ParentExitTimeoutSeconds)))
				throw new TimeoutException("Synix did not close in time for the update.");

			VerifyPayload(request.PayloadPath, request.ExpectedSha256);
			string backupPath = Path.Combine(
				request.OperationDirectory,
				"PreviousSynix.exe");
			File.Copy(request.DestinationPath, backupPath, overwrite: false);

			try
			{
				bool applied = request.Mode switch
				{
					SynixUpdateApplyMode.Standalone => ApplyStandaloneUpdate(request),
					SynixUpdateApplyMode.WinGet => ApplyWinGetOrMsiUpdate(request),
					_ => ApplyMsiUpdate(request)
				};
				if (!applied)
					throw new InvalidOperationException("The update installer did not complete successfully.");

				Process updatedProcess = StartSynix(
					request.DestinationPath,
					UpdateStartedArgument,
					request.StartupSuccessMarkerPath);
				bool startupSucceeded = WaitForUpdatedStartup(
					updatedProcess,
					request.StartupSuccessMarkerPath);

				if (!startupSucceeded && updatedProcess.HasExited)
				{
					RollbackAndRestart(request, backupPath);
					return;
				}

				TryDeleteUpdateFile(backupPath);
				TryDeleteUpdateFile(request.PayloadPath);
				TryDeleteUpdateFile(requestPath);
				TryDeleteUpdateFile(request.ReadyMarkerPath);
			}
			catch (Exception updateException)
			{
				TryWriteHelperFailure(requestPath, updateException);
				try
				{
					RollbackAndRestart(request, backupPath);
					return;
				}
				catch (Exception rollbackException)
				{
					throw new AggregateException(
						"The Synix update failed and the previous program could not be restored automatically.",
						updateException,
						rollbackException);
				}
			}
		}

		private static SynixUpdateRequest ReadAndValidateRequest(string requestPath)
		{
			string fullRequestPath = Path.GetFullPath(requestPath);
			if (!IsPathInsideUpdaterRoot(fullRequestPath) ||
				!File.Exists(fullRequestPath))
			{
				throw new InvalidDataException("The update request path is not trusted.");
			}

			FileInfo requestFile = new(fullRequestPath);
			if (requestFile.Length <= 0 || requestFile.Length > 64 * 1024)
				throw new InvalidDataException("The update request has an unsafe size.");

			SynixUpdateRequest request = JsonSerializer.Deserialize<SynixUpdateRequest>(
				File.ReadAllText(fullRequestPath)) ??
				throw new InvalidDataException("The update request is incomplete.");
			string operationDirectory = Path.GetDirectoryName(fullRequestPath) ??
				throw new InvalidDataException("The update request folder is missing.");
			request.OperationDirectory = operationDirectory;
			request.PayloadPath = Path.Combine(
				operationDirectory,
				request.Mode == SynixUpdateApplyMode.Standalone
					? "SynixUpdatePayload.exe"
					: "SynixUpdatePayload.msi");
			request.ReadyMarkerPath = Path.Combine(
				operationDirectory,
				"helper-ready.marker");
			request.StartupSuccessMarkerPath = Path.Combine(
				operationDirectory,
				"startup-success.marker");

			if (request.FormatVersion != RequestFormatVersion ||
				request.ParentProcessId <= 0 ||
				!Enum.IsDefined(request.Mode) ||
				!File.Exists(request.PayloadPath) ||
				request.ExpectedSha256.Length != 64 ||
				request.ExpectedSha256.Any(character => !Uri.IsHexDigit(character)) ||
				!IsSafeVersionText(request.PreviousVersion) ||
				!IsSafeVersionText(request.NewVersion))
			{
				throw new InvalidDataException("The update request failed validation.");
			}

			return request;
		}

		private static bool IsSafeVersionText(string? value)
		{
			return value is { Length: > 0 and <= 32 } &&
				Version.TryParse(value, out _);
		}

		private static bool ApplyStandaloneUpdate(SynixUpdateRequest request)
		{
			string directory = Path.GetDirectoryName(request.DestinationPath) ??
				throw new InvalidOperationException("The standalone Synix folder is missing.");
			string stagedPath = Path.Combine(
				directory,
				$".{Path.GetFileName(request.DestinationPath)}.{Guid.NewGuid():N}.update");

			try
			{
				File.Copy(request.PayloadPath, stagedPath, overwrite: false);
				ReplaceFileSafely(stagedPath, request.DestinationPath);
				return true;
			}
			finally
			{
				TryDeleteUpdateFile(stagedPath);
			}
		}

		private static bool ApplyMsiUpdate(SynixUpdateRequest request)
		{
			string msiexecPath = Path.Combine(
				Environment.SystemDirectory,
				"msiexec.exe");
			if (!File.Exists(msiexecPath))
				throw new FileNotFoundException("Windows Installer could not be found.", msiexecPath);

			ProcessStartInfo startInfo = new(msiexecPath)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = request.OperationDirectory
			};
			startInfo.ArgumentList.Add("/i");
			startInfo.ArgumentList.Add(request.PayloadPath);
			startInfo.ArgumentList.Add("/qn");
			startInfo.ArgumentList.Add("/norestart");
			startInfo.ArgumentList.Add(
				request.Mode == SynixUpdateApplyMode.WinGet
					? "SYNIXINSTALLSOURCE=WinGet"
					: "SYNIXINSTALLSOURCE=Setup");

			using Process installer = Process.Start(startInfo) ??
				throw new InvalidOperationException("Synix could not start its MSI update.");
			if (!installer.WaitForExit(TimeSpan.FromMinutes(InstallerTimeoutMinutes)))
			{
				StopTimedOutProcess(installer);
				return false;
			}

			if (installer.ExitCode is not (0 or 3010))
				return false;

			string? installedExecutable =
				Core.GetMsiInstalledExecutablePath();
			if (string.IsNullOrWhiteSpace(installedExecutable) ||
				!File.Exists(installedExecutable))
			{
				return false;
			}

			request.DestinationPath = installedExecutable;
			return true;
		}

		private static bool ApplyWinGetOrMsiUpdate(SynixUpdateRequest request)
		{
			string? wingetPath = GetTrustedWinGetPath();
			if (wingetPath is null)
				return ApplyMsiUpdate(request);

			try
			{
				ProcessStartInfo startInfo = new(wingetPath)
				{
					UseShellExecute = false,
					CreateNoWindow = true
				};
				startInfo.ArgumentList.Add("upgrade");
				startInfo.ArgumentList.Add("--id");
				startInfo.ArgumentList.Add(Core.WinGetPackageId);
				startInfo.ArgumentList.Add("--exact");
				startInfo.ArgumentList.Add("--silent");
				startInfo.ArgumentList.Add("--accept-package-agreements");
				startInfo.ArgumentList.Add("--accept-source-agreements");
				startInfo.ArgumentList.Add("--disable-interactivity");

				using Process? winget = Process.Start(startInfo);
				if (winget is not null)
				{
					if (!winget.WaitForExit(TimeSpan.FromMinutes(InstallerTimeoutMinutes)))
					{
						StopTimedOutProcess(winget);
					}
					else if (winget.ExitCode == 0)
					{
						string? installedExecutable =
							Core.GetMsiInstalledExecutablePath();
						if (!string.IsNullOrWhiteSpace(installedExecutable) &&
							File.Exists(installedExecutable))
						{
							request.DestinationPath = installedExecutable;
							return true;
						}
					}
				}
			}
			catch (Exception exception) when (
				exception is System.ComponentModel.Win32Exception or InvalidOperationException)
			{
			}

			return ApplyMsiUpdate(request);
		}

		private static string? GetTrustedWinGetPath()
		{
			string candidate = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Microsoft",
				"WindowsApps",
				"winget.exe");
			return File.Exists(candidate) ? candidate : null;
		}

		private static Process StartSynix(
			string executablePath,
			string argumentName,
			string argumentValue)
		{
			ProcessStartInfo startInfo = new(executablePath)
			{
				UseShellExecute = true,
				WorkingDirectory = Path.GetDirectoryName(executablePath)
			};
			startInfo.ArgumentList.Add(argumentName);
			startInfo.ArgumentList.Add(argumentValue);
			return Process.Start(startInfo) ??
				throw new InvalidOperationException("Synix could not restart after updating.");
		}

		private static bool WaitForUpdatedStartup(
			Process process,
			string markerPath)
		{
			using (process)
			{
				DateTime deadline = DateTime.UtcNow.AddSeconds(
					UpdatedStartupTimeoutSeconds);
				while (DateTime.UtcNow < deadline)
				{
					if (File.Exists(markerPath))
						return true;
					if (process.HasExited)
						return false;
					Thread.Sleep(250);
				}

				return !process.HasExited;
			}
		}

		private static void RestorePreviousExecutable(
			string backupPath,
			string destinationPath)
		{
			string directory = Path.GetDirectoryName(destinationPath) ??
				throw new InvalidOperationException("The Synix program folder is missing.");
			string restoreStage = Path.Combine(
				directory,
				$".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.rollback");

			try
			{
				File.Copy(backupPath, restoreStage, overwrite: false);
				ReplaceFileSafely(restoreStage, destinationPath);
			}
			finally
			{
				TryDeleteUpdateFile(restoreStage);
			}
		}

		private static void RollbackAndRestart(
			SynixUpdateRequest request,
			string backupPath)
		{
			RestorePreviousExecutable(backupPath, request.DestinationPath);
			if (request.Mode != SynixUpdateApplyMode.Standalone)
				RestoreRegisteredVersion(request.PreviousVersion);
			StartSynix(
				request.DestinationPath,
				UpdateRolledBackArgument,
				request.NewVersion);
		}

		private static void ReplaceFileSafely(
			string stagedPath,
			string destinationPath)
		{
			if (!File.Exists(destinationPath))
			{
				File.Move(stagedPath, destinationPath, overwrite: false);
				return;
			}

			try
			{
				File.Replace(stagedPath, destinationPath, null, true);
			}
			catch (Exception exception) when (
				exception is IOException or PlatformNotSupportedException)
			{
				File.Move(stagedPath, destinationPath, overwrite: true);
			}
		}

		private static void StopTimedOutProcess(Process process)
		{
			try
			{
				if (!process.HasExited)
				{
					process.Kill(entireProcessTree: true);
					process.WaitForExit(TimeSpan.FromSeconds(10));
				}
			}
			catch
			{
			}
		}

		private static void RestoreRegisteredVersion(string previousVersion)
		{
			try
			{
				using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
					LegacyUpdaterSetupUninstallKey,
					writable: true);
				key?.SetValue(
					"DisplayVersion",
					previousVersion,
					RegistryValueKind.String);
			}
			catch
			{
			}
		}

		private static void VerifyPayload(string payloadPath, string expectedSha256)
		{
			using FileStream stream = File.OpenRead(payloadPath);
			byte[] actual = SHA256.HashData(stream);
			byte[] expected = Convert.FromHexString(expectedSha256);
			bool matches = CryptographicOperations.FixedTimeEquals(actual, expected);
			CryptographicOperations.ZeroMemory(actual);
			CryptographicOperations.ZeroMemory(expected);
			if (!matches)
				throw new InvalidDataException("The downloaded update no longer matches GitHub's SHA-256 digest.");
		}

		private static bool FilesMatch(string leftPath, string rightPath)
		{
			FileInfo leftFile = new(leftPath);
			FileInfo rightFile = new(rightPath);
			if (!leftFile.Exists || !rightFile.Exists ||
				leftFile.Length != rightFile.Length)
			{
				return false;
			}

			using FileStream leftStream = File.OpenRead(leftPath);
			using FileStream rightStream = File.OpenRead(rightPath);
			byte[] leftHash = SHA256.HashData(leftStream);
			byte[] rightHash = SHA256.HashData(rightStream);
			bool matches = CryptographicOperations.FixedTimeEquals(
				leftHash,
				rightHash);
			CryptographicOperations.ZeroMemory(leftHash);
			CryptographicOperations.ZeroMemory(rightHash);
			return matches;
		}

		private static void EnsureFolderCanBeUpdated(string directory, long assetSize)
		{
			if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
				throw new DirectoryNotFoundException("The standalone Synix folder could not be found.");

			EnsureFreeSpace(directory, checked(assetSize * 2 + 8L * 1024 * 1024));
			string probePath = Path.Combine(
				directory,
				$".synix-update-write-test-{Guid.NewGuid():N}.tmp");
			try
			{
				using FileStream probe = new(
					probePath,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None,
					1,
					FileOptions.DeleteOnClose);
				probe.WriteByte(0);
			}
			catch (UnauthorizedAccessException exception)
			{
				throw new InvalidOperationException(
					"Windows will not allow Synix to update this standalone folder. Move Synix to a folder you can write to, then try again.",
					exception);
			}
			finally
			{
				TryDeleteUpdateFile(probePath);
			}
		}

		private static void EnsureFreeSpace(string path, long requiredBytes)
		{
			string root = Path.GetPathRoot(Path.GetFullPath(path)) ??
				throw new InvalidOperationException("Synix could not identify the update drive.");
			DriveInfo drive = new(root);
			if (drive.AvailableFreeSpace < requiredBytes)
			{
				throw new IOException(
					$"The update needs about {FormatUpdateBytes(requiredBytes)} free on {drive.Name}, but only {FormatUpdateBytes(drive.AvailableFreeSpace)} is available.");
			}
		}

		private static string GetUpdaterRoot()
		{
			return Path.GetFullPath(Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Synix",
				"Updater"));
		}

		private static bool IsPathInsideUpdaterRoot(string path)
		{
			return IsPathInsideOperation(path, GetUpdaterRoot());
		}

		private static bool IsPathInsideOperation(string path, string operationDirectory)
		{
			try
			{
				string fullRoot = Path.TrimEndingDirectorySeparator(
					Path.GetFullPath(operationDirectory)) + Path.DirectorySeparatorChar;
				string fullPath = Path.GetFullPath(path);
				return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception exception) when (
				exception is ArgumentException or NotSupportedException or PathTooLongException)
			{
				return false;
			}
		}

		private static string? TryGetProcessExecutable(Process process)
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

		private static string? GetArgumentValue(string[] args, string name)
		{
			for (int index = 0; index + 1 < args.Length; index++)
			{
				if (string.Equals(args[index], name, StringComparison.Ordinal))
					return args[index + 1];
			}
			return null;
		}

		private static void CleanupOldOperations(string updaterRoot)
		{
			try
			{
				foreach (string directory in Directory.EnumerateDirectories(updaterRoot))
				{
					if (!IsPathInsideOperation(directory, updaterRoot))
						continue;
					DateTime lastWrite = Directory.GetLastWriteTimeUtc(directory);
					if (lastWrite < DateTime.UtcNow.AddDays(-7))
						TryDeleteUpdateDirectory(directory);
				}
			}
			catch
			{
			}
		}

		private static void TryWriteHelperFailure(
			string requestPath,
			Exception exception)
		{
			try
			{
				string? directory = Path.GetDirectoryName(Path.GetFullPath(requestPath));
				if (directory is null || !IsPathInsideUpdaterRoot(directory))
					return;
				File.WriteAllText(
					Path.Combine(directory, "update-error.log"),
					$"{DateTimeOffset.Now:O} {exception.GetType().Name}: {exception.Message}");
			}
			catch
			{
			}
		}

		private static void TryDeleteUpdateFile(string path)
		{
			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch
			{
			}
		}

		private static void TryDeleteUpdateDirectory(string path)
		{
			try
			{
				if (Directory.Exists(path) && IsPathInsideUpdaterRoot(path))
					Directory.Delete(path, recursive: true);
			}
			catch
			{
			}
		}

		private static string FormatUpdateBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB"];
			double value = Math.Max(0, bytes);
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}
			return $"{value:0.##} {units[unit]}";
		}
	}
}
#else
namespace Synix_Control_Panel.SynixEngine
{
	public sealed record SynixPreparedUpdate(
		string HelperPath,
		string RequestPath,
		string ReadyMarkerPath,
		Version NewVersion);

	public partial class Core
	{
		public const string ApplyUpdateArgument = "--synix-apply-update";
		public const string UpdateStartedArgument = "--synix-update-started";
		public const string UpdateRolledBackArgument = "--synix-update-rolled-back";

		public static Task<SynixPreparedUpdate> PrepareUpdateAsync(
			SynixUpdateCheckResult check,
			IProgress<SynixUpdateDownloadProgress>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(check);
			return Task.FromException<SynixPreparedUpdate>(
				new InvalidOperationException(
					"Automatic updater operations are available only in an official Stable Synix release."));
		}

		public static void LaunchPreparedUpdate(
			SynixPreparedUpdate preparedUpdate)
		{
			ArgumentNullException.ThrowIfNull(preparedUpdate);
			throw new InvalidOperationException(
				"Automatic updater operations are disabled in development builds.");
		}

		public static bool TryRunUpdateHelper(string[] args)
		{
			ArgumentNullException.ThrowIfNull(args);
			return false;
		}

		public static string? GetStartupSuccessMarker(string[] args)
		{
			ArgumentNullException.ThrowIfNull(args);
			return null;
		}

		public static string? GetRollbackVersion(string[] args)
		{
			ArgumentNullException.ThrowIfNull(args);
			return null;
		}

		public static void MarkStartupSuccessful(string markerPath)
		{
		}

		public static void CleanupStaleOperations()
		{
		}
	}
}
#endif
