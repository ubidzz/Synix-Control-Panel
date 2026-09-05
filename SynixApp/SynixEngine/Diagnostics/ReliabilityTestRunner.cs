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
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record ReliabilitySample(
		DateTimeOffset CapturedAtUtc,
		long PrivateMemoryBytes,
		long WorkingSetBytes,
		int HandleCount,
		int ThreadCount,
		int HealthFailures,
		int HealthWarnings);

	internal sealed class ReliabilityTestReport
	{
		internal ReliabilityTestReport(
			DateTimeOffset startedAtUtc,
			DateTimeOffset completedAtUtc,
			IReadOnlyList<ReliabilitySample> samples)
		{
			StartedAtUtc = startedAtUtc;
			CompletedAtUtc = completedAtUtc;
			Samples = samples;
		}

		internal DateTimeOffset StartedAtUtc { get; }
		internal DateTimeOffset CompletedAtUtc { get; }
		internal IReadOnlyList<ReliabilitySample> Samples { get; }
		internal long PrivateMemoryGrowth => Samples.Count < 2 ? 0 : Samples[^1].PrivateMemoryBytes - Samples[0].PrivateMemoryBytes;
		internal int HandleGrowth => Samples.Count < 2 ? 0 : Samples[^1].HandleCount - Samples[0].HandleCount;
		internal int ThreadGrowth => Samples.Count < 2 ? 0 : Samples[^1].ThreadCount - Samples[0].ThreadCount;
		internal long PeakPrivateMemory => Samples.Count == 0 ? 0 : Samples.Max(sample => sample.PrivateMemoryBytes);

		internal string ToPlainText()
		{
			StringBuilder text = new();
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.Title"));
			text.AppendLine();
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.Started",
				StartedAtUtc.ToLocalTime()));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.Completed",
				CompletedAtUtc.ToLocalTime()));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.Duration",
				CompletedAtUtc - StartedAtUtc));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.Samples",
				Samples.Count));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.PeakMemory",
				FormatBytes(PeakPrivateMemory)));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.MemoryGrowth",
				FormatSignedBytes(PrivateMemoryGrowth)));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.HandleGrowth",
				HandleGrowth));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.ThreadGrowth",
				ThreadGrowth));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.HighestFailures",
				Samples.Count == 0
					? 0
					: Samples.Max(sample => sample.HealthFailures)));
			text.AppendLine(LocalizationManager.Get(
				"Diagnostics.Reliability.Report.HighestWarnings",
				Samples.Count == 0
					? 0
					: Samples.Max(sample => sample.HealthWarnings)));
			text.AppendLine();
			foreach (ReliabilitySample sample in Samples)
			{
				text.AppendLine(LocalizationManager.Get(
					"Diagnostics.Reliability.Report.Sample",
					sample.CapturedAtUtc.ToLocalTime(),
					FormatBytes(sample.PrivateMemoryBytes),
					FormatBytes(sample.WorkingSetBytes),
					sample.HandleCount,
					sample.ThreadCount,
					sample.HealthFailures,
					sample.HealthWarnings));
			}
			return text.ToString().TrimEnd();
		}

		private static string FormatSignedBytes(long bytes) =>
			(bytes >= 0 ? "+" : "-") + FormatBytes(Math.Abs(bytes));

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB"];
			double value = bytes;
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}
			return $"{value:0.##} {units[unit]}";
		}
	}

	internal static class ReliabilityTestRunner
	{
		internal static async Task<ReliabilityTestReport> RunAsync(
			IReadOnlyList<GameServer> servers,
			TimeSpan duration,
			TimeSpan interval,
			IProgress<string>? progress = null,
			CancellationToken cancellationToken = default)
		{
			if (duration <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(duration));
			if (interval <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(interval));

			DateTimeOffset started = DateTimeOffset.UtcNow;
			Stopwatch elapsed = Stopwatch.StartNew();
			List<ReliabilitySample> samples = [];
			int cycle = 0;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();
				cycle++;
				progress?.Report(LocalizationManager.Get(
					"Diagnostics.Reliability.Progress.Sample",
					cycle,
					elapsed.Elapsed));
				SynixHealthReport health = await SynixTroubleshooter.RunAsync(
					servers,
					checkForUpdates: false,
					cancellationToken: cancellationToken);
				using Process current = Process.GetCurrentProcess();
				current.Refresh();
				samples.Add(new ReliabilitySample(
					DateTimeOffset.UtcNow,
					current.PrivateMemorySize64,
					current.WorkingSet64,
					current.HandleCount,
					current.Threads.Count,
					health.FailedCount,
					health.WarningCount));

				TimeSpan remaining = duration - elapsed.Elapsed;
				if (remaining <= TimeSpan.Zero)
					break;
				await Task.Delay(remaining < interval ? remaining : interval, cancellationToken);
			}
			while (elapsed.Elapsed < duration);

			return new ReliabilityTestReport(started, DateTimeOffset.UtcNow, samples);
		}
	}
}
