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
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.Help
{
	public partial class DefaultArgumentsDisplay : Form
	{
		private const int WmNcLButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;

		public DefaultArgumentsDisplay(string requiredArgs)
		{
			InitializeComponent();
			ThemeManager.Apply(this);
			txtArgs.Text = requiredArgs;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
				return;

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLButtonDown, HtCaption, 0);
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);
	}
}
