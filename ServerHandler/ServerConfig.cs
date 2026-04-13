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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ServerConfig : Form
	{
		private string _path;
		private List<ConfigLine> _fileData;
		private ConfigFormat _format;

		// 🚀 Declare these manually to fix "does not exist" errors
		private DataGridView dgvConfig = new DataGridView();
		private Button btnSave = new Button();

		public ServerConfig(string filePath, ConfigFormat format)
		{
			InitializeComponent();
			_path = filePath;
			_format = format;

			this.Text = "Config Editor - " + Path.GetFileName(filePath);
			this.Size = new Size(800, 600);
			this.StartPosition = FormStartPosition.CenterParent;

			SetupInterface();
			LoadUI();
		}

		private void SetupInterface()
		{
			// 1. Setup the Save Button
			btnSave.Text = "Save Config";
			btnSave.Height = 40;
			btnSave.Width = 150;
			// Position at the bottom center
			btnSave.Location = new Point((this.ClientSize.Width / 2) - 75, this.ClientSize.Height - 50);
			btnSave.Anchor = AnchorStyles.Bottom;
			btnSave.Click += btnSave_Click; // Link the click event
			this.Controls.Add(btnSave);

			// 2. Setup the Grid
			dgvConfig.Location = new Point(0, 0);
			dgvConfig.Width = this.ClientSize.Width;
			dgvConfig.Height = this.ClientSize.Height - 60;
			dgvConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

			dgvConfig.AllowUserToAddRows = false;
			dgvConfig.RowHeadersVisible = false;
			dgvConfig.BackgroundColor = Color.White;
			dgvConfig.BorderStyle = BorderStyle.None;
			dgvConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvConfig.RowTemplate.Height = 35;

			this.Controls.Add(dgvConfig);
			dgvConfig.SendToBack(); // Keeps button on top

			// 3. Add Columns
			dgvConfig.Columns.Add("Key", "Setting Name");
			dgvConfig.Columns.Add("Value", "Value");
			dgvConfig.Columns[0].ReadOnly = true;
			dgvConfig.Columns[0].Width = 250;
			dgvConfig.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
		}

		private void LoadUI()
		{
			_fileData = ConfigHandler.LoadConfig(_path, _format);
			dgvConfig.Rows.Clear();
			foreach (var line in _fileData)
			{
				dgvConfig.Rows.Add(line.Key, line.Value);
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