using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Synix_Control_Panel.SynixEngine
{
	// ✅ MUST BE FIRST: The Designer class must be the first class in the file
	public partial class HelpGUI : Form
	{
		private Dictionary<string, HelpItem> _helpData;

		public HelpGUI()
		{
			InitializeComponent();
			InitializeData();
			PopulateTree();

			lblTopicTitle.Text = "Synix Support Center";
			lblAnswer.Text = "Please select a topic on the left to learn more about hosting with Synix.";
		}

		private void InitializeData()
		{
			_helpData = new Dictionary<string, HelpItem>
			{
				// --- 1. GETTING STARTED (Category: "Start") ---
				["Initial Repository Setup"] = new HelpItem("Start", "1. Deployment: Synix installs SteamCMD to C:\\Synix\\SteamCMD.\n2. Library: Use the 'SteamCMD' tab to download game binaries.\n3. Configuration: Use 'Add New Server' to define your unique {Identity}.\n4. Execution: Select your server and click 'Start Server'."),
				["Understanding {Identity}"] = new HelpItem("Start", "Synix generates a sanitized, unique {Identity} for every server. This prevents folder path conflicts and ensures multiple instances of the same game don't interfere with each other."),
				["Where are my world saves?"] = new HelpItem("Start", "All data is isolated in C:\\Synix\\Games\\[Game_Name]\\{Server_Name}. Your world saves and logs are contained within this unique folder for total portability."),

				// --- 2. DASHBOARD & CONTROLS (Category: "Dash") ---
				["Main Controls (Start/Stop)"] = new HelpItem("Dash", "• Start: Initializes the engine boot sequence.\n• Stop: Triggers a staged shutdown. Synix sends a 'Safe Close' signal, waits for the save cycle, and automatically executes a 'taskkill' if the process remains open.\n• Restart: Performs a staged Stop followed by an automatic Start."),
				["Maintenance Suite"] = new HelpItem("Dash", "• Update: Synchronizes your files with the Steam Master Manifest.\n• Validate: Repairs binaries without deleting world progress."),
				["Connection Link Status"] = new HelpItem("Dash", "• Local Link: Probes your Local LAN IP. If [ONLINE], your process and Windows Firewall are healthy.\n• WAN Link: Probes your Public IP. If [HIDDEN], check your router's Port Forwarding (UDP)."),
				["Total Resource Graph"] = new HelpItem("Dash", "Tracks your CPU and RAM health. Click the graph to open the 'Resource Monitor' window for a detailed view of the PID, CPU, RAM and Executable performance of each of your running game servers."),
				["How to see server information"] = new HelpItem("Dash", "Double click on the server row to open the 'Server Info' window."),

				// --- 3. SERVER CONFIGURATION (Category: "Config") ---
				["Display Name & Game Templates"] = new HelpItem("Config", "The 'Display Name' is your organizational label. Selecting a 'Game' applies the correct engine templates for ports and startup arguments."),
				["The Port Trio (+App Port)"] = new HelpItem("Config", "• Game Port (UDP): Core gameplay data.\n• Query Port (UDP): Server browser metadata (Use 27016 to avoid Steam conflicts).\n• RCON Port (TCP): Remote admin console.\n• App Port (TCP): External tool access (e.g., Rust+). \n\nThe Rust+ app only use ports over 10000."),
				["Access Control & Map Selection"] = new HelpItem("Config", "• Server Password: Required for players to join.\n• Admin Password: Required for console commands.\n• Map/Mode: Defined in the startup string to set the game ruleset."),
				["AppID & Binary Name"] = new HelpItem("Config", "Synix auto-fills the Steam AppID. The Binary Name (e.g., WS-Win64-Shipping) tells the Synix Watchdog exactly which process to monitor."),

				// --- 4. NETWORKING & IP (Category: "Net") ---
				["Public vs Local IP"] = new HelpItem("Net", "Local IP (192.168.x.x) is for internal testing. Public IP is for the internet. Both are displayed at the bottom of your Synix Dashboard."),
				["Port Forwarding Guide"] = new HelpItem("Net", "You must 'Port Forward' your Query, Game ports (UDP) and App port (TCP) in your router settings to your Local IP so the world can find your server."),
				["NAT Loopback / Hairpinning"] = new HelpItem("Net", "If your WAN test is [ONLINE] but you can't join, your router blocks internal WAN loops. Use your Local IP or '127.0.0.1' or your LAN IP to join your own machine."),

				// --- 5. AUTOMATION & DISCORD (Category: "Maint") ---
				["Watchdog Recovery"] = new HelpItem("Maint", "The Watchdog monitors the server 'Heartbeat.' If the process crashes or hangs, Synix forces a reboot to minimize downtime."),
				["Discord Webhook Setup"] = new HelpItem("Maint", "1. Create a Webhook in Discord.\n2. Paste it into 'New/Edit Server' in Synix.\n3. Synix will now broadcast status updates for Boots, Shutdowns, and Watchdog crash and restarts."),
				["Automated Backup Logic"] = new HelpItem("Maint", "Synix will create a backup zip file if the `Backup on Start` is turned on in the server setting. (skipped during crash-recovery)."),

				// --- 6. TROUBLESHOOTING & SYSTEM (Category: "Trouble") ---
				["No-Admin Philosophy"] = new HelpItem("Trouble", "Synix runs without Admin/UAC prompts to protect your system. This means it cannot auto-open ports; you must manually allow game binaries in Windows Firewall."),
				["Missing .DLL Errors"] = new HelpItem("Trouble", "Symptom: 'System Error' on launch. Fix: Install 'C++ Redistributables x64 (2015-2022)' from the Microsoft website. \n\n https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-supported-redistributable-version"),
				["Resource Guard Limits"] = new HelpItem("Trouble", "Synix reserves 7GB of RAM for Windows and blocks new launches at 80% CPU/RAM usage to prevent system instability."),
				["Where are My Server Backups"] = new HelpItem("Trouble", "If the backup on start feature is enabled, backups are stored in \n`C:\\Synix\\BackupGames\\Game_Name\\Your_Server_Name`"),
				["Server not showing up in the game"] = new HelpItem("Trouble", "Games online server list use steam master server lists that server are registered on. Some games query ports you have to use a strict query port number to or try changing the query port number to get your server added to the steam master lists. \n\nAfter doing these steps try the WAN connection test to see if the server is listed in the game."),
				["How to update Synix"] = new HelpItem("Trouble", "Go to GitHub and download the latest Synix version. \n\n1. Close Synix. \n2. Download: https://github.com/ubidzz/Synix-Control-Panel/releases \n3. Open the new Synix exe file"),
				
				// --- 7. SUPPORTED GAMES (Category: "Games") ---
				["How to add a game"] = new HelpItem("Games", "Synix utilizes a hardcoded database to ensure 100% engine stability. Manual 'plugin' support for custom games is not supported."),
				["No Minecraft?"] = new HelpItem("Games", "Minecraft uses a Java-based architecture that installs differently than SteamCMD games. It is not currently supported in the Synix engine."),

				// --- 8. SUPPORT (Category: "Support") ---
				["Support"] = new HelpItem("Support", "• Submit bugs on Github: https://github.com/ubidzz/Synix-Control-Panel/issues \n• Submit bug on Discord: https://discord.gg/RPRTR63GXe \n• Discord Support: https://discord.gg/2WR7ArC2Vr \n• Upcoming Features: https://discord.gg/ZKTcpgmXNM \n• Request Game: https://discord.gg/DxUXPtyVm9"),
				["Donate"] = new HelpItem("Support", "Synix is a labor of love for the community. Your support keeps the project free and the updates frequent!\n\nPayPal: https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8")
			};
		}

		private void PopulateTree(string filter = "")
		{
			treeNavigation.Nodes.Clear();
			TreeNode root = new TreeNode("Synix Documentation");

			var categories = new Dictionary<string, TreeNode>
			{
				["Start"] = new TreeNode("1. Getting Started"),
				["Dash"] = new TreeNode("2. Dashboard & Controls"),
				["Config"] = new TreeNode("3. Server Configuration"),
				["Net"] = new TreeNode("4. Networking & IP"),
				["Maint"] = new TreeNode("5. Maintenance & Discord"),
				["Trouble"] = new TreeNode("6. Troubleshooting & System"),
				["Games"] = new TreeNode("7. Supported Games"),
				["Support"] = new TreeNode("8. Support"),
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
			pbQRCode.Visible = false;
			lblAnswer.Dock = DockStyle.Fill;

			if (_helpData.TryGetValue(e.Node.Text, out HelpItem item))
			{
				lblTopicTitle.Text = e.Node.Text;
				lblAnswer.Text = item.Answer;

				if (e.Node.Text == "Donate")
				{
					lblAnswer.Dock = DockStyle.Top;
					lblAnswer.Height = 220;
					pbQRCode.Visible = true;
					pbQRCode.BringToFront();
				}
			}
		}

		private void txtSearch_TextChanged(object sender, EventArgs e) { PopulateTree(txtSearch.Text); }

		private void lblAnswer_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = e.LinkText, UseShellExecute = true });
			}
			catch (Exception ex) { MessageBox.Show("Could not open link: " + ex.Message); }
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