/// <summary>
/// Cinema Reservation System using Windows Forms.
/// Initializes the seat grid, handles booking, ticket generation, and UI events.
/// </summary>
open System
open System.Drawing
open System.Windows.Forms
open System.IO
open CinemaLogic

/// <summary>
/// Number of rows in the cinema.
/// </summary>
let rows = 5

/// <summary>
/// Number of columns in the cinema.
/// </summary>
let cols = 6

/// <summary>
/// 2D array representing the current state of seats.
/// True = booked, False = available.
/// </summary>
let seats = Array2D.create rows cols false

/// <summary>
/// Initialize the main application form for the cinema reservation.
/// </summary>
let form = new Form(Text = "Cinema Seat Reservation", Size = Size(600, 450))

/// <summary>
/// Save a ticket to a separate file with a simple design.
/// </summary>
/// <param name="row">The row of the booked seat.</param>
/// <param name="col">The column of the booked seat.</param>
/// <returns>Returns the formatted ticket information as a string.</returns>
let saveTicket row col =
    let ticketId = CinemaLogic.generateTicketId()
    let ticketInfo =
        sprintf "🎬 Cinema Ticket 🎬\n-------------------\nTicket ID: %s\nRow: %d\nColumn: %d\nEnjoy the movie!" ticketId row col

    /// <summary>
    /// Create "tickets" folder if it doesn't exist.
    /// </summary>
    let folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tickets")
    if not (Directory.Exists(folder)) then
        Directory.CreateDirectory(folder) |> ignore

    /// <summary>
    /// Save ticket info to a unique file based on Ticket ID.
    /// </summary>
    let ticketFile = Path.Combine(folder, sprintf "Ticket-%s.txt" ticketId)
    File.WriteAllText(ticketFile, ticketInfo)
    
    ticketInfo

/// <summary>
/// Create 2D array of buttons representing the seat grid.
/// Each button handles click events to book seats and generate tickets.
/// </summary>
let seatButtons = Array2D.init rows cols (fun r c ->
    /// <summary>
    /// Initialize a button for a specific seat.
    /// </summary>
    let btn = new Button(Text = "", Size = Size(50, 50))
    btn.Location <- Point(c * 55 + 20, r * 55 + 20)
    btn.BackColor <- Color.LightGreen
    
    /// <summary>
    /// Event handler when a seat is clicked.
    /// Checks if seat is already booked, updates UI, and saves ticket.
    /// </summary>
    btn.Click.Add(fun _ ->
        if seats.[r,c] then
            MessageBox.Show("Seat already booked!") |> ignore
        else
            CinemaLogic.bookSeat seats r c |> ignore
            btn.BackColor <- Color.Red
            let ticketInfo = saveTicket r c
            MessageBox.Show(sprintf "Seat booked!\n\n%s" ticketInfo) |> ignore
    )
    btn
)

/// <summary>
/// Add all seat buttons to the main form.
/// </summary>
for r in 0 .. rows - 1 do
    for c in 0 .. cols - 1 do
        form.Controls.Add(seatButtons.[r,c])

/// <summary>
/// Button to reset all seats to available and delete ticket files.
/// </summary>
let resetBtn = new Button(Text = "Reset All", Size = Size(100, 40))
resetBtn.Location <- Point(20, rows * 55 + 30)
resetBtn.BackColor <- Color.LightBlue
resetBtn.Click.Add(fun _ ->
    /// <summary>
    /// Reset in-memory seats and button colors.
    /// </summary>
    for r in 0 .. rows - 1 do
        for c in 0 .. cols - 1 do
            seats.[r,c] <- false
            seatButtons.[r,c].BackColor <- Color.LightGreen

    /// <summary>
    /// Delete all ticket files in the "tickets" folder.
    /// </summary>
    let folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tickets")
    if Directory.Exists(folder) then
        Directory.GetFiles(folder)
        |> Array.iter File.Delete
    MessageBox.Show("All seats reset!") |> ignore
)
form.Controls.Add(resetBtn)

/// <summary>
/// Application Entry Point.
/// Enables visual styles and runs the main form.
/// </summary>
[<STAThread>]
Application.EnableVisualStyles()
Application.Run(form)
