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
using Synix_Control_Panel.SynixApp.Database;

namespace Synix_Control_Panel.SynixEngine.ModManagement;

// Describes the selected import route, not a compatibility certification. Creating
// a review must not inspect secrets, create destinations or change server state.
internal sealed record ModInstallGuidance(string Compatibility, string Destination, string Activation, string Recovery)
{
	internal static ModInstallGuidance Pending => new(Text("PendingCompatibility"), Text("PendingDestination"),
		Text("PendingActivation"), Text("PendingRecovery"));

	internal static ModInstallGuidance Create(GameServer server, ModImportChoice choice, ModPackagePreview? preview = null)
	{
		ModSystemProfile profile = choice.Profile;
		ModInstallTarget target = choice.Target;
		string loader = GameDatabase.IsMinecraft(server.Game) ? server.MinecraftLoader :
			!string.IsNullOrWhiteSpace(profile.FrameworkName) ? profile.FrameworkName : server.ServerFramework;
		string context = Text("Context", server.Game, Known(server.GameVersion), Known(loader));
		string compatibility = Text(target.CanManageIds ? "ProviderCompatibility" : profile.UserConfigured ? "ManualCompatibility" :
			!choice.Ready ? "LoaderMissing" : "LayoutRecognized");
		string destination;
		if (target.CanManageIds)
		{
			destination = Text("ProviderDestination", Known(target.ProviderName));
			if (target.Mode == ModTargetMode.ArgumentIds)
				destination += Environment.NewLine + Text("LaunchArgument", target.ArgumentName);
			else
			{
				foreach (ModIdStore store in target.IdStores)
					destination += Environment.NewLine + Path.Combine(server.InstallPath, store.RelativePath.Replace('/', Path.DirectorySeparatorChar)) +
						Environment.NewLine + $"[{store.Section}] {store.Key}";
				if (target.RequiredArguments.Count > 0)
					destination += Environment.NewLine + Text("RequiredArguments", string.Join(" ", target.RequiredArguments));
			}
		}
		else
		{
			destination = Path.Combine(server.InstallPath, target.RelativePath.Replace('/', Path.DirectorySeparatorChar));
			if (preview != null)
				destination += Environment.NewLine + Text("FileChanges", preview.Files.Count,
					preview.Files.Count(file => file.ReplacesFile));
			destination += Environment.NewLine + Text("FilePreview");
		}
		string activation = Text(!choice.Ready ? "LoaderMissing" : target.CanManageIds ? "ProviderActivation" : profile.UserConfigured ? "ManualActivation" :
			target.PackageLayout == ModPackageLayout.EmpyrionScenario ? "ScenarioActivation" : "FileActivation");
		string recovery = Text(target.CanManageIds ? "ProviderRecovery" : "FileRecovery");
		if (target.PackageLayout == ModPackageLayout.EmpyrionScenario)
			recovery += Environment.NewLine + Text("ScenarioRecovery");
		return new(compatibility + Environment.NewLine + context, destination, activation, recovery);
	}

	internal string NextSteps() => Text("Activation") + Environment.NewLine + Activation + Environment.NewLine + Environment.NewLine +
		Text("Recovery") + Environment.NewLine + Recovery;

	internal static string Text(string key, params object[] arguments) => LocalizationManager.Get("ModInstallGuide." + key, arguments);
	private static string Known(string? value) => string.IsNullOrWhiteSpace(value) ? Text("Unknown") : LocalizationManager.TranslateKnownText(value);
}
