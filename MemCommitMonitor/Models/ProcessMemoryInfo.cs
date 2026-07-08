using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemCommitMonitor.Models;

/// <summary>
/// 进程内存信息
/// </summary>
public class ProcessMemoryInfo : INotifyPropertyChanged
{
    private long _privateBytes;
    private long _workingSet;

    /// <summary>进程 ID</summary>
    public int ProcessId { get; set; }

    /// <summary>进程名称</summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>已提交内存（字节）</summary>
    public long PrivateBytes
    {
        get => _privateBytes;
        set
        {
            if (_privateBytes != value)
            {
                _privateBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedPrivate));
            }
        }
    }

    /// <summary>工作集（物理内存，字节）</summary>
    public long WorkingSet
    {
        get => _workingSet;
        set
        {
            if (_workingSet != value)
            {
                _workingSet = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedWorkingSet));
            }
        }
    }

    /// <summary>格式化的已提交内存</summary>
    public string FormattedPrivate => FormatBytes(PrivateBytes);

    /// <summary>格式化的工作集</summary>
    public string FormattedWorkingSet => FormatBytes(WorkingSet);

    /// <summary>是否为受保护的系统进程</summary>
    public bool IsProtected { get; set; }

    /// <summary>进程类型（用于分类和建议）</summary>
    public string ProcessType { get; set; } = "未知";

    /// <summary>是否建议关闭（基于进程类型和内存占用）</summary>
    public bool SuggestClose { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
