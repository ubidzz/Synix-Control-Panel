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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class GameDefinitionBuilder : Form
	{
		private sealed record BuilderOption<T>(string Label, T Value)
		{
			public override string ToString() => Label;
		}

		public GameDefinitionBuilder()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			ThemeManager.Apply(this);
			ApplyTemplateGridTheme();
			cmbConfigMode.DataSource = new BuilderOption<ConfigFileCreationMode>[]
			{
				new("Not verified yet", ConfigFileCreationMode.Unknown),
				new("Game creates it after first start", ConfigFileCreationMode.GameGenerated),
				new("Synix creates it from a template", ConfigFileCreationMode.SynixTemplate),
				new("Launch arguments only", ConfigFileCreationMode.LaunchArgumentsOnly)
			};
			cmbFormat.DataSource = new BuilderOption<ConfigFormat>[]
			{
				new("INI / CFG / properties", ConfigFormat.StandardINI),
				new("XML", ConfigFormat.XML),
				new("JSON", ConfigFormat.JSON),
				new("Space-separated values", ConfigFormat.Space)
			};
			cmbArgumentTag.DataSource = GameDefinitionArgumentTags.LaunchArguments
				.Select(tag => new BuilderOption<GameDefinitionArgumentTag>(
					$"{tag.Token} — {tag.Name}",
					tag))
				.ToArray();
			numCatalogOrder.Value = Math.Min(
				numCatalogOrder.Maximum,
				GameDefinitionAuthoring.GetNextCatalogOrder());
			SelectOption(cmbConfigMode, ConfigFileCreationMode.Unknown);
			SelectOption(cmbFormat, ConfigFormat.StandardINI);
			rtbGuide.Text = BuildGuideText();
			RefreshConfigurationControls();
			RefreshPostInstallControls();
			ShowGuide();
		}

		private void cmbConfigMode_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			RefreshConfigurationControls();
		}

		private void txtGame_Leave(object? sender, EventArgs eventArgs)
		{
			if (!string.IsNullOrWhiteSpace(txtId.Text))
				return;
			txtId.Text = CreateId(txtGame.Text);
		}

		private void btnBrowseTemplate_Click(
			object? sender,
			EventArgs eventArgs)
		{
			using OpenFileDialog dialog = new()
			{
				Title = "Select a complete game configuration template",
				Filter = "Configuration files|*.ini;*.cfg;*.json;*.xml;*.txt;*.properties|All files|*.*",
				CheckFileExists = true
			};
			if (dialog.ShowDialog(this) == DialogResult.OK)
				txtTemplate.Text = dialog.FileName;
		}

		private void btnAddTemplates_Click(
			object? sender,
			EventArgs eventArgs)
		{
			using OpenFileDialog dialog = new()
			{
				Title = "Add complete game configuration templates",
				Filter = "Configuration files|*.ini;*.cfg;*.json;*.xml;*.txt;*.properties;*.lua;*.yaml;*.yml|All files|*.*",
				CheckFileExists = true,
				Multiselect = true
			};
			if (dialog.ShowDialog(this) != DialogResult.OK)
				return;

			foreach (string file in dialog.FileNames)
			{
				if (string.IsNullOrWhiteSpace(txtTemplate.Text))
				{
					txtTemplate.Text = file;
					if (string.IsNullOrWhiteSpace(txtConfigPath.Text))
						txtConfigPath.Text = Path.GetFileName(file);
					continue;
				}

				dgvAdditionalTemplates.Rows.Add(
					Path.GetFileName(file),
					file);
			}
		}

		private void btnRemoveTemplate_Click(
			object? sender,
			EventArgs eventArgs)
		{
			foreach (DataGridViewRow row in dgvAdditionalTemplates.SelectedRows
				.Cast<DataGridViewRow>()
				.OrderByDescending(row => row.Index))
			{
				if (!row.IsNewRow)
					dgvAdditionalTemplates.Rows.RemoveAt(row.Index);
			}
		}

		private void btnInsertArgumentTag_Click(
			object? sender,
			EventArgs eventArgs)
		{
			if (cmbArgumentTag.SelectedItem is not
				BuilderOption<GameDefinitionArgumentTag> option)
			{
				return;
			}

			int selectionStart = txtArguments.SelectionStart;
			string spacerBefore = selectionStart > 0 &&
				!char.IsWhiteSpace(txtArguments.Text[selectionStart - 1])
					? " "
					: string.Empty;
			string insertion = spacerBefore + option.Value.Token;
			txtArguments.Text = txtArguments.Text.Insert(selectionStart, insertion);
			txtArguments.SelectionStart = selectionStart + insertion.Length;
			txtArguments.Focus();
		}

		private void btnShowGuide_Click(object? sender, EventArgs eventArgs) =>
			ShowGuide();

		private void btnShowPreview_Click(object? sender, EventArgs eventArgs) =>
			ShowPreview();

		private void chkSteamRuntime_CheckedChanged(
			object? sender,
			EventArgs eventArgs) =>
			RefreshPostInstallControls();

		private void btnValidate_Click(object? sender, EventArgs eventArgs)
		{
			try
			{
				GameDefinitionDraft draft = BuildDraft();
				EmbeddedGamePackage package =
					GameDefinitionAuthoring.ValidateDraft(draft);
				rtbPreview.Text = GameDefinitionAuthoring.CreateDefinitionJson(draft);
				lblStatus.ForeColor = SettingsPalette.Success;
				lblStatus.Text =
					$"Valid definition • revision {package.Definition.DefinitionRevision} • " +
					$"{package.Configuration?.Templates.Count ?? 0} template(s) • " +
					$"{package.PostInstallActions.Count} safe action(s)";
				ShowPreview();
			}
			catch (Exception exception)
			{
				lblStatus.ForeColor = SettingsPalette.Danger;
				lblStatus.Text = exception.Message;
			}
		}

		private void btnSave_Click(object? sender, EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
			{
				MessageBox.Show(
					this,
					"The Game Definition Builder is available only in development builds.",
					"Development Tool",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			try
			{
				string? projectDirectory =
					Core.FindProjectDirectory(AppContext.BaseDirectory) ??
					Core.FindProjectDirectory(Environment.CurrentDirectory);
				if (projectDirectory == null)
					throw new DirectoryNotFoundException(
						"Synix Control Panel.csproj could not be found from this development build.");

				GameDefinitionSaveResult result =
					GameDefinitionAuthoring.SaveDraft(
						BuildDraft(),
						projectDirectory);
				rtbPreview.Text = result.Json;
				lblStatus.ForeColor = SettingsPalette.Success;
				lblStatus.Text = $"Saved: {result.DefinitionPath}";

				DialogResult open = MessageBox.Show(
					this,
					"The validated definition and configuration templates were saved into the project. " +
					"Rebuild Synix and run the automated tests before using it.\n\n" +
					"Open the definition folder now?",
					"Game Definition Saved",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
				if (open == DialogResult.Yes)
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = Path.GetDirectoryName(result.DefinitionPath)!,
						UseShellExecute = true
					});
				}
			}
			catch (Exception exception)
			{
				lblStatus.ForeColor = SettingsPalette.Danger;
				lblStatus.Text = exception.Message;
				MessageBox.Show(
					this,
					exception.Message,
					"Definition Could Not Be Saved",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private GameDefinitionDraft BuildDraft()
		{
			return new GameDefinitionDraft
			{
				Id = txtId.Text,
				CatalogOrder = numCatalogOrder.Value,
				DefinitionRevision = numDefinitionRevision.Value,
				Game = txtGame.Text,
				AppId = txtAppId.Text,
				RequiresSteamLogin = chkSteamLogin.Checked,
				SteamAppConfig = txtSteamAppConfig.Text,
				Executable = txtExecutable.Text,
				Arguments = txtArguments.Text,
				RconSyntax = txtRconSyntax.Text,
				Port = numPort.Value,
				QueryPort = numQueryPort.Value,
				Maps = ParseList(txtMaps.Text),
				GameModes = ParseList(txtGameModes.Text),
				ConfigFileCreation = GetSelectedValue(
					cmbConfigMode,
					ConfigFileCreationMode.Unknown),
				Format = GetSelectedValue(cmbFormat, ConfigFormat.StandardINI),
				RelativeConfigPath = txtConfigPath.Text,
				TemplateSourcePath = txtTemplate.Text,
				AdditionalTemplates = ReadAdditionalTemplates(),
				ConfigurationRevision = numConfigRevision.Value,
				ExternalDataFolderName = txtExternalDataFolder.Text,
				RequiredLaunchFiles = ParseList(txtRequiredLaunchFiles.Text),
				OptionalLaunchFiles = ParseList(txtOptionalLaunchFiles.Text),
				LaunchFileSetupInstructions = txtSetupInstructions.Text,
				NeedsConfigWarning = chkFirstStartWarning.Checked,
				WarningMessage = txtWarningMessage.Text,
				IconUrl = txtIconUrl.Text,
				CopySteamRuntimeFiles = chkSteamRuntime.Checked,
				SteamRuntimeTargetDirectory = txtSteamRuntimeTarget.Text,
				IsQueryable = chkQueryable.Checked
			};
		}

		private void RefreshConfigurationControls()
		{
			ConfigFileCreationMode mode = GetSelectedValue(
				cmbConfigMode,
				ConfigFileCreationMode.Unknown);
			bool enabled = mode == ConfigFileCreationMode.SynixTemplate;
			txtConfigPath.Enabled = enabled;
			cmbFormat.Enabled = enabled;
			txtTemplate.Enabled = enabled;
			btnBrowseTemplate.Enabled = enabled;
			numConfigRevision.Enabled = enabled;
			dgvAdditionalTemplates.Enabled = enabled;
			btnAddTemplates.Enabled = enabled;
			btnRemoveTemplate.Enabled = enabled;
			lblConfigModeHelp.Text = mode switch
			{
				ConfigFileCreationMode.SynixTemplate =>
					"Synix writes this complete template before the first start and manages its supported values.",
				ConfigFileCreationMode.GameGenerated =>
					"The game creates the file first. The captured complete file becomes Synix's repair and editing template.",
				ConfigFileCreationMode.LaunchArgumentsOnly =>
					"The required user settings are passed through launch arguments; no managed configuration is needed.",
				_ =>
					"Use this only while the game's configuration behavior is still being researched."
			};
		}

		private void RefreshPostInstallControls()
		{
			txtSteamRuntimeTarget.Enabled = chkSteamRuntime.Checked;
		}

		private IReadOnlyList<GameDefinitionTemplateDraft> ReadAdditionalTemplates()
		{
			dgvAdditionalTemplates.EndEdit();
			return dgvAdditionalTemplates.Rows
				.Cast<DataGridViewRow>()
				.Where(row => !row.IsNewRow)
				.Select(row => new GameDefinitionTemplateDraft(
					Convert.ToString(row.Cells[colTemplateDestination.Index].Value)?.Trim() ?? string.Empty,
					Convert.ToString(row.Cells[colTemplateSource.Index].Value)?.Trim() ?? string.Empty))
				.Where(template =>
					!string.IsNullOrWhiteSpace(template.RelativePath) ||
					!string.IsNullOrWhiteSpace(template.SourcePath))
				.ToArray();
		}

		private void ApplyTemplateGridTheme()
		{
			dgvAdditionalTemplates.BackgroundColor = SettingsPalette.Input;
			dgvAdditionalTemplates.GridColor = SettingsPalette.Divider;
			dgvAdditionalTemplates.ColumnHeadersDefaultCellStyle.BackColor = SettingsPalette.Sidebar;
			dgvAdditionalTemplates.ColumnHeadersDefaultCellStyle.ForeColor = SettingsPalette.SecondaryText;
			dgvAdditionalTemplates.DefaultCellStyle.BackColor = SettingsPalette.Input;
			dgvAdditionalTemplates.DefaultCellStyle.ForeColor = SettingsPalette.PrimaryText;
			dgvAdditionalTemplates.DefaultCellStyle.SelectionBackColor = SettingsPalette.Selection;
			dgvAdditionalTemplates.DefaultCellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
		}

		private void ShowGuide()
		{
			rtbGuide.Visible = true;
			rtbPreview.Visible = false;
			lblRightPane.Text = "Builder guide and supported tags";
			btnShowGuide.UseAccentStyle = true;
			btnShowPreview.UseAccentStyle = false;
			btnShowGuide.Invalidate();
			btnShowPreview.Invalidate();
		}

		private void ShowPreview()
		{
			rtbGuide.Visible = false;
			rtbPreview.Visible = true;
			lblRightPane.Text = "Validated definition preview";
			btnShowGuide.UseAccentStyle = false;
			btnShowPreview.UseAccentStyle = true;
			btnShowGuide.Invalidate();
			btnShowPreview.Invalidate();
		}

		private static T GetSelectedValue<T>(ComboBox comboBox, T fallback) =>
			comboBox.SelectedItem is BuilderOption<T> option
				? option.Value
				: fallback;

		private static void SelectOption<T>(ComboBox comboBox, T value)
			where T : struct, Enum
		{
			for (int index = 0; index < comboBox.Items.Count; index++)
			{
				if (comboBox.Items[index] is BuilderOption<T> option &&
					EqualityComparer<T>.Default.Equals(option.Value, value))
				{
					comboBox.SelectedIndex = index;
					return;
				}
			}
		}

		private static string BuildGuideText()
		{
			StringBuilder guide = new();
			guide.AppendLine("QUICK START");
			guide.AppendLine("1. Enter the exact game name and numeric Steam server AppID.");
			guide.AppendLine("2. Enter the server executable path relative to the installed server folder.");
			guide.AppendLine("3. Enter only the command text that comes after the executable. Use the supported tags below where Synix must insert a user's setting.");
			guide.AppendLine("4. Choose how the game gets its configuration file. A managed mode requires a complete, working configuration file—not a partial example.");
			guide.AppendLine("5. Select Validate & Preview. Fix every reported problem before saving.");
			guide.AppendLine("6. Save, rebuild Synix, run the automated tests, then test install/start/stop/monitoring for the game.");
			guide.AppendLine();
			guide.AppendLine("REQUIRED FIELDS");
			guide.AppendLine("• Game name: the name users see in Synix.");
			guide.AppendLine("• Definition ID: unique lowercase ID; Synix creates it from the game name for you.");
			guide.AppendLine("• Steam AppID: dedicated-server AppID, not always the client game AppID.");
			guide.AppendLine("• SteamCMD app configuration: normally blank. Shared GoldSrc AppID 90 games use the verified form '90 mod folder', such as '90 mod cstrike'. Synix accepts only this safe form.");
			guide.AppendLine("• Executable: for example Binaries\\Win64\\Server.exe. Do not include the install folder.");
			guide.AppendLine("• Ports: verified default game and query ports.");
			guide.AppendLine("• Maps and game modes: enter one exact game value per line. These populate the choices shown while creating or editing a server.");
			guide.AppendLine();
			guide.AppendLine("LAUNCH ARGUMENTS");
			guide.AppendLine("Do not enter the executable itself. Keep the game's fixed flags as normal text and insert tags only for values controlled by Synix.");
			guide.AppendLine("Example: -port {port} -queryport {query} -name \"{ServerName}\" -maxplayers {MaxPlayers}");
			guide.AppendLine();
			guide.AppendLine("SUPPORTED ARGUMENT TAGS");
			foreach (GameDefinitionArgumentTag tag in GameDefinitionArgumentTags.LaunchArguments)
				guide.AppendLine($"{tag.Token}  {tag.Description}");
			guide.AppendLine();
			guide.AppendLine("RCON RECIPE");
			guide.AppendLine("Put {rcon} in the launch arguments where the optional RCON command belongs. In RCON syntax, use:");
			foreach (GameDefinitionArgumentTag tag in GameDefinitionArgumentTags.RconSyntax)
				guide.AppendLine($"{tag.Token}  {tag.Description}");
			guide.AppendLine("Example RCON syntax: +rcon.port {rcon_port} +rcon.password \"{rcon_pass}\"");
			guide.AppendLine();
			guide.AppendLine("CONFIGURATION BEHAVIOR");
			guide.AppendLine("• Not verified yet: Synix does not manage a config file.");
			guide.AppendLine("• Game creates it after first start: use a complete captured config as the editing and repair template.");
			guide.AppendLine("• Synix creates it from a template: Synix places the complete config before first start.");
			guide.AppendLine("• Launch arguments only: the game does not require a managed config for these settings.");
			guide.AppendLine();
			guide.AppendLine("CONFIGURATION TEMPLATE TAGS");
			guide.AppendLine("{ServerName}, {Password}, {AdminPassword}, {MaxPlayers}, {Port}, {QueryPort}, {RCONPort}, {RCONPassword}, {EnableRcon}, {Identity}, {WorldName}, {WorldSeed}, {WorldSize}, {AppPort}, {LocalIP}, {PublicIP}, {IsPvp}, {IsPve}, {GameMode}");
			guide.AppendLine("Use Add configuration files when a server needs more than one file. Select each complete source file, then edit its Installed location in the table so the path is relative to the server folder. Synix validates, embeds, writes, repairs, and upgrades every listed file together.");
			guide.AppendLine();
			guide.AppendLine("REQUIRED USER-SUPPLIED FILES");
			guide.AppendLine("Enter one safe path per line, relative to the installed server folder. Required files block startup until present; optional files are imported when found but never block startup.");
			guide.AppendLine("When required files come from the normal game, explain every user step in Setup instructions. Synix never downloads, bypasses ownership, or redistributes licensed player files.");
			guide.AppendLine("External data folder is the folder name Synix may look for under the user's Documents folders. Leave it empty when the user must copy the files manually.");
			guide.AppendLine();
			guide.AppendLine("FIRST-START WARNING AND ICON");
			guide.AppendLine("Enable the warning when users must complete setup before the first launch. Write the exact steps and identify anything the user must obtain from their own game installation.");
			guide.AppendLine("Icon URL is optional and must use HTTPS. Synix falls back to the installed server executable icon when it is blank.");
			guide.AppendLine();
			guide.AppendLine("REVISIONS AND SAFETY");
			guide.AppendLine("Start new definitions and templates at revision 1. Increase definition revision whenever the game definition changes. Increase template revision only when the managed config layout changes. Synix refuses an overwrite without a higher revision and preserves upgrade backups.");
			guide.AppendLine();
			guide.AppendLine("POST-INSTALL OPTION");
			guide.AppendLine("Enable Steam runtime copying only when the server has been verified to require it. The target must be a relative folder inside that server installation. Synix copies only its three allowlisted Steam runtime DLLs.");
			return guide.ToString();
		}

		private static string CreateId(string name)
		{
			string id = Regex.Replace(
				name.Trim().ToLowerInvariant(),
				"[^a-z0-9]+",
				"-").Trim('-');
			return id;
		}

		private static IReadOnlyList<string> ParseList(string value) =>
			value
				.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
				.Select(item => item.Trim())
				.Where(item => item.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
	}
}
