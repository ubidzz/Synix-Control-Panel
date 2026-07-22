# 🛸 Synix Control Panel

### **The High-Performance Backbone for Your Personal Game Servers**

[![Latest Release](https://img.shields.io/github/v/release/ubidzz/Synix-Control-Panel?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/releases/latest)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey.svg?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg?style=for-the-badge)
![Security](https://img.shields.io/badge/Privilege-Zero--UAC%20Required-blueviolet.svg?style=for-the-badge)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg?style=for-the-badge)](https://github.com/ubidzz/Synix-Control-Panel/blob/master/LICENSE.md)
[![SECURITY](https://img.shields.io/badge/SECURITY-PASSING-brightgreen?style=for-the-badge&logo=github)](https://github.com/ubidzz/Synix-Control-Panel/actions/workflows/github-code-scanning/codeql)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-1%2F71%20Clean-yellowgreen?style=for-the-badge&logo=virustotal)](https://www.virustotal.com/gui/file/c3a62c98e52bacccb57bc4e9b342feef20d2be49de4f91bfca164f7e6487d0b8?nocache=1)
[![WinGet Status](https://img.shields.io/winget/v/ubidzz.Synix?style=for-the-badge&color=blue&label=WINGET%20INSTALL)](https://github.com/microsoft/winget-pkgs/tree/master/manifests/u/ubidzz/Synix)
[![Donate with PayPal](https://img.shields.io/badge/PAYPAL-DONATE-0079C1?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=FAHU6EH6BX9J8)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ubidzz/Synix-Control-Panel/total?style=for-the-badge&logo=github)

Synix Control Panel is an elite, engine-driven management suite designed to provide a centralized "Brain" for game server hosting. By moving beyond simple batch scripts, Synix automates deployment, process health, networking diagnostics, and hardware stewardship within a streamlined Windows environment.

[Discord](https://discord.gg/2WR7ArC2Vr)
[Youtube Video](https://www.youtube.com/watch?v=EcVLT4kgdb8&t=1796s)

---

## 🏗️ Architectural Style & Design Patterns

* **Engine-Driven Singleton Architecture: Utilizes a centralized source of truth for all server operations, separating the UI client from core processing logic to eliminate race conditions and keep background backups/updates atomic.**
* **Asynchronous Event-Driven Execution: Built on the Task-based Asynchronous Pattern (TAP) for a non-blocking user experience during heavy I/O operations like SteamCMD downloads.**
* **Resource-Aware Middleware Resource-Aware Middleware: Calculates system headroom in real-time, enforcing safety buffers and CPU throttles to maintain host stability.**

---

## 💾 Quick Install (Winget)
Installing Synix Control Panel via winget handles the application installation, start menu shortcuts, and registers it to your Windows Apps list automatically. You can run these commands from Command Prompt (cmd) or PowerShell.

To install:
* **`winget install synix`**

To uninstall:
* **`winget uninstall synix`**

**(Note: Synix can also be uninstalled directly from the standard Windows Apps list settings).**

---

## 🛡️ Security & SmartScreen Notes
Because Synix is a specialized tool developed for the community, you may encounter Windows security prompts during your first launch.

> **Note on Digital Signatures:**
Because Synix is an independently developed community tool without a paid Microsoft Digital Signature, you may encounter Windows security prompts:
> 
> * **Windows SmartScreen:** Click `More Info` -> `Run Anyway`.**
> * **Smart App Control (SAC): If enabled on Windows 11, strict SAC policies may require setting evaluation mode to run independent community apps.**
> 
> **Rest Assured:** Synix is designed to respect system safety boundaries while providing robust management features.

---

## 🛡️ Synix Network Guard
A specialized security module designed to protect the host's global network interface from saturation and resource exhaustion.

* **Global Interface Monitoring:** Tracks total bandwidth across the primary network adapter, identifying surges that exceed normal gameplay thresholds.
* **Heuristic Attack Analysis:** Differentiates between legitimate player spikes and malicious floods by cross-referencing network traffic with CPU interrupt levels.
* **SteamCMD Awareness:** Intelligent logic prevents false positives during game installations or updates by monitoring active SteamCMD processes.
* **Critical Service Alerts:** Triggers a system-wide "Network Guard" alert that identifies network issues even when the user is tabbed out.

---

## 🛡️ Core Philosophy: User-Mode Sovereignty
Synix is engineered to protect both the game server and the host operating system efficiently.
* **Non-Invasive Execution:** Operates entirely within its dedicated root structure, ensuring clean file organization..
* **Sanitized Identity Isolation:** Every server is containerized using a unique identifier string to avoid collisions and path errors.
* **Portable Infrastructure:** The entire ecosystem is file-path independent. Move your root directory to any drive (SSD/NVMe), and the engine self-heals its internal pointers.

---

## 🧠 The Synix Engine: Professional Automation
### **Proactive Hardware Stewardship (Resource Guard)**
Optimized for high-performance architectures (benchmarked on Ryzen 9 / 96GB RAM environments):
* **The 5GB Safety Buffer:** Reserves strict RAM overhead for Windows kernel processes.
* **85% CPU Ingress Throttle:** Blocks new server launches if global CPU utilization exceeds safe limits.
* **Interactive Telemetry:** Real-time history tracking hardware health and diagnostics.

### **Autonomous Process Health (Watchdog)**
* Heartbeat Monitoring: Monitors process loop health. If a server becomes unresponsive for >60 seconds, the engine initiates a recovery sequence.
* Staged Termination: Sends a `Safe Close` signal for clean world-saves before enforcing a fallback process termination if necessary.

---

## 🌐 Elite Networking & Connectivity
* **Local vs. WAN Probing:** Verifies LAN IP binding and Public WAN NAT Table forwarding.
* **NAT Hairpinning Awareness:** Detects router loopback limitations and guides users to the correct connection IP.
* **A2S Telemetry:** Uses A2S_INFO protocols to query player counts and metadata without impacting server performance.
* **AppID Synchronization:** Dynamically manages steam_appid.txt to ensure correct Steam API handshakes for titles like ARK, Rust, Soulmask, and Dune: Awakening.

---

## 📂 Deployment & Maintenance Suite
* **Binary Integrity Validation:** Compares local files against the Steam Master Manifest to repair corrupted data without purging world saves.
* **Automated DLL Injection:** Automatically injects required SteamCMD libraries into target binary folders post-install.
* **Smart Backups:** "Backup on Start" zips before manual launches or Auto Restart.
* **Smart Update** "Update on Start" will update server files before manual launches or Auto Restart.
* **Discord Webhooks:** Full lifecycle notification support for Boots, Shutdowns, and Watchdog recovery events.

---

## 💻 Technical Stack
* **Framework:** C# / .NET 8.0+ / Modern WinForms
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O.

---

## 🖼️ Gallery

<img width="1243" height="660" alt="UI" src="https://github.com/user-attachments/assets/f10cc223-b1a0-4b85-a048-2989daf8ca8d" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/4598aa5f-eb95-4589-afe2-bed4dd84d78b" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/a24ad17a-8895-4711-a6e7-7390d71a7691" />
<img width="351" height="254" alt="image" src="https://github.com/user-attachments/assets/ee8b0e34-eef3-4059-8cb8-913adc087ba0" />
<img width="797" height="616" alt="image" src="https://github.com/user-attachments/assets/8939b8f1-72ac-4a80-974c-c04f3e3e0cdf" />
<img width="343" height="247" alt="image" src="https://github.com/user-attachments/assets/a0b6f395-1850-4b3b-a531-3b45c3a6dd75" />
