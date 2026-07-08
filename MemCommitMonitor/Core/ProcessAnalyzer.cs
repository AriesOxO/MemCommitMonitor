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
    /// 常见浏览器进程
    /// </summary>
    private static readonly HashSet<string> BrowserProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome", "firefox", "msedge", "opera", "brave", "vivaldi"
    };

    /// <summary>
    /// 常见开发工具进程
    /// </summary>
    private static readonly HashSet<string> DevelopmentProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "devenv", "code", "rider", "idea64", "webstorm64", "pycharm64",
        "VisualStudio", "msbuild", "dotnet"
    };

    /// <summary>
    /// 常见办公软件进程
    /// </summary>
    private static readonly HashSet<string> OfficeProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "WINWORD", "EXCEL", "POWERPNT", "OUTLOOK", "AcroRd32", "Acrobat"
    };

    /// <summary>
    /// 最小建议关闭内存阈值（MB）
    /// </summary>
    private const long SuggestCloseThresholdMB = 500;

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
                    IsProtected = IsProtectedProcess(process.ProcessName),
                    ProcessType = GetProcessType(process.ProcessName)
                };

                // 判断是否建议关闭（非系统进程 且 内存占用 > 500 MB）
                long privateMB = info.PrivateBytes / 1024 / 1024;
                info.SuggestClose = !info.IsProtected && privateMB > SuggestCloseThresholdMB;

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

    /// <summary>
    /// 获取进程类型
    /// </summary>
    private static string GetProcessType(string processName)
    {
        if (ProtectedProcesses.Contains(processName))
            return "系统进程";

        if (BrowserProcesses.Any(b => processName.Contains(b, StringComparison.OrdinalIgnoreCase)))
            return "浏览器";

        if (DevelopmentProcesses.Any(d => processName.Contains(d, StringComparison.OrdinalIgnoreCase)))
            return "开发工具";

        if (OfficeProcesses.Any(o => processName.Contains(o, StringComparison.OrdinalIgnoreCase)))
            return "办公软件";

        return "用户程序";
    }
}
