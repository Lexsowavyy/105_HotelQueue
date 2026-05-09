using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public static class BinarySearcher
{
    public static int Search(List<Reservation> sortedList, DateTime targetDate, 
        Func<Reservation, DateTime> keySelector)
    {
        int left = 0;
        int right = sortedList.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DateTime midValue = keySelector(sortedList[mid]);

            if (midValue == targetDate)
                return mid;
            else if (midValue < targetDate)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }

    public static List<int> SearchRange(List<Reservation> sortedList, DateTime startDate, DateTime endDate,
        Func<Reservation, DateTime> keySelector)
    {
        var results = new List<int>();
        int startIndex = FindFirstIndex(sortedList, startDate, keySelector);
        int endIndex = FindLastIndex(sortedList, endDate, keySelector);

        if (startIndex != -1 && endIndex != -1)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                results.Add(i);
            }
        }

        return results;
    }

    public static int SearchByPrice(List<Reservation> sortedList, decimal targetPrice,
        Func<Reservation, decimal> priceSelector)
    {
        int left = 0;
        int right = sortedList.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            decimal midValue = priceSelector(sortedList[mid]);

            if (midValue == targetPrice)
                return mid;
            else if (midValue < targetPrice)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }

    public static List<Reservation> FindReservationsInDateRange(List<Reservation> reservations, 
        DateTime checkInStart, DateTime checkInEnd)
    {
        var sorted = QuickSorter.SortByCheckInDate(reservations);
        var indices = SearchRange(sorted, checkInStart, checkInEnd, r => r.CheckInDate);
        return indices.Select(i => sorted[i]).ToList();
    }

    public static List<Reservation> FindReservationsByStatus(List<Reservation> reservations, 
        ReservationStatus status)
    {
        var sorted = QuickSorter.SortByStatus(reservations);
        int index = SearchByStatus(sorted, status);
        
        if (index == -1)
            return new List<Reservation>();

        var results = new List<Reservation> { sorted[index] };
        
        int left = index - 1;
        while (left >= 0 && sorted[left].Status == status)
        {
            results.Insert(0, sorted[left]);
            left--;
        }

        int right = index + 1;
        while (right < sorted.Count && sorted[right].Status == status)
        {
            results.Add(sorted[right]);
            right++;
        }

        return results;
    }

    private static int SearchByStatus(List<Reservation> sortedList, ReservationStatus targetStatus)
    {
        int left = 0;
        int right = sortedList.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            ReservationStatus midValue = sortedList[mid].Status;

            if (midValue == targetStatus)
                return mid;
            else if (midValue < targetStatus)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }

    private static int FindFirstIndex(List<Reservation> sortedList, DateTime targetDate,
        Func<Reservation, DateTime> keySelector)
    {
        int left = 0;
        int right = sortedList.Count - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DateTime midValue = keySelector(sortedList[mid]);

            if (midValue >= targetDate)
            {
                if (midValue == targetDate)
                    result = mid;
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return result;
    }

    private static int FindLastIndex(List<Reservation> sortedList, DateTime targetDate,
        Func<Reservation, DateTime> keySelector)
    {
        int left = 0;
        int right = sortedList.Count - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DateTime midValue = keySelector(sortedList[mid]);

            if (midValue <= targetDate)
            {
                if (midValue == targetDate)
                    result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    public static int SearchByPriority(List<Reservation> sortedList, int targetPriority)
    {
        int left = 0;
        int right = sortedList.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int midValue = sortedList[mid].Priority;

            if (midValue == targetPriority)
                return mid;
            else if (midValue < targetPriority)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }
}
