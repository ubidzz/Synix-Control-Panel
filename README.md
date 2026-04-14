# 🛸 Synix Control Panel

### **The High-Performance, Zero-Admin Backbone for Enterprise-Grade Game Hosting**

![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey.svg?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg?style=for-the-badge)
![Security](https://img.shields.io/badge/Privilege-Zero--UAC%20Required-blueviolet.svg?style=for-the-badge)
![License](https://img.shields.io/badge/License-Proprietary-red.svg?style=for-the-badge)

**Synix Control Panel** is an elite, engine-driven management suite designed to provide a centralized "Brain" for game server hosting. By moving beyond simple batch scripts, Synix automates deployment, process health, networking diagnostics, and hardware stewardship within a **Zero-Admin (No UAC)** environment.

---

## 🛡️ Core Philosophy: User-Mode Sovereignty
Synix is engineered to protect both the game server and the host operating system without requiring elevated privileges.
* **Non-Invasive Execution:** Operates entirely within `C:\Synix`, ensuring zero modifications to Windows registry hives or protected system directories.
* **Sanitized {Identity} Isolation:** Every server is containerized using a unique {Identity} string. This prevents "Space in Path" errors, avoids folder collisions, and allows for infinite instances of the same game title on a single machine.
* **Portable Infrastructure:** The entire ecosystem is file-path independent. Move your root directory to any drive (SSD/NVMe), and the engine self-heals its internal pointers.

---

## 🧠 The Synix Engine: Professional Automation
The core engine is a **Modular Singleton** that manages the server lifecycle with proactive intelligence.

### **Proactive Hardware Stewardship (Resource Guard)**
Optimized for high-performance architectures (benchmarked on **Ryzen 9 / 96GB RAM** environments), Synix protects system stability:
* **The 7GB Safety Buffer:** Synix calculates available headroom by reserving a strict 7GB RAM overhead for Windows 11 kernel processes, preventing "Out of Memory" OS crashes.
* **80% CPU Ingress Throttle:** To maintain zero-lag performance for active players, the engine blocks new server launches if global CPU utilization exceeds 80%.
* **Interactive Telemetry:** A 60-second real-time history graph tracks hardware health. Click the graph to access deep-dive **Resource History** diagnostics.

### **Autonomous Process Health (Watchdog)**
* **Heartbeat Monitoring:** Synix monitors the process message loop to detect "Not Responding" states. If a hang is detected for >60 seconds, the engine initiates a recovery sequence.
* **Staged Termination:** The "Stop" command executes a high-integrity shutdown. It sends a 'Safe Close' signal to the engine, allows time for a world-save, and automatically triggers a `taskkill` only if the process remains stubborn.
* **Atomic Action Locking:** Prevents "State Corruption" by locking the server during critical operations (Updates, Validations, or Saves).

---

## 🌐 Elite Networking & Connectivity
Synix solves the "Hidden Server" mystery with a proprietary two-tier diagnostic suite.
* **Local vs. WAN Probing:** Natively checks if the server is binding to the LAN IP and clearing the Windows Firewall, then probes the Public WAN IP to verify router NAT Table forwarding.
* **NAT Hairpinning Awareness:** Detects if the router blocks internal WAN loops, automatically guiding the user to utilize the correct 127.0.0.1 or LAN IP for local joining.
* **A2S Telemetry:** Uses the industry-standard A2S_INFO protocol to query player counts and metadata without impacting server performance.
* **AppID Synchronization:** Dynamically manages `steam_appid.txt` to resolve the common "Invisible Server" conflict in titles like Soulmask, ensuring the Steam API handshakes with the correct Dedicated Server ID.

---

## 📂 Deployment & Maintenance Suite
* **Binary Integrity Validation:** A native tool that compares local files against the Steam Master Manifest, repairing corrupted game data without purging world saves or configurations.
* **Automated DLL Injection:** Detects Unreal and Source engine titles post-install, automatically injecting required SteamCMD libraries (`steamclient64.dll`, etc.) into the target binary folders.
* **Smart Backups:** Recursive world snapshots occur every 6 hours. "Safe-Start" backups generate a full zip before manual launches, with an automated skip-logic for crash-recovery states to prevent backing up corrupted data.
* **Discord Webhooks:** Full lifecycle notification support. Receive live JSON payloads in your Discord channels for Boots, Shutdowns, and Watchdog recovery events.

---

## 💻 Technical Stack
* **Framework:** C# / .NET 8.0+ / Modern WinForms
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O and multi-threaded logging.
* **Network Logic:** UDP-based A2S querying and HttpClient WAN detection.
* **Efficiency:** Ultra-low overhead design (~30MB RAM idle / <1% CPU footprint).

---

## 📜 License & Legal Information
*Copyright © 2026 ubidzz. All Rights Reserved.*

The **Synix Control Panel** is a proprietary software project. 
* **Permitted Use:** Free for personal, non-commercial use.
* **Source Code:** Provided for transparent viewing and educational purposes only.
* **Strict Restrictions:** Redistribution, public modification, or commercial resale of the code or compiled binaries is strictly prohibited.

---

### **Status: Revolutionizing the Personal Host Experience**
*Synix handles the hardware stewardship and networking complexity, so you can focus on the game.*

<img width="1243" height="660" alt="UI" src="https://github.com/user-attachments/assets/f10cc223-b1a0-4b85-a048-2989daf8ca8d" />
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/4598aa5f-eb95-4589-afe2-bed4dd84d78b" />
<img width="797" height="616" alt="image" src="https://github.com/user-attachments/assets/8939b8f1-72ac-4a80-974c-c04f3e3e0cdf" />
<img width="343" height="247" alt="image" src="https://github.com/user-attachments/assets/a0b6f395-1850-4b3b-a531-3b45c3a6dd75" />

