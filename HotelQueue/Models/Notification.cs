using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelQueue.Models;

[Table("Notifications")]
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId { get; set; }

    [MaxLength(50)]
    public string ReservationId { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public NotificationType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

    public Notification() { }

    public Notification(string reservationId, int customerId, string message, NotificationType type)
    {
        ReservationId = reservationId;
        CustomerId = customerId;
        Message = message;
        Type = type;
        CreatedAt = DateTime.Now;
    }
}
