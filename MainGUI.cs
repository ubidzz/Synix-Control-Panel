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
			dataGridView1.Columns["colName"].DataPropertyName = "Name";
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
					AppendLog($"--- AUTO-INSTALL STARTED: {popup.NewServer.Name} ---");

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
			// 1. Safety check: Did they select a server?
			if (dataGridView1.SelectedRows.Count == 0)
			{
				MessageBox.Show("Please select a server to edit.");
				return;
			}

			// 2. Get the current server data from the grid
			var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

			// 3. Safety check: Is the server running? (Can't rename a folder in use!)
			if (selectedServer.Status == "Running")
			{
				MessageBox.Show("Please stop the server before editing its settings or name.");
				return;
			}

			using (ServerSettingsGUI popup = new ServerSettingsGUI())
			{
				// 4. Fill the popup with existing data
				popup.FillFormForEditing(selectedServer);

				if (popup.ShowDialog() == DialogResult.OK)
				{
					try
					{
						// Pass both the old and new server objects to the brain
						ServerManager.RenameServerFolder(selectedServer, popup.NewServer);

						int index = serverList.IndexOf(selectedServer);
						serverList[index] = popup.NewServer;

						SaveServersToDisk();
						dataGridView1.Refresh();
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
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
				var confirm = MessageBox.Show($"Delete '{selectedServer.Name}'?", "Confirm", MessageBoxButtons.YesNo);
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
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				AppendLog($"Starting {selectedServer.Name}...");

				// Start a test CMD window so you can see it's working
				ProcessStartInfo start = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = $"/k echo Starting {selectedServer.Game} on Port {selectedServer.Port}",
					UseShellExecute = true
				};

				selectedServer.RunningProcess = Process.Start(start);
				selectedServer.Status = "Running";

				dataGridView1.Refresh();
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				if (selectedServer.RunningProcess != null && !selectedServer.RunningProcess.HasExited)
				{
					AppendLog($"Stopping {selectedServer.Name}...");
					selectedServer.RunningProcess.Kill();
					selectedServer.Status = "Stopped";
					dataGridView1.Refresh();
				}
			}
		}
	}
}

public class GameServer
{
	public string Name { get; set; }
	public string Game { get; set; }
	public int Port { get; set; }
	public int QueryPort { get; set; }
	public string Status { get; set; } = "Stopped";
	public string InstallPath { get; set; }
	public string Password { get; set; }
	public int MaxPlayers { get; set; } = 10;
	public string WorldName { get; set; } = "NewWorld";
	public string ExtraArgs { get; set; } = "-log";
	public bool IsDefaultPath { get; set; }
}