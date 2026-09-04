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
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;

namespace Synix_Control_Panel.SynixApp.SteamCMDHandler
{
	public static class ServerInstaller
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public static int Install(GameServer server, GameInfo blueprint, Action<string> logCallback, Action<int>? onPidStarted = null)
		{
			ArgumentNullException.ThrowIfNull(logCallback);
			if (blueprint.AppID == "0" || blueprint.AppID.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				return InstallDirectDownloadAsync(server, blueprint, logCallback).GetAwaiter().GetResult();
			}

			ProcessStartInfo startInfo;
			int? downloadThrottleKbps = GetConfiguredDownloadThrottleKbps();
			try
			{
				startInfo = CreateSteamProcessStartInfo(
					server,
					blueprint,
					downloadThrottleKbps);
			}
			catch (InvalidOperationException ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.CriticalDetail", ex.Message));
				return 97;
			}

			if (downloadThrottleKbps.HasValue)
			{
				logCallback?.Invoke(
					LocalizationManager.Get(
						"Installer.Activity.DownloadLimited",
						downloadThrottleKbps.Value / 1000));
			}

			if (blueprint.RequiresSteamLogin)
			{
				return InstallWithSteamAccount(
					server,
					blueprint,
					startInfo,
					logCallback!,
					onPidStarted);
			}

			int hasInternalError = 0;
			string lastLoggedLine = "";
			object lineSync = new();

			using Process process = new()
			{
				StartInfo = startInfo
			};

			Channel<string> logQueue = Channel.CreateBounded<string>(
				new BoundedChannelOptions(4096)
				{
					SingleReader = true,
					SingleWriter = false,
					AllowSynchronousContinuations = false,
					FullMode = BoundedChannelFullMode.DropOldest
				});

			Task dashboardWriter = Task.Run(async () =>
			{
				await foreach (string line in
					logQueue.Reader.ReadAllAsync())
				{
					try
					{
						logCallback?.Invoke(line);
					}
					catch (Exception suppressedException)
					{
						Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
					}
				}
			});

			DateTime lastProgressTime = DateTime.MinValue;

			void QueueSteamLine(string text)
			{
				string line = text.Trim();

				if (line.Length == 0)
					return;

				if (line.Contains(
						"ERROR!",
						StringComparison.OrdinalIgnoreCase) ||
					line.Contains(
						"subscription",
						StringComparison.OrdinalIgnoreCase) ||
					line.Contains(
						"AppID not found",
						StringComparison.OrdinalIgnoreCase) ||
					line.Contains(
						"FAILED",
						StringComparison.OrdinalIgnoreCase))
				{
					Interlocked.Exchange(ref hasInternalError, 1);
				}

				bool isProgressLine =
					line.Contains(
						"progress:",
						StringComparison.OrdinalIgnoreCase) ||
					line.Contains(
						"downloading",
						StringComparison.OrdinalIgnoreCase);

				lock (lineSync)
				{
					if (isProgressLine)
					{
						DateTime now = DateTime.UtcNow;

						if ((now - lastProgressTime).TotalMilliseconds < 250)
							return;

						lastProgressTime = now;
					}

					if (line.Equals(
							lastLoggedLine,
							StringComparison.Ordinal))
					{
						return;
					}

					lastLoggedLine = line;
				}

				logQueue.Writer.TryWrite(line);
			}

			void SetMainWindowTitle(string title)
			{
				ApplicationUiService.SetMainWindowTitle(title);
			}

			CancellationTokenSource heartbeatCts =
				new CancellationTokenSource();

			Stopwatch installTimer = new Stopwatch();
			Task heartbeatTask = Task.CompletedTask;
			int resultCode = -1;

			try
			{
				process.Start();
				onPidStarted?.Invoke(process.Id);

				installTimer.Start();

				heartbeatTask = Task.Run(async () =>
				{
					try
					{
						while (true)
						{
							await Task.Delay(
								1000,
								heartbeatCts.Token).ConfigureAwait(false);

							TimeSpan elapsed = installTimer.Elapsed;

							SetMainWindowTitle(
								LocalizationManager.Get(
									"Installer.Window.Working",
									elapsed.Minutes,
									elapsed.Seconds));
						}
					}
					catch (OperationCanceledException)
						when (heartbeatCts.IsCancellationRequested)
					{

					}
				});

				Task outputReader = PumpStreamAsync(
					process.StandardOutput,
					QueueSteamLine);

				Task errorReader = PumpStreamAsync(
					process.StandardError,
					QueueSteamLine);

				process.WaitForExit();

				Task.WhenAll(outputReader, errorReader)
					.GetAwaiter()
					.GetResult();

				resultCode =
					Volatile.Read(ref hasInternalError) == 1
						? 99
						: process.ExitCode;
			}
			catch (Exception ex)
			{
				logQueue.Writer.TryWrite(
					LocalizationManager.Get("Installer.Activity.LauncherError", ex.Message));

				resultCode = -1;
			}
			finally
			{

				heartbeatCts.Cancel();

				try
				{
					heartbeatTask.GetAwaiter().GetResult();
				}
				catch (OperationCanceledException suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
				}
				catch (Exception ex)
				{
					logQueue.Writer.TryWrite(
						LocalizationManager.Get("Installer.Activity.HeartbeatError", ex.Message));
				}

				installTimer.Stop();
				heartbeatCts.Dispose();

				logQueue.Writer.TryComplete();

				try
				{
					dashboardWriter.GetAwaiter().GetResult();
				}
				catch (Exception suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
				}

				SetMainWindowTitle(LocalizationManager.Get("App.Title"));
				Core.Instance.UpdateGridStatus();
			}

			return resultCode;
		}

