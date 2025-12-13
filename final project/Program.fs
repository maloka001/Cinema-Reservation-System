/// <summary>
/// Entry point for the Cinema Reservation System application.
/// Manages the UI initialization, event handling, and application lifecycle.
/// </summary>
open System
open System.Drawing
open System.Windows.Forms
open System.IO
open CinemaLogic

// Configuration constants
let rows = 5
let cols = 6

/// <summary>
/// In-memory representation of the seat grid.
/// True indicates a booked seat, False indicates a free seat.
/// </summary>
let seats = Array2D.create rows cols false

/// <summary>
/// Path to the file where ticket records are stored.
/// </summary>
let ticketFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tickets.txt")

// Initialize the main application form
let form = new Form(Text = "Cinema Seat Reservation", Size = Size(600, 450))

/// <summary>
/// 2D Array of Button controls representing the visual seat grid.
/// Handles click events to trigger booking logic.
/// </summary>
let seatButtons = Array2D.init rows cols (fun r c ->
    let btn = new Button(Text = "", Size = Size(50, 50))
    btn.Location <- Point(c * 55 + 20, r * 55 + 20)
    btn.BackColor <- Color.LightGreen
    
    // Event Handler for Seat Click
    btn.Click.Add(fun _ ->
        if seats.[r,c] then
            MessageBox.Show("Seat already booked!") |> ignore
        else
            // Call Business Logic
            CinemaLogic.bookSeat seats r c |> ignore
            
            // Update UI State
            btn.BackColor <- Color.Red
            
            // Persist Data
            let ticketInfo = CinemaLogic.saveTicket r c ticketFile
            MessageBox.Show(sprintf "Seat booked!\n%s" ticketInfo) |> ignore
    )
    btn
)

// Add all seat buttons to the form
for r in 0 .. rows - 1 do
    for c in 0 .. cols - 1 do
        form.Controls.Add(seatButtons.[r,c])

/// <summary>
/// Button control to reset the entire cinema state.
/// Clears both in-memory state and the persistence file.
/// </summary>
let resetBtn = new Button(Text = "Reset All", Size = Size(100, 40))
resetBtn.Location <- Point(20, rows * 55 + 30)
resetBtn.BackColor <- Color.LightBlue
resetBtn.Click.Add(fun _ ->
    // Reset In-Memory State and UI
    for r in 0 .. rows - 1 do
        for c in 0 .. cols - 1 do
            seats.[r,c] <- false
            seatButtons.[r,c].BackColor <- Color.LightGreen
            
    // Clear Persistence File
    File.WriteAllText(ticketFile, "")
    MessageBox.Show("All seats reset!") |> ignore
)
form.Controls.Add(resetBtn)

// Application Entry Point
[<STAThread>]
Application.EnableVisualStyles()
Application.Run(form)
