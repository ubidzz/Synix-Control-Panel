# 🛸 Synix Control Panel
### **The High-Performance Backbone for Effortless Game Server Hosting**

![License](https://img.shields.io/badge/License-Proprietary-red.svg)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)
![Build](https://img.shields.io/badge/Architecture-Engine--Driven-orange.svg)

**Synix Control Panel** is an elite, engine-driven management suite designed to make professional-grade hosting accessible to everyone. By moving beyond simple batch-file scripts, Synix provides a centralized "Brain" that handles deployment, process health, and technical directory constraints automatically.

---

## 🎮 Hosting Made Effortless
Synix is designed to remove the "technical headache" of hosting. Whether you are a veteran or a first-time host, the Synix Engine handles the complexity for you:

1.  **Select Your Game:** Choose from an extensive library including **Rust**, **ARK**, **Soulmask**, and **StarRupture**.
2.  **Smart Configuration:** The UI dynamically adapts. If a game doesn't use a "World Seed" or "PVP/PVE" toggle, Synix locks those fields so you never waste time on settings that don't matter.
3.  **One-Click Launch:** Synix validates your ports, creates your folders, and deploys the server. Your only job is to play.

---

## 🧠 The Synix Engine: Professional Automation
Synix is built on a **Modular Singleton Architecture**. The UI acts as a high-speed client, while the core engine manages your servers with built-in intelligence.

* **🤖 Autonomous Watchdog:** Synix monitors the process message loop to detect crashes or freezes (Not Responding). If a failure is detected, the engine initiates a recovery sequence: *Save Data ➔ Graceful Shutdown ➔ Automated Reboot*.
* **🛡️ Smart-Context Logic:** The engine scans game blueprints to ensure your setup is bulletproof. It automatically translates human-friendly UI choices into technical engine requirements—such as converting "PVP/PVE" into the "True/False" logic required by games like **ARK**.
* **📂 Intelligent Identity Mapping:** Synix eliminates the common "Space in Path" errors that crash many servers. It automatically generates a technical **{Identity}** for every server, using underscores for folder paths while keeping your custom server name exactly as you typed it for the hostname.
* **📡 Real-Time Telemetry:** Stay informed with live CPU and RAM monitoring, processed in the background to ensure your management console stays lightning-fast even under heavy loads.

---

## ✨ Feature Highlights

* **🔄 Atomic Action Protection:** Operations like Updates and Starts are state-locked. You can't accidentally interrupt a server while it is downloading or saving.
* **📦 SteamCMD Integration:** Native, automated deployment and updates for the world's most popular survival and shooter titles.
* **🛠️ Universal Config Suite:** A built-in, format-aware editor (JSON, INI, XML) that resolves dynamic path tags like `{Identity}` and `{Port}` to open the correct file every time.
* **🎨 Semantic Logging:** A multi-threaded console that uses color-coding to help you distinguish between system info, success states, and critical alerts.

---

## 💻 Technical Stack
> **Maximum Control. Minimal Footprint.**

* **Framework:** C# / .NET 
* **Design Pattern:** Singleton Engine Pattern with Partial Class modularity.
* **Concurrency:** Task-based Asynchronous Pattern (TAP) for non-blocking I/O.
* **Persistence:** JSON-based state management for ultra-fast server rebinding.
* **Process Hooking:** Direct Windows API integration for PID tracking and status monitoring.

---

## 📜 License & Legal Information
*Copyright © 2026 ubidzz. All Rights Reserved.*

The **Synix Control Panel** is a proprietary software project. The source code is provided for **viewing and personal educational purposes only**.

* **No License:** The author retains all legal rights to the source code.
* **Strictly Prohibited:** Modification, redistribution, or commercial resale of the source code or compiled binaries is strictly prohibited.
* **Commercial Use:** Unauthorized commercial deployment or branding is a violation of copyright.

---

### **Status: Revolutionizing the Personal Host Experience**
*Synix is currently in active development. Experience the engine that handles the hard work so you can focus on the game.*