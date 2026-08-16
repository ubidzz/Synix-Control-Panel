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


		public static string GetLocalServerIcon(string gameName, string fullExePath)
		{
			string safeName = gameName.Replace(" ", "_").Replace(":", "");

			if (_iconPathCache.TryGetValue(safeName, out string memoryPath))
			{
				return memoryPath;
			}

			FolderHandler.Create(Core.GameIconsPath);
			string localIconPath = Path.Combine(Core.GameIconsPath, $"{safeName}.png");

			if (File.Exists(localIconPath))
			{
				_iconPathCache[safeName] = localIconPath;
				return localIconPath;
			}

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

			if (File.Exists(fullExePath))
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
					// Fall through if file is locked, in use, or lacks permissions
				}
			}

			return Path.Combine(GameIconsPath, "default_server.png");
		}
	}
}