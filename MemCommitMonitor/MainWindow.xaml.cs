using MemCommitMonitor.Core;
using MemCommitMonitor.Models;
using MemCommitMonitor.Dialogs;
using MemCommitMonitor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemCommitMonitor;

public partial class MainWindow : Window
{
    private readonly MemoryMonitor _memoryMonitor;
    private readonly ProcessAnalyzer _processAnalyzer;
    private readonly MemoryReleaser _memoryReleaser;
    private readonly MemoryScanner _memoryScanner;
    private readonly ExperimentalMemoryReleaser _experimentalReleaser;
    private readonly ILoggerService _logger;
    private List<ProcessMemoryInfo> _processes = new();

    public MainWindow()
    {
        InitializeComponent();

        _logger = AppLogger.Instance;
        _logger.Info("主窗口初始化开始");

        _memoryMonitor = new MemoryMonitor();
        _processAnalyzer = new ProcessAnalyzer();
        _memoryReleaser = new MemoryReleaser();
        _memoryScanner = new MemoryScanner();
        _experimentalReleaser = new ExperimentalMemoryReleaser();

        // 应用配置中的窗口设置
        ApplyWindowSettings();

        // 应用配置中的 UI 设置
        ApplyUiSettings();

        // 启动时自动加载数据
        LoadData();

        _logger.Info("主窗口初始化完成");
    }

    /// <summary>
    /// 应用窗口设置
    /// </summary>
    private void ApplyWindowSettings()
    {
        var config = AppConfigManager.Current;

        if (config.UI.RememberWindowSize)
        {
            Width = config.UI.WindowWidth;
            Height = config.UI.WindowHeight;

            // 如果保存了窗口位置，恢复位置
            if (!double.IsNaN(config.UI.WindowLeft) && !double.IsNaN(config.UI.WindowTop))
            {
                Left = config.UI.WindowLeft;
                Top = config.UI.WindowTop;
                WindowStartupLocation = WindowStartupLocation.Manual;
            }

            _logger.Debug($"恢复窗口大小: {Width}x{Height}");
        }
    }

    /// <summary>
    /// 应用 UI 设置
    /// </summary>
    private void ApplyUiSettings()
    {
        var config = AppConfigManager.Current;

        // 应用隐藏系统进程的设置
        HideSystemProcessCheckBox.IsChecked = config.Behavior.HideSystemProcesses;

        _logger.Debug($"应用 UI 设置: 隐藏系统进程={config.Behavior.HideSystemProcesses}");
    }

