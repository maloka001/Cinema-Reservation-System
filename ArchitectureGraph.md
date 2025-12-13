# Cinema Reservation System - Full Architecture Documentation

This document provides a complete technical breakdown of the system using standard UML diagram types rendered with Mermaid.

## 1. Static Structure (Class/Module Diagram)
This diagram represents the code structure, mapping F# modules to classes and showing exact dependencies and members.

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

## 2. Dynamic Behavior (Sequence Diagram)
This diagram details the exact runtime flow when a user clicks a seat button.

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

## 3. System States (State Machine Diagram)
This diagram represents the lifecycle of a single seat within the system.

```mermaid
stateDiagram-v2
    [*] --> Free : Application Start / Reset

    state "Free (Available)" as Free {
        [*] --> GreenColor
        GreenColor --> [*]
        note right of GreenColor : Button.BackColor = LightGreen\nseats[r,c] = false
    }

    Free --> Booked : User Clicks Button
    
    state "Booked (Occupied)" as Booked {
        [*] --> RedColor
        RedColor --> PersistData
        PersistData --> [*]
        
        note right of RedColor : Button.BackColor = Red\nseats[r,c] = true
        note right of PersistData : Ticket saved to tickets.txt
    }

    Booked --> Booked : User Clicks Button\n(Show Error Message)
    Booked --> Free : User Clicks 'Reset All'
```
