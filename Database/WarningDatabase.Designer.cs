// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel.Database
{
	partial class WarningDatabase
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarningDatabase));
			button1 = new Button();
			btnNo = new Button();
			lblWarningText = new Label();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Dock = DockStyle.Bottom;
			button1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			button1.ForeColor = Color.ForestGreen;
			button1.Location = new Point(0, 304);
			button1.Name = "button1";
			button1.Size = new Size(500, 46);
			button1.TabIndex = 0;
			button1.Text = "Open Config File";
			button1.UseVisualStyleBackColor = true;
			button1.Click += btnYes_Click;
			// 
			// btnNo
			// 
			btnNo.Dock = DockStyle.Bottom;
			btnNo.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnNo.ForeColor = Color.Red;
			btnNo.Location = new Point(0, 258);
			btnNo.Name = "btnNo";
			btnNo.Size = new Size(500, 46);
			btnNo.TabIndex = 1;
			btnNo.Text = "Remine Me Later";
			btnNo.UseVisualStyleBackColor = true;
			btnNo.Click += btnNo_Click;
			// 
			// lblWarningText
			// 
			lblWarningText.BackColor = Color.Transparent;
			lblWarningText.Dock = DockStyle.Fill;
			lblWarningText.ForeColor = Color.White;
			lblWarningText.Image = Properties.Resources.background;
			lblWarningText.Location = new Point(0, 0);
			lblWarningText.Name = "lblWarningText";
			lblWarningText.Padding = new Padding(17);
			lblWarningText.Size = new Size(500, 258);
			lblWarningText.TabIndex = 2;
			lblWarningText.Text = "Warning Message Here";
			// 
			// WarningDatabase
			// 
			AutoScaleDimensions = new SizeF(8F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			ClientSize = new Size(500, 350);
			ControlBox = false;
			Controls.Add(lblWarningText);
			Controls.Add(btnNo);
			Controls.Add(button1);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ForeColor = Color.Black;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MdiChildrenMinimizedAnchorBottom = false;
			MinimizeBox = false;
			Name = "WarningDatabase";
			Text = "⚠️ Start Warning";
			ResumeLayout(false);
		}

		#endregion

		private Button button1;
		private Button btnNo;
		private Label lblWarningText;
	}
}