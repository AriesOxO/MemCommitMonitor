namespace MemCommitMonitor.Models;

/// <summary>
/// 系统内存信息
/// </summary>
public class SystemMemoryInfo
{
    /// <summary>已提交内存总量（字节）</summary>
    public long TotalCommitted { get; set; }

    /// <summary>提交限制（字节）</summary>
    public long CommitLimit { get; set; }

    /// <summary>物理内存总量（字节）</summary>
    public long TotalPhysical { get; set; }

    /// <summary>可用物理内存（字节）</summary>
    public long AvailablePhysical { get; set; }

    /// <summary>已提交内存使用率</summary>
    public double CommittedPercentage => CommitLimit > 0
        ? (double)TotalCommitted / CommitLimit * 100
        : 0;

    /// <summary>物理内存使用率</summary>
    public double PhysicalPercentage => TotalPhysical > 0
        ? (double)(TotalPhysical - AvailablePhysical) / TotalPhysical * 100
        : 0;
}
