// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel.ServerHandler
{
	partial class ServerConfig
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			flowLayoutPanel1 = new FlowLayoutPanel();
			btnSaveConfig = new Button();
			SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.Location = new Point(12, 12);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(776, 383);
			flowLayoutPanel1.TabIndex = 0;
			// 
			// btnSaveConfig
			// 
			btnSaveConfig.Location = new Point(347, 401);
			btnSaveConfig.Name = "btnSaveConfig";
			btnSaveConfig.Size = new Size(119, 40);
			btnSaveConfig.TabIndex = 1;
			btnSaveConfig.Text = "Save Config";
			btnSaveConfig.UseVisualStyleBackColor = true;
			btnSaveConfig.Click += btnSave_Click;
			// 
			// ServerConfig
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(btnSaveConfig);
			Controls.Add(flowLayoutPanel1);
			Name = "ServerConfig";
			Text = "ServerConfig";
			ResumeLayout(false);
		}

		#endregion

		private FlowLayoutPanel flowLayoutPanel1;
		private Button btnSaveConfig;
	}
}