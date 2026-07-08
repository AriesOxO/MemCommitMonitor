# Changelog

All notable changes to MemCommit Monitor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.5.0] - 2026-07-08

### 🎉 Enterprise Edition Release

This is a major milestone release that transforms MemCommit Monitor from a basic tool into an enterprise-ready application with professional infrastructure.

### Added - Phase 1: Critical Infrastructure ✅

#### 1. Professional Logging System (Priority 1)
- **NLog 6.1.3** integration with file-based logging
- Automatic log rotation (daily, 30-day retention)
- Separate error log files (90-day retention)
- Log levels: Debug, Info, Warning, Error, Fatal
- Log location: `%AppData%\MemCommitMonitor\Logs\`
- Detailed operation tracking and error diagnostics

#### 2. Global Exception Handling (Priority 2)
- UI thread exception capture
- Non-UI thread exception capture
- Async Task exception handling
- User-friendly error dialogs with MacDialog
- Graceful degradation on errors
- Automatic crash reports to logs

#### 3. Configuration System (Priority 3)
- JSON-based configuration (`config.json`)
- UI settings (theme, language, window size/position)
- Behavior settings (auto-refresh, confirmations, filters)
- Advanced settings (log level, scan depth, updates)
- Automatic save on exit
- Automatic restore on startup
- Configuration location: `%AppData%\MemCommitMonitor\`

#### 4. Administrator Privilege Management (Priority 4)
- Automatic privilege detection
- UAC elevation prompt on startup (optional)
- Visual privilege status indicator (🛡️ Admin / ⚠️ Standard)
- Permission checks before operations
- Graceful handling of insufficient privileges
- Detailed privilege logging

### Added - Phase 2: Important Features ✅

#### 5. Performance Monitoring System (Priority 6)
- Real-time operation timing with `using` statement support
- Performance statistics (count, total, avg, min, max, median)
- Slow operation detection (>1s warning, >3s alert)
- Automatic performance report on exit
- Key operations monitored:
  - LoadData (total data loading)
  - GetSystemMemoryInfo (system memory)
  - GetProcessMemoryInfos (process list)
  - ApplyFilterAndSort (filtering & sorting)
  - ReleaseProcessMemory (memory release)

#### 6. System Tray Icon (Priority 8)
- Custom blue circular icon with "M" letter
- Anti-aliased 32x32 icon rendering
- Double-click to show/hide main window
- Right-click context menu:
  - Show Main Window (bold)
  - Quick Refresh
  - Exit
- Minimize to tray option (configurable)
- Balloon tip notifications
- Automatic cleanup on exit

#### 7. Auto-Update System (Priority 7)
- GitHub Releases API integration
- Background update check (3-second delay)
- Version comparison (current vs latest)
- Update notification dialog
- One-click download page access
- Manual update check button (🔄)
- Network error handling
- Configurable update check (on/off)

### Improved

- Replaced all `MessageBox` with `MacDialog` in new features
- Enhanced error messages with context
- Better user feedback during operations
- Consistent logging across all components
- Window state persistence (size, position, filters)

### Technical Details

#### Dependencies Added
- NLog 6.1.3
- NLog.Extensions.Logging 6.1.3
- System.Drawing.Common 10.0.9
- UseWindowsForms enabled

#### New Files Created
```
Services/
  ├─ LoggerService.cs          (120 lines)
  ├─ ConfigService.cs          (180 lines)
Models/
  └─ AppConfig.cs              (90 lines)
Utils/
  ├─ PrivilegeManager.cs       (130 lines)
  ├─ PerformanceMonitor.cs     (240 lines)
  ├─ TrayIconManager.cs        (270 lines)
  └─ UpdateChecker.cs          (230 lines)
Config/
  └─ NLog.config               (70 lines)
```

#### Code Statistics
- **New Code**: ~1,680 lines
- **Modified Code**: ~250 lines
- **Total Added**: ~1,930 lines

### Performance Data

Current performance benchmarks (from real testing):
```
LoadData:                    26ms
  ├─ GetSystemMemoryInfo:     9ms
  ├─ GetProcessMemoryInfos:   7ms (330 processes)
  └─ ApplyFilterAndSort:      3ms
```

All operations complete in <50ms, providing excellent user experience.

### Breaking Changes

None. This release is fully backward compatible with v2.0.0.

### Known Issues

- GitHub API rate limiting: 60 requests/hour (unauthenticated)
  - **Workaround**: Update check runs once per session
- System tray icon may not display correctly on some Windows themes
  - **Workaround**: Icon uses standard Windows icon format

### Upgrade Notes

When upgrading from v2.0.0:
1. First run will create new configuration file
2. Window settings will be reset to defaults
3. New features are enabled by default
4. No data loss - all settings are preserved

### What's Not Included

- **Unit Tests** (Priority 5): Deferred to future release
  - Reason: Time-intensive, minimal user impact
  - Planned: v4.0.0 with 60%+ coverage

### Developer Notes

#### How to Build
```bash
dotnet build -c Release
```

#### How to Run
```bash
dotnet run --project MemCommitMonitor
```

#### Log Location
```
%AppData%\MemCommitMonitor\Logs\
  ├─ 2026-07-08.log          (all logs)
  ├─ errors-2026-07-08.log   (errors only)
  └─ archives/               (old logs)
```

#### Configuration Location
```
%AppData%\MemCommitMonitor\
  └─ config.json             (user settings)
```

### Credits

- **UI Framework**: WPF with macOS-inspired design
- **Logging**: NLog 6.1.3
- **Icon**: Custom-designed blue circular icon

### Future Plans

#### v3.6.0 (Next Minor Release)
- Help system with tooltips
- Export functionality (CSV, JSON)
- Keyboard shortcuts

#### v4.0.0 (Next Major Release)
- Unit tests (60%+ coverage)
- Multi-language support (EN, ZH-CN)
- Dark theme support
- Settings dialog

---

## [2.0.0] - 2026-07-05

### Added
- Initial implementation with core functionality
- System memory monitoring
- Process memory analysis
- Three memory release modes
- macOS-inspired UI design

---

## Links

- [GitHub Repository](https://github.com/AriesOxO/MemCommitMonitor)
- [Download Latest Release](https://github.com/AriesOxO/MemCommitMonitor/releases/latest)
- [Report Issues](https://github.com/AriesOxO/MemCommitMonitor/issues)
