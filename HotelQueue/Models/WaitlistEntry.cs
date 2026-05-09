using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelQueue.Models;

[Table("WaitlistEntries")]
public class WaitlistEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int WaitlistId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Required]
    public int RoomId { get; set; }

    [ForeignKey("RoomId")]
    public Room? Room { get; set; }

    [Required]
    public RoomType PreferredRoomType { get; set; }

    [Required]
    public DateTime PreferredCheckIn { get; set; }

    [Required]
    public CustomerType CustomerType { get; set; }

    [Required]
    public int Priority { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.Now;

    public bool IsPromoted { get; set; } = false;

    public bool IsArchived { get; set; } = false;

    public WaitlistEntry() { }

    public WaitlistEntry(int customerId, int roomId, RoomType roomType, DateTime checkIn, CustomerType customerType)
    {
        CustomerId = customerId;
        RoomId = roomId;
        PreferredRoomType = roomType;
        PreferredCheckIn = checkIn;
        CustomerType = customerType;
        Priority = customerType == CustomerType.VIP ? 1 : 0;
        AddedAt = DateTime.Now;
    }
}
