using HotelQueue.Models;

namespace HotelQueue.DataStructures.Baseline;

public class BaselineLinearSearcher
{
    public Reservation? FindById(List<Reservation> list, string reservationId)
    {
        foreach (var r in list)
        {
            if (r.ReservationId == reservationId)
                return r;
        }
        return null;
    }

    public List<Reservation> FindByCustomer(List<Reservation> list, string name)
    {
        var result = new List<Reservation>();
        foreach (var r in list)
        {
            if (r.Customer != null && r.Customer.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                result.Add(r);
        }
        return result;
    }

    public List<Reservation> FindByStatus(List<Reservation> list, ReservationStatus status)
    {
        var result = new List<Reservation>();
        foreach (var r in list)
        {
            if (r.Status == status)
                result.Add(r);
        }
        return result;
    }

    public List<Reservation> FindByDateRange(List<Reservation> list, DateTime start, DateTime end)
    {
        var result = new List<Reservation>();
        foreach (var r in list)
        {
            if (r.CheckInDate >= start && r.CheckInDate <= end)
                result.Add(r);
        }
        return result;
    }

    public List<Reservation> Search(List<Reservation> list, string term)
    {
        term = term.ToLower();
        var result = new List<Reservation>();
        foreach (var r in list)
        {
            if ((r.ReservationId != null && r.ReservationId.ToLower().Contains(term)) ||
                (r.Customer != null && r.Customer.Name != null && r.Customer.Name.ToLower().Contains(term)) ||
                (r.Customer != null && r.Customer.Email != null && r.Customer.Email.ToLower().Contains(term)) ||
                (r.Room != null && r.Room.RoomNumber != null && r.Room.RoomNumber.ToLower().Contains(term)))
            {
                result.Add(r);
            }
        }
        return result;
    }
}

public class BaselineBasicSorter
{
    public List<Reservation> BubbleSortByCheckIn(List<Reservation> list, bool ascending = true)
    {
        var arr = new List<Reservation>(list);
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                int cmp = arr[j].CheckInDate.CompareTo(arr[j + 1].CheckInDate);
                if (ascending ? cmp > 0 : cmp < 0)
                {
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                }
            }
        }
        return arr;
    }

    public List<Reservation> SelectionSortByCreatedAt(List<Reservation> list, bool ascending = true)
    {
        var arr = new List<Reservation>(list);
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int idx = i;
            for (int j = i + 1; j < n; j++)
            {
                int cmp = arr[j].CreatedAt.CompareTo(arr[idx].CreatedAt);
                if (ascending ? cmp < 0 : cmp > 0)
                    idx = j;
            }
            if (idx != i)
                (arr[i], arr[idx]) = (arr[idx], arr[i]);
        }
        return arr;
    }

    public List<Reservation> BubbleSortByPrice(List<Reservation> list, bool ascending = true)
    {
        var arr = new List<Reservation>(list);
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                int cmp = arr[j].TotalPrice.CompareTo(arr[j + 1].TotalPrice);
                if (ascending ? cmp > 0 : cmp < 0)
                {
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                }
            }
        }
        return arr;
    }

    public PerformanceMetrics MeasureSortPerformance(List<Reservation> list)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var sorted = BubbleSortByCheckIn(list);
        stopwatch.Stop();

        return new PerformanceMetrics
        {
            Operation = "Baseline_BubbleSort",
            ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds,
            Comparisons = list.Count * list.Count,
            Swaps = list.Count * list.Count / 2,
            DataSize = list.Count
        };
    }
}

public class BaselineSingleQueue
{
    private readonly Queue<Reservation> _queue = new();
    private readonly List<Reservation> _allReservations = new();

    public int Count => _queue.Count;
    public int TotalCount => _allReservations.Count;

    public void Enqueue(Reservation reservation)
    {
        _queue.Enqueue(reservation);
        _allReservations.Add(reservation);
    }

    public Reservation Dequeue()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        return _queue.Dequeue();
    }

    public Reservation? Peek()
    {
        return _queue.Count > 0 ? _queue.Peek() : null;
    }

    public bool Remove(string reservationId)
    {
        var temp = new Queue<Reservation>();
        bool found = false;
        while (_queue.Count > 0)
        {
            var r = _queue.Dequeue();
            if (r.ReservationId == reservationId)
                found = true;
            else
                temp.Enqueue(r);
        }
        while (temp.Count > 0)
            _queue.Enqueue(temp.Dequeue());

        _allReservations.RemoveAll(r => r.ReservationId == reservationId);
        return found;
    }

    public List<Reservation> GetAll()
    {
        return new List<Reservation>(_queue);
    }

    public void Clear()
    {
        _queue.Clear();
        _allReservations.Clear();
    }
}

public class BaselineSequentialAllocator
{
    private readonly BaselineSingleQueue _queue;
    private readonly List<Room> _rooms;

    public BaselineSequentialAllocator(BaselineSingleQueue queue, List<Room> rooms)
    {
        _queue = queue;
        _rooms = rooms;
    }

    public (List<Reservation> Allocated, List<Room> RoomsUsed) AllocateRooms()
    {
        var allocated = new List<Reservation>();
        var roomsUsed = new List<Room>();

        while (_queue.Count > 0)
        {
            var reservation = _queue.Dequeue();
            var room = _rooms.FirstOrDefault(r => r.IsAvailable);

            if (room != null)
            {
                allocated.Add(reservation);
                roomsUsed.Add(room);
                room.IsAvailable = false;
            }
            else
            {
                break;
            }
        }

        return (allocated, roomsUsed);
    }
}
