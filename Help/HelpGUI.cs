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
				// --- GETTING STARTED ---
				// MERGED: Both "First Time Setup" entries combined so you don't lose steps or crash
				["First Time Setup"] = "1. Synix installs SteamCMD to C:\\Synix\\SteamCMD.\n2. Download game files via the 'SteamCMD' tab into C:\\Synix\\Games.\n3. Set your ports in 'Server Settings'.\n4. Click Start.\n\nNote: If the window closes immediately, check 'Troubleshooting > Missing .DLL Errors'.",

				["How to start the server?"] = "Select your server from the sidebar, ensure status is 'Ready', and click 'Start'. Synix will initialize the process and the console window will appear shortly.",

				["What is {Identity}?"] = "Synix uses {Identity} to create a file-safe ID for your server (e.g., 'My Server' becomes 'My_Server'). This prevents folder conflicts and 'Space in Path' errors that crash many engines.",

				["Where are my world saves?"] = "By default, all game data is stored in C:\\Synix\\Games\\{Identity}. Look for a 'Saves' folder inside. Note: Custom locations (like a G: drive) stay where you picked them.",

				// --- NETWORKING & CONNECTIVITY ---
				["Port Forwarding Guide"] = "To let others join, you must 'drill a hole' in your router for the Game Port and Query Port. Redirect these to your Local IP. \n\nTip: Use 'UDP' for Game/Query ports and 'TCP' for RCON.",
				["The 'Golden Trio' of Ports"] = "Most servers need three ports open:\n1. Game Port (UDP)\n2. Query Port (UDP)\n3. RCON Port (TCP).",
				["Public vs Local IP"] = "Internal players use your Local IP. External players use your Public IP. Both are displayed at the top of the Synix Dashboard.",
				["NAT Loopback / Can't join myself?"] = "CAUSE: Many ISP routers don't allow you to connect to your own Public IP.\n\nFIX: Use '127.0.0.1' or your Local IP in the game's 'Direct Connect' box.",

				// --- TROUBLESHOOTING ---
				["Friends can't see my server?"] = "SYMPTOM: Server is running but invisible.\n\nFIX: Ensure 'Query Port' is forwarded as UDP and check Windows Firewall.",
				["'Address Already in Use' Error?"] = "SYMPTOM: Port is 'taken'.\n\nFIX: Use the 'Kill All Processes' button in Synix or change your port by 1.",
				["Missing .DLL Errors"] = "SYMPTOM: 'System Error' on launch.\n\nFIX: Install 'C++ Redistributables x64' (2015-2022) from Microsoft's site.",
				["SteamCMD stuck at 0%?"] = "SYMPTOM: No download progress.\n\nFIX: Run Synix as Admin and check for 20GB+ free disk space.",
				["Watchdog Restarting Server"] = "If the server hangs for 60s, the Synix Watchdog will force a reboot to keep the server Running.",
				["Server File Corruption"] = "Use 'Validate Files' to trigger a SteamCMD repair without deleting your world saves.",

				// --- MAINTENANCE ---
				["Automated Backups"] = "Synix snapshots world data every 6 hours by default. Adjust this in the 'Backups' tab.",
				["How to Update"] = "1. Stop the server.\n2. Go to 'Updates' tab.\n3. Click 'Check for Updates' to pull the latest version via SteamCMD.",

				// --- SUPPORT ---
				["Donate & Support"] = "Synix is a labor of love to make hosting accessible. Your support keeps the project free!\n\nPayPal: https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8"
			};
		}

		private void PopulateTree(string filter = "")
		{
			treeNavigation.Nodes.Clear();

			// Create the Main Folders
			TreeNode root = new TreeNode("Synix Documentation");
			TreeNode nodeStart = new TreeNode("Getting Started");
			TreeNode nodeNet = new TreeNode("Networking & IP");
			TreeNode nodeTrouble = new TreeNode("Troubleshooting");
			TreeNode nodeMaint = new TreeNode("Maintenance");
			TreeNode nodeSupport = new TreeNode("Support & Donations");

			// Style the support folder so it stands out
			nodeSupport.ForeColor = Color.DarkGreen;

			foreach (var key in _helpData.Keys)
			{
				// Apply search filter if user is typing
				if (!string.IsNullOrEmpty(filter) && !key.ToLower().Contains(filter.ToLower()))
					continue;

				// Sorting Logic based on keywords
				if (key.Contains("How to") || key.Contains("Setup") || key.Contains("Identity") || key.Contains("saves"))
					nodeStart.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Port") || key.Contains("IP") || key.Contains("list"))
					nodeNet.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Error") || key.Contains("Watchdog") || key.Contains("CPU") || key.Contains("Corruption"))
					nodeTrouble.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Backup") || key.Contains("Update"))
					nodeMaint.Nodes.Add(new TreeNode(key));
				else if (key.Contains("Donate"))
					nodeSupport.Nodes.Add(new TreeNode(key));
			}

			// Only add folders to the tree if they have items inside (cleaner for search)
			if (nodeStart.Nodes.Count > 0) root.Nodes.Add(nodeStart);
			if (nodeNet.Nodes.Count > 0) root.Nodes.Add(nodeNet);
			if (nodeMaint.Nodes.Count > 0) root.Nodes.Add(nodeMaint);
			if (nodeTrouble.Nodes.Count > 0) root.Nodes.Add(nodeTrouble);
			if (nodeSupport.Nodes.Count > 0) root.Nodes.Add(nodeSupport);

			treeNavigation.Nodes.Add(root);
			treeNavigation.ExpandAll(); // Keep it expanded so it's easy to read
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