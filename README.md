# Synix Control Panel
### **A Lightweight, Engine-Driven Game Server Manager**
## Coming Soon

![License](https://img.shields.io/badge/License-Proprietary-red.svg)
![Language](https://img.shields.io/badge/Language-C%23-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)

**Synix Control Panel** is a robust management tool built to bridge the gap between complex command-line tools and user-friendly interfaces. It automates the "dirty work" of server hosting—handling SteamCMD deployments, validating ports, and providing deep-level process monitoring—so you can focus on the game, not the configuration.

---

## 🚀 The Synix Engine (The Brain)

Synix has been rebuilt with an **Engine-Driven Architecture**. The User Interface (GUI) is now a "Thin Client," while the **Synix Engine** acts as the central brain, managing all rules, logic, and safety protocols.

* **🛡️ Centralized Validation:** The engine acts as a gatekeeper, performing integrity checks on game files, preventing port overlaps, and blocking duplicate naming conventions before any action is taken.
* **🤖 Autonomous Watchdog:** A high-frequency monitoring system that detects unexpected server shutdowns and automatically initiates a recovery sequence to bring servers back online.
* **📡 Thread-Safe Telemetry:** A dedicated resource engine that calculates CPU and RAM usage in the background, providing the UI with pre-processed data for real-time charting.
* **⛓️ Atomic Actions:** Every server command—Start, Stop, Update, or Delete—is handled by the engine's action layer, ensuring that "Busy" states (like active downloads) protect the system from accidental corruption.

---

## ✨ Key Features

* **🚀 "Thin" GUI Experience:** The interface is optimized for speed, serving only to display data and capture user intent while the engine handles the heavy lifting in the background.
* **🔄 Watchdog Auto-Restart:** If a server process crashes, the engine detects the failure, logs the incident in red, and automatically restarts the server after a safety cooldown.
* **🛡️ Multi-Level Safeguards:** Bulletproof validation prevents Game, Query, or RCON port overlaps and strictly blocks duplicate server names or folder paths.
* **📊 Live Resource Streams:** High-performance charts provide real-time telemetry of CPU and RAM usage for every individual server in the list.
* **⚙️ Unified Config & Folder Access:** One-click access to game-specific configuration editors and server installation directories, managed entirely by engine logic.
* **📦 SteamCMD Automation:** One-click deployment and updating for major titles (Soulmask, StarRupture, etc.), including automated exit-code failure detection.
* **🎨 Color-Coded Intelligent Log:** A multi-threaded logging console that distinguishes between system info, success states, and critical engine alerts.

---

## 💻 Technical Overview

> **Minimum Overhead, Maximum Control.**
> Synix is designed to be a "Zero-Bloat" application. It utilizes a modular Partial Class architecture to separate concerns between the UI and the Core Engine.

* **Core Engine:** C# / .NET Framework utilizing a Singleton "Brain" pattern.
* **Architecture:** Modular Logic (Actions, Validator, Watchdog, Resources, Status).
* **Concurrency:** Asynchronous Task-based I/O to ensure the UI remains responsive during SteamCMD operations.
* **Process Hooking:** Direct Windows API integration for PID tracking and status rebinding.

---

## 📜 License & Usage

*Copyright © 2026 ubidzz. All Rights Reserved.*

This source code is provided for **viewing and personal educational purposes only**.

* **No License:** This project does not carry an open-source license. The author retains all legal rights to the source code.
* **Prohibited:** You may not modify, redistribute, or sell this software or its source code in any form.
* **Commercial Use:** Unauthorized commercial use or resale is strictly prohibited.