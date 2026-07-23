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

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ServerConfig : Form
	{
		private string _path;
		private List<ConfigLine> _fileData;
		private ConfigFormat _format;

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
			btnSave.Text = "Save Config";
			btnSave.Height = 40;
			btnSave.Width = 150;
			btnSave.Location = new Point((this.ClientSize.Width / 2) - 75, this.ClientSize.Height - 50);
			btnSave.Anchor = AnchorStyles.Bottom;
			btnSave.Click += btnSave_Click;
			this.Controls.Add(btnSave);

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
			dgvConfig.SendToBack();

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
