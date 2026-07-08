# MemCommit Monitor v3.5.0 - Enterprise Edition

> 🚀 Windows 已提交内存监控和管理工具 - 企业级版本

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-3.5.0-blue.svg)](https://github.com/AriesOxO/MemCommitMonitor/releases/latest)

[中文](#中文) | [English](#english)

---

## 中文

### ⚡ v3.5.0 新特性

**企业级基础设施完整实现**：

- 🔍 **专业日志系统** - NLog 集成，完整的操作追踪
- 🛡️ **全局异常处理** - 三重保护，优雅降级
- ⚙️ **配置管理系统** - JSON 配置，自动保存/恢复
- 👑 **管理员权限** - UAC 提升，权限可视化
- 📊 **性能监控** - 实时性能分析，瓶颈定位
- 📱 **系统托盘** - 后台运行，快速访问
- 🔄 **自动更新** - GitHub Releases 集成

### 问题背景

Windows 系统中存在一个常见但容易被忽视的内存管理问题：**虚拟内存（已提交内存）不足**。

当你在任务管理器中看到：
- 物理内存还有大量空余（如 14.6 GB 可用）
- 但 "已提交" 内存接近上限（如 18.7/63.9 GB）

此时即使物理内存充足，系统也会因为已提交内存耗尽而拒绝新的内存分配请求，导致：
- 应用程序崩溃
- 无法启动新程序
- 系统运行不稳定

### 解决方案

MemCommit Monitor 是一个**企业级** Windows 桌面工具，可以：

#### 核心功能
1. 🔍 **实时监控** 系统已提交内存和物理内存使用情况
2. 📊 **分析进程** 找出哪些进程占用了大量已提交内存
3. 💾 **手动释放** 选中进程的工作集（将内存页换出到页面文件）
4. 🛡️ **安全保护** 自动识别系统关键进程，防止误操作

#### 企业级特性
5. 📝 **专业日志** 所有操作可追踪，便于问题诊断
6. ⚙️ **配置持久化** 窗口状态、过滤器自动保存
7. 🔄 **自动更新** 一键检查和下载新版本
8. 📱 **托盘运行** 最小化到系统托盘，后台监控
9. 📊 **性能分析** 实时监控操作耗时，优化体验
10. 👑 **权限管理** 智能 UAC 提升，透明权限状态

### 功能特性

#### 基础功能
- ✅ 显示系统内存状态（已提交 / 物理内存）
- ✅ 列出所有进程的已提交内存占用
- ✅ 按内存占用排序，快速定位"内存大户"
- ✅ 一键释放进程工作集
- ✅ 系统进程保护机制
- ✅ macOS 风格 UI 界面

#### 企业级功能
- ✅ NLog 日志系统（自动轮转，30天保留）
- ✅ 全局异常处理（3种异常捕获）
- ✅ JSON 配置系统（自动保存/恢复）
- ✅ 管理员权限管理（UAC 提升）
- ✅ 性能监控系统（实时性能分析）
- ✅ 系统托盘图标（后台运行）
- ✅ 自动更新检查（GitHub Releases）

### 技术栈

- **框架**: .NET 8 + WPF
- **语言**: C# 12
- **UI**: macOS-inspired design
- **日志**: NLog 6.1.3
- **API**: Windows Performance Counters + Process API + WinForms (托盘)

### 系统要求

- **操作系统**: Windows 10 / 11 (x64)
- **.NET 运行时**: .NET 8.0 Desktop Runtime
- **权限**: 建议以管理员身份运行（部分功能需要）

### 快速开始

#### 下载和安装

1. 前往 [Releases 页面](https://github.com/AriesOxO/MemCommitMonitor/releases/latest)
2. 下载 `MemCommitMonitor-v3.5.0.zip`
3. 解压到任意目录
4. 运行 `MemCommitMonitor.exe`

#### 首次启动

1. 程序会检测权限，建议选择"以管理员身份运行"
2. 窗口位置、大小会自动保存
3. 配置文件位于：`%AppData%\MemCommitMonitor\config.json`
4. 日志文件位于：`%AppData%\MemCommitMonitor\Logs\`

### 使用指南

#### 1. 监控系统内存

启动后自动显示：
- **已提交内存**: 总量 / 限制（使用率%）
- **物理内存**: 已用 / 总量（使用率%）

#### 2. 分析进程内存

- 进程列表按已提交内存降序排列
- 显示：进程名、PID、已提交内存、工作集、是否受保护
- 勾选"隐藏系统进程"只显示用户程序

#### 3. 释放内存

**右键菜单**：
- **释放工作集** - 将物理内存换出到页面文件（可逆）
- **查看详情** - 显示进程详细信息
- **终止进程** - 强制关闭（不可逆，真正释放内存）
- **实验性释放** - 深度释放（需管理员）

**注意事项**：
- 释放工作集不会关闭程序，数据不丢失
- 终止进程会关闭程序，可能丢失未保存数据
- 系统关键进程会被自动保护，无法操作

#### 4. 系统托盘

- 最小化窗口可自动隐藏到托盘（可配置）
- 双击托盘图标恢复窗口
- 右键托盘图标：显示主窗口 / 快速刷新 / 退出

#### 5. 检查更新

- 点击工具栏 🔄 按钮手动检查
- 启动后 3 秒自动后台检查（可配置）
- 发现新版本时弹窗通知

### 配置说明

配置文件位于：`%AppData%\MemCommitMonitor\config.json`

```json
{
  "ui": {
    "theme": "Light",
    "language": "zh-CN",
    "rememberWindowSize": true,
    "minimizeToTray": false
  },
  "behavior": {
    "autoRefreshInterval": 0,
    "confirmBeforeTerminate": true,
    "hideSystemProcesses": false
  },
  "advanced": {
    "logLevel": "Info",
    "checkForUpdates": true,
    "promptForAdmin": true
  }
}
```

### 日志说明

日志位于：`%AppData%\MemCommitMonitor\Logs\`

- `2026-07-08.log` - 当天所有日志
- `errors-2026-07-08.log` - 当天错误日志
- `archives/` - 历史日志归档

日志级别：
- **Debug** - 详细调试信息
- **Info** - 一般操作信息
- **Warning** - 警告（如慢操作）
- **Error** - 错误
- **Fatal** - 致命错误（导致崩溃）

### 性能数据

实际测试性能（330 个进程）：

| 操作 | 耗时 | 说明 |
|------|------|------|
| 数据加载 | 26ms | 总耗时 |
| 系统内存 | 9ms | 获取系统信息 |
| 进程列表 | 7ms | 330 个进程 |
| 过滤排序 | 3ms | 应用过滤器 |

所有操作 <50ms，用户无感知延迟。

### 常见问题

#### Q: 为什么需要管理员权限？

A: 部分功能需要管理员权限：
- 终止受保护的进程
- 实验性内存释放
- 某些系统进程的内存操作

不以管理员运行也可使用基础功能。

#### Q: 释放内存后为什么很快又占满？

A: 这是正常现象。释放工作集只是将内存页换出到页面文件，程序再次访问时会重新加载。要真正释放内存，需要终止进程。

#### Q: 日志文件会占用多少空间？

A: 日志每日轮转，保留 30 天，错误日志保留 90 天。通常每天 1-5 MB，总计不超过 500 MB。

#### Q: 如何禁用自动更新检查？

A: 编辑配置文件，设置 `"checkForUpdates": false`。

#### Q: 托盘图标不显示怎么办？

A: Windows 托盘图标设置中将 MemCommit Monitor 设为"始终显示"。

### 开发和构建

#### 环境要求

- Visual Studio 2022 或 Visual Studio Code
- .NET 8.0 SDK
- Windows 10/11

#### 克隆项目

```bash
git clone https://github.com/AriesOxO/MemCommitMonitor.git
cd MemCommitMonitor
```

#### 构建

```bash
# Debug 版本
dotnet build

# Release 版本
dotnet build -c Release
```

#### 运行

```bash
# 从源码运行
dotnet run --project MemCommitMonitor

# 运行编译后的程序
cd MemCommitMonitor/bin/Debug/net8.0-windows
./MemCommitMonitor.exe
```

#### 测试

```bash
# 运行所有测试（待实现）
dotnet test
```

### 项目结构

```
MemCommitMonitor/
├── Core/                    # 核心功能
│   ├── MemoryMonitor.cs     # 内存监控
│   ├── ProcessAnalyzer.cs   # 进程分析
│   ├── MemoryReleaser.cs    # 内存释放
│   └── ...
├── Models/                  # 数据模型
│   ├── SystemMemoryInfo.cs
│   ├── ProcessMemoryInfo.cs
│   └── AppConfig.cs
├── Services/                # 服务层
│   ├── LoggerService.cs     # 日志服务
│   └── ConfigService.cs     # 配置服务
├── Utils/                   # 工具类
│   ├── PrivilegeManager.cs  # 权限管理
│   ├── PerformanceMonitor.cs# 性能监控
│   ├── TrayIconManager.cs   # 托盘图标
│   └── UpdateChecker.cs     # 更新检查
├── Dialogs/                 # 对话框
│   └── MacDialog.xaml       # macOS 风格对话框
├── MainWindow.xaml          # 主窗口
├── App.xaml                 # 应用程序
└── NLog.config              # 日志配置
```

### 版本历史

查看 [CHANGELOG.md](CHANGELOG.md) 了解详细的版本历史。

### 路线图

#### v3.6.0 (计划中)
- [ ] 帮助系统和工具提示
- [ ] 导出功能（CSV, JSON）
- [ ] 键盘快捷键

#### v4.0.0 (计划中)
- [ ] 单元测试（60%+ 覆盖率）
- [ ] 多语言支持（EN, ZH-CN）
- [ ] 深色主题
- [ ] 设置对话框

### 贡献

欢迎贡献！请遵循以下步骤：

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

### 致谢

- UI 设计灵感来自 macOS Big Sur
- 日志系统基于 NLog
- 图标使用 System.Drawing 绘制

### 联系方式

- **问题反馈**: [GitHub Issues](https://github.com/AriesOxO/MemCommitMonitor/issues)
- **功能建议**: [GitHub Discussions](https://github.com/AriesOxO/MemCommitMonitor/discussions)

---

## English

### ⚡ What's New in v3.5.0

**Complete Enterprise Infrastructure**:

- 🔍 **Professional Logging** - NLog integration, complete operation tracking
- 🛡️ **Global Exception Handling** - Triple protection, graceful degradation
- ⚙️ **Configuration Management** - JSON config, auto save/restore
- 👑 **Admin Privileges** - UAC elevation, privilege visualization
- 📊 **Performance Monitoring** - Real-time analysis, bottleneck detection
- 📱 **System Tray** - Background running, quick access
- 🔄 **Auto Update** - GitHub Releases integration

### Overview

MemCommit Monitor is an **enterprise-grade** Windows desktop application for monitoring and managing committed memory (virtual memory).

### Problem

Windows systems can encounter memory issues even when physical RAM is abundant:
- Physical memory: 14.6 GB available
- Committed memory: 18.7/63.9 GB (near limit)

Result: Applications crash or fail to start despite available RAM.

### Solution

MemCommit Monitor provides:

1. 🔍 **Real-time Monitoring** of system and process memory
2. 📊 **Process Analysis** to identify memory-heavy processes
3. 💾 **Manual Memory Release** (swap to page file)
4. 🛡️ **System Protection** prevents operations on critical processes
5. 📝 **Professional Logging** for diagnostics and troubleshooting
6. 📱 **System Tray** for background monitoring
7. 🔄 **Auto Updates** for staying current

### Features

- ✅ System memory status display
- ✅ Process memory usage list
- ✅ Sort by memory consumption
- ✅ One-click working set release
- ✅ System process protection
- ✅ macOS-inspired UI
- ✅ Enterprise-grade infrastructure

### Quick Start

1. Download from [Releases](https://github.com/AriesOxO/MemCommitMonitor/releases/latest)
2. Extract and run `MemCommitMonitor.exe`
3. Grant admin privileges when prompted (recommended)

### System Requirements

- **OS**: Windows 10 / 11 (x64)
- **.NET**: .NET 8.0 Desktop Runtime
- **Privileges**: Administrator recommended

### License

MIT License - see [LICENSE](LICENSE) file for details.

### Links

- [GitHub Repository](https://github.com/AriesOxO/MemCommitMonitor)
- [Download Latest](https://github.com/AriesOxO/MemCommitMonitor/releases/latest)
- [Report Issues](https://github.com/AriesOxO/MemCommitMonitor/issues)
- [Changelog](CHANGELOG.md)

---

**Made with ❤️ by AriesOxO**
