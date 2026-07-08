using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using MemCommitMonitor.Utils;

namespace MemCommitMonitor.Core;

/// <summary>
/// 内存释放结果
/// </summary>
public class MemoryReleaseResult
{
    public bool Success { get; set; }
    public int RegionsAttempted { get; set; }
    public int RegionsReleased { get; set; }
    public long BytesReleased { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool ProcessCrashed { get; set; }
}

/// <summary>
/// 实验性内存释放器（Phase 3）
/// 警告：此功能极度危险，可能导致目标进程崩溃
/// </summary>
public class ExperimentalMemoryReleaser
{
    private readonly MemoryScanner _scanner = new();

    /// <summary>
    /// 保守释放模式（只释放最安全的区域）
    /// </summary>
    public MemoryReleaseResult ReleaseMemoryConservative(int processId)
    {
        return ReleaseMemory(processId, maxRiskScore: 20);
    }

    /// <summary>
    /// 平衡释放模式
    /// </summary>
    public MemoryReleaseResult ReleaseMemoryBalanced(int processId)
    {
        return ReleaseMemory(processId, maxRiskScore: 40);
    }

    /// <summary>
    /// 激进释放模式（危险）
    /// </summary>
    public MemoryReleaseResult ReleaseMemoryAggressive(int processId)
    {
        return ReleaseMemory(processId, maxRiskScore: 60);
    }

    /// <summary>
    /// 实验性内存释放
    /// </summary>
    private MemoryReleaseResult ReleaseMemory(int processId, int maxRiskScore)
    {
        var result = new MemoryReleaseResult();

        try
        {
            // 1. 扫描内存
            var scanResult = _scanner.ScanProcessMemory(processId);

            // 2. 筛选可释放的区域
            var regionsToRelease = scanResult.Regions
                .Where(r => r.RiskScore <= maxRiskScore && r.CanRelease)
                .OrderBy(r => r.RiskScore) // 从最安全的开始
                .ToList();

            result.RegionsAttempted = regionsToRelease.Count;

            if (regionsToRelease.Count == 0)
            {
                result.Success = true;
                result.Errors.Add("没有找到可以安全释放的内存区域");
                return result;
            }

            // 3. 打开进程
            using var process = Process.GetProcessById(processId);

            // 4. 逐个释放区域
            foreach (var region in regionsToRelease)
            {
                try
                {
                    // 检查进程是否还在运行
                    if (process.HasExited)
                    {
                        result.ProcessCrashed = true;
                        result.Errors.Add("进程在释放过程中崩溃");
                        break;
                    }

                    // 尝试释放
                    bool released = NativeMethods.VirtualFreeEx(
                        process.Handle,
                        region.BaseAddress,
                        IntPtr.Zero,
                        NativeMethods.MEM_RELEASE);

                    if (released)
                    {
                        result.RegionsReleased++;
                        result.BytesReleased += region.Size;
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        result.Errors.Add($"释放 0x{region.BaseAddress.ToInt64():X} 失败: Error {error}");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"释放 0x{region.BaseAddress.ToInt64():X} 异常: {ex.Message}");
                }
            }

            // 5. 最终检查进程状态
            try
            {
                process.Refresh();
                result.Success = !process.HasExited;
                if (process.HasExited)
                {
                    result.ProcessCrashed = true;
                    result.Errors.Add("进程在释放后崩溃");
                }
            }
            catch
            {
                result.ProcessCrashed = true;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"释放失败: {ex.Message}");
        }

        return result;
    }
}
