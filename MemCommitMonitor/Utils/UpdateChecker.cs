using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using MemCommitMonitor.Services;

namespace MemCommitMonitor.Utils;

/// <summary>
/// 更新信息
/// </summary>
public class UpdateInfo
{
    public bool HasUpdate { get; set; }
    public Version? CurrentVersion { get; set; }
    public Version? LatestVersion { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? ReleaseName { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// GitHub Release 数据模型
/// </summary>
public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("body")]
    public string Body { get; set; } = "";

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";

    [JsonPropertyName("assets")]
    public GitHubAsset[] Assets { get; set; } = Array.Empty<GitHubAsset>();
}

/// <summary>
/// GitHub Asset 数据模型
/// </summary>
public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

/// <summary>
/// 更新检查器
/// </summary>
public class UpdateChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/AriesOxO/MemCommitMonitor/releases/latest";
    private readonly ILoggerService _logger;
    private readonly HttpClient _httpClient;

    public UpdateChecker(ILoggerService logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MemCommitMonitor/3.4");
    }

    /// <summary>
    /// 获取当前版本
    /// </summary>
    public Version GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version ?? new Version(1, 0, 0);
        }
        catch (Exception ex)
        {
            _logger.Error("获取当前版本失败", ex);
            return new Version(1, 0, 0);
        }
    }

    /// <summary>
    /// 检查更新（异步）
    /// </summary>
    public async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        try
        {
            _logger.Info("开始检查更新");

            // 获取当前版本
            var currentVersion = GetCurrentVersion();
            _logger.Debug($"当前版本: {currentVersion}");

            // 请求 GitHub API
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var release = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (release == null)
            {
                _logger.Warning("无法解析 GitHub Release 数据");
                return new UpdateInfo
                {
                    HasUpdate = false,
                    CurrentVersion = currentVersion
                };
            }

            // 解析最新版本
            var latestVersion = ParseVersion(release.TagName);
            _logger.Debug($"最新版本: {latestVersion}");

            // 比较版本
            bool hasUpdate = latestVersion > currentVersion;

            var updateInfo = new UpdateInfo
            {
                HasUpdate = hasUpdate,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                DownloadUrl = release.HtmlUrl,
                ReleaseNotes = release.Body,
                ReleaseName = release.Name,
                PublishedAt = release.PublishedAt
            };

            if (hasUpdate)
            {
                _logger.Info($"发现新版本: {latestVersion}");
            }
            else
            {
                _logger.Info("当前已是最新版本");
            }

            return updateInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning($"检查更新失败（网络错误）: {ex.Message}");
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = GetCurrentVersion()
            };
        }
        catch (Exception ex)
        {
            _logger.Error("检查更新时发生错误", ex);
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = GetCurrentVersion()
            };
        }
    }

    /// <summary>
    /// 解析版本号
    /// </summary>
    private Version ParseVersion(string tagName)
    {
        try
        {
            // 移除 "v" 前缀（如果有）
            var versionString = tagName.TrimStart('v', 'V');

            // 尝试解析
            if (Version.TryParse(versionString, out var version))
            {
                return version;
            }

            _logger.Warning($"无法解析版本号: {tagName}");
            return new Version(0, 0, 0);
        }
        catch (Exception ex)
        {
            _logger.Error($"解析版本号失败: {tagName}", ex);
            return new Version(0, 0, 0);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// 全局更新检查器
/// </summary>
public static class AppUpdateChecker
{
    private static UpdateChecker? _instance;

    public static void Initialize(ILoggerService logger)
    {
        _instance = new UpdateChecker(logger);
        logger.Info("更新检查器初始化完成");
    }

    public static UpdateChecker Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("更新检查器未初始化");
            }
            return _instance;
        }
    }
}
