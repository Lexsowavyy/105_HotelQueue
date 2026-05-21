using HotelQueue.Data;
using HotelQueue.DataStructures;
using HotelQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelQueue.Services;

public class ReservationService
{
    private readonly HotelDbContext _context;
    private readonly ReservationHashTable _reservationHashTable;
    private readonly PriorityQueue _vipQueue;
    private readonly FIFOQueue _regularQueue;
    private readonly RoomService _roomService;
    private readonly NotificationService _notificationService;
    private readonly WaitlistService _waitlistService;
    private bool _initialized = false;
    private readonly object _initLock = new();

    public ReservationService(
        HotelDbContext context,
        ReservationHashTable reservationHashTable,
        PriorityQueue vipQueue,
        FIFOQueue regularQueue,
        RoomService roomService,
        NotificationService notificationService,
        WaitlistService waitlistService)
    {
        _context = context;
        _reservationHashTable = reservationHashTable;
        _vipQueue = vipQueue;
        _regularQueue = regularQueue;
        _roomService = roomService;
        _notificationService = notificationService;
        _waitlistService = waitlistService;
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;
            var all = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .ToList();
            foreach (var r in all)
            {
                if (!_reservationHashTable.Contains(r.ReservationId))
                {
                    _reservationHashTable.Add(r);
                    if (r.Status == ReservationStatus.Pending)
                    {
                        if (r.CustomerType == CustomerType.VIP)
                            _vipQueue.Enqueue(r);
                        else
                            _regularQueue.Enqueue(r);
                    }
                }
            }
            _initialized = true;
        }
    }

    public (Reservation? Reservation, WaitlistEntry? WaitlistEntry, bool IsWaitlist) CreateReservation(string customerName, string customerEmail, string customerPhone,
        CustomerType customerType, int roomId, DateTime checkIn, DateTime checkOut)
    {
        var room = _roomService.GetRoom(roomId);
        if (room == null || !room.IsAvailable)
        {
            var waitlistCustomer = new Customer(customerName, customerEmail, customerPhone, customerType);
            _context.Customers.Add(waitlistCustomer);
            _context.SaveChanges();

            var waitlistReservationId = GenerateReservationId();
            var waitlistReservation = new Reservation(waitlistReservationId, waitlistCustomer.CustomerId, roomId, checkIn, checkOut, customerType)
            {
                Customer = waitlistCustomer,
                Room = room,
                Status = ReservationStatus.OnWaitlist
            };

            _context.Reservations.Add(waitlistReservation);
            _context.SaveChanges();

            var waitlistEntry = _waitlistService.AddToWaitlist(waitlistCustomer.CustomerId, roomId, room?.Type ?? RoomType.Standard, checkIn, customerType);
            return (waitlistReservation, waitlistEntry, true);
        }

        var customer = new Customer(customerName, customerEmail, customerPhone, customerType);
        _context.Customers.Add(customer);
        _context.SaveChanges();

        var reservationId = GenerateReservationId();
        var reservation = new Reservation(reservationId, customer.CustomerId, roomId, checkIn, checkOut, customerType)
        {
            Customer = customer,
            Room = room,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        _reservationHashTable.Add(reservation);

        if (customerType == CustomerType.VIP)
        {
            _vipQueue.Enqueue(reservation);
        }
        else
        {
            _regularQueue.Enqueue(reservation);
        }

        _roomService.MarkRoomUnavailable(roomId);

        return (reservation, null, false);
    }

    public bool ConfirmReservation(string reservationId)
    {
        // Primary path: resolve from in-memory hash table (O(1))
        EnsureInitialized();
        var reservation = _reservationHashTable.Get(reservationId)
            ?? _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.Confirmed;
        reservation.ConfirmedAt = DateTime.Now;
        _context.SaveChanges();

        _reservationHashTable.Update(reservation);

        if (reservation.CustomerType == CustomerType.VIP)
        {
            _vipQueue.Remove(reservationId);
        }
        else
        {
            _regularQueue.Remove(reservationId);
        }

        _notificationService.SendConfirmation(reservation);

        return true;
    }

    public bool CancelReservation(string reservationId)
    {
        // Primary path: resolve from in-memory hash table (O(1))
        EnsureInitialized();
        var reservation = _reservationHashTable.Get(reservationId)
            ?? _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.Now;
        _context.SaveChanges();

        _reservationHashTable.Update(reservation);

        if (reservation.CustomerType == CustomerType.VIP)
        {
            _vipQueue.Remove(reservationId);
        }
        else
        {
            _regularQueue.Remove(reservationId);
        }

        var freedRoom = _roomService.GetRoom(reservation.RoomId);
        if (freedRoom != null)
            _roomService.MarkRoomAvailable(reservation.RoomId);

        PromoteFromWaitlist(reservation.RoomId);

        _notificationService.SendCancellation(reservation);

        return true;
    }

    public Reservation? GetReservationById(string reservationId)
    {
        EnsureInitialized();
        var result = _reservationHashTable.Get(reservationId);
        if (result != null) return result;

        return _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Room)
            .FirstOrDefault(r => r.ReservationId == reservationId);
    }

    public Reservation? GetReservation(string reservationId)
    {
        EnsureInitialized();
        var result = _reservationHashTable.Get(reservationId);
        if (result != null) return result;

        return _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Room)
            .FirstOrDefault(r => r.ReservationId == reservationId);
    }

    public bool ArchiveReservation(string reservationId)
    {
        // Primary path: resolve from in-memory hash table (O(1))
        EnsureInitialized();
        var reservation = _reservationHashTable.Get(reservationId)
            ?? _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.Archived;
        _context.SaveChanges();

        // Keep hash table in sync
        _reservationHashTable.Update(reservation);

        _notificationService.SendArchive(reservation);

        return true;
    }

    public bool UpdateReservation(string reservationId, string customerName, string customerEmail, 
        string customerPhone, CustomerType customerType, int roomId, DateTime checkIn, DateTime checkOut)
    {
        // Primary path: resolve from in-memory hash table (O(1))
        EnsureInitialized();
        var reservation = _reservationHashTable.Get(reservationId)
            ?? _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

        if (reservation == null)
            return false;

        var room = _roomService.GetRoom(roomId);

        if (reservation.Customer != null)
        {
            reservation.Customer.Name = customerName;
            reservation.Customer.Email = customerEmail;
            reservation.Customer.Phone = customerPhone;
            reservation.Customer.Type = customerType;
        }

        reservation.RoomId = roomId;
        reservation.Room = room;
        reservation.CheckInDate = checkIn;
        reservation.CheckOutDate = checkOut;
        reservation.CustomerType = customerType;

        _context.SaveChanges();
        _reservationHashTable.Update(reservation);

        return true;
    }

    public bool RestoreReservation(string reservationId)
    {
        // Primary path: resolve from in-memory hash table (O(1))
        EnsureInitialized();
        var reservation = _reservationHashTable.Get(reservationId)
            ?? _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.Pending;
        _context.SaveChanges();

        // Re-add to hash table (handles both new and existing entries)
        if (!_reservationHashTable.Contains(reservationId))
            _reservationHashTable.Add(reservation);
        else
            _reservationHashTable.Update(reservation);

        if (reservation.CustomerType == CustomerType.VIP)
        {
            _vipQueue.Enqueue(reservation);
        }
        else
        {
            _regularQueue.Enqueue(reservation);
        }

        return true;
    }

    private void PromoteFromWaitlist(int roomId)
    {
        var room = _roomService.GetRoom(roomId);
        if (room == null)
            return;

        var promotedEntry = _waitlistService.PromoteNext(roomId);
        if (promotedEntry != null && promotedEntry.Customer != null)
        {
            try
            {
                var checkIn = promotedEntry.PreferredCheckIn > DateTime.Now 
                    ? promotedEntry.PreferredCheckIn 
                    : DateTime.Now.AddHours(2);
                var checkOut = checkIn.AddDays(1);

                var (reservation, _, _) = CreateReservation(
                    promotedEntry.Customer.Name,
                    promotedEntry.Customer.Email,
                    promotedEntry.Customer.Phone,
                    promotedEntry.CustomerType,
                    roomId,
                    checkIn,
                    checkOut
                );
                
                if (reservation != null)
                {
                    ConfirmReservation(reservation.ReservationId);
                    _notificationService.SendWaitlistPromotion(reservation, room);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error promoting from waitlist: {ex.Message}");
            }
        }
    }

    public void CheckAndPromoteFromWaitlist()
    {
        var availableRooms = _roomService.GetAvailableRooms();
        foreach (var room in availableRooms)
        {
            PromoteFromWaitlist(room.RoomId);
        }
    }

    public int CompleteExpiredCheckouts()
    {
        EnsureInitialized();
        var now = DateTime.Now;
        var confirmedReservations = _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Room)
            .Where(r => r.Status == ReservationStatus.Confirmed && r.CheckOutDate <= now)
            .ToList();

        int completedCount = 0;
        foreach (var reservation in confirmedReservations)
        {
            reservation.Status = ReservationStatus.Completed;
            _context.SaveChanges();
            _reservationHashTable.Update(reservation);

            _roomService.MarkRoomAvailable(reservation.RoomId);

            PromoteFromWaitlist(reservation.RoomId);
            completedCount++;
        }

        return completedCount;
    }

    public List<Reservation> GetRoomBookings(int roomId)
    {
        EnsureInitialized();
        var roomBookings = _reservationHashTable.GetByRoom(roomId);
        return roomBookings
            .Where(r => r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Archived &&
                        r.CheckOutDate >= DateTime.Now)
            .OrderBy(r => r.CheckInDate)
            .ToList();
    }

    public bool IsDateAvailableForRoom(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var existingBookings = GetRoomBookings(roomId);
        
        foreach (var booking in existingBookings)
        {
            if (checkIn < booking.CheckOutDate && checkOut > booking.CheckInDate)
            {
                return false;
            }
        }
        return true;
    }

    public List<Reservation> GetAllReservations()
    {
        EnsureInitialized();
        var all = _reservationHashTable.GetAll();
        return QuickSorter.SortByCreatedDate(all, ascending: false);
    }

    public List<Reservation> GetReservationsByStatus(ReservationStatus status)
    {
        EnsureInitialized();
        var byStatus = _reservationHashTable.GetByStatus(status);
        return QuickSorter.SortByCreatedDate(byStatus, ascending: false);
    }

    public List<Reservation> GetVIPQueueReservations()
    {
        EnsureInitialized();
        var allVip = _reservationHashTable.GetByStatus(ReservationStatus.Pending)
            .Where(r => r.CustomerType == CustomerType.VIP)
            .ToList();
        return QuickSorter.SortByCreatedDate(allVip, ascending: false);
    }

    public List<Reservation> GetRegularQueueReservations()
    {
        EnsureInitialized();
        var allRegular = _reservationHashTable.GetByStatus(ReservationStatus.Pending)
            .Where(r => r.CustomerType == CustomerType.Regular)
            .ToList();
        return QuickSorter.SortByCreatedDate(allRegular, ascending: false);
    }

    public List<Reservation> SearchReservations(string searchTerm)
    {
        EnsureInitialized();
        return _reservationHashTable.Search(searchTerm);
    }

    public List<Reservation> GetReservationsInDateRange(DateTime startDate, DateTime endDate)
    {
        EnsureInitialized();
        var all = _reservationHashTable.GetAll();
        return BinarySearcher.FindReservationsInDateRange(all, startDate, endDate);
    }

    public List<Reservation> GetSortedReservations(string sortBy, bool ascending = true)
    {
        var reservations = GetAllReservations();
        return sortBy.ToLower() switch
        {
            "checkindate" => QuickSorter.SortByCheckInDate(reservations, ascending),
            "createddate" => QuickSorter.SortByCreatedDate(reservations, ascending),
            "priority" => QuickSorter.SortByPriority(reservations, ascending),
            "totalprice" => QuickSorter.SortByTotalPrice(reservations, ascending),
            "status" => QuickSorter.SortByStatus(reservations, ascending),
            "customertype" => QuickSorter.SortByCustomerType(reservations, ascending),
            _ => QuickSorter.MultiKeySort(reservations, ascending)
        };
    }

    public void ExpireOldReservations(TimeSpan expirationTime)
    {
        var now = DateTime.Now;
        var pendingReservations = GetReservationsByStatus(ReservationStatus.Pending);

        foreach (var reservation in pendingReservations)
        {
            if (now - reservation.CreatedAt > expirationTime)
            {
                CancelReservation(reservation.ReservationId);
                _notificationService.SendExpiration(reservation);
            }
        }
    }

    public AllocationResult ProcessAllQueues()
    {
        var rooms = _roomService.GetAvailableRooms();
        var allocator = new GreedyAllocator(rooms, _vipQueue, _regularQueue);
        return allocator.AllocateRooms();
    }

    public PerformanceMetrics GetSortPerformance(string sortType, int dataSize)
    {
        var reservations = GetAllReservations();
        if (reservations.Count < dataSize)
        {
            var additional = GenerateSampleReservations(dataSize - reservations.Count);
            reservations.AddRange(additional);
        }
        return QuickSorter.MeasureSortPerformance(reservations, sortType);
    }

    private List<Reservation> GenerateSampleReservations(int count)
    {
        var reservations = new List<Reservation>();
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            reservations.Add(new Reservation(
                GenerateReservationId(),
                random.Next(1, 100),
                random.Next(1, 20),
                DateTime.Now.AddDays(random.Next(1, 30)),
                DateTime.Now.AddDays(random.Next(31, 60)),
                random.Next(2) == 0 ? CustomerType.VIP : CustomerType.Regular
            ));
        }
        return reservations;
    }

    private string GenerateReservationId()
    {
        return $"RES-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    public void InitializeSampleData()
    {
        var rooms = _roomService.GetAllRooms();
        foreach (var room in rooms.Take(10))
        {
            try
            {
                CreateReservation(
                    $"Guest {room.RoomId}",
                    $"guest{room.RoomId}@hotel.com",
                    $"555-{room.RoomId:D3}",
                    room.RoomId % 3 == 0 ? CustomerType.VIP : CustomerType.Regular,
                    room.RoomId,
                    DateTime.Now.AddDays(room.RoomId % 5 + 1),
                    DateTime.Now.AddDays(room.RoomId % 5 + 3)
                );
            }
            catch
            {
            }
        }
    }
}
