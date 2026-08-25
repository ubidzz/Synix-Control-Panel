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
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Synix_Control_Panel.SynixApp.Design
{
	internal static class WindowMemoryReclaimer
	{
		private sealed class WindowRegistration
		{
		}

		private static readonly ConditionalWeakTable<Form, WindowRegistration>
			Registrations = new();
		private static int _cleanupScheduled;

		public static void Track(Form form)
		{
			if (form is MainGUI || form.IsDisposed)
				return;

			Registrations.GetValue(form, RegisterWindow);
		}

		private static WindowRegistration RegisterWindow(Form form)
		{
			form.FormClosed += Form_FormClosed;
			return new WindowRegistration();
		}

		private static void Form_FormClosed(object? sender, FormClosedEventArgs eventArgs)
		{
			if (sender is Form form)
				form.FormClosed -= Form_FormClosed;

			ScheduleCleanup();
		}

		private static void ScheduleCleanup()
		{
			if (Interlocked.Exchange(ref _cleanupScheduled, 1) != 0)
				return;

			MainGUI? mainWindow = MainGUI.Instance;
			if (mainWindow != null &&
				!mainWindow.IsDisposed &&
				mainWindow.IsHandleCreated)
			{
				try
				{
					mainWindow.BeginInvoke((MethodInvoker)ReleaseClosedWindowMemory);
					return;
				}
				catch (InvalidOperationException)
				{
				}
			}

			ThreadPool.QueueUserWorkItem(_ => ReleaseClosedWindowMemory());
		}

		private static void ReleaseClosedWindowMemory()
		{
			try
			{
				GCSettings.LargeObjectHeapCompactionMode =
					GCLargeObjectHeapCompactionMode.CompactOnce;
				GC.Collect(
					GC.MaxGeneration,
					GCCollectionMode.Aggressive,
					blocking: true,
					compacting: true);
				GC.WaitForPendingFinalizers();
				GC.Collect(
					GC.MaxGeneration,
					GCCollectionMode.Aggressive,
					blocking: true,
					compacting: true);
			}
			finally
			{
				Interlocked.Exchange(ref _cleanupScheduled, 0);
			}
		}
	}
}
