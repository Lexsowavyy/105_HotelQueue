using HotelQueue.Models;

namespace HotelQueue.ViewModels;

public class NotificationViewModel
{
    public List<Notification> AllNotifications { get; set; } = new();
    public List<Notification> UnreadNotifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public Dictionary<NotificationType, int> NotificationCountsByType { get; set; } = new();
}

public class NotificationDetailsViewModel
{
    public int NotificationId { get; set; }
    public string ReservationId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class BulkNotificationViewModel
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool SendToVIPOnly { get; set; }
    public bool SendToRegularOnly { get; set; }
    public bool SendToPendingOnly { get; set; }
}
