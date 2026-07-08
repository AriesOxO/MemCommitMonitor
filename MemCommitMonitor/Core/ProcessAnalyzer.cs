using MemCommitMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MemCommitMonitor.Core;

/// <summary>
/// 进程内存分析器
/// </summary>
public class ProcessAnalyzer
{
    /// <summary>
    /// 受保护的系统关键进程列表（禁止释放）
    /// </summary>
    private static readonly HashSet<string> ProtectedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "System", "Registry", "smss", "csrss", "wininit",
        "services", "lsass", "winlogon", "dwm", "explorer",
        "svchost", "audiodg", "taskmgr", "fontdrvhost"
    };

    /// <summary>
    /// 获取所有进程的内存信息，按已提交内存排序
    /// </summary>
    public List<ProcessMemoryInfo> GetProcessMemoryInfos()
    {
        var processes = Process.GetProcesses();
        var result = new List<ProcessMemoryInfo>();

        foreach (var process in processes)
        {
            try
            {
                var info = new ProcessMemoryInfo
                {
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    PrivateBytes = process.PrivateMemorySize64,
                    WorkingSet = process.WorkingSet64,
                    IsProtected = IsProtectedProcess(process.ProcessName)
                };

                result.Add(info);
            }
            catch
            {
                // 跳过无权限访问的进程
            }
            finally
            {
                process.Dispose();
            }
        }

        // 按已提交内存降序排序
        return result.OrderByDescending(p => p.PrivateBytes).ToList();
    }

    /// <summary>
    /// 检查是否为受保护的系统进程
    /// </summary>
    public static bool IsProtectedProcess(string processName)
    {
        return ProtectedProcesses.Contains(processName);
    }
}
