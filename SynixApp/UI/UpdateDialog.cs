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
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed partial class SynixUpdateDialog : Form
	{
		public SynixUpdateDialog()
		{
			InitializeComponent();
		}

		public SynixUpdateDialog(SynixUpdateCheckResult check) : this()
		{
			ArgumentNullException.ThrowIfNull(check);
			SynixReleaseInfo release = check.Release ??
				throw new ArgumentException(
					"Release details are required for the update window.",
					nameof(check));

			titleLabel.Text = $"Synix {release.VersionText} is available";
			subtitleLabel.Text = $"Running {check.CurrentVersion.ToString(3)}  •  {check.Installation.DisplayName}";
			highlightsTextBox.Text = Core.BuildHighlights(release.Notes);
			verificationLabel.ForeColor = check.Asset is null
				? SettingsPalette.Warning
				: SettingsPalette.Success;
			verificationLabel.Text = check.Asset is null
				? "The matching download is not ready for automatic installation."
				: $"SHA-256 verified download  •  {FormatBytes(check.Asset.Size)}  •  {check.Asset.Name}";
			safetyMessageLabel.ForeColor = check.CanInstall
				? SettingsPalette.SecondaryText
				: SettingsPalette.Warning;
			safetyMessageLabel.Text = GetSafetyMessage(check);
			githubButton.Click += (_, _) => OpenUrl(release.ReleaseUri);
			fullNotesButton.Click += (_, _) =>
			{
				using SynixReleaseNotesDialog notesDialog = new(release);
				notesDialog.ShowDialog(this);
			};
			installButton.Text = check.CanInstall ? "Install Update" : "Install Unavailable";
			installButton.Enabled = check.CanInstall;
			installButton.Click += (_, _) =>
			{
				DialogResult = DialogResult.OK;
				Close();
			};

			ThemeManager.Apply(this);
		}

		private static string GetSafetyMessage(SynixUpdateCheckResult check)
		{
			if (check.Installation.Kind == SynixInstallationKind.Development)
			{
				return "Automatic installation is disabled for this Visual Studio build. " +
					"Published Setup and Standalone releases enable it automatically.";
			}

			if (!string.IsNullOrWhiteSpace(check.Problem))
				return check.Problem;

			return "All game servers must be stopped before installing. Synix will update only its program files; everything inside C:\\Synix remains unchanged.";
		}

		private static void OpenUrl(Uri uri)
		{
			try
			{
				Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
				{
					UseShellExecute = true
				});
			}
			catch
			{
				MessageBox.Show(
					"Windows could not open the GitHub release page.",
					"Unable to Open GitHub",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		private static string FormatBytes(long bytes)
		{
			double megabytes = bytes / 1024d / 1024d;
			return megabytes >= 1
				? $"{megabytes:0.##} MB"
				: $"{Math.Max(0, bytes / 1024d):0.##} KB";
		}
	}

	internal sealed class SynixReleaseNotesDialog : Form
	{
		public SynixReleaseNotesDialog(SynixReleaseInfo release)
		{
			Text = $"Synix {release.VersionText} Release Notes";
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.Sizable;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(820, 650);
			MinimumSize = new Size(620, 460);
			BackColor = SettingsPalette.Window;
			Font = new Font("Segoe UI", 10F);

			Label title = new()
			{
				Dock = DockStyle.Top,
				Height = 62,
				Padding = new Padding(22, 18, 22, 8),
				Font = new Font("Segoe UI", 15F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Text = $"Complete notes for Synix {release.VersionText}"
			};
			RichTextBox notes = new()
			{
				Dock = DockStyle.Fill,
				Margin = new Padding(22),
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.SecondaryText,
				BorderStyle = BorderStyle.None,
				ReadOnly = true,
				DetectUrls = true,
				ScrollBars = RichTextBoxScrollBars.ForcedVertical,
				Text = Core.FormatReleaseNotes(release.Notes),
				WordWrap = true
			};
			Panel notesHost = new()
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(22, 0, 22, 18),
				BackColor = SettingsPalette.Window
			};
			notesHost.Controls.Add(notes);
			Controls.Add(notesHost);
			Controls.Add(title);
			ThemeManager.Apply(this);
		}
	}
}
