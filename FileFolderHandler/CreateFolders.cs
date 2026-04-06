using System.IO;

namespace Game_Server_Control_Panel.FileEditor
{
	public static class CreateFolders
	{
		public static void Create(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}