using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelQueue.Models;

[Table("Customers")]
public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public CustomerType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Customer() { }

    public Customer(string name, string email, string phone, CustomerType type)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Type = type;
        CreatedAt = DateTime.Now;
    }
}
