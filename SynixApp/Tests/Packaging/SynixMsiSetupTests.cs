using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixMsiSetupTests
{
	private static readonly XNamespace Wix = "http://wixtoolset.org/schemas/v4/wxs";
	private static readonly XNamespace Ui = "http://wixtoolset.org/schemas/v4/wxs/ui";
	private static string ProjectDirectory => Core.FindProjectDirectory(AppContext.BaseDirectory)!;
	private static string PackagingDirectory => Path.Combine(ProjectDirectory, "Packaging", "MSI");
	private static XDocument Package => XDocument.Load(Path.Combine(PackagingDirectory, "Package.wxs"));

	[Fact]
	public void FolderPickerChangesOnlyThePerUserApplicationDirectory()
	{
		XDocument source = Package;
		XElement package = Assert.Single(source.Descendants(Wix + "Package"));
		Assert.Equal("perUser", (string?)package.Attribute("Scope"));
		Assert.Equal("{e369556b-db95-4d9b-8e86-2b7d50dcd328}", (string?)package.Attribute("UpgradeCode"));
		XElement wizard = Assert.Single(source.Descendants(Ui + "WixUI"));
		Assert.Equal("WixUI_InstallDir", (string?)wizard.Attribute("Id"));
		Assert.Equal("INSTALLFOLDER", (string?)wizard.Attribute("InstallDirectory"));

		XElement appFolder = Assert.Single(source.Descendants(Wix + "Directory"),
			element => (string?)element.Attribute("Id") == "INSTALLFOLDER");
		Assert.Equal("Synix", (string?)appFolder.Attribute("Name"));
		Assert.Contains(appFolder.Ancestors(Wix + "StandardDirectory"),
			element => (string?)element.Attribute("Id") == "LocalAppDataFolder");
		Assert.Equal(@"C:\Synix", Core.RootPath);
		Assert.Equal(Path.Combine(Core.RootPath, "SynixData"), Core.DataPath);
	}

	[Fact]
	public void InteractiveAndSilentUpgradesReadTheRegisteredFolderBeforeGuardingOverrides()
	{
		XDocument source = Package;
		XElement previous = Assert.Single(source.Descendants(Wix + "Property"),
			element => (string?)element.Attribute("Id") == "SYNIX_PREVIOUS_INSTALLFOLDER");
		Assert.Equal("yes", (string?)previous.Attribute("Secure"));
		XElement search = Assert.Single(previous.Elements(Wix + "RegistrySearch"));
		Assert.Equal("HKCU", (string?)search.Attribute("Root"));
		Assert.Equal(@"Software\ubidzz\Synix Control Panel", (string?)search.Attribute("Key"));
		Assert.Equal("InstallLocation", (string?)search.Attribute("Name"));
		Assert.Equal("raw", (string?)search.Attribute("Type"));
		Assert.Equal("always64", (string?)search.Attribute("Bitness"));

		XElement restore = Assert.Single(source.Descendants(Wix + "SetProperty"),
			element => (string?)element.Attribute("Id") == "INSTALLFOLDER");
		Assert.Equal("[SYNIX_PREVIOUS_INSTALLFOLDER]", (string?)restore.Attribute("Value"));
		Assert.Equal("AppSearch", (string?)restore.Attribute("After"));
		Assert.Equal("both", (string?)restore.Attribute("Sequence"));
		Assert.Equal("NOT INSTALLFOLDER AND SYNIX_PREVIOUS_INSTALLFOLDER", (string?)restore.Attribute("Condition"));

		XElement chosen = Assert.Single(source.Descendants(Wix + "Property"),
			element => (string?)element.Attribute("Id") == "INSTALLFOLDER");
		Assert.Equal("yes", (string?)chosen.Attribute("Secure"));
		Assert.Null(chosen.Attribute("Value"));
	}

	[Fact]
	public void UpdaterRegistrationAndShortcutsFollowTheSelectedAppFolder()
	{
		XDocument source = Package;
		XElement registration = Assert.Single(source.Descendants(Wix + "RegistryValue"),
			element => (string?)element.Attribute("Name") == "InstallLocation");
		Assert.Equal("[INSTALLFOLDER]", (string?)registration.Attribute("Value"));
		Assert.Equal("HKCU", (string?)registration.Attribute("Root"));
		Assert.Equal(@"Software\ubidzz\Synix Control Panel", (string?)registration.Attribute("Key"));
		Assert.Equal("string", (string?)registration.Attribute("Type"));
		XElement arp = Assert.Single(source.Descendants(Wix + "SetProperty"),
			element => (string?)element.Attribute("Id") == "ARPINSTALLLOCATION");
		Assert.Equal("[INSTALLFOLDER]", (string?)arp.Attribute("Value"));
		Assert.Equal("CostFinalize", (string?)arp.Attribute("After"));
		Assert.All(source.Descendants(Wix + "Shortcut"), shortcut =>
			Assert.Equal("INSTALLFOLDER", (string?)shortcut.Attribute("WorkingDirectory")));
		Assert.Contains(source.Descendants(Wix + "Property"), element =>
			(string?)element.Attribute("Id") == "SYNIXINSTALLSOURCE" &&
			(string?)element.Attribute("Secure") == "yes");
		XElement cleanup = Assert.Single(source.Descendants(Wix + "RemoveRegistryKey"));
		Assert.Equal("HKCU", (string?)cleanup.Attribute("Root"));
		Assert.Equal(@"Software\ubidzz\Synix Control Panel", (string?)cleanup.Attribute("Key"));
		Assert.Equal("removeOnUninstall", (string?)cleanup.Attribute("Action"));
	}

	[Fact]
	public void InstallerOwnsOnlyItsExecutableAndHasNoUnconditionalServerDeletion()
	{
		XDocument source = Package;
		XElement executable = Assert.Single(source.Descendants(Wix + "File"));
		Assert.Equal("SynixExecutable", (string?)executable.Attribute("Id"));
		Assert.Equal(@"$(var.SynixPublishDir)\$(var.SynixAssemblyName).exe", (string?)executable.Attribute("Source"));
		XElement component = Assert.Single(source.Descendants(Wix + "Component"));
		Assert.Equal("{015b398b-e6e1-48e6-b03a-f6ddb52f0998}", (string?)component.Attribute("Guid"));
		Assert.Equal("INSTALLFOLDER", (string?)component.Parent?.Attribute("Id"));
		Assert.DoesNotContain(source.Descendants(), element =>
			element.Name.LocalName is "CopyFile" or "MoveFile" or "RemoveFile" or
				"RemoveFolderEx" or "Files" or "Execute" or "ExePackage");
		Assert.All(source.Descendants(Wix + "RemoveFolder"), folder =>
		{
			Assert.Contains((string?)folder.Attribute("Directory"), new[] { "INSTALLFOLDER", "LocalProgramsFolder" });
			Assert.Equal("uninstall", (string?)folder.Attribute("On"));
		});
	}

	[Fact]
	public void UninstallHelperIsNeverRunByInstallationRepairOrMajorUpgrade()
	{
		XDocument source = Package;
		XElement action = Assert.Single(source.Descendants(Wix + "CustomAction"),
			element => (string?)element.Attribute("Id") == "SynixUninstallOptions");
		Assert.Equal("SynixUninstallOptions", (string?)action.Attribute("Id"));
		Assert.Equal("SynixExecutable", (string?)action.Attribute("FileRef"));
		Assert.Equal("--synix-uninstall [UILevel]", (string?)action.Attribute("ExeCommand"));
		Assert.Equal("immediate", (string?)action.Attribute("Execute"));
		Assert.Equal("check", (string?)action.Attribute("Return"));
		XElement schedule = Assert.Single(source.Descendants(Wix + "InstallExecuteSequence").Elements(Wix + "Custom"),
			element => (string?)element.Attribute("Action") == "SynixUninstallOptions");
		Assert.Equal("SynixUninstallOptions", (string?)schedule.Attribute("Action"));
		Assert.Equal("InstallInitialize", (string?)schedule.Attribute("Before"));
		Assert.Equal("Installed AND REMOVE = \"ALL\" AND NOT UPGRADINGPRODUCTCODE AND NOT REINSTALL", (string?)schedule.Attribute("Condition"));
		Assert.Empty(source.Descendants(Wix + "InstallUISequence").Elements(Wix + "Custom"));
	}

	[Fact]
	public void PublishingFindsTheLocalizedInstallerAndKeepsTheReleaseFileName()
	{
		XDocument project = XDocument.Load(Path.Combine(PackagingDirectory, "SynixInstaller.wixproj"));
		Assert.Equal("en-US", Assert.Single(project.Descendants("Cultures")).Value);
		XDocument app = XDocument.Load(Path.Combine(ProjectDirectory, "Synix Control Panel.csproj"));
		Assert.Contains("'Release', 'en-US', 'SynixSetup.msi'", Assert.Single(app.Descendants("_SynixMsiBuildOutput")).Value);
		Assert.All(app.Descendants("_SynixMsiOutput"), value => Assert.Contains("'$(PublishDir)', 'SynixSetup.msi'", value.Value));
	}

	[Fact]
	public void OnlyStrictlyOlderRelatedProductsChangeWelcomeReviewAndButtonToUpdate()
	{
		XDocument source = Package;
		XElement upgrade = Assert.Single(source.Descendants(Wix + "MajorUpgrade"));
		Assert.Equal("yes", (string?)upgrade.Attribute("AllowSameVersionUpgrades"));
		Assert.Equal("afterInstallExecute", (string?)upgrade.Attribute("Schedule"));
		Assert.NotNull(upgrade.Attribute("DowngradeErrorMessage"));
		Assert.Null(upgrade.Attribute("AllowDowngrades"));
		foreach (string id in new[] { "SYNIX_SETUP_HEADING", "SYNIX_SETUP_DESCRIPTION", "SYNIX_SETUP_REVIEW", "SYNIX_SETUP_ACTION" })
		{
			XElement action = Assert.Single(source.Descendants(Wix + "SetProperty"), element => (string?)element.Attribute("Id") == id);
			Assert.Equal("FindRelatedProducts", (string?)action.Attribute("After"));
			Assert.Equal("ui", (string?)action.Attribute("Sequence"));
			Assert.Equal("WIX_UPGRADE_DETECTED AND NOT Installed AND NOT SYNIX_SAME_VERSION_FOUND AND NOT WIX_DOWNGRADE_DETECTED", (string?)action.Attribute("Condition"));
		}
		Assert.Equal("Update Synix", (string?)source.Descendants(Wix + "SetProperty")
			.Single(element => (string?)element.Attribute("Id") == "SYNIX_SETUP_HEADING").Attribute("Value"));
		string localization = File.ReadAllText(Path.Combine(PackagingDirectory, "Package.en-us.wxl"));
		Assert.Contains("[SYNIX_SETUP_HEADING]", localization);
		Assert.Contains("[SYNIX_SETUP_DESCRIPTION]", localization);
		Assert.Contains("[SYNIX_SETUP_REVIEW]", localization);
		Assert.Contains("[SYNIX_SETUP_ACTION]", localization);
	}

	[Fact]
	public void RebuiltSameVersionIsDetectedSeparatelyForExplicitRepair()
	{
		XDocument source = Package;
		XElement package = Assert.Single(source.Descendants(Wix + "Package"));
		XElement upgrade = Assert.Single(source.Descendants(Wix + "Upgrade"));
		Assert.Equal((string?)package.Attribute("UpgradeCode"), (string?)upgrade.Attribute("Id"));
		XElement version = Assert.Single(upgrade.Elements(Wix + "UpgradeVersion"));
		Assert.Equal("$(var.SynixVersion)", (string?)version.Attribute("Minimum"));
		Assert.Equal("$(var.SynixVersion)", (string?)version.Attribute("Maximum"));
		Assert.Equal("yes", (string?)version.Attribute("IncludeMinimum"));
		Assert.Equal("yes", (string?)version.Attribute("IncludeMaximum"));
		Assert.Equal("yes", (string?)version.Attribute("OnlyDetect"));
		Assert.Equal("SYNIX_SAME_VERSION_FOUND", (string?)version.Attribute("Property"));
		Assert.Null(version.Attribute("RemoveFeatures"));
		Assert.DoesNotContain(source.Descendants(Wix + "Property"), element =>
			(string?)element.Attribute("Id") is "Installed" or "SYNIX_SAME_VERSION_FOUND");
		Assert.DoesNotContain(source.Descendants(Wix + "SetProperty"), element =>
			(string?)element.Attribute("Id") is "Installed" or "SYNIX_SAME_VERSION_FOUND");
	}

	[Fact]
	public void RebuiltSameVersionOffersExplicitRepairOrUninstallBeforeTheInstallWizard()
	{
		XDocument source = XDocument.Load(Path.Combine(PackagingDirectory, "SameVersionMaintenance.wxs"));
		Assert.Contains(Package.Descendants(Wix + "UIRef"), element =>
			(string?)element.Attribute("Id") == "SynixSameVersionMaintenance");
		XElement show = Assert.Single(source.Descendants(Wix + "Show"), element => (string?)element.Attribute("Dialog") == "SynixSameVersionDlg");
		Assert.Equal("SynixSameVersionDlg", (string?)show.Attribute("Dialog"));
		Assert.Equal("CostFinalize", (string?)show.Attribute("After"));
		Assert.Equal("NOT Installed AND SYNIX_SAME_VERSION_FOUND AND NOT WIX_DOWNGRADE_DETECTED", (string?)show.Attribute("Condition"));
		foreach ((string button, string actionId, string mode) in new[] { ("Uninstall", "SynixUninstallInstalledVersion", "/x") })
		{
			XElement action = Assert.Single(source.Descendants(Wix + "CustomAction"),
				element => (string?)element.Attribute("Id") == actionId);
			Assert.Equal("System64Folder", (string?)action.Attribute("Directory"));
			Assert.Equal($"\"[System64Folder]msiexec.exe\" {mode} \"[SYNIX_SAME_VERSION_FOUND]\" /qb /norestart",
				(string?)action.Attribute("ExeCommand"));
			Assert.Equal("immediate", (string?)action.Attribute("Execute"));
			Assert.Equal("asyncNoWait", (string?)action.Attribute("Return"));
			XElement control = Assert.Single(source.Descendants(Wix + "Control"),
				element => (string?)element.Attribute("Id") == button);
			Assert.Null(control.Attribute("Default"));
			XElement invoke = Assert.Single(control.Elements(Wix + "Publish"),
				element => (string?)element.Attribute("Event") == "DoAction");
			Assert.Equal(actionId, (string?)invoke.Attribute("Value"));
			Assert.Equal("1", (string?)invoke.Attribute("Order"));
			XElement finish = Assert.Single(control.Elements(Wix + "Publish"),
				element => (string?)element.Attribute("Event") == "EndDialog");
			// Ignore means Finished, not Return/Success: it ends this package's
			// sequence instead of continuing into a new installation transaction.
			Assert.Equal("Ignore", (string?)finish.Attribute("Value"));
			Assert.Equal("2", (string?)finish.Attribute("Order"));
		}
		Assert.Empty(source.Descendants(Wix + "InstallExecuteSequence"));
		Assert.Empty(source.Descendants(Wix + "InstallUISequence").Elements(Wix + "Custom"));
		Assert.DoesNotContain(source.Descendants(Wix + "CustomAction"), element => (string?)element.Attribute("Id") == "SynixRepairInstalledVersion");
		XElement repair = Assert.Single(source.Descendants(Wix + "Control"), element => (string?)element.Attribute("Id") == "Repair");
		Assert.Null(repair.Attribute("Default"));
		Assert.Contains(repair.Elements(Wix + "Publish"), element =>
			(string?)element.Attribute("Property") == "SYNIX_REPAIR_CONFIRMED" && (string?)element.Attribute("Value") == "1");
		Assert.Contains(repair.Elements(Wix + "Publish"), element =>
			(string?)element.Attribute("Event") == "NewDialog" && (string?)element.Attribute("Value") == "LicenseAgreementDlg");
		Assert.DoesNotContain(repair.Elements(Wix + "Publish"), element => (string?)element.Attribute("Event") is "DoAction" or "Reinstall" or "ReinstallMode" or "EndDialog");
		Assert.Contains(source.Descendants(Wix + "Show"), element => (string?)element.Attribute("Dialog") == "override WelcomeDlg" &&
			(string?)element.Attribute("Condition") == "(NOT Installed OR PATCH) AND NOT SYNIX_SAME_VERSION_FOUND");
		Assert.Contains(source.Descendants(Wix + "Publish"), element => (string?)element.Attribute("Dialog") == "LicenseAgreementDlg" &&
			(string?)element.Attribute("Control") == "Next" && (string?)element.Attribute("Value") == "VerifyReadyDlg" &&
			(string?)element.Attribute("Condition") == "NOT Installed AND LicenseAccepted = \"1\" AND (SYNIX_REPAIR_CONFIRMED = \"1\" OR (WIX_UPGRADE_DETECTED AND NOT SYNIX_SAME_VERSION_FOUND AND SYNIX_PREVIOUS_INSTALLFOLDER))");
		Assert.Contains(source.Descendants(Wix + "Publish"), element => (string?)element.Attribute("Dialog") == "VerifyReadyDlg" &&
			(string?)element.Attribute("Control") == "Back" && (string?)element.Attribute("Value") == "LicenseAgreementDlg" &&
			(string?)element.Attribute("Condition") == "NOT Installed AND (SYNIX_REPAIR_CONFIRMED = \"1\" OR (WIX_UPGRADE_DETECTED AND NOT SYNIX_SAME_VERSION_FOUND AND SYNIX_PREVIOUS_INSTALLFOLDER))");
		XElement cancel = Assert.Single(source.Descendants(Wix + "Control"),
			element => (string?)element.Attribute("Id") == "Cancel");
		Assert.Equal("yes", (string?)cancel.Attribute("Default"));
		Assert.Equal("yes", (string?)cancel.Attribute("Cancel"));
		Assert.Equal("Exit", (string?)Assert.Single(cancel.Elements(Wix + "Publish")).Attribute("Value"));
		XDocument localization = XDocument.Load(Path.Combine(PackagingDirectory, "Package.en-us.wxl"));
		XNamespace wxl = "http://wixtoolset.org/schemas/v4/wxl";
		Assert.Equal(@"{\WixUI_Font_Bigger}Repair or uninstall Synix", (string?)Assert.Single(
			localization.Descendants(wxl + "String"), element =>
				(string?)element.Attribute("Id") == "MaintenanceWelcomeDlgTitle").Attribute("Value"));
		Assert.Equal("&Uninstall", (string?)Assert.Single(localization.Descendants(wxl + "String"),
			element => (string?)element.Attribute("Id") == "MaintenanceTypeDlgRemoveButton").Attribute("Value"));
	}

	[Fact]
	public void SameVersionRebuildRequiresExplicitRepairAndCannotRelocateTheApp()
	{
		XDocument source = Package;
		XElement guard = Assert.Single(source.Descendants(Wix + "CustomAction"),
			element => (string?)element.Attribute("Id") == "SynixBlockSameVersionInstall");
		Assert.Contains("already installed", (string?)guard.Attribute("Error"));
		Assert.Null(guard.Attribute("ExeCommand"));
		XElement schedule = Assert.Single(source.Descendants(Wix + "InstallExecuteSequence").Elements(Wix + "Custom"),
			element => (string?)element.Attribute("Action") == "SynixBlockSameVersionInstall");
		Assert.Equal("InstallInitialize", (string?)schedule.Attribute("Before"));
		Assert.Equal("NOT Installed AND NOT (REMOVE = \"ALL\") AND SYNIX_SAME_VERSION_FOUND AND SYNIX_REPAIR_CONFIRMED <> \"1\"", (string?)schedule.Attribute("Condition"));
		XElement consent = Assert.Single(source.Descendants(Wix + "Property"), element => (string?)element.Attribute("Id") == "SYNIX_REPAIR_CONFIRMED");
		Assert.Equal("yes", (string?)consent.Attribute("Secure"));
		Assert.Null(consent.Attribute("Value"));
		XElement duplicates = Assert.Single(source.Descendants(Wix + "Launch"), element => ((string?)element.Attribute("Condition"))?.Contains(';') == true);
		Assert.Equal("Installed OR REMOVE = \"ALL\" OR NOT (SYNIX_SAME_VERSION_FOUND >< \";\")", (string?)duplicates.Attribute("Condition"));
		Assert.Contains(source.Descendants(Wix + "Launch"), element =>
			(string?)element.Attribute("Condition") == "Installed OR REMOVE = \"ALL\" OR NOT SYNIX_SAME_VERSION_FOUND OR SYNIX_PREVIOUS_INSTALLFOLDER");
		Assert.Contains(source.Descendants(Wix + "InstallExecuteSequence").Elements(Wix + "Custom"), element =>
			(string?)element.Attribute("Action") == "SynixBlockRepairFolderChange" &&
			(string?)element.Attribute("Condition") == "NOT Installed AND NOT (REMOVE = \"ALL\") AND WIX_UPGRADE_DETECTED AND SYNIX_PREVIOUS_INSTALLFOLDER AND NOT (INSTALLFOLDER ~= SYNIX_PREVIOUS_INSTALLFOLDER)");
		XElement overwrite = Assert.Single(source.Descendants(Wix + "SetProperty"), element => (string?)element.Attribute("Id") == "REINSTALLMODE");
		Assert.Equal("amus", (string?)overwrite.Attribute("Value"));
		Assert.Equal("CostFinalize", (string?)overwrite.Attribute("Before"));
		Assert.Equal("both", (string?)overwrite.Attribute("Sequence"));
		Assert.Equal("NOT Installed AND SYNIX_SAME_VERSION_FOUND AND SYNIX_REPAIR_CONFIRMED = \"1\"", (string?)overwrite.Attribute("Condition"));
	}

	[Fact]
	public void InstallerUiUsesTheSameWixVersionAndTheSynixLicense()
	{
		XDocument project = XDocument.Load(Path.Combine(PackagingDirectory, "SynixInstaller.wixproj"));
		XElement extension = Assert.Single(project.Descendants("PackageReference"),
			element => (string?)element.Attribute("Include") == "WixToolset.UI.wixext");
		Assert.Equal("WixToolset.Sdk/" + (string?)extension.Attribute("Version"),
			(string?)project.Root?.Attribute("Sdk"));
		XElement license = Assert.Single(Package.Descendants(Wix + "WixVariable"),
			element => (string?)element.Attribute("Id") == "WixUILicenseRtf");
		Assert.Equal(@"$(var.SynixProjectDir)\Packaging\MSI\License.rtf", (string?)license.Attribute("Value"));
	}

	[Fact]
	public void InstallerEmbedsTheApprovedArtworkWithoutChangingOrStretchingIt()
	{
		XElement artwork = Assert.Single(Package.Descendants(Wix + "WixVariable"),
			element => (string?)element.Attribute("Id") == "WixUIDialogBmp");
		Assert.Equal(@"$(var.SynixProjectDir)\Packaging\MSI\Assets\SynixSidebar.png",
			(string?)artwork.Attribute("Value"));
		string artworkPath = Path.Combine(PackagingDirectory, "Assets", "SynixSidebar.png");
		using FileStream stream = File.OpenRead(artworkPath);
		Assert.Equal("E284B79A1C4B366B417BDBEDDE1C22BE7AEC7127E5E18B123DD25E07CA59F216",
			Convert.ToHexString(SHA256.HashData(stream)));
		using Image image = Image.FromFile(artworkPath);
		Assert.Equal(909, image.Width);
		Assert.Equal(1730, image.Height);
		Assert.InRange(Math.Abs((double)image.Width / image.Height - 123d / 234), 0, 0.001);
	}

	[Fact]
	public void ArtworkStaysInTheLeftPanelOnEveryStockFullHeightDialog()
	{
		XDocument localization = XDocument.Load(Path.Combine(PackagingDirectory, "Package.en-us.wxl"));
		XNamespace wxl = "http://wixtoolset.org/schemas/v4/wxl";
		XElement[] layouts = localization.Descendants(wxl + "UI").ToArray();
		string[] dialogs = ["WelcomeDlg", "MaintenanceWelcomeDlg", "PrepareDlg", "ResumeDlg", "ExitDialog", "FatalError", "UserExit"];
		Assert.Equal(dialogs.Length, layouts.Length);
		foreach (string dialog in dialogs)
		{
			XElement layout = Assert.Single(layouts, element => (string?)element.Attribute("Dialog") == dialog);
			Assert.Equal("Bitmap", (string?)layout.Attribute("Control"));
			Assert.Equal("0", (string?)layout.Attribute("X"));
			Assert.Equal("0", (string?)layout.Attribute("Y"));
			Assert.Equal("123", (string?)layout.Attribute("Width"));
			Assert.Equal("234", (string?)layout.Attribute("Height"));
			Assert.Null(layout.Attribute("Text"));
		}
		// The installer still owns only the app executable, not a loose image on disk.
		Assert.Single(Package.Descendants(Wix + "File"));
	}

	[Fact]
	public void InstallerBuildRequiresTheSidebarAsset()
	{
		XDocument project = XDocument.Load(Path.Combine(PackagingDirectory, "SynixInstaller.wixproj"));
		XElement validation = Assert.Single(project.Descendants("Target"),
			element => (string?)element.Attribute("Name") == "ValidateSynixMsiInputs");
		Assert.Contains("'Packaging', 'MSI', 'Assets', 'SynixSidebar.png'",
			Assert.Single(validation.Descendants("_SynixInstallerSidebar")).Value);
		Assert.Contains(validation.Elements("Error"), element =>
			(string?)element.Attribute("Condition") == "!Exists('$(_SynixInstallerSidebar)')");
	}

	[Fact]
	public void SetupWizardRequiresAnExplicitUncheckedSynixLicenseAgreement()
	{
		XDocument source = Package;
		XElement wizard = Assert.Single(source.Descendants(Ui + "WixUI"));
		Assert.Equal("WixUI_InstallDir", (string?)wizard.Attribute("Id"));
		// WiX supplies the license dialog, its unchecked checkbox, and the Next
		// enable/disable conditions. Never bypass or pre-accept them in our wizard.
		Assert.DoesNotContain(source.Descendants(), element =>
			(element.Name == Wix + "Property" || element.Name == Wix + "SetProperty") &&
			(string?)element.Attribute("Id") == "LicenseAccepted");
		Assert.DoesNotContain(source.Descendants(Wix + "Dialog"), element =>
			(string?)element.Attribute("Id") == "LicenseAgreementDlg");
		Assert.DoesNotContain(source.Descendants(Wix + "Publish"), element =>
			(string?)element.Attribute("Property") == "LicenseAccepted" ||
			(string?)element.Attribute("Dialog") == "LicenseAgreementDlg" ||
			((string?)element.Attribute("Dialog") == "WelcomeDlg" &&
			 (string?)element.Attribute("Control") == "Next"));

		XDocument localization = XDocument.Load(Path.Combine(PackagingDirectory, "Package.en-us.wxl"));
		XNamespace wxl = "http://wixtoolset.org/schemas/v4/wxl";
		string? Label(string id) => (string?)Assert.Single(localization.Descendants(wxl + "String"),
			element => (string?)element.Attribute("Id") == id).Attribute("Value");
		Assert.Equal(@"{\WixUI_Font_Title}Synix license agreement", Label("LicenseAgreementDlgTitle"));
		Assert.Equal("Read the license. You must accept its terms to continue.", Label("LicenseAgreementDlgDescription"));
		Assert.Equal("I &accept the terms of the Synix license agreement", Label("LicenseAgreementDlgLicenseAcceptedCheckBox"));
	}

	[Fact]
	public void DisplayedLicenseContainsTheEntireCanonicalLicenseWithoutChangedTerms()
	{
		string canonical = File.ReadAllText(Path.Combine(ProjectDirectory, "..", "LICENSE.md"));
		string expected = string.Join('\n', canonical.ReplaceLineEndings("\n").Split('\n')
			.Select(line => Regex.Replace(line, @"^#{1,6} ", "").Replace("**", "").TrimEnd())).Trim();
		WorkflowUiTest.Run(() =>
		{
			using RichTextBox text = new();
			text.LoadFile(Path.Combine(PackagingDirectory, "License.rtf"), RichTextBoxStreamType.RichText);
			Assert.Equal(expected, text.Text.ReplaceLineEndings("\n").Trim());
		});
	}
}
