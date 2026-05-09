using HotelQueue.DataStructures;
using HotelQueue.Models;

namespace HotelQueue.ViewModels;

public class ReservationViewModel
{
    public string ReservationId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public int RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public RoomType RoomType { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalPrice { get; set; }
    public List<Reservation> VIPQueue { get; set; } = new();
    public List<Reservation> RegularQueue { get; set; } = new();
    public List<Reservation> AllReservations { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; } = true;
}

public class CreateReservationViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public int RoomId { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public List<Room> AvailableRooms { get; set; } = new();
    public List<Room> AllRooms { get; set; } = new();
    public Dictionary<int, List<DateRange>> RoomBookings { get; set; } = new();
}

public class DateRange
{
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}

public class ReservationSearchViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public List<Reservation> Results { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReservationStatus? Status { get; set; }
    public CustomerType? CustomerType { get; set; }
}

public class ReservationQueueViewModel
{
    public List<Reservation> VIPQueue { get; set; } = new();
    public List<Reservation> RegularQueue { get; set; } = new();
    public int TotalVIPCount { get; set; }
    public int TotalRegularCount { get; set; }
    public AllocationResult? LastAllocationResult { get; set; }
}

public class ReservationStatisticsViewModel
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int VIPReservations { get; set; }
    public int RegularReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal VIPRevenue { get; set; }
    public decimal RegularRevenue { get; set; }
}
