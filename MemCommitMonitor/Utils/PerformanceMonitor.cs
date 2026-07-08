using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MemCommitMonitor.Services;

namespace MemCommitMonitor.Utils;

/// <summary>
/// 性能计时器（用于 using 语句）
/// </summary>
public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private readonly PerformanceMonitor _monitor;

    public PerformanceTimer(string operationName, PerformanceMonitor monitor)
    {
        _operationName = operationName;
        _monitor = monitor;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _monitor.RecordMetric(_operationName, _stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// 性能监控器
/// </summary>
public class PerformanceMonitor
{
    private readonly Dictionary<string, List<long>> _metrics = new();
    private readonly ILoggerService _logger;
    private readonly object _lock = new();

    // 性能阈值（毫秒）
    private const long SlowOperationThreshold = 1000;  // 1秒
    private const long VerySlowOperationThreshold = 3000;  // 3秒

    public PerformanceMonitor(ILoggerService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 开始性能测量
    /// </summary>
    public PerformanceTimer Measure(string operationName)
    {
        return new PerformanceTimer(operationName, this);
    }

    /// <summary>
    /// 记录性能指标
    /// </summary>
    public void RecordMetric(string operationName, long milliseconds)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new List<long>();
            }

            _metrics[operationName].Add(milliseconds);

            // 检查是否超过阈值
            if (milliseconds > VerySlowOperationThreshold)
            {
                _logger.Warning($"⚠️ 非常慢的操作: {operationName} 耗时 {milliseconds}ms");
            }
            else if (milliseconds > SlowOperationThreshold)
            {
                _logger.Warning($"⚠️ 慢操作: {operationName} 耗时 {milliseconds}ms");
            }
            else
            {
                _logger.Debug($"性能: {operationName} 耗时 {milliseconds}ms");
            }
        }
    }

    /// <summary>
    /// 获取操作的统计信息
    /// </summary>
    public PerformanceStats? GetStats(string operationName)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(operationName) || _metrics[operationName].Count == 0)
            {
                return null;
            }

            var times = _metrics[operationName];
            return new PerformanceStats
            {
                OperationName = operationName,
                Count = times.Count,
                TotalMs = times.Sum(),
                AverageMs = times.Average(),
                MinMs = times.Min(),
                MaxMs = times.Max(),
                MedianMs = CalculateMedian(times)
            };
        }
    }

    /// <summary>
    /// 获取所有操作的统计信息
    /// </summary>
    public List<PerformanceStats> GetAllStats()
    {
        lock (_lock)
        {
            var stats = new List<PerformanceStats>();
            foreach (var operationName in _metrics.Keys)
            {
                var stat = GetStats(operationName);
                if (stat != null)
                {
                    stats.Add(stat);
                }
            }
            return stats.OrderByDescending(s => s.TotalMs).ToList();
        }
    }

    /// <summary>
    /// 清空所有统计数据
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
            _logger.Info("性能统计数据已清空");
        }
    }

    /// <summary>
    /// 生成性能报告
    /// </summary>
    public string GenerateReport()
    {
        var allStats = GetAllStats();

        if (allStats.Count == 0)
        {
            return "没有性能数据";
        }

        var report = "=== 性能监控报告 ===\n\n";

        foreach (var stat in allStats)
        {
            report += $"{stat.OperationName}:\n";
            report += $"  调用次数: {stat.Count}\n";
            report += $"  总耗时: {stat.TotalMs}ms\n";
            report += $"  平均: {stat.AverageMs:F2}ms\n";
            report += $"  最小: {stat.MinMs}ms\n";
            report += $"  最大: {stat.MaxMs}ms\n";
            report += $"  中位数: {stat.MedianMs:F2}ms\n";
            report += "\n";
        }

        return report;
    }

    /// <summary>
    /// 计算中位数
    /// </summary>
    private double CalculateMedian(List<long> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;

        if (count == 0)
            return 0;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }
}

/// <summary>
/// 性能统计信息
/// </summary>
public class PerformanceStats
{
    public string OperationName { get; set; } = "";
    public int Count { get; set; }
    public long TotalMs { get; set; }
    public double AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public double MedianMs { get; set; }
}

/// <summary>
/// 全局性能监控器
/// </summary>
public static class AppPerformanceMonitor
{
    private static PerformanceMonitor? _instance;

    public static void Initialize(ILoggerService logger)
    {
        _instance = new PerformanceMonitor(logger);
        logger.Info("性能监控系统初始化完成");
    }

    public static PerformanceMonitor Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("性能监控系统未初始化");
            }
            return _instance;
        }
    }
}
