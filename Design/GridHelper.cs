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
namespace Synix_Control_Panel.Design
{
	public static class GridHelper
	{
		/// <summary>
		/// Refreshes the DataGridView while preserving the user's scroll position and selected row.
		/// </summary>
		public static void RefreshWithPersistence(DataGridView dgv, object dataSource)
		{
			// 1. Capture the current state before the refresh
			int scrollPosition = dgv.FirstDisplayedScrollingRowIndex;
			int selectedIndex = dgv.CurrentRow != null ? dgv.CurrentRow.Index : -1;

			// 2. Perform the "Nuclear Refresh"
			dgv.DataSource = null;
			dgv.DataSource = dataSource;

			// 3. Restore the scroll bar position
			if (scrollPosition != -1 && scrollPosition < dgv.Rows.Count)
			{
				dgv.FirstDisplayedScrollingRowIndex = scrollPosition;
			}

			// 4. Restore the user's selection
			if (selectedIndex != -1 && selectedIndex < dgv.Rows.Count)
			{
				dgv.ClearSelection();
				dgv.Rows[selectedIndex].Selected = true;
			}

			// 5. Force the visual repaint
			dgv.Refresh();
		}
	}
}

