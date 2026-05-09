using HotelQueue.Models;
using HotelQueue.Services;
using HotelQueue.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class RoomController : Controller
{
    private readonly RoomService _roomService;

    public RoomController(RoomService roomService)
    {
        _roomService = roomService;
    }

    public IActionResult Index(string? searchTerm)
    {
        var model = new RoomViewModel
        {
            AllRooms = _roomService.GetAllRooms(),
            AvailableRooms = _roomService.GetAvailableRooms(),
            OccupiedRooms = _roomService.GetUnavailableRooms(),
            RoomCountByType = _roomService.GetRoomCountByType(),
            AvailableCountByType = _roomService.GetAvailableRoomCountByType()
        };

        if (!string.IsNullOrEmpty(searchTerm))
        {
            model.AllRooms = _roomService.SearchRooms(searchTerm);
        }

        return View(model);
    }

    public IActionResult Details(int id)
    {
        var room = _roomService.GetRoom(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateRoomViewModel model)
    {
        try
        {
            var room = _roomService.AddRoom(
                model.RoomNumber,
                model.Type,
                model.PricePerNight,
                model.Capacity
            );

            TempData["Success"] = $"Room {room.RoomNumber} created successfully!";
            return RedirectToAction(nameof(Details), new { id = room.RoomId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public IActionResult Edit(int id)
    {
        var room = _roomService.GetRoom(id);
        if (room == null)
        {
            return NotFound();
        }

        var model = new CreateRoomViewModel
        {
            RoomNumber = room.RoomNumber,
            Type = room.Type,
            PricePerNight = room.PricePerNight,
            Capacity = room.Capacity
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CreateRoomViewModel model)
    {
        try
        {
            var success = _roomService.UpdateRoom(id, model.RoomNumber, model.Type, model.PricePerNight, model.Capacity);
            if (success)
            {
                TempData["Success"] = "Room updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            var success = _roomService.DeleteRoom(id);
            if (success)
            {
                TempData["Success"] = "Room deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete room.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Available()
    {
        var model = new RoomViewModel
        {
            AvailableRooms = _roomService.GetAvailableRooms(),
            AvailableCountByType = _roomService.GetAvailableRoomCountByType()
        };
        return View(model);
    }

    public IActionResult Occupied()
    {
        var model = new RoomViewModel
        {
            OccupiedRooms = _roomService.GetUnavailableRooms()
        };
        return View(model);
    }

    public IActionResult Statistics()
    {
        var rooms = _roomService.GetAllRooms();
        var statistics = new List<RoomTypeStatistics>();

        foreach (RoomType type in Enum.GetValues(typeof(RoomType)))
        {
            var typeRooms = rooms.Where(r => r.Type == type).ToList();
            var available = typeRooms.Count(r => r.IsAvailable);

            statistics.Add(new RoomTypeStatistics
            {
                Type = type,
                TypeName = type.ToString(),
                TotalRooms = typeRooms.Count,
                AvailableRooms = available,
                OccupiedRooms = typeRooms.Count - available,
                AveragePrice = typeRooms.Any() ? typeRooms.Average(r => r.PricePerNight) : 0,
                TotalPotentialRevenue = typeRooms.Sum(r => r.PricePerNight),
                OccupancyRate = typeRooms.Any() ? (double)(typeRooms.Count - available) / typeRooms.Count * 100 : 0
            });
        }

        return View(statistics);
    }

    public IActionResult ByType(RoomType? type)
    {
        List<Room> rooms;
        if (type.HasValue)
        {
            rooms = _roomService.GetRoomsByType(type.Value);
        }
        else
        {
            rooms = _roomService.GetAllRooms();
        }

        var model = new RoomViewModel
        {
            AllRooms = rooms
        };
        return View(model);
    }
}
