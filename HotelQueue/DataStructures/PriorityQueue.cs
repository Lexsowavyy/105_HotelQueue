using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public class PriorityQueue
{
    private readonly List<Reservation> _heap = new();
    private readonly Dictionary<string, int> _reservationIndex = new();

    public int Count => _heap.Count;

    public void Enqueue(Reservation reservation)
    {
        _heap.Add(reservation);
        int index = _heap.Count - 1;
        _reservationIndex[reservation.ReservationId] = index;
        HeapifyUp(index);
    }

    public Reservation Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Priority queue is empty");

        Reservation top = _heap[0];
        _reservationIndex.Remove(top.ReservationId);

        if (_heap.Count > 1)
        {
            _heap[0] = _heap[^1];
            _reservationIndex[_heap[0].ReservationId] = 0;
            _heap.RemoveAt(_heap.Count - 1);
            HeapifyDown(0);
        }
        else
        {
            _heap.RemoveAt(0);
        }

        return top;
    }

    public Reservation? Peek()
    {
        return _heap.Count > 0 ? _heap[0] : null;
    }

    public bool Remove(string reservationId)
    {
        if (!_reservationIndex.TryGetValue(reservationId, out int index))
            return false;

        _reservationIndex.Remove(reservationId);

        if (index == _heap.Count - 1)
        {
            _heap.RemoveAt(index);
            return true;
        }

        Reservation last = _heap[^1];
        _heap[index] = last;
        _reservationIndex[last.ReservationId] = index;
        _heap.RemoveAt(_heap.Count - 1);

        int parentIndex = (index - 1) / 2;
        if (index > 0 && CompareReservations(_heap[index], _heap[parentIndex]) < 0)
            HeapifyUp(index);
        else
            HeapifyDown(index);

        return true;
    }

    public void UpdatePriority(string reservationId, int newPriority)
    {
        if (!_reservationIndex.TryGetValue(reservationId, out int index))
            return;

        _heap[index].Priority = newPriority;

        int parentIndex = (index - 1) / 2;
        if (index > 0 && CompareReservations(_heap[index], _heap[parentIndex]) < 0)
            HeapifyUp(index);
        else
            HeapifyDown(index);
    }

    public List<Reservation> GetAll()
    {
        return new List<Reservation>(_heap);
    }

    public Reservation? Find(string reservationId)
    {
        if (_reservationIndex.TryGetValue(reservationId, out int index))
            return _heap[index];
        return null;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (CompareReservations(_heap[index], _heap[parentIndex]) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = _heap.Count - 1;
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild <= lastIndex && CompareReservations(_heap[leftChild], _heap[smallest]) < 0)
                smallest = leftChild;

            if (rightChild <= lastIndex && CompareReservations(_heap[rightChild], _heap[smallest]) < 0)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        _reservationIndex[_heap[i].ReservationId] = j;
        _reservationIndex[_heap[j].ReservationId] = i;
        (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
    }

    private int CompareReservations(Reservation a, Reservation b)
    {
        int priorityCompare = a.Priority.CompareTo(b.Priority);
        if (priorityCompare != 0)
            return priorityCompare;
        return a.CreatedAt.CompareTo(b.CreatedAt);
    }

    public void Clear()
    {
        _heap.Clear();
        _reservationIndex.Clear();
    }
}
