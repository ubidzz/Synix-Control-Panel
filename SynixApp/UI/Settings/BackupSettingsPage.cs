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
using System.ComponentModel;

namespace Synix_Control_Panel.SynixApp.UI.Settings
{
	public partial class BackupSettingsPage : UserControl
	{
		private readonly Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton
			_exportSynixButton;
		private readonly Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton
			_normalExportButton;
		private readonly Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton
			_importSynixButton;
		private readonly Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton
			_verifyPackageButton;
		private readonly Label _transferStatusLabel;
		private readonly Label _transferEtaLabel;
		private readonly ProgressBar _transferProgressBar;
		private readonly Label _exportEstimateLabel;
		private readonly Label _importEstimateLabel;
		private DateTime _transferStartedUtc;
		private bool _verifyPackageReady;

		public BackupSettingsPage()
		{
			InitializeComponent();
			(
				_exportSynixButton,
				_normalExportButton,
				_importSynixButton,
				_verifyPackageButton,
				_transferStatusLabel,
				_transferEtaLabel,
				_transferProgressBar,
				_exportEstimateLabel,
				_importEstimateLabel) = CreateTransferCard();
			UpdatePathState();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UseCustomBackupPath
		{
			get => chkCustomBackup.Checked;
			set
			{
				chkCustomBackup.Checked = value;
				UpdatePathState();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BackupPath
		{
			get => txtBackupPath.Text;
			set => txtBackupPath.Text = value ?? string.Empty;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaximumBackups
		{
			get => decimal.ToInt32(numMaxBackups.Value);
			set
			{
				int clampedValue = Math.Clamp(
					value,
					decimal.ToInt32(numMaxBackups.Minimum),
					decimal.ToInt32(numMaxBackups.Maximum));
				numMaxBackups.Value = clampedValue;
			}
		}

		[Browsable(false)]
		public event EventHandler? CustomBackupChanged
		{
			add => chkCustomBackup.CheckedChanged += value;
			remove => chkCustomBackup.CheckedChanged -= value;
		}

		[Browsable(false)]
		public event EventHandler? BrowseRequested
		{
			add => btnBrowseBackup.Click += value;
			remove => btnBrowseBackup.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? MaximumBackupsChanged
		{
			add => numMaxBackups.ValueChanged += value;
			remove => numMaxBackups.ValueChanged -= value;
		}

		[Browsable(false)]
		public event EventHandler? ExportSynixRequested
		{
			add => _exportSynixButton.Click += value;
			remove => _exportSynixButton.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? NormalExportRequested
		{
			add => _normalExportButton.Click += value;
			remove => _normalExportButton.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? ImportSynixRequested
		{
			add => _importSynixButton.Click += value;
			remove => _importSynixButton.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? VerifyPackageRequested
		{
			add => _verifyPackageButton.Click += value;
			remove => _verifyPackageButton.Click -= value;
		}

		public void SetVerifyPackageReady(bool ready)
		{
			_verifyPackageReady = ready;
			_verifyPackageButton.Enabled =
				ready && !_transferProgressBar.Visible;
		}

		public void SetTransferBusy(bool busy)
		{
			_exportSynixButton.Enabled = !busy;
			_normalExportButton.Enabled = !busy;
			_importSynixButton.Enabled = !busy;
			_verifyPackageButton.Enabled = !busy && _verifyPackageReady;
			_exportSynixButton.Visible = !busy;
			_normalExportButton.Visible = !busy;
			_importSynixButton.Visible = !busy;
			_verifyPackageButton.Visible = !busy;
			_transferProgressBar.Visible = busy;
			if (busy)
			{
				_transferStatusLabel.Location = new Point(22, 132);
				_transferStatusLabel.Size = new Size(774, 19);
				_transferEtaLabel.Location = new Point(22, 151);
				_transferEtaLabel.Size = new Size(774, 17);
				_transferProgressBar.Location = new Point(22, 169);
				_transferProgressBar.Size = new Size(774, 9);
				_transferStartedUtc = DateTime.UtcNow;
				_transferEtaLabel.Text = string.Empty;
				_transferEtaLabel.Visible = false;
			}
			else
			{
				_transferStatusLabel.Location = new Point(570, 132);
				_transferStatusLabel.Size = new Size(226, 19);
				_transferEtaLabel.Location = new Point(570, 151);
				_transferEtaLabel.Size = new Size(226, 17);
				_transferProgressBar.Location = new Point(570, 169);
				_transferProgressBar.Size = new Size(226, 9);
				_transferEtaLabel.Visible = false;
			}

			if (!busy && _transferProgressBar.Value < 100)
			{
				_transferProgressBar.Value = 0;
			}
		}

		public void ReportTransferProgress(SynixTransferProgress progress)
		{
			string etaText = string.Empty;
			if (progress.TotalBytes > 0 &&
				progress.BytesProcessed > 0 &&
				progress.BytesProcessed < progress.TotalBytes)
			{
				double elapsedSeconds = Math.Max(
					0,
					(DateTime.UtcNow - _transferStartedUtc).TotalSeconds);
				if (elapsedSeconds >= 3)
				{
					double bytesPerSecond =
						progress.BytesProcessed / elapsedSeconds;
					if (bytesPerSecond > 0)
					{
						double remainingSeconds =
						(progress.TotalBytes - progress.BytesProcessed) /
						bytesPerSecond;
						etaText = LocalizationManager.Get(
							"Settings.Backup.Transfer.Eta",
							FormatDuration(remainingSeconds));
					}
				}
			}

			_transferStatusLabel.Text =
				LocalizationManager.TranslateRuntimeText(progress.Message);
			_transferEtaLabel.Text = etaText;
			_transferEtaLabel.Visible =
				_transferProgressBar.Visible && etaText.Length > 0;
			_transferProgressBar.Value = Math.Clamp(progress.Percent, 0, 100);
		}

		public void ShowExportEstimate(
			long sourceBytes,
			int fileCount,
			long estimatedPackageBytes,
			string encryptedTime,
			string normalTime)
		{
			LocalizationManager.BindText(
				_exportEstimateLabel,
				"Settings.Backup.Transfer.ExportEstimate",
				FormatBytes(sourceBytes),
				fileCount,
				FormatBytes(estimatedPackageBytes),
				encryptedTime,
				normalTime);
		}

		public void ShowImportEstimate(
			string packageName,
			long dataBytes,
			long additionalSpaceBytes,
			string estimatedTime,
			bool lowDiskFormat,
			bool passwordProtected)
		{
			LocalizationManager.BindText(
				_importEstimateLabel,
				"Settings.Backup.Transfer.ImportEstimate",
				packageName,
				FormatBytes(dataBytes),
				FormatBytes(additionalSpaceBytes),
				estimatedTime,
				LocalizationManager.Get(
					passwordProtected
						? "Settings.Backup.Transfer.Encrypted"
						: "Settings.Backup.Transfer.Unencrypted"),
				LocalizationManager.Get(
					lowDiskFormat
						? "Settings.Backup.Transfer.LowDisk"
						: "Settings.Backup.Transfer.Legacy"));
			LocalizationManager.BindText(
				_importSynixButton,
				"Text.617663EEC34944ABC976");
		}

		public void ShowImportSelectionPrompt()
		{
			LocalizationManager.BindText(
				_importEstimateLabel,
				"Text.5D385B4C5044DA97B77C");
			LocalizationManager.BindText(
				_importSynixButton,
				"Text.2D57929BE2D29C7BE3DC");
		}

		public void SetImportReady(bool ready)
		{
			LocalizationManager.BindText(
				_importSynixButton,
				ready
					? "Text.617663EEC34944ABC976"
					: "DynamicText.AB80BEDBA7075DEA64E2");
		}

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
			double value = Math.Max(0, bytes);
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}

			return $"{value:0.##} {units[unit]}";
		}

		private static string FormatDuration(double seconds)
		{
			TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, seconds));
			if (duration.TotalHours >= 1)
			{
				return LocalizationManager.Get(
					"Settings.Backup.Duration.Hours",
					(int)duration.TotalHours,
					duration.Minutes);
			}

			if (duration.TotalMinutes >= 1)
			{
				return LocalizationManager.Get(
					"Settings.Backup.Duration.Minutes",
					Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes)));
			}

			return LocalizationManager.Get(
				"Settings.Backup.Duration.Seconds",
				Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds)));
		}

