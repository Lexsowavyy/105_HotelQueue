# HotelQueue - Data Structures and Algorithms Implementation

## Overview

HotelQueue is a hotel reservation management system that demonstrates the practical application of various data structures and algorithms. The system compares baseline implementations (using simple, inefficient approaches) against optimized implementations using advanced data structures.

## Data Structures Implementation

### Optimized System Data Structures

#### 1. ReservationHashTable (`ReservationHashTable.cs`)
- **Primary Storage**: Uses C# `Dictionary<string, Reservation>` for O(1) average case lookup
- **Secondary Indexes**: Custom implementations for customer, room, and status-based queries
- **Disclosure**: This implementation uses library-backed hashing (Dictionary) with custom secondary indexes
- **Operations**: Add, Get, Remove, Update, Search, GetByCustomer, GetByRoom, GetByStatus

#### 2. Priority Queue (`PriorityQueue.cs`)
- **Implementation**: Heap-based priority queue for VIP customers
- **Complexity**: O(log n) insertion and extraction
- **Usage**: Manages VIP reservations with priority based on customer type and creation time

#### 3. FIFO Queue (`FIFOQueue.cs`)
- **Implementation**: Standard First-In-First-Out queue for regular customers
- **Complexity**: O(1) insertion and extraction
- **Usage**: Manages regular customer reservations in arrival order

#### 4. Binary Search (`BinarySearcher.cs`)
- **Implementation**: Custom binary search for sorted reservation lists
- **Complexity**: O(log n) search time
- **Usage**: Fast lookup in sorted reservation datasets

#### 5. Quick Sort (`QuickSorter.cs`)
- **Implementation**: Custom Quick Sort with median-of-three pivot selection
- **Complexity**: O(n log n) average case, O(n²) worst case
- **Features**: Multiple sorting criteria (check-in date, creation date, priority, price, status, customer type)
- **Performance Metrics**: Tracks comparisons and swaps for analysis

#### 6. Greedy Allocator (`GreedyAllocator.cs`)
- **Implementation**: Greedy algorithm for room allocation
- **Strategy**: Allocates cheapest suitable room, prioritizes VIP customers
- **Features**: Automatic reallocation on cancellations, waitlist promotion

### Baseline System Data Structures

#### 1. Baseline Linear Search (`BaselineLinearSearcher.cs`)
- **Implementation**: Simple linear search through reservation lists
- **Complexity**: O(n) search time
- **Usage**: Baseline comparison for search operations

#### 2. Baseline Basic Sorter (`BaselineBasicSorter.cs`)
- **Implementation**: Bubble Sort and Selection Sort
- **Complexity**: O(n²) sorting time
- **Usage**: Baseline comparison for sorting operations

#### 3. Baseline Single Queue (`BaselineSingleQueue.cs`)
- **Implementation**: Single FIFO queue for all customers
- **Complexity**: O(1) insertion and extraction
- **Usage**: No differentiation between VIP and regular customers

## Performance Benchmarking

### New Benchmarking System

The consultation feedback highlighted issues with the previous performance evaluation. The system now includes:

#### Test Sizes
- **Small**: 100 records (minimum recommended size)
- **Medium**: 500 records
- **Large**: 1000+ records

#### Testing Methodology
- Each test runs **5 times** and reports the average
- **Operation-specific measurements** (not full UI rendering)
- **Separate benchmarking** for each operation:
  - Search (100 lookups per test)
  - Sorting (Quick Sort vs Bubble Sort)
  - Priority Queue insertion/extraction
  - Cancellation and reassignment
  - Room allocation

#### Benchmark Endpoints

1. **Individual Operation Tests**:
   - `/Baseline/RunSortTest?dataSize=100`
   - `/Baseline/RunSearchTest?dataSize=100`
   - `/Baseline/RunQueueTest?dataSize=100`
   - `/Baseline/RunAllocationTest?dataSize=100`
   - `/Baseline/RunCancelTest?dataSize=100`

2. **Comprehensive Benchmark Suite**:
   - `/Baseline/RunFullBenchmarkSuite` - Runs all tests with 100, 500, and 1000 records

