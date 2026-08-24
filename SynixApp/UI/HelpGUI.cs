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

				["First-Time Setup Guide"] = new HelpItem("Start",
					"WELCOME TO SYNIX CONTROL PANEL!\n\n" +
					"Synix manages local dedicated game servers from one Windows desktop dashboard. Most day-to-day actions do not require administrator access.\n\n" +
					"CREATE YOUR FIRST SERVER:\n" +
					"1. Wait for the footer to report that SteamCMD is ready. Synix installs its shared SteamCMD files automatically when needed.\n" +
					"2. Click '+ Add Server' on the Server Dashboard.\n" +
					"3. General: enter a unique Server Name, choose the Game Server template, and select a version when the template provides one.\n" +
					"4. World Generation: choose the map, game mode, player limit, RAM, and other settings that are available for that game. Disabled controls are intentionally unsupported by the selected template.\n" +
					"5. Network & RCON: review the service ports. Every server instance must use ports that do not conflict with another Synix server or running process.\n" +
					"6. Automation: choose backup, update, restart schedule, and Discord notification options.\n" +
					"7. Install & Launch: keep the default folder or choose a custom empty folder, review launch options, and click 'Save Server'.\n" +
					"8. Select the new row on the dashboard and click 'Start'. The first installation can take several minutes; follow Activity & Diagnostics for progress.\n\n" +
					"The bottom status message in Server Setup names the exact missing field or port conflict. Save Server unlocks only after those requirements are resolved."),

				["Server Setup Page Guide"] = new HelpItem("Start",
					"USING THE FIVE SERVER SETUP PAGES:\n\n" +
					"• General: server identity, game template, version, map/profile choices, passwords, and Minecraft runtime choices when applicable.\n" +
					"• World Generation: world name, seed, size, and other template-supported world options.\n" +
					"• Network & RCON: game, query, app, and RCON ports. Synix checks active processes and saved servers for conflicts.\n" +
					"• Automation: backup on start, update on start, restart scheduling, and Discord alerts.\n" +
					"• Install & Launch: server folder, default-path selection, extra launch arguments, and final save.\n\n" +
					"TEMPLATE-AWARE CONTROLS:\n" +
					"Synix enables only the controls supported by the selected game. A disabled field is not an error and should not be forced into that game's launch command. The configuration status in the lower-left area and the detailed footer message update while you work."),

				["After Saving: Install, Start, and Verify"] = new HelpItem("Start",
					"WHAT HAPPENS AFTER YOU CLICK SAVE SERVER:\n\n" +
					"1. Synix stores the server definition and adds it to the Game Servers list.\n" +
					"2. Select the server row and click Start. If files are missing, Synix runs the correct installer before launch.\n" +
					"3. Watch Activity & Diagnostics. Download, validation, launch arguments, process binding, and probe results appear there.\n" +
					"4. Wait for the status to change from Starting to Running. Some games create their configuration files only after the first complete boot.\n" +
					"5. Stop the server cleanly before editing generated files. Use Configure or Server Options -> Open Config Editor when that template exposes a config file.\n" +
					"6. Start the server again and test joining through the LAN address before troubleshooting public access.\n\n" +
					"Do not close an installer or server console while Synix reports Starting, Updating, Backing Up, or Stopping."),

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

				["Main Dashboard Operations"] = new HelpItem("Dash",
					"USING THE SERVER DASHBOARD:\n\n" +
					"1. Select a server row. The action bar at the bottom changes to that server and unlocks its controls.\n" +
					"2. Start runs pre-flight checks, optional backup/update work, installation when required, and then launches the server.\n" +
					"3. Restart performs a verified stop before launching again. It does not intentionally start a second copy.\n" +
					"4. Stop requests a clean shutdown and keeps the status in Stopping until the tracked PID and server process have exited. A forced process-tree shutdown is used only when the game does not stop normally.\n" +
					"5. Configure reopens Server Setup for the selected server. Save only after resolving the exact validation message shown in the footer.\n" +
					"6. Server Options opens folder, configuration, update, backup, connection-test, batch-export, and delete actions that are supported by the selected game.\n\n" +
					"Use the search box and status filter above the grid to find a server. Activity & Diagnostics records each operation and is the first place to check when a status does not change as expected."),

				["Server Options Menu"] = new HelpItem("Dash",
					"ACTIONS AVAILABLE FROM SERVER OPTIONS:\n\n" +
					"• Open Server Folder: opens the server's active installation directory.\n" +
					"• Open Backup Folder: opens the backup repository for that server.\n" +
					"• Open Config Editor: opens the configured game file in Synix's format-aware editor.\n" +
					"• Update Server: runs the supported game update workflow.\n" +
					"• Validate Game Files: asks the supported installer to verify/repair game files.\n" +
					"• Create Batch File: exports the resolved launch command for supported templates.\n" +
					"• Backup Server: creates a manual archive.\n" +
					"• Test LAN/WAN Connectivity: appears only for games with a reliable supported probe.\n" +
					"• Delete Server: removes the Synix server entry and, after confirmation, can remove associated files/backups.\n\n" +
					"Some actions are intentionally hidden or disabled for templates that use a different installer, generate settings only after first boot, or cannot be tested reliably."),

				["Server Details & Double-Click Inspector"] = new HelpItem("Dash",
					"INSPECTING A SERVER:\n\n" +
					"Double-click a server row to open Server Info. This read-only view collects the selected server's identity, ports, credentials state, automation choices, paths, schedule, arguments, and current status in one window.\n\n" +
					"LIVE METERS:\n" +
					"When the server is running, the CPU and RAM cards update from its bound process. A stopped server reports zero usage. Closing Server Info disposes its timer and process resources; it does not stop the game server."),

				["Live Resource Telemetry Graph"] = new HelpItem("Dash",
					"RESOURCE TELEMETRY:\n\n" +
					"The CPU Usage and RAM Usage cards at the top of the dashboard show total host usage. These numbers describe the computer, not only one game server.\n\n" +
					"RESOURCE MONITOR:\n" +
					"Open Resource Monitor to see each bound running server in a sortable list with Server Name, PID, executable, CPU usage, and RAM usage. Use Server Info when you need the detailed view for one server. Closing either monitor window does not stop a server."),

				["Global Settings Menu & Privacy Mode"] = new HelpItem("Dash",
					"SYNIX GLOBAL SETTINGS:\n\n" +
					"Access the global settings menu via the gear icon in the top right corner of the main dashboard.\n\n" +
					"• General: choose whether native server console windows are shown and switch between Dark Mode and Light Mode. The theme is saved and reapplied across Synix windows.\n" +
					"• Backups: enable a custom backup location and set the maximum archives retained per server from 1 to 100. Changing the location does not move or delete older archives.\n" +
					"• Privacy & Security: Privacy Mode hides IP addresses, passwords, and other sensitive values while screen sharing. DDoS Attack Detection is marked experimental and should be treated as an alerting aid, not a replacement for router or hosting-provider protection.\n" +
					"• Advanced: Elevated System Tasks requests administrator permission only for approved operations such as firewall cleanup or Network Guard actions. Normal server management remains a standard-user operation."),

				["Dark Mode, Light Mode, and Console Windows"] = new HelpItem("Dash",
					"DISPLAY AND CONSOLE SETTINGS:\n\n" +
					"Open Settings -> General.\n\n" +
					"• Dark Mode ON: uses the original navy Synix theme.\n" +
					"• Dark Mode OFF: switches supported forms and controls to the light card-based theme.\n" +
					"• Show Server Console Window ON: opens the game's native command window when a server starts. This is useful for live console interaction and troubleshooting.\n" +
					"• Show Server Console Window OFF: runs supported servers silently in the background. Games that require an interactive manager, including Space Engineers, keep that required window visible.\n\n" +
					"Changing the visual theme does not restart game servers. If an already-open secondary window does not repaint immediately, close and reopen that window."),

				["Adding and Editing Servers"] = new HelpItem("Config",
					"NEW SERVER VS EXISTING SERVER:\n\n" +
					"• Add Server opens Server Setup in NEW SERVER mode. Choosing a game template fills its supported defaults and determines which controls are available.\n" +
					"• Configure opens the selected entry in EDIT SERVER mode. Existing values are loaded so you can adjust the server without creating a duplicate.\n" +
					"• The exact validation message at the bottom identifies missing fields, invalid values, paths, or port conflicts. Save Server remains locked until the configuration is valid.\n" +
					"• The Folder Path is read-only. Change it with Browse Folder or the default-folder option so Synix can validate and normalize the selected path.\n" +
					"• Renaming a server changes its Synix identity. Review its folder and game-specific save locations carefully before saving an existing installation.\n\n" +
					"Stop a running server before changing ports, folders, versions, or launch arguments."),

				["The Port Trio & App Port Architecture"] = new HelpItem("Config",
					"UNDERSTANDING SERVICE PORTS:\n\n" +
					"• Game Port: carries player/game traffic for most dedicated servers. The protocol is game-specific and is not always UDP.\n" +
					"• Query Port: used by a server browser, A2S query, REST service, or other status protocol when the game supports one. Synix stores the value independently from launch arguments. If a template does not use a {query} argument, the exported launch command remains clean.\n" +
					"• RCON Port: remote console administration endpoint for games that support RCON. Enable it only with a strong unique password.\n" +
					"• App Port: an additional API or companion-service endpoint required by specific games.\n\n" +
					"When the Game Port changes, Synix can preserve the template's default game-to-query offset and update Query Port automatically. Every enabled port must be unique for simultaneously running servers. Disabled port controls are not used by that template."),

				["Using the Format-Aware Config Editor"] = new HelpItem("Config",
					"SAFE CONFIGURATION FILE EDITING:\n\n" +
					"1. Stop the server so the game cannot overwrite the file while you edit it.\n" +
					"2. Select the server and choose Server Options -> Open Config Editor.\n" +
					"3. Structured View shows a clean setting name, detected type, and editable value. Hover the setting cell to see its complete nested path.\n" +
					"4. Boolean values use a True/False dropdown. Text and number values remain normal editable cells.\n" +
					"5. Use the search field and type filter to find a setting. Raw Preview helps you inspect the original file.\n" +
					"6. Click Save Changes, then restart the server.\n\n" +
					"FORMAT PROTECTION:\n" +
					"Synix uses a lexical span patcher for supported INI, XML, JSON, and space-delimited configurations. It replaces only the value spans you changed, preserving the original comments, section headers, key order, line endings, whitespace, quotes, and surrounding structure. It does not rewrite the entire file with a generic serializer."),

				["Config Editor Safety and Recovery"] = new HelpItem("Config",
					"BEFORE SAVING A GAME CONFIG:\n\n" +
					"• Create a manual backup before large changes.\n" +
					"• Do not change the raw file externally while the Config Editor is open.\n" +
					"• Keep the setting's expected type. A Boolean should remain True/False and a number should remain a valid number for that game.\n" +
					"• Palworld's large OptionSettings value is displayed as individual rows but is packed back into the single-line structure expected by the engine.\n" +
					"• If a game rejects a value, stop it, restore the previous backup or correct the value, and start it again.\n\n" +
					"The editor protects file formatting, but it cannot guarantee that every value is valid for every game version. Refer to the game's official server documentation for valid ranges and names."),

				["Local Link vs WAN Link Diagnostic"] = new HelpItem("Net",
					"LAN AND WAN CONNECTION TESTING:\n\n" +
					"Select a server, open Server Options, and use Test LAN Connectivity or Test WAN Connectivity when those actions are available.\n\n" +
					"• LAN test targets the computer's local network address. Use it first to confirm the server is running and reachable inside your home network.\n" +
					"• WAN test targets the public address. It helps diagnose router forwarding, host firewall, and protocol-specific reachability from outside the local network.\n" +
					"• The probe type is selected by the game template. Synix may use an A2S UDP query, TCP connection, REST/HTTP request, or another supported health check.\n" +
					"• Query Port is saved even when a game calculates it internally or does not place it in the launch command.\n\n" +
					"IMPORTANT: The test actions are hidden for games that do not expose a dependable compatible query or health endpoint. Hidden buttons mean 'unsupported test', not 'server offline'. Verify those games from their client/server browser and the Activity & Diagnostics process logs."),

				["Port Forwarding Master Guide"] = new HelpItem("Net",
					"PORT FORWARDING INSTRUCTIONS:\n\n" +
					"To make your server accessible on the internet, forward the required ports on your home router:\n\n" +
					"1. Determine your LAN IP (e.g., `192.168.1.50`) from the bottom status bar in Synix.\n" +
					"2. Open your router's admin panel (typically `192.168.1.1` or `192.168.0.1`).\n" +
					"3. Check the selected game's official dedicated-server documentation for every required port and whether each uses UDP, TCP, or both. Do not assume every game follows the same protocol map.\n" +
					"4. Create router rules that forward those external ports to the same ports on this computer's LAN IP.\n" +
					"5. Allow the game server executable through Windows Defender Firewall when Windows prompts you.\n" +
					"6. Reserve this computer's LAN IP in the router so it does not change later.\n" +
					"7. Save the router settings and run the WAN test when the selected game supports it. Otherwise ask someone outside your home network to join.\n\n" +
					"Do not expose RCON or a web administration port unless you need it. Use a strong password, forward only required ports, and never place the Synix data folder on a public file share."),

				["NAT Hairpinning & Joining Your Own Server"] = new HelpItem("Net",
					"ROUTER LOOPBACK / NAT HAIRPINNING:\n\n" +
					"Symptom: Your friends can join via your Public IP, but you get 'Connection Timeout' when trying to use your own Public IP.\n\n" +
					"CAUSE: Most home routers lack 'NAT Loopback/Hairpinning' support and block internal devices from routing back into their own public WAN interface.\n\n" +
					"SOLUTION: Connect to your server using your local LAN IP (e.g., `192.168.x.x`) or loopback address (`127.0.0.1`). External players must continue to use your Public IP."),

				["Steam Master Server Query Rules"] = new HelpItem("Net",
					"SERVER BROWSER AND QUERY RULES:\n\n" +
					"Many Steam-based games use A2S_INFO on a Query Port, but this is not universal. Other titles use EOS, REST/HTTP, direct TCP, a game-specific browser, or no reliable public query endpoint.\n\n" +
					"IMPORTANT RULES:\n" +
					"• Give simultaneously running instances unique enabled ports.\n" +
					"• Keep the template's default game/query offset unless the game's documentation says otherwise.\n" +
					"• A successful local process binding does not prove router forwarding is correct.\n" +
					"• A public server browser can take time to index a new server.\n" +
					"• EOS-based listing is not the same as a public EOS Web API that can be queried without game-specific credentials. Synix hides manual tests when it cannot make a dependable test."),

				["Synix Network Guard"] = new HelpItem("Net",
					"SYNIX NETWORK GUARD MODULE:\n\n" +
					"Network Guard continuously monitors global bandwidth across your network adapter.\n\n" +
					"FEATURES:\n" +
					"• Surge Monitoring: Detects extreme packet/bandwidth floods that exceed regular gameplay.\n" +
					"• False-Positive Suppression: Automatically suppresses network flood warnings when active SteamCMD downloads or game updates are detected.\n" +
					"• Alert Overlay: Displays visual desktop notifications if network saturation occurs while you are tabbed out."),

				["Smart Backup on Start"] = new HelpItem("Maint",
					"AUTOMATED WORLD BACKUPS:\n\n" +
					"Enabling 'Backup on Start' in server settings triggers an automated archival process before the game boots.\n\n" +
					"BEHAVIOR:\n" +
					"1. Synix targets the server's save data directory (default or custom path).\n" +
					"2. Compresses world state into a ZIP file.\n" +
					"3. Stores the ZIP archive in your designated Backup Repository with a timestamp suffix.\n" +
					"4. Rolling Limit Enforcement: Synix checks the 'Max Saved Backups Limit' in your Global Settings and automatically deletes the oldest archive if the limit is exceeded.\n" +
					"5. Automated crash recoveries bypass backups to ensure rapid reboot times."),

				["Manual Backups and Restore Workflow"] = new HelpItem("Maint",
					"CREATE A MANUAL BACKUP:\n\n" +
					"1. Stop the server and wait until its status is Stopped for the most consistent world archive.\n" +
					"2. Select it, open Server Options, and choose Backup Server.\n" +
					"3. Wait for Activity & Diagnostics to confirm completion. Do not start or delete the server while the backup state is active.\n" +
					"4. Use Open Backup Folder to locate the timestamped ZIP archive.\n\n" +
					"RESTORE A BACKUP:\n" +
					"1. Stop the server and make a separate copy of the current server folder.\n" +
					"2. Extract the selected archive to the correct active server/save location, preserving its folder structure.\n" +
					"3. Start the server and verify the world before removing the safety copy.\n\n" +
					"A custom backup path changes where new archives are written; Synix does not move or delete archives left in the previous location."),

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

				["Autonomous Watchdog Loop"] = new HelpItem("Watch",
					"AUTONOMOUS WATCHDOG HEALTH MONITORING:\n\n" +
					"Synix tracks each managed server's process identity, PID, exit events, and supported health signals in the background.\n\n" +
					"• Normal Stop: an intentional stop suppresses crash recovery and does not immediately restart the server.\n" +
					"• Restart or Scheduled Restart: Synix first requests a clean shutdown, verifies the old process has exited, and then launches the replacement.\n" +
					"• Unexpected Exit: when automatic recovery is enabled, the watchdog records the failure context and can restart the server.\n" +
					"• Unsupported Probe: process tracking remains authoritative when a game has no dependable network probe.\n\n" +
					"Review Activity & Diagnostics before repeatedly clicking Start or Restart. If a process survives a stop attempt, Synix should keep the live PID/status instead of falsely reporting Stopped."),

				["Resource Guard (RAM Buffer & CPU Throttling)"] = new HelpItem("Watch",
					"PROACTIVE HARDWARE STEWARDSHIP:\n\n" +
					"Resource Guard prevents game server hosting from crashing your Windows host operating system.\n\n" +
					"SAFETY POLICIES:\n" +
					"• 5GB RAM Overhead: Synix calculates system memory headroom and enforces a strict 5GB RAM reserve for Windows kernel tasks.\n" +
					"• 85% CPU Ingress Throttle: New server boot sequences are blocked if host CPU utilization exceeds 85% to maintain game server tick rates."),

				["Process Rebinding on Application Restart"] = new HelpItem("Watch",
					"APPLICATION REBINDING LOGIC:\n\n" +
					"If Synix is closed or updated while game servers or SteamCMD are actively running, reopening Synix initiates process rebinding.\n\n" +
					"The engine queries active OS processes, matches them to saved server identities and executable information, and restores live monitoring without intentionally interrupting active players.\n\n" +
					"After reopening Synix, watch Activity & Diagnostics for rebind success or a specific rebind error. Do not click Start on an instance that is visibly still running until the rebind check finishes."),

				["No-Admin Philosophy & Windows Firewall"] = new HelpItem("Trouble",
					"USER-MODE SOVEREIGNTY (NO-ADMIN):\n\n" +
					"Synix runs entirely in User-Mode without requesting Administrator (UAC) privileges by default. It will not edit your Windows registry or modify host system settings.\n\n" +
					"SMART FIREWALL CLEANUP (OPTIONAL):\n" +
					"If you enable Settings -> Advanced -> Elevated System Tasks, Synix may request Just-In-Time administrator permission only for an approved action such as removing orphaned Windows Firewall rules. If you decline the Windows UAC prompt, Synix skips the elevated task and continues normal user-mode operations.\n\n" +
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

				["Reading Activity, Logs, and Crash Reports"] = new HelpItem("Trouble",
					"WHERE TO START TROUBLESHOOTING:\n\n" +
					"1. Activity & Diagnostics: read the most recent timestamped entries around the failed action. Look for INSTALL, ARGUMENT, PROBE, SHUTDOWN, WATCHDOG, REBIND, or ERROR context.\n" +
					"2. Native server console: enable Settings -> General -> Show Server Console Window when you need the game's own startup or shutdown messages.\n" +
					"3. Server folder: use Server Options -> Open Server Folder and inspect the game's logs/configuration files.\n" +
					"4. Synix logs: fatal crash messages name the dated log file under the Synix data Logs directory. Attach that log when requesting support.\n\n" +
					"Before sharing a screenshot or log, enable Privacy Mode and still review the text for passwords, webhook URLs, public IPs, RCON secrets, or tokens. Synix scrubs known launch-argument credentials, but a game may print its own sensitive data."),

				["Server Stuck on Starting, Stopping, or Offline"] = new HelpItem("Trouble",
					"STATUS TROUBLESHOOTING CHECKLIST:\n\n" +
					"STARTING:\n" +
					"• Allow time for first installation, updates, world generation, and game-specific initialization.\n" +
					"• Check that the executable remains running and read the server console for an EULA, missing runtime, bad argument, or port-binding error.\n" +
					"• For games without a supported network probe, Synix relies on process health rather than pretending an unsupported query succeeded.\n\n" +
					"STOPPING:\n" +
					"• Wait for the game to save. Minecraft uses its native `stop` command and can take time for all dimensions to finish saving.\n" +
					"• If the console asks 'Terminate batch job (Y/N)?', the legacy wrapper is holding the window open. Use the current Synix-generated launcher and let Synix verify the Java process tree has exited.\n\n" +
					"OFFLINE OR FAILED PROBE:\n" +
					"• Confirm the configured ports match the game's own generated config, then check Windows Firewall and router rules. A running PID and a reachable public endpoint are separate checks."),

				["Why Connection Test Actions May Be Hidden"] = new HelpItem("Trouble",
					"HIDDEN LAN/WAN TESTS ARE INTENTIONAL:\n\n" +
					"Synix only shows manual connection tests for game templates with a dependable supported test method. Some servers do not answer A2S, expose only an internal/EOS listing flow, auto-calculate ports, or require game-specific authentication. Showing a generic UDP/TCP result for those games would create false failures or false success.\n\n" +
					"When the actions are hidden, verify the server with:\n" +
					"• Activity & Diagnostics process/binding messages.\n" +
					"• The game's native server browser or direct-connect feature.\n" +
					"• A second device on the LAN, followed by a player outside your home network for WAN testing.\n" +
					"• The game's official port and hosting documentation."),

				["Internal IReadOnlyList<GameInfo> Database"] = new HelpItem("Games",
					"INTERNAL GAME DATABASE ARCHITECTURE:\n\n" +
					"Synix does not use SQLite or external SQL databases. Instead, it relies entirely on a high-performance, compiled internal `IReadOnlyList<GameInfo>` database.\n\n" +
					"WHY THIS DESIGN:\n" +
					"• Absolute Thread Safety: Compiled directly into the engine, guaranteeing instant lookups with zero database corruption risks.\n" +
					"• Strict Parameter Enforced: Because game executables and Steam AppIDs require strict arguments, manual custom game plugins or third-party database tables are not supported."),

				["Dune: Awakening Setup & Definition Rules"] = new HelpItem("Games",
					"DUNE: AWAKENING DEFINITION-DRIVEN INTEGRATION:\n\n" +
					"Dune's built-in game definition declares its hardware requirements, elevated launcher, external deployment lifecycle, and launch-file export restriction. The shared Synix engine reads those fields instead of relying on game-name checks.\n\n" +
					"1. Admin Execution: Synix launches the official battlegroup script with the required permission.\n" +
					"2. Deployment Tracking: Dune owns the Hyper-V virtual machines, so Synix does not mistake the launcher process for the complete server lifecycle.\n" +
					"3. Launch Export: Synix does not replace the official deployment script with a generated launch file."),

				["Rust & Rust+ Mobile App Integration"] = new HelpItem("Games",
					"RUST SERVER & RUST+ MOBILE CONFIGURATION:\n\n" +
					"• Steam AppID: 258550\n" +
					"• App Port (Rust+): Set to a unique port above 10000 (e.g., 28082 TCP).\n" +
					"• Identity Isolation: Synix enforces `+server.identity \"{Server_Name}\"` automatically to isolate world save files and blueprints cleanly.\n\n" +
					"RUST SERVER FRAMEWORK:\n" +
					"• Vanilla: Uses the official Steam server files.\n" +
					"• Oxide: Synix downloads and verifies only the latest official Oxide.Rust runtime.\n" +
					"• Plugins: Synix never installs or manages plugins. Add your own files to oxide\\plugins.\n" +
					"• Updates: Synix reapplies Oxide after Steam updates and validations. Switching back to Vanilla requires one Update or Validate to restore the official files."),

				["Minecraft Vanilla, Fabric, and Forge"] = new HelpItem("Games",
					"MINECRAFT SERVER AUTOMATION:\n\n" +
					"Choose Minecraft in Server Setup, then select the Minecraft game version. The Minecraft Runtime card lets you choose Vanilla, Fabric, or Forge.\n\n" +
					"• Version discovery: Synix reads Mojang's version manifest to populate supported Minecraft releases.\n" +
					"• Loader discovery: Fabric or Forge loader versions are filtered for the selected Minecraft version.\n" +
					"• Portable Java: Synix determines the Java major required by the selected game version and can download a private Eclipse Temurin runtime, avoiding changes to the computer's system Java.\n" +
					"• Vanilla: downloads and launches the official server JAR.\n" +
					"• Fabric: installs the selected Fabric loader and prepares the executable server launcher. Install Fabric API and gameplay mods yourself when the modpack requires them.\n" +
					"• Forge: runs the Forge server installer and launches the generated modern or legacy Forge server structure.\n" +
					"• EULA: you must accept Mojang's EULA before the server can complete startup.\n" +
					"• Mods: Synix creates/uses the server structure, but users remain responsible for installing compatible mods and matching client-side requirements.\n\n" +
					"Minecraft's service port is normally configured in `server.properties`, so template port controls may be locked instead of injected into launch arguments. For multiple Minecraft servers, give each installation a unique `server-port` in its own `server.properties` file."),

				["Minecraft Setup and First Launch"] = new HelpItem("Games",
					"CREATE A MINECRAFT SERVER:\n\n" +
					"1. Click Add Server and choose Minecraft.\n" +
					"2. Select the Minecraft version, then choose Vanilla, Fabric, or Forge and a compatible loader version when required.\n" +
					"3. Review the Portable Java version selected by Synix and choose RAM appropriate for the world and mod count.\n" +
					"4. Save the server, select it on the dashboard, and click Start. Keep Synix open while Java, the server JAR, and loader files are downloaded/installed.\n" +
					"5. Accept the Minecraft EULA when prompted.\n" +
					"6. Wait until the console reports Done and Synix changes the server to Running.\n" +
					"7. Stop the server with Synix before adding mods or changing `server.properties`.\n" +
					"8. For Fabric, install Fabric API when required. For Forge/Fabric, use mods built for the exact Minecraft and loader versions you selected.\n\n" +
					"To shut down safely, use Synix Stop. It sends Minecraft's native `stop` command, waits for world saves and the Java process to exit, and updates the status only after shutdown is verified."),

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