		internal static ProcessStartInfo CreateSteamProcessStartInfo(
			GameServer server,
			GameInfo blueprint)
		{
			return CreateSteamProcessStartInfo(
				server,
				blueprint,
				GetConfiguredDownloadThrottleKbps());
		}

		internal static ProcessStartInfo CreateSteamProcessStartInfo(
			GameServer server,
			GameInfo blueprint,
			int? downloadThrottleKbps)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(blueprint);

			if (blueprint.RequiresSteamLogin &&
				string.IsNullOrWhiteSpace(server.SteamAccountName))
			{
				throw new InvalidOperationException(
					LocalizationManager.Get("Installer.Error.SteamAccountRequiredForInstall", blueprint.Game));
			}

			bool authenticated = blueprint.RequiresSteamLogin;
			ProcessStartInfo startInfo = new()
			{
				FileName = Core.SteamCmdExe,
				WorkingDirectory = Core.SteamCmdPath,
				UseShellExecute = false,
				RedirectStandardOutput = !authenticated,
				RedirectStandardError = !authenticated,
				CreateNoWindow = !authenticated,
				WindowStyle = authenticated
					? ProcessWindowStyle.Normal
					: ProcessWindowStyle.Hidden
			};

			startInfo.ArgumentList.Add("+force_install_dir");
			startInfo.ArgumentList.Add(server.InstallPath);
			startInfo.ArgumentList.Add("+login");
			startInfo.ArgumentList.Add(authenticated
				? server.SteamAccountName.Trim()
				: "anonymous");
			if (downloadThrottleKbps is > 0)
			{
				startInfo.ArgumentList.Add("+set_download_throttle");
				startInfo.ArgumentList.Add(downloadThrottleKbps.Value.ToString());
				startInfo.ArgumentList.Add("false");
			}
			if (!string.IsNullOrWhiteSpace(blueprint.SteamAppConfig))
			{
				startInfo.ArgumentList.Add("+app_set_config");
				startInfo.ArgumentList.Add(blueprint.SteamAppConfig);
			}
			startInfo.ArgumentList.Add("+app_update");
			startInfo.ArgumentList.Add(blueprint.AppID);
			startInfo.ArgumentList.Add("validate");
			startInfo.ArgumentList.Add("+quit");

