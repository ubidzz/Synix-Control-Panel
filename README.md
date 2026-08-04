# 🛸 Synix Control Panel

### **The High-Performance Backbone for Your Personal Game Servers**

[![Latest Release](https://img.shields.io/github/v/release/ubidzz/Synix-Control-Panel?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/releases/latest)
[![WinGet Status](https://img.shields.io/winget/v/ubidzz.Synix?style=for-the-badge&color=blue&label=WINGET%20INSTALL)](https://github.com/microsoft/winget-pkgs/tree/master/manifests/u/ubidzz/Synix)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey.svg?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg?style=for-the-badge)
![Security](https://img.shields.io/badge/Privilege-Standard%20User%20by%20Default-blueviolet.svg?style=for-the-badge)
![Supported Games](https://img.shields.io/badge/Supported%20Game%20Profiles-210%2B-00c8ff.svg?style=for-the-badge)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/blob/master/LICENSE.md)
[![SECURITY](https://img.shields.io/badge/SECURITY-PASSING-brightgreen?style=for-the-badge&logo=github)](https://github.com/ubidzz/Synix-Control-Panel/actions/workflows/github-code-scanning/codeql)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-View%20Scan-yellowgreen?style=for-the-badge&logo=virustotal)](https://www.virustotal.com/gui/file/c3a62c98e52bacccb57bc4e9b342feef20d2be49de4f91bfca164f7e6487d0b8?nocache=1)
[![Donate with PayPal](https://img.shields.io/badge/PAYPAL-DONATE-0079C1?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ubidzz/Synix-Control-Panel/total?style=for-the-badge&logo=github)

Synix Control Panel is an elite, engine-driven management suite designed to provide a centralized "Brain" for game server hosting. By moving beyond simple batch scripts, Synix automates deployment, process health, networking diagnostics, and hardware stewardship within a streamlined Windows environment.

Synix currently includes **210+ built-in game server profiles**, with more games and profile updates continuing to be added.

[Discord](https://discord.gg/2WR7ArC2Vr)  
[YouTube Video](https://www.youtube.com/watch?v=EcVLT4kgdb8&t=1796s)

---

## 🎮 Supported Games

Synix currently supports **210+ built-in dedicated game server profiles**, and the supported game list continues to grow.

Popular supported games include:

| Game | Game | Game | Game |
|---|---|---|---|
| **Rust** | **Soulmask** | **StarRupture** | **Dune: Awakening** |
| **7 Days to Die** | **ARK: Survival Evolved** | **ARK: Survival Ascended** | **Valheim** |
| **Palworld** | **Project Zomboid** | **V Rising** | **DayZ** |
| **Conan Exiles** | **Satisfactory** | **Space Engineers** | **Arma 3** |
| **Icarus** | **DeadPoly** | **Terraria** | **RuneScape: Dragonwilds** |
| **Team Fortress 2** | **Enshrouded** | **Core Keeper** | **Scrap Mechanic** |
| **Sons Of The Forest** | **HumanitZ** | **Stationeers** | **Barotrauma** |
| **The Forest** | **Abiotic Factor** | **Sunkenland** | **Bellwright** |
| **SCUM** | **Squad** | **Unturned** | **Garry's Mod** |
| **Starground** | **Windrose** | **Dune: Awakening** | **Survive the Nights** |
| **Desynced** | **Arma Reforger** | **Just Cause 3: Multiplayer** | **Myth of Empires** |

* **And many more**

New games, updates, fixes, and community-requested profiles are still being added.

---

## 🏗️ Architectural Style & Design Patterns

* **Engine-Driven Singleton Architecture:** Utilizes a centralized source of truth for all server operations, separating the UI client from core processing logic to reduce race conditions and keep background backups and updates atomic.
* **Asynchronous Event-Driven Execution:** Built on the Task-based Asynchronous Pattern (TAP) for a non-blocking user experience during heavy I/O operations like SteamCMD downloads.
* **Resource-Aware Middleware:** Calculates system headroom in real-time, enforcing safety buffers and CPU throttles to maintain host stability.

---

## 💾 Quick Install (WinGet)

Installing Synix Control Panel through WinGet handles the application installation, Start Menu shortcuts, and registration in the Windows Apps list automatically. You can run these commands from Command Prompt or PowerShell.

To install:

* **`winget install synix`**

To uninstall:

* **`winget uninstall synix`**

**Note: Synix can also be uninstalled directly from the standard Windows Apps list in Settings.**

---

## 🛡️ Security & SmartScreen Notes

Because Synix is a specialized tool developed for the community, you may encounter Windows security prompts during your first launch.

> **Note on Digital Signatures:**  
> Because Synix is an independently developed community tool without a paid Microsoft Digital Signature, you may encounter Windows security prompts:
>
> * **Windows SmartScreen:** Click `More Info` -> `Run Anyway`.
> * **Smart App Control (SAC):** If enabled on Windows 11, strict SAC policies may block unsigned independent community apps.
>
> **Rest Assured:** Synix is designed to respect system safety boundaries while providing robust management features.

---

## 🛡️ Synix Network Guard

A specialized monitoring module designed to warn the user about possible network saturation and resource exhaustion.

* **Global Interface Monitoring:** Tracks total bandwidth across active network adapters and identifies surges that exceed normal gameplay thresholds.
* **Heuristic Traffic Analysis:** Cross-references unusual network traffic with high CPU activity to help identify possible network-flood conditions.
* **SteamCMD Awareness:** Prevents false positives during game installations or updates by monitoring active SteamCMD processes.
* **Critical Service Alerts:** Triggers a system-wide Network Guard alert that can notify the user even while tabbed out.

> **Note:** Network Guard is an experimental warning feature. It is not a firewall and does not block or stop DDoS attacks.

---

## 🛡️ Core Philosophy: User-Mode Sovereignty

Synix is engineered to protect both the game server and the host operating system efficiently.

* **Non-Invasive Execution:** Operates inside its dedicated root structure, helping keep server files organized.
* **Sanitized Identity Isolation:** Every server uses a unique identifier string to help avoid collisions and path errors.
* **Portable Infrastructure:** Supports default and custom server paths so users can place game servers on their preferred SSD, NVMe drive, or storage location.
* **Standard User by Default:** Synix does not require permanent Administrator access for normal game server management.
* **Just-In-Time Elevation:** Optional system tasks, such as Windows Firewall cleanup, can request UAC only when needed.

---

## 🧠 The Synix Engine: Professional Automation

### **Proactive Hardware Stewardship (Resource Guard)**

Optimized for high-performance architectures and tested in Ryzen 9 / high-memory environments:

* **The 5GB Safety Buffer:** Reserves RAM overhead for Windows and other host processes.
* **85% CPU Ingress Throttle:** Blocks new server launches if global CPU utilization reaches an unsafe level.
* **Interactive Telemetry:** Real-time CPU and RAM history tracking with per-server resource details.

### **Autonomous Process Health (Watchdog)**

* **Startup Verification:** A server remains in `Starting` until Synix confirms that the server is responding.
* **Heartbeat Monitoring:** Monitors process health after the server reaches the `Running` state.
* **Crash Recovery:** If a running server crashes or becomes unresponsive, Synix can log the failure, send an alert, and begin recovery.
* **Staged Termination:** Sends a safe close signal for clean world saves before using forced process termination as a fallback.
* **Startup Cancellation:** Closing the server console while it is still starting cancels the startup instead of triggering a restart loop.

---

## 🌐 Elite Networking & Connectivity

* **Local vs. WAN Probing:** Tests local server connectivity and public WAN reachability.
* **NAT Hairpinning Awareness:** Helps explain router loopback limitations and guides users toward the correct connection IP.
* **A2S Telemetry:** Uses A2S_INFO where supported to query player counts and server metadata.
* **TCP Fallback Checks:** Uses additional connection checks when a normal A2S response is not available.
* **Port Collision Protection:** Checks for duplicate ports inside Synix and ports already in use by another Windows process.
* **AppID Synchronization:** Manages `steam_appid.txt` where required for Steam API handshakes for games such as ARK, Rust, Soulmask, and Dune: Awakening.

---

## 📂 Deployment & Maintenance Suite

* **SteamCMD Installation:** Automatically installs and configures SteamCMD when needed.
* **Binary Integrity Validation:** Compares local files against Steam manifests to repair corrupted or missing files without intentionally deleting world saves.
* **Automated DLL Injection:** Copies required SteamCMD libraries into supported game binary folders when they are missing.
* **Smart Backups:** `Backup on Start` creates a ZIP backup before manual launches.
* **Smart Updates:** `Update on Start` checks for game server updates before launch.
* **Scheduled Restarts:** Allows maintenance restarts on selected days and times.
* **Custom Backup Locations:** Allows backups to be stored on another drive or folder.
* **Discord Webhooks:** Supports lifecycle notifications for startup, shutdown, scheduled maintenance, and watchdog recovery.
* **Batch File Export:** Exports the current server configuration into a standalone `.bat` launcher.

---

## 📚 Built-In Synix Knowledge Base

Synix includes a searchable help and command knowledge base directly inside the application.

The built-in guide includes information about:

* First-time setup
* Dashboard controls
* Server configuration
* Port forwarding
* Local and public connection testing
* NAT hairpinning
* Steam server query rules
* Backups and updates
* Discord webhooks
* Watchdog behavior
* Resource Guard
* Windows Firewall
* SmartScreen
* Game-specific setup instructions

The help content is built directly into Synix, so users do not need to download extra help files or search the internet for basic instructions.

---

## 💻 Technical Stack

* **Framework:** C# / .NET 8.0+ / Windows Forms
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O
* **Dependencies:** Uses built-in .NET and WinForms functionality wherever possible
* **Publishing:** Single-file ReadyToRun publishing
* **Installer:** Inno Setup installer generated automatically during publishing
* **Distribution:** GitHub Releases and WinGet

---

## 🖼️ Gallery

<img width="1243" height="660" alt="UI" src="https://github.com/user-attachments/assets/f10cc223-b1a0-4b85-a048-2989daf8ca8d" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/4598aa5f-eb95-4589-afe2-bed4dd84d78b" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/a24ad17a-8895-4711-a6e7-7390d71a7691" />
<img width="351" height="254" alt="image" src="https://github.com/user-attachments/assets/ee8b0e34-eef3-4059-8cb8-913adc087ba0" />
<img width="797" height="616" alt="image" src="https://github.com/user-attachments/assets/8939b8f1-72ac-4a80-974c-c04f3e3e0cdf" />
<img width="343" height="247" alt="image" src="https://github.com/user-attachments/assets/a0b6f395-1850-4b3b-a531-3b45c3a6dd75" />
