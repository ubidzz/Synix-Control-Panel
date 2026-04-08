

## Coming Soon


# Synix Control Panel
### **A Lightweight, High-Performance Game Server Manager**

![License](https://img.shields.io/badge/License-Proprietary-red.svg)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)

**Synix Control Panel** is a robust management tool built to bridge the gap between complex command-line tools and user-friendly interfaces. It automates the "dirty work" of server hosting—handling SteamCMD deployments, validating ports, and providing deep-level process monitoring—so you can focus on the game, not the configuration.

---

## 🚀 What it Does

Synix streamlines the entire lifecycle of a game server. It acts as a central nervous system for your hosting environment, providing a unified dashboard to install, update, and monitor multiple game servers simultaneously.

## ✨ Key Features

* **🛡️ Port Guard Validation:** Built-in conflict detection prevents Game, Query, or RCON port overlaps. It intelligently skips inactive features (like disabled RCON) to ensure maximum flexibility.
* **🔄 Smart Process Rebinding:** Even if the app closes or crashes, Synix "remembers" its children. Upon restart, it automatically re-detects and adopts running server processes using saved PIDs.
* **📊 Real-Time Telemetry:** Integrated high-performance charts provide live-streaming waves of CPU and RAM usage for each individual server.
* **📦 SteamCMD Automation:** One-click deployment for major titles (Soulmask, StarRupture, etc.), including automated verification and update handling.
* **🎨 Pro-Grade UI:** A customized, double-buffered Dark Theme grid and a multi-threaded, color-coded logging console.
* **🛠️ Post-Install Fixes:** Automatically applies custom patches, configuration tweaks, or missing DLLs immediately after a SteamCMD update.

## 💻 Technical Overview

> **Minimum Overhead, Maximum Control.**
> Synix is designed to be a "Zero-Bloat" application. It does not install hidden background services; it only runs when you need it.

* **Core Engine:** C# / .NET Framework
* **Process Hooking:** Direct Windows API integration for PID tracking.
* **Concurrency:** Multi-threaded asynchronous I/O to ensure the UI never freezes during heavy downloads.

---

## 📝 App "About" Description
**Synix Control Panel v1.0**

Synix is a high-performance game server manager designed for efficiency and reliability. By combining automated SteamCMD handling with real-time hardware telemetry and smart process adoption, Synix provides a professional-grade hosting environment for home labs and dedicated server hosts alike.

**Key Operations:**
* Live Process Monitoring (PID Tracking)
* Automated SteamCMD Lifecycle Management
* Smart Port Conflict Prevention
* Asynchronous Logging & Real-time Resource Graphing

---

## 📜 License & Usage

*Copyright © 2026 ubidzz. All Rights Reserved.*

This source code is provided for **viewing and personal educational purposes only**.

* **No License:** This project does not carry an open-source license. The author retains all legal rights to the source code.
* **Prohibited:** You may not modify, redistribute, or sell this software or its source code in any form.
* **Commercial Use:** Unauthorized commercial use or resale is strictly prohibited.