// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel
{
	partial class ResourceMonitorGUI
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceMonitorGUI));
			listViewResources = new ListView();
			colPID = new ColumnHeader();
			colServerName = new ColumnHeader();
			colCPU = new ColumnHeader();
			colRAM = new ColumnHeader();
			colExeName = new ColumnHeader();
			tmrRefresh = new System.Windows.Forms.Timer(components);
			lblTotalCpu = new Label();
			lblTotalRam = new Label();
			SuspendLayout();
			// 
			// listViewResources
			// 
			listViewResources.BackgroundImage = Properties.Resources.logo;
			listViewResources.BorderStyle = BorderStyle.None;
			listViewResources.Columns.AddRange(new ColumnHeader[] { colPID, colServerName, colCPU, colRAM, colExeName });
			listViewResources.Location = new Point(12, 32);
			listViewResources.Name = "listViewResources";
			listViewResources.Size = new Size(776, 406);
			listViewResources.TabIndex = 0;
			listViewResources.UseCompatibleStateImageBehavior = false;
			listViewResources.View = View.Details;
			listViewResources.DrawColumnHeader += listViewResources_DrawColumnHeader;
			listViewResources.DrawItem += listViewResources_DrawItem;
			listViewResources.DrawSubItem += listViewResources_DrawSubItem;
			listViewResources.Resize += listViewResources_Resize;
			// 
			// colPID
			// 
			colPID.Text = "PID";
			// 
			// colServerName
			// 
			colServerName.Text = "Server Name";
			// 
			// colCPU
			// 
			colCPU.Text = "CPU Usage";
			// 
			// colRAM
			// 
			colRAM.Text = "RAM Usage";
			// 
			// colExeName
			// 
			colExeName.Text = "Executable";
			colExeName.Width = 200;
			// 
			// tmrRefresh
			// 
			tmrRefresh.Enabled = true;
			tmrRefresh.Interval = 1000;
			tmrRefresh.Tick += tmrRefresh_Tick;
			// 
			// lblTotalCpu
			// 
			lblTotalCpu.AutoSize = true;
			lblTotalCpu.BackColor = Color.Transparent;
			lblTotalCpu.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblTotalCpu.ForeColor = Color.DeepSkyBlue;
			lblTotalCpu.Location = new Point(12, 9);
			lblTotalCpu.Name = "lblTotalCpu";
			lblTotalCpu.Size = new Size(118, 20);
			lblTotalCpu.TabIndex = 1;
			lblTotalCpu.Text = "Total CPU Label";
			// 
			// lblTotalRam
			// 
			lblTotalRam.AutoSize = true;
			lblTotalRam.BackColor = Color.Transparent;
			lblTotalRam.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblTotalRam.ForeColor = SystemColors.HotTrack;
			lblTotalRam.Location = new Point(456, 9);
			lblTotalRam.Name = "lblTotalRam";
			lblTotalRam.Size = new Size(124, 20);
			lblTotalRam.TabIndex = 2;
			lblTotalRam.Text = "Total RAM Label";
			// 
			// ResourceMonitorGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(800, 450);
			Controls.Add(lblTotalRam);
			Controls.Add(lblTotalCpu);
			Controls.Add(listViewResources);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ResourceMonitorGUI";
			Text = "Resource Monitor";
			FormClosed += ResourceMonitorGUI_FormClosed;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ListView listViewResources;
		private ColumnHeader colPID;
		private ColumnHeader colServerName;
		private ColumnHeader colCPU;
		private ColumnHeader colRAM;
		private System.Windows.Forms.Timer tmrRefresh;
		private Label lblTotalCpu;
		private Label lblTotalRam;
		private ColumnHeader colExeName;
	}
}