// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

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

		public ServerConfig(string filePath, ConfigFormat format)
		{
			InitializeComponent();
			_path = filePath;
			_format = format;

			this.Text = "Config Editor - " + Path.GetFileName(filePath);

			flowLayoutPanel1.AutoScroll = true;
			flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanel1.WrapContents = false;

			LoadUI();
		}

		private void LoadUI()
		{
			_fileData = ConfigHandler.LoadConfig(_path, _format);

			flowLayoutPanel1.Controls.Clear();
			flowLayoutPanel1.SuspendLayout();

			foreach (var line in _fileData)
			{
				Panel p = new Panel
				{
					Width = flowLayoutPanel1.Width - 40,
					Height = 35,
					Padding = new Padding(5)
				};

				Label lbl = new Label
				{
					Text = line.Key,
					Width = 250,
					Location = new Point(5, 8),
					ForeColor = Color.Black,
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					AutoSize = false,
					TextAlign = ContentAlignment.MiddleLeft
				};

				TextBox txt = new TextBox
				{
					Text = line.Value,
					Width = p.Width - 270,
					Location = new Point(260, 5),
					BackColor = Color.White,
					ForeColor = Color.Black,
					BorderStyle = BorderStyle.FixedSingle,
					Font = new Font("Segoe UI", 9, FontStyle.Regular)
				};

				p.Controls.Add(lbl);
				p.Controls.Add(txt);
				flowLayoutPanel1.Controls.Add(p);
			}

			flowLayoutPanel1.ResumeLayout();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			List<ConfigLine> updatedData = new List<ConfigLine>();

			foreach (Control ctrl in flowLayoutPanel1.Controls)
			{
				if (ctrl is Panel p && p.Controls.Count >= 2)
				{
					string key = p.Controls[0].Text.Trim();
					string val = p.Controls[1].Text.Trim();

					updatedData.Add(new ConfigLine { Key = key, Value = val });
				}
			}

			ConfigHandler.SaveConfig(_path, updatedData, _format);

			this.Close();
		}
	}
}