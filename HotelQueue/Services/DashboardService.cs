using HotelQueue.DataStructures;
using HotelQueue.Models;

namespace HotelQueue.Services;

public class DashboardService
{
    private readonly ReservationService _reservationService;
    private readonly RoomService _roomService;
    private readonly WaitlistService _waitlistService;
    private readonly NotificationService _notificationService;

    public DashboardService(
        ReservationService reservationService,
        RoomService roomService,
        WaitlistService waitlistService,
        NotificationService notificationService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
        _waitlistService = waitlistService;
        _notificationService = notificationService;
    }

    public DashboardStatistics GetDashboardStatistics(DateTime? startDate = null, DateTime? endDate = null)
    {
        _reservationService.CompleteExpiredCheckouts();
        
        var reservations = _reservationService.GetAllReservations();
        
        if (startDate.HasValue && endDate.HasValue)
        {
            reservations = reservations.Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate).ToList();
        }

        var rooms = _roomService.GetAllRooms();

        return new DashboardStatistics
        {
            TotalReservations = reservations.Count,
            PendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending),
            ConfirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
            CancelledReservations = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
            CompletedReservations = reservations.Count(r => r.Status == ReservationStatus.Completed),
            ExpiredReservations = reservations.Count(r => r.Status == ReservationStatus.Expired),
            VIPReservations = reservations.Count(r => r.CustomerType == CustomerType.VIP),
            RegularReservations = reservations.Count(r => r.CustomerType == CustomerType.Regular),
            TotalRooms = rooms.Count,
            AvailableRooms = rooms.Count(r => r.IsAvailable),
            OccupiedRooms = rooms.Count(r => !r.IsAvailable),
            TotalVIPInQueue = _waitlistService.GetWaitlistByType(CustomerType.VIP).Count,
            TotalRegularInQueue = _waitlistService.GetWaitlistByType(CustomerType.Regular).Count,
            WaitlistCount = _waitlistService.Count,
            UnreadNotifications = _notificationService.GetUnreadCount(),
            Revenue = reservations
                .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
                .Where(r => r.Room != null)
                .Sum(r => r.Room!.PricePerNight * r.TotalNights)
        };
    }

    public Dictionary<string, int> GetReservationsByStatus()
    {
        return _reservationService.GetAllReservations()
            .GroupBy(r => r.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public Dictionary<string, int> GetRoomOccupancyByType()
    {
        return _roomService.GetAllRooms()
            .GroupBy(r => r.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count(r => !r.IsAvailable));
    }

    public Dictionary<string, int> GetAvailableRoomsByType()
    {
        return _roomService.GetAllRooms()
            .Where(r => r.IsAvailable)
            .GroupBy(r => r.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public List<Reservation> GetRecentReservations(int count = 10)
    {
        return _reservationService.GetAllReservations()
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToList();
    }

    public List<Room> GetRecentRooms()
    {
        return _roomService.GetAllRooms().Take(10).ToList();
    }

    public PerformanceComparison GetPerformanceComparison(int dataSize)
    {
        var sortMetrics = _reservationService.GetSortPerformance("CreatedDate", dataSize);

        return new PerformanceComparison
        {
            SortExecutionTimeMs = sortMetrics.ExecutionTimeMs,
            SortComparisons = sortMetrics.Comparisons,
            SortSwaps = sortMetrics.Swaps,
            DataSize = dataSize,
            BaselineTimeO_n2 = dataSize * dataSize,
            OptimizedTimeO_nlogn = (int)(dataSize * Math.Log(dataSize, 2))
        };
    }

    public AllocationResult ProcessQueueAllocation()
    {
        return _reservationService.ProcessAllQueues();
    }
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
    public int TotalVIPInQueue { get; set; }
    public int TotalRegularInQueue { get; set; }
    public int WaitlistCount { get; set; }
    public int UnreadNotifications { get; set; }
    public decimal Revenue { get; set; }

    public double OccupancyRate => TotalRooms > 0 ? (double)OccupiedRooms / TotalRooms * 100 : 0;
    public double VIPPercentage => TotalReservations > 0 ? (double)VIPReservations / TotalReservations * 100 : 0;
}

public class PerformanceComparison
{
    public double SortExecutionTimeMs { get; set; }
    public int SortComparisons { get; set; }
    public int SortSwaps { get; set; }
    public int DataSize { get; set; }
    public int BaselineTimeO_n2 { get; set; }
    public int OptimizedTimeO_nlogn { get; set; }

    public double ImprovementRatio => BaselineTimeO_n2 > 0 ? (double)BaselineTimeO_n2 / OptimizedTimeO_nlogn : 0;
}
