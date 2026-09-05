// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class ServerSettingsNetworkPage
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
			cardPorts = new ModernSettingsCard();
			lblPortsIcon = new Label();
			lblPortsTitle = new Label();
			lblPortsDescription = new Label();
			PortLabel = new Label();
			numPort = new ModernSettingsNumericUpDown();
			QueryPortLabel = new Label();
			numQueryPort = new ModernSettingsNumericUpDown();
			lblAppPort = new Label();
			numAppPort = new ModernSettingsNumericUpDown();
			cardRcon = new ModernSettingsCard();
			lblRconIcon = new Label();
			lblRconTitle = new Label();
			lblRconDescription = new Label();
			lblRconToggleTitle = new Label();
			chkEnableRcon = new ModernSettingsToggle();
			lblRCONport = new Label();
			numRconPort = new ModernSettingsNumericUpDown();
			lblRCONpassword = new Label();
			txtRconPassword = new TextBox();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numAppPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			cardPorts.SuspendLayout();
			cardRcon.SuspendLayout();
			SuspendLayout();
			// cardPorts
			cardPorts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardPorts.BackColor = Color.FromArgb(17, 27, 45);
			cardPorts.BorderColor = Color.FromArgb(38, 52, 77);
			cardPorts.Controls.Add(lblPortsIcon);
			cardPorts.Controls.Add(lblPortsTitle);
			cardPorts.Controls.Add(lblPortsDescription);
			cardPorts.Controls.Add(PortLabel);
			cardPorts.Controls.Add(numPort);
			cardPorts.Controls.Add(QueryPortLabel);
			cardPorts.Controls.Add(numQueryPort);
			cardPorts.Controls.Add(lblAppPort);
			cardPorts.Controls.Add(numAppPort);
			cardPorts.CornerRadius = 12;
			cardPorts.FillColor = Color.FromArgb(17, 27, 45);
			cardPorts.Location = new Point(0, 0);
			cardPorts.Name = "cardPorts";
			cardPorts.Size = new Size(914, 190);
			cardPorts.TabIndex = 0;

			// lblPortsIcon
			lblPortsIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblPortsIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblPortsIcon.Location = new Point(20, 14);
			lblPortsIcon.Name = "lblPortsIcon";
			lblPortsIcon.Size = new Size(28, 30);
			lblPortsIcon.TabIndex = 0;
			lblPortsIcon.Text = LocalizationManager.Get("Text.70B4B62A6095E612F4AD");
			lblPortsIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblPortsTitle
			lblPortsTitle.AutoSize = true;
			lblPortsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblPortsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPortsTitle.Location = new Point(54, 19);
			lblPortsTitle.Name = "lblPortsTitle";
			lblPortsTitle.Size = new Size(117, 21);
			lblPortsTitle.TabIndex = 1;
			lblPortsTitle.Text = LocalizationManager.Get("Text.F86BB6FC21DEB1E1F841");

			// lblPortsDescription
			lblPortsDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPortsDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsDescription.Font = new Font("Segoe UI", 8.5F);
			lblPortsDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblPortsDescription.Location = new Point(24, 49);
			lblPortsDescription.Name = "lblPortsDescription";
			lblPortsDescription.Size = new Size(866, 24);
			lblPortsDescription.TabIndex = 2;
			lblPortsDescription.Text = LocalizationManager.Get("Text.F2492B6E06AE47BEB928");

			// PortLabel
			PortLabel.AutoSize = true;
			PortLabel.BackColor = Color.FromArgb(17, 27, 45);
			PortLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			PortLabel.ForeColor = Color.FromArgb(245, 247, 251);
			PortLabel.Location = new Point(24, 91);
			PortLabel.Name = "PortLabel";
			PortLabel.Size = new Size(66, 15);
			PortLabel.TabIndex = 3;
			PortLabel.Text = LocalizationManager.Get("PortRole.Game");

			// numPort
			numPort.BackColor = Color.FromArgb(12, 21, 36);
			numPort.Font = new Font("Segoe UI", 10F);
			numPort.ForeColor = Color.FromArgb(245, 247, 251);
			numPort.Location = new Point(24, 113);
			numPort.Maximum = 65535;
			numPort.Minimum = 1024;
			numPort.Name = "numPort";
			numPort.Size = new Size(266, 36);
			numPort.TabIndex = 4;
			numPort.Value = 1024;

			// QueryPortLabel
			QueryPortLabel.AutoSize = true;
			QueryPortLabel.BackColor = Color.FromArgb(17, 27, 45);
			QueryPortLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			QueryPortLabel.ForeColor = Color.FromArgb(245, 247, 251);
			QueryPortLabel.Location = new Point(314, 91);
			QueryPortLabel.Name = "QueryPortLabel";
			QueryPortLabel.Size = new Size(66, 15);
			QueryPortLabel.TabIndex = 5;
			QueryPortLabel.Text = LocalizationManager.Get("PortRole.Query");

			// numQueryPort
			numQueryPort.BackColor = Color.FromArgb(12, 21, 36);
			numQueryPort.Font = new Font("Segoe UI", 10F);
			numQueryPort.ForeColor = Color.FromArgb(245, 247, 251);
			numQueryPort.Location = new Point(314, 113);
			numQueryPort.Maximum = 65535;
			numQueryPort.Minimum = 1024;
			numQueryPort.Name = "numQueryPort";
			numQueryPort.Size = new Size(266, 36);
			numQueryPort.TabIndex = 6;
			numQueryPort.Value = 27015;

			// lblAppPort
			lblAppPort.AutoSize = true;
			lblAppPort.BackColor = Color.FromArgb(17, 27, 45);
			lblAppPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAppPort.ForeColor = Color.FromArgb(245, 247, 251);
			lblAppPort.Location = new Point(604, 91);
			lblAppPort.Name = "lblAppPort";
			lblAppPort.Size = new Size(54, 15);
			lblAppPort.TabIndex = 7;
			lblAppPort.Text = LocalizationManager.Get("PortRole.App");

			// numAppPort
			numAppPort.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			numAppPort.BackColor = Color.FromArgb(12, 21, 36);
			numAppPort.Font = new Font("Segoe UI", 10F);
			numAppPort.ForeColor = Color.FromArgb(245, 247, 251);
			numAppPort.Location = new Point(604, 113);
			numAppPort.Maximum = 65535;
			numAppPort.Minimum = 10000;
			numAppPort.Name = "numAppPort";
			numAppPort.Size = new Size(286, 36);
			numAppPort.TabIndex = 8;
			numAppPort.Value = 10000;

			// cardRcon
			cardRcon.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardRcon.BackColor = Color.FromArgb(17, 27, 45);
			cardRcon.BorderColor = Color.FromArgb(38, 52, 77);
			cardRcon.Controls.Add(lblRconIcon);
			cardRcon.Controls.Add(lblRconTitle);
			cardRcon.Controls.Add(lblRconDescription);
			cardRcon.Controls.Add(lblRconToggleTitle);
			cardRcon.Controls.Add(chkEnableRcon);
			cardRcon.Controls.Add(lblRCONport);
			cardRcon.Controls.Add(numRconPort);
			cardRcon.Controls.Add(lblRCONpassword);
			cardRcon.Controls.Add(txtRconPassword);
			cardRcon.CornerRadius = 12;
			cardRcon.FillColor = Color.FromArgb(17, 27, 45);
			cardRcon.Location = new Point(0, 206);
			cardRcon.Name = "cardRcon";
			cardRcon.Size = new Size(914, 210);
			cardRcon.TabIndex = 1;

			// lblRconIcon
			lblRconIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblRconIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblRconIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblRconIcon.Location = new Point(20, 14);
			lblRconIcon.Name = "lblRconIcon";
			lblRconIcon.Size = new Size(28, 30);
			lblRconIcon.TabIndex = 0;
			lblRconIcon.Text = LocalizationManager.Get("Text.F88C1B7702D3AFFE04D4");
			lblRconIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblRconTitle
			lblRconTitle.AutoSize = true;
			lblRconTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblRconTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblRconTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconTitle.Location = new Point(54, 19);
			lblRconTitle.Name = "lblRconTitle";
			lblRconTitle.Size = new Size(185, 21);
			lblRconTitle.TabIndex = 1;
			lblRconTitle.Text = LocalizationManager.Get("Text.BBC627D7B11E5339088A");

			// lblRconDescription
			lblRconDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblRconDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblRconDescription.Font = new Font("Segoe UI", 8.5F);
			lblRconDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconDescription.Location = new Point(24, 50);
			lblRconDescription.Name = "lblRconDescription";
			lblRconDescription.Size = new Size(700, 24);
			lblRconDescription.TabIndex = 2;
			lblRconDescription.Text = LocalizationManager.Get("Text.615A691F61191EE9C393");

			// lblRconToggleTitle
			lblRconToggleTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblRconToggleTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblRconToggleTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRconToggleTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconToggleTitle.Location = new Point(735, 24);
			lblRconToggleTitle.Name = "lblRconToggleTitle";
			lblRconToggleTitle.Size = new Size(92, 22);
			lblRconToggleTitle.TabIndex = 3;
			lblRconToggleTitle.Text = LocalizationManager.Get("ServerSetup.Network.RconToggle.AccessibleName");

			// chkEnableRcon
			chkEnableRcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkEnableRcon.BackColor = Color.FromArgb(17, 27, 45);
			chkEnableRcon.Location = new Point(836, 18);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(54, 30);
			chkEnableRcon.TabIndex = 4;

			// lblRCONport
			lblRCONport.AutoSize = true;
			lblRCONport.BackColor = Color.FromArgb(17, 27, 45);
			lblRCONport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRCONport.ForeColor = Color.FromArgb(245, 247, 251);
			lblRCONport.Location = new Point(24, 95);
			lblRCONport.Name = "lblRCONport";
			lblRCONport.Size = new Size(68, 15);
			lblRCONport.TabIndex = 5;
			lblRCONport.Text = LocalizationManager.Get("PortRole.Rcon");

			// numRconPort
			numRconPort.BackColor = Color.FromArgb(12, 21, 36);
			numRconPort.Font = new Font("Segoe UI", 10F);
			numRconPort.ForeColor = Color.FromArgb(245, 247, 251);
			numRconPort.Location = new Point(24, 117);
			numRconPort.Maximum = 65535;
			numRconPort.Minimum = 1024;
			numRconPort.Name = "numRconPort";
			numRconPort.Size = new Size(250, 36);
			numRconPort.TabIndex = 6;
			numRconPort.Value = 1024;

			// lblRCONpassword
			lblRCONpassword.AutoSize = true;
			lblRCONpassword.BackColor = Color.FromArgb(17, 27, 45);
			lblRCONpassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRCONpassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblRCONpassword.Location = new Point(298, 95);
			lblRCONpassword.Name = "lblRCONpassword";
			lblRCONpassword.Size = new Size(98, 15);
			lblRCONpassword.TabIndex = 7;
			lblRCONpassword.Text = LocalizationManager.Get("Text.16C5D21817849765677F");

			// txtRconPassword
			txtRconPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtRconPassword.AutoSize = false;
			txtRconPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtRconPassword.BorderStyle = BorderStyle.FixedSingle;
			txtRconPassword.Font = new Font("Segoe UI", 10F);
			txtRconPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtRconPassword.Location = new Point(298, 117);
			txtRconPassword.Name = "txtRconPassword";
			txtRconPassword.Size = new Size(592, 36);
			txtRconPassword.TabIndex = 8;

			// ServerSettingsNetworkPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardPorts);
			Controls.Add(cardRcon);
			Name = "ServerSettingsNetworkPage";
			Size = new Size(914, 496);
			((System.ComponentModel.ISupportInitialize)numPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numAppPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).EndInit();
			cardPorts.ResumeLayout(false);
			cardPorts.PerformLayout();
			cardRcon.ResumeLayout(false);
			cardRcon.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardPorts;
		internal Label lblPortsIcon;
		internal Label lblPortsTitle;
		internal Label lblPortsDescription;
		internal Label PortLabel;
		internal ModernSettingsNumericUpDown numPort;
		internal Label QueryPortLabel;
		internal ModernSettingsNumericUpDown numQueryPort;
		internal Label lblAppPort;
		internal ModernSettingsNumericUpDown numAppPort;
		internal ModernSettingsCard cardRcon;
		internal Label lblRconIcon;
		internal Label lblRconTitle;
		internal Label lblRconDescription;
		internal Label lblRconToggleTitle;
		internal ModernSettingsToggle chkEnableRcon;
		internal Label lblRCONport;
		internal ModernSettingsNumericUpDown numRconPort;
		internal Label lblRCONpassword;
		internal TextBox txtRconPassword;
	}
}
