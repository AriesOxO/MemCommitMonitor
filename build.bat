@echo off
echo ================================
echo MemCommit Monitor - 构建脚本
echo ================================
echo.

echo [1/3] 检查 .NET SDK...
dotnet --version
if errorlevel 1 (
    echo 错误: 未找到 .NET SDK，请先安装 .NET 8 SDK
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo.
echo [2/3] 还原依赖包...
dotnet restore MemCommitMonitor.sln
if errorlevel 1 (
    echo 错误: 依赖包还原失败
    pause
    exit /b 1
)

echo.
echo [3/3] 编译项目 (Release x64)...
dotnet build MemCommitMonitor.sln -c Release -p:Platform=x64
if errorlevel 1 (
    echo 错误: 编译失败
    pause
    exit /b 1
)

echo.
echo ================================
echo 构建成功!
echo ================================
echo 可执行文件位置:
echo MemCommitMonitor\bin\Release\net8.0-windows\x64\MemCommitMonitor.exe
echo.
pause
