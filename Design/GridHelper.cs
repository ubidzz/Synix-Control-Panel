/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
