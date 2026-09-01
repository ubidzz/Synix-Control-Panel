// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	internal enum AddServerChoice
	{
		None,
		CreateNew,
		ImportExisting,
		BrowseCatalog
	}

	internal sealed class AddServerChoiceDialog : Form
	{
		internal AddServerChoice SelectedChoice { get; private set; }

		internal AddServerChoiceDialog()
		{
			Text = "Add a Server";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(720, 430);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = "How would you like to add a server?",
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 24),
				Size = new Size(660, 42),
				ForeColor = SettingsPalette.PrimaryText
			});
			Controls.Add(new Label
			{
				Text = "Synix can install a new server or safely register files that are already on this PC.",
				Location = new Point(30, 70),
				Size = new Size(650, 28),
				ForeColor = SettingsPalette.SecondaryText
			});

			AddChoiceCard(
				"Create and install a new server",
				"Choose the game and settings, then let Synix download the server files.",
				112,
				"Create New",
				AddServerChoice.CreateNew,
				true);
			AddChoiceCard(
				"Import an existing server",
				"Point Synix to an existing server folder. Your files are not moved or replaced.",
				208,
				"Import Existing",
				AddServerChoice.ImportExisting,
				false);
			AddChoiceCard(
				"Check game support first",
				"Search the catalog to see executable, configuration, crossplay, and player-query support.",
				304,
				"View Catalog",
				AddServerChoice.BrowseCatalog,
				false);

			ThemeManager.Apply(this);
		}

		private void AddChoiceCard(
			string title,
			string description,
			int top,
			string buttonText,
			AddServerChoice choice,
			bool accent)
		{
			ModernSettingsCard card = new()
			{
				Location = new Point(28, top),
				Size = new Size(664, 80),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider
			};
			card.Controls.Add(new Label
			{
				Text = title,
				Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
				Location = new Point(18, 12),
				Size = new Size(430, 24),
				ForeColor = SettingsPalette.PrimaryText
			});
			card.Controls.Add(new Label
			{
				Text = description,
				Location = new Point(18, 39),
				Size = new Size(440, 34),
				ForeColor = SettingsPalette.SecondaryText
			});
			ModernSettingsButton button = new()
			{
				Text = buttonText,
				Location = new Point(486, 19),
				Size = new Size(158, 42),
				UseAccentStyle = accent
			};
			button.Click += (_, _) =>
			{
				SelectedChoice = choice;
				DialogResult = DialogResult.OK;
				Close();
			};
			card.Controls.Add(button);
			Controls.Add(card);
		}
	}
}
