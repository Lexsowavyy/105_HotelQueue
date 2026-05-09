using HotelQueue.Models;
using HotelQueue.Services;
using HotelQueue.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class WaitlistController : Controller
{
    private readonly WaitlistService _waitlistService;
    private readonly RoomService _roomService;
    private readonly NotificationService _notificationService;

    public WaitlistController(WaitlistService waitlistService, RoomService roomService, NotificationService notificationService)
    {
        _waitlistService = waitlistService;
        _roomService = roomService;
        _notificationService = notificationService;
    }

    public IActionResult Index(string? type, string? dateFilter, DateTime? startDate, DateTime? endDate, string? search)
    {
        var waitlist = _waitlistService.GetWaitlist();

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<CustomerType>(type, out var customerType))
            {
                waitlist = waitlist.Where(w => w.CustomerType == customerType).ToList();
            }
        }

        if (!string.IsNullOrEmpty(dateFilter))
        {
            var today = DateTime.Today;
            switch (dateFilter.ToLower())
            {
                case "today":
                    waitlist = waitlist.Where(w => w.PreferredCheckIn.Date == today).ToList();
                    break;
                case "week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7);
                    waitlist = waitlist.Where(w => w.PreferredCheckIn >= startOfWeek && w.PreferredCheckIn < endOfWeek).ToList();
                    break;
                case "month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);
                    waitlist = waitlist.Where(w => w.PreferredCheckIn >= startOfMonth && w.PreferredCheckIn < endOfMonth).ToList();
                    break;
                case "year":
                    var startOfYear = new DateTime(today.Year, 1, 1);
                    var endOfYear = new DateTime(today.Year, 12, 31);
                    waitlist = waitlist.Where(w => w.PreferredCheckIn >= startOfYear && w.PreferredCheckIn <= endOfYear).ToList();
                    break;
            }
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            waitlist = waitlist.Where(w => w.PreferredCheckIn >= startDate && w.PreferredCheckIn <= endDate).ToList();
        }

        if (!string.IsNullOrEmpty(search))
        {
            waitlist = waitlist.Where(w => 
                (w.Customer != null && w.Customer.Name != null && w.Customer.Name.ToLower().Contains(search.ToLower())) ||
                (w.Customer != null && w.Customer.Email != null && w.Customer.Email.ToLower().Contains(search.ToLower())) ||
                (w.Room != null && w.Room.RoomNumber != null && w.Room.RoomNumber.ToLower().Contains(search.ToLower()))
            ).ToList();
        }

        var model = new WaitlistViewModel
        {
            Waitlist = waitlist,
            WaitlistHistory = _waitlistService.GetWaitlistHistory(),
            TotalCount = _waitlistService.Count,
            VIPCount = _waitlistService.GetVIPCount(),
            RegularCount = _waitlistService.GetRegularCount(),
            NextInLine = _waitlistService.PeekNext()
        };
        return View(model);
    }

    public IActionResult Create()
    {
        var model = new CreateWaitlistViewModel
        {
            AvailableRooms = _roomService.GetAllRooms()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateWaitlistViewModel model)
    {
        if (!model.PreferredCheckIn.HasValue)
        {
            ModelState.AddModelError("", "Preferred check-in date is required.");
            model.AvailableRooms = _roomService.GetAllRooms();
            return View(model);
        }

        model.AvailableRooms = _roomService.GetAllRooms();

        var entry = _waitlistService.AddToWaitlist(
            0,
            model.RoomId,
            model.PreferredRoomType,
            model.PreferredCheckIn.Value,
            model.CustomerType,
            model.CustomerName,
            model.CustomerEmail,
            model.CustomerPhone
        );

        if (entry != null && entry.Customer != null)
        {
            _notificationService.SendWaitlistConfirmation(entry);
        }

        TempData["Success"] = $"Customer added to waitlist for Room {_roomService.GetRoom(model.RoomId)?.RoomNumber ?? model.RoomId.ToString()}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PromoteNext(int roomId)
    {
        var promoted = _waitlistService.PromoteNext(roomId);
        if (promoted != null)
        {
            TempData["Success"] = $"Customer {promoted.Customer?.Name ?? "ID: " + promoted.CustomerId} promoted from waitlist!";
        }
        else
        {
            TempData["Info"] = "No customers in waitlist for the specified room type.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var removed = _waitlistService.RemoveFromWaitlist(id);
        if (removed != null)
        {
            TempData["Success"] = "Customer removed from waitlist.";
        }
        else
        {
            TempData["Error"] = "Failed to remove from waitlist.";
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult VIP()
    {
        var model = new WaitlistViewModel
        {
            Waitlist = _waitlistService.GetWaitlistByType(CustomerType.VIP),
            VIPCount = _waitlistService.GetVIPCount()
        };
        return View(model);
    }

    public IActionResult Regular()
    {
        var model = new WaitlistViewModel
        {
            Waitlist = _waitlistService.GetWaitlistByType(CustomerType.Regular),
            RegularCount = _waitlistService.GetRegularCount()
        };
        return View(model);
    }

    public IActionResult Statistics()
    {
        var waitlist = _waitlistService.GetWaitlist();
        var promoted = _waitlistService.GetPromotedEntries();

        var model = new WaitlistStatistics
        {
            TotalWaitlist = _waitlistService.Count,
            VIPWaitlist = _waitlistService.GetVIPCount(),
            RegularWaitlist = _waitlistService.GetRegularCount(),
            WaitlistByRoomType = Enum.GetValues<RoomType>()
                .ToDictionary(t => t, t => _waitlistService.GetWaitlistByRoomType(t).Count),
            PromotionsToday = promoted.Count
        };

        return View(model);
    }

    public IActionResult History()
    {
        var model = new WaitlistViewModel
        {
            WaitlistHistory = _waitlistService.GetWaitlistHistory()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _waitlistService.ClearWaitlist();
        TempData["Success"] = "Waitlist cleared successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InitializeSample()
    {
        _waitlistService.InitializeSampleWaitlist();
        TempData["Success"] = "Sample waitlist data initialized.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var entry = _waitlistService.GetWaitlistEntry(id);
        if (entry == null)
        {
            TempData["Error"] = "Waitlist entry not found.";
            return RedirectToAction(nameof(Index));
        }
        return View(entry);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var entry = _waitlistService.GetWaitlistEntry(id);
        if (entry == null)
        {
            TempData["Error"] = "Waitlist entry not found.";
            return RedirectToAction(nameof(Index));
        }
        var model = new CreateWaitlistViewModel
        {
            CustomerName = entry.Customer?.Name ?? "",
            CustomerEmail = entry.Customer?.Email ?? "",
            CustomerPhone = entry.Customer?.Phone ?? "",
            CustomerType = entry.CustomerType,
            RoomId = entry.RoomId,
            PreferredRoomType = entry.PreferredRoomType,
            PreferredCheckIn = entry.PreferredCheckIn,
            AvailableRooms = _roomService.GetAllRooms()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CreateWaitlistViewModel model)
    {
        var success = _waitlistService.UpdateWaitlistEntry(id, model.CustomerName, model.CustomerEmail, 
            model.CustomerPhone, model.CustomerType, model.RoomId, model.PreferredRoomType, 
            model.PreferredCheckIn ?? DateTime.Now);
        
        if (success)
        {
            TempData["Success"] = "Waitlist entry updated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to update waitlist entry.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(int id)
    {
        var success = _waitlistService.ArchiveWaitlistEntry(id);
        if (success)
        {
            TempData["Success"] = "Waitlist entry archived successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to archive waitlist entry.";
        }
        return RedirectToAction(nameof(Index));
    }
}
