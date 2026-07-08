@echo off
echo ================================
echo MemCommit Monitor - 发布脚本
echo ================================
echo.
echo 创建独立可执行文件（不需要安装 .NET Runtime）
echo.

echo [1/2] 发布为单文件应用...
dotnet publish MemCommitMonitor\MemCommitMonitor.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishTrimmed=false

if errorlevel 1 (
    echo 错误: 发布失败
    pause
    exit /b 1
)

echo.
echo ================================
echo 发布成功!
echo ================================
echo 可执行文件位置:
echo MemCommitMonitor\bin\Release\net8.0-windows\win-x64\publish\MemCommitMonitor.exe
echo.
echo 文件大小约 70-80 MB（包含 .NET Runtime）
echo 可直接分发此 EXE 文件，无需安装任何依赖
echo.
pause
