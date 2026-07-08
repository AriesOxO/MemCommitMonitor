using System;
using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;
using MemCommitMonitor.Services;

namespace MemCommitMonitor.Utils;

/// <summary>
/// 权限管理器
/// </summary>
public static class PrivilegeManager
{
    private static readonly ILoggerService Logger = AppLogger.Instance;

    /// <summary>
    /// 检查是否以管理员身份运行
    /// </summary>
    public static bool IsRunningAsAdmin()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            Logger.Info($"权限检查: {(isAdmin ? "管理员" : "标准用户")}");
            return isAdmin;
        }
        catch (Exception ex)
        {
            Logger.Error("检查管理员权限失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 重启程序并请求管理员权限
    /// </summary>
    /// <returns>是否成功启动（注意：成功后当前进程会退出）</returns>
    public static bool RestartAsAdmin()
    {
        try
        {
            Logger.Info("请求以管理员身份重启");

            var currentProcess = Process.GetCurrentProcess();
            var fileName = currentProcess.MainModule?.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Error("无法获取当前程序路径");
                return false;
            }

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = fileName,
                Verb = "runas" // 请求管理员权限（UAC 提示）
            };

            Logger.Debug($"启动新进程: {fileName}");
            Process.Start(startInfo);

            Logger.Info("新进程已启动，当前进程即将退出");
            return true;
        }
        catch (Win32Exception ex)
        {
            // 用户取消了 UAC 提示
            Logger.Warning("用户取消了权限提升");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error("重启为管理员失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    public static string GetCurrentUserName()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            return identity.Name;
        }
        catch
        {
            return Environment.UserName;
        }
    }

    /// <summary>
    /// 获取权限状态描述
    /// </summary>
    public static string GetPrivilegeStatus()
    {
        if (IsRunningAsAdmin())
        {
            return "🛡️ 管理员权限";
        }
        else
        {
            return "⚠️ 标准用户权限";
        }
    }

    /// <summary>
    /// 检查操作是否需要管理员权限
    /// </summary>
    public static bool CheckPermissionForOperation(string operationName)
    {
        var isAdmin = IsRunningAsAdmin();
        Logger.Debug($"权限检查 [{operationName}]: {(isAdmin ? "允许" : "可能需要管理员权限")}");
        return isAdmin;
    }
}
