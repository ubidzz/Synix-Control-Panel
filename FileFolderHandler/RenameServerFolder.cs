/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System;
using System.IO;
using Synix_Control_Panel.ServerHandler;

namespace Synix_Control_Panel.FileFolderHandler
{
	public static class RenameServerFolder
	{
		public static bool Rename(GameServer oldServer, GameServer newServer)
		{
			// 1. GATEKEEPER: If they didn't use Default Location, DO NOT RENAME.
			if (!oldServer.IsDefaultPath)
			{
				return false; // Exit early; no physical folder movement
			}

			// 2. Only move if the path actually changed
			if (oldServer.InstallPath != newServer.InstallPath)
			{
				try
				{
					if (Directory.Exists(oldServer.InstallPath))
					{
						Directory.Move(oldServer.InstallPath, newServer.InstallPath);
						return true;
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Folder move failed: " + ex.Message);
				}
			}
			return false;
		}
	}
}
