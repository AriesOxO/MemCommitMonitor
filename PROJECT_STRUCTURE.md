# MemCommit Monitor - 项目结构

## 📁 目录结构

```
MemCommitMonitor/
│
├── MemCommitMonitor/                  # 主项目目录
│   ├── Core/                          # 核心业务逻辑
│   │   ├── MemoryMonitor.cs           # 系统内存监控器
│   │   ├── ProcessAnalyzer.cs         # 进程内存分析器
│   │   └── MemoryReleaser.cs          # 内存释放器
│   │
│   ├── Models/                        # 数据模型
│   │   ├── SystemMemoryInfo.cs        # 系统内存信息模型
│   │   └── ProcessMemoryInfo.cs       # 进程内存信息模型
│   │
│   ├── Utils/                         # 工具类
│   │   └── NativeMethods.cs           # Windows API 声明
│   │
│   ├── MainWindow.xaml                # 主窗口 UI 定义
│   ├── MainWindow.xaml.cs             # 主窗口逻辑代码
│   ├── App.xaml                       # 应用入口 UI
│   ├── App.xaml.cs                    # 应用入口逻辑
│   └── MemCommitMonitor.csproj        # 项目文件
│
├── MemCommitMonitor.sln               # Visual Studio 解决方案
├── build.bat                          # 构建脚本
├── publish.bat                        # 发布脚本
├── README.md                          # 项目说明文档
├── LICENSE                            # MIT 开源协议
├── CONTRIBUTING.md                    # 贡献指南
└── .gitignore                         # Git 忽略文件
```

## 📦 核心模块说明

### Core/ - 核心业务逻辑

#### MemoryMonitor.cs
- **职责**：监控系统级内存状态
- **关键方法**：`GetSystemMemoryInfo()` - 获取已提交内存和物理内存信息
- **使用 API**：`GlobalMemoryStatusEx`, `PerformanceCounter`

#### ProcessAnalyzer.cs
- **职责**：分析所有进程的内存占用
- **关键方法**：`GetProcessMemoryInfos()` - 返回按已提交内存排序的进程列表
- **保护机制**：维护系统关键进程黑名单，防止误操作

#### MemoryReleaser.cs
- **职责**：释放进程工作集
- **关键方法**：`ReleaseProcessMemory()` - 调用 Windows API 清空进程工作集
- **安全检查**：拒绝释放受保护的系统进程

### Models/ - 数据模型

#### SystemMemoryInfo.cs
包含系统内存信息：
- `TotalCommitted` - 已提交内存总量
- `CommitLimit` - 提交限制
- `TotalPhysical` - 物理内存总量
- `AvailablePhysical` - 可用物理内存
- 计算属性：`CommittedPercentage`, `PhysicalPercentage`

#### ProcessMemoryInfo.cs
包含进程内存信息：
- `ProcessId` - 进程 ID
- `ProcessName` - 进程名称
- `PrivateBytes` - 已提交内存（Private Bytes）
- `WorkingSet` - 工作集（物理内存）
- `IsProtected` - 是否为受保护进程
- 格式化属性：`FormattedPrivate`, `FormattedWorkingSet`
- 实现 `INotifyPropertyChanged` 支持数据绑定

### Utils/ - 工具类

#### NativeMethods.cs
- **职责**：声明 Windows Native API
- **关键 API**：
  - `EmptyWorkingSet` - 清空进程工作集
  - `GlobalMemoryStatusEx` - 获取系统内存状态
  - `MEMORYSTATUSEX` - 内存状态结构体

## 🎨 UI 层

### MainWindow.xaml / MainWindow.xaml.cs
- **主界面**：系统内存状态面板 + 进程列表 + 操作按钮
- **数据绑定**：使用 WPF DataGrid 绑定进程列表
- **交互逻辑**：
  - 刷新数据
  - 选择进程
  - 释放内存（带确认对话框）
  - 状态提示

## 🔧 技术实现细节

### Windows API 调用

```csharp
// 1. 获取系统内存状态
[DllImport("kernel32.dll")]
bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

// 2. 清空进程工作集
[DllImport("psapi.dll")]
bool EmptyWorkingSet(IntPtr hProcess);
```

### 内存释放原理

1. 调用 `Process.GetProcessById(pid)` 获取进程句柄
2. 调用 `EmptyWorkingSet(process.Handle)` 将内存页换出到页面文件
3. 释放的是物理内存（WorkingSet），已提交内存（PrivateBytes）不变
4. 进程下次访问这些内存时会自动从页面文件加载

### 受保护进程列表

```csharp
System, Registry, smss, csrss, wininit, services,
lsass, winlogon, dwm, explorer, svchost, audiodg,
taskmgr, fontdrvhost
```

## 🚀 构建和运行

### 开发模式
```bash
dotnet build -c Debug
dotnet run --project MemCommitMonitor
```

### 发布独立可执行文件
```bash
# 使用 publish.bat 脚本
publish.bat

# 或手动命令
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 📊 性能考量

- **刷新频率**：手动刷新（避免后台线程开销）
- **进程枚举**：约 1-2 秒（取决于进程数量）
- **内存占用**：应用本身约 30-50 MB
- **CPU 占用**：刷新时短暂峰值，平时 0%

## 🔒 安全机制

1. **系统进程保护**：黑名单机制，拒绝释放关键进程
2. **权限处理**：无权限访问的进程自动跳过
3. **确认对话框**：所有释放操作需用户二次确认
4. **错误处理**：完善的异常捕获和用户提示


