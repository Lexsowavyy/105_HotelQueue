using HotelQueue.DataStructures;
using HotelQueue.Models;
using HotelQueue.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;
    private readonly ReservationService _reservationService;
    private readonly NotificationService _notificationService;

    public DashboardController(
        DashboardService dashboardService,
        ReservationService reservationService,
        NotificationService notificationService)
    {
        _dashboardService = dashboardService;
        _reservationService = reservationService;
        _notificationService = notificationService;
    }

    public IActionResult Index(string? dateFilter, DateTime? startDate, DateTime? endDate)
    {
        DateTime? filterStartDate = null;
        DateTime? filterEndDate = null;

        if (!string.IsNullOrEmpty(dateFilter))
        {
            ViewBag.DateFilter = dateFilter;
            var today = DateTime.Today;
            switch (dateFilter.ToLower())
            {
                case "today":
                    filterStartDate = today;
                    filterEndDate = today.AddDays(1).AddSeconds(-1);
                    break;
                case "week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
                    filterStartDate = startOfWeek;
                    filterEndDate = endOfWeek;
                    break;
                case "month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);
                    filterStartDate = startOfMonth;
                    filterEndDate = endOfMonth;
                    break;
                case "year":
                    var startOfYear = new DateTime(today.Year, 1, 1);
                    var endOfYear = new DateTime(today.Year, 12, 31, 23, 59, 59);
                    filterStartDate = startOfYear;
                    filterEndDate = endOfYear;
                    break;
            }
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            filterStartDate = startDate.Value;
            filterEndDate = endDate.Value.AddDays(1).AddSeconds(-1);
        }

        var stats = _dashboardService.GetDashboardStatistics(filterStartDate, filterEndDate);
        var model = new DashboardIndexViewModel
        {
            TotalReservations = stats.TotalReservations,
            PendingReservations = stats.PendingReservations,
            ConfirmedReservations = stats.ConfirmedReservations,
            CancelledReservations = stats.CancelledReservations,
            CompletedReservations = stats.CompletedReservations,
            ExpiredReservations = stats.ExpiredReservations,
            VIPReservations = stats.VIPReservations,
            RegularReservations = stats.RegularReservations,
            TotalRooms = stats.TotalRooms,
            AvailableRooms = stats.AvailableRooms,
            OccupiedRooms = stats.OccupiedRooms,
            VIPQueueCount = stats.TotalVIPInQueue,
            RegularQueueCount = stats.TotalRegularInQueue,
            WaitlistCount = stats.WaitlistCount,
            UnreadNotifications = stats.UnreadNotifications,
            Revenue = stats.Revenue,
            OccupancyRate = stats.OccupancyRate,
            VIPPercentage = stats.VIPPercentage,
            RecentReservations = _dashboardService.GetRecentReservations(10),
            Rooms = _dashboardService.GetRecentRooms(),
            RecentNotifications = _notificationService.GetAllNotifications().Take(5).ToList(),
            ReservationsByStatus = _dashboardService.GetReservationsByStatus(),
            RoomOccupancyByType = _dashboardService.GetRoomOccupancyByType()
        };

        return View(model);
    }

    public IActionResult Statistics()
    {
        var stats = _dashboardService.GetDashboardStatistics();
        var model = new DashboardIndexViewModel
        {
            TotalReservations = stats.TotalReservations,
            PendingReservations = stats.PendingReservations,
            ConfirmedReservations = stats.ConfirmedReservations,
            CancelledReservations = stats.CancelledReservations,
            CompletedReservations = stats.CompletedReservations,
            ExpiredReservations = stats.ExpiredReservations,
            VIPReservations = stats.VIPReservations,
            RegularReservations = stats.RegularReservations,
            TotalRooms = stats.TotalRooms,
            AvailableRooms = stats.AvailableRooms,
            OccupiedRooms = stats.OccupiedRooms,
            VIPQueueCount = stats.TotalVIPInQueue,
            RegularQueueCount = stats.TotalRegularInQueue,
            WaitlistCount = stats.WaitlistCount,
            UnreadNotifications = stats.UnreadNotifications,
            Revenue = stats.Revenue,
            OccupancyRate = stats.OccupancyRate,
            VIPPercentage = stats.VIPPercentage
        };
        return View(model);
    }

    public IActionResult Performance(int dataSize = 100)
    {
        var performance = _dashboardService.GetPerformanceComparison(dataSize);
        return View(performance);
    }

    public IActionResult QueueOverview()
    {
        var allocationResult = _dashboardService.ProcessQueueAllocation();
        return View(allocationResult);
    }
}

public class DashboardIndexViewModel
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
    public List<Reservation> RecentReservations { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Notification> RecentNotifications { get; set; } = new();
    public Dictionary<string, int> ReservationsByStatus { get; set; } = new();
    public Dictionary<string, int> RoomOccupancyByType { get; set; } = new();
}