		private void chkCustomBackup_CheckedChanged(
			object? sender,
			EventArgs eventArgs)
		{
			UpdatePathState();
		}

		private void UpdatePathState()
		{
			bool enabled = chkCustomBackup.Checked;
			btnBrowseBackup.Enabled = enabled;
			txtBackupPath.ForeColor = enabled
				? Color.FromArgb(245, 247, 251)
				: Color.FromArgb(105, 124, 153);
			backupPathHost.BorderColor = enabled
				? Color.FromArgb(55, 76, 108)
				: Color.FromArgb(38, 52, 77);
			backupPathHost.Invalidate();
		}

		private (
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton ExportButton,
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton NormalExportButton,
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton ImportButton,
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton VerifyButton,
			Label StatusLabel,
			Label EtaLabel,
			ProgressBar ProgressBar,
			Label ExportEstimateLabel,
			Label ImportEstimateLabel) CreateTransferCard()
		{
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsCard card = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = Color.FromArgb(17, 27, 45),
				BorderColor = Color.FromArgb(38, 52, 77),
				CornerRadius = 13,
				FillColor = Color.FromArgb(17, 27, 45),
				Location = new Point(0, 326),
				Padding = new Padding(22, 16, 22, 16),
				Size = new Size(818, 184)
			};

