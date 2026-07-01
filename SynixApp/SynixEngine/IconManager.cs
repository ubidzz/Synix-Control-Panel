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
using Synix_Control_Panel.SynixApp.FileFolderHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Dictionary<string, string> _iconPathCache = new Dictionary<string, string>();
		private const string SynixRoot = @"C:\Synix\SynixData";

		public static string GetLocalServerIcon(string Appid, string serverPath)
		{
			// 1. Check in-memory session cache first
			if (_iconPathCache.TryGetValue(Appid, out string memoryPath))
			{
				return memoryPath;
			}

			// 2. Setup the output path in C:\Synix\GameIcons
			string iconFolder = Path.Combine(SynixRoot, "GameIcons");
			FolderHandler.Create(iconFolder);
			string localIconPath = Path.Combine(iconFolder, $"{Appid}.png");

			// 3. If already extracted in a past session, return it
			if (File.Exists(localIconPath))
			{
				_iconPathCache[Appid] = localIconPath;
				return localIconPath;
			}

			// 4. Extract directly using the full path to the executable
			if (File.Exists(serverPath))
			{
				try
				{
					using (Icon extractedIcon = Icon.ExtractAssociatedIcon(serverPath))
					{
						if (extractedIcon != null)
						{
							using (Bitmap bitmap = extractedIcon.ToBitmap())
							{
								bitmap.Save(localIconPath, System.Drawing.Imaging.ImageFormat.Png);
								_iconPathCache[Appid] = localIconPath;
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

			// 5. Hard Fallback if file doesn't exist or extraction fails
			return Path.Combine(SynixRoot, "GameIcons", "default_server.png");
		}
	}
}