using HotelQueue.Models;

namespace HotelQueue.ViewModels;

public class WaitlistViewModel
{
    public List<WaitlistEntry> Waitlist { get; set; } = new();
    public List<WaitlistEntry> WaitlistHistory { get; set; } = new();
    public int TotalCount { get; set; }
    public int VIPCount { get; set; }
    public int RegularCount { get; set; }
    public WaitlistEntry? NextInLine { get; set; }
}

public class CreateWaitlistViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public int RoomId { get; set; }
    public RoomType PreferredRoomType { get; set; } = RoomType.Standard;
    public DateTime? PreferredCheckIn { get; set; }
    public List<Room> AvailableRooms { get; set; } = new();
}

public class WaitlistStatistics
{
    public int TotalWaitlist { get; set; }
    public int VIPWaitlist { get; set; }
    public int RegularWaitlist { get; set; }
    public Dictionary<RoomType, int> WaitlistByRoomType { get; set; } = new();
    public double AverageWaitTime { get; set; }
    public int PromotionsToday { get; set; }
}

public class JoinWaitlistViewModel
{
    public RoomType RoomType { get; set; } = RoomType.Standard;
    public DateTime PreferredCheckIn { get; set; } = DateTime.Now.AddDays(1);
}
