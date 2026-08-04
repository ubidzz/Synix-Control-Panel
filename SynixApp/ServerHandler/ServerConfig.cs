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
using Synix_Control_Panel.SynixApp.ServerHandler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ServerConfig : Form
	{
		private string _path;
		private List<ConfigLine> _fileData = new List<ConfigLine>();
		private ConfigFormat _format;

		private DataGridView dgvConfig = new DataGridView();
		private Button btnSave = new Button();
		private Panel pnlBottom = new Panel();

		public ServerConfig(string filePath, ConfigFormat format)
		{
			InitializeComponent();
			_path = filePath;
			_format = format;

			this.Text = "Config Editor - " + Path.GetFileName(filePath);
			this.Size = new Size(800, 600);
			this.MinimumSize = new Size(600, 400);
			this.StartPosition = FormStartPosition.CenterParent;

			SetupInterface();
			LoadUI();
		}

		private void SetupInterface()
		{
			// Bottom Container Panel for Save Button
			pnlBottom.Dock = DockStyle.Bottom;
			pnlBottom.Height = 60;
			pnlBottom.BackColor = SystemColors.Control;

			btnSave.Text = "Save Config";
			btnSave.Size = new Size(150, 40);
			btnSave.Location = new Point((pnlBottom.Width - btnSave.Width) / 2, (pnlBottom.Height - btnSave.Height) / 2);
			btnSave.Anchor = AnchorStyles.None;
			btnSave.Click += btnSave_Click;
			pnlBottom.Controls.Add(btnSave);

			// DataGridView configuration set to Fill top area
			dgvConfig.Dock = DockStyle.Fill;
			dgvConfig.AllowUserToAddRows = false;
			dgvConfig.RowHeadersVisible = false;
			dgvConfig.BackgroundColor = Color.White;
			dgvConfig.BorderStyle = BorderStyle.None;
			dgvConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvConfig.RowTemplate.Height = 35;

			dgvConfig.Columns.Clear();
			dgvConfig.Columns.Add("Key", "Setting Name");
			dgvConfig.Columns.Add("Value", "Value");

			dgvConfig.Columns[0].ReadOnly = true;
			dgvConfig.Columns[0].FillWeight = 35; // 35% key column
			dgvConfig.Columns[1].FillWeight = 65; // 65% value column
			dgvConfig.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

			// Add controls to Form
			this.Controls.Add(dgvConfig);
			this.Controls.Add(pnlBottom);
		}

		private void LoadUI()
		{
			try
			{
				if (!File.Exists(_path))
				{
					MessageBox.Show($"Config file does not exist at:\n{_path}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				_fileData = ConfigHandler.LoadConfig(_path, _format) ?? new List<ConfigLine>();
				dgvConfig.Rows.Clear();

				if (_fileData.Count == 0)
				{
					MessageBox.Show("The configuration file was opened, but no editable keys or settings were found.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				foreach (var line in _fileData)
				{
					if (line != null)
					{
						dgvConfig.Rows.Add(line.Key ?? "", line.Value ?? "");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error reading config file:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			dgvConfig.EndEdit();
			List<ConfigLine> updatedData = new List<ConfigLine>();

			foreach (DataGridViewRow row in dgvConfig.Rows)
			{
				if (row.Cells[0].Value != null)
				{
					updatedData.Add(new ConfigLine
					{
						Key = row.Cells[0].Value.ToString(),
						Value = row.Cells[1].Value?.ToString() ?? ""
					});
				}
			}

			ConfigHandler.SaveConfig(_path, updatedData, _format);
			this.Close();
		}
	}
}
