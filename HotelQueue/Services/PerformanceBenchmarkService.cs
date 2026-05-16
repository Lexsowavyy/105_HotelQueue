using HotelQueue.DataStructures;
using HotelQueue.DataStructures.Baseline;
using HotelQueue.Models;

namespace HotelQueue.Services;

public class PerformanceBenchmarkService
{
    private readonly ReservationService _optimizedService;
    private readonly BaselineReservationService _baselineService;
    private readonly RoomService _roomService;
    private const int DefaultRuns = 5;

    public PerformanceBenchmarkService(
        ReservationService optimizedService,
        BaselineReservationService baselineService,
        RoomService roomService)
    {
        _optimizedService = optimizedService;
        _baselineService = baselineService;
        _roomService = roomService;
    }

    public class BenchmarkConfig
    {
        public int SmallSize { get; set; } = 100;
        public int MediumSize { get; set; } = 500;
        public int LargeSize { get; set; } = 1000;
        public int Runs { get; set; } = 5;
        public int SearchIterations { get; set; } = 100;
    }

    public class BenchmarkResult
    {
        public int DataSize { get; set; }
        public int Runs { get; set; }
        
        // Sort Performance
        public double BaselineSortMs { get; set; }
        public double OptimizedSortMs { get; set; }
        public int BaselineSortComparisons { get; set; }
        public int OptimizedSortComparisons { get; set; }
        public int BaselineSortSwaps { get; set; }
        public int OptimizedSortSwaps { get; set; }
        public double SortImprovementPercent { get; set; }
        
        // Search Performance
        public double BaselineSearchMs { get; set; }
        public double OptimizedSearchMs { get; set; }
        public double SearchImprovementPercent { get; set; }
        
        // Queue Insert Performance
        public double BaselineQueueInsertMs { get; set; }
        public double OptimizedQueueInsertMs { get; set; }
        public double QueueInsertImprovementPercent { get; set; }
        
        // Queue Extract Performance
        public double BaselineQueueExtractMs { get; set; }
        public double OptimizedQueueExtractMs { get; set; }
        public double QueueExtractImprovementPercent { get; set; }
        
        // Allocation Performance
        public double BaselineAllocationMs { get; set; }
        public double OptimizedAllocationMs { get; set; }
        public double AllocationImprovementPercent { get; set; }
        
        // Cancellation/Reassignment Performance
        public double BaselineCancelMs { get; set; }
        public double OptimizedCancelMs { get; set; }
        public double CancelImprovementPercent { get; set; }

        // Aggregate (Total) Performance
        public double BaselineTotalMs =>
            BaselineSortMs + BaselineSearchMs + BaselineQueueInsertMs +
            BaselineQueueExtractMs + BaselineAllocationMs + BaselineCancelMs;
        public double OptimizedTotalMs =>
            OptimizedSortMs + OptimizedSearchMs + OptimizedQueueInsertMs +
            OptimizedQueueExtractMs + OptimizedAllocationMs + OptimizedCancelMs;
        public double OverallImprovementPercent =>
            BaselineTotalMs > 0
                ? Math.Round((BaselineTotalMs - OptimizedTotalMs) / BaselineTotalMs * 100, 2)
                : 0;
    }