    /// <summary>
    /// 加载系统和进程数据
    /// </summary>
    private void LoadData()
    {
        try
        {
            _logger.Debug("开始加载数据");
            StatusText.Text = "正在加载数据...";
            StatusText.Foreground = System.Windows.Media.Brushes.Blue;

            // 加载系统内存信息
            var sysInfo = _memoryMonitor.GetSystemMemoryInfo();
            UpdateSystemMemoryDisplay(sysInfo);
            _logger.Debug($"系统内存: 已提交 {sysInfo.TotalCommitted / 1024 / 1024 / 1024:F2} GB");

            // 加载进程列表
            _processes = _processAnalyzer.GetProcessMemoryInfos();
            _logger.Debug($"加载了 {_processes.Count} 个进程");

            // 应用过滤和排序
            ApplyFilterAndSort();

            StatusText.Text = $"✓ 已加载 {ProcessDataGrid.Items.Count} 个进程（总计 {_processes.Count}）";
            StatusText.Foreground = System.Windows.Media.Brushes.Green;
            LastUpdateText.Text = $"最后更新: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            _logger.Info($"数据加载成功，显示 {ProcessDataGrid.Items.Count}/{_processes.Count} 个进程");
        }
        catch (Exception ex)
        {
            _logger.Error("加载数据失败", ex);
            StatusText.Text = $"✗ 加载失败: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MacDialog.Show("加载失败", $"加载数据时出错:\n{ex.Message}",
                MacDialog.DialogIcon.Error, MacDialog.DialogButton.OK, this);
        }
    }

    /// <summary>
    /// 更新系统内存显示
    /// </summary>
    private void UpdateSystemMemoryDisplay(SystemMemoryInfo info)
    {
        CommittedMemoryText.Text = $"已提交内存: {FormatBytes(info.TotalCommitted)} / " +
            $"{FormatBytes(info.CommitLimit)} ({info.CommittedPercentage:F1}%)";

        long usedPhysical = info.TotalPhysical - info.AvailablePhysical;
        PhysicalMemoryText.Text = $"物理内存: {FormatBytes(usedPhysical)} / " +
            $"{FormatBytes(info.TotalPhysical)} ({info.PhysicalPercentage:F1}%)";
    }

    /// <summary>
    /// 刷新按钮点击事件
    /// </summary>
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    /// <summary>
    /// 应用过滤和排序
    /// </summary>
    private void ApplyFilterAndSort()
    {
        var filteredProcesses = _processes.AsEnumerable();

        // 应用过滤：隐藏系统进程
        if (HideSystemProcessCheckBox?.IsChecked == true)
        {
            filteredProcesses = filteredProcesses.Where(p => !p.IsProtected);
        }

        // 排序：用户进程优先（IsProtected = false 排前面），然后按已提交内存降序
        var sortedProcesses = filteredProcesses
            .OrderBy(p => p.IsProtected)  // false (用户进程) 在前
            .ThenByDescending(p => p.PrivateBytes)  // 按内存降序
            .ToList();

        ProcessDataGrid.ItemsSource = sortedProcesses;
    }

    /// <summary>
    /// 过滤条件变化事件
    /// </summary>
    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        if (_processes != null && _processes.Count > 0)
        {
            ApplyFilterAndSort();
            StatusText.Text = $"✓ 显示 {ProcessDataGrid.Items.Count} 个进程（总计 {_processes.Count}）";
        }
    }

    /// <summary>
    /// 进程选择变化事件
    /// </summary>
    private void ProcessDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 右键菜单会自动处理启用/禁用
    }

    /// <summary>
    /// 释放按钮点击事件
    /// </summary>
    private void ReleaseButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessDataGrid.SelectedItem is not ProcessMemoryInfo selectedProcess)
            return;

        // 检查是否为受保护进程
        if (selectedProcess.IsProtected)
        {
            MessageBox.Show(
                $"进程 \"{selectedProcess.ProcessName}\" 是系统关键进程，不允许释放其内存。\n\n" +
                "释放系统进程可能导致系统不稳定或崩溃。",
                "操作被拒绝",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // 确认对话框
        var result = MessageBox.Show(
            $"确定要释放以下进程的工作集（物理内存）吗？\n\n" +
            $"进程名称: {selectedProcess.ProcessName}\n" +
            $"PID: {selectedProcess.ProcessId}\n" +
            $"当前工作集: {selectedProcess.FormattedWorkingSet}\n\n" +
            "注意：\n" +
            "• 此操作只释放物理内存，不会减少已提交内存\n" +
            "• 内存页会被换出到页面文件\n" +
            "• 进程不会被关闭，数据不会丢失",
            "确认释放工作集",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        // 执行释放
        try
        {
            StatusText.Text = "正在释放内存...";
            StatusText.Foreground = System.Windows.Media.Brushes.Blue;

            var (before, after, success) = _memoryReleaser.ReleaseProcessMemory(selectedProcess);

            if (success)
            {
                long released = before - after;
                StatusText.Text = $"✓ 已释放 {FormatBytes(released)}";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show(
                    $"释放成功!\n\n" +
                    $"释放前: {FormatBytes(before)}\n" +
                    $"释放后: {FormatBytes(after)}\n" +
                    $"释放量: {FormatBytes(released)}",
                    "操作成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 刷新数据
                LoadData();
            }
            else
            {
                StatusText.Text = "✗ 释放失败";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show("释放内存失败，可能需要管理员权限。", "操作失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 错误: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"释放内存时出错:\n{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 终止进程按钮点击事件
    /// </summary>
    private void TerminateButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessDataGrid.SelectedItem is not ProcessMemoryInfo selectedProcess)
            return;

        // 检查是否为受保护进程
        if (selectedProcess.IsProtected)
        {
            MessageBox.Show(
                $"进程 \"{selectedProcess.ProcessName}\" 是系统关键进程，不允许终止。\n\n" +
                "终止系统进程会导致系统崩溃。",
                "操作被拒绝",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        // 严格的确认对话框
        var result = MessageBox.Show(
            $"⚠️ 警告：即将终止进程（真正释放已提交内存）\n\n" +
            $"进程名称: {selectedProcess.ProcessName}\n" +
            $"PID: {selectedProcess.ProcessId}\n" +
            $"已提交内存: {selectedProcess.FormattedPrivate}\n" +
            $"工作集: {selectedProcess.FormattedWorkingSet}\n\n" +
            $"此操作将：\n" +
            $"✅ 真正释放已提交内存（约 {selectedProcess.FormattedPrivate}）\n" +
            $"❌ 强制关闭程序\n" +
            $"❌ 丢失所有未保存的数据\n" +
            $"❌ 无法撤销\n\n" +
            $"确定要终止此进程吗？",
            "⚠️ 危险操作确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
            return;

        // 二次确认
        var confirm2 = MessageBox.Show(
            "最后确认：\n\n" +
            "您确定要终止此进程吗？\n" +
            "这将导致数据丢失且无法撤销！",
            "二次确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Stop,
            MessageBoxResult.No);

        if (confirm2 != MessageBoxResult.Yes)
            return;

        // 执行终止
        try
        {
            StatusText.Text = "正在终止进程...";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;

            var (success, releasedCommitted, error) = _memoryReleaser.TerminateProcess(selectedProcess);

            if (success)
            {
                StatusText.Text = $"✓ 已终止进程，释放 {FormatBytes(releasedCommitted)}";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show(
                    $"进程已终止\n\n" +
                    $"已释放已提交内存: {FormatBytes(releasedCommitted)}\n\n" +
                    $"系统已提交内存总量应已减少。",
                    "操作成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 刷新数据
                LoadData();
            }
            else
            {
                StatusText.Text = "✗ 终止失败";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"终止进程失败:\n{error}", "操作失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 错误: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"终止进程时出错:\n{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 实验性释放按钮点击事件（Phase 3）
    /// </summary>
    private void ExperimentalReleaseButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessDataGrid.SelectedItem is not ProcessMemoryInfo selectedProcess)
            return;

        // 检查是否为受保护进程
        if (selectedProcess.IsProtected)
        {
            MessageBox.Show(
                $"进程 \"{selectedProcess.ProcessName}\" 是系统关键进程，不允许实验性操作。",
                "操作被拒绝",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        // 第一次警告：说明风险
        var warning1 = MessageBox.Show(
            "🧪 实验性功能警告\n\n" +
            "您即将使用 VirtualFreeEx 实验性内存释放功能。\n\n" +
            "⚠️ 极度危险：\n" +
            "• 可能导致目标进程立即崩溃（50%+ 概率）\n" +
            "• 可能导致数据损坏\n" +
            "• 成功率很低（10-30%）\n" +
            "• 即使成功，释放量也可能很少\n\n" +
            "此功能仅供研究和学习使用。\n" +
            "如果需要可靠释放内存，请使用\"终止进程\"功能。\n\n" +
            "是否继续？",
            "⚠️ 实验性功能警告",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (warning1 != MessageBoxResult.Yes)
            return;

        // 第二次：扫描内存并显示结果
        StatusText.Text = "正在扫描进程内存...";
        StatusText.Foreground = System.Windows.Media.Brushes.Blue;

        MemoryScanResult scanResult;
        try
        {
            scanResult = _memoryScanner.ScanProcessMemory(selectedProcess.ProcessId);
        }
        catch (Exception ex)
        {
            StatusText.Text = "✗ 扫描失败";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"扫描进程内存失败:\n{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        StatusText.Text = "扫描完成";
        StatusText.Foreground = System.Windows.Media.Brushes.Green;

        // 显示扫描结果
        var scanInfo = $"内存扫描结果：\n\n" +
            $"进程: {scanResult.ProcessName} (PID: {scanResult.ProcessId})\n" +
            $"内存区域数: {scanResult.Regions.Count}\n" +
            $"已提交内存: {FormatBytes(scanResult.TotalCommitted)}\n\n" +
            $"可释放评估：\n" +
            $"• 低风险（安全）: {FormatBytes(scanResult.SafeToRelease)}\n" +
            $"• 中风险（谨慎）: {FormatBytes(scanResult.RiskyToRelease)}\n" +
            $"• 高风险（危险）: {FormatBytes(scanResult.TotalCommitted - scanResult.SafeToRelease - scanResult.RiskyToRelease)}\n\n" +
            $"扫描耗时: {scanResult.ScanTimeMs} ms\n\n" +
            "选择释放模式：";

        // 让用户选择释放模式
        var modeDialog = MessageBox.Show(
            scanInfo +
            "\n是（Yes）= 保守模式（只释放低风险）" +
            "\n否（No）= 取消操作" +
            "\n\n推荐：如果低风险为 0，建议取消",
            "选择释放模式",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question,
            MessageBoxResult.Cancel);

        if (modeDialog == MessageBoxResult.Cancel || modeDialog == MessageBoxResult.No)
        {
            StatusText.Text = "已取消";
            StatusText.Foreground = System.Windows.Media.Brushes.Gray;
            return;
        }

        // 确定释放模式
        bool useConservative = modeDialog == MessageBoxResult.Yes;

        // 第三次最终确认
        var confirm = MessageBox.Show(
            $"最终确认\n\n" +
            $"进程: {selectedProcess.ProcessName}\n" +
            $"模式: {(useConservative ? "保守模式（风险 ≤ 20）" : "平衡模式（风险 ≤ 40）")}\n" +
            $"预计释放: {(useConservative ? FormatBytes(scanResult.SafeToRelease) : FormatBytes(scanResult.SafeToRelease + scanResult.RiskyToRelease))}\n\n" +
            "再次提醒：\n" +
            "• 进程可能崩溃\n" +
            "• 数据可能损坏\n" +
            "• 无法撤销\n\n" +
            "确定执行吗？",
            "最终确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Stop,
            MessageBoxResult.No);

        if (confirm != MessageBoxResult.Yes)
        {
            StatusText.Text = "已取消";
            StatusText.Foreground = System.Windows.Media.Brushes.Gray;
            return;
        }

        // 执行释放
        StatusText.Text = "正在执行实验性释放...";
        StatusText.Foreground = System.Windows.Media.Brushes.Orange;

        try
        {
            MemoryReleaseResult releaseResult = useConservative
                ? _experimentalReleaser.ReleaseMemoryConservative(selectedProcess.ProcessId)
                : _experimentalReleaser.ReleaseMemoryBalanced(selectedProcess.ProcessId);

            // 显示结果
            string resultMessage = $"实验性释放结果：\n\n" +
                $"尝试释放: {releaseResult.RegionsAttempted} 个区域\n" +
                $"成功释放: {releaseResult.RegionsReleased} 个区域\n" +
                $"释放字节: {FormatBytes(releaseResult.BytesReleased)}\n" +
                $"进程状态: {(releaseResult.ProcessCrashed ? "❌ 已崩溃" : "✅ 仍在运行")}\n\n";

            if (releaseResult.Errors.Any())
            {
                resultMessage += $"错误数量: {releaseResult.Errors.Count}\n";
                resultMessage += "前3个错误:\n";
                foreach (var error in releaseResult.Errors.Take(3))
                {
                    resultMessage += $"• {error}\n";
                }
            }

            if (releaseResult.Success && !releaseResult.ProcessCrashed)
            {
                StatusText.Text = $"✓ 释放了 {FormatBytes(releaseResult.BytesReleased)}";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show(
                    resultMessage,
                    "释放成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (releaseResult.ProcessCrashed)
            {
                StatusText.Text = "✗ 进程已崩溃";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;

                MessageBox.Show(
                    resultMessage + "\n⚠️ 如预期，进程在释放后崩溃了。\n这就是为什么这个功能标记为\"实验性\"。",
                    "进程崩溃",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                StatusText.Text = "⚠️ 释放部分成功";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;

                MessageBox.Show(
                    resultMessage,
                    "释放完成（有错误）",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // 刷新数据
            LoadData();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 错误: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"实验性释放失败:\n{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 格式化字节数
    /// </summary>
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

    /// <summary>
    /// 查看进程详情
    /// </summary>
    private void ViewProcessDetails_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessDataGrid.SelectedItem is not ProcessMemoryInfo selectedProcess)
        {
            MacDialog.Show("提示", "请先选择一个进程", MacDialog.DialogIcon.Info, MacDialog.DialogButton.OK, this);
            return;
        }

        try
        {
            // 扫描内存获取详细信息
            var scanResult = _memoryScanner.ScanProcessMemory(selectedProcess.ProcessId);

            // 使用自定义详情对话框
            var detailsDialog = new ProcessDetailsDialog(selectedProcess, scanResult);
            detailsDialog.Owner = this;
            detailsDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MacDialog.Show("错误", $"获取进程详情失败:\n{ex.Message}", MacDialog.DialogIcon.Error, MacDialog.DialogButton.OK, this);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // 保存窗口设置到配置
        SaveWindowSettings();

        // 清理资源
        base.OnClosed(e);
        _memoryMonitor.Dispose();

        _logger.Info("主窗口已关闭");
    }

    /// <summary>
    /// 保存窗口设置
    /// </summary>
    private void SaveWindowSettings()
    {
        try
        {
            var config = AppConfigManager.Current;

            if (config.UI.RememberWindowSize)
            {
                config.UI.WindowWidth = Width;
                config.UI.WindowHeight = Height;
                config.UI.WindowLeft = Left;
                config.UI.WindowTop = Top;
            }

            // 保存复选框状态
            config.Behavior.HideSystemProcesses = HideSystemProcessCheckBox.IsChecked == true;

            AppConfigManager.Save();

            _logger.Debug("窗口设置已保存");
        }
        catch (Exception ex)
        {
            _logger.Error("保存窗口设置失败", ex);
        }
    }
}





