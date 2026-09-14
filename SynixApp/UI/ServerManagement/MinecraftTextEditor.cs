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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class MinecraftTextEditor : Form
{
	private readonly TextBox _text;
	private readonly string _original;

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (Properties.Settings.Default.PrivacyMode)
			_ = SetWindowDisplayAffinity(Handle, 0x00000011);
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
	private static extern bool SetWindowDisplayAffinity(IntPtr window, uint affinity);

	internal MinecraftTextEditor(GameServer server, string relative)
	{
		string path = ModPathSafety.Resolve(server.InstallPath, relative);
		if (new FileInfo(path).Length > 4 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
		string? hash = MinecraftContentTransactions.HashFile(path);
		_original = ConfigurationTextSnapshot.Read(path).Text;
		Text = relative;
		ClientSize = new Size(1000, 720);
		MinimumSize = new Size(720, 520);
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;
		Font = new Font("Segoe UI", 10);
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3, ColumnCount = 1 };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
		layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = MinecraftControlCenter.TextFor("EditorHelp"),
			ForeColor = SettingsPalette.SecondaryText }, 0, 0);
		_text = new TextBox { Name = "minecraftFileText", Multiline = true, AcceptsTab = true, AcceptsReturn = true,
			WordWrap = false, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
			Font = new Font("Cascadia Mono", 10), Text = _original, MaxLength = 4 * 1024 * 1024,
			BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText };
		ModernSettingsCard inputCard = new() { Dock = DockStyle.Fill, Padding = new Padding(12),
			FillColor = SettingsPalette.Input, BackColor = SettingsPalette.Input, Margin = new Padding(3, 3, 3, 10) };
		inputCard.Controls.Add(_text);
		layout.Controls.Add(inputCard, 0, 1);
		FlowLayoutPanel buttons = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
		ModernSettingsButton save = new() { Text = MinecraftControlCenter.TextFor("Save"), Width = 150, Height = 42, UseAccentStyle = true };
		save.Click += (_, _) =>
		{
			try
			{
				if (_text.Text == _original) { DialogResult = DialogResult.OK; return; }
				ConfigFormat? format = Path.GetExtension(path).ToLowerInvariant() switch
				{ ".json" => ConfigFormat.JSON, ".yaml" or ".yml" => ConfigFormat.YAML, ".xml" => ConfigFormat.XML, _ => null };
				if (format.HasValue) _ = ConfigHandler.LoadConfigText(_text.Text, format.Value);
				MinecraftConfigurationSync.Save(server, path, _text.Text, hash, FileHandler.SaveServers);
				DialogResult = DialogResult.OK;
			}
			catch (Exception exception) { LocalizedMessageBox.Show(this, Core.SanitizeProblemReportText(exception.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
		};
		buttons.Controls.Add(save);
		layout.Controls.Add(buttons, 0, 2);
		Controls.Add(layout);
		FormClosing += (_, e) =>
		{
			if (DialogResult != DialogResult.OK && _text.Text != _original &&
				LocalizedMessageBox.Show(this, MinecraftControlCenter.TextFor("Discard"), Text,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				e.Cancel = true;
		};
		ThemeManager.Apply(this);
	}
}
