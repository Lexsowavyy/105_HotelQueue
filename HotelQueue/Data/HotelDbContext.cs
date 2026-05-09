using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelQueue.Models;

namespace HotelQueue.Data;

public class HotelDbContext : IdentityDbContext<ApplicationUser>
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<WaitlistEntry> WaitlistEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomNumber).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId);
            entity.Property(e => e.ReservationId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CustomerType).HasConversion<string>().HasMaxLength(20);
            entity.Ignore(e => e.TotalPrice);
            entity.Ignore(e => e.TotalNights);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Room)
                .WithMany()
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ReservationId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.RoomId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.ReservationId).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.HasKey(e => e.WaitlistId);
            entity.Property(e => e.PreferredRoomType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CustomerType).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().HasData(
            new Room { RoomId = 1, RoomNumber = "101", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 2, RoomNumber = "102", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 3, RoomNumber = "103", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 4, RoomNumber = "104", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 3, IsAvailable = true },
            new Room { RoomId = 5, RoomNumber = "105", Type = RoomType.Standard, PricePerNight = 3500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 6, RoomNumber = "201", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 7, RoomNumber = "202", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 8, RoomNumber = "203", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 3, IsAvailable = true },
            new Room { RoomId = 9, RoomNumber = "204", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 2, IsAvailable = true },
            new Room { RoomId = 10, RoomNumber = "205", Type = RoomType.Deluxe, PricePerNight = 7500m, Capacity = 3, IsAvailable = true },
            new Room { RoomId = 11, RoomNumber = "301", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomId = 12, RoomNumber = "302", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomId = 13, RoomNumber = "303", Type = RoomType.Suite, PricePerNight = 15000m, Capacity = 4, IsAvailable = true },
            new Room { RoomId = 14, RoomNumber = "304", Type = RoomType.Suite, PricePerNight = 20000m, Capacity = 4, IsAvailable = true },
            new Room { RoomId = 15, RoomNumber = "305", Type = RoomType.Suite, PricePerNight = 20000m, Capacity = 4, IsAvailable = true }
        );
    }
}
