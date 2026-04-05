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
			if (dataGridView1.InvokeRequired)
			{
				// This pushes the command back to the Main UI thread so it doesn't crash
				dataGridView1.Invoke(new Action(UpdateGrid));
				return;
			}

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

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			using (ServerSettingsGUI settingsForm = new ServerSettingsGUI())
			{
				if (settingsForm.ShowDialog() == DialogResult.OK)
				{
					GameServer newServer = settingsForm.NewServer;

					// 1. ADD & REFRESH THE GRID
					serverList.Add(newServer);
					SaveServersToDisk();
					UpdateGrid(); // Shows the server in the list immediately

					// 2. RAISE THE SHIELD (Locks the 'X' button)
					isDownloadActive = true;

					AppendLog($"--- AUTO-INSTALL STARTED: {newServer.Game} ---");

					string steamPath = @"C:\Games\SteamCMD\steamcmd.exe";

					// 3. RUN IN BACKGROUND
					// 'await Task.Run' ensures the UI stays responsive so the 'X' lock can work
					await Task.Run(() =>
						ServerManager.RunUpdate(steamPath, newServer.InstallPath, newServer.AppID, msg => AppendLog(msg))
					);

					// 4. LOWER THE SHIELD (Unlocks the 'X' button)
					isDownloadActive = false;

					// 5. FINAL LOGGING
					AppendLog($"--- AUTO-INSTALL FINISHED: {newServer.Game} ---");

					// Final refresh to update Status from 'Installing' to 'Stopped'
					UpdateGrid();
				}
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				// 1. Safety Check: Don't rename a folder while the EXE is running!
				if (selectedServer.Status == "Running")
				{
					MessageBox.Show("Please stop the server before renaming it.", "Server Busy");
					return;
				}

				string oldPath = selectedServer.InstallPath;

				using (var editForm = new ServerSettingsGUI(selectedServer))
				{
					if (editForm.ShowDialog() == DialogResult.OK)
					{
						var updatedServer = editForm.NewServer;
						string newPath = updatedServer.InstallPath;

						// 2. The Rename Logic
						if (oldPath != newPath && Directory.Exists(oldPath))
						{
							try
							{
								// Directory.Move acts as a 'Rename' if the parent folder is the same
								if (!Directory.Exists(newPath))
								{
									Directory.Move(oldPath, newPath);
									AppendLog($"[RENAME] Folder changed to: {newPath}");
								}
							}
							catch (Exception ex)
							{
								MessageBox.Show($"Could not rename folder: {ex.Message}");
								// Optional: Roll back the path in the object if rename fails
								updatedServer.InstallPath = oldPath;
							}
						}

						// 3. Update the UI and Save
						int index = serverList.IndexOf(selectedServer);
						serverList[index] = updatedServer;
						SaveServersToDisk();
					}
				}
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				if (selectedServer.Status == "Running")
				{
					MessageBox.Show("Stop the server before deleting it!", "Server Active");
					return;
				}

				// 1. Expanded Safety Confirmation
				var confirm = MessageBox.Show($"Are you sure you want to delete '{selectedServer.ServerName}'?\n\nTHIS WILL PERMANENTLY REMOVE ALL SERVER FILES AT:\n{selectedServer.InstallPath}",
											"Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (confirm == DialogResult.Yes)
				{
					try
					{
						// 2. Delete the physical files first
						if (Directory.Exists(selectedServer.InstallPath))
						{
							// 'true' means it deletes all subfolders and files inside
							Directory.Delete(selectedServer.InstallPath, true);
						}

						// 3. Remove from the UI list and Save JSON
						serverList.Remove(selectedServer);
						SaveServersToDisk();

						AppendLog($"[CLEANUP] Deleted server '{selectedServer.ServerName}' and all files at {selectedServer.InstallPath}");
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Files were deleted from the list, but some physical files couldn't be removed: {ex.Message}", "Cleanup Error");
					}
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

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			// 1. Set the lock immediately
			isDownloadActive = true;

			AppendLog("Checking SteamCMD dependencies...");

			// 2. Run the check on a background thread
			// This allows the 'X' button to stay active and trigger GUI_FormClosing
			await Task.Run(() => ServerManager.EnsureSteamCMD(AppendLog));

			// 3. Release the lock once the background task is done
			isDownloadActive = false;

			AppendLog("Initialization complete.");
		}

		private void GUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			// 1. Check if the 'steamcmd' process exists on the PC right now
			bool isSteamRunning = Process.GetProcessesByName("steamcmd").Length > 0;

			// 2. Check your manual flag (for internal initialization)
			if (isDownloadActive || isSteamRunning)
			{
				MessageBox.Show("SteamCMD is currently active and performing operations.\n\n" +
								"Closing now will corrupt your game files or SteamCMD installation. " +
								"Please wait for the console to finish.",
								"Process in Progress",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);

				e.Cancel = true; // LOCK THE X
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