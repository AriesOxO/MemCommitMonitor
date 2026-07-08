# MemCommit Monitor

> Windows 已提交内存（Committed Memory）监控和管理工具

[中文](#中文) | [English](#english)

---

## 中文

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

MemCommit Monitor 是一个轻量级的 Windows 桌面工具，可以：

1. **实时监控** 系统已提交内存和物理内存使用情况
2. **分析进程** 找出哪些进程占用了大量已提交内存
3. **手动释放** 选中进程的工作集（将内存页换出到页面文件）
4. **安全保护** 自动识别系统关键进程，防止误操作

### 功能特性

✅ 显示系统内存状态（已提交 / 物理内存）  
✅ 列出所有进程的已提交内存占用  
✅ 按内存占用排序，快速定位"内存大户"  
✅ 一键释放进程工作集  
✅ 系统进程保护机制  
✅ 简洁直观的 UI 界面

### 技术栈

- **框架**: .NET 8 + WPF
- **语言**: C#
- **架构**: MVVM 模式
- **API**: Windows Performance Counters + Process API

### 系统要求

- Windows 10/11 (x64)
- .NET 8 Runtime（或使用自包含版本）

### 使用方法

#### 方式一：从源码构建

```bash
# 克隆仓库
git clone https://github.com/yourusername/MemCommitMonitor.git
cd MemCommitMonitor

# 构建项目
dotnet build -c Release

# 运行
dotnet run --project MemCommitMonitor
```

#### 方式二：发布为独立可执行文件

```bash
# 发布为单文件自包含应用（无需安装 .NET Runtime）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 可执行文件位于
# MemCommitMonitor\bin\Release\net8.0-windows\win-x64\publish\MemCommitMonitor.exe
```

### 使用说明

1. **启动应用** - 程序会自动加载系统和进程内存信息
2. **查看进程** - 进程列表按已提交内存降序排列
3. **选择进程** - 点击要释放的进程
4. **释放内存** - 点击"释放选中进程"按钮
5. **确认操作** - 确认对话框中查看详情
6. **查看结果** - 显示释放前后的内存对比

**注意事项**：
- 标记为 "🔒 受保护" 的系统进程无法释放
- 释放操作是安全的，只是将内存页换出到页面文件
- 部分进程可能需要管理员权限才能访问

### 工作原理

程序调用 Windows API `EmptyWorkingSet()` 来释放进程的工作集：

```csharp
[DllImport("psapi.dll")]
static extern bool EmptyWorkingSet(IntPtr hProcess);
```

这个操作会：
1. 将进程的物理内存页换出到页面文件（页面文件位于磁盘）
2. 释放物理内存供其他进程使用
3. 不会终止进程或丢失数据
4. 进程下次访问这些内存时会自动从页面文件加载回来

### 安全性说明

- ✅ 开源透明，代码可审计
- ✅ 不收集任何用户数据
- ✅ 不联网，纯本地运行
- ✅ 内置系统进程保护机制
- ✅ 所有操作需用户确认

### 路线图

**v1.0（当前 MVP）**
- [x] 系统内存监控
- [x] 进程列表展示
- [x] 手动释放功能
- [x] 基础保护机制

**v1.1（计划中）**
- [ ] 实时内存图表
- [ ] 自动释放策略（阈值触发）
- [ ] 系统托盘运行
- [ ] 白名单/黑名单管理

**v1.2（计划中）**
- [ ] 多语言支持
- [ ] 日志记录与导出
- [ ] 命令行参数支持

### 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 许可证

[MIT License](LICENSE)

### 致谢

灵感来源于日常使用 Windows 时遇到的内存管理问题。

---

## English

### Background

Windows systems have a common but often overlooked memory management issue: **Virtual Memory (Committed Memory) Exhaustion**.

When you see in Task Manager:
- Plenty of available physical memory (e.g., 14.6 GB available)
- But "Committed" memory is near the limit (e.g., 18.7/63.9 GB)

Even with sufficient physical memory, the system will refuse new memory allocation requests due to committed memory exhaustion, causing:
- Application crashes
- Unable to start new programs
- System instability

### Solution

MemCommit Monitor is a lightweight Windows desktop tool that can:

1. **Real-time monitoring** of system committed memory and physical memory usage
2. **Process analysis** to identify which processes consume large amounts of committed memory
3. **Manual release** of selected process working sets (swap memory pages to page file)
4. **Safety protection** to automatically identify critical system processes and prevent misoperation

### Features

✅ Display system memory status (committed / physical memory)  
✅ List all processes' committed memory usage  
✅ Sort by memory usage to quickly locate "memory hogs"  
✅ One-click release of process working set  
✅ System process protection mechanism  
✅ Clean and intuitive UI

### Tech Stack

- **Framework**: .NET 8 + WPF
- **Language**: C#
- **Architecture**: MVVM pattern
- **API**: Windows Performance Counters + Process API

### System Requirements

- Windows 10/11 (x64)
- .NET 8 Runtime (or use self-contained version)

### License

[MIT License](LICENSE)

---

**Star ⭐ this repo if you find it helpful!**




