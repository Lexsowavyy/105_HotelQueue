using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelQueue.Models;

[Table("Rooms")]
public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomId { get; set; }

    [Required]
    [MaxLength(10)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    public RoomType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Required]
    public bool IsAvailable { get; set; } = true;

    [Required]
    [Range(1, 10)]
    public int Capacity { get; set; }

    public Room() { }

    public Room(int roomId, string roomNumber, RoomType type, decimal pricePerNight, int capacity)
    {
        RoomId = roomId;
        RoomNumber = roomNumber;
        Type = type;
        PricePerNight = pricePerNight;
        Capacity = capacity;
        IsAvailable = true;
    }
}
