// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using System.Drawing;
using System.Windows.Forms;

namespace Synix_Control_Panel.UI
{
	public static class UIStyleHelper
	{
		// 🎯 This method handles the logic for any toggle-style CheckBox
		public static void StyleToggleButton(CheckBox chk, string labelPrefix)
		{
			if (chk.Checked)
			{
				chk.Text = $"{labelPrefix}: ON";
				chk.BackColor = Color.Green;
				chk.ForeColor = Color.White;
			}
			else
			{
				chk.Text = $"{labelPrefix}: OFF";
				chk.BackColor = Color.FromArgb(45, 45, 45); // Your theme dark gray
				chk.ForeColor = Color.Gray;
			}
		}
	}
}