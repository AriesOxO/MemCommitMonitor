using System;

namespace MemCommitMonitor.Models;

/// <summary>
/// 应用程序配置
/// </summary>
public class AppConfig
{
    /// <summary>UI 设置</summary>
    public UiSettings UI { get; set; } = new();

    /// <summary>行为设置</summary>
    public BehaviorSettings Behavior { get; set; } = new();

    /// <summary>高级设置</summary>
    public AdvancedSettings Advanced { get; set; } = new();
}

/// <summary>
/// UI 设置
/// </summary>
public class UiSettings
{
    /// <summary>主题（Light/Dark）</summary>
    public string Theme { get; set; } = "Light";

    /// <summary>语言</summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>启动时最小化</summary>
    public bool StartMinimized { get; set; } = false;

    /// <summary>最小化到托盘</summary>
    public bool MinimizeToTray { get; set; } = false;

    /// <summary>记住窗口大小</summary>
    public bool RememberWindowSize { get; set; } = true;

    /// <summary>窗口宽度</summary>
    public double WindowWidth { get; set; } = 1200;

    /// <summary>窗口高度</summary>
    public double WindowHeight { get; set; } = 720;

    /// <summary>窗口左边距</summary>
    public double WindowLeft { get; set; } = double.NaN;

    /// <summary>窗口顶边距</summary>
    public double WindowTop { get; set; } = double.NaN;
}

/// <summary>
/// 行为设置
/// </summary>
public class BehaviorSettings
{
    /// <summary>自动刷新间隔（毫秒，0=禁用）</summary>
    public int AutoRefreshInterval { get; set; } = 0;

    /// <summary>终止进程前确认</summary>
    public bool ConfirmBeforeTerminate { get; set; } = true;

    /// <summary>实验性释放前确认</summary>
    public bool ConfirmBeforeExperimental { get; set; } = true;

    /// <summary>默认隐藏系统进程</summary>
    public bool HideSystemProcesses { get; set; } = false;

    /// <summary>默认排序列（ProcessName/PrivateBytes/WorkingSet）</summary>
    public string DefaultSortColumn { get; set; } = "PrivateBytes";

    /// <summary>默认排序方向（Asc/Desc）</summary>
    public string DefaultSortDirection { get; set; } = "Desc";
}

/// <summary>
/// 高级设置
/// </summary>
public class AdvancedSettings
{
    /// <summary>日志级别（Debug/Info/Warning/Error）</summary>
    public string LogLevel { get; set; } = "Info";

    /// <summary>内存扫描深度（Fast/Normal/Deep）</summary>
    public string MemoryScanDepth { get; set; } = "Normal";

    /// <summary>启用遥测（未来功能）</summary>
    public bool EnableTelemetry { get; set; } = false;

    /// <summary>检查更新</summary>
    public bool CheckForUpdates { get; set; } = true;

    /// <summary>启动时以管理员身份运行（提示）</summary>
    public bool PromptForAdmin { get; set; } = true;
}
