using MemCommitMonitor.Core;
using MemCommitMonitor.Models;
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
    private List<ProcessMemoryInfo> _processes = new();

    public MainWindow()
    {
        InitializeComponent();

        _memoryMonitor = new MemoryMonitor();
        _processAnalyzer = new ProcessAnalyzer();
        _memoryReleaser = new MemoryReleaser();

        // 启动时自动加载数据
        LoadData();
    }

    /// <summary>
    /// 加载系统和进程数据
    /// </summary>
    private void LoadData()
    {
        try
        {
            StatusText.Text = "正在加载数据...";
            StatusText.Foreground = System.Windows.Media.Brushes.Blue;

            // 加载系统内存信息
            var sysInfo = _memoryMonitor.GetSystemMemoryInfo();
            UpdateSystemMemoryDisplay(sysInfo);

            // 加载进程列表
            _processes = _processAnalyzer.GetProcessMemoryInfos();
            ProcessDataGrid.ItemsSource = _processes;

            StatusText.Text = $"✓ 已加载 {_processes.Count} 个进程";
            StatusText.Foreground = System.Windows.Media.Brushes.Green;
            LastUpdateText.Text = $"最后更新: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 加载失败: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            MessageBox.Show($"加载数据时出错:\n{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
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
    /// 进程选择变化事件
    /// </summary>
    private void ProcessDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        bool hasSelection = ProcessDataGrid.SelectedItem != null;
        ReleaseButton.IsEnabled = hasSelection;
        TerminateButton.IsEnabled = hasSelection;
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _memoryMonitor.Dispose();
    }
}





