using HotelQueue.Models;

namespace HotelQueue.ViewModels;

public class RoomViewModel
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
    public int Capacity { get; set; }
    public List<Room> AllRooms { get; set; } = new();
    public List<Room> AvailableRooms { get; set; } = new();
    public List<Room> OccupiedRooms { get; set; } = new();
    public Dictionary<RoomType, int> RoomCountByType { get; set; } = new();
    public Dictionary<RoomType, int> AvailableCountByType { get; set; } = new();
}

public class CreateRoomViewModel
{
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType Type { get; set; } = RoomType.Standard;
    public decimal PricePerNight { get; set; } = 100.00m;
    public int Capacity { get; set; } = 2;
}

public class RoomTypeStatistics
{
    public RoomType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalPotentialRevenue { get; set; }
    public double OccupancyRate { get; set; }
}
