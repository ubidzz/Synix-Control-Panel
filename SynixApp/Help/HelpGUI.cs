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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class HelpGUI : Form
	{
		private Dictionary<string, HelpItem> _helpData;

		public HelpGUI()
		{
			// Note: InitializeComponent() and all UI controls (lblTopicTitle, lblAnswer, treeNavigation, txtSearch, pbQRCode) 
			// are fully managed inside HelpGUI.Designer.cs. Do NOT re-declare them here to prevent CS0121 / CS0229 errors.
			InitializeComponent();
			InitializeData();
			PopulateTree();

			// Set initial heading text safely
			if (lblTopicTitle != null)
				lblTopicTitle.Text = "🛸 Synix Support & Command Knowledge Base";

			if (lblAnswer != null)
				lblAnswer.Text = "Welcome to the Synix Engine Knowledge Base.\n\nPlease select a topic from the navigation tree on the left or use the search bar above to query specific setup guides, networking protocols, automation rules, custom directory settings, or game launcher details.";
		}

		private void InitializeData()
		{
			_helpData = new Dictionary<string, HelpItem>
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
					"• Backup Repository: `C:\\Synix\\BackupGames\\[Game_Name]\\{Server_Name}` (Automated ZIP archives created during pre-flight maintenance)."),

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
					"3. Stores the ZIP archive in `C:\\Synix\\BackupGames\\[Game_Name]\\{Server_Name}\\` with a timestamp suffix.\n" +
					"4. Automated crash recoveries bypass backups to ensure rapid reboot times."),

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
					"Synix runs entirely in User-Mode without requesting Administrator (UAC) privileges. It will not edit your Windows registry or modify host system settings.\n\n" +
					"FIREWALL REQUIREMENTS:\n" +
					"Because Synix runs without Admin privileges, Windows Defender Firewall may prompt you the first time a game server binary executes. Always check 'Allow on Private and Public Networks' when prompted by Windows."),

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
					"All automated ZIP backups created by Synix are stored at:\n\n" +
					"  C:\\Synix\\BackupGames\\[Game_Name]\\{Server_Name}\\\n\n" +
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

				// --- 9. COMMUNITY & LEGAL (Category: "Support") ---
				["Official Support Links"] = new HelpItem("Support",
					"COMMUNITY & SUPPORT RESOURCES:\n\n" +
					"• GitHub Issues: https://github.com/ubidzz/Synix-Control-Panel/issues \n" +
					"• Official Discord: https://discord.gg/2WR7ArC2Vr \n" +
					"• Feature Requests: https://discord.gg/ZKTcpgmXNM \n" +
					"• Game Support Requests: https://discord.gg/DxUXPtyVm9 \n" +
					"• YouTube Video Showcase: https://www.youtube.com/watch?v=EcVLT4kgdb8&t=1796s"),

				["License & Proprietary Terms"] = new HelpItem("Support",
					"SYNIX CONTROL PANEL - LIMITED PROPRIETARY LICENSE\n\n" +
					"Copyright (c) 2026 Jason Turner. All Rights Reserved.\n\n" +
					"1. GRANT OF LICENSE\n" +
					"Permission is hereby granted to any individual to download, view, and modify the source code of the Synix Control Panel for PERSONAL, NON-COMMERCIAL USE ONLY.\n\n" +
					"2. MODIFICATIONS\n" +
					"You are permitted to modify the source code to suit your personal requirements. However, you are STRICTLY PROHIBITED from publishing, sharing, or distributing these modifications (forks) to the public without express written consent from the original author.\n\n" +
					"3. PROHIBITIONS & RESTRICTIONS\n" +
					"• NO COMMERCIAL USE: You may not sell, lease, rent, or profit from this software, its source code, or any compiled binaries.\n" +
					"• NO REBRANDING: You may not remove the \"Synix\" name, logos, or author credits. You may not represent this software as your own creation.\n" +
					"• NO REDISTRIBUTION: You may not host this software or its source code on other repositories, websites, or file-sharing platforms for public download.\n" +
					"• NO DECOMPILATION: You may not attempt to reverse engineer or decompile the official binaries for the purpose of removing licensing checks or branding.\n\n" +
					"4. OWNERSHIP & TRADEMARK\n" +
					"\"Synix\" and its associated code are the intellectual property of Jason Turner. This license does not transfer ownership. Any unauthorized use of the Synix brand for commercial purposes is a violation of trademark and copyright law.\n\n" +
					"5. TERMINATION & LEGAL ACTION\n" +
					"Any violation of these terms automatically terminates your right to use or possess this software. The author reserves the right to take legal action, including DMCA takedown notices and civil litigation, against any party found to be in violation of this license.\n\n" +
					"6. DISCLAIMER\n" +
					"THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER LIABILITY."),

				["Donate & Support Development"] = new HelpItem("Support",
					"SUPPORT THE PROJECT:\n\n" +
					"Synix Control Panel is developed with passion for the server hosting community. Your support keeps updates frequent and features growing!\n\n" +
					"PayPal Donation: https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8")
			};
		}

		private void PopulateTree(string filter = "")
		{
			if (treeNavigation == null) return;

			treeNavigation.Nodes.Clear();
			TreeNode root = new TreeNode("🛸 Synix Knowledge Base");

			var categories = new Dictionary<string, TreeNode>
			{
				["Start"] = new TreeNode("1. Getting Started & Setup"),
				["Dash"] = new TreeNode("2. Dashboard & Controls"),
				["Config"] = new TreeNode("3. Server Configuration"),
				["Net"] = new TreeNode("4. Networking & IP Rules"),
				["Maint"] = new TreeNode("5. Maintenance & Discord"),
				["Watch"] = new TreeNode("6. Watchdog & Safeguards"),
				["Trouble"] = new TreeNode("7. Troubleshooting & System"),
				["Games"] = new TreeNode("8. Game Engines & Custom Rules"),
				["Support"] = new TreeNode("9. Support, License & Donate")
			};

			foreach (var entry in _helpData)
			{
				if (!string.IsNullOrEmpty(filter) && !entry.Key.ToLower().Contains(filter.ToLower()))
					continue;

				if (categories.ContainsKey(entry.Value.Category))
				{
					categories[entry.Value.Category].Nodes.Add(new TreeNode(entry.Key));
				}
			}

			foreach (var node in categories.Values)
			{
				if (node.Nodes.Count > 0) root.Nodes.Add(node);
			}

			treeNavigation.Nodes.Add(root);
			treeNavigation.ExpandAll();
		}

		private void treeNavigation_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node == null) return;

			if (pbQRCode != null) pbQRCode.Visible = false;
			if (lblAnswer != null) lblAnswer.Dock = DockStyle.Fill;

			if (_helpData.TryGetValue(e.Node.Text, out HelpItem item))
			{
				if (lblTopicTitle != null) lblTopicTitle.Text = e.Node.Text;
				if (lblAnswer != null) lblAnswer.Text = item.Answer;

				if (e.Node.Text == "Donate & Support Development" || e.Node.Text == "Donate")
				{
					if (lblAnswer != null)
					{
						lblAnswer.Dock = DockStyle.Top;
						lblAnswer.Height = 220;
					}
					if (pbQRCode != null)
					{
						pbQRCode.Visible = true;
						pbQRCode.BringToFront();
					}
				}
			}
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			if (txtSearch != null)
			{
				PopulateTree(txtSearch.Text);
			}
		}

		private void lblAnswer_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = e.LinkText,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not launch external web link: " + ex.Message, "Synix Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
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