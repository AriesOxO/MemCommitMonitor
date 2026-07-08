using System;
using System.IO;
using NLog;

namespace MemCommitMonitor.Services;

/// <summary>
/// 日志服务接口
/// </summary>
public interface ILoggerService
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Fatal(string message, Exception? exception = null);
}

/// <summary>
/// NLog 日志服务实现
/// </summary>
public class LoggerService : ILoggerService
{
    private readonly ILogger _logger;

    public LoggerService(string loggerName = "MemCommitMonitor")
    {
        _logger = LogManager.GetLogger(loggerName);
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Warning(string message)
    {
        _logger.Warn(message);
    }

    public void Error(string message, Exception? exception = null)
    {
        if (exception != null)
            _logger.Error(exception, message);
        else
            _logger.Error(message);
    }

    public void Fatal(string message, Exception? exception = null)
    {
        if (exception != null)
            _logger.Fatal(exception, message);
        else
            _logger.Fatal(message);
    }
}

/// <summary>
/// 全局日志管理器
/// </summary>
public static class AppLogger
{
    private static ILoggerService? _instance;

    public static ILoggerService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LoggerService();
            }
            return _instance;
        }
    }

    public static void Initialize()
    {
        _instance = new LoggerService();
        Instance.Info("=== MemCommit Monitor 启动 ===");
        Instance.Info($"版本: {typeof(AppLogger).Assembly.GetName().Version}");
        Instance.Info($"平台: {Environment.OSVersion}");
        Instance.Info($"用户: {Environment.UserName}");
        Instance.Info($"日志位置: {GetLogDirectory()}");
    }

    private static string GetLogDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "MemCommitMonitor", "Logs");
    }
}
