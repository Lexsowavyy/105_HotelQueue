using HotelQueue.Data;
using HotelQueue.DataStructures;
using HotelQueue.DataStructures.Baseline;
using HotelQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelQueue.Services;

public class BaselineReservationService
{
    private readonly HotelDbContext _context;
    private readonly BaselineSingleQueue _singleQueue;
    private readonly BaselineLinearSearcher _linearSearcher;
    private readonly BaselineBasicSorter _basicSorter;
    private readonly RoomService _roomService;

    public BaselineReservationService(
        HotelDbContext context,
        BaselineSingleQueue singleQueue,
        RoomService roomService)
    {
        _context = context;
        _singleQueue = singleQueue;
        _linearSearcher = new BaselineLinearSearcher();
        _basicSorter = new BaselineBasicSorter();
        _roomService = roomService;
    }

    public (Reservation? Reservation, WaitlistEntry? WaitlistEntry, bool IsWaitlist) CreateReservation(
        string customerName, string customerEmail, string customerPhone,
        CustomerType customerType, int roomId, DateTime checkIn, DateTime checkOut)
    {
        var room = _roomService.GetRoom(roomId);
        if (room == null || !room.IsAvailable)
        {
            var waitlistCustomer = new Customer(customerName, customerEmail, customerPhone, customerType);
            _context.Customers.Add(waitlistCustomer);
            _context.SaveChanges();

            var waitlistReservationId = $"RES-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var waitlistReservation = new Reservation(waitlistReservationId, waitlistCustomer.CustomerId, roomId, checkIn, checkOut, customerType)
            {
                Customer = waitlistCustomer,
                Room = room,
                Status = ReservationStatus.OnWaitlist
            };

            _context.Reservations.Add(waitlistReservation);
            _context.SaveChanges();

            WaitlistEntry? waitlistEntry = null;
            try
            {
                var waitlistService = new WaitlistService(_context);
                waitlistEntry = waitlistService.AddToWaitlist(waitlistCustomer.CustomerId, roomId, room?.Type ?? RoomType.Standard, checkIn, customerType);
            }
            catch { }

            return (waitlistReservation, waitlistEntry, true);
        }

        var customer = new Customer(customerName, customerEmail, customerPhone, customerType);
        _context.Customers.Add(customer);
        _context.SaveChanges();

        var reservationId = $"RES-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var reservation = new Reservation(reservationId, customer.CustomerId, roomId, checkIn, checkOut, customerType)
        {
            Customer = customer,
            Room = room,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        _singleQueue.Enqueue(reservation);
        _roomService.MarkRoomUnavailable(roomId);

        return (reservation, null, false);
    }

    public bool ConfirmReservation(string reservationId)
    {
        var allReservations = _context.Reservations
            .Include(r => r.Customer).Include(r => r.Room).ToList();
        var reservation = _linearSearcher.FindById(allReservations, reservationId);

        if (reservation == null) return false;

        reservation.Status = ReservationStatus.Confirmed;
        reservation.ConfirmedAt = DateTime.Now;
        _context.SaveChanges();

        _singleQueue.Remove(reservationId);

        return true;
    }

    public bool CancelReservation(string reservationId)
    {
        var allReservations = _context.Reservations
            .Include(r => r.Customer).Include(r => r.Room).ToList();
        var reservation = _linearSearcher.FindById(allReservations, reservationId);

        if (reservation == null) return false;

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.Now;
        _context.SaveChanges();

        _singleQueue.Remove(reservationId);

        var freedRoom = _roomService.GetRoom(reservation.RoomId);
        if (freedRoom != null)
            _roomService.MarkRoomAvailable(reservation.RoomId);

        return true;
    }

    public List<Reservation> GetAllReservations()
    {
        return _context.Reservations
            .Include(r => r.Customer).Include(r => r.Room)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public List<Reservation> SearchReservations(string searchTerm)
    {
        var all = GetAllReservations();
        return _linearSearcher.Search(all, searchTerm);
    }

    public List<Reservation> GetSortedByCheckIn(bool ascending = true)
    {
        var all = GetAllReservations();
        return _basicSorter.BubbleSortByCheckIn(all, ascending);
    }

    public List<Reservation> GetSortedByCreatedAt(bool ascending = true)
    {
        var all = GetAllReservations();
        return _basicSorter.SelectionSortByCreatedAt(all, ascending);
    }

    public List<Reservation> GetReservationsInDateRange(DateTime start, DateTime end)
    {
        var all = GetAllReservations();
        return _linearSearcher.FindByDateRange(all, start, end);
    }

    public PerformanceMetrics MeasureBaselineSortPerformance(List<Reservation> reservations)
    {
        return _basicSorter.MeasureSortPerformance(reservations);
    }

    public BaselineAllocationResult ProcessAllocation()
    {
        var rooms = _roomService.GetAvailableRooms().Select(r => new Room
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            Type = r.Type,
            PricePerNight = r.PricePerNight,
            Capacity = r.Capacity,
            IsAvailable = true
        }).ToList();

        var allocator = new BaselineSequentialAllocator(_singleQueue, rooms);
        var (allocated, roomsUsed) = allocator.AllocateRooms();

        return new BaselineAllocationResult
        {
            AllocatedReservations = allocated,
            RoomsUsed = roomsUsed,
            TotalRoomsAvailable = rooms.Count,
            RoomsUtilized = roomsUsed.Count
        };
    }
}

public class BaselineAllocationResult
{
    public List<Reservation> AllocatedReservations { get; set; } = new();
    public List<Room> RoomsUsed { get; set; } = new();
    public int TotalRoomsAvailable { get; set; }
    public int RoomsUtilized { get; set; }
    public double UtilizationRate => TotalRoomsAvailable > 0 ? (double)RoomsUtilized / TotalRoomsAvailable * 100 : 0;
}
