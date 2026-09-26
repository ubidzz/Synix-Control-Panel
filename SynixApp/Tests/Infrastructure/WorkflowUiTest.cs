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
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

internal static class WorkflowUiTest
{
	internal static void Run(Action action)
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			// DoEvents restores the previous context. Keep a UI context installed for
			// the whole fixture so awaited work never resumes on a pool thread.
			using WindowsFormsSynchronizationContext context = new();
			SynchronizationContext.SetSynchronizationContext(context);
			try { action(); }
			catch (Exception exception) { failure = exception; }
			finally { SynchronizationContext.SetSynchronizationContext(null); }
		}) { IsBackground = true };
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "The workflow UI test did not finish.");
		Assert.Null(failure);
	}

	internal static object? Invoke(object target, string method, params object[] arguments) =>
		target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(target, arguments);

	internal static void Pump(Task task)
	{
		WaitUntil(() => task.IsCompleted);
		task.GetAwaiter().GetResult();
	}

	internal static void WaitUntil(Func<bool> completed)
	{
		Stopwatch timeout = Stopwatch.StartNew();
		while (!completed() && timeout.Elapsed < TimeSpan.FromSeconds(15))
		{
			Application.DoEvents();
			Thread.Sleep(1);
		}
		Assert.True(completed(), "The asynchronous UI action did not finish.");
	}
}
