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
    private readonly PerformanceBenchmarkService _benchmarkService;
    private const int DefaultRuns = 5;

    public BaselineController(
        BaselineReservationService baselineService,
        ReservationService optimizedService,
        DashboardService dashboardService,
        PerformanceBenchmarkService benchmarkService)
    {
        _baselineService = baselineService;
        _optimizedService = optimizedService;
        _dashboardService = dashboardService;
        _benchmarkService = benchmarkService;
    }

    public IActionResult Index(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        var viewModel = new BaselineComparisonViewModel
        {
            DataSize = result.DataSize,
            RunCount = result.Runs,
            TotalReservations = _optimizedService.GetAllReservations().Count,

            BaselineSortTimeMs = result.BaselineSortMs,
            BaselineSortComparisons = result.BaselineSortComparisons,
            BaselineSortSwaps = result.BaselineSortSwaps,
            OptimizedSortTimeMs = result.OptimizedSortMs,
            OptimizedSortComparisons = result.OptimizedSortComparisons,
            OptimizedSortSwaps = result.OptimizedSortSwaps,

            BaselineSearchTimeMs = result.BaselineSearchMs,
            OptimizedSearchTimeMs = result.OptimizedSearchMs,

            BaselineQueueInsertMs = result.BaselineQueueInsertMs,
            OptimizedQueueInsertMs = result.OptimizedQueueInsertMs,
            BaselineQueueExtractMs = result.BaselineQueueExtractMs,
            OptimizedQueueExtractMs = result.OptimizedQueueExtractMs,

            BaselineAllocationMs = result.BaselineAllocationMs,
            OptimizedAllocationMs = result.OptimizedAllocationMs,

            BaselineCancelMs = result.BaselineCancelMs,
            OptimizedCancelMs = result.OptimizedCancelMs,

            BaselineSearchComplexity = "O(n) - Linear Search",
            OptimizedSearchComplexity = "O(1) avg - HashTable / O(log n) - Binary Search",
            BaselineSortComplexity = "O(n\u00B2) - Bubble Sort",
            OptimizedSortComplexity = "O(n log n) average case, O(n\u00B2) worst case - Quick Sort",
            BaselineQueueType = "Single FIFO Queue - FCFS for ALL customers",
            OptimizedQueueType = "PriorityQueue (Heap) for VIP + FIFOQueue for Regular",
            BaselineAllocationType = "Sequential (O(n\u00D7m)) - First available room",
            OptimizedAllocationType = "Greedy (O(n\u00D7m)) - Cheapest suitable, VIP first (better quality)",
            BaselineLookupType = "Linear Scan O(n) - No indexing",
            OptimizedLookupType = "HashTable (Dictionary-backed) O(1) avg + custom secondary indexes",
            BaselineWaitlistType = "Manual staff intervention",
            OptimizedWaitlistType = "Auto-promote via Queue + Heap operations",
            BaselineCancellationType = "Manual reassignment",
            OptimizedCancellationType = "Real-time auto reallocate + waitlist promote"
        };

        return View(viewModel);
    }

    public IActionResult RunSortTest(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            baselineMs = Math.Round(result.BaselineSortMs, 3),
            optimizedMs = Math.Round(result.OptimizedSortMs, 3),
            improvementPercent = result.SortImprovementPercent,
            baselineComparisons = result.BaselineSortComparisons,
            optimizedComparisons = result.OptimizedSortComparisons
        });
    }

    public IActionResult RunSearchTest(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            lookups = 100,
            baselineMs = Math.Round(result.BaselineSearchMs, 3),
            optimizedMs = Math.Round(result.OptimizedSearchMs, 3),
            improvementPercent = result.SearchImprovementPercent
        });
    }

    public IActionResult RunQueueTest(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            baselineInsertMs = Math.Round(result.BaselineQueueInsertMs, 3),
            optimizedInsertMs = Math.Round(result.OptimizedQueueInsertMs, 3),
            baselineExtractMs = Math.Round(result.BaselineQueueExtractMs, 3),
            optimizedExtractMs = Math.Round(result.OptimizedQueueExtractMs, 3),
            insertImprovement = result.QueueInsertImprovementPercent,
            extractImprovement = result.QueueExtractImprovementPercent
        });
    }

    public IActionResult RunAllocationTest(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            baselineMs = Math.Round(result.BaselineAllocationMs, 3),
            optimizedMs = Math.Round(result.OptimizedAllocationMs, 3),
            improvementPercent = result.AllocationImprovementPercent
        });
    }

    public IActionResult RunCancelTest(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            baselineMs = Math.Round(result.BaselineCancelMs, 3),
            optimizedMs = Math.Round(result.OptimizedCancelMs, 3),
            improvementPercent = result.CancelImprovementPercent
        });
    }

    public IActionResult RunAllTests(int dataSize = 100)
    {
        var result = _benchmarkService.RunBenchmark(dataSize, DefaultRuns);

        return Json(new
        {
            dataSize,
            runs = DefaultRuns,
            sort = new
            {
                baselineMs = Math.Round(result.BaselineSortMs, 3),
                optimizedMs = Math.Round(result.OptimizedSortMs, 3),
                improvementPercent = result.SortImprovementPercent
            },
            search = new
            {
                baselineMs = Math.Round(result.BaselineSearchMs, 3),
                optimizedMs = Math.Round(result.OptimizedSearchMs, 3),
                improvementPercent = result.SearchImprovementPercent
            },
            queue = new
            {
                insert = new
                {
                    baselineMs = Math.Round(result.BaselineQueueInsertMs, 3),
                    optimizedMs = Math.Round(result.OptimizedQueueInsertMs, 3),
                    improvementPercent = result.QueueInsertImprovementPercent
                },
                extract = new
                {
                    baselineMs = Math.Round(result.BaselineQueueExtractMs, 3),
                    optimizedMs = Math.Round(result.OptimizedQueueExtractMs, 3),
                    improvementPercent = result.QueueExtractImprovementPercent
                }
            },
            allocation = new
            {
                baselineMs = Math.Round(result.BaselineAllocationMs, 3),
                optimizedMs = Math.Round(result.OptimizedAllocationMs, 3),
                improvementPercent = result.AllocationImprovementPercent
            },
            cancellation = new
            {
                baselineMs = Math.Round(result.BaselineCancelMs, 3),
                optimizedMs = Math.Round(result.OptimizedCancelMs, 3),
                improvementPercent = result.CancelImprovementPercent
            },
            totals = new
            {
                baselineMs = Math.Round(result.BaselineTotalMs, 3),
                optimizedMs = Math.Round(result.OptimizedTotalMs, 3),
                improvementPercent = result.OverallImprovementPercent
            }
        });
    }

    private List<Reservation> PrepareTestData(int dataSize)
    {
        var allReservations = _optimizedService.GetAllReservations();
        if (allReservations.Count >= dataSize)
            return allReservations.Take(dataSize).ToList();

        var result = new List<Reservation>(allReservations);
        var random = new Random();
        while (result.Count < dataSize)
        {
            result.Add(new Reservation(
                $"SMPL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                random.Next(1, 100),
                random.Next(1, 20),
                DateTime.Now.AddDays(random.Next(1, 30)),
                DateTime.Now.AddDays(random.Next(31, 60)),
                random.Next(2) == 0 ? CustomerType.VIP : CustomerType.Regular
            ));
        }
        return result;
    }

    
    
    public IActionResult RunFullBenchmarkSuite()
    {
        var config = new PerformanceBenchmarkService.BenchmarkConfig
        {
            SmallSize = 100,
            MediumSize = 500,
            LargeSize = 1000,
            Runs = 5
        };

        var results = _benchmarkService.RunFullBenchmarkSuite(config);

        return Json(new
        {
            config = new
            {
                smallSize = config.SmallSize,
                mediumSize = config.MediumSize,
                largeSize = config.LargeSize,
                runs = config.Runs
            },
            results = results.Select(r => new
            {
                dataSize = r.DataSize,
                runs = r.Runs,
                sort = new
                {
                    baselineMs = Math.Round(r.BaselineSortMs, 3),
                    optimizedMs = Math.Round(r.OptimizedSortMs, 3),
                    improvementPercent = r.SortImprovementPercent,
                    baselineComparisons = r.BaselineSortComparisons,
                    optimizedComparisons = r.OptimizedSortComparisons,
                    baselineSwaps = r.BaselineSortSwaps,
                    optimizedSwaps = r.OptimizedSortSwaps
                },
                search = new
                {
                    baselineMs = Math.Round(r.BaselineSearchMs, 3),
                    optimizedMs = Math.Round(r.OptimizedSearchMs, 3),
                    improvementPercent = r.SearchImprovementPercent
                },
                queue = new
                {
                    insert = new
                    {
                        baselineMs = Math.Round(r.BaselineQueueInsertMs, 3),
                        optimizedMs = Math.Round(r.OptimizedQueueInsertMs, 3),
                        improvementPercent = r.QueueInsertImprovementPercent
                    },
                    extract = new
                    {
                        baselineMs = Math.Round(r.BaselineQueueExtractMs, 3),
                        optimizedMs = Math.Round(r.OptimizedQueueExtractMs, 3),
                        improvementPercent = r.QueueExtractImprovementPercent
                    }
                },
                allocation = new
                {
                    baselineMs = Math.Round(r.BaselineAllocationMs, 3),
                    optimizedMs = Math.Round(r.OptimizedAllocationMs, 3),
                    improvementPercent = r.AllocationImprovementPercent
                },
                cancellation = new
                {
                    baselineMs = Math.Round(r.BaselineCancelMs, 3),
                    optimizedMs = Math.Round(r.OptimizedCancelMs, 3),
                    improvementPercent = r.CancelImprovementPercent
                }
            })
        });
    }
}

