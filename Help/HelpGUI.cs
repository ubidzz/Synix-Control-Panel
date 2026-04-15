/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class HelpGUI : Form
	{
		// This is our data list
		private Dictionary<string, string> _helpData;

		public HelpGUI()
		{
			InitializeComponent();
			InitializeData();
			PopulateTree();

			// Set default view
			lblTopicTitle.Text = "Synix Support Center";
			lblAnswer.Text = "Please select a topic on the left to learn more about hosting with Synix.";
		}

		private void InitializeData()
		{
			_helpData = new Dictionary<string, string>
			{
				// --- 1. GETTING STARTED ---
				["Initial Repository Setup"] = "1. Deployment: Synix installs SteamCMD to C:\\Synix\\SteamCMD.\n2. Library: Use the 'SteamCMD' tab to download game binaries.\n3. Configuration: Use 'Add New Server' to define your unique {Identity}.\n4. Execution: Select your server and click 'Start Server'.",
				["Understanding {Identity}"] = "Synix generates a sanitized, unique {Identity} for every server. This prevents folder path conflicts and ensures multiple instances of the same game don't interfere with each other.",
				["Where are my world saves?"] = "All data is isolated in C:\\Synix\\Games\\{Identity}. Your world saves and logs are contained within this unique folder for total portability.",

				// --- 2. DASHBOARD & CONTROLS ---
				["Main Controls (Start/Stop)"] = "• Start: Initializes the engine boot sequence.\n• Stop: Triggers a staged shutdown. Synix sends a 'Safe Close' signal, waits for the save cycle, and automatically executes a 'taskkill' if the process remains open.\n• Kill: Instantly terminates the Process ID (PID). Use this only if the engine is completely unresponsive.\n• Restart: Performs a staged Stop followed by an automatic Start.",
				["Maintenance Suite"] = "• Update: Synchronizes your files with the Steam Master Manifest.\n• Validate: Repairs binaries without deleting world progress.\n• Backups: Opens the snapshot manager to restore previous world states.",
				["Connection Link Status"] = "• Local Link: Probes your Local LAN IP. If [ONLINE], your process and Windows Firewall are healthy.\n• WAN Link: Probes your Public IP. If [HIDDEN], check your router's Port Forwarding (UDP).",
				["Total Resource Graph"] = "Tracks your CPU and RAM health. Click the graph to open the 'Resource History' window for a detailed view of CPU and RAM performance of each of your running game servers.",

				// --- 3. SERVER CONFIGURATION (ADD/EDIT) ---
				["Display Name & Game Templates"] = "The 'Display Name' is your organizational label. Selecting a 'Game' applies the correct engine templates for ports and startup arguments.",
				["The Port Trio (+App Port)"] = "• Game Port (UDP): Core gameplay data.\n• Query Port (UDP): Server browser metadata (Use 27016 to avoid Steam conflicts).\n• RCON Port (TCP): Remote admin console.\n• App Port (TCP): External tool access (e.g., Rust+). \n\nThe Rust+ app only use ports over 10000.",
				["Access Control & Map Selection"] = "• Server Password: Required for players to join.\n• Admin Password: Required for console commands.\n• Map/Mode: Defined in the startup string to set the game ruleset.",
				["AppID & Binary Name"] = "Synix auto-fills the Steam AppID. The Binary Name (e.g., WS-Win64-Shipping) tells the Synix Watchdog exactly which process to monitor.",

				// --- 4. NETWORKING & IP ---
				["Public vs Local IP"] = "Local IP (192.168.x.x) is for internal testing. Public IP is for the internet. Both are displayed at the top of your Synix Dashboard.",
				["Port Forwarding Guide"] = "You must 'Forward' your Query and Game ports (UDP) in your router settings to your Local IP so the world can find your server.",
				["NAT Loopback / Hairpinning"] = "If your WAN test is [ONLINE] but you can't join, your router blocks internal WAN loops. Use your Local IP or '127.0.0.1' or your LAN IP to join your own machine.",

				// --- 5. AUTOMATION & DISCORD ---
				["Watchdog Recovery"] = "The Watchdog monitors the server 'Heartbeat.' If the process crashes or hangs for 60 seconds, Synix forces a reboot to minimize downtime.",
				["Discord Webhook Setup"] = "1. Create a Webhook in Discord.\n2. Paste it into 'Edit Server' in Synix.\n3. Synix will now broadcast status updates for Boots, Shutdowns, and Watchdog restarts.",
				["Automated Backup Logic"] = "Synix takes snapshots every 6 hours. If enabled, a backup is also created before every manual startup (skipped during crash-recovery).",

				// --- 6. TROUBLESHOOTING & SYSTEM ---
				["No-Admin Philosophy"] = "Synix runs without Admin/UAC prompts to protect your system. This means it cannot auto-open ports; you must manually allow game binaries in Windows Firewall.",
				["Missing .DLL Errors"] = "Symptom: 'System Error' on launch. Fix: Install 'C++ Redistributables x64 (2015-2022)' from the Microsoft website.",
				["Resource Guard Limits"] = "Synix reserves 7GB of RAM for Windows 11 and blocks new launches at 80% CPU usage to prevent system instability.",
				["Where are My Server Backups"] = "If the auto backup feature is enabled, backups are stored in C:\\Synix\\BackupGames\\Game_Name\\Your_Server_Name.",
				// --- 7. SUPPORTED GAMES ---
				["How to add a game"] = "Synix utilizes a hardcoded database to ensure 100% engine stability. Manual 'plugin' support for custom games is not supported.",
				["No Minecraft?"] = "Minecraft uses a Java-based architecture that installs differently than SteamCMD games. It is not currently supported in the Synix engine.",

				// --- 8. DONATIONS ---
				["Donate & Support"] = "Synix is a labor of love for the community. \n\nSubmit bugs on Github: https://github.com/ubidzz/Synix-Control-Panel/issues \nDiscord Support: https://discord.gg/2WR7ArC2Vr \nUpcoming Features: https://discord.gg/ZKTcpgmXNM \nRequest Game: https://discord.gg/DxUXPtyVm9 \n\nYour support keeps the project free and the updates frequent!\n\nPayPal: https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8"
			};
		}

		private void PopulateTree(string filter = "")
		{
			treeNavigation.Nodes.Clear();

			TreeNode root = new TreeNode("Synix Documentation");
			TreeNode nodeStart = new TreeNode("1. Getting Started");
			TreeNode nodeDash = new TreeNode("2. Dashboard & Controls");
			TreeNode nodeConfig = new TreeNode("3. Server Configuration");
			TreeNode nodeNet = new TreeNode("4. Networking & IP");
			TreeNode nodeMaint = new TreeNode("5. Maintenance & Discord");
			TreeNode nodeTrouble = new TreeNode("6. Troubleshooting & System");
			TreeNode nodeGames = new TreeNode("7. Supported Games");
			TreeNode nodeSupport = new TreeNode("8. Support & Donations");

			nodeSupport.ForeColor = Color.DarkGreen;

			foreach (var key in _helpData.Keys)
			{
				if (!string.IsNullOrEmpty(filter) && !key.ToLower().Contains(filter.ToLower()))
					continue;

				// 🎯 Logic with the new Graph entry
				if (key.Contains("Setup") || key.Contains("Identity") || key.Contains("saves") || key.Contains("Structure"))
					nodeStart.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Controls") || key.Contains("Link") || key.Contains("Graph"))
					nodeDash.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Name") || key.Contains("Trio") || key.Contains("Password") || key.Contains("AppID") || key.Contains("Map"))
					nodeConfig.Nodes.Add(new TreeNode(key));
				else if (key.Contains("IP") || key.Contains("Forwarding") || key.Contains("Loopback"))
					nodeNet.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Backup") || key.Contains("Update") || key.Contains("Discord") || key.Contains("Validation") || key.Contains("Suite"))
					nodeMaint.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Watchdog") || key.Contains("Guard") || key.Contains("Philosophy") || key.Contains("Dependencies") || key.Contains("Use"))
					nodeTrouble.Nodes.Add(new TreeNode(key));
				else if (key.Contains("game") || key.Contains("Minecraft"))
					nodeGames.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Donate"))
					nodeSupport.Nodes.Add(new TreeNode(key));
			}

			// Only add folders that actually have items
			if (nodeStart.Nodes.Count > 0) root.Nodes.Add(nodeStart);
			if (nodeDash.Nodes.Count > 0) root.Nodes.Add(nodeDash);
			if (nodeConfig.Nodes.Count > 0) root.Nodes.Add(nodeConfig);
			if (nodeNet.Nodes.Count > 0) root.Nodes.Add(nodeNet);
			if (nodeMaint.Nodes.Count > 0) root.Nodes.Add(nodeMaint);
			if (nodeTrouble.Nodes.Count > 0) root.Nodes.Add(nodeTrouble);
			if (nodeGames.Nodes.Count > 0) root.Nodes.Add(nodeGames);
			if (nodeSupport.Nodes.Count > 0) root.Nodes.Add(nodeSupport);

			treeNavigation.Nodes.Add(root);
			treeNavigation.ExpandAll();
		}

		private void treeNavigation_AfterSelect(object sender, TreeViewEventArgs e)
		{
			// Reset defaults for normal topics
			pbQRCode.Visible = false;
			lblAnswer.Dock = DockStyle.Fill;

			if (_helpData.TryGetValue(e.Node.Text, out string answer))
			{
				lblTopicTitle.Text = e.Node.Text;
				lblAnswer.Text = answer;

				if (e.Node.Text == "Donate & Support")
				{
					// 🚀 MAKE IT BIGGER HERE
					lblAnswer.Dock = DockStyle.Top;
					lblAnswer.Height = 220;
					pbQRCode.Margin = new Padding(0, 0, 0, 60);

					pbQRCode.Visible = true;
					pbQRCode.BringToFront();
				}
			}
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			PopulateTree(txtSearch.Text);
		}
		private void lblAnswer_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				// This is the 'Pro' way to open a browser in modern C# (.NET Core/5/6/7/8/2026)
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = e.LinkText,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not open the link: " + ex.Message);
			}
		}
	}
}