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

namespace Synix_Control_Panel.SynixEngine
{
	public partial class BackupSettingsPage : UserControl
	{
		private readonly Synix_Control_Panel.SynixApp.Design.ModernSettingsButton
			_exportSynixButton;
		private readonly Synix_Control_Panel.SynixApp.Design.ModernSettingsButton
			_importSynixButton;
		private readonly Label _transferStatusLabel;
		private readonly ProgressBar _transferProgressBar;

		public BackupSettingsPage()
		{
			InitializeComponent();
			(
				_exportSynixButton,
				_importSynixButton,
				_transferStatusLabel,
				_transferProgressBar) = CreateTransferCard();
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
		public event EventHandler? ImportSynixRequested
		{
			add => _importSynixButton.Click += value;
			remove => _importSynixButton.Click -= value;
		}

		public void SetTransferBusy(bool busy)
		{
			_exportSynixButton.Enabled = !busy;
			_importSynixButton.Enabled = !busy;
			_transferProgressBar.Visible = busy;

			if (!busy && _transferProgressBar.Value < 100)
			{
				_transferProgressBar.Value = 0;
			}
		}

		public void ReportTransferProgress(SynixTransferProgress progress)
		{
			_transferStatusLabel.Text = progress.Message;
			_transferProgressBar.Value = Math.Clamp(progress.Percent, 0, 100);
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
			Synix_Control_Panel.SynixApp.Design.ModernSettingsButton ExportButton,
			Synix_Control_Panel.SynixApp.Design.ModernSettingsButton ImportButton,
			Label StatusLabel,
			ProgressBar ProgressBar) CreateTransferCard()
		{
			Synix_Control_Panel.SynixApp.Design.ModernSettingsCard card = new()
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
				Text = "Move Synix to another PC"
			};
			Label description = new()
			{
				AutoSize = false,
				Location = new Point(22, 47),
				Size = new Size(774, 42),
				Font = new Font("Segoe UI", 9.5F),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = "Create one password-protected file containing everything in C:\\Synix, or restore it on a new computer.\n" +
					"This process can take some time depending on how much data needs to be packaged."
			};

			Synix_Control_Panel.SynixApp.Design.ModernSettingsButton exportButton = new()
			{
				Location = new Point(22, 98),
				Size = new Size(148, 42),
				Text = "Export Synix",
				UseAccentStyle = true
			};
			Synix_Control_Panel.SynixApp.Design.ModernSettingsButton importButton = new()
			{
				Location = new Point(182, 98),
				Size = new Size(148, 42),
				Text = "Import Synix"
			};
			Label statusLabel = new()
			{
				AutoEllipsis = true,
				Location = new Point(346, 98),
				Size = new Size(450, 22),
				Font = new Font("Segoe UI", 9F),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = "All servers must be stopped before transferring."
			};
			ProgressBar progressBar = new()
			{
				Location = new Point(346, 124),
				Size = new Size(450, 16),
				Style = ProgressBarStyle.Continuous,
				Visible = false
			};

			card.Controls.Add(title);
			card.Controls.Add(description);
			card.Controls.Add(exportButton);
			card.Controls.Add(importButton);
			card.Controls.Add(statusLabel);
			card.Controls.Add(progressBar);
			Controls.Add(card);
			card.BringToFront();

			return (exportButton, importButton, statusLabel, progressBar);
		}
	}
}
