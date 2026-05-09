using HotelQueue.Data;
using HotelQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelQueue.Services;

public class WaitlistService
{
    private readonly HotelDbContext _context;

    public WaitlistService(HotelDbContext context)
    {
        _context = context;
    }

    public int Count => _context.WaitlistEntries.Count();

    public WaitlistEntry AddToWaitlist(int customerId, int roomId, RoomType preferredRoomType, DateTime preferredCheckIn,
        CustomerType customerType, string? customerName = null, string? customerEmail = null, string? customerPhone = null)
    {
        Customer? customer = null;
        if (!string.IsNullOrEmpty(customerName))
        {
            customer = new Customer(customerName, customerEmail ?? "", customerPhone ?? "", customerType);
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }

        var entry = new WaitlistEntry
        {
            CustomerId = customer?.CustomerId ?? customerId,
            RoomId = roomId,
            PreferredRoomType = preferredRoomType,
            PreferredCheckIn = preferredCheckIn,
            CustomerType = customerType,
            Priority = customerType == CustomerType.VIP ? 1 : 0,
            Customer = customer
        };

        _context.WaitlistEntries.Add(entry);
        _context.SaveChanges();

        return entry;
    }

    public WaitlistEntry? PromoteNext(int roomId)
    {
        var waitlist = _context.WaitlistEntries
            .Include(w => w.Customer)
            .Where(w => !w.IsPromoted)
            .OrderByDescending(w => w.CustomerType == CustomerType.VIP)
            .ThenBy(w => w.AddedAt)
            .ToList();

        WaitlistEntry? promoted = null;

        foreach (var entry in waitlist)
        {
            if (entry.RoomId == roomId || entry.PreferredRoomType == RoomType.Standard)
            {
                promoted = entry;
                promoted.IsPromoted = true;
                _context.SaveChanges();
                break;
            }
        }

        return promoted;
    }

    public WaitlistEntry? PeekNext()
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .Where(w => !w.IsPromoted)
            .OrderByDescending(w => w.CustomerType == CustomerType.VIP)
            .ThenBy(w => w.AddedAt)
            .FirstOrDefault();
    }

    public WaitlistEntry? RemoveFromWaitlist(int waitlistId)
    {
        var entry = _context.WaitlistEntries.Find(waitlistId);
        if (entry != null)
        {
            _context.WaitlistEntries.Remove(entry);
            _context.SaveChanges();
        }
        return entry;
    }

    public List<WaitlistEntry> GetWaitlist()
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .Where(w => !w.IsPromoted && !w.IsArchived)
            .OrderByDescending(w => w.CustomerType == CustomerType.VIP)
            .ThenBy(w => w.AddedAt)
            .ToList();
    }

    public List<WaitlistEntry> GetWaitlistHistory()
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .OrderByDescending(w => w.AddedAt)
            .ToList();
    }

    public List<WaitlistEntry> GetActiveWaitlist()
    {
        return GetWaitlist();
    }

    public List<WaitlistEntry> GetPromotedEntries()
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .Where(e => e.IsPromoted)
            .ToList();
    }

    public List<WaitlistEntry> GetWaitlistByType(CustomerType customerType)
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .Where(e => e.CustomerType == customerType && !e.IsPromoted)
            .ToList();
    }

    public List<WaitlistEntry> GetWaitlistByRoomType(RoomType roomType)
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .Where(e => e.PreferredRoomType == roomType && !e.IsPromoted)
            .ToList();
    }

    public int GetVIPCount()
    {
        return _context.WaitlistEntries.Count(e => e.CustomerType == CustomerType.VIP && !e.IsPromoted);
    }

    public int GetRegularCount()
    {
        return _context.WaitlistEntries.Count(e => e.CustomerType == CustomerType.Regular && !e.IsPromoted);
    }

    public void ClearWaitlist()
    {
        var entries = _context.WaitlistEntries.Where(w => !w.IsPromoted).ToList();
        _context.WaitlistEntries.RemoveRange(entries);
        _context.SaveChanges();
    }

    public void InitializeSampleWaitlist()
    {
        var roomTypes = Enum.GetValues<RoomType>();
        var random = new Random();

        for (int i = 0; i < 5; i++)
        {
            AddToWaitlist(
                0,
                0,
                roomTypes[random.Next(roomTypes.Length)],
                DateTime.Now.AddDays(random.Next(1, 14)),
                random.Next(3) == 0 ? CustomerType.VIP : CustomerType.Regular,
                $"Waitlist Guest {i + 1}",
                $"waitlist{i + 1}@hotel.com",
                $"555-WL{i + 1:D3}"
            );
        }
    }

    public WaitlistEntry? GetWaitlistEntry(int id)
    {
        return _context.WaitlistEntries
            .Include(w => w.Customer)
            .Include(w => w.Room)
            .FirstOrDefault(w => w.WaitlistId == id);
    }

    public bool UpdateWaitlistEntry(int id, string customerName, string customerEmail, string customerPhone,
        CustomerType customerType, int roomId, RoomType preferredRoomType, DateTime preferredCheckIn)
    {
        var entry = _context.WaitlistEntries
            .Include(w => w.Customer)
            .FirstOrDefault(w => w.WaitlistId == id);

        if (entry == null)
            return false;

        if (entry.Customer != null)
        {
            entry.Customer.Name = customerName;
            entry.Customer.Email = customerEmail;
            entry.Customer.Phone = customerPhone;
            entry.Customer.Type = customerType;
        }

        entry.RoomId = roomId;
        entry.PreferredRoomType = preferredRoomType;
        entry.PreferredCheckIn = preferredCheckIn;
        entry.CustomerType = customerType;
        entry.Priority = customerType == CustomerType.VIP ? 1 : 0;

        _context.SaveChanges();
        return true;
    }

    public bool ArchiveWaitlistEntry(int id)
    {
        var entry = _context.WaitlistEntries.Find(id);
        if (entry == null)
            return false;

        entry.IsArchived = true;
        _context.SaveChanges();
        return true;
    }
}
