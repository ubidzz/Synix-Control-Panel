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
			button1.Location = new Point(0, 409);
			button1.Name = "button1";
			button1.Size = new Size(800, 41);
			button1.TabIndex = 0;
			button1.Text = "Open Config File";
			button1.UseVisualStyleBackColor = true;
			button1.Click += btnYes_Click;
			// 
			// btnNo
			// 
			btnNo.Dock = DockStyle.Bottom;
			btnNo.Location = new Point(0, 368);
			btnNo.Name = "btnNo";
			btnNo.Size = new Size(800, 41);
			btnNo.TabIndex = 1;
			btnNo.Text = "Remine Me Later";
			btnNo.UseVisualStyleBackColor = true;
			btnNo.Click += btnNo_Click;
			// 
			// lblWarningText
			// 
			lblWarningText.Dock = DockStyle.Fill;
			lblWarningText.Location = new Point(0, 0);
			lblWarningText.Name = "lblWarningText";
			lblWarningText.Padding = new Padding(15);
			lblWarningText.Size = new Size(800, 368);
			lblWarningText.TabIndex = 2;
			lblWarningText.Text = "label1";
			// 
			// WarningDatabase
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			ControlBox = false;
			Controls.Add(lblWarningText);
			Controls.Add(btnNo);
			Controls.Add(button1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MdiChildrenMinimizedAnchorBottom = false;
			MinimizeBox = false;
			Name = "WarningDatabase";
			Text = "Warning";
			ResumeLayout(false);
		}

		#endregion

		private Button button1;
		private Button btnNo;
		private Label lblWarningText;
	}
}