			return startInfo;
		}

		internal static int ConvertDownloadLimitToKbps(int megabitsPerSecond)
		{
			return Math.Clamp(megabitsPerSecond, 1, 10000) * 1000;
		}

		private static int? GetConfiguredDownloadThrottleKbps()
		{
			if (!Properties.Settings.Default.LimitSteamCmdDownloadSpeed)
			{
				return null;
			}

			return ConvertDownloadLimitToKbps(
				Properties.Settings.Default.SteamCmdDownloadLimitMbps);
		}

		internal static ProcessStartInfo CreateSteamAuthenticationStartInfo(
			GameServer server,
			GameInfo blueprint)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(blueprint);

			if (!blueprint.RequiresSteamLogin)
			{
				throw new InvalidOperationException(
					LocalizationManager.Get("Installer.Error.AuthorizationNotRequired", blueprint.Game));
			}

			if (string.IsNullOrWhiteSpace(server.SteamAccountName))
			{
				throw new InvalidOperationException(
					LocalizationManager.Get("Installer.Error.SteamAccountRequiredForAuthorization", blueprint.Game));
			}

			ProcessStartInfo startInfo = new()
			{
				FileName = Core.SteamCmdExe,
				WorkingDirectory = Core.SteamCmdPath,
				UseShellExecute = false,
				RedirectStandardOutput = false,
				RedirectStandardError = false,
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Normal
			};

			startInfo.ArgumentList.Add("+login");
			startInfo.ArgumentList.Add(server.SteamAccountName.Trim());
			startInfo.ArgumentList.Add("+quit");

			return startInfo;
		}

		public static int AuthenticateSteamAccount(
			GameServer server,
			GameInfo blueprint,
			Action<string> logCallback,
			Action<int>? onPidStarted = null)
		{
			ProcessStartInfo startInfo;
			try
			{
				startInfo = CreateSteamAuthenticationStartInfo(server, blueprint);
			}
			catch (InvalidOperationException ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.CriticalDetail", ex.Message));
				return 97;
			}

			string consoleLogPath = Path.Combine(
				Core.SteamCmdPath,
				"logs",
				"console_log.txt");
			long consoleLogStart = GetFileLength(consoleLogPath);
			Stopwatch authenticationTimer = Stopwatch.StartNew();

			using Process process = new()
			{
				StartInfo = startInfo
			};

			try
			{
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.CheckingAuthorization", blueprint.Game));
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.AuthorizationInstructions"));

				process.Start();
				onPidStarted?.Invoke(process.Id);

				while (!process.WaitForExit(1000))
				{
					TimeSpan elapsed = authenticationTimer.Elapsed;
					SetMainWindowTitle(
						LocalizationManager.Get(
							"Installer.Window.Authorization",
							elapsed.Minutes,
							elapsed.Seconds));
				}

				string newConsoleLog = ReadFileSince(
					consoleLogPath,
					consoleLogStart);

				if (ContainsSteamFailure(newConsoleLog) || process.ExitCode != 0)
				{
					logCallback?.Invoke(
						LocalizationManager.Get("Installer.Activity.AuthorizationFailed"));
					return 96;
				}

				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.AuthorizationReady", blueprint.Game));
				return 0;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.AuthorizationError", ex.Message));
				return 96;
			}
			finally
			{
				authenticationTimer.Stop();
				SetMainWindowTitle(LocalizationManager.Get("App.Title"));
				Core.Instance.UpdateGridStatus();
			}
		}

		private static int InstallWithSteamAccount(
			GameServer server,
			GameInfo blueprint,
			ProcessStartInfo startInfo,
			Action<string> logCallback,
			Action<int>? onPidStarted)
		{
			string consoleLogPath = Path.Combine(
				Core.SteamCmdPath,
				"logs",
				"console_log.txt");
			long consoleLogStart = GetFileLength(consoleLogPath);
			Stopwatch installTimer = Stopwatch.StartNew();

			using Process process = new()
			{
				StartInfo = startInfo
			};

			try
			{
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.OpeningSteamCmd", blueprint.Game));
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.LoginInstructions"));

				process.Start();
				onPidStarted?.Invoke(process.Id);

				while (!process.WaitForExit(1000))
				{
					TimeSpan elapsed = installTimer.Elapsed;
					SetMainWindowTitle(
						LocalizationManager.Get(
							"Installer.Window.LoginInstall",
							elapsed.Minutes,
							elapsed.Seconds));
				}

				string newConsoleLog = ReadFileSince(
					consoleLogPath,
					consoleLogStart);

				string successMarker =
					$"Success! App '{blueprint.AppID}' fully installed.";
				if (newConsoleLog.Contains(
						successMarker,
						StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke(
						LocalizationManager.Get("Installer.Activity.AuthenticatedDownloadComplete", blueprint.Game));
					return 0;
				}

				if (ContainsSteamFailure(newConsoleLog))
				{
					logCallback?.Invoke(
						LocalizationManager.Get("Installer.Activity.AuthenticatedInstallFailed"));
					return 98;
				}

				if (process.ExitCode != 0)
					return process.ExitCode;

				string installedExecutable = Path.Combine(
					server.InstallPath,
					blueprint.ExeName);
				if (File.Exists(installedExecutable))
				{
					logCallback?.Invoke(
						LocalizationManager.Get("Installer.Activity.ExecutablePresentWithoutCompletion"));
					return 0;
				}

				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.InstallationUnverified"));
				return 98;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(
					LocalizationManager.Get("Installer.Activity.LoginInstallerError", ex.Message));
				return -1;
			}
			finally
			{
				installTimer.Stop();
				SetMainWindowTitle(LocalizationManager.Get("App.Title"));
				Core.Instance.UpdateGridStatus();
			}
		}

		private static long GetFileLength(string filePath)
		{
			try
			{
				return File.Exists(filePath)
					? new FileInfo(filePath).Length
					: 0;
			}
			catch (Exception suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				return 0;
			}
		}

		private static string ReadFileSince(string filePath, long offset)
		{
			try
			{
				using FileStream stream = new(
					filePath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite | FileShare.Delete);

				if (offset > 0 && offset <= stream.Length)
					stream.Seek(offset, SeekOrigin.Begin);

				using StreamReader reader = new(stream, Encoding.UTF8, true);
				return reader.ReadToEnd();
			}
			catch (Exception suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				return string.Empty;
			}
		}

		private static bool ContainsSteamFailure(string output)
		{
			return output.Contains("ERROR!", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("No subscription", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("AppID not found", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("Invalid Password", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("Account Logon Denied", StringComparison.OrdinalIgnoreCase) ||
				output.Contains("Login Failure", StringComparison.OrdinalIgnoreCase);
		}

		private static void SetMainWindowTitle(string title)
		{
			ApplicationUiService.SetMainWindowTitle(title);
		}

		private static async Task<int> InstallDirectDownloadAsync(GameServer server, GameInfo blueprint, Action<string> logCallback)
		{
			string downloadUrl = "";
			string fileName = "";
			int requiredJava = 8;
			string javaExeCmd = "java";
			string javaExecutable = "java";
			string minecraftLoader = MinecraftMetadataService.VanillaLoader;
			string minecraftLoaderVersion = "Official";
			string forgeArtifactVersion = "";
			string neoForgeArtifactVersion = "";
			string expectedDownloadSha1 = "";
			MinecraftMetadataService.MinecraftVersionMetadata? minecraftMetadata = null;
			bool isBedrock = MinecraftControlProfile.IsBedrock(server);

			if (blueprint.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					if (MinecraftControlProfile.IsBedrock(server))
					{
						logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.QueryingBedrock"));
						MinecraftMetadataService.BedrockServerMetadata bedrockMetadata =
							await MinecraftMetadataService.GetBedrockServerMetadataAsync();
						server.MinecraftEdition = MinecraftControlProfile.BedrockEdition;
						server.GameVersion = bedrockMetadata.Version;
						server.MinecraftLoader = MinecraftMetadataService.VanillaLoader;
						server.MinecraftLoaderVersion = "Official";
						server.RequiredJavaVersion = 0;
						downloadUrl = bedrockMetadata.DownloadUri.AbsoluteUri;
						fileName = $"bedrock-server-{bedrockMetadata.Version}.zip";
						logCallback?.Invoke(
							LocalizationManager.Get("Installer.Activity.ResolvedBedrock", bedrockMetadata.Version));
					}
					else
					{
						minecraftLoader = MinecraftMetadataService.NormalizeLoader(server.MinecraftLoader);
						logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.QueryingMinecraftJava", minecraftLoader));
					minecraftMetadata = await MinecraftMetadataService.GetVersionMetadataAsync(server.GameVersion);
					server.MinecraftEdition = MinecraftControlProfile.JavaEdition;
					server.GameVersion = minecraftMetadata.Version;
					requiredJava = minecraftMetadata.JavaMajorVersion;
					server.RequiredJavaVersion = requiredJava;
					server.MinecraftLoader = minecraftLoader;

					minecraftLoaderVersion = await MinecraftMetadataService.ResolveLoaderVersionAsync(
						minecraftLoader,
						server.GameVersion,
						server.MinecraftLoaderVersion);
					server.MinecraftLoaderVersion = minecraftLoaderVersion;

					if (minecraftLoader == MinecraftMetadataService.FabricLoader)
					{
						downloadUrl = (await MinecraftMetadataService.GetFabricServerJarUriAsync(
							server.GameVersion,
							minecraftLoaderVersion)).AbsoluteUri;
						fileName = "server.jar";
					}
					else if (minecraftLoader == MinecraftMetadataService.ForgeLoader)
					{
						Uri forgeInstallerUri = await MinecraftMetadataService.GetForgeInstallerUriAsync(
							server.GameVersion,
							minecraftLoaderVersion);
						downloadUrl = forgeInstallerUri.AbsoluteUri;
						forgeArtifactVersion = Uri.UnescapeDataString(
							forgeInstallerUri.Segments[^2].TrimEnd('/'));
						fileName = "forge-installer.jar";
						try
						{
							expectedDownloadSha1 = (await _httpClient.GetStringAsync(downloadUrl + ".sha1"))
								.Trim()
								.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];
						}
						catch (Exception suppressedException)
						{
							ApplicationLogService.WriteSuppressedException(suppressedException);
							logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ForgeChecksumUnavailable"));
						}
					}
					else if (minecraftLoader == MinecraftMetadataService.NeoForgeLoader)
					{
						Uri neoForgeInstallerUri = await MinecraftMetadataService.GetNeoForgeInstallerUriAsync(
							server.GameVersion,
							minecraftLoaderVersion);
						downloadUrl = neoForgeInstallerUri.AbsoluteUri;
						neoForgeArtifactVersion = Uri.UnescapeDataString(
							neoForgeInstallerUri.Segments[^2].TrimEnd('/'));
						fileName = "neoforge-installer.jar";
						try
						{
							expectedDownloadSha1 = (await _httpClient.GetStringAsync(downloadUrl + ".sha1"))
								.Trim()
								.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];
						}
						catch (Exception suppressedException)
						{
							ApplicationLogService.WriteSuppressedException(suppressedException);
							logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.NeoForgeChecksumUnavailable"));
						}
					}
					else
					{
						downloadUrl = minecraftMetadata.ServerDownloadUrl;
						expectedDownloadSha1 = minecraftMetadata.ServerSha1;
						fileName = "server.jar";
					}

					logCallback?.Invoke(
						LocalizationManager.Get(
							"Installer.Activity.ResolvedMinecraftJava",
							server.GameVersion,
							minecraftLoader,
							minecraftLoaderVersion,
							requiredJava));

					}
				}
				catch (Exception ex)
				{
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.MetadataResolutionFailed", ex.Message));
					return -1;
				}
			}
			else if (!string.IsNullOrWhiteSpace(blueprint.DownloadUrl))
			{
				downloadUrl = blueprint.DownloadUrl;
				fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
				if (string.IsNullOrWhiteSpace(fileName)) fileName = "server_files.zip";

				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
				{
					requiredJava = 21;
				}
			}
			else
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.DownloadUrlMissing", blueprint.Game));
				return 1;
			}

			string fullFilePath = "";
			try
			{
				if (!Directory.Exists(server.InstallPath))
					Directory.CreateDirectory(server.InstallPath);

				fullFilePath = Path.Combine(server.InstallPath, fileName);
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.DownloadStarting", fileName));

				using (HttpRequestMessage request = new(HttpMethod.Get, downloadUrl))
				{
					request.Version = new Version(1, 1);
					using HttpResponseMessage response = await _httpClient.SendAsync(
						request,
						HttpCompletionOption.ResponseHeadersRead);
					response.EnsureSuccessStatusCode();
					long? totalBytes = response.Content.Headers.ContentLength;

					await using Stream contentStream = await response.Content.ReadAsStreamAsync();
					await using FileStream fileStream = new(
						fullFilePath,
						FileMode.Create,
						FileAccess.Write,
						FileShare.None);
					byte[] buffer = new byte[8192];
					long totalRead = 0;
					int bytesRead;
					int lastReportedPercent = -1;

					while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
					{
						await fileStream.WriteAsync(buffer, 0, bytesRead);
						totalRead += bytesRead;

						if (totalBytes.HasValue)
						{
							int percent = (int)((double)totalRead / totalBytes.Value * 100);
							if (percent > lastReportedPercent)
							{
								logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.Downloading", fileName, percent));
								lastReportedPercent = percent;
							}
						}
					}
				}

				if (minecraftMetadata != null &&
					minecraftLoader == MinecraftMetadataService.VanillaLoader &&
					minecraftMetadata.ServerSize > 0 &&
					new FileInfo(fullFilePath).Length != minecraftMetadata.ServerSize)
				{
					File.Delete(fullFilePath);
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.MinecraftSizeMismatch"));
					return -1;
				}

				if (!string.IsNullOrWhiteSpace(expectedDownloadSha1) &&
					!VerifyFileSha1(fullFilePath, expectedDownloadSha1))
				{
					File.Delete(fullFilePath);
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ChecksumFailed"));
					return -1;
				}

				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.DownloadComplete", fullFilePath));
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.DownloadError", ex.Message));
				return -1;
			}

			try
			{
				if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.Extracting", fileName));

					if (isBedrock)
						ExtractBedrockArchive(fullFilePath, server.InstallPath);
					else
						System.IO.Compression.ZipFile.ExtractToDirectory(fullFilePath, server.InstallPath, overwriteFiles: true);

					File.Delete(fullFilePath);
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ExtractionComplete"));
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ExtractionFailed", ex.Message));
				return -1;
			}

			try
			{
				if (isBedrock &&
					!File.Exists(Path.Combine(
						server.InstallPath,
						MinecraftControlProfile.BedrockExecutableName)))
				{
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.BedrockExecutableMissing"));
					return -1;
				}

				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase) && !isBedrock)
				{
					string runtimeFolder = Path.Combine(Core.RuntimesPath, $"Java{requiredJava}");

					string[] existingExecutables = Directory.Exists(runtimeFolder)
						? Directory.GetFiles(runtimeFolder, "java.exe", SearchOption.AllDirectories)
						: Array.Empty<string>();

					if (existingExecutables.Length > 0)
					{
						javaExecutable = existingExecutables[0];
						javaExeCmd = QuoteCommandArgument(javaExecutable);
						logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.UsingCachedJava", requiredJava));
					}
					else
					{
						int systemJava = Core.GetSystemJavaVersion();

						if (systemJava < requiredJava)
						{
							string javaStatus = systemJava == 0
								? LocalizationManager.Get("Installer.JavaStatus.None")
								: LocalizationManager.Get("Installer.JavaStatus.Version", systemJava);

							System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.No;

							if (ApplicationUiService.IsAvailable)
							{
								result = ApplicationUiService.Invoke(() =>
									LocalizedMessageBox.Show(
										ApplicationUiService.DialogOwner,
										LocalizationManager.Get(
											"Installer.JavaMismatch.Body",
											requiredJava,
											javaStatus),
										LocalizationManager.Get(
											"Installer.JavaMismatch.Title"),
										System.Windows.Forms.MessageBoxButtons.YesNo,
										System.Windows.Forms.MessageBoxIcon.Question));
							}

							if (result == System.Windows.Forms.DialogResult.Yes)
							{
								logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.DownloadingJava", requiredJava));

								string jreUrl = $"https://api.adoptium.net/v3/binary/latest/{requiredJava}/ga/windows/x64/jre/hotspot/normal/eclipse?project=jdk";
								string zipPath = Path.Combine(Core.RuntimesPath, $"java{requiredJava}_temp.zip");
								Directory.CreateDirectory(runtimeFolder);

								using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, jreUrl))
								{
									req.Version = new Version(1, 1);
									using (HttpResponseMessage resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead))
									{
										resp.EnsureSuccessStatusCode();
										using (Stream cStream = await resp.Content.ReadAsStreamAsync())
										using (FileStream fStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
										{
											await cStream.CopyToAsync(fStream);
										}
									}
								}

								logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ExtractingJava"));
								System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, runtimeFolder, overwriteFiles: true);
								File.Delete(zipPath);

								string[] newlyExtracted = Directory.GetFiles(runtimeFolder, "java.exe", SearchOption.AllDirectories);
								if (newlyExtracted.Length > 0)
								{
									javaExecutable = newlyExtracted[0];
									javaExeCmd = QuoteCommandArgument(javaExecutable);
									logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.JavaInstalled"));
								}
							}
							else
							{
								logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.JavaSkipped"));
							}
						}
					}

					if (minecraftLoader == MinecraftMetadataService.ForgeLoader)
					{
						if (Core.GetSystemJavaVersion() < requiredJava &&
							javaExecutable.Equals("java", StringComparison.OrdinalIgnoreCase))
						{
							logCallback?.Invoke(
								LocalizationManager.Get("Installer.Activity.ForgeJavaRequired", requiredJava));
							return -1;
						}

						int forgeResult = await InstallForgeServerAsync(
							server,
							javaExecutable,
							fullFilePath,
							forgeArtifactVersion,
							logCallback!);
						if (forgeResult != 0)
							return forgeResult;
					}
					else if (minecraftLoader == MinecraftMetadataService.NeoForgeLoader)
					{
						if (Core.GetSystemJavaVersion() < requiredJava &&
							javaExecutable.Equals("java", StringComparison.OrdinalIgnoreCase))
						{
							logCallback?.Invoke(
								LocalizationManager.Get("Installer.Activity.NeoForgeJavaRequired", requiredJava));
							return -1;
						}

						int neoForgeResult = await InstallNeoForgeServerAsync(
							server,
							javaExecutable,
							fullFilePath,
							neoForgeArtifactVersion,
							logCallback!);
						if (neoForgeResult != 0)
							return neoForgeResult;
					}

					if (minecraftLoader != MinecraftMetadataService.VanillaLoader)
					{
						Directory.CreateDirectory(Path.Combine(server.InstallPath, "mods"));
						logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ModsFolderReady"));
					}

					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.GeneratingLauncher", minecraftLoader));
					string batPath = Path.Combine(server.InstallPath, "Start.bat");

					string launchCommand = minecraftLoader switch
					{
						MinecraftMetadataService.ForgeLoader =>
							BuildForgeLaunchCommand(server, javaExeCmd, forgeArtifactVersion),
						MinecraftMetadataService.NeoForgeLoader =>
							BuildNeoForgeLaunchCommand(server, javaExeCmd, neoForgeArtifactVersion),
						_ => $"{javaExeCmd} %*"
					};
					File.WriteAllText(
						batPath,
						$"@echo off\r\n{launchCommand}\r\nexit /b %errorlevel%\r\n");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.PostInstallFailed", ex.Message));
				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
					return -1;
			}

			Core.Instance.UpdateGridStatus();
			return 0;
		}

		private static void ExtractBedrockArchive(string archivePath, string installPath)
		{
			Dictionary<string, byte[]> protectedFiles = new(StringComparer.OrdinalIgnoreCase);
			foreach (string relativePath in new[]
			{
				"server.properties",
				"allowlist.json",
				"permissions.json"
			})
			{
				string fullPath = Path.Combine(installPath, relativePath);
				if (File.Exists(fullPath))
					protectedFiles[relativePath] = File.ReadAllBytes(fullPath);
			}

			System.IO.Compression.ZipFile.ExtractToDirectory(
				archivePath,
				installPath,
				overwriteFiles: true);
			foreach ((string relativePath, byte[] content) in protectedFiles)
				File.WriteAllBytes(Path.Combine(installPath, relativePath), content);
		}

		private static async Task<int> InstallForgeServerAsync(
			GameServer server,
			string javaExecutable,
			string installerPath,
			string forgeArtifactVersion,
			Action<string> logCallback)
		{
			if (!File.Exists(installerPath))
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ForgeInstallerMissing"));
				return -1;
			}

			logCallback?.Invoke(
				LocalizationManager.Get("Installer.Activity.InstallingForge", server.MinecraftLoaderVersion, server.GameVersion));
			ProcessStartInfo startInfo = new()
			{
				FileName = javaExecutable,
				WorkingDirectory = server.InstallPath,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			startInfo.ArgumentList.Add("-jar");
			startInfo.ArgumentList.Add(installerPath);
			startInfo.ArgumentList.Add("--installServer");

			try
			{
				using Process installer = new() { StartInfo = startInfo };
				if (!installer.Start())
					throw new InvalidOperationException(LocalizationManager.Get("Installer.Error.ForgeStart"));

				Task<string> outputTask = installer.StandardOutput.ReadToEndAsync();
				Task<string> errorTask = installer.StandardError.ReadToEndAsync();
				using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(10));
				try
				{
					await installer.WaitForExitAsync(timeout.Token);
				}
				catch (OperationCanceledException)
				{
					try { installer.Kill(entireProcessTree: true); } catch (Exception suppressedException) { Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException); }
					throw new TimeoutException(LocalizationManager.Get("Installer.Error.ForgeTimeout"));
				}

				string output = await outputTask;
				string errors = await errorTask;
				LogProcessOutput(output, "FORGE", logCallback!);
				LogProcessOutput(errors, "FORGE", logCallback!);

				if (installer.ExitCode != 0)
				{
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ForgeExitCode", installer.ExitCode));
					return installer.ExitCode;
				}

				_ = BuildForgeLaunchCommand(server, QuoteCommandArgument(javaExecutable), forgeArtifactVersion);
				try { File.Delete(installerPath); } catch (Exception suppressedException) { Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException); }
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ForgeInstalled"));
				return 0;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.ForgeFailed", ex.Message));
				return -1;
			}
		}

		private static async Task<int> InstallNeoForgeServerAsync(
			GameServer server,
			string javaExecutable,
			string installerPath,
			string neoForgeArtifactVersion,
			Action<string> logCallback)
		{
			if (!File.Exists(installerPath))
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.NeoForgeInstallerMissing"));
				return -1;
			}

			logCallback?.Invoke(
				LocalizationManager.Get("Installer.Activity.InstallingNeoForge", server.MinecraftLoaderVersion, server.GameVersion));
			ProcessStartInfo startInfo = new()
			{
				FileName = javaExecutable,
				WorkingDirectory = server.InstallPath,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			startInfo.ArgumentList.Add("-jar");
			startInfo.ArgumentList.Add(installerPath);
			startInfo.ArgumentList.Add("--installServer");

			try
			{
				using Process installer = new() { StartInfo = startInfo };
				if (!installer.Start())
					throw new InvalidOperationException(LocalizationManager.Get("Installer.Error.NeoForgeStart"));

				Task<string> outputTask = installer.StandardOutput.ReadToEndAsync();
				Task<string> errorTask = installer.StandardError.ReadToEndAsync();
				using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(10));
				try
				{
					await installer.WaitForExitAsync(timeout.Token);
				}
				catch (OperationCanceledException)
				{
					try { installer.Kill(entireProcessTree: true); } catch (Exception suppressedException) { Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException); }
					throw new TimeoutException(LocalizationManager.Get("Installer.Error.NeoForgeTimeout"));
				}

				LogProcessOutput(await outputTask, "NEOFORGE", logCallback!);
				LogProcessOutput(await errorTask, "NEOFORGE", logCallback!);
				if (installer.ExitCode != 0)
				{
					logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.NeoForgeExitCode", installer.ExitCode));
					return installer.ExitCode;
				}

				_ = BuildNeoForgeLaunchCommand(
					server,
					QuoteCommandArgument(javaExecutable),
					neoForgeArtifactVersion);
				try { File.Delete(installerPath); } catch (Exception suppressedException) { Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException); }
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.NeoForgeInstalled"));
				return 0;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("Installer.Activity.NeoForgeFailed", ex.Message));
				return -1;
			}
		}

		private static string BuildNeoForgeLaunchCommand(
			GameServer server,
			string javaExeCmd,
			string neoForgeArtifactVersion)
		{
			string argsPath = Path.Combine(
				server.InstallPath,
				"libraries",
				"net",
				"neoforged",
				"neoforge",
				neoForgeArtifactVersion,
				"win_args.txt");
			if (!File.Exists(argsPath))
			{
				throw new FileNotFoundException(
					LocalizationManager.Get("Installer.Error.NeoForgeArgumentsMissing"),
					argsPath);
			}

			string relativeArgsPath = Path.GetRelativePath(server.InstallPath, argsPath);
			return $"{javaExeCmd} %* @\"{relativeArgsPath}\" nogui";
		}

		private static string BuildForgeLaunchCommand(
			GameServer server,
			string javaExeCmd,
			string forgeArtifactVersion)
		{
			string modernArgsPath = Path.Combine(
				server.InstallPath,
				"libraries",
				"net",
				"minecraftforge",
				"forge",
				forgeArtifactVersion,
				"win_args.txt");

			if (File.Exists(modernArgsPath))
			{
				string relativeArgsPath = Path.GetRelativePath(server.InstallPath, modernArgsPath);
				return $"{javaExeCmd} %* @\"{relativeArgsPath}\" nogui";
			}

			string? legacyJar = Directory
				.EnumerateFiles(server.InstallPath, "forge-*.jar", SearchOption.TopDirectoryOnly)
				.Where(path => !path.EndsWith("-installer.jar", StringComparison.OrdinalIgnoreCase))
				.OrderByDescending(path => Path.GetFileName(path).Contains(
					forgeArtifactVersion,
					StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (legacyJar != null)
				return $"{javaExeCmd} %* -jar {QuoteCommandArgument(Path.GetFileName(legacyJar))} nogui";

			throw new InvalidOperationException(
				LocalizationManager.Get("Installer.Error.ForgeLauncherMissing"));
		}

		private static void LogProcessOutput(
			string output,
			string source,
			Action<string> logCallback)
		{
			foreach (string rawLine in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
			{
				string line = rawLine.Trim();
				if (line.Length > 0)
					logCallback?.Invoke($"[{source}] {line}");
			}
		}

		private static string QuoteCommandArgument(string value)
		{
			return $"\"{value.Replace("\"", "\"\"")}\"";
		}

		private static bool VerifyFileSha1(string filePath, string expectedSha1)
		{
			using FileStream stream = File.OpenRead(filePath);
			string actualSha1 = Convert.ToHexString(SHA1.HashData(stream));
			return actualSha1.Equals(expectedSha1.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		private static async Task PumpStreamAsync(StreamReader reader, Action<string> queueLine)
		{
			Stream stream = reader.BaseStream;
			byte[] buffer = new byte[256];
			StringBuilder pending = new();
			bool previousWasCarriageReturn = false;

			while (true)
			{
				int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
				if (bytesRead == 0)
					break;

				string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

				foreach (char character in chunk)
				{
					if (character == '\r')
					{
						FlushPending(pending, queueLine);
						previousWasCarriageReturn = true;
						continue;
					}

					if (character == '\n')
					{
						if (!previousWasCarriageReturn)
							FlushPending(pending, queueLine);

						previousWasCarriageReturn = false;
						continue;
					}

					previousWasCarriageReturn = false;
					pending.Append(character);
				}
			}

			FlushPending(pending, queueLine);
		}

		private static void FlushPending(
			StringBuilder pending,
			Action<string> queueLine)
		{
			if (pending.Length == 0)
				return;

			string line = pending.ToString();
			pending.Clear();
			queueLine(line);
		}

		public static string GetSteamError(int code)
		{
			return code switch
			{
				0 => LocalizationManager.Get("Installer.SteamError.Success"),
				96 => LocalizationManager.Get("Installer.SteamError.AuthorizationFailed"),
				97 => LocalizationManager.Get("Installer.SteamError.AccountRequired"),
				98 => LocalizationManager.Get("Installer.SteamError.LoginFailed"),
				99 => LocalizationManager.Get("Installer.SteamError.AppFailure"),
				5 => LocalizationManager.Get("Installer.SteamError.InvalidArguments"),
				7 => LocalizationManager.Get("Installer.SteamError.DiskFull"),
				8 => LocalizationManager.Get("Installer.SteamError.NetworkLost"),
				_ => LocalizationManager.Get("Installer.SteamError.Code", code)
			};
		}
	}
}
