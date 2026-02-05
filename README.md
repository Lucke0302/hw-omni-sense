# HW OmniSense 🛰️
> **Advanced Hardware Intelligence & Remote Monitoring Ecosystem**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Tauri](https://img.shields.io/badge/built%20with-Tauri-24C8D8?logo=tauri)](https://tauri.app/)
[![React](https://img.shields.io/badge/Frontend-React-61DAFB?logo=react)](https://reactjs.org/)
[![Node.js](https://img.shields.io/badge/Bot-Node.js-339933?logo=node.js)](https://nodejs.org/)

[![Download Latest](https://img.shields.io/github/v/release/lucke0302/hw-omni-sense?label=Download%20Latest%20.msi&style=for-the-badge&color=green)](https://github.com/lucke0302/hw-omni-sense/releases/latest/)

**HW OmniSense** is a lightweight, open-source hardware monitoring solution designed for enthusiasts who demand total control. Built with a modern **Tauri + React + TypeScript** stack, it bridges the gap between low-level hardware metrics and high-level remote accessibility.

Originally conceived to monitor high-performance GPUs in challenging tropical climates, HW OmniSense provides real-time tracking of Temperatures, Hot Spots, VRAM, and FPS, with a unique **WhatsApp integration** for remote status alerts.

---

## ✨ Key Features

* **⚡ Ultra-Lightweight Architecture:** Powered by **Tauri**, using a fraction of the RAM compared to Electron-based monitors.
* **🎯 Hot Spot Precision:** Advanced tracking of GPU "Delta" temperatures (Core vs. Hot Spot) to monitor thermal paste health and mounting pressure.
* **📱 WhatsApp Remote Link:** Securely pair your PC with a WhatsApp Bot (via **Baileys**) to receive real-time status updates and thermal alerts on your phone.
* **🛡️ "Santos Mode" (Climate Awareness):** Intelligent monitoring tailored for high-humidity environments to prevent oxidation and manage thermal peaks.
* **📊 Local History (SQLite):** Persistent logging of performance metrics without cloud dependency (your data stays on your machine).
* **🛠️ 3D-Mod Friendly:** Built-in support and presets for custom cooling solutions and 3D-printed shrouds.

---

## 🏗️ Architecture & Tech Stack



* **Frontend:** React 19, TypeScript, TailwindCSS.
* **Desktop Shell:** Tauri (Rust).
* **Hardware Extraction:** C#/.NET Sidecar (utilizing `LibreHardwareMonitor`).
* **Database:** SQLite for local time-series logging.
* **Mobile Bridge:** Node.js + Baileys (WhatsApp Web API).

---

## 🚀 Why HW OmniSense?

Most hardware monitors are either too bloated or trapped within your desktop. **HW OmniSense** was built for the modder who is also a developer. Whether you are benchmarking a heavy session of *Warzone* or monitoring your rig while away from your desk, OmniSense gives you the data you need, where you need it.

---

## 🤝 Contributing

This is an **Open Source** project. We welcome contributions regarding:
* **New Hardware Sensors:** Expanding support for different GPU/CPU architectures.
* **UI/UX Improvements:** Refining the Dashboard experience with React.
* **Localization (i18n):** Helping us reach a global audience.
* **C# Sidecar Optimization:** Enhancing the performance of the data collector.
* **🐧 Linux Support:** Since the lead developer is on Windows, help with implementing Linux hardware monitoring (e.g., via `lm-sensors`) is highly encouraged to make OmniSense truly universal!

---

## ⚖️ License

Distributed under the MIT License. See `LICENSE` for more information.

---
*Developed for hardware enthusiasts, by hardware enthusiasts.*
