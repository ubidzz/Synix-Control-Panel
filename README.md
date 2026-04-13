# 🛸 Synix Control Panel

## Coming Soon.....

### **The High-Performance Backbone for Effortless Game Server Hosting**

![Language](https://img.shields.io/badge/Language-C%23-blue.svg?style=for-the-badge&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg?style=for-the-badge)
![Games Supported](https://img.shields.io/badge/Supported_Games-250-brightgreen.svg?style=for-the-badge)
![License](https://img.shields.io/badge/License-Proprietary-red.svg?style=for-the-badge)

**Synix Control Panel** is an elite, engine-driven management suite designed to make professional-grade hosting accessible to everyone. By moving beyond simple batch-file scripts and fragmented community plugins, Synix provides a centralized "Brain" that handles deployment, process health, configuration, and technical directory constraints automatically.

With **250+ unique game servers fully supported right out of the box**, Synix handles the heavy lifting so you can focus on playing.

---

## 🎮 Hosting Made Effortless
Synix is designed to remove the "technical headache" of hosting. Whether you are a veteran admin or a first-time host, the Synix Engine handles the complexity for you:

1.  **Select Your Game:** Choose from a massive native library including **Rust**, **ARK**, **Soulmask**, **Palworld**, and **StarRupture**.
2.  **Network Validation:** Built-in port checkers ensure your local sockets are clear before launch, while the **Connection Test** verifies if your server is actually visible on the public internet (WAN).
3.  **Live Player Monitoring:** Keep track of your community at a glance. Synix displays real-time player counts directly in the grid using high-speed UDP queries.
4.  **One-Click Launch:** Synix validates your ports, creates your folders, resolves dependencies, and deploys the server instantly.

---

## 🧠 The Synix Engine: Professional Automation
Synix is built on a **Modular Singleton Architecture**. The UI acts as a high-speed client, while the core engine natively manages your servers with built-in intelligence.

* **🤖 Autonomous Watchdog:** Synix monitors the process message loop to detect crashes or freezes ("Not Responding" states). If a failure is detected, the engine initiates a recovery sequence: *Save Data ➔ Graceful Shutdown ➔ Automated Reboot*.
* **📡 A2S Player Telemetry:** The engine uses the industry-standard A2S_INFO protocol to query running servers. It tracks `CurrentPlayers` vs `MaxPlayers` without impacting server performance.
* **📂 Universal AppID Discovery:** No more hardcoded launch IDs. Synix dynamically searches server directories for `steam_appid.txt`, self-healing the file and injecting the correct ID from the GameDatabase blueprint.
* **🌐 WAN/LAN Awareness:** Synix automatically detects your Local IP and Public IP, allowing for instant network diagnostics and easier port forwarding verification.

---

## 🚀 The Dual-Pass Deployment System
Synix completely automates the traditional frustrations of Windows server hosting. Once SteamCMD finishes downloading the server files, the Synix `PostInstall` routine intercepts the deployment:

* **Pass 1 (Automated DLL Injection):** Natively detects Unreal Engine and Source engine titles, automatically grabbing required SteamCMD DLLs (`steamclient64.dll`, `tier0_s64.dll`, `vstdlib_s64.dll`) and injecting them directly into the game's `Binaries\Win64` folders.
* **Pass 2 (Dynamic Config Generation):** Automatically builds, formats, and deploys default configuration files before the server boots. Synix natively handles `.json`, `.xml`, `.ini`, `.lua`, `cfg`, and proprietary formats.

---

## ✨ Feature Highlights

* **🔄 Atomic Action Protection:** Operations like Updates and Starts are state-locked. You can't accidentally interrupt or corrupt a server while it is downloading, updating, or saving.
* **📦 Safe-Start Backups:** Optional automated compression before every launch. Synix zips your world data into a timestamped archive, rotating the newest 3 backups to save disk space.
* **📡 Port & Connection Diagnostics:** An integrated scanner that checks both local socket availability and external WAN accessibility to ensure players can join.
* **🎨 Semantic Logging:** A multi-threaded console that uses color-coding to help you instantly distinguish between system info, success states, and critical alerts.

---

## 💻 Technical Stack
> **Maximum Control. Minimal Footprint.**

* **Performance Benchmarks:** Optimized for ultra-low overhead, maintaining a **~30MB RAM** idle footprint and **<1% CPU** usage.
* **Framework:** C# / .NET 8.0+
* **Design Pattern:** Singleton Engine Pattern with Partial Class modularity.
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O.
* **Network Logic:** UDP-based A2S querying and HttpClient WAN detection.

---

## 📜 License & Legal Information
*Copyright © 2026 ubidzz. All Rights Reserved.*

The **Synix Control Panel** is a proprietary software project. 

* **Permitted Use:** This software is **free for personal use**.
* **Source Code:** The source code is provided for **viewing and personal educational purposes only**.
* **No License:** The author retains all legal rights to the software and its source code.

### 🚫 Strictly Prohibited:
* **No Trading/Modding:** You may not modify or trade this software or its source code.
* **No Selling:** Commercial resale of the source code or compiled binaries is strictly prohibited.
* **No Redistribution:** You may not redistribute this software in any form.
* **Commercial Use:** Unauthorized commercial deployment is a direct violation of copyright.

---

### **Status: Revolutionizing the Personal Host Experience**
*Synix is currently in active development. Experience the engine that handles the hard work so you can focus on the game.*