using HotelQueue.Data;
using HotelQueue.Models;

namespace HotelQueue.Services;

public class NotificationService
{
    private readonly HotelDbContext _context;

    public NotificationService(HotelDbContext context)
    {
        _context = context;
    }

    public List<Notification> GetAllNotifications()
    {
        return _context.Notifications.ToList();
    }

    public List<Notification> GetUnreadNotifications()
    {
        return _context.Notifications.Where(n => !n.IsRead).ToList();
    }

    public List<Notification> GetNotificationsByCustomer(int customerId)
    {
        return _context.Notifications.Where(n => n.CustomerId == customerId).ToList();
    }

    public List<Notification> GetNotificationsByType(NotificationType type)
    {
        return _context.Notifications.Where(n => n.Type == type).ToList();
    }

    public void MarkAsRead(int notificationId)
    {
        var notification = _context.Notifications.Find(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            _context.SaveChanges();
        }
    }

    public void MarkAllAsRead()
    {
        var unreadNotifications = _context.Notifications.Where(n => !n.IsRead).ToList();
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }
        _context.SaveChanges();
    }

    public void SendConfirmation(Reservation reservation)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Your reservation {reservation.ReservationId} has been confirmed for Room {reservation.Room?.RoomNumber} from {reservation.CheckInDate:MMM dd, yyyy} to {reservation.CheckOutDate:MMM dd, yyyy}. Total: ₱{reservation.TotalPrice:N2}",
            NotificationType.Confirmation
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendCancellation(Reservation reservation)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Your reservation {reservation.ReservationId} has been cancelled. Room {reservation.Room?.RoomNumber} is now available.",
            NotificationType.Cancellation
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendArchive(Reservation reservation)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Your reservation {reservation.ReservationId} has been archived.",
            NotificationType.Archive
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendWaitlistPromotion(Reservation reservation, Room room)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Great news! Room {room.RoomNumber} ({room.Type}) is now available! Your waitlist position has been promoted. Your reservation {reservation.ReservationId} is now confirmed for {reservation.CheckInDate:MMM dd, yyyy HH:mm} to {reservation.CheckOutDate:MMM dd, yyyy HH:mm}.",
            NotificationType.WaitlistPromotion
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendWaitlistConfirmation(WaitlistEntry entry)
    {
        var notification = new Notification(
            $"WL-{entry.WaitlistId}",
            entry.CustomerId,
            $"You have been added to our waitlist. Room Type: {entry.PreferredRoomType}, Preferred Check-in: {entry.PreferredCheckIn:MMM dd, yyyy HH:mm}. You will be notified when a room becomes available!",
            NotificationType.Reminder
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendReminder(Reservation reservation, int daysUntilCheckIn)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Reminder: Your reservation {reservation.ReservationId} is in {daysUntilCheckIn} days. Room {reservation.Room?.RoomNumber} is waiting for you!",
            NotificationType.Reminder
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendExpiration(Reservation reservation)
    {
        var notification = new Notification(
            reservation.ReservationId,
            reservation.CustomerId,
            $"Your reservation {reservation.ReservationId} has expired due to lack of confirmation. Room {reservation.Room?.RoomNumber} is now available.",
            NotificationType.Expiration
        );
        _context.Notifications.Add(notification);
        _context.SaveChanges();
    }

    public void SendBulkNotification(List<int> customerIds, string message, NotificationType type)
    {
        foreach (var customerId in customerIds)
        {
            var notification = new Notification(
                string.Empty,
                customerId,
                message,
                type
            );
            _context.Notifications.Add(notification);
        }
        _context.SaveChanges();
    }

    public int GetUnreadCount()
    {
        return _context.Notifications.Count(n => !n.IsRead);
    }

    public int GetUnreadCountByCustomer(int customerId)
    {
        return _context.Notifications.Count(n => n.CustomerId == customerId && !n.IsRead);
    }

    public void ClearOldNotifications(TimeSpan maxAge)
    {
        var cutoff = DateTime.Now - maxAge;
        var oldNotifications = _context.Notifications.Where(n => n.CreatedAt < cutoff).ToList();
        _context.Notifications.RemoveRange(oldNotifications);
        _context.SaveChanges();
    }

    public Dictionary<NotificationType, int> GetNotificationCountsByType()
    {
        return _context.Notifications
            .GroupBy(n => n.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public void DeleteNotification(int notificationId)
    {
        var notification = _context.Notifications.Find(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            _context.SaveChanges();
        }
    }
}
