using HotelQueue.DataStructures;
using HotelQueue.Models;
using HotelQueue.Services;
using HotelQueue.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class ReservationController : Controller
{
    private readonly ReservationService _reservationService;
    private readonly RoomService _roomService;

    public ReservationController(ReservationService reservationService, RoomService roomService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
    }

    public IActionResult Index(string? status, string? date, string? dateFilter, DateTime? startDate, DateTime? endDate, string? search, string? sortBy, bool ascending = true)
    {
        var reservations = _reservationService.GetAllReservations();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, out var reservationStatus))
        {
            reservations = reservations.Where(r => r.Status == reservationStatus).ToList();
        }
        else
        {
            reservations = reservations.Where(r => r.Status != ReservationStatus.Archived).ToList();
        }

        if (!string.IsNullOrEmpty(dateFilter))
        {
            var today = DateTime.Today;
            switch (dateFilter.ToLower())
            {
                case "today":
                    reservations = reservations.Where(r => r.CheckInDate.Date == today).ToList();
                    break;
                case "week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7);
                    reservations = reservations.Where(r => r.CheckInDate >= startOfWeek && r.CheckInDate < endOfWeek).ToList();
                    break;
                case "month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);
                    reservations = reservations.Where(r => r.CheckInDate >= startOfMonth && r.CheckInDate < endOfMonth).ToList();
                    break;
                case "year":
                    var startOfYear = new DateTime(today.Year, 1, 1);
                    var endOfYear = new DateTime(today.Year, 12, 31);
                    reservations = reservations.Where(r => r.CheckInDate >= startOfYear && r.CheckInDate <= endOfYear).ToList();
                    break;
            }
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            reservations = reservations.Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate).ToList();
        }
        else if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
        {
            reservations = reservations.Where(r => r.CheckInDate.Date == filterDate.Date || r.CheckOutDate.Date == filterDate.Date).ToList();
        }

        if (!string.IsNullOrEmpty(search))
        {
            reservations = reservations.Where(r => 
                (r.ReservationId != null && r.ReservationId.ToLower().Contains(search.ToLower())) ||
                (r.Customer != null && r.Customer.Name != null && r.Customer.Name.ToLower().Contains(search.ToLower())) ||
                (r.Customer != null && r.Customer.Email != null && r.Customer.Email.ToLower().Contains(search.ToLower())) ||
                (r.Room != null && r.Room.RoomNumber != null && r.Room.RoomNumber.ToLower().Contains(search.ToLower()))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            reservations = sortBy.ToLower() switch
            {
                "createddate" => ascending ? reservations.OrderBy(r => r.CreatedAt).ToList() : reservations.OrderByDescending(r => r.CreatedAt).ToList(),
                "checkindate" => ascending ? reservations.OrderBy(r => r.CheckInDate).ToList() : reservations.OrderByDescending(r => r.CheckInDate).ToList(),
                "checkoutdate" => ascending ? reservations.OrderBy(r => r.CheckOutDate).ToList() : reservations.OrderByDescending(r => r.CheckOutDate).ToList(),
                _ => reservations
            };
        }

        var model = new ReservationViewModel
        {
            AllReservations = reservations,
            VIPQueue = _reservationService.GetVIPQueueReservations(),
            RegularQueue = _reservationService.GetRegularQueueReservations(),
            SortBy = sortBy,
            SortAscending = ascending
        };
        return View(model);
    }

    public IActionResult Create(int? roomId)
    {
        var allRooms = _roomService.GetAllRooms();
        var roomBookings = new Dictionary<int, List<DateRange>>();
        
        foreach (var room in allRooms)
        {
            var bookings = _reservationService.GetRoomBookings(room.RoomId);
            if (bookings.Any())
            {
                roomBookings[room.RoomId] = bookings.Select(b => new DateRange 
                { 
                    CheckIn = b.CheckInDate, 
                    CheckOut = b.CheckOutDate 
                }).ToList();
            }
        }
        
        var model = new CreateReservationViewModel
        {
            AvailableRooms = _roomService.GetAvailableRooms(),
            AllRooms = allRooms,
            RoomId = roomId ?? 0,
            RoomBookings = roomBookings
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateReservationViewModel model)
    {
        try
        {
            if (!model.CheckInDate.HasValue || !model.CheckOutDate.HasValue)
            {
                ModelState.AddModelError("", "Check-in and check-out dates are required.");
                model.AvailableRooms = _roomService.GetAvailableRooms();
                return View(model);
            }

            var (reservation, waitlistEntry, isWaitlist) = _reservationService.CreateReservation(
                model.CustomerName,
                model.CustomerEmail,
                model.CustomerPhone,
                model.CustomerType,
                model.RoomId,
                model.CheckInDate.Value,
                model.CheckOutDate.Value
            );

            if (isWaitlist && waitlistEntry != null)
            {
                var latestReservation = _reservationService.GetAllReservations()
                    .Where(r => r.Customer != null && r.Customer.Email == model.CustomerEmail)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();
                    
                if (latestReservation != null)
                {
                    TempData["WaitlistMessage"] = "You have been added to the waitlist. You will be notified when the room becomes available.";
                    return RedirectToAction(nameof(Details), new { id = latestReservation.ReservationId });
                }
                return RedirectToAction("Index", "Waitlist");
            }

            TempData["Success"] = $"Reservation {reservation!.ReservationId} created successfully!";
            return RedirectToAction(nameof(Details), new { id = reservation.ReservationId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableRooms = _roomService.GetAvailableRooms();
            model.AllRooms = _roomService.GetAllRooms();
            return View(model);
        }
    }

    public IActionResult Details(string id)
    {
        var reservation = _reservationService.GetReservation(id);
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    [HttpGet]
    public IActionResult Edit(string id)
    {
        var reservation = _reservationService.GetReservationById(id);
        if (reservation == null)
        {
            return NotFound();
        }

        var model = new CreateReservationViewModel
        {
            AvailableRooms = _roomService.GetAvailableRooms(),
            AllRooms = _roomService.GetAllRooms(),
            CustomerName = reservation.Customer?.Name ?? "",
            CustomerEmail = reservation.Customer?.Email ?? "",
            CustomerPhone = reservation.Customer?.Phone ?? "",
            CustomerType = reservation.CustomerType,
            RoomId = reservation.RoomId,
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string id, CreateReservationViewModel model)
    {
        try
        {
            var success = _reservationService.UpdateReservation(id, model.CustomerName, model.CustomerEmail, 
                model.CustomerPhone, model.CustomerType, model.RoomId, model.CheckInDate ?? DateTime.Now, 
                model.CheckOutDate ?? DateTime.Now);
            
            if (success)
            {
                TempData["Success"] = "Reservation updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            TempData["Error"] = "Failed to update reservation.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            model.AvailableRooms = _roomService.GetAvailableRooms();
            model.AllRooms = _roomService.GetAllRooms();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Confirm(string id)
    {
        var success = _reservationService.ConfirmReservation(id);
        if (success)
        {
            TempData["Success"] = "Reservation confirmed successfully!";
        }
        else
        {
            TempData["Error"] = "Reservation not found.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(string id)
    {
        var success = _reservationService.CancelReservation(id);
        if (success)
        {
            TempData["Success"] = "Reservation cancelled successfully!";
        }
        else
        {
            TempData["Error"] = "Reservation not found.";
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult VIPQueue()
    {
        var model = new ReservationViewModel
        {
            VIPQueue = _reservationService.GetVIPQueueReservations()
        };
        return View(model);
    }

    public IActionResult RegularQueue()
    {
        var model = new ReservationViewModel
        {
            RegularQueue = _reservationService.GetRegularQueueReservations()
        };
        return View(model);
    }

    public IActionResult Search(string searchTerm)
    {
        var results = _reservationService.SearchReservations(searchTerm ?? "");
        var model = new ReservationViewModel
        {
            AllReservations = results
        };
        ViewBag.SearchTerm = searchTerm;
        return View(model);
    }

    public IActionResult Pending()
    {
        var reservations = _reservationService.GetReservationsByStatus(ReservationStatus.Pending);
        var model = new ReservationViewModel
        {
            AllReservations = reservations
        };
        return View(model);
    }

    public IActionResult Confirmed()
    {
        var reservations = _reservationService.GetReservationsByStatus(ReservationStatus.Confirmed);
        var model = new ReservationViewModel
        {
            AllReservations = reservations
        };
        return View(model);
    }

    public IActionResult Cancelled()
    {
        var reservations = _reservationService.GetReservationsByStatus(ReservationStatus.Cancelled);
        var model = new ReservationViewModel
        {
            AllReservations = reservations
        };
        return View(model);
    }

    public IActionResult ProcessQueues()
    {
        var result = _reservationService.ProcessAllQueues();
        TempData["Info"] = $"Processed {result.AllocatedVIPReservations.Count} VIP and {result.AllocatedRegularReservations.Count} Regular reservations.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Statistics()
    {
        var allReservations = _reservationService.GetAllReservations();
        var viewModel = new ReservationStatisticsViewModel
        {
            TotalReservations = allReservations.Count,
            PendingReservations = allReservations.Count(r => r.Status == ReservationStatus.Pending),
            ConfirmedReservations = allReservations.Count(r => r.Status == ReservationStatus.Confirmed),
            CancelledReservations = allReservations.Count(r => r.Status == ReservationStatus.Cancelled),
            VIPReservations = allReservations.Count(r => r.CustomerType == CustomerType.VIP),
            RegularReservations = allReservations.Count(r => r.CustomerType == CustomerType.Regular),
            TotalRevenue = allReservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.TotalPrice),
            VIPRevenue = allReservations.Where(r => r.Status == ReservationStatus.Confirmed && r.CustomerType == CustomerType.VIP).Sum(r => r.TotalPrice),
            RegularRevenue = allReservations.Where(r => r.Status == ReservationStatus.Confirmed && r.CustomerType == CustomerType.Regular).Sum(r => r.TotalPrice)
        };
        return View(viewModel);
    }

    public IActionResult DateRange(DateTime startDate, DateTime endDate)
    {
        var reservations = _reservationService.GetReservationsInDateRange(startDate, endDate);
        var model = new ReservationViewModel
        {
            AllReservations = reservations
        };
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExpireOld(int hours = 24)
    {
        _reservationService.ExpireOldReservations(TimeSpan.FromHours(hours));
        TempData["Success"] = $"Expired reservations older than {hours} hours.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(string id)
    {
        var reservation = _reservationService.GetReservationById(id);
        if (reservation == null)
        {
            TempData["Error"] = "Reservation not found.";
            return RedirectToAction(nameof(Index));
        }

        _reservationService.ArchiveReservation(id);
        TempData["Success"] = $"Reservation {id} has been archived.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Restore(string id)
    {
        var reservation = _reservationService.GetReservationById(id);
        if (reservation == null)
        {
            TempData["Error"] = "Reservation not found.";
            return RedirectToAction(nameof(Index));
        }

        _reservationService.RestoreReservation(id);
        TempData["Success"] = $"Reservation {id} has been restored.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult PerformanceTest(string sortType = "CheckInDate", int dataSize = 100)
    {
        var metrics = _reservationService.GetSortPerformance(sortType, dataSize);
        return View(metrics);
    }
}
