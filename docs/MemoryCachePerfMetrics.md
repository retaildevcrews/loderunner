# Memory Cache Performance Metrics

![License](https://img.shields.io/badge/license-MIT-green.svg)

## Introduction

This document is meant to show performance results when getting all entries from two different types of Cache object using different implementations 

Cache data size 1,000,000 entries.

## 1. System.Runtime.Caching.MemoryCache

Below are the performance results for 7 different implementations, 2 of them with different degree of parallelism.

The first two strategies ustilize a separat key list, so the list can be sort in Natural order or Shuffled, which it shows a big difference.

|                               Solution Strategy|  Natural Sort(ms)|  Shuffle Sort(ms)|  Sort N/A [Run1 - Run2]|  MaxDegreeOfParallelism|  RequiresSeparateList|Pass Validation|
|:-----------------------------------------------|:-----------------|:-----------------|:-----------------------|:-----------------------|:---------------------|:--------------|
|               Cache built-it function GetValues|            959.35|           1656.86|                     N/A|                     N/A|                  True|           True|
|          Loop thru key list then gets the entry|            496.78|           1089.69|                     N/A|                     N/A|                  True|           True|
|         Loop thru Cache and check on key prefix|               N/A|               N/A|       2816.89 - 3155.57|                     N/A|                 False|           True|
|                                     Linq lambda|               N/A|               N/A|       3024.77 - 2227.96|                     N/A|                 False|           True|
|                      Parallel ForEach with Lock|               N/A|               N/A|       2324.30 - 2197.50|                       1|                 False|           True|
|                  Parallel ForEach ConcurrentBag|               N/A|               N/A|       2292.77 - 2233.28|                       1|                 False|           True|
|                      Parallel ForEach with Lock|               N/A|               N/A|       1160.70 - 2126.84|                       4|                 False|           True|
|                  Parallel ForEach ConcurrentBag|               N/A|               N/A|       1236.73 - 1174.87|                       4|                 False|           True|
|                      Loop thru Cache Enumerator|               N/A|               N/A|       2347.46 - 2121.64|                     N/A|                 False|           True|

## 2. Microsoft.Extensions.Caching.Memory.MemoryCache

Below are the performnce results for 2 different implementations with different degree of parallelism for side-by-side for two runs.

|                               Solution Strategy|        [Run1 - Run2] ms|  MaxDegreeOfParallelism|  RequiresSeparateList|Pass Validation|
|:-----------------------------------------------|:-----------------------|:-----------------------|:---------------------|:--------------|
|                                 Loop thru Cache|         815.48 - 795.79|                     N/A|                 False|           True|
|                  Parallel ForEach ConcurrentBag|         952.56 - 956.59|                       1|                 False|           True|
|                  Parallel ForEach ConcurrentBag|         608.16 - 563.14|                       2|                 False|           True|
|                  Parallel ForEach ConcurrentBag|         423.16 - 498.80|                       3|                 False|           True|
|                  Parallel ForEach ConcurrentBag|         435.18 - 394.34|                       4|                 False|           True|
|                  Parallel ForEach ConcurrentBag|         405.75 - 636.87|                       8|                 False|           True|