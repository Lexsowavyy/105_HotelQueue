using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelQueue.Models;

[Table("Reservations")]
public class Reservation
{
    [Key]
    [MaxLength(50)]
    public string ReservationId { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Required]
    public int RoomId { get; set; }

    [ForeignKey("RoomId")]
    public Room? Room { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Required]
    public CustomerType CustomerType { get; set; }

    [Required]
    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    [NotMapped]
    public int TotalNights => (CheckOutDate - CheckInDate).Days;

    [NotMapped]
    public decimal TotalPrice => Room != null ? Room.PricePerNight * TotalNights : 0;

    public Reservation() { }

    public Reservation(string reservationId, int customerId, int roomId, DateTime checkIn, DateTime checkOut, CustomerType customerType)
    {
        ReservationId = reservationId;
        CustomerId = customerId;
        RoomId = roomId;
        CheckInDate = checkIn;
        CheckOutDate = checkOut;
        CustomerType = customerType;
        Priority = customerType == CustomerType.VIP ? 1 : 0;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.Now;
    }
}
