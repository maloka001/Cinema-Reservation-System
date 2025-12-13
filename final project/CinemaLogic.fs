/// <summary>
/// Contains the core business logic for the Cinema Reservation System.
/// Handles seat booking validation and ticket generation.
/// </summary>
module CinemaLogic

open System
open System.IO

/// <summary>
/// Attempts to book a specific seat in the cinema.
/// </summary>
/// <param name="seats">The 2D array representing the current state of all seats.</param>
/// <param name="row">The row index of the seat to book.</param>
/// <param name="col">The column index of the seat to book.</param>
/// <returns>
/// True if the booking was successful (seat was free).
/// False if the seat was already booked.
/// </returns>
let bookSeat (seats: bool[,]) row col =
    if seats.[row, col] then
        false
    else
        seats.[row, col] <- true
        true

/// <summary>
/// Generates a unique identifier for a ticket.
/// </summary>
/// <returns>A string representation of a new GUID.</returns>
let generateTicketId () = Guid.NewGuid().ToString()

/// <summary>
/// Persists ticket information to a file and returns the ticket details.
/// </summary>
/// <param name="row">The row index of the booked seat.</param>
/// <param name="col">The column index of the booked seat.</param>
/// <param name="ticketFile">The file path where the ticket should be saved.</param>
/// <returns>A formatted string containing the Ticket ID and seat location.</returns>
let saveTicket row col ticketFile =
    let ticketId = generateTicketId()
    let ticketInfo = sprintf "Ticket ID: %s | Row: %d, Column: %d" ticketId row col
    File.AppendAllText(ticketFile, ticketInfo + Environment.NewLine)
    ticketInfo