public class BaselineComparisonViewModel
{
    public int DataSize { get; set; }
    public int RunCount { get; set; } = 5;
    public int TotalReservations { get; set; }

    public double BaselineSortTimeMs { get; set; }
    public int BaselineSortComparisons { get; set; }
    public int BaselineSortSwaps { get; set; }
    public double OptimizedSortTimeMs { get; set; }
    public int OptimizedSortComparisons { get; set; }
    public int OptimizedSortSwaps { get; set; }

    public double BaselineSearchTimeMs { get; set; }
    public double OptimizedSearchTimeMs { get; set; }

    public double BaselineQueueInsertMs { get; set; }
    public double OptimizedQueueInsertMs { get; set; }
    public double BaselineQueueExtractMs { get; set; }
    public double OptimizedQueueExtractMs { get; set; }

    public double BaselineAllocationMs { get; set; }
    public double OptimizedAllocationMs { get; set; }

    public double BaselineCancelMs { get; set; }
    public double OptimizedCancelMs { get; set; }

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
    public double QueueInsertImprovementPercent =>
        BaselineQueueInsertMs > 0 ? Math.Round((BaselineQueueInsertMs - OptimizedQueueInsertMs) / BaselineQueueInsertMs * 100, 2) : 0;
    public double QueueExtractImprovementPercent =>
        BaselineQueueExtractMs > 0 ? Math.Round((BaselineQueueExtractMs - OptimizedQueueExtractMs) / BaselineQueueExtractMs * 100, 2) : 0;
    public double AllocationImprovementPercent =>
        BaselineAllocationMs > 0 ? Math.Round((BaselineAllocationMs - OptimizedAllocationMs) / BaselineAllocationMs * 100, 2) : 0;
    public double CancelImprovementPercent =>
        BaselineCancelMs > 0 ? Math.Round((BaselineCancelMs - OptimizedCancelMs) / BaselineCancelMs * 100, 2) : 0;

    // Aggregate totals
    public double BaselineTotalMs =>
        BaselineSortTimeMs + BaselineSearchTimeMs + BaselineQueueInsertMs +
        BaselineQueueExtractMs + BaselineAllocationMs + BaselineCancelMs;
    public double OptimizedTotalMs =>
        OptimizedSortTimeMs + OptimizedSearchTimeMs + OptimizedQueueInsertMs +
        OptimizedQueueExtractMs + OptimizedAllocationMs + OptimizedCancelMs;
    public double OverallImprovementPercent =>
        BaselineTotalMs > 0 ? Math.Round((BaselineTotalMs - OptimizedTotalMs) / BaselineTotalMs * 100, 2) : 0;
}


