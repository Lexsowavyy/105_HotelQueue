namespace HotelQueue.Models;

public enum CustomerType
{
    Regular = 0,
    VIP = 1
}

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3,
    Completed = 4,
    OnWaitlist = 5,
    Archived = 6
}

public enum RoomType
{
    Standard = 0,
    Deluxe = 1,
    Suite = 2
}

public enum NotificationType
{
    Confirmation = 0,
    Cancellation = 1,
    WaitlistPromotion = 2,
    Reminder = 3,
    Expiration = 4,
    Archive = 5
}
