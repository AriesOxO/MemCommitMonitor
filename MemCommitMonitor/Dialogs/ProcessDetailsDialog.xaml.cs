using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MemCommitMonitor.Models;
using MemCommitMonitor.Core;

namespace MemCommitMonitor.Dialogs;

public partial class ProcessDetailsDialog : Window
{
    public ProcessDetailsDialog(ProcessMemoryInfo processInfo, MemoryScanResult scanResult)
    {
        InitializeComponent();

        TitleText.Text = $"进程详情 - {processInfo.ProcessName}";

        BuildContent(processInfo, scanResult);
    }

    private void BuildContent(ProcessMemoryInfo processInfo, MemoryScanResult scanResult)
    {
        // 基本信息
        AddSection("基本信息");
        AddInfoRow("进程名称", processInfo.ProcessName);
        AddInfoRow("进程 ID", processInfo.ProcessId.ToString());
        AddInfoRow("进程类型", processInfo.ProcessType);
        AddInfoRow("保护状态", processInfo.IsProtected ? "🔒 系统保护进程" : "✓ 可操作");
        AddInfoRow("建议关闭", processInfo.SuggestClose ? "是（内存占用高）" : "否");

        AddSpacer();

        // 内存使用
        AddSection("内存使用");
        AddInfoRow("已提交内存", processInfo.FormattedPrivate);
        AddInfoRow("工作集（物理）", processInfo.FormattedWorkingSet);

        AddSpacer();

        // 内存布局分析
        AddSection("内存布局分析");
        AddInfoRow("内存区域数", scanResult.Regions.Count.ToString());
        AddInfoRow("已提交总量", FormatBytes(scanResult.TotalCommitted));
        AddInfoRow("扫描耗时", $"{scanResult.ScanTimeMs} ms");

        AddSpacer();

        // 可释放评估
        AddSection("可释放评估（VirtualFreeEx）");
        AddInfoRow("• 低风险", FormatBytes(scanResult.SafeToRelease), "#FF34C759");
        AddInfoRow("• 中风险", FormatBytes(scanResult.RiskyToRelease), "#FFFF9500");
        AddInfoRow("• 高风险", FormatBytes(scanResult.TotalCommitted - scanResult.SafeToRelease - scanResult.RiskyToRelease), "#FFFF3B30");

        AddSpacer();

        // 操作建议
        AddSection("操作建议");
        string suggestion = GetSuggestion(processInfo, scanResult);
        AddInfoText(suggestion);
    }

    private void AddSection(string title)
    {
        var text = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12),
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        ContentPanel.Children.Add(text);
    }

    private void AddInfoRow(string label, string value, string? colorHex = null)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 8)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(108, 108, 112))
        };
        Grid.SetColumn(labelText, 0);

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Foreground = colorHex != null
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
                : new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(valueText);
        ContentPanel.Children.Add(grid);
    }

    private void AddInfoText(string text)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        ContentPanel.Children.Add(textBlock);
    }

    private void AddSpacer()
    {
        var separator = new Rectangle
        {
            Height = 1,
            Fill = new SolidColorBrush(Color.FromRgb(229, 229, 234)),
            Margin = new Thickness(0, 16, 0, 16)
        };
        ContentPanel.Children.Add(separator);
    }

    private string GetSuggestion(ProcessMemoryInfo processInfo, MemoryScanResult scanResult)
    {
        if (processInfo.IsProtected)
        {
            return "⚠️ 这是系统关键进程，不建议进行任何操作。";
        }
        else if (scanResult.SafeToRelease > 0)
        {
            return $"✓ 可以尝试实验性释放（保守模式）\n预计释放: {FormatBytes(scanResult.SafeToRelease)}";
        }
        else if (processInfo.SuggestClose)
        {
            return "💡 建议终止此进程以释放内存\n（记得先保存数据）";
        }
        else
        {
            return "✓ 可以尝试释放工作集\n（不会减少已提交内存）";
        }
    }

    private string FormatBytes(long bytes)
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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
