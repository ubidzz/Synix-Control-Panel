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
using Synix_Control_Panel.SynixApp.Design;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class HelpGUI : Form
	{
		private const uint WdaExcludeFromCapture = 0x00000011;
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 10;
		private const int HtRight = 11;
		private const int HtTop = 12;
		private const int HtTopLeft = 13;
		private const int HtTopRight = 14;
		private const int HtBottom = 15;
		private const int HtBottomLeft = 16;
		private const int HtBottomRight = 17;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;
		private const int ResizeBorder = 7;
		private const int EmSetCueBanner = 0x1501;
		private readonly ModernSettingsButton btnDonateAction = new();

		private static readonly (
			string Key,
			string DisplayName,
			string Index)[] CategoryDefinitions =
		[
			("Start", "Getting Started & Setup", "01"),
			("Dash", "Dashboard & Controls", "02"),
			("Config", "Server Configuration", "03"),
			("Net", "Networking & IP Rules", "04"),
			("Maint", "Maintenance & Discord", "05"),
			("Watch", "Watchdog & Safeguards", "06"),
			("Trouble", "Troubleshooting & System", "07"),
			("Games", "Game Engines & Custom Rules", "08"),
			("Support", "Support, License & Donate", "09")
		];

		private Dictionary<string, HelpItem> _helpData =
			new(StringComparer.OrdinalIgnoreCase);
		private int _visibleArticleCount;

		public HelpGUI()
		{
			InitializeComponent();

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}
			ThemeManager.Apply(this);

			InitializeData();
			PopulateTree();
			ShowWelcome();

			_ = SendMessageText(
				txtSearch.Handle,
				EmSetCueBanner,
				IntPtr.Zero,
				"Search topics, guides, or answers...");

			btnDonateAction.Text = "Open PayPal Donation";
			btnDonateAction.UseAccentStyle = true;
			btnDonateAction.Size = new Size(176, 38);
			// Position it inside the QR card right below lblQrCaption
			btnDonateAction.Location = new Point(20, 304);
			btnDonateAction.Click += (s, e) =>
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8",
					UseShellExecute = true
				});
			};
			qrCard.Controls.Add(btnDonateAction);
		}

		private void InitializeData()
		{
			_helpData = new Dictionary<string, HelpItem>(
				StringComparer.OrdinalIgnoreCase)
			{
				// --- 1. GETTING STARTED (Category: "Start") ---
				["First-Time Setup Guide"] = new HelpItem("Start",
					"WELCOME TO SYNIX CONTROL PANEL!\n\n" +
					"Synix is designed as a zero-admin, non-invasive management suite for game servers on Windows 11.\n\n" +
					"STEP-BY-STEP INITIAL BOOT:\n" +
					"1. SteamCMD Engine Download: On first startup, Synix automatically downloads and configures SteamCMD into `C:\\Synix\\SteamCMD`.\n" +
					"2. Game Binary Acquisition: Navigate to the 'SteamCMD' tab, select your desired game title, and click 'Download Game Files'.\n" +
					"3. Server Creation: Click 'Add New Server'. Choose your game template, fill in server details, and save.\n" +
					"4. First Boot Launch: Select your new server in the dashboard list and click 'Start Server'."),

				["Understanding Server {Identity}"] = new HelpItem("Start",
					"WHAT IS AN {IDENTITY}?\n\n" +
					"Synix containerizes every game server using a unique, sanitized `{Identity}` string.\n\n" +
					"WHY THIS IS CRITICAL:\n" +
					"• Collision Prevention: Prevents multiple instances of the same game (e.g., two Rust servers) from reading or overwriting each other's configuration files.\n" +
					"• Folder Path Sanitization: Synix strips out illegal Windows characters (`<`, `>`, `*`, `?`, `\"`, `\\`, `|`, `/`) and spaces to ensure launch arguments execute cleanly without command-line parameter failure.\n" +
					"• Total Portability: The entire root structure is relative. Moving `C:\\Synix` to another drive auto-heals all internal pointers."),

				["File Structure, Default Directories & Custom Paths"] = new HelpItem("Start",
					"SYNIX FILE ECOSYSTEM & FOLDER MANAGEMENT:\n\n" +
					"All files are isolated in user-space to keep your host OS clean:\n\n" +
					"• Root Control Engine: `C:\\Synix` (Application executables and internal JSON state databases).\n" +
					"• SteamCMD Core: `C:\\Synix\\SteamCMD` (Shared Steam binary downloader library).\n" +
					"• Default Active Server Directory: `C:\\Synix\\Games\\[Game_Name]\\{Server_Name}` (Contains dedicated binaries, game configs, and world saves).\n" +
					"• Custom Folder Locations: Users are not strictly bound to the default path; you can assign a custom directory path for any server during setup or editing.\n" +
					"• Backup Repository: `C:\\Synix\\BackupGames\\[Game_Name]\\{Server_Name}` (Default) OR your Custom Backup Location defined in Settings. Note: When deleting a server, you can use the verification checkbox to automatically wipe all associated .zip backup archives at the same time."),

				["Winget Package Installation"] = new HelpItem("Start",
					"WINGET DEPLOYMENT & MAINTENANCE:\n\n" +
					"Synix can be installed, updated, or removed via Windows Package Manager (`winget`) using Command Prompt (`cmd`) or PowerShell.\n\n" +
					"INSTALLATION COMMAND:\n" +
					"  winget install synix\n\n" +
					"UNINSTALLATION COMMAND:\n" +
					"  winget uninstall synix\n\n" +
					"Note: Winget automatically registers Synix in your Start Menu and Windows Settings 'Installed Apps' list."),

				// --- 2. DASHBOARD & CONTROLS (Category: "Dash") ---
				["Main Dashboard Operations"] = new HelpItem("Dash",
					"SERVER CONTROL SEQUENCES:\n\n" +
					"• START SERVER: Initiates pre-flight checks (RAM safety check, process sanity, optional Backup on Start, optional Update on Start) and launches the dedicated executable.\n" +
					"• STOP SERVER: Initiates a staged, graceful shutdown sequence. Synix issues a soft termination signal to trigger world/player saves before using process termination fallback if necessary.\n" +
					"• RESTART SERVER: Executes a staged Stop sequence followed by an automated delay and clean boot cycle."),

				["Server Details & Double-Click Inspector"] = new HelpItem("Dash",
					"INSPECTING RUNNING SERVERS:\n\n" +
					"Double-click on any server row in the main grid view to launch the interactive 'Server Info' Inspector Window.\n\n" +
					"FEATURES IN SERVER INFO:\n" +
					"• Real-time PID tracking and memory consumption.\n" +
					"• Exact command-line launch arguments verification.\n" +
					"• Quick-access shortcuts to log files and root game directories.\n" +
					"• Live LAN / WAN port binding telemetry."),

				["Live Resource Telemetry Graph"] = new HelpItem("Dash",
					"RESOURCE TELEMETRY GRAPH:\n\n" +
					"The interactive graph at the bottom of the dashboard monitors global host CPU and RAM utilization.\n\n" +
					"RESOURCE MONITOR DEEP-DIVE:\n" +
					"Click anywhere on the total resource graph to open the dedicated 'Resource Monitor' window. Here you can inspect PID, CPU utilization, working memory, and executable paths for every active game server managed by Synix."),

				["Global Settings Menu & Privacy Mode"] = new HelpItem("Dash",
					"SYNIX GLOBAL SETTINGS:\n\n" +
					"Access the global settings menu via the gear icon in the top right corner of the main dashboard.\n\n" +
					"FEATURES:\n" +
					"• Custom Backups & Rolling Limits: Route all automated and manual server backups to a preferred global folder or secondary drive, and define a maximum saved backups limit to automatically purge old archives.\n" +
					"• Privacy Mode: Hides sensitive information (like IPs) while screen sharing. (Tip: You can still click the hidden IP labels on the dashboard to copy your actual IP address to your clipboard!)\n" +
					"• Run as Administrator: Enables advanced system management tasks (like Firewall Cleanup)."),

				// --- 3. SERVER CONFIGURATION (Category: "Config") ---
				["Adding vs Editing Servers & FirstBoot"] = new HelpItem("Config",
					"NEW INSTALL VS EDIT SERVER LOGIC:\n\n" +
					"• New Server Install: When creating a server, `IsFirstBoot` is set to `TRUE`. This ensures the setup wizard executes, downloads initial binaries, and prompts configuration windows.\n" +
					"• Editing Existing Servers: Editing a server via 'Edit Server Settings' enforces `IsFirstBoot = FALSE`. The configuration warning window will NOT trigger repeatedly when adjusting existing server settings.\n" +
					"• Folder Cleaning: Modifying a server name automatically triggers path sanitization to guarantee path stability on disk."),

				["The Port Trio & App Port Architecture"] = new HelpItem("Config",
					"UNDERSTANDING PORT TYPES:\n\n" +
					"Every hosted game server requires specific port bindings:\n\n" +
					"1. GAME PORT (UDP): Core gameplay data and player movement packet synchronization.\n" +
					"2. QUERY PORT (UDP): Used by Steam Server Browser and Master Lists (e.g., 27015/27016). Always assign unique query ports per server instance.\n" +
					"3. RCON PORT (TCP): Remote console administration protocol for kick, ban, and state management tools.\n" +
					"4. APP PORT (TCP): Specialized external management API port used by titles like Rust (Rust+ Mobile Companion App). Rust+ ports must always be set above 10000."),

				["Configuration Warning Dialog"] = new HelpItem("Config",
					"CONFIGURATION WARNING WINDOW:\n\n" +
					"When launching a brand-new game server for the first time, Synix displays a reminder dialogue prompting you to review configuration options (`server.cfg`, admin passwords, maps).\n\n" +
					"This dialogue only appears on new server installations and will not bother you during subsequent boots or parameter edits."),

				// --- 4. NETWORKING & IP (Category: "Net") ---
				["Local Link vs WAN Link Diagnostic"] = new HelpItem("Net",
					"NETWORK PROBING ARCHITECTURE:\n\n" +
					"Synix utilizes a two-tier network diagnostic check shown on the dashboard:\n\n" +
					"• LOCAL LINK [ONLINE / OFFLINE]: Probes your local adapter IP and process listener status. If ONLINE, your local process is running and Windows Firewall is allowing local traffic.\n" +
					"• WAN LINK [ONLINE / HIDDEN]: Probes your Public IP address over NAT. If HIDDEN, external players cannot find your server in the public browser list. Check router port forwarding."),

				["Port Forwarding Master Guide"] = new HelpItem("Net",
					"PORT FORWARDING INSTRUCTIONS:\n\n" +
					"To make your server accessible on the internet, forward the required ports on your home router:\n\n" +
					"1. Determine your LAN IP (e.g., `192.168.1.50`) from the bottom status bar in Synix.\n" +
					"2. Open your router's admin panel (typically `192.168.1.1` or `192.168.0.1`).\n" +
					"3. Add Port Forwarding Rules pointing to your LAN IP:\n" +
					"   • Game Port -> UDP\n" +
					"   • Query Port -> UDP\n" +
					"   • RCON / App Port -> TCP\n" +
					"4. Save router settings and re-run the WAN diagnostic test in Synix."),

				["NAT Hairpinning & Joining Your Own Server"] = new HelpItem("Net",
					"ROUTER LOOPBACK / NAT HAIRPINNING:\n\n" +
					"Symptom: Your friends can join via your Public IP, but you get 'Connection Timeout' when trying to use your own Public IP.\n\n" +
					"CAUSE: Most home routers lack 'NAT Loopback/Hairpinning' support and block internal devices from routing back into their own public WAN interface.\n\n" +
					"SOLUTION: Connect to your server using your local LAN IP (e.g., `192.168.x.x`) or loopback address (`127.0.0.1`). External players must continue to use your Public IP."),

				["Steam Master Server Query Rules"] = new HelpItem("Net",
					"STEAM MASTER LIST REGISTRATION:\n\n" +
					"Steam indexes game servers via A2S_INFO query protocols sent to the Query Port.\n\n" +
					"IMPORTANT RULES:\n" +
					"• Standard Query Ports: 27015, 27016, 27017 are preferred defaults.\n" +
					"• Delay on First Boot: It can take 5 to 15 minutes for Steam's Master Server indexers to broadcast a newly created server globally.\n" +
					"• Query Port Clashes: Ensure no two servers on your local network share the same Query Port."),

				["Synix Network Guard"] = new HelpItem("Net",
					"SYNIX NETWORK GUARD MODULE:\n\n" +
					"Network Guard continuously monitors global bandwidth across your network adapter.\n\n" +
					"FEATURES:\n" +
					"• Surge Monitoring: Detects extreme packet/bandwidth floods that exceed regular gameplay.\n" +
					"• False-Positive Suppression: Automatically suppresses network flood warnings when active SteamCMD downloads or game updates are detected.\n" +
					"• Alert Overlay: Displays visual desktop notifications if network saturation occurs while you are tabbed out."),

				// --- 5. AUTOMATION & DISCORD (Category: "Maint") ---
				["Smart Backup on Start"] = new HelpItem("Maint",
					"AUTOMATED WORLD BACKUPS:\n\n" +
					"Enabling 'Backup on Start' in server settings triggers an automated archival process before the game boots.\n\n" +
					"BEHAVIOR:\n" +
					"1. Synix targets the server's save data directory (default or custom path).\n" +
					"2. Compresses world state into a ZIP file.\n" +
					"3. Stores the ZIP archive in your designated Backup Repository with a timestamp suffix.\n" +
					"4. Rolling Limit Enforcement: Synix checks the 'Max Saved Backups Limit' in your Global Settings and automatically deletes the oldest archive if the limit is exceeded.\n" +
					"5. Automated crash recoveries bypass backups to ensure rapid reboot times."),

				["Smart Update on Start & Manifest Validation"] = new HelpItem("Maint",
					"AUTOMATED GAME UPDATES:\n\n" +
					"• Update on Start: When enabled, Synix contacts SteamCMD before launching the server to download any newly released game patches.\n" +
					"• Validate Binaries: Comparing local files against the official Steam Master Manifest repairs corrupt executables without altering or deleting save files."),

				["Discord Webhook Integration"] = new HelpItem("Maint",
					"SETTING UP DISCORD WEBHOOK NOTIFICATIONS:\n\n" +
					"Receive real-time notifications directly in your Discord server:\n\n" +
					"1. Open Discord -> Server Settings -> Integrations -> Webhooks.\n" +
					"2. Click 'New Webhook', copy the Webhook URL.\n" +
					"3. Open Synix -> Edit Server -> Paste URL into 'Discord Webhook' field.\n" +
					"4. Synix will post rich embedded messages for:\n" +
					"   • 🚀 Server Boot Events\n" +
					"   • 🛑 Graceful Shutdowns\n" +
					"   • ⚠️ Watchdog Recovery & Crash Reboots"),

				// --- 6. WATCHDOG & RESOURCE GUARD (Category: "Watch") ---
				["Autonomous Watchdog Loop"] = new HelpItem("Watch",
					"AUTONOMOUS WATCHDOG HEALTH MONITORING:\n\n" +
					"The Synix Watchdog operates as a background thread monitoring server process loop health.\n\n" +
					"RECOVERY TIMELINE:\n" +
					"• Heartbeat Monitoring: Monitors PID execution and responsiveness continuously.\n" +
					"• Freeze Detection: If a process hangs or becomes unresponsive for >60 seconds, the Watchdog marks it as CRASHED.\n" +
					"• Staged Recovery: The Watchdog safely terminates the frozen PID, logs the failure, sends a Discord alert, and reboots the game server automatically."),

				["Resource Guard (RAM Buffer & CPU Throttling)"] = new HelpItem("Watch",
					"PROACTIVE HARDWARE STEWARDSHIP:\n\n" +
					"Resource Guard prevents game server hosting from crashing your Windows host operating system.\n\n" +
					"SAFETY POLICIES:\n" +
					"• 5GB RAM Overhead: Synix calculates system memory headroom and enforces a strict 5GB RAM reserve for Windows kernel tasks.\n" +
					"• 85% CPU Ingress Throttle: New server boot sequences are blocked if host CPU utilization exceeds 85% to maintain game server tick rates."),

				["Process Rebinding on Application Restart"] = new HelpItem("Watch",
					"APPLICATION REBINDING LOGIC:\n\n" +
					"If Synix is closed or updated while game servers or SteamCMD are actively running, reopening Synix initiates process rebinding.\n\n" +
					"The engine queries active OS PID handles, matches binary names to `{Identity}` configurations, and restores live monitoring without interrupting active players or background SteamCMD downloads."),

				// --- 7. TROUBLESHOOTING & SYSTEM (Category: "Trouble") ---
				["No-Admin Philosophy & Windows Firewall"] = new HelpItem("Trouble",
					"USER-MODE SOVEREIGNTY (NO-ADMIN):\n\n" +
					"Synix runs entirely in User-Mode without requesting Administrator (UAC) privileges by default. It will not edit your Windows registry or modify host system settings.\n\n" +
					"SMART FIREWALL CLEANUP (OPTIONAL):\n" +
					"If you enable the 'Run as Administrator' toggle in the Settings window, Synix can perform advanced system tasks. Currently, this allows Synix to use Just-In-Time Elevation to automatically find and delete orphaned Windows Firewall rules when you permanently delete a game server. If you decline the Windows UAC prompt, Synix gracefully skips the task and continues normal operations.\n\n" +
					"FIREWALL REQUIREMENTS:\n" +
					"Because Synix runs without Admin privileges by default, Windows Defender Firewall may prompt you the first time a game server binary executes. Always check 'Allow on Private and Public Networks' when prompted by Windows."),

				["Missing Visual C++ Redistributables & DLL Errors"] = new HelpItem("Trouble",
					"RESOLVING MISSING DLL ERRORS:\n\n" +
					"Symptom: Server fails to launch or throws error `MSVCP140.dll` / `VCRUNTIME140.dll missing`.\n\n" +
					"SOLUTION:\n" +
					"Install the official Microsoft Visual C++ Redistributable Package (x64):\n" +
					"https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-supported-redistributable-version"),

				["Windows SmartScreen & SAC Warnings"] = new HelpItem("Trouble",
					"SECURITY PROMPTS & SMART APP CONTROL:\n\n" +
					"As an independent community project without a paid Microsoft Digital Signature, Synix may trigger security prompts on new Windows installations:\n\n" +
					"• Windows SmartScreen: Click `More Info` -> `Run Anyway`.\n" +
					"• Windows 11 Smart App Control (SAC): SAC blocks unsigned executables entirely. If enabled, set SAC to 'Evaluation' or 'Off' to run community tools like Synix."),

				["Where are My Server Backups Located?"] = new HelpItem("Trouble",
					"LOCATING BACKUP ARCHIVES:\n\n" +
					"By default, all automated ZIP backups created by Synix are stored at:\n\n" +
					"  C:\\Synix\\BackupGames\\[Game_Name]\\{Server_Name}\\\n\n" +
					"If you have activated a Custom Backup Location in the Synix Settings menu, your backups will be routed to that specific drive/folder instead.\n\n" +
					"Note: Synix actively monitors this folder and will automatically delete the oldest backup if your rolling 'Max Saved Backups Limit' (configurable in Settings) is reached.\n\n" +
					"You can extract any ZIP directly over your active server folder to restore previous world saves."),

				// --- 8. SUPPORTED GAMES (Category: "Games") ---
				["Internal IReadOnlyList<GameInfo> Database"] = new HelpItem("Games",
					"INTERNAL GAME DATABASE ARCHITECTURE:\n\n" +
					"Synix does not use SQLite or external SQL databases. Instead, it relies entirely on a high-performance, compiled internal `IReadOnlyList<GameInfo>` database.\n\n" +
					"WHY THIS DESIGN:\n" +
					"• Absolute Thread Safety: Compiled directly into the engine, guaranteeing instant lookups with zero database corruption risks.\n" +
					"• Strict Parameter Enforced: Because game executables and Steam AppIDs require strict arguments, manual custom game plugins or third-party database tables are not supported."),

				["Dune: Awakening Setup & Custom Rules"] = new HelpItem("Games",
					"DUNE: AWAKENING ENGINE INTEGRATION:\n\n" +
					"Dune: Awakening requires unique engine execution rules:\n\n" +
					"1. Admin Execution: Dune: Awakening dedicated launchers require Administrator privileges to execute their startup scripts properly.\n" +
					"2. Custom Batch File Handling: Synix disables standard batch file creation for Dune: Awakening, as the game utilizes its own internal launcher structure.\n" +
					"3. Folder Sanitization: Ensure paths contain no illegal characters prior to initial download."),

				["Rust & Rust+ Mobile App Integration"] = new HelpItem("Games",
					"RUST SERVER & RUST+ MOBILE CONFIGURATION:\n\n" +
					"• Steam AppID: 258550\n" +
					"• App Port (Rust+): Set to a unique port above 10000 (e.g., 28082 TCP).\n" +
					"• Identity Isolation: Synix enforces `+server.identity \"{Server_Name}\"` automatically to isolate world save files and blueprints cleanly."),

				["Minecraft Java Automation"] = new HelpItem("Games",
					"MINECRAFT JAVA EDITION INTEGRATION:\n\n" +
					"Synix provides full lifecycle automation for Vanilla Minecraft Java servers.\n\n" +
					"FEATURES:\n" +
					"• Automated Java Provisioning: Synix queries the Mojang API to determine the exact Java version required. If missing, it safely downloads a portable Eclipse Temurin JRE specifically for that server.\n" +
					"• EULA Compliance: During the first boot, Synix intercepts the startup to present Microsoft's EULA. Clicking 'I Agree' automatically generates the required eula.txt file.\n" +
					"• Configuration: Settings are managed natively through the 'server.properties' file via the Synix editor.\n" +
					"• Native Player Tracking: Synix uses a custom TCP Server List Ping (SLP) protocol to display live, accurate player counts on your dashboard.\n\n" +
					"Note: Synix currently supports official Vanilla Minecraft (modded environments like Forge/Fabric are not included)."),

				// --- 9. COMMUNITY & LEGAL (Category: "Support") ---
				["Official Support Links"] = new HelpItem("Support",
					"COMMUNITY & SUPPORT RESOURCES:\n\n" +
					"• GitHub Issues: https://github.com/ubidzz/Synix-Control-Panel/issues \n" +
					"• Official Discord: https://discord.gg/2WR7ArC2Vr \n" +
					"• Feature Requests: https://discord.gg/ZKTcpgmXNM \n" +
					"• Game Support Requests: https://discord.gg/DxUXPtyVm9 \n" +
					"• YouTube Video Showcase: https://www.youtube.com/watch?v=EcVLT4kgdb8&t=1796s"),

				["License & Proprietary Terms"] = new HelpItem("Support",
					"SYNIX CONTROL PANEL — LIMITED PROPRIETARY SOURCE-AVAILABLE LICENSE\n" +
					"\n" +
					"Version 1.0 — August 2, 2026\n" +
					"Copyright © 2026 Jason Turner. All Rights Reserved.\n" +
					"U.S. Copyright Registration Application: Pending\n" +
					"\n" +
					"This license applies to the Synix Control Panel software, including its source code, compiled binaries, user interface, documentation, artwork, logos, and other included materials (collectively, the \"Software\").\n" +
					"\n" +
					"1. OWNERSHIP\n" +
					"\n" +
					"The Software is licensed, not sold.\n" +
					"\n" +
					"Jason Turner retains all right, title, and interest in and to the Software, including all copyrights and other intellectual-property rights. No ownership rights are transferred to you under this license.\n" +
					"\n" +
					"2. LIMITED LICENSE GRANT\n" +
					"\n" +
					"Subject to all terms of this license, you are granted a limited, non-exclusive, non-transferable, non-sublicensable, and revocable license to:\n" +
					"\n" +
					"• Download and use the Software for your own personal, non-commercial purposes.\n" +
					"• View and study the source code.\n" +
					"• Make backup copies for your own personal use.\n" +
					"• Modify the source code for your own personal, non-commercial use.\n" +
					"• Compile and run your personal modifications on devices you own or control.\n" +
					"\n" +
					"Any right not expressly granted by this license is reserved by the copyright holder.\n" +
					"\n" +
					"3. PERSONAL MODIFICATIONS\n" +
					"\n" +
					"You may modify the Software to meet your own personal requirements.\n" +
					"\n" +
					"Personal modifications must remain private unless Jason Turner gives you prior written permission to distribute them.\n" +
					"\n" +
					"You may not publish, upload, share, release, distribute, or make modified source code or modified compiled binaries available to the public as a separate version of Synix.\n" +
					"\n" +
					"4. RESTRICTIONS\n" +
					"\n" +
					"You may not, without prior written permission from Jason Turner:\n" +
					"\n" +
					"4.1 COMMERCIAL USE\n" +
					"\n" +
					"• Sell, rent, lease, license, sublicense, or charge for the Software.\n" +
					"• Sell or distribute modified or unmodified compiled binaries.\n" +
					"• Use the Software or its source code as part of a paid product or commercial service.\n" +
					"• Directly or indirectly profit from distributing, licensing, rebranding, or providing access to the Software.\n" +
					"\n" +
					"4.2 REDISTRIBUTION AND PUBLIC HOSTING\n" +
					"\n" +
					"• Upload, mirror, or host the Software on another website, repository, download service, file-sharing platform, or application store.\n" +
					"• Redistribute the source code or compiled binaries, whether modified or unmodified.\n" +
					"• Create or publish unofficial installers, portable packages, mirrors, or download links.\n" +
					"• Publish releases from a fork or modified copy of the Software.\n" +
					"• Present a fork, modification, or derivative version as an independent or competing product.\n" +
					"\n" +
					"4.3 REBRANDING AND ATTRIBUTION\n" +
					"\n" +
					"• Remove, hide, or alter the Synix name, copyright notices, author credits, license notices, logos, or attribution.\n" +
					"• Rename or rebrand the Software for public release or distribution.\n" +
					"• Claim that you created the original Software.\n" +
					"• Use the Synix name or branding in a way that falsely suggests sponsorship, approval, partnership, or official status.\n" +
					"\n" +
					"4.4 REVERSE ENGINEERING AND PROTECTION BYPASS\n" +
					"\n" +
					"To the maximum extent permitted by applicable law, you may not reverse engineer, decompile, disassemble, or otherwise attempt to bypass licensing notices, attribution, security features, update checks, or branding contained in an official compiled release.\n" +
					"\n" +
					"This restriction does not limit any non-waivable right that applicable law expressly gives you.\n" +
					"\n" +
					"5. GITHUB FORKS AND CONTRIBUTIONS\n" +
					"\n" +
					"The official Synix repository may be publicly visible on GitHub. GitHub's platform may allow users to view and fork a public repository through GitHub's built-in functionality.\n" +
					"\n" +
					"A GitHub fork does not give you permission to:\n" +
					"\n" +
					"• Publish an unofficial Synix release.\n" +
					"• Distribute modified or unmodified binaries.\n" +
					"• Rebrand Synix.\n" +
					"• Operate a separate public download page or mirror.\n" +
					"• Present the fork as your own software.\n" +
					"• Redistribute the Software outside the limited functionality provided by GitHub.\n" +
					"\n" +
					"GitHub forks may be used only to review the code, prepare a contribution, or submit a pull request to the official Synix repository. Personal modifications that are not being submitted as a contribution must remain local and private.\n" +
					"\n" +
					"By submitting source code, documentation, artwork, or another contribution to the official Synix repository, you represent that you have the right to submit it and grant Jason Turner a perpetual, worldwide, non-exclusive, irrevocable, royalty-free license to use, reproduce, modify, publish, distribute, sublicense, and incorporate that contribution into Synix.\n" +
					"\n" +
					"6. SYNIX NAME AND BRANDING\n" +
					"\n" +
					"The Synix name, logo, visual identity, and related branding remain the property of Jason Turner.\n" +
					"\n" +
					"No trademark or branding license is granted by this license except for displaying the unmodified Synix name and branding while personally using the Software.\n" +
					"\n" +
					"Unauthorized use of Synix branding may violate applicable copyright, trademark, unfair-competition, or other laws.\n" +
					"\n" +
					"7. TERMINATION\n" +
					"\n" +
					"Your rights under this license terminate automatically if you violate any term of this license.\n" +
					"\n" +
					"After termination, you must:\n" +
					"\n" +
					"• Stop using and modifying the Software.\n" +
					"• Stop distributing or displaying any unauthorized copy.\n" +
					"• Remove unauthorized public copies, releases, mirrors, or downloads under your control.\n" +
					"• Delete personal copies when legally required to do so.\n" +
					"\n" +
					"Sections concerning ownership, restrictions, contributions, disclaimers, and enforcement survive termination.\n" +
					"\n" +
					"8. ENFORCEMENT\n" +
					"\n" +
					"The copyright holder may pursue any remedies available under applicable law for unauthorized copying, distribution, public hosting, sale, rebranding, or other infringement.\n" +
					"\n" +
					"Available remedies may include removal requests, platform takedown procedures, injunctive relief, damages, and other civil remedies where legally appropriate.\n" +
					"\n" +
					"Nothing in this license guarantees that any particular enforcement action or remedy will be available in every jurisdiction.\n" +
					"\n" +
					"9. DISCLAIMER AND LIMITATION OF LIABILITY\n" +
					"\n" +
					"THE SOFTWARE IS PROVIDED \"AS IS\" AND \"AS AVAILABLE,\" WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.\n" +
					"\n" +
					"TO THE MAXIMUM EXTENT PERMITTED BY LAW, JASON TURNER SHALL NOT BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR EXEMPLARY DAMAGES, OR FOR ANY LOSS OF DATA, PROFITS, REVENUE, SERVER FILES, GAME WORLDS, BUSINESS, OR SERVICE AVAILABILITY ARISING FROM OR RELATED TO THE SOFTWARE OR ITS USE.\n" +
					"\n" +
					"You are responsible for maintaining backups and verifying that the Software is suitable for your system and game-server environment.\n" +
					"\n" +
					"10. THIRD-PARTY COMPONENTS\n" +
					"\n" +
					"Third-party software, libraries, game-server files, SteamCMD components, trademarks, and other materials are owned by their respective owners and remain subject to their own licenses and terms.\n" +
					"\n" +
					"This license applies only to the portions of Synix owned by Jason Turner.\n" +
					"\n" +
					"11. WRITTEN PERMISSION\n" +
					"\n" +
					"Exceptions to this license are valid only when provided in writing by Jason Turner.\n" +
					"\n" +
					"Questions or permission requests should be submitted through the official Synix GitHub repository or another official Synix contact method."),

				["Donate & Support Development"] = new HelpItem("Support",
					"SUPPORT THE PROJECT:\n\n" +
					"Synix Control Panel is developed with passion for the server hosting community. \nYour support keeps updates frequent and features growing!\n\n" +
					"Click the button below or scan the QR code to open the official PayPal \ndonation page securely in your browser.")
			};
		}

		private void PopulateTree(string filter = "")
		{
			string normalizedFilter = filter.Trim();
			string? selectedTopic = treeNavigation.SelectedNode?.Tag as string;
			TreeNode? nodeToReselect = null;
			TreeNode? firstTopicNode = null;
			_visibleArticleCount = 0;

			treeNavigation.BeginUpdate();
			try
			{
				treeNavigation.Nodes.Clear();

				foreach ((string categoryKey, string displayName, string index)
					in CategoryDefinitions)
				{
					TreeNode categoryNode = new(displayName)
					{
						Name = categoryKey,
						ToolTipText = displayName
					};

					foreach (KeyValuePair<string, HelpItem> entry in _helpData)
					{
						if (!string.Equals(
							entry.Value.Category,
							categoryKey,
							StringComparison.OrdinalIgnoreCase) ||
							!MatchesFilter(entry, normalizedFilter))
						{
							continue;
						}

						TreeNode topicNode = new(CreateNavigationCaption(entry.Key))
						{
							Tag = entry.Key,
							ToolTipText = entry.Key
						};
						categoryNode.Nodes.Add(topicNode);
						firstTopicNode ??= topicNode;
						_visibleArticleCount++;

						if (string.Equals(
							entry.Key,
							selectedTopic,
							StringComparison.OrdinalIgnoreCase))
						{
							nodeToReselect = topicNode;
						}
					}

					if (categoryNode.Nodes.Count == 0)
					{
						continue;
					}

					treeNavigation.Nodes.Add(categoryNode);
					categoryNode.Expand();
				}

				if (nodeToReselect != null)
				{
					treeNavigation.SelectedNode = nodeToReselect;
				}
				else if (!string.IsNullOrEmpty(normalizedFilter) &&
					firstTopicNode != null)
				{
					treeNavigation.SelectedNode = firstTopicNode;
					firstTopicNode.EnsureVisible();
				}
			}
			finally
			{
				treeNavigation.EndUpdate();
			}

			UpdateSearchStatus(normalizedFilter);

			if (_visibleArticleCount == 0)
			{
				ShowNoResults(normalizedFilter);
			}
			else if (string.IsNullOrEmpty(normalizedFilter) &&
				selectedTopic == null)
			{
				ShowWelcome();
			}
		}

		private static bool MatchesFilter(
			KeyValuePair<string, HelpItem> entry,
			string filter)
		{
			return string.IsNullOrEmpty(filter) ||
				entry.Key.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
				entry.Value.Answer.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		private static string CreateNavigationCaption(string topicTitle)
		{
			const int MaximumCaptionLength = 34;
			return topicTitle.Length <= MaximumCaptionLength
				? topicTitle
				: topicTitle[..(MaximumCaptionLength - 1)] + "…";
		}

		private void UpdateSearchStatus(string filter)
		{
			lblArticleCount.Text = string.IsNullOrEmpty(filter)
				? $"{_visibleArticleCount} help articles"
				: $"{_visibleArticleCount} matching article" +
					(_visibleArticleCount == 1 ? string.Empty : "s");
			lblFooterStatus.Text = string.IsNullOrEmpty(filter)
				? "KNOWLEDGE BASE READY"
				: $"{_visibleArticleCount} SEARCH RESULT" +
					(_visibleArticleCount == 1 ? string.Empty : "S");
			btnClearSearch.Visible = txtSearch.TextLength > 0;
		}

		private void ShowWelcome()
		{
			qrCard.Visible = false;
			lblTopicCategory.Text = "SYNIX KNOWLEDGE BASE";
			lblTopicTitle.Text = "How can we help?";
			lblArticleBadge.Text = "WELCOME";
			lblAnswer.Text =
				"Welcome to the Synix Engine Knowledge Base.\n\n" +
				"Choose a topic from the navigation panel or search for a game, " +
				"feature, error, networking rule, or setup step. The search checks " +
				"both article names and their full contents, so you can describe the " +
				"problem you are trying to solve.\n\n" +
				"QUICK START\n" +
				"• Start with Getting Started & Setup for first-time installation.\n" +
				"• Open Networking & IP Rules for port and connection guidance.\n" +
				"• Use Troubleshooting & System when a server will not launch.\n" +
				"• Press Ctrl+F at any time to jump directly to search.";
			ResetAnswerScroll();
		}

		private void ShowNoResults(string filter)
		{
			qrCard.Visible = false;
			lblTopicCategory.Text = "SEARCH";
			lblTopicTitle.Text = "No matching help articles";
			lblArticleBadge.Text = "NO RESULTS";
			lblAnswer.Text = string.IsNullOrWhiteSpace(filter)
				? "No help articles are currently available."
				: $"Synix could not find a topic containing \"{filter}\".\n\n" +
					"Try a shorter phrase or search for a related term such as " +
					"ports, backup, watchdog, SteamCMD, RCON, or firewall.";
			ResetAnswerScroll();
		}

		private void ShowTopic(string topicKey, HelpItem item)
		{
			bool isDonationTopic = string.Equals(
				topicKey,
				"Donate & Support Development",
				StringComparison.OrdinalIgnoreCase) ||
				string.Equals(
					topicKey,
					"Donate",
					StringComparison.OrdinalIgnoreCase);

			lblTopicCategory.Text =
				GetCategoryDisplayName(item.Category).ToUpperInvariant();
			lblTopicTitle.Text = topicKey;
			lblArticleBadge.Text = isDonationTopic ? "SUPPORT" : "ARTICLE";
			lblAnswer.Text = item.Answer;
			qrCard.Visible = isDonationTopic;
			btnDonateAction.Visible = isDonationTopic;
			if (isDonationTopic)
			{
				qrCard.BringToFront();
				btnDonateAction.BringToFront();
			}

			btnDonateAction.Visible = isDonationTopic;
			if (isDonationTopic)
			{
				qrCard.BringToFront();
				btnDonateAction.BringToFront();
			}

			lblFooterStatus.Text = "VIEWING HELP ARTICLE";
			ResetAnswerScroll();
		}

		private void ResetAnswerScroll()
		{
			lblAnswer.SelectionStart = 0;
			lblAnswer.SelectionLength = 0;
			lblAnswer.ScrollToCaret();
		}

		private static string GetCategoryDisplayName(string categoryKey)
		{
			foreach ((string key, string displayName, string index)
				in CategoryDefinitions)
			{
				if (string.Equals(
					key,
					categoryKey,
					StringComparison.OrdinalIgnoreCase))
				{
					return displayName;
				}
			}

			return "Help & Support";
		}

		private static string GetCategoryIndex(string categoryKey)
		{
			foreach ((string key, string displayName, string index)
				in CategoryDefinitions)
			{
				if (string.Equals(
					key,
					categoryKey,
					StringComparison.OrdinalIgnoreCase))
				{
					return index;
				}
			}

			return "•";
		}

		private void treeNavigation_AfterSelect(
			object? sender,
			TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node?.Tag is not string topicKey ||
				!_helpData.TryGetValue(topicKey, out HelpItem item))
			{
				return;
			}

			ShowTopic(topicKey, item);
			treeNavigation.Invalidate();
		}

		private void treeNavigation_NodeMouseClick(
			object? sender,
			TreeNodeMouseClickEventArgs eventArgs)
		{
			if (eventArgs.Node.Level != 0)
			{
				return;
			}

			if (eventArgs.Node.IsExpanded)
			{
				eventArgs.Node.Collapse();
			}
			else
			{
				eventArgs.Node.Expand();
			}

			treeNavigation.Invalidate();
		}

		private void treeNavigation_DrawNode(
			object? sender,
			DrawTreeNodeEventArgs eventArgs)
		{
			TreeNode node = eventArgs.Node;
			Graphics graphics = eventArgs.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle rowBounds = new(
				4,
				eventArgs.Bounds.Y + 2,
				Math.Max(1, treeNavigation.ClientSize.Width - 8),
				Math.Max(1, eventArgs.Bounds.Height - 4));
			using SolidBrush sidebarBrush = new(SettingsPalette.Sidebar);
			graphics.FillRectangle(sidebarBrush, rowBounds);

			if (node.Level == 0)
			{
				DrawCategoryNode(graphics, node, rowBounds);
				return;
			}

			bool selected = ReferenceEquals(treeNavigation.SelectedNode, node);
			if (selected)
			{
				using GraphicsPath selectedPath = CreateRoundedRectangle(rowBounds, 8);
				using SolidBrush selectedBrush = new(SettingsPalette.AccentSoft);
				graphics.FillPath(selectedBrush, selectedPath);

				using SolidBrush accentBrush = new(SettingsPalette.Accent);
				graphics.FillRectangle(
					accentBrush,
					rowBounds.Left,
					rowBounds.Top + 7,
					3,
					Math.Max(4, rowBounds.Height - 14));
			}

			Rectangle textBounds = new(
				rowBounds.Left + 34,
				rowBounds.Top,
				Math.Max(0, rowBounds.Width - 44),
				rowBounds.Height);
			string topicTitle = node.Tag as string ?? node.Text;
			TextRenderer.DrawText(
				graphics,
				topicTitle,
				treeNavigation.Font,
				textBounds,
				selected ? SettingsPalette.PrimaryText : SettingsPalette.SecondaryText,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);
		}

		private void DrawCategoryNode(
			Graphics graphics,
			TreeNode node,
			Rectangle rowBounds)
		{
			string categoryIndex = GetCategoryIndex(node.Name);
			Rectangle indexBounds = new(
				rowBounds.Left + 10,
				rowBounds.Top,
				24,
				rowBounds.Height);
			TextRenderer.DrawText(
				graphics,
				categoryIndex,
				lblSidebarEyebrow.Font,
				indexBounds,
				SettingsPalette.Accent,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPrefix);

			Rectangle textBounds = new(
				rowBounds.Left + 40,
				rowBounds.Top,
				Math.Max(0, rowBounds.Width - 70),
				rowBounds.Height);
			TextRenderer.DrawText(
				graphics,
				node.Text,
				lblSidebarEyebrow.Font,
				textBounds,
				SettingsPalette.PrimaryText,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);

			int centerX = rowBounds.Right - 17;
			int centerY = rowBounds.Top + (rowBounds.Height / 2);
			using Pen arrowPen = new(SettingsPalette.MutedText, 1.5F)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			};
			if (node.IsExpanded)
			{
				graphics.DrawLine(arrowPen, centerX - 4, centerY - 2, centerX, centerY + 2);
				graphics.DrawLine(arrowPen, centerX, centerY + 2, centerX + 4, centerY - 2);
			}
			else
			{
				graphics.DrawLine(arrowPen, centerX - 2, centerY - 4, centerX + 2, centerY);
				graphics.DrawLine(arrowPen, centerX + 2, centerY, centerX - 2, centerY + 4);
			}
		}

		private static GraphicsPath CreateRoundedRectangle(
			Rectangle bounds,
			int radius)
		{
			GraphicsPath path = new();
			int diameter = Math.Min(
				radius * 2,
				Math.Min(bounds.Width, bounds.Height));
			if (diameter <= 1)
			{
				path.AddRectangle(bounds);
				return path;
			}

			Rectangle arc = new(bounds.X, bounds.Y, diameter, diameter);
			path.AddArc(arc, 180, 90);
			arc.X = bounds.Right - diameter;
			path.AddArc(arc, 270, 90);
			arc.Y = bounds.Bottom - diameter;
			path.AddArc(arc, 0, 90);
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);
			path.CloseFigure();
			return path;
		}

		private void txtSearch_TextChanged(object? sender, EventArgs eventArgs)
		{
			PopulateTree(txtSearch.Text);
		}

		private void btnClearSearch_Click(object? sender, EventArgs eventArgs)
		{
			txtSearch.Clear();
			txtSearch.Focus();
		}

		private void lblAnswer_LinkClicked(
			object? sender,
			LinkClickedEventArgs eventArgs)
		{
			try
			{
				if (!Uri.TryCreate(
					eventArgs.LinkText,
					UriKind.Absolute,
					out Uri? linkUri) ||
					(linkUri.Scheme != Uri.UriSchemeHttp &&
						linkUri.Scheme != Uri.UriSchemeHttps))
				{
					throw new InvalidOperationException(
						"Only secure web links can be opened from the help center.");
				}

				Process.Start(new ProcessStartInfo
				{
					FileName = linkUri.AbsoluteUri,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					this,
					"Could not launch external web link: " + ex.Message,
					"Synix Link Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void TitleBar_MouseDown(
			object? sender,
			MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, HtCaption, 0);
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			if (Properties.Settings.Default.PrivacyMode)
			{
				_ = SetWindowDisplayAffinity(Handle, WdaExcludeFromCapture);
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{
				// Older Windows versions do not support rounded DWM corners.
			}
		}

		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				message.Msg != WmNcHitTest ||
				WindowState == FormWindowState.Maximized)
			{
				return;
			}

			Point cursor = PointToClient(Cursor.Position);
			bool left = cursor.X <= ResizeBorder;
			bool right = cursor.X >= ClientSize.Width - ResizeBorder;
			bool top = cursor.Y <= ResizeBorder;
			bool bottom = cursor.Y >= ClientSize.Height - ResizeBorder;

			if (left && top) message.Result = (IntPtr)HtTopLeft;
			else if (right && top) message.Result = (IntPtr)HtTopRight;
			else if (left && bottom) message.Result = (IntPtr)HtBottomLeft;
			else if (right && bottom) message.Result = (IntPtr)HtBottomRight;
			else if (left) message.Result = (IntPtr)HtLeft;
			else if (right) message.Result = (IntPtr)HtRight;
			else if (top) message.Result = (IntPtr)HtTop;
			else if (bottom) message.Result = (IntPtr)HtBottom;
		}

		protected override bool ProcessCmdKey(
			ref Message message,
			Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.F))
			{
				txtSearch.Focus();
				txtSearch.SelectAll();
				return true;
			}

			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		[DllImport("user32.dll")]
		private static extern uint SetWindowDisplayAffinity(
			IntPtr windowHandle,
			uint affinity);

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);

		[DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessageText(
			IntPtr windowHandle,
			int message,
			IntPtr wordParameter,
			string text);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int attributeValue,
			int attributeSize);
	}

	public class HelpItem
	{
		public string Category { get; set; }
		public string Answer { get; set; }

		public HelpItem(string category, string answer)
		{
			Category = category;
			Answer = answer;
		}
	}
}
