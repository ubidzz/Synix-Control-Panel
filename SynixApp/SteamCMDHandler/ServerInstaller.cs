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

			if (blueprint.AppID == "0" || blueprint.AppID.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				return InstallDirectDownloadAsync(server, blueprint, logCallback).GetAwaiter().GetResult();
			}

			int hasInternalError = 0;
			string lastLoggedLine = "";
			object lineSync = new();

			ProcessStartInfo startInfo = new()
			{
				FileName = Core.SteamCmdExe,
				Arguments =
					$"+force_install_dir \"{server.InstallPath}\" " +
					$"+login anonymous " +
					$"+app_update {blueprint.AppID} validate +quit",
				WorkingDirectory = Core.SteamCmdPath,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

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
					catch
					{

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
				MainGUI? mainWindow = MainGUI.Instance;

				if (mainWindow == null ||
					mainWindow.IsDisposed ||
					!mainWindow.IsHandleCreated)
				{
					return;
				}

				try
				{
					mainWindow.BeginInvoke(new Action(() =>
					{
						if (!mainWindow.IsDisposed)
							mainWindow.Text = title;
					}));
				}
				catch (InvalidOperationException)
				{

				}
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
								"Synix Control Panel - Working... " +
								$"[{elapsed.Minutes:D2}m " +
								$"{elapsed.Seconds:D2}s]");
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
					$"[CRITICAL] Launcher Error: {ex.Message}");

				resultCode = -1;
			}
			finally
			{

				heartbeatCts.Cancel();

				try
				{
					heartbeatTask.GetAwaiter().GetResult();
				}
				catch (OperationCanceledException)
				{

				}
				catch (Exception ex)
				{
					logQueue.Writer.TryWrite(
						$"[WARNING] Heartbeat cleanup failed: {ex.Message}");
				}

				installTimer.Stop();
				heartbeatCts.Dispose();

				logQueue.Writer.TryComplete();

				try
				{
					dashboardWriter.GetAwaiter().GetResult();
				}
				catch
				{

				}

				SetMainWindowTitle("Synix Control Panel");
				Core.Instance.UpdateGridStatus();
			}

			return resultCode;
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
			string expectedDownloadSha1 = "";
			MinecraftMetadataService.MinecraftVersionMetadata? minecraftMetadata = null;

			if (blueprint.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				minecraftLoader = MinecraftMetadataService.NormalizeLoader(server.MinecraftLoader);
				logCallback?.Invoke($"Querying official metadata for Minecraft {minecraftLoader}...");
				try
				{
					minecraftMetadata = await MinecraftMetadataService.GetVersionMetadataAsync(server.GameVersion);
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
						catch
						{
							logCallback?.Invoke("[WARNING] Forge checksum metadata was unavailable; HTTPS transport validation remains active.");
						}
					}
					else
					{
						downloadUrl = minecraftMetadata.ServerDownloadUrl;
						expectedDownloadSha1 = minecraftMetadata.ServerSha1;
						fileName = "server.jar";
					}

					logCallback?.Invoke(
						$"Resolved Minecraft {server.GameVersion}, {minecraftLoader} {minecraftLoaderVersion}, Java {requiredJava}.");
				}
				catch (Exception ex)
				{
					logCallback?.Invoke($"[CRITICAL] Failed to resolve official Minecraft metadata: {ex.Message}");
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
				logCallback?.Invoke($"[ERROR] Non-Steam game '{blueprint.Game}' is missing a DownloadUrl.");
				return 1;
			}

			string fullFilePath = "";
			try
			{
				if (!Directory.Exists(server.InstallPath))
					Directory.CreateDirectory(server.InstallPath);

				fullFilePath = Path.Combine(server.InstallPath, fileName);
				logCallback?.Invoke($"Starting download: {fileName}...");

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
								logCallback?.Invoke($"Downloading {fileName}... {percent}%");
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
					logCallback?.Invoke("[CRITICAL] Minecraft server download size did not match Mojang metadata.");
					return -1;
				}

				if (!string.IsNullOrWhiteSpace(expectedDownloadSha1) &&
					!VerifyFileSha1(fullFilePath, expectedDownloadSha1))
				{
					File.Delete(fullFilePath);
					logCallback?.Invoke("[CRITICAL] Download checksum validation failed. The file was deleted.");
					return -1;
				}

				logCallback?.Invoke($"Download complete! Saved to {fullFilePath}");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Download Error: {ex.Message}");
				return -1;
			}

			try
			{
				if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke($"[SYSTEM] Unzipping {fileName} into server directory... Please wait.");

					System.IO.Compression.ZipFile.ExtractToDirectory(fullFilePath, server.InstallPath, overwriteFiles: true);

					File.Delete(fullFilePath);
					logCallback?.Invoke("[SYSTEM] Extraction complete. Temporary archive deleted.");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Failed to extract archive: {ex.Message}");
				return -1;
			}

			try
			{
				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
				{
					string runtimeFolder = Path.Combine(Core.RuntimesPath, $"Java{requiredJava}");

					string[] existingExecutables = Directory.Exists(runtimeFolder)
						? Directory.GetFiles(runtimeFolder, "java.exe", SearchOption.AllDirectories)
						: Array.Empty<string>();

					if (existingExecutables.Length > 0)
					{
						javaExecutable = existingExecutables[0];
						javaExeCmd = QuoteCommandArgument(javaExecutable);
						logCallback?.Invoke($"[SYSTEM] Using previously cached Portable Java {requiredJava}.");
					}
					else
					{
						int systemJava = Core.GetSystemJavaVersion();

						if (systemJava < requiredJava)
						{
							string javaStatus = systemJava == 0 ? "no Java installed" : $"Java {systemJava}";

							System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.No;

							MainGUI.Instance?.Invoke((Action)(() =>
							{
								result = System.Windows.Forms.MessageBox.Show(
									MainGUI.Instance,
									$"This server requires Java {requiredJava}, but your system has {javaStatus}.\n\nWould you like Synix to automatically download a portable Java {requiredJava} runtime specifically for this server?\n\n(This is completely safe and will not change your computer's global Java settings).",
									"Java Version Mismatch",
									System.Windows.Forms.MessageBoxButtons.YesNo,
									System.Windows.Forms.MessageBoxIcon.Question);
							}));

							if (result == System.Windows.Forms.DialogResult.Yes)
							{
								logCallback?.Invoke($"[SYSTEM] Downloading Portable Java {requiredJava} (Eclipse Temurin JRE)...");

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

								logCallback?.Invoke("[SYSTEM] Extracting Portable Java environment...");
								System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, runtimeFolder, overwriteFiles: true);
								File.Delete(zipPath);

								string[] newlyExtracted = Directory.GetFiles(runtimeFolder, "java.exe", SearchOption.AllDirectories);
								if (newlyExtracted.Length > 0)
								{
									javaExecutable = newlyExtracted[0];
									javaExeCmd = QuoteCommandArgument(javaExecutable);
									logCallback?.Invoke("[SYSTEM] Portable Java installed successfully!");
								}
							}
							else
							{
								logCallback?.Invoke("[WARNING] Java download skipped by user. The server will likely crash on startup.");
							}
						}
					}

					if (minecraftLoader == MinecraftMetadataService.ForgeLoader)
					{
						if (Core.GetSystemJavaVersion() < requiredJava &&
							javaExecutable.Equals("java", StringComparison.OrdinalIgnoreCase))
						{
							logCallback?.Invoke(
								$"[CRITICAL] Forge installation requires Java {requiredJava}. Portable Java installation was not completed.");
							return -1;
						}

						int forgeResult = await InstallForgeServerAsync(
							server,
							javaExecutable,
							fullFilePath,
							forgeArtifactVersion,
							logCallback);
						if (forgeResult != 0)
							return forgeResult;
					}

					if (minecraftLoader != MinecraftMetadataService.VanillaLoader)
					{
						Directory.CreateDirectory(Path.Combine(server.InstallPath, "mods"));
						logCallback?.Invoke("[MINECRAFT] Mods folder is ready. Synix leaves mod selection and installation to the user.");
					}

					logCallback?.Invoke($"[SYSTEM] Generating Minecraft {minecraftLoader} Start.bat bootstrapper...");
					string batPath = Path.Combine(server.InstallPath, "Start.bat");

					string launchCommand = minecraftLoader == MinecraftMetadataService.ForgeLoader
						? BuildForgeLaunchCommand(server, javaExeCmd, forgeArtifactVersion)
						: $"{javaExeCmd} %*";
					File.WriteAllText(
						batPath,
						$"@echo off\r\n{launchCommand}\r\nexit /b %errorlevel%\r\n");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[WARNING] Failed to generate post-install files: {ex.Message}");
				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
					return -1;
			}

			Core.Instance.UpdateGridStatus();
			return 0;
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
				logCallback?.Invoke("[CRITICAL] The downloaded Forge installer is missing.");
				return -1;
			}

			logCallback?.Invoke(
				$"[FORGE] Installing Forge {server.MinecraftLoaderVersion} for Minecraft {server.GameVersion}...");
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
					throw new InvalidOperationException("Windows could not start the Forge installer.");

				Task<string> outputTask = installer.StandardOutput.ReadToEndAsync();
				Task<string> errorTask = installer.StandardError.ReadToEndAsync();
				using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(10));
				try
				{
					await installer.WaitForExitAsync(timeout.Token);
				}
				catch (OperationCanceledException)
				{
					try { installer.Kill(entireProcessTree: true); } catch { }
					throw new TimeoutException("The Forge installer did not finish within 10 minutes.");
				}

				string output = await outputTask;
				string errors = await errorTask;
				LogProcessOutput(output, "FORGE", logCallback);
				LogProcessOutput(errors, "FORGE", logCallback);

				if (installer.ExitCode != 0)
				{
					logCallback?.Invoke($"[CRITICAL] Forge installer exited with code {installer.ExitCode}.");
					return installer.ExitCode;
				}

				_ = BuildForgeLaunchCommand(server, QuoteCommandArgument(javaExecutable), forgeArtifactVersion);
				try { File.Delete(installerPath); } catch { }
				logCallback?.Invoke("[FORGE] Server loader installed successfully.");
				return 0;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Forge installation failed: {ex.Message}");
				return -1;
			}
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
				"Forge completed but no Windows argument file or legacy Forge server jar was created.");
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
				0 => "Success",
				99 => "Steam Error: AppID not found, no subscription, or SteamCMD reported a failure.",
				5 => "Invalid Arguments",
				7 => "Disk Space Full",
				8 => "Network Connection Lost",
				_ => $"SteamCMD Failure (Code: {code})"
			};
		}
	}
}
