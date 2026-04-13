# 🛸 Synix Control Panel

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
2.  **Smart Configuration:** The UI dynamically adapts. Fields like "App Port" or "RCON" only unlock when the game actually supports them, preventing "Ghost Conflicts" in your network.
3.  **Visual Lifecycle:** See exactly what’s happening with real-time grid status updates. From **"Backing up"** archives to **"Downloading"** updates, you're never left wondering what the engine is doing.
4.  **One-Click Launch:** Synix validates your ports, creates your folders, resolves dependencies, and deploys the server instantly.

---

## 🧠 The Synix Engine: Professional Automation
Synix is built on a **Modular Singleton Architecture**. The UI acts as a high-speed client, while the core engine natively manages your servers with built-in intelligence.

* **🤖 Autonomous Watchdog:** Synix monitors the process message loop to detect crashes or freezes ("Not Responding" states). If a failure is detected, the engine initiates a recovery sequence: *Save Data ➔ Graceful Shutdown ➔ Automated Reboot*.
* **📂 Universal AppID Discovery:** No more hardcoded launch IDs. Synix dynamically searches your server directories for `steam_appid.txt`, self-healing the file and injecting the correct ID from the GameDatabase blueprint before the process even starts.
* **🛡️ Smart-Context Logic:** The engine scans game blueprints to ensure your setup is bulletproof. It automatically translates human-friendly UI choices into technical engine requirements—such as converting "PVP/PVE" toggles into the "True/False" arguments required by games like **ARK**.
* **📂 Intelligent Identity Mapping:** Synix eliminates the common "Space in Path" errors. It automatically generates a technical `{Identity}` for every server, using safe underscores for folder paths while keeping your custom server name exactly as you typed it for the public hostname.

---

## 🚀 The Dual-Pass Deployment System
Synix completely automates the traditional frustrations of Windows server hosting. Once SteamCMD finishes downloading the server files, the Synix `PostInstall` routine intercepts the deployment:

* **Pass 1 (Automated DLL Injection):** Natively detects Unreal Engine and Source engine titles, automatically grabbing required SteamCMD DLLs (`steamclient64.dll`, `tier0_s64.dll`, `vstdlib_s64.dll`) and injecting them directly into the game's `Binaries\Win64` folders to prevent launch crashes.
* **Pass 2 (Dynamic Config Generation):** Automatically builds, formats, and deploys default configuration files before the server boots. Synix natively handles `.json`, `.xml`, `.ini`, `.lua`, `cfg`, and proprietary formats.

---

## ✨ Feature Highlights

* **🔄 Atomic Action Protection:** Operations like Updates and Starts are state-locked. You can't accidentally interrupt or corrupt a server while it is downloading, updating, or saving.
* **📦 Safe-Start Backups:** Optional automated compression before every launch. Synix zips your game data into a timestamped archive, rotating the newest 3 backups to save disk space.
* **🛠️ Universal Config Suite:** A built-in, format-aware editor that resolves dynamic path tags like `{Identity}` and `{Port}` to open the correct file every time.
* **🎨 Semantic Logging:** A multi-threaded console that uses color-coding to help you instantly distinguish between system info, success states, and critical alerts.

---

## 💻 Technical Stack
> **Maximum Control. Minimal Footprint.**

* **Performance Benchmarks:** Optimized for ultra-low overhead, maintaining a **~30MB RAM** idle footprint and **<1% CPU** usage.
* **Framework:** C# / .NET 8.0+
* **Design Pattern:** Singleton Engine Pattern with Partial Class modularity.
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O.
* **Persistence:** JSON-based state management for ultra-fast server rebinding.
* **Process Hooking:** Direct Windows API integration for PID tracking and status monitoring.

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