using HotelQueue.Models;

namespace HotelQueue.DataStructures;

public static class QuickSorter
{
    public static List<Reservation> SortByCheckInDate(List<Reservation> reservations, bool ascending = true)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.CheckInDate.CompareTo(r2.CheckInDate) 
            : r2.CheckInDate.CompareTo(r1.CheckInDate));
        return list;
    }

    public static List<Reservation> SortByCreatedDate(List<Reservation> reservations, bool ascending = true)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.CreatedAt.CompareTo(r2.CreatedAt) 
            : r2.CreatedAt.CompareTo(r1.CreatedAt));
        return list;
    }

    public static List<Reservation> SortByPriority(List<Reservation> reservations, bool ascending = false)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.Priority.CompareTo(r2.Priority) 
            : r2.Priority.CompareTo(r1.Priority));
        return list;
    }

    public static List<Reservation> SortByTotalPrice(List<Reservation> reservations, bool ascending = true)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.TotalPrice.CompareTo(r2.TotalPrice) 
            : r2.TotalPrice.CompareTo(r1.TotalPrice));
        return list;
    }

    public static List<Reservation> SortByStatus(List<Reservation> reservations, bool ascending = true)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.Status.CompareTo(r2.Status) 
            : r2.Status.CompareTo(r1.Status));
        return list;
    }

    public static List<Reservation> SortByCustomerType(List<Reservation> reservations, bool ascending = false)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) => ascending 
            ? r1.CustomerType.CompareTo(r2.CustomerType) 
            : r2.CustomerType.CompareTo(r1.CustomerType));
        return list;
    }

    public static List<Reservation> MultiKeySort(List<Reservation> reservations, bool ascending = true)
    {
        var list = new List<Reservation>(reservations);
        QuickSort(list, 0, list.Count - 1, (r1, r2) =>
        {
            int customerTypeCompare = r2.CustomerType.CompareTo(r1.CustomerType);
            if (customerTypeCompare != 0)
                return ascending ? -customerTypeCompare : customerTypeCompare;

            int priorityCompare = r2.Priority.CompareTo(r1.Priority);
            if (priorityCompare != 0)
                return ascending ? -priorityCompare : priorityCompare;

            return ascending 
                ? r1.CreatedAt.CompareTo(r2.CreatedAt) 
                : r2.CreatedAt.CompareTo(r1.CreatedAt);
        });
        return list;
    }

    private static void QuickSort(List<Reservation> list, int low, int high, Func<Reservation, Reservation, int> comparison)
    {
        if (low < high)
        {
            int pivotIndex = Partition(list, low, high, comparison);
            QuickSort(list, low, pivotIndex - 1, comparison);
            QuickSort(list, pivotIndex + 1, high, comparison);
        }
    }

    private static int Partition(List<Reservation> list, int low, int high, Func<Reservation, Reservation, int> comparison)
    {
        Reservation pivot = list[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (comparison(list[j], pivot) <= 0)
            {
                i++;
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        (list[i + 1], list[high]) = (list[high], list[i + 1]);
        return i + 1;
    }

    public static PerformanceMetrics MeasureSortPerformance(List<Reservation> reservations, string sortType)
    {
        var list = new List<Reservation>(reservations);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int comparisons = 0;
        int swaps = 0;

        QuickSortWithMetrics(list, 0, list.Count - 1, (r1, r2) =>
        {
            comparisons++;
            return r1.CreatedAt.CompareTo(r2.CreatedAt);
        }, ref swaps);

        stopwatch.Stop();

        return new PerformanceMetrics
        {
            Operation = $"QuickSort_{sortType}",
            ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds,
            Comparisons = comparisons,
            Swaps = swaps,
            DataSize = reservations.Count
        };
    }

    private static void QuickSortWithMetrics(List<Reservation> list, int low, int high, 
        Func<Reservation, Reservation, int> comparison, ref int swaps)
    {
        if (low < high)
        {
            int pivotIndex = PartitionWithMetrics(list, low, high, comparison, ref swaps);
            QuickSortWithMetrics(list, low, pivotIndex - 1, comparison, ref swaps);
            QuickSortWithMetrics(list, pivotIndex + 1, high, comparison, ref swaps);
        }
    }

    private static int PartitionWithMetrics(List<Reservation> list, int low, int high, 
        Func<Reservation, Reservation, int> comparison, ref int swaps)
    {
        Reservation pivot = list[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (comparison(list[j], pivot) <= 0)
            {
                i++;
                (list[i], list[j]) = (list[j], list[i]);
                swaps++;
            }
        }

        (list[i + 1], list[high]) = (list[high], list[i + 1]);
        swaps++;
        return i + 1;
    }
}

public class PerformanceMetrics
{
    public string Operation { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public int Comparisons { get; set; }
    public int Swaps { get; set; }
    public int DataSize { get; set; }
}
