# 🛸 Synix Control Panel

### **Easy Personal Game Server Hosting for Windows**

[![Latest Release](https://img.shields.io/github/v/release/ubidzz/Synix-Control-Panel?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/releases/latest)
[![WinGet](https://img.shields.io/winget/v/ubidzz.Synix?style=for-the-badge&color=blue&label=WINGET)](https://github.com/microsoft/winget-pkgs/tree/master/manifests/u/ubidzz/Synix)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-lightgrey.svg?style=for-the-badge&logo=windows)
![Privilege](https://img.shields.io/badge/Admin-Not%20Required-blueviolet.svg?style=for-the-badge)
[![Supported Profiles](https://img.shields.io/badge/Game%20Server%20Profiles-228-00c8ff.svg?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/wiki/Game-List)
[![CodeQL](https://img.shields.io/badge/Security-CodeQL-2088FF?style=for-the-badge&logo=github)](https://github.com/ubidzz/Synix-Control-Panel/security/code-scanning)
[![License](https://img.shields.io/badge/License-Personal%20Use-red.svg?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/blob/master/LICENSE.md)
[![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate-0079C1?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8)
[![Discord](https://img.shields.io/badge/Discord-Join%20the%20Community-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/WduKEU3j8s)
[![YouTube](https://img.shields.io/badge/YouTube-Watch%20the%20Showcase-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/EcVLT4kgdb8)
[![Downloads](https://img.shields.io/github/downloads/ubidzz/Synix-Control-Panel/total?style=for-the-badge&logo=github)](https://github.com/ubidzz/Synix-Control-Panel/releases)

**Synix Control Panel makes hosting personal game servers approachable for people who do not want to learn command-line tools, write batch files, or manually manage SteamCMD.**

Choose a server profile, enter the settings you want, and let Synix handle installation, startup commands, updates, monitoring, backups, and common maintenance tasks from one Windows application.

- **No subscription is required.**
- **No paid server-instance slots are required.**
- **No permanent administrator access is required.**
- **Your servers and settings stay on your own computer.**

> Synix is built for personal, non-commercial game-server hosting. The number of servers you can run is determined by your computer, storage, network, and the requirements of each server.

## 🚀 Why Use Synix?

Running a dedicated server normally means finding the correct Steam App ID, installing SteamCMD, building startup commands, tracking ports, watching processes, and maintaining backups yourself. Synix brings those jobs together behind a straightforward desktop interface.

| Without a Server Manager | With Synix |
|---|---|
| Install and configure SteamCMD manually | SteamCMD is checked and installed automatically |
| Search for App IDs and startup arguments | Select from 228 built-in server profiles |
| Write and maintain batch files | Configure the server through normal forms and controls |
| Watch Task Manager for crashes | Use built-in process and resource monitoring |
| Remember updates, backups, and restarts | Enable automatic maintenance options per server |
| Move server files and settings by hand | Export, verify, and import your Synix installation |

## 🕹️ How Synix Works

1. **Install or open Synix.** Choose the MSI, WinGet, or standalone version.
2. **Add a server.** Select a supported profile and give the server a name.
3. **Choose your settings.** Enter ports, player limits, passwords, map options, and other available settings.
4. **Install and start.** Synix prepares the files, runs the required tools, launches the server, and begins monitoring it.

Synix keeps the management experience local. The main Synix data folder is `C:\Synix`, while custom game-server locations can be placed on another SSD, NVMe drive, or storage folder.

## 🌟 What Is New in v1.0.24

v1.0.24 expands Synix from a server launcher into a more complete local server-management workspace:

- **Server Readiness Center:** Checks required files, runtimes, configuration health, ports, Windows Firewall rules, disk space, process ownership, and recent logs.
- **Import Existing Server:** Registers a supported existing installation without moving, reinstalling, or overwriting it.
- **First-Start Assistant:** Explains the remaining setup, connection, configuration, backup, and recovery steps before the first launch.
- **Plain-English errors:** Shows what happened, what the user can do next, and optional technical details.
- **Reliable multi-process control:** Tracks launchers, consoles, child processes, replacement processes, and workers so stop and restart operations do not leave ghost processes or start duplicate servers.
- **Smart Maintenance:** Can wait for players, back up the server, stop its verified process group, update it, and restart it on selected days and times.
- **Game Support Catalog:** Search and filter all built-in profiles by name, compatibility, configuration, player details, crossplay, server program, and verification status.
- **Minecraft Java and Bedrock control:** Adds edition-aware installation, configuration, process tracking, console commands, player management, loaders, and native game modes.
- **Mod & Plugin Manager:** Discovers supported add-on folders, imports reviewed local packages, manages provider IDs, and records rollback information.

## 💾 Install Synix

### Option 1: WinGet

Open Command Prompt or PowerShell and run:

```powershell
winget install --exact --id ubidzz.Synix
```

To uninstall:

```powershell
winget uninstall --exact --id ubidzz.Synix
```

### Option 2: Windows Installer

Download **`SynixSetup.msi`** from the [latest release](https://github.com/ubidzz/Synix-Control-Panel/releases/latest).

The MSI version:

- Installs Synix for the current Windows user.
- Adds Synix to the Start Menu.
- Adds Synix to the Windows Apps list.
- Supports normal Windows installation, upgrade, and uninstall behavior.

### Option 3: Standalone

Download **`Synix.Control.Panel.exe`** from the [latest release](https://github.com/ubidzz/Synix-Control-Panel/releases/latest) and run it from any folder.

- No installation is required.
- The executable can be placed wherever you prefer.
- Synix server data remains stored separately inside `C:\Synix`.

> **.NET 10 is included with Synix.** Official Windows releases are self-contained and package the required .NET 10 runtime inside the published Synix executable, so users do not need to install .NET separately. `SynixSetup.msi` contains that same self-contained executable. Including the runtime is why the published application increased from roughly 40 MB to about 119 MB; the MSI download can remain smaller because the installed executable is compressed inside the installer.

> **Upgrading from v1.0.20 or earlier:** Older installed releases used the Inno Setup `SynixSetup.exe` installer. Uninstall that installed version first, then install `SynixSetup.msi`. This does not remove the servers and settings stored inside `C:\Synix`.

## ✨ Designed for Normal Windows Users

Synix is intended to make common server-management jobs understandable without hiding important information from the user.

- **Guided server setup:** Configure supported servers without building every command manually.
- **Central dashboard:** Install, start, stop, update, back up, and monitor servers from one place.
- **Automatic SteamCMD setup:** Synix downloads and configures SteamCMD when it is missing.
- **Clear status information:** See whether a server is stopped, starting, running, updating, or experiencing a problem.
- **Built-in help:** Learn about setup, ports, networking, backups, updates, watchdog behavior, and supported server options inside the application.
- **No artificial instance limit:** Synix does not charge for or restrict the number of server entries you create.

## 🎮 228 Built-In Server Profiles

Synix includes **228 built-in dedicated game-server profiles**, with additional profiles and corrections added over time.

Popular profiles include:

| Game | Game | Game | Game |
|---|---|---|---|
| **7 Days to Die** | **ARK: Survival Evolved** | **ARK: Survival Ascended** | **Arma 3** |
| **Arma Reforger** | **Conan Exiles** | **Core Keeper** | **DayZ** |
| **Dune: Awakening** | **Enshrouded** | **Garry's Mod** | **Icarus** |
| **Palworld** | **Project Zomboid** | **Rust** | **Satisfactory** |
| **Soulmask** | **Space Engineers** | **Squad** | **Stationeers** |
| **Terraria** | **Unturned** | **Valheim** | **V Rising** |

See the [complete supported game list](https://github.com/ubidzz/Synix-Control-Panel/wiki/Game-List).

> Dedicated servers change over time. A profile may need updated arguments after a game or server update. Synix records locally verified install, start, stop, and monitoring results to help identify what has been tested on your computer.

## ⛏️ Minecraft Java and Bedrock

Minecraft support is isolated from other game-server workflows and includes:

- Separate Java and Bedrock setup, installation, importing, ports, icons, configuration, and connection guidance.
- Automatic Java and loader selection for Vanilla, Fabric, Forge, and compatible NeoForge 1.21+ releases.
- Installation of Microsoft's official Bedrock Dedicated Server package.
- Managed `server.properties` and Bedrock configuration values for names, ports, maximum players, worlds, seeds, RCON, supported local-management settings, and native game modes.
- Native **Survival**, **Creative**, and **Adventure** choices instead of generic PVE/PVP values.
- A local Minecraft Server Console that works with Synix-managed hidden windows, supported localhost management, or optional RCON.
- Prepared commands for announcements, player lists, moderation, operators, allowlists, world time, weather, saving, help, and clean shutdown.
- Player-name management when the selected Java server exposes an authenticated local management or RCON channel.

## 🧩 Mod & Plugin Manager

Synix uses game and framework profiles instead of maintaining a database of every individual mod. Initial support includes Rust Oxide/uMod files, Minecraft mod and plugin folders, 7 Days to Die mod packages, ARK: Survival Evolved Steam Workshop IDs, and ARK: Survival Ascended provider mod IDs.

- Discovers add-ons already installed in supported folders.
- Imports supported local files and ZIP packages through guarded staging.
- Opens supported external catalogs in the user's browser; Synix does not download catalog files or require catalog API credentials for those links.
- Records SHA-256 values and rollback information for changes made by Synix.
- Blocks unsafe archive paths, symbolic links, disguised libraries, prohibited executable or script types, oversized packages, duplicate destinations, and confirmed antivirus detections.
- Treats an unavailable or inconclusive Microsoft Defender review as a warning instead of claiming that malware was found.

> Mods and plugins execute with the game server's Windows permissions. A clean scan cannot prove that third-party code is trustworthy. Install add-ons only from sources you trust, and run Synix with standard Windows permissions.

## 🧠 Server Management and Automation

### Installation and Updates

- Installs supported Steam servers through SteamCMD.
- Handles anonymous or authenticated SteamCMD installation workflows when required.
- Checks for missing SteamCMD components automatically.
- Supports `Update on Start` for servers that should be checked before launching.
- Can validate and repair supported server installations through SteamCMD.
- Keeps the Synix application updated through its dashboard update button.
- Supports standalone, MSI, and WinGet installation types.
- Verifies downloaded Synix updates before applying them and can roll back a failed update.

### Starting and Stopping

- Builds the required startup command from the selected profile and user settings.
- Tracks the server from `Starting` to `Running` instead of assuming a launched process is ready.
- Sends a normal close request first so supported servers have time to save.
- Tracks the complete verified process group and uses a staged process-tree fallback when a server cannot close normally.
- Prevents duplicate launches and common port conflicts.
- Allows a startup attempt to be cancelled without creating a restart loop.
- Keeps a private local command channel for supported Minecraft servers even when their command window is hidden.

### Monitoring and Recovery

- Displays overall and per-server CPU and memory information.
- Monitors running server processes through the Synix Watchdog.
- Can record crashes and begin configured recovery behavior.
- Uses A2S, TCP, UDP, HTTP, process, or socket checks where supported.
- Supports Discord webhook notifications for server lifecycle and maintenance events.
- Helps prevent new server launches when the host is already under heavy resource pressure.
- Can use an optional background agent at Windows sign-in for scheduled monitoring without leaving a ghost Synix process after an explicit shutdown.

## 💾 Backups, Transfers, and Password Protection

- **Backup on Start:** Create a server backup before a manual launch.
- **Custom backup locations:** Store backups on another drive or folder.
- **Scheduled maintenance:** Configure server restarts for selected days and times.
- **Encrypted Synix export:** Package `C:\Synix` into a password-protected transfer file for another PC.
- **Normal export option:** Create an unencrypted package when speed and temporary disk usage matter more than package privacy.
- **Size and free-space checks:** Review estimated package size, working-space requirements, and approximate time before transferring.
- **Package verification:** Check a transfer package before importing it.
- **Crash-safe importing:** Stage changes and automatically roll back incomplete imports when possible.
- **Protected Synix passwords:** Passwords and Discord webhooks saved by Synix are protected for the current Windows user.
- **Automatic migration:** Older plaintext Synix records and legacy server-data formats are upgraded when loaded.
- **Migration backup:** Before a server-data schema upgrade is saved, Synix preserves the original file as `servers.json.before-data-v<version>.bak`.

> Some game servers require passwords in their command line or configuration files. Synix protects its own saved values, but it cannot change how third-party game-server software stores or receives those values.

## 🌐 Networking Assistance

- Checks for duplicate ports in Synix and ports already being used by Windows processes.
- Tests local server connectivity and supported public connection paths.
- Explains common NAT hairpinning and router-loopback behavior.
- Queries player counts and server information through A2S where supported.
- Uses fallback checks for servers that do not provide a normal A2S response.
- Manages `steam_appid.txt` for profiles that require a Steam API handshake.

> Games that expose server discovery only through Epic Online Services may show **N/A** for player counts and hide unsupported Player Management or LAN/WAN connection-test actions. Synix does not ask normal users to provide EOS integration data that is not available through the normal server installation.

### Synix Network Guard

Network Guard watches for unusual bandwidth and resource conditions that may affect the host computer. It is aware of SteamCMD activity to reduce false warnings during large installations and updates.

> **Network Guard is an experimental warning system. It is not a firewall and cannot block or stop a DDoS attack.**

## 🛡️ Standard-User Design

Synix is designed to run as a normal Windows user for routine server management.

- Normal server installation and management do not require Synix to remain elevated.
- Server files stay organized under `C:\Synix` or a location selected by the user.
- Optional Windows-level actions can request permission only when Windows requires it.
- Synix does not upload your server list or passwords to a Synix-hosted cloud service.

## 🔐 Windows SmartScreen Notice

Synix is an independently developed application and does not currently have a paid code-signing certificate. Microsoft Defender SmartScreen may therefore show a warning while a new release builds reputation.

- Download Synix only from the [official GitHub releases](https://github.com/ubidzz/Synix-Control-Panel/releases).
- Confirm that the download came from the official Synix project.
- If SmartScreen provides the option and you trust the download, select **More info** and then **Run anyway**.
- Do not disable Windows security features solely to run Synix.
- Report unexpected security warnings so they can be investigated.

## 🧪 Compatibility and Community Testing

Synix includes automated tests and release-readiness checks, but one developer cannot test every supported server after every game update.

If a server fails to install, start, stop, update, or report its status correctly:

1. Open **Settings → Report a Problem** in Synix.
2. Select the action that failed.
3. Review the generated report and remove anything you do not want to share.
4. Submit it through GitHub, copy it for Discord, or open the community support links.

Reports can also be submitted through:

- [GitHub Issues](https://github.com/ubidzz/Synix-Control-Panel/issues)
- [Synix Discord](https://discord.gg/WduKEU3j8s)

Please include the correct startup arguments or a link to official server documentation when you know the solution. Never include passwords, authentication tokens, Discord webhooks, or other private information.

## 📚 Built-In Help

The Synix knowledge base is included directly in the application and covers:

- First-time setup and dashboard controls
- Server installation and configuration
- Ports and port forwarding
- Local and public connection checks
- NAT hairpinning
- Steam server queries
- Backups and updates
- Scheduled maintenance
- Discord webhooks
- Watchdog and Resource Guard behavior
- Windows Firewall and SmartScreen
- Game-specific guidance

## 💻 Technical Overview

- **Application:** C# with .NET 10 and Windows Forms
- **Architecture:** Shared, asynchronous server-management engine with a local desktop interface
- **Game definitions:** Strict embedded JSON profiles with validated capabilities and no arbitrary script, plugin, assembly, or DLL loading
- **Controller selection:** Manifest-driven built-in lifecycle, console, configuration, and player controllers
- **Installer:** WiX-based per-user MSI
- **Distribution:** GitHub Releases and WinGet
- **Publishing:** Self-contained .NET 10 Windows x64 single-file executable; the runtime is included with both the standalone application and `SynixSetup.msi`
- **Data:** Local versioned JSON persistence with sequential migrations and pre-migration backups; no SQL database required
- **Password protection:** Windows user-bound encryption with portable encrypted transfer support
- **Monitoring:** Windows process, CPU, memory, socket, and WMI integration
- **Networking:** A2S and additional UDP, TCP, HTTP, and local process checks
- **Quality checks:** Automated tests, CodeQL scanning, package hashes, and a release-readiness report

Official Synix release packages include the .NET 10 runtime and do not require users to install .NET separately. Developers building Synix from source still need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

## 🖼️ Gallery

<img width="1430" height="891" alt="image" src="https://github.com/user-attachments/assets/19313d7e-a475-4f7e-9363-653c50ffc718" />
<img width="1180" height="780" alt="image" src="https://github.com/user-attachments/assets/c75e2514-79a2-4069-9b9b-a2d247ed244c" />
<img width="1180" height="760" alt="image" src="https://github.com/user-attachments/assets/b9b0931d-0396-49c0-a64b-66242ce69a6c" />
<img width="1100" height="720" alt="image" src="https://github.com/user-attachments/assets/2d18f9fb-4c29-4b20-9c3b-4ed4485ce7cf" />
<img width="1200" height="780" alt="image" src="https://github.com/user-attachments/assets/ef720d72-d0f8-4804-8cdc-0b0297c628e2" />
<img width="1180" height="760" alt="image" src="https://github.com/user-attachments/assets/de6bf296-504b-4eac-a9fe-ef36b0c172c8" />


## ❤️ Support Synix

Synix is developed independently and made available for personal use without a subscription.

- [Join the Synix Discord](https://discord.gg/WduKEU3j8s)
- [Report a problem or request an improvement](https://github.com/ubidzz/Synix-Control-Panel/issues)
- [Donate through PayPal](https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8)
- [Watch the video showcase](https://youtu.be/EcVLT4kgdb8)

## 📄 License

Synix Control Panel is source-available for **personal, non-commercial use** under the [Synix Limited Proprietary License](https://github.com/ubidzz/Synix-Control-Panel/blob/master/LICENSE.md).

You may view the source and make private personal modifications. Redistribution, public modified releases, rebranding, and commercial use require written permission from the copyright holder.

---

**Synix Control Panel — spend less time maintaining commands and more time running your servers.**
