using HotelQueue.DataStructures;
using HotelQueue.DataStructures.Baseline;
using HotelQueue.Models;
using HotelQueue.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelQueue.Controllers;

public class BaselineController : Controller
{
    private readonly BaselineReservationService _baselineService;
    private readonly ReservationService _optimizedService;
    private readonly DashboardService _dashboardService;
    private readonly BaselineLinearSearcher _linearSearcher;
    private readonly BaselineBasicSorter _basicSorter;

    public BaselineController(
        BaselineReservationService baselineService,
        ReservationService optimizedService,
        DashboardService dashboardService)
    {
        _baselineService = baselineService;
        _optimizedService = optimizedService;
        _dashboardService = dashboardService;
        _linearSearcher = new BaselineLinearSearcher();
        _basicSorter = new BaselineBasicSorter();
    }

    public IActionResult Index(int dataSize = 100)
    {
        var allReservations = _optimizedService.GetAllReservations();
        var testData = allReservations.Take(dataSize).ToList();

        var baselineSortMetrics = _basicSorter.MeasureSortPerformance(testData);
        var optimizedSortMetrics = _optimizedService.GetSortPerformance("CheckInDate", dataSize);

        var baselineSearchTime = MeasureBaselineSearch(testData);
        var optimizedSearchTime = MeasureOptimizedSearch(testData);

        var viewModel = new BaselineComparisonViewModel
        {
            DataSize = dataSize,
            TotalReservations = allReservations.Count,

            BaselineSortTimeMs = baselineSortMetrics.ExecutionTimeMs,
            BaselineSortComparisons = baselineSortMetrics.Comparisons,
            BaselineSortSwaps = baselineSortMetrics.Swaps,
            OptimizedSortTimeMs = optimizedSortMetrics.ExecutionTimeMs,
            OptimizedSortComparisons = optimizedSortMetrics.Comparisons,
            OptimizedSortSwaps = optimizedSortMetrics.Swaps,

            BaselineSearchTimeMs = baselineSearchTime,
            OptimizedSearchTimeMs = optimizedSearchTime,

            BaselineSearchComplexity = "O(n) - Linear Search",
            OptimizedSearchComplexity = "O(1) avg - HashTable / O(log n) - Binary Search",
            BaselineSortComplexity = "O(n\u00B2) - Bubble Sort",
            OptimizedSortComplexity = "O(n log n) - Quick Sort",
            BaselineQueueType = "Single FIFO Queue - FCFS for ALL customers",
            OptimizedQueueType = "PriorityQueue (Heap) for VIP + FIFOQueue for Regular",
            BaselineAllocationType = "Sequential - First available room",
            OptimizedAllocationType = "Greedy Algorithm - Cheapest suitable room, VIP first",
            BaselineLookupType = "Linear Scan O(n) - No indexing",
            OptimizedLookupType = "HashTable O(1) - With secondary indexes",
            BaselineWaitlistType = "Manual staff intervention",
            OptimizedWaitlistType = "Auto-promote via Queue + Heap operations",
            BaselineCancellationType = "Manual reassignment",
            OptimizedCancellationType = "Real-time auto reallocate + waitlist promote"
        };

        return View(viewModel);
    }

    public IActionResult RunSortTest(int dataSize = 100)
    {
        var allReservations = _optimizedService.GetAllReservations();
        var testData = allReservations.Take(dataSize).ToList();

        var baselineTime = _basicSorter.MeasureSortPerformance(testData);
        var optimizedTime = _optimizedService.GetSortPerformance("CheckInDate", dataSize);

        var improvement = baselineTime.ExecutionTimeMs > 0
            ? Math.Round((baselineTime.ExecutionTimeMs - optimizedTime.ExecutionTimeMs) / baselineTime.ExecutionTimeMs * 100, 2)
            : 0;

        return Json(new
        {
            dataSize,
            baselineMs = Math.Round(baselineTime.ExecutionTimeMs, 3),
            optimizedMs = Math.Round(optimizedTime.ExecutionTimeMs, 3),
            improvementPercent = improvement,
            baselineComparisons = baselineTime.Comparisons,
            optimizedComparisons = optimizedTime.Comparisons
        });
    }

    public IActionResult RunSearchTest(int dataSize = 100)
    {
        var allReservations = _optimizedService.GetAllReservations();
        var testData = allReservations.Take(dataSize).ToList();

        var baselineTime = MeasureBaselineSearch(testData);
        var optimizedTime = MeasureOptimizedSearch(testData);

        var improvement = baselineTime > 0
            ? Math.Round((baselineTime - optimizedTime) / baselineTime * 100, 2)
            : 0;

        return Json(new
        {
            dataSize,
            baselineMs = Math.Round(baselineTime, 3),
            optimizedMs = Math.Round(optimizedTime, 3),
            improvementPercent = improvement
        });
    }

    private double MeasureBaselineSearch(List<Reservation> data)
    {
        if (data.Count == 0) return 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            var target = data[i % data.Count].ReservationId;
            _linearSearcher.FindById(data, target);
        }

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private double MeasureOptimizedSearch(List<Reservation> data)
    {
        if (data.Count == 0) return 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var hashTable = new ReservationHashTable();
        foreach (var r in data)
            hashTable.Add(r);

        for (int i = 0; i < 100; i++)
        {
            var target = data[i % data.Count].ReservationId;
            hashTable.Get(target);
        }

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}

public class BaselineComparisonViewModel
{
    public int DataSize { get; set; }
    public int TotalReservations { get; set; }

    public double BaselineSortTimeMs { get; set; }
    public int BaselineSortComparisons { get; set; }
    public int BaselineSortSwaps { get; set; }
    public double OptimizedSortTimeMs { get; set; }
    public int OptimizedSortComparisons { get; set; }
    public int OptimizedSortSwaps { get; set; }

    public double BaselineSearchTimeMs { get; set; }
    public double OptimizedSearchTimeMs { get; set; }

    public string BaselineSearchComplexity { get; set; } = "";
    public string OptimizedSearchComplexity { get; set; } = "";
    public string BaselineSortComplexity { get; set; } = "";
    public string OptimizedSortComplexity { get; set; } = "";
    public string BaselineQueueType { get; set; } = "";
    public string OptimizedQueueType { get; set; } = "";
    public string BaselineAllocationType { get; set; } = "";
    public string OptimizedAllocationType { get; set; } = "";
    public string BaselineLookupType { get; set; } = "";
    public string OptimizedLookupType { get; set; } = "";
    public string BaselineWaitlistType { get; set; } = "";
    public string OptimizedWaitlistType { get; set; } = "";
    public string BaselineCancellationType { get; set; } = "";
    public string OptimizedCancellationType { get; set; } = "";

    public double SortImprovementPercent =>
        BaselineSortTimeMs > 0 ? Math.Round((BaselineSortTimeMs - OptimizedSortTimeMs) / BaselineSortTimeMs * 100, 2) : 0;
    public double SearchImprovementPercent =>
        BaselineSearchTimeMs > 0 ? Math.Round((BaselineSearchTimeMs - OptimizedSearchTimeMs) / BaselineSearchTimeMs * 100, 2) : 0;
}
