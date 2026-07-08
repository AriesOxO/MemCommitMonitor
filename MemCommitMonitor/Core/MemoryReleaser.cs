using MemCommitMonitor.Models;
using MemCommitMonitor.Utils;
using System;
using System.Diagnostics;

namespace MemCommitMonitor.Core;

/// <summary>
/// 内存释放器
/// </summary>
public class MemoryReleaser
{
    /// <summary>
    /// 释放进程工作集
    /// </summary>
    /// <param name="processInfo">进程信息</param>
    /// <returns>释放前后的工作集大小（字节）</returns>
    public (long before, long after, bool success) ReleaseProcessMemory(ProcessMemoryInfo processInfo)
    {
        if (processInfo.IsProtected)
        {
            throw new InvalidOperationException($"无法释放系统保护进程: {processInfo.ProcessName}");
        }

        try
        {
            using var process = Process.GetProcessById(processInfo.ProcessId);

            long workingSetBefore = process.WorkingSet64;

            // 调用 Windows API 清空工作集
            bool success = NativeMethods.EmptyWorkingSet(process.Handle);

            if (!success)
            {
                return (workingSetBefore, workingSetBefore, false);
            }

            // 刷新进程信息
            process.Refresh();
            long workingSetAfter = process.WorkingSet64;

            return (workingSetBefore, workingSetAfter, true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"释放进程 {processInfo.ProcessName} (PID: {processInfo.ProcessId}) 失败: {ex.Message}",
                ex);
        }
    }
}
