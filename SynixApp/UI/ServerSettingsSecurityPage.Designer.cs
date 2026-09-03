// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel
{
	partial class ServerSettingsSecurityPage
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
			cardCredentials = new ModernSettingsCard();
			lblCredentialsIcon = new Label();
			lblCredentialsTitle = new Label();
			lblPassword = new Label();
			txtPassword = new TextBox();
			lblAdminPassword = new Label();
			txtAdminPassword = new TextBox();
			lblCredentialsNote = new Label();
			cardAuthenticationToken = new ModernSettingsCard();
			lblAuthenticationTokenIcon = new Label();
			lblAuthenticationTokenTitle = new Label();
			lblAuthenticationToken = new Label();
			txtAuthenticationToken = new TextBox();
			btnAuthenticationTokenHelp = new ModernSettingsButton();
			lblAuthenticationTokenNote = new Label();
			cardInviteCode = new ModernSettingsCard();
			lblInviteCodeIcon = new Label();
			lblInviteCodeTitle = new Label();
			lblInviteCode = new Label();
			txtInviteCode = new TextBox();
			lblInviteCodeNote = new Label();
			cardCredentials.SuspendLayout();
			cardAuthenticationToken.SuspendLayout();
			cardInviteCode.SuspendLayout();
			SuspendLayout();
			// cardCredentials
			cardCredentials.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardCredentials.BackColor = Color.FromArgb(17, 27, 45);
			cardCredentials.BorderColor = Color.FromArgb(38, 52, 77);
			cardCredentials.Controls.Add(lblCredentialsIcon);
			cardCredentials.Controls.Add(lblCredentialsTitle);
			cardCredentials.Controls.Add(lblPassword);
			cardCredentials.Controls.Add(txtPassword);
			cardCredentials.Controls.Add(lblAdminPassword);
			cardCredentials.Controls.Add(txtAdminPassword);
			cardCredentials.Controls.Add(lblCredentialsNote);
			cardCredentials.CornerRadius = 12;
			cardCredentials.FillColor = Color.FromArgb(17, 27, 45);
			cardCredentials.Location = new Point(0, 0);
			cardCredentials.Name = "cardCredentials";
			cardCredentials.Size = new Size(914, 154);
			cardCredentials.TabIndex = 2;

			// lblCredentialsIcon
			lblCredentialsIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblCredentialsIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblCredentialsIcon.Location = new Point(20, 12);
			lblCredentialsIcon.Name = "lblCredentialsIcon";
			lblCredentialsIcon.Size = new Size(28, 30);
			lblCredentialsIcon.TabIndex = 0;
			lblCredentialsIcon.Text = "◇";
			lblCredentialsIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblCredentialsTitle
			lblCredentialsTitle.AutoSize = true;
			lblCredentialsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblCredentialsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblCredentialsTitle.Location = new Point(54, 17);
			lblCredentialsTitle.Name = "lblCredentialsTitle";
			lblCredentialsTitle.Size = new Size(153, 21);
			lblCredentialsTitle.TabIndex = 1;
			lblCredentialsTitle.Text = "Access Credentials";

			// lblPassword
			lblPassword.AutoSize = true;
			lblPassword.BackColor = Color.FromArgb(17, 27, 45);
			lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblPassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblPassword.Location = new Point(24, 50);
			lblPassword.Name = "lblPassword";
			lblPassword.Size = new Size(100, 15);
			lblPassword.TabIndex = 2;
			lblPassword.Text = "Server Password";

			// txtPassword
			txtPassword.AutoSize = false;
			txtPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtPassword.BorderStyle = BorderStyle.FixedSingle;
			txtPassword.Font = new Font("Segoe UI", 10F);
			txtPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtPassword.Location = new Point(24, 70);
			txtPassword.Name = "txtPassword";
			txtPassword.Size = new Size(414, 34);
			txtPassword.TabIndex = 3;

			// lblAdminPassword
			lblAdminPassword.AutoSize = true;
			lblAdminPassword.BackColor = Color.FromArgb(17, 27, 45);
			lblAdminPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAdminPassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblAdminPassword.Location = new Point(462, 50);
			lblAdminPassword.Name = "lblAdminPassword";
			lblAdminPassword.Size = new Size(102, 15);
			lblAdminPassword.TabIndex = 4;
			lblAdminPassword.Text = "Admin Password";

			// txtAdminPassword
			txtAdminPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtAdminPassword.AutoSize = false;
			txtAdminPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtAdminPassword.BorderStyle = BorderStyle.FixedSingle;
			txtAdminPassword.Font = new Font("Segoe UI", 10F);
			txtAdminPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtAdminPassword.Location = new Point(462, 70);
			txtAdminPassword.Name = "txtAdminPassword";
			txtAdminPassword.Size = new Size(428, 34);
			txtAdminPassword.TabIndex = 5;

			// lblCredentialsNote
			lblCredentialsNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblCredentialsNote.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsNote.Font = new Font("Segoe UI", 8F);
			lblCredentialsNote.ForeColor = Color.FromArgb(158, 172, 194);
			lblCredentialsNote.Location = new Point(24, 116);
			lblCredentialsNote.Name = "lblCredentialsNote";
			lblCredentialsNote.Size = new Size(866, 22);
			lblCredentialsNote.TabIndex = 6;
			lblCredentialsNote.Text = "◇  Sensitive fields follow the Synix Privacy Mode setting.";


			// cardAuthenticationToken
			cardAuthenticationToken.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardAuthenticationToken.BackColor = Color.FromArgb(17, 27, 45);
			cardAuthenticationToken.BorderColor = Color.FromArgb(38, 52, 77);
			cardAuthenticationToken.Controls.Add(lblAuthenticationTokenIcon);
			cardAuthenticationToken.Controls.Add(lblAuthenticationTokenTitle);
			cardAuthenticationToken.Controls.Add(lblAuthenticationToken);
			cardAuthenticationToken.Controls.Add(txtAuthenticationToken);
			cardAuthenticationToken.Controls.Add(btnAuthenticationTokenHelp);
			cardAuthenticationToken.Controls.Add(lblAuthenticationTokenNote);
			cardAuthenticationToken.CornerRadius = 12;
			cardAuthenticationToken.FillColor = Color.FromArgb(17, 27, 45);
			cardAuthenticationToken.Location = new Point(0, 170);
			cardAuthenticationToken.Name = "cardAuthenticationToken";
			cardAuthenticationToken.Size = new Size(914, 154);
			cardAuthenticationToken.TabIndex = 1;

			// lblAuthenticationTokenIcon
			lblAuthenticationTokenIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblAuthenticationTokenIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblAuthenticationTokenIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblAuthenticationTokenIcon.Location = new Point(20, 12);
			lblAuthenticationTokenIcon.Name = "lblAuthenticationTokenIcon";
			lblAuthenticationTokenIcon.Size = new Size(28, 30);
			lblAuthenticationTokenIcon.TabIndex = 0;
			lblAuthenticationTokenIcon.Text = "⌘";
			lblAuthenticationTokenIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblAuthenticationTokenTitle
			lblAuthenticationTokenTitle.AutoSize = true;
			lblAuthenticationTokenTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblAuthenticationTokenTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblAuthenticationTokenTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblAuthenticationTokenTitle.Location = new Point(54, 17);
			lblAuthenticationTokenTitle.Name = "lblAuthenticationTokenTitle";
			lblAuthenticationTokenTitle.Size = new Size(244, 21);
			lblAuthenticationTokenTitle.TabIndex = 1;
			lblAuthenticationTokenTitle.Text = "Online Service Authentication";

			// lblAuthenticationToken
			lblAuthenticationToken.AutoSize = true;
			lblAuthenticationToken.BackColor = Color.FromArgb(17, 27, 45);
			lblAuthenticationToken.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAuthenticationToken.ForeColor = Color.FromArgb(245, 247, 251);
			lblAuthenticationToken.Location = new Point(24, 50);
			lblAuthenticationToken.Name = "lblAuthenticationToken";
			lblAuthenticationToken.Size = new Size(129, 15);
			lblAuthenticationToken.TabIndex = 2;
			lblAuthenticationToken.Text = "Authentication Token";

			// txtAuthenticationToken
			txtAuthenticationToken.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtAuthenticationToken.AutoSize = false;
			txtAuthenticationToken.BackColor = Color.FromArgb(12, 21, 36);
			txtAuthenticationToken.BorderStyle = BorderStyle.FixedSingle;
			txtAuthenticationToken.Font = new Font("Segoe UI", 10F);
			txtAuthenticationToken.ForeColor = Color.FromArgb(245, 247, 251);
			txtAuthenticationToken.Location = new Point(24, 70);
			txtAuthenticationToken.Name = "txtAuthenticationToken";
			txtAuthenticationToken.Size = new Size(706, 34);
			txtAuthenticationToken.TabIndex = 3;

			// btnAuthenticationTokenHelp
			btnAuthenticationTokenHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnAuthenticationTokenHelp.BackColor = Color.FromArgb(12, 21, 36);
			btnAuthenticationTokenHelp.ForeColor = Color.FromArgb(245, 247, 251);
			btnAuthenticationTokenHelp.Location = new Point(750, 66);
			btnAuthenticationTokenHelp.Name = "btnAuthenticationTokenHelp";
			btnAuthenticationTokenHelp.Size = new Size(140, 42);
			btnAuthenticationTokenHelp.TabIndex = 4;
			btnAuthenticationTokenHelp.Text = "Get Token";

			// lblAuthenticationTokenNote
			lblAuthenticationTokenNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblAuthenticationTokenNote.BackColor = Color.FromArgb(17, 27, 45);
			lblAuthenticationTokenNote.Font = new Font("Segoe UI", 8F);
			lblAuthenticationTokenNote.ForeColor = Color.FromArgb(158, 172, 194);
			lblAuthenticationTokenNote.Location = new Point(24, 116);
			lblAuthenticationTokenNote.Name = "lblAuthenticationTokenNote";
			lblAuthenticationTokenNote.Size = new Size(866, 22);
			lblAuthenticationTokenNote.TabIndex = 5;
			lblAuthenticationTokenNote.Text = "Protected in Synix and hidden from its logs. Generated batch files include the usable token in readable text.";

			// cardInviteCode
			cardInviteCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardInviteCode.BackColor = Color.FromArgb(17, 27, 45);
			cardInviteCode.BorderColor = Color.FromArgb(38, 52, 77);
			cardInviteCode.Controls.Add(lblInviteCodeIcon);
			cardInviteCode.Controls.Add(lblInviteCodeTitle);
			cardInviteCode.Controls.Add(lblInviteCode);
			cardInviteCode.Controls.Add(txtInviteCode);
			cardInviteCode.Controls.Add(lblInviteCodeNote);
			cardInviteCode.CornerRadius = 12;
			cardInviteCode.FillColor = Color.FromArgb(17, 27, 45);
			cardInviteCode.Location = new Point(0, 340);
			cardInviteCode.Name = "cardInviteCode";
			cardInviteCode.Size = new Size(914, 154);
			cardInviteCode.TabIndex = 2;

			// lblInviteCodeIcon
			lblInviteCodeIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblInviteCodeIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblInviteCodeIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblInviteCodeIcon.Location = new Point(20, 12);
			lblInviteCodeIcon.Name = "lblInviteCodeIcon";
			lblInviteCodeIcon.Size = new Size(28, 30);
			lblInviteCodeIcon.TabIndex = 0;
			lblInviteCodeIcon.Text = "◇";
			lblInviteCodeIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblInviteCodeTitle
			lblInviteCodeTitle.AutoSize = true;
			lblInviteCodeTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblInviteCodeTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblInviteCodeTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblInviteCodeTitle.Location = new Point(54, 17);
			lblInviteCodeTitle.Name = "lblInviteCodeTitle";
			lblInviteCodeTitle.Size = new Size(187, 21);
			lblInviteCodeTitle.TabIndex = 1;
			lblInviteCodeTitle.Text = "Windrose Invite Access";

			// lblInviteCode
			lblInviteCode.AutoSize = true;
			lblInviteCode.BackColor = Color.FromArgb(17, 27, 45);
			lblInviteCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblInviteCode.ForeColor = Color.FromArgb(245, 247, 251);
			lblInviteCode.Location = new Point(24, 50);
			lblInviteCode.Name = "lblInviteCode";
			lblInviteCode.Size = new Size(68, 15);
			lblInviteCode.TabIndex = 2;
			lblInviteCode.Text = "Invite Code";

			// txtInviteCode
			txtInviteCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtInviteCode.AutoSize = false;
			txtInviteCode.BackColor = Color.FromArgb(12, 21, 36);
			txtInviteCode.BorderStyle = BorderStyle.FixedSingle;
			txtInviteCode.Font = new Font("Segoe UI", 10F);
			txtInviteCode.ForeColor = Color.FromArgb(245, 247, 251);
			txtInviteCode.Location = new Point(24, 70);
			txtInviteCode.MaxLength = 64;
			txtInviteCode.Name = "txtInviteCode";
			txtInviteCode.Size = new Size(866, 34);
			txtInviteCode.TabIndex = 3;

			// lblInviteCodeNote
			lblInviteCodeNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblInviteCodeNote.BackColor = Color.FromArgb(17, 27, 45);
			lblInviteCodeNote.Font = new Font("Segoe UI", 8F);
			lblInviteCodeNote.ForeColor = Color.FromArgb(158, 172, 194);
			lblInviteCodeNote.Location = new Point(24, 116);
			lblInviteCodeNote.Name = "lblInviteCodeNote";
			lblInviteCodeNote.Size = new Size(866, 22);
			lblInviteCodeNote.TabIndex = 4;
			lblInviteCodeNote.Text = "Privacy Mode masks this access credential. Enter a custom code, or leave it empty on first install to let Windrose generate one.";

			// ServerSettingsSecurityPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardCredentials);
			Controls.Add(cardAuthenticationToken);
			Controls.Add(cardInviteCode);
			Name = "ServerSettingsSecurityPage";
			Size = new Size(914, 496);
			cardCredentials.ResumeLayout(false);
			cardCredentials.PerformLayout();
			cardAuthenticationToken.ResumeLayout(false);
			cardAuthenticationToken.PerformLayout();
			cardInviteCode.ResumeLayout(false);
			cardInviteCode.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardCredentials;
		internal Label lblCredentialsIcon;
		internal Label lblCredentialsTitle;
		internal Label lblPassword;
		internal TextBox txtPassword;
		internal Label lblAdminPassword;
		internal TextBox txtAdminPassword;
		internal Label lblCredentialsNote;
		internal ModernSettingsCard cardAuthenticationToken;
		internal Label lblAuthenticationTokenIcon;
		internal Label lblAuthenticationTokenTitle;
		internal Label lblAuthenticationToken;
		internal TextBox txtAuthenticationToken;
		internal ModernSettingsButton btnAuthenticationTokenHelp;
		internal Label lblAuthenticationTokenNote;
		internal ModernSettingsCard cardInviteCode;
		internal Label lblInviteCodeIcon;
		internal Label lblInviteCodeTitle;
		internal Label lblInviteCode;
		internal TextBox txtInviteCode;
		internal Label lblInviteCodeNote;
	}
}
