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
			btnStart = new Button();
			btnNo = new Button();
			lblWarningText = new LinkLabel();
			SuspendLayout();
			// 
			// btnStart
			// 
			btnStart.Dock = DockStyle.Bottom;
			btnStart.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStart.ForeColor = Color.ForestGreen;
			btnStart.Location = new Point(0, 382);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(576, 46);
			btnStart.TabIndex = 0;
			btnStart.Text = "Start Server";
			btnStart.UseVisualStyleBackColor = true;
			btnStart.Click += btnStart_Click;
			// 
			// btnNo
			// 
			btnNo.Dock = DockStyle.Bottom;
			btnNo.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnNo.ForeColor = Color.Red;
			btnNo.Location = new Point(0, 336);
			btnNo.Name = "btnNo";
			btnNo.Size = new Size(576, 46);
			btnNo.TabIndex = 1;
			btnNo.Text = "Remind Me Later";
			btnNo.UseVisualStyleBackColor = true;
			btnNo.Click += btnNo_Click;
			// 
			// lblWarningText
			// 
			lblWarningText.ActiveLinkColor = Color.Cyan;
			lblWarningText.BackColor = Color.Transparent;
			lblWarningText.Dock = DockStyle.Fill;
			lblWarningText.ForeColor = Color.White;
			lblWarningText.LinkBehavior = LinkBehavior.HoverUnderline;
			lblWarningText.LinkColor = Color.Yellow;
			lblWarningText.Location = new Point(0, 0);
			lblWarningText.Name = "lblWarningText";
			lblWarningText.Size = new Size(576, 336);
			lblWarningText.TabIndex = 2;
			lblWarningText.TabStop = true;
			lblWarningText.Text = "linkLabel1";
			// 
			// WarningDatabase
			// 
			AutoScaleDimensions = new SizeF(8F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			ClientSize = new Size(576, 428);
			ControlBox = false;
			Controls.Add(lblWarningText);
			Controls.Add(btnNo);
			Controls.Add(btnStart);
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

		private Button btnStart;
		private Button btnNo;
		private LinkLabel lblWarningText;
	}
}
