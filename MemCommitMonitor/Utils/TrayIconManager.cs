using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using MemCommitMonitor.Services;
using Application = System.Windows.Application;
using FontStyle = System.Drawing.FontStyle;

namespace MemCommitMonitor.Utils;

/// <summary>
/// 托盘图标管理器
/// </summary>
public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private readonly ILoggerService _logger;
    private Window? _mainWindow;
    private bool _disposed;

    public TrayIconManager(ILoggerService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 初始化托盘图标
    /// </summary>
    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
            Text = "MemCommit Monitor",
            Visible = true
        };

        // 双击托盘图标显示/隐藏窗口
        _notifyIcon.MouseDoubleClick += OnTrayIconDoubleClick;

        // 创建右键菜单
        CreateContextMenu();

        _logger.Info("托盘图标已初始化");
    }

    /// <summary>
    /// 创建图标
    /// </summary>
    private Icon CreateIcon()
    {
        try
        {
            // 创建一个简单的蓝色圆形图标
            using var bitmap = new Bitmap(32, 32);
            using var graphics = Graphics.FromImage(bitmap);

            // 抗锯齿
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 绘制蓝色圆形背景
            using var brush = new SolidBrush(System.Drawing.Color.FromArgb(0, 122, 255)); // macOS Blue
            graphics.FillEllipse(brush, 2, 2, 28, 28);

            // 绘制白色 M 字母
            using var font = new Font(new FontFamily("Segoe UI"), 16, FontStyle.Bold);
            using var textBrush = new SolidBrush(System.Drawing.Color.White);

            var textSize = graphics.MeasureString("M", font);
            var x = (32 - textSize.Width) / 2;
            var y = (32 - textSize.Height) / 2;

            graphics.DrawString("M", font, textBrush, x, y);

            // 转换为 Icon
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);

            _logger.Debug("托盘图标已创建");
            return icon;
        }
        catch (Exception ex)
        {
            _logger.Error("创建托盘图标失败", ex);

            // 返回默认图标
            return SystemIcons.Application;
        }
    }

    /// <summary>
    /// 创建右键菜单
    /// </summary>
    private void CreateContextMenu()
    {
        if (_notifyIcon == null)
            return;

        var contextMenu = new ContextMenuStrip();

        // 显示主窗口
        var showMenuItem = new ToolStripMenuItem("显示主窗口", null, (s, e) => ShowMainWindow());
        showMenuItem.Font = new Font(showMenuItem.Font, FontStyle.Bold);
        contextMenu.Items.Add(showMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        // 快速刷新
        contextMenu.Items.Add("快速刷新", null, (s, e) => QuickRefresh());

        contextMenu.Items.Add(new ToolStripSeparator());

        // 退出
        contextMenu.Items.Add("退出", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    /// <summary>
    /// 托盘图标双击事件
    /// </summary>
    private void OnTrayIconDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowMainWindow();
        }
    }

    /// <summary>
    /// 显示主窗口
    /// </summary>
    public void ShowMainWindow()
    {
        if (_mainWindow == null)
            return;

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();

        _logger.Debug("从托盘恢复主窗口");
    }

    /// <summary>
    /// 隐藏主窗口到托盘
    /// </summary>
    public void HideMainWindow()
    {
        if (_mainWindow == null)
            return;

        _mainWindow.Hide();

        // 显示气泡提示
        ShowBalloonTip("MemCommit Monitor", "程序已最小化到系统托盘", 2000);

        _logger.Debug("主窗口已隐藏到托盘");
    }

    /// <summary>
    /// 显示气泡提示
    /// </summary>
    public void ShowBalloonTip(string title, string text, int timeout = 3000)
    {
        if (_notifyIcon == null)
            return;

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(timeout);
    }

    /// <summary>
    /// 快速刷新
    /// </summary>
    private void QuickRefresh()
    {
        _logger.Info("托盘菜单：快速刷新");

        // 触发主窗口的刷新
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_mainWindow is MainWindow mainWindow)
            {
                // 通过反射调用 LoadData 方法
                var method = mainWindow.GetType().GetMethod("LoadData",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(mainWindow, null);
            }
        });
    }

    /// <summary>
    /// 退出应用程序
    /// </summary>
    private void ExitApplication()
    {
        _logger.Info("托盘菜单：退出应用程序");
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        _disposed = true;
        _logger.Info("托盘图标已清理");
    }
}

/// <summary>
/// 全局托盘图标管理器
/// </summary>
public static class AppTrayIcon
{
    private static TrayIconManager? _instance;

    public static void Initialize(ILoggerService logger, Window mainWindow)
    {
        _instance = new TrayIconManager(logger);
        _instance.Initialize(mainWindow);
        logger.Info("托盘图标系统初始化完成");
    }

    public static TrayIconManager? Instance => _instance;
}
