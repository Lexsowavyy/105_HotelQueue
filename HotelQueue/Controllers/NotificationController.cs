using HotelQueue.Models;
using HotelQueue.Services;
using HotelQueue.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public IActionResult Index()
    {
        var notifications = _notificationService.GetAllNotifications()
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        var model = new NotificationViewModel
        {
            AllNotifications = notifications,
            UnreadNotifications = _notificationService.GetUnreadNotifications(),
            UnreadCount = _notificationService.GetUnreadCount(),
            NotificationCountsByType = _notificationService.GetNotificationCountsByType()
        };

        return View(model);
    }

    public IActionResult Details(int id)
    {
        var notifications = _notificationService.GetAllNotifications();
        var notification = notifications.FirstOrDefault(n => n.NotificationId == id);

        if (notification == null)
        {
            return NotFound();
        }

        _notificationService.MarkAsRead(id);

        var model = new NotificationDetailsViewModel
        {
            NotificationId = notification.NotificationId,
            ReservationId = notification.ReservationId,
            CustomerId = notification.CustomerId,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt,
            IsRead = notification.IsRead
        };

        return View(model);
    }

    public IActionResult ByType(NotificationType type)
    {
        var notifications = _notificationService.GetNotificationsByType(type)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        var model = new NotificationViewModel
        {
            AllNotifications = notifications,
            NotificationCountsByType = _notificationService.GetNotificationCountsByType()
        };

        return View(model);
    }

    public IActionResult Unread()
    {
        var model = new NotificationViewModel
        {
            UnreadNotifications = _notificationService.GetUnreadNotifications(),
            UnreadCount = _notificationService.GetUnreadCount()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAsRead(int id)
    {
        _notificationService.MarkAsRead(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAllAsRead()
    {
        _notificationService.MarkAllAsRead();
        TempData["Success"] = "All notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(BulkNotificationViewModel model)
    {
        var customerIds = new List<int>();

        switch (model.Type)
        {
            case NotificationType.Reminder:
                customerIds = GetCustomerIdsByType(model.SendToVIPOnly, model.SendToRegularOnly);
                break;
            case NotificationType.Confirmation:
            case NotificationType.Cancellation:
            case NotificationType.WaitlistPromotion:
            case NotificationType.Expiration:
                break;
        }

        if (customerIds.Any())
        {
            _notificationService.SendBulkNotification(customerIds, model.Message, model.Type);
            TempData["Success"] = "Notifications sent successfully!";
        }
        else
        {
            TempData["Info"] = "No customers to notify.";
        }

        return RedirectToAction(nameof(Index));
    }

    private List<int> GetCustomerIdsByType(bool vipOnly, bool regularOnly)
    {
        var ids = new List<int>();
        return ids;
    }

    public IActionResult ClearOld(int days = 30)
    {
        _notificationService.ClearOldNotifications(TimeSpan.FromDays(days));
        TempData["Success"] = $"Old notifications cleared (older than {days} days).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _notificationService.DeleteNotification(id);
        TempData["Success"] = "Notification deleted.";
        return RedirectToAction(nameof(Index));
    }
}
