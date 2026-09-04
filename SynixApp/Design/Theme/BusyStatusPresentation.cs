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
using System.Drawing.Drawing2D;
using Synix_Control_Panel.SynixApp.Localization;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.Design
{
	internal static class BusyStatusPresentation
	{
		private const int DotCount = 8;
		private static readonly string[] BusyStates =
		{
			StatusManager.GetStatus(ServerState.Starting),
			StatusManager.GetStatus(ServerState.Stopping),
			StatusManager.GetStatus(ServerState.Installing),
			StatusManager.GetStatus(ServerState.Updating),
			StatusManager.GetStatus(ServerState.BackingUp),
			StatusManager.GetStatus(ServerState.Restoring),
			StatusManager.GetStatus(ServerState.Validating),
			StatusManager.GetStatus(ServerState.Export),
			StatusManager.GetStatus(ServerState.Deleting)
		};

		public const int FrameCount = DotCount;

		public static bool TryGetBusyState(string? status, out string busyState)
		{
			string currentStatus = status?.Trim() ?? string.Empty;
			foreach (string candidate in BusyStates)
			{
				if (currentStatus.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
				{
					busyState = candidate;
					return true;
				}
			}

			busyState = string.Empty;
			return false;
		}

		public static string GetDisplayStatus(string? status)
		{
			string displayStatus = TryGetBusyState(status, out string busyState)
				? busyState
				: status?.Trim() ?? string.Empty;
			return GetLocalizedStatus(displayStatus);
		}

		private static string GetLocalizedStatus(string status)
		{
			return status switch
			{
				"Stopped" => LocalizationManager.Get("Status.Stopped"),
				"Running" => LocalizationManager.Get("Status.Running"),
				"Starting" => LocalizationManager.Get("Status.Starting"),
				"Crashed" => LocalizationManager.Get("Status.Crashed"),
				"Stopping" => LocalizationManager.Get("Status.Stopping"),
				"Installing" => LocalizationManager.Get("Status.Installing"),
				"Updating" => LocalizationManager.Get("Status.Updating"),
				"Backing Up" => LocalizationManager.Get("Status.BackingUp"),
				"Validating" => LocalizationManager.Get("Status.Validating"),
				"Exporting" => LocalizationManager.Get("Status.Exporting"),
				"Restoring" => LocalizationManager.Get("Status.Restoring"),
				"Deleting" => LocalizationManager.Get("Status.Deleting"),
				"Unknown" => LocalizationManager.Get("Status.Unknown"),
				_ => status
			};
		}

		public static void DrawIndicator(
			Graphics graphics,
			Rectangle bounds,
			Color color,
			bool isBusy,
			int frame)
		{
			SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if (isBusy)
			{
				DrawBusyIndicator(graphics, bounds, color, frame);
			}
			else
			{
				int diameter = Math.Max(4, Math.Min(bounds.Width, bounds.Height) / 2);
				Rectangle dotBounds = new(
					bounds.Left + (bounds.Width - diameter) / 2,
					bounds.Top + (bounds.Height - diameter) / 2,
					diameter,
					diameter);
				using SolidBrush brush = new(color);
				graphics.FillEllipse(brush, dotBounds);
			}

			graphics.SmoothingMode = previousSmoothingMode;
		}

		private static void DrawBusyIndicator(
			Graphics graphics,
			Rectangle bounds,
			Color color,
			int frame)
		{
			float centerX = bounds.Left + bounds.Width / 2F;
			float centerY = bounds.Top + bounds.Height / 2F;
			float orbitRadius = Math.Max(3F, Math.Min(bounds.Width, bounds.Height) * 0.34F);
			float dotDiameter = Math.Max(2.2F, Math.Min(bounds.Width, bounds.Height) * 0.18F);
			int activeFrame = ((frame % DotCount) + DotCount) % DotCount;

			for (int index = 0; index < DotCount; index++)
			{
				int trailPosition = (index - activeFrame + DotCount) % DotCount;
				int alpha = Math.Clamp(255 - trailPosition * 27, 58, 255);
				double angle = index * Math.PI * 2.0 / DotCount - Math.PI / 2.0;
				float x = centerX + (float)Math.Cos(angle) * orbitRadius - dotDiameter / 2F;
				float y = centerY + (float)Math.Sin(angle) * orbitRadius - dotDiameter / 2F;
				using SolidBrush brush = new(Color.FromArgb(alpha, color));
				graphics.FillEllipse(brush, x, y, dotDiameter, dotDiameter);
			}
		}
	}
}
