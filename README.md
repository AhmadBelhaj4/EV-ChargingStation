# ⚡ EV-ChargingStation — EV Battery & Energy Twin Simulation

An interactive 3D simulation developed with **Unity 6** to explore electric-vehicle battery assembly, battery thermal management, and renewable-energy charging infrastructure in a virtual environment.

The project combines multiple simulation environments covering **EV battery assembly, Battery Thermal Management (BTMS), and solar-powered charging infrastructure**.

---

## 👥 Team & Project Organization

The project was developed collaboratively as a student team and divided into three major technical components:

* 🏭 **EV Battery Assembly & Workshop**
* ❄️ **Battery Thermal Management / Cooling System**
* ☀️ **Solar Energy & Charging Infrastructure Digital Twin**

### ❄️ My Contribution — Battery Thermal Management System

I worked with **Kalil Moalla** on the **Battery Thermal Management System (BTMS)** component.

Our work focused on creating an interactive EV battery cooling and maintenance scenario, where the player follows a guided maintenance procedure and is evaluated based on their performance.

The BTMS component includes:

* Vehicle arrival and garage entry animation
* Mechanic cutscene and guided instructions
* Cooling-pipe draining
* Damaged pipe removal
* Workbench interaction
* Interactive pipe welding
* Reinstallation of the repaired pipe
* Coolant refill
* Battery voltage and coolant checks
* Time-based performance scoring and rating

> **Note:** The overall project was developed by a larger team. The features described above represent the Battery Thermal Management component developed by Ahmad Belhaj and Kalil Moalla.

---

## 🌟 Project Overview

The project integrates three major virtual environments.

### 1. 🏭 Workshop & Battery Assembly

The main workshop provides an interactive 3D environment for EV battery assembly and maintenance.

Features include:

* Interactive workshop tools
* Battery pack assembly
* Mechanical interactions
* Battery components and cooling loops
* Physics-based interactions
* Collision detection and object interaction

### 2. ❄️ Battery Thermal Management System

The BTMS environment focuses on the cooling and maintenance of an electric-vehicle battery.

The player is guided through a complete maintenance procedure:

1. **Vehicle arrival** — The EV enters the garage through an animated sequence and the mechanic exits the vehicle.
2. **Mechanic guidance** — The player approaches the mechanic and receives instructions for the maintenance procedure.
3. **Pipe draining** — The player empties the cooling-system pipes.
4. **Damaged pipe removal** — The damaged pipe is removed from the vehicle.
5. **Welding repair** — The damaged pipe is taken to a workbench and repaired through an interactive welding task.
6. **Pipe installation** — The repaired pipe is returned to its position in the cooling system.
7. **Coolant refill** — The cooling pipes are refilled with coolant.
8. **System verification** — The player checks the battery voltage and coolant status.
9. **Performance evaluation** — The completed procedure is evaluated according to the time taken, producing a final score/rating.

The goal was to transform the cooling-system maintenance process into an **interactive, task-oriented 3D simulation** rather than a static visualization.

### 3. ☀️ Solar Energy Digital Twin

The solar-energy environment explores the relationship between renewable energy generation, energy storage, and EV charging infrastructure.

Features include:

* Photovoltaic energy generation
* Solar-energy visualization
* Battery energy storage
* EV charging infrastructure
* Dynamic environmental conditions
* Energy-flow visualization

---

## 🛠️ Technologies

* **Unity 6**
* **C#**
* **Blender**
* **Universal Render Pipeline (URP)**
* 3D modeling and interactive simulation
* Physics and collision systems
* Unity animation and visualization systems

---

## 🎮 Demo

### Full Gameplay Demonstration

A full gameplay demonstration of the project is available here:

🎥 **https://youtu.be/YmDXvR9qYHk**

The video demonstrates the different interactive environments developed for the project, including the Battery Thermal Management System.

### BTMS Demo Highlights

The Battery Thermal Management section demonstrates:

* Vehicle arrival and mechanic interaction
* Guided maintenance instructions
* Cooling-pipe draining
* Damaged-pipe removal
* Welding interaction
* Pipe reinstallation
* Coolant refill
* Voltage and coolant verification
* Performance scoring

---

## 🚀 Getting Started

### Prerequisites

* Unity Hub
* **Unity 6000.3.6f1**
* Windows x64

### Installation

Clone the repository:

```bash
git clone YOUR_REPOSITORY_URL
cd EV-ChargingStation
```

Open the project using **Unity Hub** and select the project directory.

Then open the relevant scene from:

```text
Assets/Scenes/
```

Available environments include:

```text
0.unity
cooling scene.unity
SolarTwinScene.unity
```

> **Note:** Unity's `Library/` directory is excluded from version control because it contains generated project cache data.

---

## 📁 Repository Structure

```text
EV-ChargingStation/
│
├── Assets/
│   ├── 3d models/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Shaders/
│   └── ...
│
├── Packages/
├── ProjectSettings/
├── Project_Report.tex
├── .gitignore
├── .gitattributes
└── README.md
```

---

## 📜 Academic Project

This project was developed as part of an academic engineering project focused on:

* 3D simulation
* Electric-vehicle technology
* Battery systems
* Battery thermal management
* Digital-twin concepts
* Renewable-energy infrastructure

A technical report is included in the repository where applicable.

---

## 👨‍💻 Team

The project was developed collaboratively by:

* **Ahmad Belhaj** — Battery Thermal Management System
* **Kalil Moalla** — Battery Thermal Management System
* **Fedi Hesine** — Project component
* **Mohamed Jahha** — Project component
* **Sofiene Wannes** — Project component
* **Yessine Ben Ayed** — Project component
* **Yessine Achour** — Project component

### Contribution Note

The project was divided into multiple technical components, with team members collaborating within their respective areas.

**Ahmad Belhaj and Kalil Moalla** were responsible for the **Battery Thermal Management / Cooling System** component described in this README.

---

## 📄 License

This project was developed for academic purposes.
