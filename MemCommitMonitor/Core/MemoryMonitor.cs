using MemCommitMonitor.Models;
using MemCommitMonitor.Utils;
using System.Diagnostics;

namespace MemCommitMonitor.Core;

/// <summary>
/// 系统内存监控器
/// </summary>
public class MemoryMonitor
{
    private readonly PerformanceCounter? _committedBytesCounter;

    public MemoryMonitor()
    {
        try
        {
            // 尝试初始化性能计数器
            _committedBytesCounter = new PerformanceCounter("Memory", "Committed Bytes");
        }
        catch
        {
            // 如果失败，使用 Native API 作为后备方案
            _committedBytesCounter = null;
        }
    }

    /// <summary>
    /// 获取系统内存信息
    /// </summary>
    public SystemMemoryInfo GetSystemMemoryInfo()
    {
        var memStatus = new NativeMethods.MEMORYSTATUSEX();
        NativeMethods.GlobalMemoryStatusEx(memStatus);

        long committedBytes;
        if (_committedBytesCounter != null)
        {
            try
            {
                committedBytes = (long)_committedBytesCounter.NextValue();
            }
            catch
            {
                // 使用页面文件作为估算
                committedBytes = (long)(memStatus.ullTotalPageFile - memStatus.ullAvailPageFile);
            }
        }
        else
        {
            committedBytes = (long)(memStatus.ullTotalPageFile - memStatus.ullAvailPageFile);
        }

        return new SystemMemoryInfo
        {
            TotalCommitted = committedBytes,
            CommitLimit = (long)memStatus.ullTotalPageFile,
            TotalPhysical = (long)memStatus.ullTotalPhys,
            AvailablePhysical = (long)memStatus.ullAvailPhys
        };
    }

    public void Dispose()
    {
        _committedBytesCounter?.Dispose();
    }
}
