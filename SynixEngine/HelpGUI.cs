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
				["How to start the server?"] = "Select your server from the sidebar list, ensure the status is 'Ready', and click the green 'Start' button. The console will initialize shortly after.",
				["What is {Identity}?"] = "The {Identity} tag is a unique variable Synix uses to prevent folder conflicts. It automatically converts your server name into a file-safe ID (e.g., 'My Server' becomes 'My_Server').",
				["First Time Setup"] = "1. Install the game files via the 'SteamCMD' tab.\n2. Configure your ports in 'Server Settings'.\n3. Click Start. \n\nNote: Make sure your Windows Firewall isn't blocking the application.",

				// --- NETWORKING ---
				["Port Forwarding Guide"] = "To allow players to join, you must forward the Game Port and Query Port (usually 27015 or 7777) in your router settings. Redirect these to your local IP address.",
				["Can't find server in list?"] = "This is usually caused by the 'Query Port' being blocked. Ensure your router and Windows Firewall allow UDP traffic on the specified port.",
				["Public vs Local IP"] = "Internal players on your Wi-Fi should use your Local IP. Players over the internet must use your Public IP, which can be found at the top of the Synix Dashboard.",

				// --- TROUBLESHOOTING ---
				["Port Conflict Error"] = "This error means another program (or another instance of the server) is already using the selected port. Try changing the port number in Settings or restarting your PC.",
				["Watchdog Restarting"] = "The Synix Watchdog monitors server health. If the server stops responding for more than 60 seconds, it will automatically reboot the instance to keep it online.",
				["High CPU Usage"] = "Game servers are demanding. If usage stays at 100%, try reducing the 'Tick Rate' or 'Player Limit' in the game's configuration files.",
				["Server File Corruption"] = "If the server crashes on startup, use the 'Validate Files' button. This runs a SteamCMD check to repair missing or broken game data.",

				// --- MAINTENANCE ---
				["Automated Backups"] = "Synix creates a snapshot of your world data every 6 hours. You can change this frequency in the 'Backups' tab. Always backup before updating!",
				["How to Update"] = "Stop the server first. Go to the 'Updates' tab and click 'Check for Updates'. If an update is found, Synix will download it via SteamCMD automatically.",

				// --- SUPPORT ---
				["Donate & Support"] = "Synix Control Panel is a labor of love. Your gift keeps this project free for all and fuels the next generation of server automation. Thank you for supporting the journey!\n\nhttps://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8"
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
				if (key.Contains("How to") || key.Contains("Setup") || key.Contains("Identity"))
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