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

		private void UpdateGrid()
		{
			// This tells the DataGridView to look at the 'serverList' 
			// and update the screen with the new Status/PID values.
			dataGridView1.ResetBindings();
		}

		private void timerMonitor_Tick(object sender, EventArgs e)
		{
			// Pass your list and the log method
			ServerManager.CheckServerStatus(serverList, msg => AppendLog(msg));

			// Refresh the Grid so the "Status" column updates visually
			UpdateGrid();
		}

		private void PrepareServerFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static void SaveServersToDisk(BindingList<GameServer> servers)
		{
			try
			{
				string jsonString = JsonSerializer.Serialize(servers);
				File.WriteAllText("servers.json", jsonString);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Save failed: " + ex.Message);
			}
		}

		// VERSION B: The "Bridge" (Used by all your existing buttons)
		public void SaveServersToDisk()
		{
			// This calls Version A and gives it the current list
			SaveServersToDisk(serverList);
		}

		private void LoadServersFromDisk()
		{
			isInitializing = true;
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
			isInitializing = false;
		}

		private void btnAddServer_Click(object sender, EventArgs e)
		{
			using (ServerSettingsGUI settingsForm = new ServerSettingsGUI())
			{
				if (settingsForm.ShowDialog() == DialogResult.OK)
				{
					GameServer newServer = settingsForm.NewServer;
					serverList.Add(newServer);
					SaveServersToDisk();

					AppendLog($"--- AUTO-INSTALL STARTED: {newServer.Game} ---");

					// Define the path to your SteamCMD
					string steamPath = @"C:\Games\SteamCMD\steamcmd.exe";

					AppendLog($"--- Checking if the install path, AppID are not null: {newServer.InstallPath} %% {newServer.AppID} ---");

					// Call your existing RunUpdate method
					// We pass the InstallPath, the AppID, and our AppendLog method
					ServerManager.RunUpdate(steamPath, newServer.InstallPath, newServer.AppID, msg => AppendLog(msg));
				}
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				using (ServerSettingsGUI settingsForm = new ServerSettingsGUI())
				{
					settingsForm.FillFormForEditing(selectedServer);

					if (settingsForm.ShowDialog() == DialogResult.OK)
					{
						int index = serverList.IndexOf(selectedServer);
						if (index != -1)
						{
							serverList[index] = settingsForm.NewServer;
							serverList.ResetBindings();

							// Matches your local save method
							SaveServersToDisk();

							// THE FIX: Using your ACTUAL property 'ServerName' and method 'AppendLog'
							AppendLog($"Updated settings for server: {settingsForm.NewServer.ServerName}");
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
			// 1. Get the actual server object from the selected row
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				AppendLog($"Launching {selectedServer.Game}: {selectedServer.ServerName}...");

				// 2. Hand it off to the ServerManager (The Expert)
				// This uses the REAL InstallPath and REAL ExeName from the object
				ServerManager.StartServer(selectedServer, msg =>
				{
					// Use Invoke to make sure the log updates on the UI thread
					if (this.InvokeRequired)
					{
						this.Invoke((MethodInvoker)delegate { AppendLog(msg); });
					}
					else
					{
						AppendLog(msg);
					}
				});

				// 3. Refresh the UI to show "Running"
				UpdateGrid();
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.");
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