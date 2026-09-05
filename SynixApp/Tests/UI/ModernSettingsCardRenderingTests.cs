// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ModernSettingsCardRenderingTests
{
	[Theory]
	[InlineData(334, 112, 12, false)]
	[InlineData(335, 113, 12, false)]
	[InlineData(334, 112, 12, true)]
	[InlineData(418, 140, 15, false)]
	[InlineData(501, 168, 18, true)]
	[InlineData(668, 224, 24, false)]
	[InlineData(80, 48, 40, false)]
	public void RoundedBorder_DoesNotClipAntialiasedCorners(
		int width, int height, int radius, bool lightTheme)
	{
		RunOnStaThread(() =>
		{
			using Panel parent = new()
			{
				BackColor = lightTheme ? Color.FromArgb(230, 234, 240) : Color.FromArgb(8, 13, 24),
				Size = new Size(width, height)
			};
			using ModernSettingsCard card = new()
			{
				Size = parent.Size,
				CornerRadius = radius,
				BackColor = lightTheme ? Color.FromArgb(248, 250, 252) : Color.FromArgb(17, 27, 45),
				FillColor = lightTheme ? Color.FromArgb(248, 250, 252) : Color.FromArgb(17, 27, 45),
				BorderColor = lightTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(38, 52, 77)
			};
			parent.Controls.Add(card);
			using Bitmap unclipped = RenderWithoutWindowRegion(card);
			using Bitmap rendered = RenderCard(card, parent.BackColor);
			int cornerSize = Math.Min(radius + 2, Math.Min(width, height) / 2);
			for (int y = 0; y < cornerSize; y++)
			{
				for (int x = 0; x < cornerSize; x++)
				{
					foreach (Point corner in new[]
					{
						new Point(x, y), new Point(width - 1 - x, y),
						new Point(x, height - 1 - y), new Point(width - 1 - x, height - 1 - y)
					})
					{
						Assert.Equal(unclipped.GetPixel(corner.X, corner.Y).ToArgb(),
							rendered.GetPixel(corner.X, corner.Y).ToArgb());
					}
				}
			}

			Assert.Equal(parent.BackColor.ToArgb(), rendered.GetPixel(0, 0).ToArgb());
			Assert.Equal(card.FillColor.ToArgb(), rendered.GetPixel(width / 2, height / 2).ToArgb());
			Assert.Equal(card.BorderColor.ToArgb(), rendered.GetPixel(width / 2, 0).ToArgb());
			Assert.Equal(card.BorderColor.ToArgb(), rendered.GetPixel(width - 1, height / 2).ToArgb());
		});
	}

	[Fact]
	public void RoundedRegion_StillClipsDockedChildren_AfterResizeAndRadiusChanges()
	{
		RunOnStaThread(() =>
		{
			using Panel parent = new() { BackColor = Color.Black, Size = new Size(100, 80) };
			using ModernSettingsCard card = new() { Size = parent.Size, CornerRadius = 12 };
			using Panel child = new() { Dock = DockStyle.Fill, BackColor = Color.Red };
			parent.Controls.Add(card);
			card.Controls.Add(child);

			foreach (int radius in new[] { 12, 20, 0, 8 })
			{
				card.Size = new Size(card.Width + 1, card.Height + 1);
				card.CornerRadius = radius;
				Assert.NotNull(card.Region);
				Assert.True(card.Region.IsVisible(card.Width / 2, card.Height / 2));
				using Bitmap rendered = RenderCard(card, parent.BackColor);
				Assert.Equal(Color.Red.ToArgb(), rendered.GetPixel(card.Width / 2, card.Height / 2).ToArgb());
				if (radius > 0)
				{
					Assert.False(card.Region.IsVisible(0, 0));
					Assert.Equal(parent.BackColor.ToArgb(), rendered.GetPixel(0, 0).ToArgb());
				}
			}
		});
	}

	private static Bitmap RenderCard(ModernSettingsCard card, Color parentColor)
	{
		using Bitmap controlBitmap = new(card.Width, card.Height);
		card.DrawToBitmap(controlBitmap, card.ClientRectangle);
		Bitmap result = new(card.Width, card.Height);
		using Graphics graphics = Graphics.FromImage(result);
		graphics.Clear(parentColor);
		// Also apply the managed region when compositing the control onto its parent.
		if (card.Region is not null)
			graphics.SetClip(card.Region, CombineMode.Replace);
		graphics.DrawImageUnscaled(controlBitmap, Point.Empty);
		return result;
	}

	private static Bitmap RenderWithoutWindowRegion(ModernSettingsCard card)
	{
		Bitmap result = new(card.Width, card.Height);
		using Graphics graphics = Graphics.FromImage(result);
		using PaintEventArgs paint = new(graphics, card.ClientRectangle);
		// Invoke painting directly to obtain the full smoothed edge, before HWND clipping.
		foreach (string method in new[] { "OnPaintBackground", "OnPaint" })
			typeof(ModernSettingsCard).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(card, new object[] { paint });
		return result;
	}

	private static void RunOnStaThread(Action action)
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try { action(); }
			catch (Exception exception) { failure = exception; }
		}) { IsBackground = true };
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Card rendering did not finish.");
		if (failure is not null)
			ExceptionDispatchInfo.Capture(failure).Throw();
	}
}