### Performance Metrics

Each benchmark reports:
- **Execution time** in milliseconds (average of 5 runs)
- **Algorithmic operations** (comparisons, swaps for sorting)
- **Improvement percentage** over baseline
- **Data size** and number of runs

## Algorithm Complexity Summary

| Operation | Baseline Complexity | Optimized Complexity | Implementation |
|-----------|-------------------|---------------------|----------------|
| Search | O(n) - Linear Search | O(1) avg - Hash Table / O(log n) - Binary Search | Dictionary + custom indexes |
| Sort | O(n²) - Bubble Sort | O(n log n) avg, O(n²) worst - Quick Sort | Median-of-three Quick Sort |
| Queue Insert | O(1) - Single Queue | O(1) - Priority Queue + FIFO Queue | Heap for VIP, FIFO for Regular |
| Queue Extract | O(1) - Single Queue | O(log n) - Priority Queue + O(1) - FIFO Queue | Heap operations |
| Allocation | Sequential - First available | Greedy - Cheapest suitable, VIP first | Greedy algorithm |
| Cancellation | Manual reassignment | Real-time auto reallocate | Hash table lookup + reallocation |

## Key Improvements Addressing Consultation Feedback

### 1. Removed Inappropriate Test Sizes
- **Removed**: 10-record test case (too small for meaningful evaluation)
- **Added**: Proper test sizes (100, 500, 1000+ records)

### 2. Fixed Performance Measurement
- **Operation-specific**: Measures algorithmic operations, not UI rendering
- **Multiple runs**: Each test runs 5 times with averaging
- **Consistent methodology**: Same data and conditions for baseline and optimized

### 3. Corrected Complexity Documentation
- **Quick Sort**: Now correctly documented as O(n log n) average case, O(n²) worst case
- **Hash Table**: Properly disclosed as Dictionary-backed with custom secondary indexes

### 4. Honest Performance Reporting
- **No false claims**: Performance results reflect actual measurements
- **Transparency**: Both positive and negative results are reported honestly
- **Context**: Results include data size, runs, and specific operations measured

## Usage Instructions

### Running Benchmarks

1. **Start the application**: Run the HotelQueue web application
2. **Access baseline comparison**: Navigate to `/Baseline`
3. **Run individual tests**: Use the specific test endpoints
4. **Full benchmark suite**: Access `/Baseline/RunFullBenchmarkSuite` for comprehensive results

### Interpreting Results

- **Positive improvement percentages**: Optimized version is faster
- **Negative improvement percentages**: Optimized version has higher overhead
- **Small differences**: May be within measurement error for small datasets
- **Large datasets**: More likely to show significant algorithmic differences

## Architecture Notes

### Service Dependencies
- `PerformanceBenchmarkService`: Centralized benchmarking logic
- `ReservationService`: Optimized reservation management
- `BaselineReservationService`: Baseline implementation for comparison
- `DashboardService`: Statistics and overview data

### Data Flow
1. Test data generation with reproducible random seeds
2. Separate execution of baseline and optimized implementations
3. Performance measurement using high-precision timers
4. Statistical analysis and reporting

## Future Considerations

### Potential Optimizations
- **Hybrid algorithms**: Combine approaches for different data sizes
- **Memory pooling**: Reduce allocation overhead for frequent operations
- **Parallel processing**: For large datasets where applicable

### Benchmarking Improvements
- **Statistical significance**: Add confidence intervals
- **Memory usage**: Track memory allocation patterns
- **Scalability testing**: Test with larger datasets (10,000+ records)

## Conclusion

This implementation addresses all consultation feedback:
- ✅ Proper test sizes (100, 500, 1000+ records)
- ✅ Operation-specific benchmarking (5 runs, averaged)
- ✅ Correct complexity documentation (Quick Sort: O(n log n) avg, O(n²) worst)
- ✅ Honest performance reporting
- ✅ Library-backed hash table disclosure
- ✅ Separate measurement of algorithmic operations

The system provides a robust, honest comparison between baseline and optimized data structure implementations, suitable for academic evaluation and practical demonstration.