			Label title = new()
			{
				AutoSize = false,
				Location = new Point(22, 15),
				Size = new Size(520, 30),
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = Color.FromArgb(245, 247, 251),
				Text = LocalizationManager.Get("DynamicText.78A8F90383342A65EBBA")
			};
			Label description = new()
			{
				AutoSize = false,
				Location = new Point(22, 45),
				Size = new Size(774, 34),
				Font = new Font("Segoe UI", 9.5F),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = LocalizationManager.Get("Settings.Backup.Transfer.Description")
			};

			Label exportEstimateLabel = new()
			{
				AutoEllipsis = true,
				Location = new Point(22, 79),
				Size = new Size(374, 51),
				Font = new Font("Segoe UI", 8.2F),
				ForeColor = Color.FromArgb(125, 230, 221),
				Text = LocalizationManager.Get("DynamicText.CBBA62AF17CFB9FA453B")
			};
			Label importEstimateLabel = new()
			{
				AutoEllipsis = true,
				Location = new Point(410, 79),
				Size = new Size(386, 51),
				Font = new Font("Segoe UI", 8.5F),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = LocalizationManager.Get("Text.5D385B4C5044DA97B77C")
			};

			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton exportButton = new()
			{
				Location = new Point(22, 136),
				Size = new Size(128, 36),
				Text = LocalizationManager.Get("Text.35632C4A3CA55AE70481"),
				UseAccentStyle = true
			};
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton normalExportButton = new()
			{
				Location = new Point(158, 136),
				Size = new Size(128, 36),
				Text = LocalizationManager.Get("DynamicText.975D0C0D2656474C5CD5")
			};
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton importButton = new()
			{
				Location = new Point(294, 136),
				Size = new Size(128, 36),
				Text = LocalizationManager.Get("Text.2D57929BE2D29C7BE3DC")
			};
			Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton verifyButton = new()
			{
				Enabled = false,
				Location = new Point(430, 136),
				Size = new Size(128, 36),
				Text = LocalizationManager.Get("DynamicText.A728732ECF56432BF453")
			};
			Label statusLabel = new()
			{
				AutoEllipsis = true,
				Location = new Point(570, 132),
				Size = new Size(226, 19),
				Font = new Font("Segoe UI", 9F),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = LocalizationManager.Get("DynamicText.C1710D59A83DE3DB42F8")
			};
			Label etaLabel = new()
			{
				AutoEllipsis = false,
				Location = new Point(570, 151),
				Size = new Size(226, 17),
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
				ForeColor = Color.FromArgb(125, 230, 221),
				TextAlign = ContentAlignment.MiddleRight,
				Visible = false
			};
			ProgressBar progressBar = new()
			{
				Location = new Point(570, 169),
				Size = new Size(226, 9),
				Style = ProgressBarStyle.Continuous,
				Visible = false
			};

			card.Controls.Add(title);
			card.Controls.Add(description);
			card.Controls.Add(exportEstimateLabel);
			card.Controls.Add(importEstimateLabel);
			card.Controls.Add(exportButton);
			card.Controls.Add(normalExportButton);
			card.Controls.Add(importButton);
			card.Controls.Add(verifyButton);
			card.Controls.Add(statusLabel);
			card.Controls.Add(etaLabel);
			card.Controls.Add(progressBar);
			Controls.Add(card);
			card.BringToFront();

			return (
				exportButton,
				normalExportButton,
				importButton,
				verifyButton,
				statusLabel,
				etaLabel,
				progressBar,
				exportEstimateLabel,
				importEstimateLabel);
		}
	}
}
