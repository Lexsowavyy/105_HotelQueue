using HotelQueue.Data;
using HotelQueue.Models;

namespace HotelQueue.Services;

public class RoomService
{
    private readonly HotelDbContext _context;

    public RoomService(HotelDbContext context)
    {
        _context = context;
        
        if (!_context.Rooms.Any())
        {
            SeedRooms();
        }
    }

    private void SeedRooms()
    {
        var rooms = new List<Room>
        {
            new Room { RoomNumber = "101", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "102", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "103", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "104", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 3, IsAvailable = true },
            new Room { RoomNumber = "105", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "201", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "202", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "203", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 3, IsAvailable = true },
            new Room { RoomNumber = "204", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomNumber = "205", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 3, IsAvailable = true },
            new Room { RoomNumber = "301", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomNumber = "302", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomNumber = "303", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomNumber = "304", Type = RoomType.Suite, PricePerNight = 20000m, Capacity = 4, IsAvailable = true },
            new Room { RoomNumber = "305", Type = RoomType.Suite, PricePerNight = 20000m, Capacity = 4, IsAvailable = true }
        };

        _context.Rooms.AddRange(rooms);
        _context.SaveChanges();
    }

    public Room? GetRoom(int roomId)
    {
        return _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
    }

    public Room? GetRoomByNumber(string roomNumber)
    {
        return _context.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
    }

    public List<Room> GetAllRooms()
    {
        return _context.Rooms.ToList();
    }

    public List<Room> GetAvailableRooms()
    {
        return _context.Rooms.Where(r => r.IsAvailable).ToList();
    }

    public List<Room> GetUnavailableRooms()
    {
        return _context.Rooms.Where(r => !r.IsAvailable).ToList();
    }

    public List<Room> GetRoomsByType(RoomType type)
    {
        return _context.Rooms.Where(r => r.Type == type).ToList();
    }

    public List<Room> GetAvailableRoomsByType(RoomType type)
    {
        return _context.Rooms.Where(r => r.Type == type && r.IsAvailable).ToList();
    }

    public Room AddRoom(string roomNumber, RoomType type, decimal pricePerNight, int capacity)
    {
        if (_context.Rooms.Any(r => r.RoomNumber == roomNumber))
        {
            throw new InvalidOperationException("Room number already exists");
        }

        var room = new Room
        {
            RoomNumber = roomNumber,
            Type = type,
            PricePerNight = pricePerNight,
            Capacity = capacity,
            IsAvailable = true
        };

        _context.Rooms.Add(room);
        _context.SaveChanges();
        return room;
    }

    public bool UpdateRoom(int roomId, string? roomNumber = null, RoomType? type = null,
        decimal? pricePerNight = null, int? capacity = null)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return false;

        if (roomNumber != null && _context.Rooms.Any(r => r.RoomNumber == roomNumber && r.RoomId != roomId))
        {
            throw new InvalidOperationException("Room number already exists");
        }

        if (roomNumber != null) room.RoomNumber = roomNumber;
        if (type.HasValue) room.Type = type.Value;
        if (pricePerNight.HasValue) room.PricePerNight = pricePerNight.Value;
        if (capacity.HasValue) room.Capacity = capacity.Value;

        _context.SaveChanges();
        return true;
    }

    public bool DeleteRoom(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return false;

        if (!room.IsAvailable)
        {
            throw new InvalidOperationException("Cannot delete a room that is currently booked");
        }

        _context.Rooms.Remove(room);
        _context.SaveChanges();
        return true;
    }

    public bool MarkRoomUnavailable(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return false;

        room.IsAvailable = false;
        _context.SaveChanges();
        return true;
    }

    public bool MarkRoomAvailable(int roomId)
    {
        var room = GetRoom(roomId);
        if (room == null)
            return false;

        room.IsAvailable = true;
        _context.SaveChanges();
        return true;
    }

    public int GetTotalRooms()
    {
        return _context.Rooms.Count();
    }

    public int GetAvailableRoomCount()
    {
        return _context.Rooms.Count(r => r.IsAvailable);
    }

    public int GetUnavailableRoomCount()
    {
        return _context.Rooms.Count(r => !r.IsAvailable);
    }

    public Dictionary<RoomType, int> GetRoomCountByType()
    {
        return _context.Rooms
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public Dictionary<RoomType, int> GetAvailableRoomCountByType()
    {
        return _context.Rooms
            .Where(r => r.IsAvailable)
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public decimal GetAveragePrice()
    {
        return _context.Rooms.Any() ? _context.Rooms.Average(r => r.PricePerNight) : 0;
    }

    public Dictionary<RoomType, decimal> GetAveragePriceByType()
    {
        return _context.Rooms
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Average(r => r.PricePerNight));
    }

    public List<Room> SearchRooms(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return _context.Rooms
            .Where(r => r.RoomNumber.ToLower().Contains(searchTerm) ||
                        r.Type.ToString().ToLower().Contains(searchTerm))
            .ToList();
    }
}
