using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MemCommitMonitor.Services;
using MemCommitMonitor.Dialogs;

namespace MemCommitMonitor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化日志系统
        AppLogger.Initialize();
        AppLogger.Instance.Info("应用程序启动");

        // 设置全局异常处理
        SetupExceptionHandling();

        AppLogger.Instance.Info("全局异常处理已配置");
    }

    private void SetupExceptionHandling()
    {
        // 捕获 UI 线程未处理的异常
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // 捕获非 UI 线程未处理的异常
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // 捕获 Task 异步任务未观察到的异常
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleException(e.Exception, "UI 线程异常");
        e.Handled = true; // 标记为已处理，防止应用崩溃
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            HandleException(exception, "应用程序域异常", e.IsTerminating);
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception, "Task 异步异常");
        e.SetObserved(); // 标记为已观察，防止程序崩溃
    }

    private void HandleException(Exception exception, string source, bool isTerminating = false)
    {
        try
        {
            // 记录日志
            AppLogger.Instance.Fatal($"[{source}] 发生未处理的异常", exception);

            if (isTerminating)
            {
                AppLogger.Instance.Fatal("应用程序即将终止");
            }

            // 显示友好的错误对话框
            Dispatcher.Invoke(() =>
            {
                var errorMessage = $"程序遇到了一个错误：\n\n{exception.Message}\n\n" +
                                 $"错误详情已记录到日志文件。\n" +
                                 $"日志位置：%AppData%\\MemCommitMonitor\\Logs";

                if (isTerminating)
                {
                    errorMessage += "\n\n程序需要关闭。";
                }

                MacDialog.Show(
                    "程序错误",
                    errorMessage,
                    MacDialog.DialogIcon.Error,
                    MacDialog.DialogButton.OK,
                    Current.MainWindow
                );
            });

            if (isTerminating)
            {
                // 尝试优雅退出
                Shutdown();
            }
        }
        catch
        {
            // 如果连异常处理都失败了，直接退出
            Environment.Exit(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Instance.Info($"应用程序退出，退出代码: {e.ApplicationExitCode}");
        AppLogger.Instance.Info("=== MemCommit Monitor 关闭 ===");
        base.OnExit(e);
    }
}
