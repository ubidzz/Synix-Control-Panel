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
namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class Arma3Configuration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("server.cfg",
				"""
				hostname = "{ServerName}";
				password = "{Password}";
				passwordAdmin = "{AdminPassword}";
				serverCommandPassword = "";
				maxPlayers = {MaxPlayers};
				motd[] = { "Welcome to {ServerName}" };
				motdInterval = 5;
				admins[] = {};
				headlessClients[] = {};
				localClient[] = { "127.0.0.1" };
				filePatchingExceptions[] = {};
				voteMissionPlayers = 1;
				voteThreshold = 0.33;
				votingTimeOut = 60;
				roleTimeOut = 90;
				briefingTimeOut = 60;
				debriefingTimeOut = 45;
				lobbyIdleTimeout = 300;
				disableVoN = 0;
				vonCodec = 1;
				vonCodecQuality = 20;
				persistent = 1;
				timeStampFormat = "short";
				timeStampFormatConsole = "short";
				BattlEye = 1;
				verifySignatures = 2;
				equalModRequired = 0;
				drawingInMap = true;
				kickDuplicate = 1;
				allowedFilePatching = 0;
				requiredSecureId = 2;
				steamProtocolMaxDataSize = 1024;
				forcedDifficulty = "Regular";
				autoSelectMission = false;
				randomMissionOrder = false;
				missionsToServerRestart = 0;
				missionsToShutdown = 0;
				idleTimeout = 0;
				idleFPSLimit = 30;
				logFile = "server_console.log";
				statisticsEnabled = 0;
				""")
		];

		public override string GameName => "Arma 3";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
