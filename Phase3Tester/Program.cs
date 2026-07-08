using System;
using System.Linq;
using MemCommitMonitor.Core;

namespace MemCommitMonitor.Testing;

/// <summary>
/// Phase 3 测试程序
/// 测试内存扫描和实验性释放功能
/// </summary>
class Phase3Tester
{
    static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  Phase 3 - 内存扫描测试程序");
        Console.WriteLine("========================================");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("用法: Phase3Tester <进程ID或进程名>");
            Console.WriteLine("示例: Phase3Tester 1234");
            Console.WriteLine("示例: Phase3Tester MemoryEater");
            return;
        }

        int processId;
        if (!int.TryParse(args[0], out processId))
        {
            // 按名称查找进程
            var processes = System.Diagnostics.Process.GetProcessesByName(args[0]);
            if (processes.Length == 0)
            {
                Console.WriteLine($"错误: 找不到进程 '{args[0]}'");
                return;
            }
            processId = processes[0].Id;
            Console.WriteLine($"找到进程: {processes[0].ProcessName} (PID: {processId})");
        }

        Console.WriteLine();
        Console.WriteLine("开始扫描进程内存...");
        Console.WriteLine();

        try
        {
            var scanner = new MemoryScanner();
            var result = scanner.ScanProcessMemory(processId);

            Console.WriteLine($"扫描完成！耗时: {result.ScanTimeMs} ms");
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("  扫描结果");
            Console.WriteLine("========================================");
            Console.WriteLine($"进程: {result.ProcessName} (PID: {result.ProcessId})");
            Console.WriteLine($"内存区域总数: {result.Regions.Count}");
            Console.WriteLine($"已提交内存总量: {FormatBytes(result.TotalCommitted)}");
            Console.WriteLine($"低风险可释放: {FormatBytes(result.SafeToRelease)}");
            Console.WriteLine($"中风险可释放: {FormatBytes(result.RiskyToRelease)}");
            Console.WriteLine();

            // 按风险分组统计
            var byRisk = result.Regions
                .GroupBy(r => r.RiskScore switch
                {
                    <= 30 => "低风险",
                    <= 60 => "中风险",
                    _ => "高风险"
                })
                .Select(g => new
                {
                    Risk = g.Key,
                    Count = g.Count(),
                    TotalSize = g.Sum(r => r.Size)
                })
                .OrderBy(x => x.Risk);

            Console.WriteLine("风险分布：");
            foreach (var group in byRisk)
            {
                Console.WriteLine($"  {group.Risk}: {group.Count} 个区域，共 {FormatBytes(group.TotalSize)}");
            }
            Console.WriteLine();

            // 显示前10个最大的低风险区域
            var safeLarge = result.Regions
                .Where(r => r.RiskScore <= 30)
                .OrderByDescending(r => r.Size)
                .Take(10)
                .ToList();

            if (safeLarge.Any())
            {
                Console.WriteLine("前10个最大的低风险区域（可以尝试释放）：");
                foreach (var region in safeLarge)
                {
                    Console.WriteLine($"  地址: 0x{region.BaseAddress.ToInt64():X16}");
                    Console.WriteLine($"  大小: {FormatBytes(region.Size)}");
                    Console.WriteLine($"  类型: {region.Type}");
                    Console.WriteLine($"  保护: {region.Protection}");
                    Console.WriteLine($"  风险: {region.RiskScore}/100");
                    Console.WriteLine($"  原因: {region.Reason}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("⚠️ 没有找到低风险可释放的内存区域");
            }

            Console.WriteLine("========================================");
            Console.WriteLine("  实验性释放测试");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("⚠️ 警告：实验性内存释放功能可能导致进程崩溃！");
            Console.Write("是否继续测试？(y/N): ");
            var input = Console.ReadLine();

            if (input?.ToLower() == "y")
            {
                Console.WriteLine();
                Console.WriteLine("选择释放模式：");
                Console.WriteLine("1. 保守模式（风险 <= 20）");
                Console.WriteLine("2. 平衡模式（风险 <= 40）");
                Console.WriteLine("3. 激进模式（风险 <= 60）");
                Console.Write("选择 (1-3): ");
                var modeInput = Console.ReadLine();

                var releaser = new ExperimentalMemoryReleaser();
                MemoryReleaseResult releaseResult;

                switch (modeInput)
                {
                    case "1":
                        Console.WriteLine("\n执行保守释放...");
                        releaseResult = releaser.ReleaseMemoryConservative(processId);
                        break;
                    case "2":
                        Console.WriteLine("\n执行平衡释放...");
                        releaseResult = releaser.ReleaseMemoryBalanced(processId);
                        break;
                    case "3":
                        Console.WriteLine("\n执行激进释放...");
                        releaseResult = releaser.ReleaseMemoryAggressive(processId);
                        break;
                    default:
                        Console.WriteLine("无效选择");
                        return;
                }

                Console.WriteLine();
                Console.WriteLine("释放结果：");
                Console.WriteLine($"  尝试释放: {releaseResult.RegionsAttempted} 个区域");
                Console.WriteLine($"  成功释放: {releaseResult.RegionsReleased} 个区域");
                Console.WriteLine($"  释放字节: {FormatBytes(releaseResult.BytesReleased)}");
                Console.WriteLine($"  成功状态: {(releaseResult.Success ? "✓ 成功" : "✗ 失败")}");
                Console.WriteLine($"  进程崩溃: {(releaseResult.ProcessCrashed ? "✗ 是" : "✓ 否")}");

                if (releaseResult.Errors.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("错误信息：");
                    foreach (var error in releaseResult.Errors.Take(5))
                    {
                        Console.WriteLine($"  • {error}");
                    }
                    if (releaseResult.Errors.Count > 5)
                    {
                        Console.WriteLine($"  ... 还有 {releaseResult.Errors.Count - 5} 个错误");
                    }
                }
            }
            else
            {
                Console.WriteLine("已取消实验性释放");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
            Console.WriteLine($"详细: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("测试完成");
    }

    static string FormatBytes(long bytes)
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
