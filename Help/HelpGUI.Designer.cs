// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel.SynixEngine
{
	partial class HelpGUI
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpGUI));
			splitContainer1 = new SplitContainer();
			treeNavigation = new TreeView();
			txtSearch = new TextBox();
			lblAnswer = new RichTextBox();
			lblTopicTitle = new Label();
			pbQRCode = new PictureBox();
			((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pbQRCode).BeginInit();
			SuspendLayout();
			// 
			// splitContainer1
			// 
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer1.Location = new Point(0, 0);
			splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			splitContainer1.Panel1.Controls.Add(treeNavigation);
			splitContainer1.Panel1.Controls.Add(txtSearch);
			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(lblAnswer);
			splitContainer1.Panel2.Controls.Add(lblTopicTitle);
			splitContainer1.Panel2.Controls.Add(pbQRCode);
			splitContainer1.Size = new Size(800, 450);
			splitContainer1.SplitterDistance = 296;
			splitContainer1.TabIndex = 0;
			// 
			// treeNavigation
			// 
			treeNavigation.Dock = DockStyle.Fill;
			treeNavigation.Location = new Point(0, 23);
			treeNavigation.Name = "treeNavigation";
			treeNavigation.Size = new Size(296, 427);
			treeNavigation.TabIndex = 1;
			treeNavigation.AfterSelect += treeNavigation_AfterSelect;
			// 
			// txtSearch
			// 
			txtSearch.Dock = DockStyle.Top;
			txtSearch.Location = new Point(0, 0);
			txtSearch.Name = "txtSearch";
			txtSearch.Size = new Size(296, 23);
			txtSearch.TabIndex = 0;
			txtSearch.TextChanged += txtSearch_TextChanged;
			// 
			// lblAnswer
			// 
			lblAnswer.Dock = DockStyle.Top;
			lblAnswer.Location = new Point(0, 15);
			lblAnswer.Name = "lblAnswer";
			lblAnswer.ReadOnly = true;
			lblAnswer.Size = new Size(500, 243);
			lblAnswer.TabIndex = 1;
			lblAnswer.Text = "";
			lblAnswer.LinkClicked += lblAnswer_LinkClicked;
			// 
			// lblTopicTitle
			// 
			lblTopicTitle.AutoSize = true;
			lblTopicTitle.Dock = DockStyle.Top;
			lblTopicTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblTopicTitle.Location = new Point(0, 0);
			lblTopicTitle.Name = "lblTopicTitle";
			lblTopicTitle.Size = new Size(40, 15);
			lblTopicTitle.TabIndex = 0;
			lblTopicTitle.Text = "label1";
			// 
			// pbQRCode
			// 
			pbQRCode.Dock = DockStyle.Fill;
			pbQRCode.Image = (Image)resources.GetObject("pbQRCode.Image");
			pbQRCode.Location = new Point(0, 0);
			pbQRCode.Margin = new Padding(3, 3, 3, 60);
			pbQRCode.Name = "pbQRCode";
			pbQRCode.Padding = new Padding(40, 40, 40, 60);
			pbQRCode.Size = new Size(500, 450);
			pbQRCode.SizeMode = PictureBoxSizeMode.Zoom;
			pbQRCode.TabIndex = 2;
			pbQRCode.TabStop = false;
			pbQRCode.Visible = false;
			// 
			// HelpGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(splitContainer1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "HelpGUI";
			Text = "HelpGUI";
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel1.PerformLayout();
			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
			splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pbQRCode).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Label lblTopicTitle;
		private RichTextBox lblAnswer;
		private TextBox textBox1;
		private SplitContainer splitContainer1;
		private TreeView treeNavigation;
		private TextBox txtSearch;
		private PictureBox pbQRCode;
	}
}