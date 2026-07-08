using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MemCommitMonitor.Utils;

namespace MemCommitMonitor.Core;

/// <summary>
/// 进程内存区域信息
/// </summary>
public class MemoryRegionInfo
{
    public IntPtr BaseAddress { get; set; }
    public long Size { get; set; }
    public string State { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Protection { get; set; } = string.Empty;
    public int RiskScore { get; set; } // 0-100，越低越安全
    public bool CanRelease { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 进程内存扫描结果
/// </summary>
public class MemoryScanResult
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public List<MemoryRegionInfo> Regions { get; set; } = new();
    public long TotalCommitted { get; set; }
    public long SafeToRelease { get; set; }
    public long RiskyToRelease { get; set; }
    public int ScanTimeMs { get; set; }
}

/// <summary>
/// 内存扫描器（Phase 3）
/// 扫描进程内存布局，识别可能可以释放的区域
/// </summary>
public class MemoryScanner
{
    /// <summary>
    /// 扫描进程内存
    /// </summary>
    public MemoryScanResult ScanProcessMemory(int processId)
    {
        var sw = Stopwatch.StartNew();
        var result = new MemoryScanResult
        {
            ProcessId = processId
        };

        try
        {
            using var process = Process.GetProcessById(processId);
            result.ProcessName = process.ProcessName;

            var regions = new List<MemoryRegionInfo>();
            IntPtr address = IntPtr.Zero;
            IntPtr maxAddress = Environment.Is64BitProcess
                ? new IntPtr(0x7FFFFFFFFFFF)
                : new IntPtr(0x7FFFFFFF);

            while (address.ToInt64() < maxAddress.ToInt64())
            {
                int queryResult = NativeMethods.VirtualQueryEx(
                    process.Handle,
                    address,
                    out NativeMethods.MEMORY_BASIC_INFORMATION mbi,
                    Marshal.SizeOf(typeof(NativeMethods.MEMORY_BASIC_INFORMATION)));

                if (queryResult == 0)
                    break;

                // 只关注已提交的内存
                if (mbi.State == NativeMethods.MEM_COMMIT)
                {
                    var region = AnalyzeRegion(mbi);
                    regions.Add(region);

                    result.TotalCommitted += region.Size;
                    if (region.RiskScore <= 30)
                        result.SafeToRelease += region.Size;
                    else if (region.RiskScore <= 60)
                        result.RiskyToRelease += region.Size;
                }

                // 移动到下一个区域
                address = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
            }

            result.Regions = regions;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"扫描进程内存失败: {ex.Message}", ex);
        }

        sw.Stop();
        result.ScanTimeMs = (int)sw.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// 分析内存区域，评估释放风险
    /// </summary>
    private MemoryRegionInfo AnalyzeRegion(NativeMethods.MEMORY_BASIC_INFORMATION mbi)
    {
        var region = new MemoryRegionInfo
        {
            BaseAddress = mbi.BaseAddress,
            Size = mbi.RegionSize.ToInt64(),
            State = GetStateName(mbi.State),
            Type = GetTypeName(mbi.Type),
            Protection = GetProtectionName(mbi.Protect)
        };

        // 风险评分算法
        region.RiskScore = CalculateRiskScore(mbi);
        region.CanRelease = region.RiskScore <= 30;
        region.Reason = GetReleaseReason(mbi, region.RiskScore);

        return region;
    }

    /// <summary>
    /// 计算释放风险评分（0-100）
    /// 0-30: 低风险（可能可以安全释放）
    /// 31-60: 中等风险（可能导致问题）
    /// 61-100: 高风险（几乎肯定会崩溃）
    /// </summary>
    private int CalculateRiskScore(NativeMethods.MEMORY_BASIC_INFORMATION mbi)
    {
        int score = 50; // 基础分数

        // 内存类型评分
        if (mbi.Type == NativeMethods.MEM_IMAGE)
            score += 50; // 代码段，绝对不能释放
        else if (mbi.Type == NativeMethods.MEM_MAPPED)
            score += 40; // 映射文件，很危险
        else if (mbi.Type == NativeMethods.MEM_PRIVATE)
            score -= 20; // 私有内存，相对安全

        // 保护属性评分
        if ((mbi.Protect & NativeMethods.PAGE_NOACCESS) != 0)
            score -= 30; // 不可访问，最安全
        else if ((mbi.Protect & NativeMethods.PAGE_READONLY) != 0)
            score -= 10; // 只读，比较安全
        else if ((mbi.Protect & NativeMethods.PAGE_READWRITE) != 0)
            score += 20; // 读写，可能正在使用
        else if ((mbi.Protect & (NativeMethods.PAGE_EXECUTE |
                                NativeMethods.PAGE_EXECUTE_READ |
                                NativeMethods.PAGE_EXECUTE_READWRITE)) != 0)
            score += 50; // 可执行，绝对不能释放

        // Guard 页面
        if ((mbi.Protect & NativeMethods.PAGE_GUARD) != 0)
            score += 30; // Guard 页面，可能是栈

        // 大小评分
        long sizeMB = mbi.RegionSize.ToInt64() / 1024 / 1024;
        if (sizeMB < 1)
            score += 20; // 小块内存，可能是关键结构
        else if (sizeMB > 100)
            score -= 10; // 大块内存，可能是缓存

        // 确保范围在 0-100
        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// 获取释放建议原因
    /// </summary>
    private string GetReleaseReason(NativeMethods.MEMORY_BASIC_INFORMATION mbi, int riskScore)
    {
        if (mbi.Type == NativeMethods.MEM_IMAGE)
            return "代码段，不能释放";

        if (mbi.Type == NativeMethods.MEM_MAPPED)
            return "内存映射文件，不能释放";

        if ((mbi.Protect & (NativeMethods.PAGE_EXECUTE |
                           NativeMethods.PAGE_EXECUTE_READ |
                           NativeMethods.PAGE_EXECUTE_READWRITE)) != 0)
            return "可执行内存，不能释放";

        if ((mbi.Protect & NativeMethods.PAGE_GUARD) != 0)
            return "Guard 页面，可能是栈，不能释放";

        if ((mbi.Protect & NativeMethods.PAGE_NOACCESS) != 0 && riskScore <= 30)
            return "不可访问的私有内存，可以尝试释放";

        if (riskScore <= 30)
            return "低风险区域，可以尝试释放";
        else if (riskScore <= 60)
            return "中等风险，释放可能导致问题";
        else
            return "高风险，释放可能导致崩溃";
    }

    private string GetStateName(uint state)
    {
        return state switch
        {
            NativeMethods.MEM_COMMIT => "已提交",
            NativeMethods.MEM_RESERVE => "预留",
            NativeMethods.MEM_FREE => "空闲",
            _ => $"未知({state:X})"
        };
    }

    private string GetTypeName(uint type)
    {
        return type switch
        {
            NativeMethods.MEM_IMAGE => "代码段",
            NativeMethods.MEM_MAPPED => "映射文件",
            NativeMethods.MEM_PRIVATE => "私有内存",
            _ => $"未知({type:X})"
        };
    }

    private string GetProtectionName(uint protect)
    {
        var names = new List<string>();

        if ((protect & NativeMethods.PAGE_NOACCESS) != 0)
            names.Add("无访问");
        if ((protect & NativeMethods.PAGE_READONLY) != 0)
            names.Add("只读");
        if ((protect & NativeMethods.PAGE_READWRITE) != 0)
            names.Add("读写");
        if ((protect & NativeMethods.PAGE_EXECUTE) != 0)
            names.Add("执行");
        if ((protect & NativeMethods.PAGE_EXECUTE_READ) != 0)
            names.Add("执行+读");
        if ((protect & NativeMethods.PAGE_EXECUTE_READWRITE) != 0)
            names.Add("执行+读写");
        if ((protect & NativeMethods.PAGE_GUARD) != 0)
            names.Add("Guard");

        return names.Count > 0 ? string.Join("|", names) : $"未知({protect:X})";
    }
}

