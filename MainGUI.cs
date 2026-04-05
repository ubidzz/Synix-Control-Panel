using System.ComponentModel; // Add this at the very top of the file
using System.Diagnostics;
using System.Text.Json;

namespace Game_Server_Control_Panel
{
	public partial class MainGUI : Form
	{
		BindingList<GameServer> serverList = new BindingList<GameServer>();
		private bool isDownloadActive = false;
		private bool isInitializing = false;

		public MainGUI()
		{
			InitializeComponent();
			LoadServersFromDisk();

			// Link the Grid to the List
			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.DataSource = serverList;

			// Map the Columns to the Class Properties
			// Check your Designer.cs to make sure these names (colName, etc.) match!
			dataGridView1.Columns["colName"].DataPropertyName = "ServerName";
			dataGridView1.Columns["colGame"].DataPropertyName = "Game";
			dataGridView1.Columns["colPort"].DataPropertyName = "Port";
			dataGridView1.Columns["colStatus"].DataPropertyName = "Status";
		}

		private void PrepareServerFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private void SaveServersToDisk()
		{
			try
			{
				// This turns your server list into a string of text
				string jsonString = JsonSerializer.Serialize(serverList);

				// This writes that text to a file named 'servers.json' in your app folder
				File.WriteAllText("servers.json", jsonString);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to save servers: " + ex.Message);
			}
		}

		private void LoadServersFromDisk()
		{
			if (File.Exists("servers.json"))
			{
				try
				{
					string jsonString = File.ReadAllText("servers.json");
					var loadedServers = JsonSerializer.Deserialize<List<GameServer>>(jsonString);

					if (loadedServers != null)
					{
						serverList.Clear();
						foreach (var server in loadedServers)
						{
							serverList.Add(server);
						}
					}
				}
				catch
				{
					// If the file is corrupted, we just start fresh
					MessageBox.Show("Could not load previous servers. Starting fresh.");
				}
			}
		}

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			using (ServerSettingsGUI popup = new ServerSettingsGUI())
			{
				if (popup.ShowDialog() == DialogResult.OK)
				{
					serverList.Add(popup.NewServer);
					SaveServersToDisk();

					// Start Tracking
					isDownloadActive = true;
					AppendLog($"--- AUTO-INSTALL STARTED: {popup.NewServer.Game} ---");

					try
					{
						// We use 'await' here so the app knows when the task is actually DONE
						await Task.Run(() => ServerManager.RunUpdate(
							@"C:\Games\SteamCMD\steamcmd.exe",
							popup.NewServer.InstallPath,
							popup.NewServer.Game,
							AppendLog
						));

						AppendLog("--- INSTALLATION FINISHED ---");
					}
					catch (Exception ex)
					{
						AppendLog("ERROR: " + ex.Message);
					}
					finally
					{
						// Stop Tracking
						isDownloadActive = false;
					}
				}
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			// 1. Matches your grid name
			if (dataGridView1.SelectedRows.Count > 0)
			{
				// 2. Casts to your GameServer class
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				// 3. Matches your Settings Form name
				using (ServerSettingsGUI settingsForm = new ServerSettingsGUI())
				{
					// 4. Matches your EXACT method name for loading the data
					settingsForm.FillFormForEditing(selectedServer);

					if (settingsForm.ShowDialog() == DialogResult.OK)
					{
						// 5. Update the object in your BindingList<GameServer>
						int index = serverList.IndexOf(selectedServer);
						if (index != -1)
						{
							serverList[index] = settingsForm.NewServer;

							// 6. Force UI refresh
							serverList.ResetBindings();

							// 7. Matches your actual save method in MainGUI.cs
							SaveServersToDisk();

							// 8. Matches your actual logging method in MainGUI.cs
							LogToConsole($"Updated server settings for: {settingsForm.NewServer.Name}");
						}
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a server to edit.");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			// 1. Check if a row is actually selected
			if (dataGridView1.SelectedRows.Count > 0)
			{
				// 2. Identify the server FIRST
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				// 3. NOW check if it's running
				if (selectedServer.Status == "Running")
				{
					MessageBox.Show("Stop the server before deleting it!", "Server Active");
					return;
				}

				// 4. Safety Confirmation
				var confirm = MessageBox.Show($"Delete '{selectedServer.Game}'?", "Confirm", MessageBoxButtons.YesNo);
				if (confirm == DialogResult.Yes)
				{
					serverList.Remove(selectedServer);
					SaveServersToDisk();
				}
			}
			else
			{
				MessageBox.Show("Click a server in the list first!");
			}
		}
		public void AppendLog(string message)
		{
			// If the window isn't ready or is closing, don't try to log
			if (!this.IsHandleCreated || this.IsDisposed) return;

			if (rtbLog.InvokeRequired)
			{
				// This 'BeginInvoke' is what prevents the app from hanging 
				// when SteamCMD sends dozens of lines at once
				rtbLog.BeginInvoke(new Action(() => AppendLog(message)));
				return;
			}

			string timeStamp = $"[{DateTime.Now:HH:mm:ss}] ";
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);

			// Auto-scroll to ensure you see the live SteamCMD output
			rtbLog.SelectionStart = rtbLog.Text.Length;
			rtbLog.ScrollToCaret();
		}

		private void MainGUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (isDownloadActive)
			{
				// 1. Tell the user WHY they can't leave
				MessageBox.Show("SteamCMD is currently downloading or updating a game server. " +
								"Please wait for the process to finish to avoid file corruption.",
								"Download in Progress",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);

				// 2. This line cancels the "Close" action
				e.Cancel = true;
			}
		}
		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			await Task.Delay(1500);

			// LOCK: Set active before starting
			isDownloadActive = true;
			AppendLog("Checking dependencies... Close button locked.");

			await Task.Run(() =>
			{
				try
				{
					ServerManager.EnsureSteamCMD(AppendLog);
				}
				finally
				{
					// UNLOCK: Ensure this runs even if there is an error
					isDownloadActive = false;
					AppendLog("Process finished. Close button unlocked.");
				}
			});
		}

