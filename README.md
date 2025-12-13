# Cinema Reservation System

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)
![Language](https://img.shields.io/badge/language-F%23-378bba.svg)

A desktop application for managing cinema seat reservations. Built with F# and WinForms, this project demonstrates functional programming principles applied to a UI application, featuring persistent storage and a testable business logic layer.

---

## Features

*   **Interactive Seat Grid**: Visual 5x6 grid representing the cinema hall.
*   **Real-time Feedback**: Immediate visual cues (Green for free, Red for booked).
*   **Conflict Prevention**: Logic to prevent double-booking of seats.
*   **Data Persistence**: Automatic saving of ticket data to local file storage.
*   **Reset Functionality**: Administrative reset to clear all bookings.
*   **Ticket Generation**: Unique UUID generation for every successful booking.

---

## Architecture and Design

This project follows a layered architecture, separating the Presentation, Business Logic, and Data Persistence layers.

### System Block Diagram

```mermaid
graph TD
    %% Actors
    User((User))
    Tester((Developer/CI))

    %% Subgraph for the Application Entry Point and UI
    subgraph "Program.fs (UI Layer)"
        AppStart(["Application Start"])
        InitForm["Initialize Main Form"]
        InitGrid["Initialize Seat Grid (5x6)"]
        
        subgraph "UI Components"
            SeatBtn["Seat Button"]
            ResetBtn["Reset Button"]
            MsgBox["Message Box"]
        end
        
        subgraph "Event Handlers"
            OnSeatClick{"Seat Clicked?"}
            OnResetClick{"Reset Clicked?"}
            CheckBooked{"Is Seat Booked?"}
            UpdateUI_Booked["Change Color to Red"]
            UpdateUI_Reset["Change All Colors to Green"]
        end
    end

    %% Subgraph for Business Logic
    subgraph "CinemaLogic.fs (Logic Layer)"
        BookSeatFunc["bookSeat Function"]
        SaveTicketFunc["saveTicket Function"]
        GenIdFunc["generateTicketId Function"]
    end

    %% Subgraph for Data Storage
    subgraph "Data Layer"
        MemoryArray[("In-Memory Seat Array<br>(bool matrix)")]
        FileSystem[("File System<br>(tickets.txt)")]
    end

    %% Subgraph for Tests
    subgraph "final project.Tests (Test Layer)"
        Test1["Test: Booking empty seat"]
        Test2["Test: Booking taken seat"]
    end

    %% Flow Connections
    AppStart --> InitForm
    InitForm --> InitGrid
    InitGrid --> SeatBtn
    InitForm --> ResetBtn

    %% User Interactions
    User -->|Clicks| SeatBtn
    User -->|Clicks| ResetBtn

    %% Seat Click Flow
    SeatBtn --> OnSeatClick
    OnSeatClick --> CheckBooked
    CheckBooked -- Yes --> MsgBox
    MsgBox -.->|Show 'Already booked!'| User
    
    CheckBooked -- No --> BookSeatFunc
    BookSeatFunc -->|Update State| MemoryArray
    BookSeatFunc -->|Return True| UpdateUI_Booked
    
    UpdateUI_Booked --> SaveTicketFunc
    SaveTicketFunc --> GenIdFunc
    GenIdFunc -->|Return UUID| SaveTicketFunc
    SaveTicketFunc -->|Append Text| FileSystem
    SaveTicketFunc -->|Return Ticket Info| MsgBox
    MsgBox -.->|Show 'Seat booked!'| User

    %% Reset Click Flow
    ResetBtn --> OnResetClick
    OnResetClick -->|Loop Rows/Cols| MemoryArray
    MemoryArray -->|Set all false| UpdateUI_Reset
    UpdateUI_Reset -->|Clear Content| FileSystem
    UpdateUI_Reset -->|Show 'All seats reset!'| MsgBox
    MsgBox -.->|Confirmation| User

    %% Test Flow
    Tester -->|Runs Tests| Test1
    Tester -->|Runs Tests| Test2
    Test1 -->|Calls| BookSeatFunc
    Test2 -->|Calls| BookSeatFunc
    Test1 -.->|Asserts True| Tester
    Test2 -.->|Asserts False| Tester

    %% Styling
    classDef ui fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef logic fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;
    classDef data fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef test fill:#fce4ec,stroke:#880e4f,stroke-width:2px;
    classDef actor fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px;

    class AppStart,InitForm,InitGrid,SeatBtn,ResetBtn,MsgBox,OnSeatClick,OnResetClick,CheckBooked,UpdateUI_Booked,UpdateUI_Reset ui;
    class BookSeatFunc,SaveTicketFunc,GenIdFunc logic;
    class MemoryArray,FileSystem data;
    class Test1,Test2 test;
    class User,Tester actor;
```

### Technical Architecture

#### Class Structure

```mermaid
classDiagram
    direction TB

    %% Program Entry Point
    class Program {
        +int rows = 5
        +int cols = 6
        +bool[,] seats
        +string ticketFile
        +Form form
        +Button[,] seatButtons
        +Button resetBtn
        +main()
    }

    %% Business Logic Module
    class CinemaLogic {
        <<Module>>
        +bookSeat(seats: bool[,], row: int, col: int) bool
        +saveTicket(row: int, col: int, ticketFile: string) string
        -generateTicketId() string
    }

    %% External Dependencies
    class System_IO {
        <<Library>>
        +File.AppendAllText(path, content)
        +File.WriteAllText(path, content)
    }

    class System_Windows_Forms {
        <<Library>>
        +MessageBox.Show(text)
        +Button
        +Form
    }

    %% Relationships
    Program ..> CinemaLogic : Import & Call
    Program ..> System_Windows_Forms : Inherits/Uses
    Program ..> System_IO : Uses (Reset)
    CinemaLogic ..> System_IO : Uses (Save)
```

#### Booking Sequence

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as Program (Button_Click)
    participant Logic as CinemaLogic
    participant State as In-Memory Array
    participant FileSys as File System

    User->>UI: Click Seat Button (row, col)
    
    %% Check if already booked (UI Logic)
    UI->>State: Check seats[row, col]
    alt Seat is already booked (true)
        State-->>UI: Returns true
        UI->>User: MessageBox "Seat already booked!"
    else Seat is free (false)
        State-->>UI: Returns false
        
        %% Call Business Logic
        UI->>Logic: bookSeat(seats, row, col)
        activate Logic
        Logic->>State: seats[row, col] <- true
        Logic-->>UI: Returns true
        deactivate Logic

        %% Update UI Visuals
        UI->>UI: Button.BackColor = Color.Red

        %% Save Ticket
        UI->>Logic: saveTicket(row, col, ticketFile)
        activate Logic
        Logic->>Logic: generateTicketId()
        Logic-->>Logic: Returns UUID
        Logic->>FileSys: File.AppendAllText(ticketInfo)
        Logic-->>UI: Returns ticketInfo string
        deactivate Logic

        %% Confirmation
        UI->>User: MessageBox "Seat booked! \n ID: ..."
    end
```

---

## Getting Started

### Prerequisites
*   .NET SDK (Version 6.0 or later)

### Installation
1.  Clone the repository.
2.  Navigate to the project directory: `Cinema_Reservation_System/final project`

### Running the Application
Run the project using the dotnet CLI:
```bash
dotnet run
```

---

## Project Structure

*   **final project/**: Main Application
    *   `Program.fs`: UI Entry Point and Event Handling
    *   `CinemaLogic.fs`: Pure Business Logic
*   **final project.Tests/**: Unit Tests
    *   `Tests.fs`: Xunit Test Cases

---

## Testing Strategy

The project employs a dual-layer testing strategy to ensure reliability across both business logic and user interaction.

### 1. Automated Unit Testing (Logic Layer)
*   **Framework**: Xunit
*   **Scope**: `CinemaLogic.fs`
*   **Coverage**:
    *   **Valid Booking**: Ensures a free seat returns `true` and updates state.
    *   **Double Booking**: Ensures attempting to book an occupied seat returns `false`.
    *   **State Integrity**: Verifies the in-memory array reflects changes accurately.

To execute the automated suite:
```bash
dotnet test
```

### 2. Manual User Acceptance Testing (UI Layer)
*   **Scope**: `Program.fs` (WinForms UI)
*   **Test Cases**:
    *   **Visual Verification**: Confirm seats change from Green to Red upon clicking.
    *   **Persistence Check**: Verify `tickets.txt` is created/updated after a booking.
    *   **Reset Validation**: Confirm "Reset All" clears the grid visually and empties the file.
    *   **Error Handling**: Verify the "Seat already booked!" message appears when clicking a red seat.

---

## Team Roles and Architecture Mapping

The project's modular architecture is designed to support the specific roles defined in the project requirements.

| Role | Responsibility | Project Component |
| :--- | :--- | :--- |
| **1. Seat Layout Architect** | Defines 2D array structure | `Program.fs` (rows/cols definition) |
| **2. Display Developer** | Visualizes grid in UI | `Program.fs` (Button grid generation) |
| **3. Booking Logic Developer** | Implements core rules | `CinemaLogic.bookSeat` |
| **4. Ticket System Developer** | Unique IDs + format | `CinemaLogic.generateTicketId` |
| **5. File Storage Developer** | Saves data to disk | `CinemaLogic.saveTicket` |
| **6. UI Developer** | Main Form & Controls | `Program.fs` (Form, Controls) |
| **7. Tester** | Verifies functionality | `final project.Tests` |
| **8. Documentation Lead** | Maintains docs & graphs | `README.md`, `ArchitectureGraph.md` |

---

## Code Reference

### Module: CinemaLogic
Contains the core business logic for the Cinema Reservation System. Handles seat booking validation and ticket generation.

*   **`bookSeat(seats: bool[,], row: int, col: int) -> bool`**
    *   Attempts to book a specific seat.
    *   Returns `true` if successful (seat was free), `false` if already booked.

*   **`saveTicket(row: int, col: int, ticketFile: string) -> string`**
    *   Persists ticket information to a file and returns the ticket details.
    *   Generates a unique Ticket ID internally.

*   **`generateTicketId() -> string`**
    *   Generates a unique identifier (UUID) for a ticket.

### Module: Program
Entry point for the application. Manages UI initialization, event handling, and application lifecycle.

*   **`seats: bool[,]`**
    *   In-memory representation of the seat grid (5x6).
    *   `true` = Booked, `false` = Free.

*   **`seatButtons: Button[,]`**
    *   Visual representation of the grid.
    *   Handles click events to trigger `CinemaLogic.bookSeat`.

*   **`resetBtn: Button`**
    *   Resets the entire cinema state (memory and file) to initial empty state.

---

## Technologies

*   F#
*   .NET
*   Windows Forms
*   Xunit
*   Mermaid.js
