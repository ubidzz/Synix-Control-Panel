// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SuppressedExceptionLoggingTests
{
	[Fact]
	public void SuppressedExceptionsDoNotPublishToTheDashboard()
	{
		int dashboardMessageCount = 0;
		EventHandler<ApplicationLogEventArgs> handler = (_, _) =>
			dashboardMessageCount++;
		ApplicationUiService.LogRequested += handler;

		try
		{
			ApplicationLogService.WriteSuppressedException(
				new InvalidOperationException("Suppressed logging regression check."));

			Assert.Equal(0, dashboardMessageCount);
		}
		finally
		{
			ApplicationUiService.LogRequested -= handler;
		}
	}
}
