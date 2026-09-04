// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	public partial class ServerSettingsSecurityPage : UserControl
	{
		private GameInfo? _gameData;
		private bool _isLoading;

		public event EventHandler? SettingsChanged;

		public ServerSettingsSecurityPage()
		{
			InitializeComponent();
			txtPassword.TextChanged += SettingsControlChanged;
			txtAdminPassword.TextChanged += SettingsControlChanged;
			txtAuthenticationToken.TextChanged += SettingsControlChanged;
			txtInviteCode.TextChanged += SettingsControlChanged;
			btnAuthenticationTokenHelp.Click += AuthenticationTokenHelpClicked;
		}

		public string ServerPassword => ReadManagedValue(txtPassword);
		public string AdminPassword => ReadManagedValue(txtAdminPassword);
		public string AuthenticationToken =>
			_gameData?.RequiresAuthenticationToken == true
				? ReadManagedValue(txtAuthenticationToken).Trim()
				: string.Empty;
		public string InviteCode => SupportsInviteCode
			? txtInviteCode.Text.Trim()
			: string.Empty;
		public bool SupportsInviteCode =>
			(GameFix.GetManagementCapabilities(_gameData) &
			 GameManagementCapability.InviteCode) != 0;
		public bool RequiredAdminPasswordMissing =>
			_gameData?.RequiresAdminPassword == true &&
			string.IsNullOrWhiteSpace(AdminPassword);
		public bool RequiredAuthenticationTokenMissing =>
			_gameData?.RequiresAuthenticationToken == true &&
			string.IsNullOrWhiteSpace(AuthenticationToken);
		public string AuthenticationTokenLabel =>
			string.IsNullOrWhiteSpace(_gameData?.AuthenticationTokenLabel)
				? "Authentication Token"
				: _gameData.AuthenticationTokenLabel;

		public void SetPrivacyMode(bool enabled)
		{
			txtPassword.UseSystemPasswordChar = enabled;
			txtAdminPassword.UseSystemPasswordChar = enabled;
			txtAuthenticationToken.UseSystemPasswordChar = enabled;
			txtInviteCode.UseSystemPasswordChar = enabled;
		}

		public void ClearSecrets()
		{
			txtPassword.Clear();
			txtAdminPassword.Clear();
			txtAuthenticationToken.Clear();
			txtInviteCode.Clear();
		}

		public void LoadSecrets(
			SynixServerPasswords passwords,
			string inviteCode)
		{
			_isLoading = true;
			try
			{
				txtPassword.Text = passwords.ServerPassword;
				txtAdminPassword.Text = passwords.AdminPassword;
				txtAuthenticationToken.Text = passwords.AuthenticationToken;
				txtInviteCode.Text = inviteCode ?? string.Empty;
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ClearProtectedSecrets(string inviteCode)
		{
			_isLoading = true;
			try
			{
				txtPassword.Clear();
				txtAdminPassword.Clear();
				txtAuthenticationToken.Clear();
				txtInviteCode.Text = inviteCode ?? string.Empty;
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ConfigureForGame(GameInfo? gameData)
		{
			_gameData = gameData;
			_isLoading = true;
			try
			{
				GameManagementCapability capabilities = gameData == null
					? GameManagementCapability.None
					: GameFix.GetManagementCapabilities(gameData);
				ConfigureManagedTextBox(
					txtPassword,
					(capabilities & GameManagementCapability.ServerPassword) != 0,
					gameData == null ? "Select a game..." : "Not Required");
				ConfigureManagedTextBox(
					txtAdminPassword,
					(capabilities & GameManagementCapability.AdminPassword) != 0,
					gameData == null ? "Select a game..." : "Not Required");

				bool tokenVisible = gameData?.RequiresAuthenticationToken == true;
				int nextTop = cardCredentials.Bottom + 16;
				cardAuthenticationToken.Location = new Point(0, nextTop);
				cardAuthenticationToken.Visible = tokenVisible;
				if (tokenVisible)
					nextTop = cardAuthenticationToken.Bottom + 16;
				cardInviteCode.Location = new Point(0, nextTop);
				cardInviteCode.Visible = SupportsInviteCode;
				lblAuthenticationToken.Text = AuthenticationTokenLabel;
				btnAuthenticationTokenHelp.Visible =
					!string.IsNullOrWhiteSpace(gameData?.AuthenticationTokenHelpUrl);
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ApplyAvailability(bool hasGame)
		{
			txtPassword.Enabled = hasGame &&
				txtPassword.Tag?.ToString() == "Required";
			txtAdminPassword.Enabled = hasGame &&
				txtAdminPassword.Tag?.ToString() == "Required";
			txtAuthenticationToken.Enabled = hasGame &&
				_gameData?.RequiresAuthenticationToken == true;
			txtInviteCode.Enabled = hasGame && SupportsInviteCode;
		}

		public bool TryValidate(
			string serverName,
			string rconPassword,
			out string error)
		{
			if (_gameData == null)
			{
				error = string.Empty;
				return true;
			}

			return GameServerInputValidator.TryValidate(
				_gameData,
				serverName,
				new SynixServerPasswords(
					ServerPassword,
					AdminPassword,
					rconPassword,
					AuthenticationToken),
				out error);
		}

		public void FocusFirstRequiredInput()
		{
			if (RequiredAuthenticationTokenMissing)
				txtAuthenticationToken.Focus();
			else if (RequiredAdminPasswordMissing)
				txtAdminPassword.Focus();
			else
				txtPassword.Focus();
		}

		private void AuthenticationTokenHelpClicked(object? sender, EventArgs eventArgs)
		{
			if (_gameData == null ||
				!Uri.TryCreate(
					_gameData.AuthenticationTokenHelpUrl,
					UriKind.Absolute,
					out Uri? helpUri) ||
				helpUri.Scheme != Uri.UriSchemeHttps)
			{
				return;
			}

			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
					helpUri.AbsoluteUri)
				{
					UseShellExecute = true
				});
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(
					FindForm(),
					"open the authentication-token page",
					exception.Message);
			}
		}

		private void SettingsControlChanged(object? sender, EventArgs eventArgs)
		{
			if (!_isLoading)
				SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private static string ReadManagedValue(TextBox textBox)
		{
			return textBox.ForeColor == Color.Gray ||
				textBox.Text is "Select a game..." or "Not Required"
					? string.Empty
					: textBox.Text;
		}

		private static void ConfigureManagedTextBox(
			TextBox textBox,
			bool required,
			string placeholder)
		{
			textBox.GotFocus -= ManagedTextBoxGotFocus;
			textBox.LostFocus -= ManagedTextBoxLostFocus;
			textBox.Tag = required ? "Required" : placeholder;
			if (required)
			{
				if (textBox.ForeColor == Color.Gray ||
					textBox.Text is "Select a game..." or "Not Required")
				{
					textBox.Text = string.Empty;
				}
				textBox.ForeColor = SettingsPalette.PrimaryText;
				return;
			}

			textBox.ForeColor = Color.Gray;
			textBox.Text = placeholder;
			textBox.GotFocus += ManagedTextBoxGotFocus;
			textBox.LostFocus += ManagedTextBoxLostFocus;
		}

		private static void ManagedTextBoxGotFocus(object? sender, EventArgs eventArgs)
		{
			if (sender is TextBox textBox &&
				textBox.Text == textBox.Tag?.ToString())
			{
				textBox.Text = string.Empty;
				textBox.ForeColor = SettingsPalette.PrimaryText;
			}
		}

		private static void ManagedTextBoxLostFocus(object? sender, EventArgs eventArgs)
		{
			if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
			{
				textBox.ForeColor = Color.Gray;
				textBox.Text = textBox.Tag?.ToString() ?? string.Empty;
			}
		}
	}
}
