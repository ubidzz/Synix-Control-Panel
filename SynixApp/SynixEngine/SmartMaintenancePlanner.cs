// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================

namespace Synix_Control_Panel.SynixEngine
{
	internal enum SmartMaintenanceDecision
	{
		NotDue,
		DeferForPlayers,
		RunNow
	}

	internal sealed record SmartMaintenancePlan(
		SmartMaintenanceDecision Decision,
		DateTime ScheduledFor,
		TimeSpan Delay,
		string Reason);

	internal static class SmartMaintenancePlanner
	{
		internal static SmartMaintenancePlan Evaluate(GameServer server, DateTime now)
		{
			ArgumentNullException.ThrowIfNull(server);
			DateTime fallback = now.Date;
			if (!server.IsScheduledRestartEnabled)
				return NotDue(fallback, "Scheduled maintenance is disabled.");

			int dayIndex = (int)now.DayOfWeek;
			if (server.RestartDays == null ||
				server.RestartDays.Length <= dayIndex ||
				!server.RestartDays[dayIndex])
			{
				return NotDue(fallback, "Today is not a selected maintenance day.");
			}

			if (server.LastMaintenanceDate == now.ToString("yyyy-MM-dd"))
				return NotDue(fallback, "Today's maintenance is already complete.");

			if (!TimeSpan.TryParseExact(
				server.RestartTime,
				@"hh\:mm",
				System.Globalization.CultureInfo.InvariantCulture,
				out TimeSpan scheduledTime))
			{
				return NotDue(fallback, "The maintenance time is invalid.");
			}

			DateTime scheduledFor = now.Date.Add(scheduledTime);
			if (now < scheduledFor)
				return NotDue(scheduledFor, "The scheduled time has not arrived.");

			TimeSpan delay = now - scheduledFor;
			if (!server.SmartMaintenanceEnabled && delay >= TimeSpan.FromMinutes(1))
				return NotDue(scheduledFor, "The standard maintenance minute has passed.");

			int maximumDelay = Math.Clamp(server.MaintenanceMaximumDelayMinutes, 0, 720);
			if (server.SmartMaintenanceEnabled &&
				server.MaintenanceWaitForPlayers &&
				server.CurrentPlayers > 0 &&
				delay < TimeSpan.FromMinutes(maximumDelay))
			{
				return new(
					SmartMaintenanceDecision.DeferForPlayers,
					scheduledFor,
					delay,
					$"Waiting for {server.CurrentPlayers} connected player(s) to leave.");
			}

			return new(
				SmartMaintenanceDecision.RunNow,
				scheduledFor,
				delay,
				server.CurrentPlayers > 0 && maximumDelay > 0
					? "The maximum player-aware delay has been reached."
					: "The server is ready for scheduled maintenance.");
		}

		private static SmartMaintenancePlan NotDue(DateTime scheduledFor, string reason) =>
			new(SmartMaintenanceDecision.NotDue, scheduledFor, TimeSpan.Zero, reason);
	}
}
