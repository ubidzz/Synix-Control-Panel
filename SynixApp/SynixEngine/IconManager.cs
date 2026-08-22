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

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static readonly Dictionary<string, string> _iconPathCache =
			new(StringComparer.OrdinalIgnoreCase);
		private static readonly HttpClient _iconHttpClient = new()
		{
			Timeout = TimeSpan.FromSeconds(20)
		};

		public static string GetLocalServerIcon(string gameName, string fullExePath)
		{
			GameInfo? blueprint = GameDatabase.GetGame(gameName);
			string canonicalGameName = blueprint?.Game ?? GameDatabase.GetCanonicalGameName(gameName);
			string safeName = canonicalGameName.Replace(" ", "_").Replace(":", "");

			if (_iconPathCache.TryGetValue(safeName, out string memoryPath))
			{
				if (IsValidIconFile(memoryPath))
					return memoryPath;

				_iconPathCache.Remove(safeName);
			}

			FolderHandler.Create(Core.GameIconsPath);
			string localIconPath = Path.Combine(Core.GameIconsPath, $"{safeName}.png");

			if (File.Exists(localIconPath))
			{
				if (IsValidIconFile(localIconPath))
				{
					_iconPathCache[safeName] = localIconPath;
					return localIconPath;
				}

				TryDeleteFile(localIconPath);
			}

			if (blueprint != null && !string.IsNullOrWhiteSpace(blueprint.IconUrl))
			{
				string temporaryIconPath = localIconPath + ".download";
				try
				{
					byte[] imageBytes = Task.Run(
						() => _iconHttpClient.GetByteArrayAsync(blueprint.IconUrl))
						.GetAwaiter()
						.GetResult();

					using (var stream = new MemoryStream(imageBytes))
					using (var downloadedImage = Image.FromStream(
						stream,
						useEmbeddedColorManagement: false,
						validateImageData: true))
					using (var normalizedBitmap = new Bitmap(downloadedImage))
					{
						normalizedBitmap.Save(
							temporaryIconPath,
							System.Drawing.Imaging.ImageFormat.Png);
					}

					File.Move(temporaryIconPath, localIconPath, overwrite: true);
					_iconPathCache[safeName] = localIconPath;
					return localIconPath;
				}
				catch
				{
					TryDeleteFile(temporaryIconPath);

				}
			}

			if (File.Exists(fullExePath) &&
				!fullExePath.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) &&
				!fullExePath.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					using (Icon extractedIcon = Icon.ExtractAssociatedIcon(fullExePath))
					{
						if (extractedIcon != null)
						{
							using (Bitmap bitmap = extractedIcon.ToBitmap())
							{
								bitmap.Save(localIconPath, System.Drawing.Imaging.ImageFormat.Png);
								_iconPathCache[safeName] = localIconPath;
								return localIconPath;
							}
						}
					}
				}
				catch
				{

				}
			}

			return Path.Combine(GameIconsPath, "default_server.png");
		}

		public static async Task<bool> RefreshServerIconAsync(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			GameInfo? blueprint = GameDatabase.GetGame(server.Game);
			string executableName = blueprint?.ExeName ?? server.ExeName;
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				string.IsNullOrWhiteSpace(executableName))
			{
				return false;
			}

			string fullExePath = Path.Combine(server.InstallPath, executableName);
			string iconPath = await Task.Run(() =>
				GetLocalServerIcon(server.Game, fullExePath));
			if (!IsValidIconFile(iconPath))
			{
				return false;
			}

			MainGUI? mainWindow = MainGUI.Instance;
			if (mainWindow != null &&
				!mainWindow.IsDisposed &&
				mainWindow.IsHandleCreated &&
				mainWindow.InvokeRequired)
			{
				return (bool)mainWindow.Invoke(
					new Func<bool>(() => ApplyServerIcon(server, iconPath)));
			}

			return ApplyServerIcon(server, iconPath);
		}

		internal static bool ApplyServerIcon(GameServer server, string iconPath)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (!IsValidIconFile(iconPath))
			{
				return false;
			}

			Bitmap refreshedIcon;
			using (MemoryStream stream = new(File.ReadAllBytes(iconPath)))
			using (Image sourceImage = Image.FromStream(
				stream,
				useEmbeddedColorManagement: false,
				validateImageData: true))
			{
				refreshedIcon = new Bitmap(sourceImage);
			}

			string canonicalGameName = GameDatabase.GetCanonicalGameName(server.Game);
			MainGUI.ServerIconsCache[canonicalGameName] = refreshedIcon;
			server.DisplayIcon = refreshedIcon;
			foreach (GameServer installedServer in MainGUI.serverList.Where(item =>
				string.Equals(
					GameDatabase.GetCanonicalGameName(item.Game),
					canonicalGameName,
					StringComparison.OrdinalIgnoreCase)))
			{
				installedServer.DisplayIcon = refreshedIcon;
			}

			return true;
		}

		private static bool IsValidIconFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
				return false;

			try
			{
				using FileStream stream = File.Open(
					path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite);
				using Image image = Image.FromStream(
					stream,
					useEmbeddedColorManagement: false,
					validateImageData: true);
				return image.Width > 0 && image.Height > 0;
			}
			catch
			{
				return false;
			}
		}

		private static void TryDeleteFile(string path)
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
	}
}
