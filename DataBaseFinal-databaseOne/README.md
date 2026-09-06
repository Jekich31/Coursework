# Remote Car Control System

![.NET Version](https://img.shields.io/badge/.NET-8.0%20%2F%209.0-blue)
![Project Type](https://img.shields.io/badge/Project-Console%20Application-green)
![Architecture](https://img.shields.io/badge/Architecture-OOP%20%2F%20Patterns-orange)

**Remote Car Control** is a .NET console application that simulates a modern ecosystem of smart vehicles. This project is designed to demonstrate practical proficiency in core and advanced C# concepts, object-oriented design (OOD), design patterns, and resource management.

The application allows users to manage their fleet, track geolocation in real time, receive instant alerts regarding security boundary breaches (Geofencing), and maintain a detailed audit trail of system logs.

---

## 🚀 Key Features

* **Fleet Management:** User registration, adding electric vehicles (`ElectricCar`) and conventional cars (`GasolineCar`) utilizing the Factory Method pattern.
* **Remote Control:** Locking/unlocking doors, starting and stopping the engine through a unified interface.
* **Telemetry Simulation & Geofencing:** Real-time updates of vehicle coordinates with automatic boundary violation checks driven by events.
* **Full State Persistence:** Automatic serialization of the entire database into JSON format upon exiting, and seamless deserialization on application startup.
* **End-to-End Logging:** Keeping a continuous record of all critical events and user actions in a `telemetry.log` file.

---

## 🛠️ Technical Stack & Implementation (By Course Modules)

### Module 1: Fundamentals, Data Structures & Core Logic
* **C# 12/13 (.NET 8/9):** Leveraging modern syntax features, including **Top-level statements** in `Program.cs` for a clean, concise entry point.
* **Encapsulation:** Entity properties utilize restricted access modifiers (`get; private set;` / `get; init;`), preventing accidental modifications of critical data (IDs, VIN codes).
* **Architectural Separation:** Clear boundaries established using dedicated namespaces: `CarControl.Models`, `CarControl.Services`, `CarControl.Interfaces`.
* **Efficient Data Types:** Geolocation coordinates are implemented as a performance-efficient `struct GeoCoordinate`, and car states use type-safe `enum` structures (`EngineState`, `DoorState`).
* **Nullable Types:** Used for flexible handling of optional or missing data (e.g., `GeoCoordinate? HomeZone`).
* **Custom Exceptions:** Domain-specific error handling is managed via custom exceptions like `CarSecurityException` (e.g., attempting to start the engine while doors are unlocked).

### Module 2: Arrays, Collections & Advanced OOP
* **Validation & String API:** Strict verification of 17-character VIN codes and comprehensive processing of incoming user commands.
* **Operator Overloading:** The `==` and `!=` operators are overridden for the `Car` class to enable direct object comparison based on unique VIN codes.
* **Indexers:** Implemented indexing in the `User` class for rapid vehicle lookups by index `user[0]` or by a specific VIN code `user["VIN123"]`.
* **Polymorphism:** Built upon an abstract base class `Vehicle` and its derived classes, `ElectricCar` (tracking battery capacity) and `GasolineCar` (simulating fuel consumption), with overridden `DisplayStatus()` behavior.
* **Interfaces:** The hardware control contract is strictly defined and enforced through `IRemoteControllable`.

### Module 3: Events, Generics, LINQ & Data Processing
* **Events & Records:** The `OnGeofenceViolation` event leverages immutable `record` types to reliably transmit breach data payloads.
* **Generics:** Created a generic `CommandsHistory<T>` class to store an audit trail of actions or breadcrumb movement tracks.
* **LINQ:** Applied Fluent/Query syntax for advanced fleet filtration (finding vehicles with low energy levels, sorting by brand, or querying running vehicles).
* **I/O & Serialization:** File auditing is implemented using `StreamWriter`, and system state preservation is achieved via `System.Text.Json`.

### Module 4: Architecture & Resource Management
* **Memory Management:** Proper implementation of the `IDisposable` pattern alongside `using` statements ensures guaranteed, deterministic release of system log file handles.

---

## 🖥️ Console Menu Structure

```text
Main Screen
 ├── [1] Register
 ├── [2] Log In to Personal Account
 └── [3] Exit (JSON Auto-save)

Personal Account (LINQ Fleet Monitor)
 ├── [1] Add Vehicle (Factory Method Call: Electric / Gasoline)
 ├── [2] Select Vehicle from Garage (Indexer-based lookup)
 └── [3] Back

Vehicle Control Screen (IRemoteControllable)
 ├── Status: [Doors: Locked | Engine: Stopped | Battery: 85% | Coordinates: 50.45, 30.52]
 ├── Commands: Lock/Unlock, Start/Stop Engine
 └── [Simulate Coordinate Change] ──> (Trigger Geofence -> Event -> Write to .log)
