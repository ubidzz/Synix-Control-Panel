# 🛸 Synix Control Panel

### **The High-Performance, Zero-Admin Backbone for Your Personal Game Servers**

![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey.svg?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg?style=for-the-badge)
![Security](https://img.shields.io/badge/Privilege-Zero--UAC%20Required-blueviolet.svg?style=for-the-badge)
![License](https://img.shields.io/badge/License-Proprietary-red.svg?style=for-the-badge)
[![SECURITY](https://img.shields.io/badge/SECURITY-PASSING-brightgreen?style=for-the-badge&logo=github)](https://github.com/ubidzz/Synix-Control-Panel/actions/workflows/github-code-scanning/codeql)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-1%2F71%20Clean-yellowgreen?style=for-the-badge&logo=virustotal)](https://www.virustotal.com/gui/file/c3a62c98e52bacccb57bc4e9b342feef20d2be49de4f91bfca164f7e6487d0b8?nocache=1)

**Synix Control Panel** is an elite, engine-driven management suite designed to provide a centralized "Brain" for game server hosting. By moving beyond simple batch scripts, Synix automates deployment, process health, networking diagnostics, and hardware stewardship within a **Zero-Admin (No UAC)** environment.

---

## 🏗️ Architectural Style & Design Patterns
Synix is built with a focus on modularity, thread safety, and low-latency execution.

### **Engine-Driven Singleton Architecture**
The application utilizes a **Singleton Engine Pattern**, ensuring a single, centralized source of truth for all server operations. By separating the UI client from the core processing logic, Synix prevents race conditions and ensures that state-heavy operations—such as updates or backups—remain atomic and synchronized.

### **Asynchronous Event-Driven Execution**
Built on the **Task-based Asynchronous Pattern (TAP)**, Synix ensures a non-blocking user experience. Heavy I/O operations, including SteamCMD downloads and network telemetry, are executed on background threads. The UI remains responsive while the engine reacts to lifecycle events.

### **Resource-Aware Middleware**
Synix acts as a deterministic middleware layer between the Operating System and the Game Engine. Its **Resource Guard** logic calculates system headroom in real-time, enforcing a 5GB RAM safety buffer and an 85% CPU throttle to maintain host stability.

---

## 🛡️ Security & Installation Notes
Because Synix is a specialized tool developed for the community, you may encounter Windows security prompts during your first launch.

> **Note on Digital Signatures:**
> Synix is currently a new, independent project and does not yet have a paid Microsoft Digital Signature. This often triggers **Windows SmartScreen** or **Smart App Control**.
> 
> * **Windows SmartScreen:** Click `More Info` -> `Run Anyway`.
> * **Windows 11 Smart App Control (SAC):** If your system has Smart App Control enabled, it may block unsigned executables entirely. You may need to set Smart App Control to 'Evaluation' or 'Off' to run independent community tools like Synix.
> 
> **Rest Assured:** Synix is a **No-Admin** tool. It does not require or request UAC/Administrative privileges, meaning it cannot modify your system registry or protected Windows files.

---

## 🛡️ Synix Network Guard
A specialized security module designed to protect the host's global network interface from saturation and resource exhaustion.

* **Global Interface Monitoring:** Tracks total bandwidth (Bytes/s) across the primary network adapter, identifying surges that exceed normal gameplay thresholds.
* **Heuristic Attack Analysis:** Differentiates between legitimate player spikes and malicious floods by cross-referencing network traffic with CPU interrupt levels.
* **SteamCMD Awareness:** Intelligent logic prevents false positives during game installations or updates by monitoring active SteamCMD processes.
* **Critical Service Alerts:** Triggers a system-wide "Network Guard" MessageBox that identifies a DDoS attack even when the user is tabbed out.

---

## 🛡️ Core Philosophy: User-Mode Sovereignty
Synix is engineered to protect both the game server and the host operating system without requiring elevated privileges.
* **Non-Invasive Execution:** Operates entirely within `C:\Synix`, ensuring zero modifications to Windows registry hives or protected system directories.
* **Sanitized {Identity} Isolation:** Every server is containerized using a unique {Identity} string to avoid collisions and "Space in Path" errors.
* **Portable Infrastructure:** The entire ecosystem is file-path independent. Move your root directory to any drive (SSD/NVMe), and the engine self-heals its internal pointers.

---

## 🧠 The Synix Engine: Professional Automation
The core engine is a **Modular Singleton** that manages the server lifecycle with proactive intelligence.

### **Proactive Hardware Stewardship (Resource Guard)**
Optimized for high-performance architectures (benchmarked on **Ryzen 9 / 96GB RAM** environments), Synix protects system stability:
* **The 5GB Safety Buffer:** Synix calculates available headroom by reserving a strict 5GB RAM overhead for Windows 11 kernel processes.
* **85% CPU Ingress Throttle:** The engine blocks new server launches if global CPU utilization exceeds 80% to ensure smooth performance for active players.
* **Interactive Telemetry:** A 60-second real-time history graph tracks hardware health with deep-dive **Resource History** diagnostics.

### **Autonomous Process Health (Watchdog)**
* **Heartbeat Monitoring:** Synix monitors process loop health to detect hangs. If a process is unresponsive for >60 seconds, the engine initiates a recovery sequence.
* **Staged Termination:** Sends a 'Safe Close' signal for world-saves, triggering a `taskkill` only if the process remains stubborn.

---

## 🌐 Elite Networking & Connectivity
Synix solves the "Hidden Server" mystery with a proprietary two-tier diagnostic suite.
* **Local vs. WAN Probing:** Verifies LAN IP binding and Public WAN NAT Table forwarding.
* **NAT Hairpinning Awareness:** Detects router loopback limitations and guides users to the correct connection IP.
* **A2S Telemetry:** Uses A2S_INFO protocols to query player counts and metadata without impacting server performance.
* **AppID Synchronization:** Dynamically manages `steam_appid.txt` to ensure correct Steam API handshakes for titles like ARK, Rust, Soulmask.

---

## 📂 Deployment & Maintenance Suite
* **Binary Integrity Validation:** Compares local files against the Steam Master Manifest to repair corrupted data without purging world saves.
* **Automated DLL Injection:** Automatically injects required SteamCMD libraries into target binary folders post-install.
* **Smart Backups:** Recursive world snapshots every 6 hours and "Safe-Start" zips before manual launches.
* **Discord Webhooks:** Full lifecycle notification support for Boots, Shutdowns, and Watchdog recovery events.

---

## 💻 Technical Stack
* **Framework:** C# / .NET 8.0+ / Modern WinForms
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O.
* **Efficiency:** Ultra-low overhead design (~30MB RAM idle / <1% CPU footprint).

---

## 📜 License & Legal Information
*Copyright © 2026 ubidzz. All Rights Reserved.*

The **Synix Control Panel** is a proprietary software project. 
* **Permitted Use:** Free for personal, non-commercial use.
* **Source Code:** Provided for transparent viewing and educational purposes only.
* **Strict Restrictions:** Redistribution, public modification, or commercial resale of the code or compiled binaries is strictly prohibited.

---

Support: https://discord.gg/2WR7ArC2Vr
Youtube Video: https://youtu.be/uxLvrT548Hk

<img width="1243" height="660" alt="UI" src="https://github.com/user-attachments/assets/f10cc223-b1a0-4b85-a048-2989daf8ca8d" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/4598aa5f-eb95-4589-afe2-bed4dd84d78b" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/a24ad17a-8895-4711-a6e7-7390d71a7691" />
<img width="351" height="254" alt="image" src="https://github.com/user-attachments/assets/ee8b0e34-eef3-4059-8cb8-913adc087ba0" />
<img width="797" height="616" alt="image" src="https://github.com/user-attachments/assets/8939b8f1-72ac-4a80-974c-c04f3e3e0cdf" />
<img width="343" height="247" alt="image" src="https://github.com/user-attachments/assets/a0b6f395-1850-4b3b-a531-3b45c3a6dd75" />