    public BenchmarkResult RunBenchmark(int dataSize, int runs = 5)
    {
        var testData = PrepareTestData(dataSize);
        
        // Run all benchmarks
        var (baselineSortTime, baselineSortComps, baselineSortSwaps) = RunBaselineSortBenchmark(testData, runs);
        var (optimizedSortTime, optimizedSortComps, optimizedSortSwaps) = RunOptimizedSortBenchmark(testData, runs);
        var (baselineSearchTime, optimizedSearchTime) = RunSearchBenchmark(testData, runs);
        var (baselineQueueInsert, optimizedQueueInsert) = RunQueueInsertBenchmark(testData, runs);
        var (baselineQueueExtract, optimizedQueueExtract) = RunQueueExtractBenchmark(testData, runs);
        var (baselineAllocTime, optimizedAllocTime) = RunAllocationBenchmark(testData, runs);
        var (baselineCancelTime, optimizedCancelTime) = RunCancellationBenchmark(testData, runs);

        return new BenchmarkResult
        {
            DataSize = dataSize,
            Runs = runs,
            
            BaselineSortMs = baselineSortTime,
            OptimizedSortMs = optimizedSortTime,
            BaselineSortComparisons = baselineSortComps,
            OptimizedSortComparisons = optimizedSortComps,
            BaselineSortSwaps = baselineSortSwaps,
            OptimizedSortSwaps = optimizedSortSwaps,
            SortImprovementPercent = baselineSortTime > 0 
                ? Math.Round((baselineSortTime - optimizedSortTime) / baselineSortTime * 100, 2) : 0,
            
            BaselineSearchMs = baselineSearchTime,
            OptimizedSearchMs = optimizedSearchTime,
            SearchImprovementPercent = baselineSearchTime > 0 
                ? Math.Round((baselineSearchTime - optimizedSearchTime) / baselineSearchTime * 100, 2) : 0,
            
            BaselineQueueInsertMs = baselineQueueInsert,
            OptimizedQueueInsertMs = optimizedQueueInsert,
            QueueInsertImprovementPercent = baselineQueueInsert > 0 
                ? Math.Round((baselineQueueInsert - optimizedQueueInsert) / baselineQueueInsert * 100, 2) : 0,
            
            BaselineQueueExtractMs = baselineQueueExtract,
            OptimizedQueueExtractMs = optimizedQueueExtract,
            QueueExtractImprovementPercent = baselineQueueExtract > 0 
                ? Math.Round((baselineQueueExtract - optimizedQueueExtract) / baselineQueueExtract * 100, 2) : 0,
            
            BaselineAllocationMs = baselineAllocTime,
            OptimizedAllocationMs = optimizedAllocTime,
            AllocationImprovementPercent = baselineAllocTime > 0 
                ? Math.Round((baselineAllocTime - optimizedAllocTime) / baselineAllocTime * 100, 2) : 0,
            
            BaselineCancelMs = baselineCancelTime,
            OptimizedCancelMs = optimizedCancelTime,
            CancelImprovementPercent = baselineCancelTime > 0 
                ? Math.Round((baselineCancelTime - optimizedCancelTime) / baselineCancelTime * 100, 2) : 0
        };
    }

    public List<BenchmarkResult> RunFullBenchmarkSuite(BenchmarkConfig? config = null)
    {
        config ??= new BenchmarkConfig();
        var results = new List<BenchmarkResult>();

        // Run benchmarks for each size
        var sizes = new[] { config.SmallSize, config.MediumSize, config.LargeSize };
        
        foreach (var size in sizes)
        {
            var result = RunBenchmark(size, config.Runs);
            results.Add(result);
        }

        return results;
    }

    private List<Reservation> PrepareTestData(int dataSize)
    {
        var allReservations = _optimizedService.GetAllReservations();
        if (allReservations.Count >= dataSize)
            return allReservations.Take(dataSize).ToList();

        var result = new List<Reservation>(allReservations);
        var random = new Random(42); // Fixed seed for reproducible results
        
        while (result.Count < dataSize)
        {
            result.Add(new Reservation(
                $"BENCH-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                random.Next(1, 100),
                random.Next(1, 20),
                DateTime.Now.AddDays(random.Next(1, 30)),
                DateTime.Now.AddDays(random.Next(31, 60)),
                random.Next(2) == 0 ? CustomerType.VIP : CustomerType.Regular
            ));
        }
        return result;
    }

    private List<Room> PrepareTestRooms()
    {
        var rooms = new List<Room>();
        for (int i = 1; i <= 20; i++)
        {
            rooms.Add(new Room
            {
                RoomId = i,
                RoomNumber = $"{100 + i}",
                Type = (RoomType)(i % 3),
                PricePerNight = 100 + (i * 25),
                Capacity = (i % 2) + 1,
                IsAvailable = true
            });
        }
        return rooms;
    }

    private (double avgTimeMs, int comparisons, int swaps) RunBaselineSortBenchmark(List<Reservation> data, int runs)
    {
        double totalTime = 0;
        PerformanceMetrics? last = null;
        var basicSorter = new BaselineBasicSorter();
        
        for (int i = 0; i < runs; i++)
        {
            var metrics = basicSorter.MeasureSortPerformance(data);
            totalTime += metrics.ExecutionTimeMs;
            last = metrics;
        }
        return (totalTime / runs, last?.Comparisons ?? 0, last?.Swaps ?? 0);
    }

