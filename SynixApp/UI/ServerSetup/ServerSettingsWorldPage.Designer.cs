// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class ServerSettingsWorldPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			cardWorldGeneration = new ModernSettingsCard();
			lblWorldIcon = new Label();
			lblWorldTitle = new Label();
			lblWorldDescription = new Label();
			lblWorldSeed = new Label();
			txtWorldSeed = new TextBox();
			lblWorldSize = new Label();
			numWorldSize = new ModernSettingsNumericUpDown();
			((System.ComponentModel.ISupportInitialize)numWorldSize).BeginInit();
			cardWorldGeneration.SuspendLayout();
			SuspendLayout();
			// cardWorldGeneration
			cardWorldGeneration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardWorldGeneration.BackColor = Color.FromArgb(17, 27, 45);
			cardWorldGeneration.BorderColor = Color.FromArgb(38, 52, 77);
			cardWorldGeneration.Controls.Add(lblWorldIcon);
			cardWorldGeneration.Controls.Add(lblWorldTitle);
			cardWorldGeneration.Controls.Add(lblWorldDescription);
			cardWorldGeneration.Controls.Add(lblWorldSeed);
			cardWorldGeneration.Controls.Add(txtWorldSeed);
			cardWorldGeneration.Controls.Add(lblWorldSize);
			cardWorldGeneration.Controls.Add(numWorldSize);
			cardWorldGeneration.CornerRadius = 12;
			cardWorldGeneration.FillColor = Color.FromArgb(17, 27, 45);
			cardWorldGeneration.Location = new Point(0, 0);
			cardWorldGeneration.Name = "cardWorldGeneration";
			cardWorldGeneration.Size = new Size(914, 206);
			cardWorldGeneration.TabIndex = 0;

			// lblWorldIcon
			lblWorldIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblWorldIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblWorldIcon.Location = new Point(20, 14);
			lblWorldIcon.Name = "lblWorldIcon";
			lblWorldIcon.Size = new Size(28, 30);
			lblWorldIcon.TabIndex = 0;
			lblWorldIcon.Text = "◎";
			lblWorldIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblWorldTitle
			lblWorldTitle.AutoSize = true;
			lblWorldTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWorldTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldTitle.Location = new Point(54, 19);
			lblWorldTitle.Name = "lblWorldTitle";
			lblWorldTitle.Size = new Size(145, 21);
			lblWorldTitle.TabIndex = 1;
			lblWorldTitle.Text = "World Generation";

			// lblWorldDescription
			lblWorldDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblWorldDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldDescription.Font = new Font("Segoe UI", 8.5F);
			lblWorldDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblWorldDescription.Location = new Point(24, 50);
			lblWorldDescription.Name = "lblWorldDescription";
			lblWorldDescription.Size = new Size(866, 22);
			lblWorldDescription.TabIndex = 2;
			lblWorldDescription.Text = "These values are enabled only when the selected server template supports them.";

			// lblWorldSeed
			lblWorldSeed.AutoSize = true;
			lblWorldSeed.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldSeed.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWorldSeed.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldSeed.Location = new Point(24, 90);
			lblWorldSeed.Name = "lblWorldSeed";
			lblWorldSeed.Size = new Size(68, 15);
			lblWorldSeed.TabIndex = 3;
			lblWorldSeed.Text = "World Seed";

			// txtWorldSeed
			txtWorldSeed.AutoSize = false;
			txtWorldSeed.BackColor = Color.FromArgb(12, 21, 36);
			txtWorldSeed.BorderStyle = BorderStyle.FixedSingle;
			txtWorldSeed.Font = new Font("Segoe UI", 10F);
			txtWorldSeed.ForeColor = Color.FromArgb(245, 247, 251);
			txtWorldSeed.Location = new Point(24, 112);
			txtWorldSeed.Name = "txtWorldSeed";
			txtWorldSeed.Size = new Size(580, 36);
			txtWorldSeed.TabIndex = 4;

			// lblWorldSize
			lblWorldSize.AutoSize = true;
			lblWorldSize.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldSize.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWorldSize.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldSize.Location = new Point(628, 90);
			lblWorldSize.Name = "lblWorldSize";
			lblWorldSize.Size = new Size(65, 15);
			lblWorldSize.TabIndex = 5;
			lblWorldSize.Text = "World Size";

			// numWorldSize
			numWorldSize.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			numWorldSize.BackColor = Color.FromArgb(12, 21, 36);
			numWorldSize.Font = new Font("Segoe UI", 10F);
			numWorldSize.ForeColor = Color.FromArgb(245, 247, 251);
			numWorldSize.Location = new Point(628, 112);
			numWorldSize.Maximum = 5000;
			numWorldSize.Minimum = 50;
			numWorldSize.Name = "numWorldSize";
			numWorldSize.Size = new Size(262, 36);
			numWorldSize.TabIndex = 6;
			numWorldSize.Value = 4000;

			// ServerSettingsWorldPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardWorldGeneration);
			Name = "ServerSettingsWorldPage";
			Size = new Size(914, 496);
			((System.ComponentModel.ISupportInitialize)numWorldSize).EndInit();
			cardWorldGeneration.ResumeLayout(false);
			cardWorldGeneration.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardWorldGeneration;
		internal Label lblWorldIcon;
		internal Label lblWorldTitle;
		internal Label lblWorldDescription;
		internal Label lblWorldSeed;
		internal TextBox txtWorldSeed;
		internal Label lblWorldSize;
		internal ModernSettingsNumericUpDown numWorldSize;
	}
}
