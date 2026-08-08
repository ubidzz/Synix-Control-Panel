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
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Synix_Control_Panel.SynixApp.SteamCMDHandler
{
	public static class ServerInstaller
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public static int Install(GameServer server, GameInfo blueprint, Action<string> logCallback, Action<int>? onPidStarted = null)
		{
			// --------------------------------------------------------
			// PHASE 1: DIRECT DOWNLOAD ROUTING (NON-STEAM GAMES)
			// --------------------------------------------------------
			if (blueprint.AppID == "0" || blueprint.AppID.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				return InstallDirectDownloadAsync(server, blueprint, logCallback).GetAwaiter().GetResult();
			}

			// --------------------------------------------------------
			// PHASE 2: STANDARD STEAMCMD INSTALLATION
			// --------------------------------------------------------
			int hasInternalError = 0;
			string lastLoggedLine = "";
			object lineSync = new();

			ProcessStartInfo startInfo = new()
			{
				FileName = @"C:\Synix\SteamCMD\steamcmd.exe",
				// Map directly to the object properties
				Arguments = $"+force_install_dir \"{server.InstallPath}\" +login anonymous +app_update {blueprint.AppID} validate +quit",
				WorkingDirectory = @"C:\Synix\SteamCMD",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			using Process process = new() { StartInfo = startInfo };

			// The stream readers must never wait for Core.Log/MainGUI.AppendLog.
			// They only place complete SteamCMD messages in this queue.
			Channel<string> logQueue = Channel.CreateUnbounded<string>(
				new UnboundedChannelOptions
				{
					SingleReader = true,
					SingleWriter = false,
					AllowSynchronousContinuations = false
				});

			Task dashboardWriter = Task.Run(async () =>
			{
				await foreach (string line in logQueue.Reader.ReadAllAsync())
				{
					try
					{
						// Keep the existing Core.Log -> MainGUI.AppendLog chain unchanged.
						logCallback?.Invoke(line);
					}
					catch
					{
						// A dashboard logging failure must not stop SteamCMD output capture.
					}
				}
			});

			void QueueSteamLine(string text)
			{
				string line = text.Trim();

				if (line.Length == 0)
					return;

				if (line.Contains("ERROR!", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("subscription", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("AppID not found", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("FAILED", StringComparison.OrdinalIgnoreCase))
				{
					Interlocked.Exchange(ref hasInternalError, 1);
				}

				lock (lineSync)
				{
					if (line.Equals(lastLoggedLine, StringComparison.Ordinal))
						return;

					lastLoggedLine = line;
				}

				logQueue.Writer.TryWrite(line);
			}

			try
			{
				process.Start();
				onPidStarted?.Invoke(process.Id);

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

				logQueue.Writer.TryComplete();
				dashboardWriter.GetAwaiter().GetResult();

				Core.Instance.UpdateGridStatus();

				return Volatile.Read(ref hasInternalError) == 1
					? 99
					: process.ExitCode;
			}
			catch (Exception ex)
			{
				logQueue.Writer.TryWrite(
					$"[CRITICAL] Launcher Error: {ex.Message}");

				logQueue.Writer.TryComplete();

				try
				{
					dashboardWriter.GetAwaiter().GetResult();
				}
				catch
				{
					// Preserve the original launcher error result.
				}

				return -1;
			}
		}

		// --------------------------------------------------------
		// NEW: DIRECT DOWNLOAD ENGINE FOR NON-STEAM GAMES
		// --------------------------------------------------------
		private static async Task<int> InstallDirectDownloadAsync(GameServer server, GameInfo blueprint, Action<string> logCallback)
		{
			string downloadUrl = "";
			string fileName = "";
			int requiredJava = 8; // Default fallback for older Minecraft versions
			string javaExeCmd = "java"; // Defaults to the system's global Java

			// ========================================================
			// 1. THE ROUTER: DETERMINE THE TARGET URL
			// ========================================================
			// ---> CHANGE 1: Use 'Equals' so only Vanilla hits the Mojang API <---
			if (blueprint.Game.Equals("Minecraft Java", StringComparison.OrdinalIgnoreCase))
			{
				logCallback?.Invoke("Querying Mojang API for the latest Vanilla Java version...");
				try
				{
					string manifestJson = await _httpClient.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
					var manifestNode = System.Text.Json.Nodes.JsonNode.Parse(manifestJson);

					string targetVersion = manifestNode?["latest"]?["release"]?.ToString() ?? "";
					logCallback?.Invoke($"Resolved latest Minecraft version: {targetVersion}");

					string versionUrl = "";
					var versionsArray = manifestNode?["versions"]?.AsArray();
					if (versionsArray != null)
					{
						foreach (var version in versionsArray)
						{
							if (version?["id"]?.ToString() == targetVersion)
							{
								versionUrl = version?["url"]?.ToString() ?? "";
								break;
							}
						}
					}

					if (string.IsNullOrEmpty(versionUrl)) throw new Exception("Version not found in Mojang manifest.");

					string versionJson = await _httpClient.GetStringAsync(versionUrl);
					var versionNode = System.Text.Json.Nodes.JsonNode.Parse(versionJson);
					downloadUrl = versionNode?["downloads"]?["server"]?["url"]?.ToString() ?? "";
					fileName = "server.jar";

					// PULL REQUIRED JAVA VERSION FROM MOJANG
					if (versionNode?["javaVersion"]?["majorVersion"] != null)
					{
						requiredJava = (int)versionNode["javaVersion"]["majorVersion"];
					}
					logCallback?.Invoke($"Target Minecraft version requires Java {requiredJava}.");
				}
				catch (Exception ex)
				{
					logCallback?.Invoke($"[CRITICAL] Failed to fetch Mojang API data: {ex.Message}");
					return -1;
				}
			}
			else if (!string.IsNullOrWhiteSpace(blueprint.DownloadUrl))
			{
				downloadUrl = blueprint.DownloadUrl;
				fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
				if (string.IsNullOrWhiteSpace(fileName)) fileName = "server_files.zip";

				// ---> CHANGE 2: Set modern Java 21 default for Modded Minecraft <---
				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
				{
					requiredJava = 21; // Forge/Fabric 1.20+ requires Java 21
				}
			}
			else
			{
				logCallback?.Invoke($"[ERROR] Non-Steam game '{blueprint.Game}' is missing a DownloadUrl.");
				return 1;
			}

			// ========================================================
			// 2. HTTP DOWNLOADER (WITH PROGRESS LOGGING)
			// ========================================================
			string fullFilePath = "";
			try
			{
				if (!Directory.Exists(server.InstallPath))
					Directory.CreateDirectory(server.InstallPath);

				fullFilePath = Path.Combine(server.InstallPath, fileName);
				logCallback?.Invoke($"Starting download: {fileName}...");

				// Use HttpRequestMessage to explicitly force HTTP/1.1
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, downloadUrl))
				{
					request.Version = new Version(1, 1);

					using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
					{
						response.EnsureSuccessStatusCode();
						long? totalBytes = response.Content.Headers.ContentLength;

						using (Stream contentStream = await response.Content.ReadAsStreamAsync())
						using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
						{
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
					}
				}
				logCallback?.Invoke($"Download complete! Saved to {fullFilePath}");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Download Error: {ex.Message}");
				return -1;
			}

			// ========================================================
			// 3. POST-DOWNLOAD AUTO-EXTRACTION
			// ========================================================
			try
			{
				if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke($"[SYSTEM] Unzipping {fileName} into server directory... Please wait.");

					// Extract all files directly into the InstallPath, overwriting existing files
					System.IO.Compression.ZipFile.ExtractToDirectory(fullFilePath, server.InstallPath, overwriteFiles: true);

					// Clean up the massive .zip file to save user disk space
					File.Delete(fullFilePath);
					logCallback?.Invoke("[SYSTEM] Extraction complete. Temporary archive deleted.");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Failed to extract archive: {ex.Message}");
				return -1;
			}

			// ========================================================
			// 4. SMART JAVA CHECKER & SCRIPT GENERATION
			// ========================================================
			try
			{
				if (blueprint.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
				{
					string runtimeFolder = Path.Combine(@"C:\Synix\SynixData\Runtimes", $"Java{requiredJava}");

					// 1. Check if Synix ALREADY downloaded this Java version
					string[] existingExecutables = Directory.Exists(runtimeFolder)
						? Directory.GetFiles(runtimeFolder, "java.exe", SearchOption.AllDirectories)
						: Array.Empty<string>();

					if (existingExecutables.Length > 0)
					{
						// Perfect! We already have it. Skip the system check and the popup.
						javaExeCmd = $"\"{existingExecutables[0]}\"";
						logCallback?.Invoke($"[SYSTEM] Using previously cached Portable Java {requiredJava}.");
					}
					else
					{
						// 2. No portable version found, so NOW we check the user's PC
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
								string zipPath = Path.Combine(@"C:\Synix\SynixData\Runtimes", $"java{requiredJava}_temp.zip");
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
									javaExeCmd = $"\"{newlyExtracted[0]}\"";
									logCallback?.Invoke("[SYSTEM] Portable Java installed successfully!");
								}
							}
							else
							{
								logCallback?.Invoke("[WARNING] Java download skipped by user. The server will likely crash on startup.");
							}
						}
					}

					logCallback?.Invoke("[SYSTEM] Generating Minecraft Start.bat bootstrapper...");
					string batPath = Path.Combine(server.InstallPath, "Start.bat");

					// Write the bat file using either global "java" or the specific portable path
					File.WriteAllText(batPath, $"@echo off\r\n{javaExeCmd} %* <NUL\r\nif %errorlevel% neq 0 pause\r\n");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[WARNING] Failed to generate post-install files: {ex.Message}");
			}

			Core.Instance.UpdateGridStatus();
			return 0; // Success
		}

		private static async Task PumpStreamAsync(
			StreamReader reader,
			Action<string> queueLine)
		{
			char[] readBuffer = new char[256];
			StringBuilder pending = new();
			bool previousWasCarriageReturn = false;

			while (true)
			{
				int charactersRead = await reader
					.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length))
					.ConfigureAwait(false);

				if (charactersRead == 0)
					break;

				for (int index = 0; index < charactersRead; index++)
				{
					char character = readBuffer[index];

					if (character == '\r')
					{
						FlushPending(pending, queueLine);
						previousWasCarriageReturn = true;
						continue;
					}

					if (character == '\n')
					{
						// Do not create an empty second message for a CRLF pair.
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