    private (double avgTimeMs, int comparisons, int swaps) RunOptimizedSortBenchmark(List<Reservation> data, int runs)
    {
        double totalTime = 0;
        PerformanceMetrics? last = null;
        
        for (int i = 0; i < runs; i++)
        {
            var metrics = QuickSorter.MeasureSortPerformance(data, "CheckInDate");
            totalTime += metrics.ExecutionTimeMs;
            last = metrics;
        }
        return (totalTime / runs, last?.Comparisons ?? 0, last?.Swaps ?? 0);
    }

    private (double baselineAvg, double optimizedAvg) RunSearchBenchmark(List<Reservation> data, int runs)
    {
        if (data.Count == 0) return (0, 0);

        var hashTable = new ReservationHashTable();
        foreach (var r in data)
            hashTable.Add(r);

        double baselineTotal = 0;
        double optimizedTotal = 0;
        var linearSearcher = new BaselineLinearSearcher();

        for (int run = 0; run < runs; run++)
        {
            // Baseline: Linear Search
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var target = data[i % data.Count].ReservationId;
                linearSearcher.FindById(data, target);
            }
            sw.Stop();
            baselineTotal += sw.Elapsed.TotalMilliseconds;

            // Optimized: Hash Table Lookup
            sw.Restart();
            for (int i = 0; i < 100; i++)
            {
                var target = data[i % data.Count].ReservationId;
                hashTable.Get(target);
            }
            sw.Stop();
            optimizedTotal += sw.Elapsed.TotalMilliseconds;
        }

