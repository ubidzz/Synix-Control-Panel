// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
namespace Synix_Control_Panel.SynixApp.UI.ServerSetup;

public partial class ServerSettingsReviewPage : UserControl
{
	public ServerSettingsReviewPage() => InitializeComponent();

	internal void SetSummary(IEnumerable<(string ResourceKey, string Value)> settings)
	{
		gridSummary.Rows.Clear();
		foreach ((string resourceKey, string value) in settings)
			gridSummary.Rows.Add(LocalizationManager.Get(resourceKey), value);
		gridSummary.ClearSelection();
		// Values are data-grid cells, not localizable labels: names, paths, map names
		// and game identifiers must remain exactly as the user entered them.
	}
}
