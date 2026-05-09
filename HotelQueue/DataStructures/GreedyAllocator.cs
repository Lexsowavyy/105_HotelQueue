using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public class GreedyAllocator
{
    private readonly List<Room> _availableRooms;
    private readonly PriorityQueue _vipQueue;
    private readonly FIFOQueue _regularQueue;

    public GreedyAllocator(List<Room> availableRooms, PriorityQueue vipQueue, FIFOQueue regularQueue)
    {
        _availableRooms = availableRooms ?? new List<Room>();
        _vipQueue = vipQueue ?? new PriorityQueue();
        _regularQueue = regularQueue ?? new FIFOQueue();
    }

    public AllocationResult AllocateRooms()
    {
        var result = new AllocationResult();
        var tempVIPQueue = new PriorityQueue();
        var tempRegularQueue = new FIFOQueue();

        while (_vipQueue.Count > 0 && _availableRooms.Count(r => r.IsAvailable) > 0)
        {
            var reservation = _vipQueue.Dequeue();
            var allocatedRoom = FindBestRoom(reservation);

            if (allocatedRoom != null)
            {
                result.AllocatedVIPReservations.Add(reservation);
                result.RoomsUsed.Add(allocatedRoom);
                allocatedRoom.IsAvailable = false;
            }
            else
            {
                tempVIPQueue.Enqueue(reservation);
            }
        }

        while (_vipQueue.Count > 0)
        {
            tempVIPQueue.Enqueue(_vipQueue.Dequeue());
        }

        while (_regularQueue.Count > 0 && _availableRooms.Count(r => r.IsAvailable) > 0)
        {
            var reservation = _regularQueue.Dequeue();
            var allocatedRoom = FindBestRoom(reservation);

            if (allocatedRoom != null)
            {
                result.AllocatedRegularReservations.Add(reservation);
                result.RoomsUsed.Add(allocatedRoom);
                allocatedRoom.IsAvailable = false;
            }
            else
            {
                tempRegularQueue.Enqueue(reservation);
            }
        }

        while (_regularQueue.Count > 0)
        {
            tempRegularQueue.Enqueue(_regularQueue.Dequeue());
        }

        result.RemainingVIPQueue = tempVIPQueue;
        result.RemainingRegularQueue = tempRegularQueue;
        result.TotalRoomsAvailable = _availableRooms.Count;
        result.RoomsUtilized = result.RoomsUsed.Count;

        return result;
    }

    private Room? FindBestRoom(Reservation reservation)
    {
        var availableRooms = _availableRooms.Where(r => r.IsAvailable && IsRoomSuitable(r, reservation)).ToList();

        if (!availableRooms.Any())
            return null;

        Room? bestRoom = null;
        decimal minCost = decimal.MaxValue;

        foreach (var room in availableRooms)
        {
            if (room.PricePerNight < minCost)
            {
                minCost = room.PricePerNight;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private bool IsRoomSuitable(Room room, Reservation reservation)
    {
        if (reservation.Room != null && reservation.Room.Type == room.Type)
            return true;

        if (room.Type == RoomType.Suite)
            return true;

        if (room.Type == RoomType.Deluxe && reservation.Room?.Type != RoomType.Suite)
            return true;

        if (room.Type == RoomType.Standard)
            return true;

        return false;
    }

    public Room? ReallocateRoomOnCancellation(List<Room> rooms, Reservation cancelledReservation, 
        PriorityQueue vipQueue, FIFOQueue regularQueue)
    {
        var freedRoom = rooms.FirstOrDefault(r => r.RoomId == cancelledReservation.RoomId);
        if (freedRoom != null)
            freedRoom.IsAvailable = true;

        if (vipQueue.Count > 0)
        {
            var vipReservation = vipQueue.Peek();
            if (vipReservation != null)
            {
                var suitableRoom = rooms.FirstOrDefault(r => r.IsAvailable && IsRoomSuitable(r, vipReservation));

                if (suitableRoom != null)
                {
                    vipQueue.Dequeue();
                    suitableRoom.IsAvailable = false;
                    return suitableRoom;
                }
            }
        }

        if (regularQueue.Count > 0)
        {
            var regularReservation = regularQueue.Peek();
            if (regularReservation != null)
            {
                var suitableRoom = rooms.FirstOrDefault(r => r.IsAvailable && IsRoomSuitable(r, regularReservation));

                if (suitableRoom != null)
                {
                    regularQueue.Dequeue();
                    suitableRoom.IsAvailable = false;
                    return suitableRoom;
                }
            }
        }

        return null;
    }

    public List<(Reservation Reservation, Room Room)> GreedyAllocationByCheckIn(List<Reservation> reservations)
    {
        var sortedReservations = QuickSorter.SortByCheckInDate(reservations);
        var allocation = new List<(Reservation, Room)>();
        var tempRooms = _availableRooms.Select(r => new Room
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            Type = r.Type,
            PricePerNight = r.PricePerNight,
            Capacity = r.Capacity,
            IsAvailable = true
        }).ToList();

        foreach (var reservation in sortedReservations)
        {
            var room = FindBestRoomForDate(tempRooms, reservation);
            if (room != null)
            {
                allocation.Add((reservation, room));
                room.IsAvailable = false;
            }
        }

        return allocation;
    }

    private Room? FindBestRoomForDate(List<Room> rooms, Reservation reservation)
    {
        var availableRooms = rooms.Where(r => r.IsAvailable).ToList();
        
        if (!availableRooms.Any())
            return null;

        return availableRooms.OrderBy(r => r.PricePerNight).FirstOrDefault();
    }
}

public class AllocationResult
{
    public List<Reservation> AllocatedVIPReservations { get; set; } = new();
    public List<Reservation> AllocatedRegularReservations { get; set; } = new();
    public List<Room> RoomsUsed { get; set; } = new();
    public PriorityQueue? RemainingVIPQueue { get; set; }
    public FIFOQueue? RemainingRegularQueue { get; set; }
    public int TotalRoomsAvailable { get; set; }
    public int RoomsUtilized { get; set; }
    public double UtilizationRate => TotalRoomsAvailable > 0 
        ? (double)RoomsUtilized / TotalRoomsAvailable * 100 : 0;
}
