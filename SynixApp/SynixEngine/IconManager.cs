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
		private static Dictionary<string, string> _iconPathCache = new Dictionary<string, string>();
		private const string SynixRoot = @"C:\Synix\SynixData";

		public static string GetLocalServerIcon(string gameName, string fullExePath)
		{
			// Make the game name safe for a filename (e.g., "7 Days to Die" -> "7_Days_to_Die")
			string safeName = gameName.Replace(" ", "_").Replace(":", "");

			// 1. Check in-memory session cache first
			if (_iconPathCache.TryGetValue(safeName, out string memoryPath))
			{
				return memoryPath;
			}

			// 2. Setup the output path in C:\Synix\GameIcons
			string iconFolder = Path.Combine(SynixRoot, "GameIcons");
			FolderHandler.Create(iconFolder);
			string localIconPath = Path.Combine(iconFolder, $"{safeName}.png");

			// 3. If already extracted in a past session, return it
			if (File.Exists(localIconPath))
			{
				_iconPathCache[safeName] = localIconPath;
				return localIconPath;
			}

			// ========================================================
			// 4. DYNAMIC ICON DOWNLOADER FOR NON-STEAM GAMES
			// ========================================================
			var blueprint = GameDatabase.GetGame(gameName);
			if (blueprint != null && !string.IsNullOrWhiteSpace(blueprint.IconUrl))
			{
				try
				{
					using (var client = new System.Net.Http.HttpClient())
					{
						var imageBytes = Task.Run(() => client.GetByteArrayAsync(blueprint.IconUrl)).GetAwaiter().GetResult();
						File.WriteAllBytes(localIconPath, imageBytes);

						_iconPathCache[safeName] = localIconPath;
						return localIconPath;
					}
				}
				catch
				{
					// If the URL is dead, fall through to try normal extraction
				}
			}

			// ========================================================
			// 5. STANDARD SYSTEM ICON EXTRACTION (PULLS FROM EXENAME)
			// ========================================================
			if (File.Exists(fullExePath))
			{
				try
				{
					// This API pulls the embedded icon directly out of the ExeName file!
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
					// Fall through if file is locked, in use, or lacks permissions
				}
			}

			// 6. Hard Fallback if ExeName doesn't exist yet or extraction fails
			return Path.Combine(SynixRoot, "GameIcons", "default_server.png");
		}
	}
}