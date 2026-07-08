# 快速开始指南

## 前置要求

- Windows 10/11 (x64)
- .NET 8 SDK（开发）或 .NET 8 Runtime（运行）

### 安装 .NET 8 SDK

1. 访问：https://dotnet.microsoft.com/download/dotnet/8.0
2. 下载并安装 ".NET 8 SDK (v8.0.x)"
3. 验证安装：
   ```bash
   dotnet --version
   # 应输出类似: 8.0.xxx
   ```

## 方式一：从源码构建

### 1. 克隆仓库

```bash
git clone https://github.com/yourusername/MemCommitMonitor.git
cd MemCommitMonitor
```

### 2. 构建项目

**Windows (推荐)：**
```bash
# 双击运行
build.bat
```

**命令行：**
```bash
dotnet restore
dotnet build -c Release
```

### 3. 运行程序

```bash
# 开发模式
dotnet run --project MemCommitMonitor

# 或直接运行编译后的 EXE
MemCommitMonitor\bin\Release\net8.0-windows\x64\MemCommitMonitor.exe
```

## 方式二：发布独立可执行文件

### 1. 发布为单文件应用

**Windows (推荐)：**
```bash
# 双击运行
publish.bat
```

**命令行：**
```bash
dotnet publish MemCommitMonitor\MemCommitMonitor.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true
```

### 2. 获取可执行文件

发布后的文件位于：
```
MemCommitMonitor\bin\Release\net8.0-windows\win-x64\publish\MemCommitMonitor.exe
```

**特点：**
- 单个 EXE 文件（约 70-80 MB）
- 包含 .NET Runtime，无需安装任何依赖
- 可直接分发给其他用户

## 使用说明

### 启动程序

双击 `MemCommitMonitor.exe` 或通过命令行启动。

### 主界面说明

```
┌─────────────────────────────────────────┐
│  系统内存状态                              │
│  已提交: 18.7 GB / 63.9 GB (29.2%)       │
│  物理内存: 17.0 GB / 32.0 GB (53.1%)     │
├─────────────────────────────────────────┤
│  [刷新]  [释放选中进程]                    │
├─────────────────────────────────────────┤
│  进程列表（按已提交内存排序）               │
│  PID  │ 进程名     │ 已提交  │ 工作集   │
│  ─────┼───────────┼────────┼─────────│
│  1234 │ chrome.exe│ 2.5 GB │ 1.8 GB  │
└─────────────────────────────────────────┘
```

### 操作步骤

1. **查看进程**
   - 启动后自动加载进程列表
   - 进程按 "已提交内存" 降序排列
   - 找出占用最多的进程

2. **释放内存**
   - 点击选择要释放的进程
   - 点击 "释放选中进程" 按钮
   - 确认对话框中查看详细信息
   - 点击 "是" 执行释放

3. **查看结果**
   - 弹窗显示释放前后的内存对比
   - 自动刷新进程列表

4. **定期刷新**
   - 点击 "刷新" 按钮更新数据
   - 查看最新的内存状态

### 注意事项

⚠️ **受保护进程**
- 标记为 "🔒 受保护" 的系统进程无法释放
- 这些进程是系统关键进程，释放可能导致系统不稳定

⚠️ **管理员权限**
- 访问某些进程可能需要管理员权限
- 右键点击程序 → "以管理员身份运行"

⚠️ **释放效果**
- 释放的是物理内存（WorkingSet）
- 不会终止进程或丢失数据
- 进程会在需要时自动重新加载内存

## 常见问题

### Q: 为什么某些进程显示 "🔒 受保护"？
A: 这些是系统关键进程（如 explorer.exe, dwm.exe），释放它们可能导致系统不稳定。

### Q: 释放内存后为什么进程还在运行？
A: 释放操作只是将内存页换出到页面文件，不会终止进程。进程继续正常运行。

### Q: 释放后内存为什么又涨回去了？
A: 进程访问被换出的内存时，系统会自动将其加载回物理内存。这是正常现象。

### Q: 我可以自动化释放内存吗？
A: 当前版本仅支持手动释放。自动释放策略将在 v1.1 版本中加入。

### Q: 需要安装什么依赖？
A: 如果使用发布的独立可执行文件（publish.bat），无需任何依赖。否则需要安装 .NET 8 Runtime。

## 下一步

- 阅读 [README.md](README.md) 了解项目详情
- 查看 [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) 了解代码架构
- 阅读 [CONTRIBUTING.md](CONTRIBUTING.md) 了解如何贡献

## 获取帮助

遇到问题？欢迎在 GitHub Issues 中提问！
