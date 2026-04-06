using Game_Server_Control_Panel.FileEditor;
using Game_Server_Control_Panel.MonitoringHandler;
using Game_Server_Control_Panel.ServerHandler;
using Game_Server_Control_Panel.SteamCMD;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace Game_Server_Control_Panel
{
	public partial class MainGUI : Form
	{
		public static BindingList<GameServer> serverList = [];
		private bool isDownloadActive = false;
		private static bool isInitializing = false;
		public static MainGUI? Instance { get; private set; }

		public MainGUI()
		{
			InitializeComponent();
			JsonManager.Load();

			Instance = this;

			// Link the Grid to the List
			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.DataSource = serverList;

			// Map the Columns to the Class Properties
			// Check your Designer.cs to make sure these names (colName, etc.) match!
			dataGridView1.Columns["colName"].DataPropertyName = "ServerName";
			dataGridView1.Columns["colGame"].DataPropertyName = "Game";
			dataGridView1.Columns["colPort"].DataPropertyName = "Port";
			dataGridView1.Columns["colPassword"].DataPropertyName = "Password";
			dataGridView1.Columns["colAdminPassword"].DataPropertyName = "AdminPassword";
			dataGridView1.Columns["colStatus"].DataPropertyName = "Status";

			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
		}

		private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "Status")
			{
				if (e.Value != null)
				{
					string status = e.Value.ToString() ?? "";
					Font boldFont = new Font(dataGridView1.Font, FontStyle.Bold);

					if (status == "Stopped") e.CellStyle.ForeColor = Color.Red;
					else if (status == "Installing") e.CellStyle.ForeColor = Color.Blue;
					else if (status == "Running") e.CellStyle.ForeColor = Color.Green;

					e.CellStyle.Font = boldFont;
				}
			}
		}

		private void UpdateGrid()
		{
			// 1. Thread safety check
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(UpdateGrid));
				return;
			}

			// 2. Remember where the user was looking and clicking so the screen doesn't jump
			int scrollPosition = dataGridView1.FirstDisplayedScrollingRowIndex;
			int selectedIndex = dataGridView1.CurrentRow != null ? dataGridView1.CurrentRow.Index : -1;

			// 3. THE NUCLEAR REFRESH: Unbind and rebind the list. 
			// This is the ONLY way a standard List forces the DataGridView to update its text.
			dataGridView1.DataSource = null;
			dataGridView1.DataSource = serverList;

			// 4. Put the user's selection and scroll bar exactly back where it was
			if (scrollPosition != -1 && scrollPosition < dataGridView1.Rows.Count)
			{
				dataGridView1.FirstDisplayedScrollingRowIndex = scrollPosition;
			}

			if (selectedIndex != -1 && selectedIndex < dataGridView1.Rows.Count)
			{
				dataGridView1.ClearSelection();
				dataGridView1.Rows[selectedIndex].Selected = true;
			}

			// 5. Force Windows to physically repaint the Green/Red colors on your monitor right now
			dataGridView1.Refresh();
		}

		public void AppendLog(string message)
		{
			if (!this.IsHandleCreated || this.IsDisposed) return;

			if (rtbLog.InvokeRequired)
			{
				rtbLog.BeginInvoke(new Action(() => AppendLog(message)));
				return;
			}

			string timeStamp = $"[{DateTime.Now:HH:mm:ss}] ";
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);

			rtbLog.SelectionStart = rtbLog.Text.Length;
			rtbLog.ScrollToCaret();

			// ADD THIS: Forces the box to draw the new text IMMEDIATELY
			// This stops the "Frozen" look during SteamCMD installs
			rtbLog.Update();
		}

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			// 1. Set the lock immediately
			isDownloadActive = true;

			AppendLog("Checking SteamCMD dependencies...");

			// 2. Run the check on a background thread
			// This allows the 'X' button to stay active and trigger GUI_FormClosing
			await Task.Run(() => SteamCMD.SteamCMD.EnsureSteamCMD(AppendLog));

			// 3. Release the lock once the background task is done
			isDownloadActive = false;

			AppendLog("Initialization complete.");
		}

		private void timerMonitor_Tick(object sender, EventArgs e)
		{
			// Pass your list and the log method
			Check.ServerStatus();

			// Refresh the Grid so the "Status" column updates visually
			UpdateGrid();
		}

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			using ServerSettingsGUI settingsForm = new();
			if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.NewServer != null)
			{
				GameServer newServer = settingsForm.NewServer;
				var gameData = GameDatabase.GetGame(newServer.Game);
				string correctAppId = gameData?.AppID ?? "";

				// 1. Set status to Installing immediately
				newServer.Status = "Installing";
				isDownloadActive = true;
				dataGridView1.Refresh();

				AppendLog($"--- AUTO-INSTALL STARTED: {newServer.Game} ---");
				string steamPath = @"C:\Games\SteamCMD\steamcmd.exe";

				// 2. Run SteamCMD in the background
				await Task.Run(() =>
				{
					ServerInstaller.Install(
						steamPath,
						newServer.InstallPath,
						correctAppId,
						AppendLog
					);
				});

				newServer.Status = "Stopped";
				isDownloadActive = false;

				dataGridView1.Invalidate();
				dataGridView1.Refresh();

				JsonManager.Save();
				AppendLog($"--- AUTO-INSTALL FINISHED: {newServer.Game} ---");
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				if (selectedServer.Status == "Running")
				{
					MessageBox.Show("Please stop the server before editing.", "Server Active");
					return;
				}

				using var editForm = new ServerSettingsGUI(selectedServer);
				if (editForm.ShowDialog() == DialogResult.OK && editForm.NewServer != null)
				{
					// ServerSettingsGUI.cs handles the list swap and JSON save internally.
					dataGridView1.Refresh();
					AppendLog($"[SUCCESS] {editForm.NewServer.ServerName} updated and saved.");
				}
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				if (selectedServer.Status == "Running")
				{
					MessageBox.Show("Stop the server before deleting it!", "Server Active");
					return;
				}

				// 1. Expanded Safety Confirmation (Stay in GUI)
				var confirm = MessageBox.Show($"Are you sure you want to delete '{selectedServer.ServerName}'?\n\n" +
											$"THIS WILL PERMANENTLY REMOVE ALL SERVER FILES AT:\n{selectedServer.InstallPath}",
											"Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (confirm == DialogResult.Yes)
				{
					try
					{
						// 2. Call the backend to do the dirty work
						Delete.Server(selectedServer, AppendLog);

						// 3. Update the UI
						UpdateGrid();
					}
					catch (Exception ex)
					{
						// 4. Show your specific error message if the physical delete fails
						MessageBox.Show($"Files were deleted from the list, but some physical files couldn't be removed: {ex.Message}", "Cleanup Error");
					}
				}
			}
			else
			{
				MessageBox.Show("Click a server in the list first!");
			}
		}

		private void GUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			// 1. Check if the 'steamcmd' process exists on the PC right now
			bool isSteamRunning = Process.GetProcessesByName("steamcmd").Any();

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

				// Set to running immediately
				selectedServer.Status = "Running";
				UpdateGrid(); // <--- Added this to snap the grid to Green immediately

				// 2. Hand it off to the ServerManager (The Expert)
				Servers.Start(selectedServer, msg =>
				{
					// Use Invoke to make sure the log updates on the UI thread safely
					if (this.InvokeRequired)
					{
						this.Invoke((System.Windows.Forms.MethodInvoker)delegate
						{
							AppendLog(msg);
							UpdateGrid(); // Refresh the GUI
						});
					}
					else
					{
						AppendLog(msg);
						UpdateGrid();
					}
				});
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
				if (selectedServer.Status != "Running")
				{
					MessageBox.Show("This server is already stopped.", "Info");
					return;
				}

				// One line does all the heavy lifting now!
				Servers.Stop(selectedServer, AppendLog);

				// Update the colors on the screen
				UpdateGrid();
			}
		}
	}
}