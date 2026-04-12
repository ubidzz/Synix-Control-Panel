// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel.Help
{
	partial class DefaultArgumentsDisplay
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultArgumentsDisplay));
			btnClose = new Button();
			txtArgs = new Label();
			SuspendLayout();
			// 
			// btnClose
			// 
			btnClose.AutoSize = true;
			btnClose.Dock = DockStyle.Bottom;
			btnClose.Location = new Point(0, 235);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(314, 25);
			btnClose.TabIndex = 0;
			btnClose.Text = "Close";
			btnClose.UseVisualStyleBackColor = true;
			btnClose.Click += btnClose_Click;
			// 
			// txtArgs
			// 
			txtArgs.BackColor = Color.Transparent;
			txtArgs.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			txtArgs.ForeColor = Color.White;
			txtArgs.Location = new Point(12, 9);
			txtArgs.Name = "txtArgs";
			txtArgs.Size = new Size(290, 215);
			txtArgs.TabIndex = 1;
			txtArgs.Text = "Default arguments view";
			// 
			// DefaultArgumentsDisplay
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(314, 260);
			Controls.Add(txtArgs);
			Controls.Add(btnClose);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "DefaultArgumentsDisplay";
			Text = "DefaultArgumentsDisplay";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btnClose;
		private Label txtArgs;
	}
}