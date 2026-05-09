using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public class ReservationHashTable
{
    private readonly Dictionary<string, Reservation> _hashTable = new();
    private readonly Dictionary<string, List<string>> _customerReservations = new();
    private readonly Dictionary<string, List<string>> _roomReservations = new();
    private readonly Dictionary<string, List<string>> _statusReservations = new();

    public int Count => _hashTable.Count;

    public void Add(Reservation reservation)
    {
        _hashTable[reservation.ReservationId] = reservation;

        if (!_customerReservations.ContainsKey(reservation.CustomerId.ToString()))
            _customerReservations[reservation.CustomerId.ToString()] = new List<string>();
        _customerReservations[reservation.CustomerId.ToString()].Add(reservation.ReservationId);

        string roomKey = reservation.RoomId.ToString();
        if (!_roomReservations.ContainsKey(roomKey))
            _roomReservations[roomKey] = new List<string>();
        _roomReservations[roomKey].Add(reservation.ReservationId);

        string statusKey = reservation.Status.ToString();
        if (!_statusReservations.ContainsKey(statusKey))
            _statusReservations[statusKey] = new List<string>();
        _statusReservations[statusKey].Add(reservation.ReservationId);
    }

    public Reservation? Get(string reservationId)
    {
        return _hashTable.TryGetValue(reservationId, out var reservation) ? reservation : null;
    }

    public bool Remove(string reservationId)
    {
        if (!_hashTable.TryGetValue(reservationId, out var reservation))
            return false;

        _hashTable.Remove(reservationId);

        if (_customerReservations.ContainsKey(reservation.CustomerId.ToString()))
            _customerReservations[reservation.CustomerId.ToString()].Remove(reservationId);

        string roomKey = reservation.RoomId.ToString();
        if (_roomReservations.ContainsKey(roomKey))
            _roomReservations[roomKey].Remove(reservationId);

        string statusKey = reservation.Status.ToString();
        if (_statusReservations.ContainsKey(statusKey))
            _statusReservations[statusKey].Remove(reservationId);

        return true;
    }

    public bool Update(Reservation reservation)
    {
        if (!_hashTable.ContainsKey(reservation.ReservationId))
            return false;

        Reservation oldReservation = _hashTable[reservation.ReservationId];
        string oldStatusKey = oldReservation.Status.ToString();

        if (_statusReservations.ContainsKey(oldStatusKey))
            _statusReservations[oldStatusKey].Remove(reservation.ReservationId);

        string newStatusKey = reservation.Status.ToString();
        if (!_statusReservations.ContainsKey(newStatusKey))
            _statusReservations[newStatusKey] = new List<string>();
        _statusReservations[newStatusKey].Add(reservation.ReservationId);

        _hashTable[reservation.ReservationId] = reservation;
        return true;
    }

    public List<Reservation> GetByCustomer(int customerId)
    {
        if (!_customerReservations.TryGetValue(customerId.ToString(), out var ids))
            return new List<Reservation>();

        return ids
            .Where(id => _hashTable.ContainsKey(id))
            .Select(id => _hashTable[id])
            .ToList();
    }

    public List<Reservation> GetByRoom(int roomId)
    {
        if (!_roomReservations.TryGetValue(roomId.ToString(), out var ids))
            return new List<Reservation>();

        return ids
            .Where(id => _hashTable.ContainsKey(id))
            .Select(id => _hashTable[id])
            .ToList();
    }

    public List<Reservation> GetByStatus(ReservationStatus status)
    {
        string statusKey = status.ToString();
        if (!_statusReservations.TryGetValue(statusKey, out var ids))
            return new List<Reservation>();

        return ids
            .Where(id => _hashTable.ContainsKey(id))
            .Select(id => _hashTable[id])
            .ToList();
    }

    public List<Reservation> GetAll()
    {
        return _hashTable.Values.ToList();
    }

    public List<Reservation> Search(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return _hashTable.Values
            .Where(r =>
                r.ReservationId.ToLower().Contains(searchTerm) ||
                (r.Customer != null && r.Customer.Name.ToLower().Contains(searchTerm)) ||
                (r.Customer != null && r.Customer.Email.ToLower().Contains(searchTerm)) ||
                (r.Room != null && r.Room.RoomNumber.ToLower().Contains(searchTerm)) ||
                r.Status.ToString().ToLower().Contains(searchTerm))
            .ToList();
    }

    public bool Contains(string reservationId)
    {
        return _hashTable.ContainsKey(reservationId);
    }

    public void Clear()
    {
        _hashTable.Clear();
        _customerReservations.Clear();
        _roomReservations.Clear();
        _statusReservations.Clear();
    }
}
