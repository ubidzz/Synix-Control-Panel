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

namespace Synix_Control_Panel.FileFolderHandler
{
	public static class CreateFiles
	{
		public static bool Create(string folderPath, string fileName, string content)
		{
			try
			{
				// Use your dedicated CreateFolders utility to handle the directory logic
				CreateFolders.Create(folderPath);

				string fullPath = Path.Combine(folderPath, fileName);

				// WriteAllText creates a new file, or overwrites an existing one
				File.WriteAllText(fullPath, content);
				return true;
			}
			catch (Exception)
			{
				// Error tracking can be added here if needed
				return false;
			}
		}
	}
}