		private void GUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (isDownloadActive)
			{
				// 1. Tell the user WHY they can't close it
				MessageBox.Show("SteamCMD is currently downloading or updating files.\n\n" +
								"Closing now may corrupt your installation. Please wait until it finishes.",
								"Download in Progress",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);

				// 2. This is the 'Lock' - it cancels the closing action
				e.Cancel = true;
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			// 1. Get the server the user selected in the list
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// 2. Look up the GameInfo (to get the .exe name like "SoulmaskServer.exe")
				var info = GameDatabase.GetGame(selectedServer.Game);

				if (info == null)
				{
					AppendLog("Error: Game template not found.");
					return;
				}

				AppendLog($"Launching {selectedServer.Game}...");

				// 3. Set up the launch
				ProcessStartInfo start = new ProcessStartInfo
				{
					// We'll need to make sure the "Path" is saved in your GameServer class later
					FileName = info.ExeName,
					Arguments = $"{info.ExtraArgs} -port={selectedServer.Port}",
					UseShellExecute = true,
					WorkingDirectory = "C:\\Path\\To\\Server" // We need to add this to your GUI!
				};

				try
				{
					selectedServer.RunningProcess = Process.Start(start);
					selectedServer.Status = "Running";
					dataGridView1.Refresh();
				}
				catch (Exception ex)
				{
					AppendLog($"Failed to start: {ex.Message}");
				}
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				if (selectedServer.RunningProcess != null && !selectedServer.RunningProcess.HasExited)
				{
					AppendLog($"Stopping {selectedServer.Game}...");
					selectedServer.RunningProcess.Kill();
					selectedServer.Status = "Stopped";
					dataGridView1.Refresh();
				}
			}
		}
	}
}