using System;
using System.IO;
using System.Text.Json;
using MemCommitMonitor.Models;

namespace MemCommitMonitor.Services;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigService
{
    /// <summary>加载配置</summary>
    AppConfig Load();

    /// <summary>保存配置</summary>
    void Save(AppConfig config);

    /// <summary>获取配置文件路径</summary>
    string GetConfigPath();

    /// <summary>重置为默认配置</summary>
    AppConfig ResetToDefault();
}

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private readonly ILoggerService _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigService(ILoggerService logger)
    {
        _logger = logger;

        // 配置文件路径: %AppData%\MemCommitMonitor\config.json
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "MemCommitMonitor");

        // 确保目录存在
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
            _logger.Debug($"创建配置目录: {appFolder}");
        }

        _configPath = Path.Combine(appFolder, "config.json");
    }

    public AppConfig Load()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger.Info("配置文件不存在，使用默认配置");
                var defaultConfig = new AppConfig();
                Save(defaultConfig); // 保存默认配置
                return defaultConfig;
            }

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);

            if (config == null)
            {
                _logger.Warning("配置文件解析失败，使用默认配置");
                return new AppConfig();
            }

            _logger.Info($"成功加载配置: {_configPath}");
            _logger.Debug($"配置: 主题={config.UI.Theme}, 语言={config.UI.Language}");

            return config;
        }
        catch (Exception ex)
        {
            _logger.Error($"加载配置失败: {ex.Message}", ex);
            return new AppConfig();
        }
    }

    public void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(_configPath, json);

            _logger.Debug($"配置已保存: {_configPath}");
        }
        catch (Exception ex)
        {
            _logger.Error($"保存配置失败: {ex.Message}", ex);
        }
    }

    public string GetConfigPath()
    {
        return _configPath;
    }

    public AppConfig ResetToDefault()
    {
        _logger.Info("重置为默认配置");
        var defaultConfig = new AppConfig();
        Save(defaultConfig);
        return defaultConfig;
    }
}

/// <summary>
/// 全局配置管理器
/// </summary>
public static class AppConfigManager
{
    private static IConfigService? _configService;
    private static AppConfig? _currentConfig;

    /// <summary>初始化配置系统</summary>
    public static void Initialize(ILoggerService logger)
    {
        _configService = new ConfigService(logger);
        _currentConfig = _configService.Load();

        logger.Info("配置系统初始化完成");
    }

    /// <summary>获取当前配置</summary>
    public static AppConfig Current
    {
        get
        {
            if (_currentConfig == null)
            {
                throw new InvalidOperationException("配置系统未初始化，请先调用 AppConfigManager.Initialize()");
            }
            return _currentConfig;
        }
    }

    /// <summary>保存当前配置</summary>
    public static void Save()
    {
        if (_configService == null || _currentConfig == null)
        {
            throw new InvalidOperationException("配置系统未初始化");
        }

        _configService.Save(_currentConfig);
    }

    /// <summary>重置为默认配置</summary>
    public static void ResetToDefault()
    {
        if (_configService == null)
        {
            throw new InvalidOperationException("配置系统未初始化");
        }

        _currentConfig = _configService.ResetToDefault();
    }

    /// <summary>获取配置文件路径</summary>
    public static string GetConfigPath()
    {
        if (_configService == null)
        {
            throw new InvalidOperationException("配置系统未初始化");
        }

        return _configService.GetConfigPath();
    }
}
