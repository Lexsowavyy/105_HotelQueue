using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public class FIFOQueue
{
    private readonly Queue<Reservation> _queue = new();
    private readonly Dictionary<string, int> _positionTracker = new();
    private int _globalPosition = 0;

    public int Count => _queue.Count;

    public void Enqueue(Reservation reservation)
    {
        _queue.Enqueue(reservation);
        _positionTracker[reservation.ReservationId] = _globalPosition++;
    }

    public Reservation Dequeue()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("FIFO queue is empty");

        Reservation reservation = _queue.Dequeue();
        _positionTracker.Remove(reservation.ReservationId);
        return reservation;
    }

    public Reservation? Peek()
    {
        return _queue.Count > 0 ? _queue.Peek() : null;
    }

    public bool Remove(string reservationId)
    {
        if (!_positionTracker.ContainsKey(reservationId))
            return false;

        var tempQueue = new Queue<Reservation>();
        bool found = false;

        while (_queue.Count > 0)
        {
            Reservation current = _queue.Dequeue();
            if (current.ReservationId == reservationId)
            {
                found = true;
                _positionTracker.Remove(reservationId);
            }
            else
            {
                tempQueue.Enqueue(current);
            }
        }

        _queue.Clear();
        while (tempQueue.Count > 0)
        {
            _queue.Enqueue(tempQueue.Dequeue());
        }

        return found;
    }

    public List<Reservation> GetAll()
    {
        return _queue.ToList();
    }

    public Reservation? Find(string reservationId)
    {
        return _queue.FirstOrDefault(r => r.ReservationId == reservationId);
    }

    public void Clear()
    {
        _queue.Clear();
        _positionTracker.Clear();
        _globalPosition = 0;
    }
}
