using System;
using System.IO;

namespace Game_Server_Control_Panel.FileEditor
{
	public static class CreateFiles
	{
		public static bool Create(string folderPath, string fileName, string content)
		{
			try
			{
				// Safety: Ensure the target directory exists before writing
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				string fullPath = Path.Combine(folderPath, fileName);

				// WriteAllText creates a new file, or overwrites an existing one
				File.WriteAllText(fullPath, content);
				return true;
			}
			catch (Exception)
			{
				// You can add your logCallback here later if you want error tracking
				return false;
			}
		}
	}
}