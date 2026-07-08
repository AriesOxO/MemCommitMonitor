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
    /// 释放进程工作集（物理内存）
    /// 注意：此操作不会减少已提交内存，只会释放物理内存到页面文件
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

    /// <summary>
    /// 终止进程（真正释放已提交内存）
    /// 警告：此操作会关闭程序并丢失未保存的数据
    /// </summary>
    /// <param name="processInfo">进程信息</param>
    /// <returns>已释放的已提交内存大小（字节）</returns>
    public (bool success, long releasedCommitted, string error) TerminateProcess(ProcessMemoryInfo processInfo)
    {
        if (processInfo.IsProtected)
        {
            return (false, 0, $"无法终止系统保护进程: {processInfo.ProcessName}");
        }

        try
        {
            using var process = Process.GetProcessById(processInfo.ProcessId);

            long committedBefore = process.PrivateMemorySize64;

            // 终止进程
            process.Kill();
            process.WaitForExit(5000); // 等待最多 5 秒

            return (true, committedBefore, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, 0, $"终止进程失败: {ex.Message}");
        }
    }
}