        return (baselineTotal / runs, optimizedTotal / runs);
    }

    private (double baselineAvg, double optimizedAvg) RunQueueInsertBenchmark(List<Reservation> data, int runs)
    {
        double baselineTotal = 0;
        double optimizedTotal = 0;

        for (int r = 0; r < runs; r++)
        {
            // Baseline: Single Queue
            var baselineQueue = new BaselineSingleQueue();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (var res in data)
                baselineQueue.Enqueue(res);
            sw.Stop();
            baselineTotal += sw.Elapsed.TotalMilliseconds;

            // Optimized: Priority Queue for VIP + FIFO for Regular
            var vipQueue = new PriorityQueue();
            var regQueue = new FIFOQueue();
            sw.Restart();
            foreach (var res in data)
            {
                if (res.CustomerType == CustomerType.VIP)
                    vipQueue.Enqueue(res);
                else
                    regQueue.Enqueue(res);
            }
            sw.Stop();
            optimizedTotal += sw.Elapsed.TotalMilliseconds;
        }

        return (baselineTotal / runs, optimizedTotal / runs);
    }

    private (double baselineAvg, double optimizedAvg) RunQueueExtractBenchmark(List<Reservation> data, int runs)
    {
        double baselineTotal = 0;
        double optimizedTotal = 0;

        for (int r = 0; r < runs; r++)
        {
            // Setup queues
            var baselineQueue = new BaselineSingleQueue();
            var vipQueue = new PriorityQueue();
            var regQueue = new FIFOQueue();
            
            foreach (var res in data)
            {
                baselineQueue.Enqueue(res);
                if (res.CustomerType == CustomerType.VIP)
                    vipQueue.Enqueue(res);
                else
                    regQueue.Enqueue(res);
            }

            // Baseline extraction
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (baselineQueue.Count > 0)
                baselineQueue.Dequeue();
            sw.Stop();
            baselineTotal += sw.Elapsed.TotalMilliseconds;

            // Optimized extraction
            sw.Restart();
            while (vipQueue.Count > 0)
                vipQueue.Dequeue();
            while (regQueue.Count > 0)
                regQueue.Dequeue();
            sw.Stop();
            optimizedTotal += sw.Elapsed.TotalMilliseconds;
        }

        return (baselineTotal / runs, optimizedTotal / runs);
    }

    private (double baselineAvg, double optimizedAvg) RunAllocationBenchmark(List<Reservation> data, int runs)
    {
        if (data.Count == 0) return (0, 0);

        var rooms = PrepareTestRooms();
        int iterations = Math.Min(data.Count, rooms.Count);

        double baselineTotal = 0;
        double optimizedTotal = 0;

        for (int r = 0; r < runs; r++)
        {
            // Setup baseline
            var baseQueue = new BaselineSingleQueue();
            foreach (var res in data)
                baseQueue.Enqueue(res);

            // Setup optimized
            var hashTable = new ReservationHashTable();
            var vipQueue = new PriorityQueue();
            var regQueue = new FIFOQueue();
            foreach (var res in data)
            {
                hashTable.Add(res);
                if (res.CustomerType == CustomerType.VIP)
                    vipQueue.Enqueue(res);
                else
                    regQueue.Enqueue(res);
            }

            var freshRooms = rooms.Select(rm => new Room
            {
                RoomId = rm.RoomId,
                RoomNumber = rm.RoomNumber,
                Type = rm.Type,
                PricePerNight = rm.PricePerNight,
                Capacity = rm.Capacity,
                IsAvailable = true
            }).ToList();

            // Baseline: Sequential Allocation
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var seqAllocator = new BaselineSequentialAllocator(baseQueue, freshRooms.Select(rm => new Room
            {
                RoomId = rm.RoomId,
                RoomNumber = rm.RoomNumber,
                Type = rm.Type,
                PricePerNight = rm.PricePerNight,
                Capacity = rm.Capacity,
                IsAvailable = true
            }).ToList());
            seqAllocator.AllocateRooms();
            sw.Stop();
            baselineTotal += sw.Elapsed.TotalMilliseconds;

            // Optimized: Greedy Allocation
            sw.Restart();
            var greedyAllocator = new GreedyAllocator(freshRooms, vipQueue, regQueue);
            greedyAllocator.AllocateRooms();
            sw.Stop();
            optimizedTotal += sw.Elapsed.TotalMilliseconds;
        }

        return (baselineTotal / runs, optimizedTotal / runs);
    }

    private (double baselineAvg, double optimizedAvg) RunCancellationBenchmark(List<Reservation> data, int runs)
    {
        if (data.Count == 0) return (0, 0);

        var rooms = PrepareTestRooms();
        int iterations = Math.Min(data.Count, rooms.Count);

        double baselineTotal = 0;
        double optimizedTotal = 0;

        for (int r = 0; r < runs; r++)
        {
            // Setup baseline
            var baseQueue = new BaselineSingleQueue();
            foreach (var res in data)
                baseQueue.Enqueue(res);

            // Setup optimized
            var hashTable = new ReservationHashTable();
            var vipQueue = new PriorityQueue();
            var regQueue = new FIFOQueue();
            foreach (var res in data)
            {
                hashTable.Add(res);
                if (res.CustomerType == CustomerType.VIP)
                    vipQueue.Enqueue(res);
                else
                    regQueue.Enqueue(res);
            }

            var baseRooms = rooms.Select(rm => new Room
            {
                RoomId = rm.RoomId, RoomNumber = rm.RoomNumber,
                Type = rm.Type, PricePerNight = rm.PricePerNight,
                Capacity = rm.Capacity, IsAvailable = true
            }).ToList();

            var optRooms = rooms.Select(rm => new Room
            {
                RoomId = rm.RoomId, RoomNumber = rm.RoomNumber,
                Type = rm.Type, PricePerNight = rm.PricePerNight,
                Capacity = rm.Capacity, IsAvailable = true
            }).ToList();

            var linearSearcher = new BaselineLinearSearcher();

            // Baseline: Manual cancellation and reassignment
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var targetId = data[i].ReservationId;
                linearSearcher.FindById(data, targetId);
                baseQueue.Remove(targetId);
                for (int j = 0; j < baseRooms.Count; j++)
                {
                    if (baseRooms[j].RoomId == data[i].RoomId)
                    {
                        baseRooms[j].IsAvailable = true;
                        break;
                    }
                }
                if (baseQueue.Count > 0)
                    baseQueue.Dequeue();
            }
            sw.Stop();
            baselineTotal += sw.Elapsed.TotalMilliseconds;

            // Optimized: Hash table lookup + automatic reallocation
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var targetId = data[i].ReservationId;
                var found = hashTable.Get(targetId);
                if (found != null)
                {
                    if (found.CustomerType == CustomerType.VIP)
                        vipQueue.Remove(targetId);
                    else
                        regQueue.Remove(targetId);
                }
                var greedyAllocator = new GreedyAllocator(optRooms, vipQueue, regQueue);
                greedyAllocator.ReallocateRoomOnCancellation(optRooms, found ?? data[i], vipQueue, regQueue);
            }
            sw.Stop();
            optimizedTotal += sw.Elapsed.TotalMilliseconds;
        }

        return (baselineTotal / runs, optimizedTotal / runs);
    }
}
