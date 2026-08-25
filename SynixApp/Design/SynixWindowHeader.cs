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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixApp.Design
{
	internal static class SynixWindowHeader
	{
		private const int HeaderHeight = 56;
		private const int WindowResizeBorder = 7;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int WmNcHitTest = 0x0084;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 0x000A;
		private const int HtRight = 0x000B;
		private const int HtTop = 0x000C;
		private const int HtTopLeft = 0x000D;
		private const int HtTopRight = 0x000E;
		private const int HtBottom = 0x000F;
		private const int HtBottomLeft = 0x0010;
		private const int HtBottomRight = 0x0011;

		private sealed class HeaderRegistration
		{
			internal required Panel Header { get; init; }
			internal required Label Title { get; init; }
			internal BorderlessResizeWindow? ResizeWindow { get; init; }
		}

		private sealed class BorderlessResizeWindow : NativeWindow
		{
			private readonly Form _form;

			internal BorderlessResizeWindow(Form form)
			{
				_form = form;
				_form.HandleCreated += Form_HandleCreated;
				_form.HandleDestroyed += Form_HandleDestroyed;
				if (_form.IsHandleCreated)
					AssignHandle(_form.Handle);
			}

			private void Form_HandleCreated(object? sender, EventArgs eventArgs)
			{
				if (Handle == IntPtr.Zero)
					AssignHandle(_form.Handle);
			}

			private void Form_HandleDestroyed(object? sender, EventArgs eventArgs)
			{
				if (Handle != IntPtr.Zero)
					ReleaseHandle();
			}

			protected override void WndProc(ref Message message)
			{
				base.WndProc(ref message);
				if (message.Msg != WmNcHitTest ||
					_form.WindowState != FormWindowState.Normal ||
					(int)message.Result != 1)
				{
					return;
				}

				Point cursor = _form.PointToClient(Cursor.Position);
				bool left = cursor.X <= WindowResizeBorder;
				bool right = cursor.X >= _form.ClientSize.Width - WindowResizeBorder;
				bool top = cursor.Y <= WindowResizeBorder;
				bool bottom = cursor.Y >= _form.ClientSize.Height - WindowResizeBorder;

				if (left && top)
					message.Result = (IntPtr)HtTopLeft;
				else if (right && top)
					message.Result = (IntPtr)HtTopRight;
				else if (left && bottom)
					message.Result = (IntPtr)HtBottomLeft;
				else if (right && bottom)
					message.Result = (IntPtr)HtBottomRight;
				else if (left)
					message.Result = (IntPtr)HtLeft;
				else if (right)
					message.Result = (IntPtr)HtRight;
				else if (top)
					message.Result = (IntPtr)HtTop;
				else if (bottom)
					message.Result = (IntPtr)HtBottom;
			}
		}

		private static readonly ConditionalWeakTable<Form, HeaderRegistration>
			Registrations = new();

		internal static void Apply(Form form)
		{
			if (form.IsDisposed ||
				LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				form.FormBorderStyle == FormBorderStyle.None ||
				Registrations.TryGetValue(form, out _))
			{
				return;
			}

			FormBorderStyle originalBorderStyle = form.FormBorderStyle;
			bool canResize = originalBorderStyle is
				FormBorderStyle.Sizable or FormBorderStyle.SizableToolWindow;
			bool canMinimize = form.ControlBox && form.MinimizeBox;
			bool canMaximize = form.ControlBox && form.MaximizeBox && canResize;
			bool canClose = form.ControlBox;
			Size originalClientSize = form.ClientSize;
			Size originalMinimumSize = form.MinimumSize;
			Size originalMaximumSize = form.MaximumSize;
			Control[] existingControls = form.Controls.Cast<Control>().ToArray();

			form.SuspendLayout();
			try
			{
				form.Controls.Clear();
				form.FormBorderStyle = FormBorderStyle.None;
				form.ControlBox = false;

				Panel contentPanel = new()
				{
					Name = "synixWindowContent",
					BackColor = form.BackColor,
					Dock = DockStyle.Fill,
					Location = new Point(0, HeaderHeight),
					Margin = Padding.Empty,
					Padding = Padding.Empty,
					Size = originalClientSize,
					TabIndex = 1
				};
				contentPanel.Controls.AddRange(existingControls);

				Panel header = CreateHeader(
					form,
					originalClientSize.Width,
					canMinimize,
					canMaximize,
					canClose,
					out Label titleLabel);

				form.ClientSize = new Size(
					originalClientSize.Width,
					originalClientSize.Height + HeaderHeight);
				form.MinimumSize = AddHeaderHeight(originalMinimumSize);
				form.MaximumSize = AddHeaderHeight(originalMaximumSize);
				form.Controls.Add(contentPanel);
				form.Controls.Add(header);
				form.Controls.SetChildIndex(
					header,
					form.Controls.Count - 1);

				BorderlessResizeWindow? resizeWindow = canResize
					? new BorderlessResizeWindow(form)
					: null;
				Registrations.Add(form, new HeaderRegistration
				{
					Header = header,
					Title = titleLabel,
					ResizeWindow = resizeWindow
				});

				form.TextChanged += (_, _) => titleLabel.Text = form.Text;
			}
			finally
			{
				form.ResumeLayout(true);
			}
		}

		private static Panel CreateHeader(
			Form form,
			int width,
			bool canMinimize,
			bool canMaximize,
			bool canClose,
			out Label titleLabel)
		{
			Panel header = new()
			{
				Name = "synixWindowHeader",
				BackColor = SettingsPalette.TitleBar,
				Dock = DockStyle.Top,
				Location = Point.Empty,
				Margin = Padding.Empty,
				Size = new Size(width, HeaderHeight),
				TabIndex = 0
			};

			Panel divider = new()
			{
				Name = "synixWindowHeaderDivider",
				BackColor = SettingsPalette.Divider,
				Dock = DockStyle.Bottom,
				Height = 1,
				TabStop = false
			};
			header.Controls.Add(divider);

			PictureBox logo = new()
			{
				Name = "synixWindowLogo",
				AccessibleName = "Synix logo",
				BackColor = Color.Transparent,
				Image = global::Synix_Control_Panel.Properties.Resources.synix_logo,
				Location = new Point(17, 10),
				Size = new Size(38, 36),
				SizeMode = PictureBoxSizeMode.Zoom,
				TabStop = false
			};
			header.Controls.Add(logo);

			titleLabel = new Label
			{
				Name = "synixWindowTitle",
				AccessibleName = "Window title",
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				AutoEllipsis = true,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(64, 15),
				Size = new Size(Math.Max(80, width - 132), 28),
				Text = form.Text,
				TextAlign = ContentAlignment.MiddleLeft,
				UseMnemonic = false
			};
			header.Controls.Add(titleLabel);

			int buttonX = width - 50;
			if (canClose)
			{
				Button closeButton = CreateWindowButton(
					"synixWindowCloseButton",
					"✕",
					"Close",
					buttonX,
					isCloseButton: true);
				closeButton.Click += (_, _) => form.Close();
				header.Controls.Add(closeButton);
				buttonX -= 42;
			}

			if (canMaximize)
			{
				Button maximizeButton = CreateWindowButton(
					"synixWindowMaximizeButton",
					"□",
					"Maximize or restore",
					buttonX,
					isCloseButton: false);
				maximizeButton.Click += (_, _) => ToggleMaximize(form);
				header.Controls.Add(maximizeButton);
				buttonX -= 42;
			}

			if (canMinimize)
			{
				Button minimizeButton = CreateWindowButton(
					"synixWindowMinimizeButton",
					"—",
					"Minimize",
					buttonX,
					isCloseButton: false);
				minimizeButton.Click += (_, _) =>
					form.WindowState = FormWindowState.Minimized;
				header.Controls.Add(minimizeButton);
				buttonX -= 42;
			}

			titleLabel.Width = Math.Max(80, buttonX - titleLabel.Left + 34);
			MouseEventHandler dragHandler = (_, eventArgs) => BeginDrag(form, eventArgs);
			header.MouseDown += dragHandler;
			logo.MouseDown += dragHandler;
			titleLabel.MouseDown += dragHandler;
			if (canMaximize)
			{
				header.DoubleClick += (_, _) => ToggleMaximize(form);
				logo.DoubleClick += (_, _) => ToggleMaximize(form);
				titleLabel.DoubleClick += (_, _) => ToggleMaximize(form);
			}

			return header;
		}

		private static Button CreateWindowButton(
			string name,
			string text,
			string accessibleName,
			int left,
			bool isCloseButton)
		{
			Button button = new()
			{
				Name = name,
				AccessibleName = accessibleName,
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = SettingsPalette.TitleBar,
				Cursor = Cursors.Hand,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI Symbol", 11F, FontStyle.Regular),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(left, 8),
				Size = new Size(40, 40),
				TabStop = false,
				Text = text,
				UseVisualStyleBackColor = false
			};
			button.FlatAppearance.BorderSize = 0;
			button.FlatAppearance.MouseDownBackColor = isCloseButton
				? Color.FromArgb(176, 34, 46)
				: SettingsPalette.Selection;
			button.FlatAppearance.MouseOverBackColor = isCloseButton
				? Color.FromArgb(205, 49, 61)
				: SettingsPalette.CardHover;
			return button;
		}

		private static Size AddHeaderHeight(Size size)
		{
			if (size.IsEmpty || size.Height == 0)
				return size;
			return new Size(size.Width, checked(size.Height + HeaderHeight));
		}

		private static void ToggleMaximize(Form form)
		{
			form.WindowState = form.WindowState == FormWindowState.Maximized
				? FormWindowState.Normal
				: FormWindowState.Maximized;
		}

		private static void BeginDrag(Form form, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
				return;
			_ = ReleaseCapture();
			_ = SendMessage(
				form.Handle,
				WmNcLeftButtonDown,
				(IntPtr)HtCaption,
				IntPtr.Zero);
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			IntPtr wordParameter,
			IntPtr longParameter);
	}
}
