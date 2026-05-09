using HotelQueue.Models;

namespace HotelQueue.ViewModels;

public class DashboardViewModel
{
    public DashboardStatistics Statistics { get; set; } = new();
    public List<Reservation> RecentReservations { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Notification> RecentNotifications { get; set; } = new();
    public Dictionary<string, int> ReservationsByStatus { get; set; } = new();
    public Dictionary<string, int> RoomOccupancyByType { get; set; } = new();
    public PerformanceComparison? PerformanceData { get; set; }
}

public class DashboardStatistics
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int CompletedReservations { get; set; }
    public int ExpiredReservations { get; set; }
    public int VIPReservations { get; set; }
    public int RegularReservations { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int VIPQueueCount { get; set; }
    public int RegularQueueCount { get; set; }
    public int WaitlistCount { get; set; }
    public int UnreadNotifications { get; set; }
    public decimal Revenue { get; set; }
    public double OccupancyRate { get; set; }
    public double VIPPercentage { get; set; }
}

public class PerformanceComparison
{
    public double SortTimeMs { get; set; }
    public int Comparisons { get; set; }
    public int Swaps { get; set; }
    public int DataSize { get; set; }
    public double ImprovementRatio { get; set; }
}
