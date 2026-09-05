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
using System.Drawing.Drawing2D;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.Design.Controls
{
	[ToolboxItem(false)]
	public sealed class ModernSettingsDataGridViewInformationalCell :
   DataGridViewTextBoxCell
	{
		protected override void Paint(
			Graphics graphics,
			Rectangle clipBounds,
			Rectangle cellBounds,
			int rowIndex,
			DataGridViewElementStates cellState,
			object? value,
			object? formattedValue,
			string? errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle,
			DataGridViewPaintParts paintParts)
		{
			DataGridViewElementStates informationalState =
				cellState & ~DataGridViewElementStates.Selected;
			DataGridViewPaintParts informationalPaintParts =
				paintParts & ~DataGridViewPaintParts.Focus;

			base.Paint(
				graphics,
				clipBounds,
				cellBounds,
				rowIndex,
				informationalState,
				value,
				formattedValue,
				errorText,
				cellStyle,
				advancedBorderStyle,
				informationalPaintParts);
		}
	}
}
