# FilePilot — 技术设计文档

## 技术栈

| 层级 | 技术 | 说明 |
|------|------|------|
| 运行时 | .NET 8.0 | 跨平台运行时 |
| UI 框架 | Avalonia UI 11.2.x | 跨平台桌面 UI |
| MVVM | CommunityToolkit.Mvvm 8.x | 轻量级 MVVM 工具包 |
| 配置 | System.Text.Json | JSON 序列化/反序列化 |
| 文件监视 | System.IO.FileSystemWatcher | 文件系统事件监听 |
| 通知 | 平台原生 | macOS 通知中心 / Windows Toast |

## 项目结构

```
FilePilot/
├── FilePilot.App/              # 主应用项目
│   ├── Models/                 # 数据模型
│   │   ├── Rule.cs             # 扩展名规则
│   │   ├── LogEntry.cs         # 操作日志条目
│   │   └── AppConfig.cs        # 应用配置
│   ├── ViewModels/             # MVVM ViewModel 层
│   │   ├── ViewModelBase.cs    # ViewModel 基类
│   │   ├── MainViewModel.cs    # 主窗口 ViewModel
│   │   ├── RuleListViewModel.cs
│   │   ├── LogPanelViewModel.cs
│   │   └── SettingsViewModel.cs
│   ├── Views/                  # Avalonia UI 视图
│   │   ├── MainWindow.axaml    # 主窗口
│   │   ├── RuleListView.axaml  # 规则列表面板
│   │   ├── LogPanelView.axaml  # 操作日志面板
│   │   └── SettingsWindow.axaml
│   ├── Services/               # 业务逻辑服务
│   │   ├── ConfigService.cs    # 配置读写
│   │   ├── FileOrganizer.cs    # 文件扫描/分类/移动
│   │   ├── FileWatcherService.cs
│   │   ├── UndoService.cs
│   │   ├── ConflictResolver.cs
│   │   └── NotificationService.cs
│   ├── Assets/                 # 图标、字体等资源
│   ├── App.axaml               # 应用入口 XAML
│   └── Program.cs              # 程序入口
├── docs/                       # 项目文档
├── dev-logs/                   # 开发日志
├── CLAUDE.md                   # AI 助手指引
└── FilePilot.sln               # 解决方案文件
```

## 架构模式：MVVM

```
View (AXAML) ←→ ViewModel (C#) ←→ Model (C#) ←→ Service (C#)
     ↑                ↑                ↑              ↑
  用户界面         数据绑定         数据模型       业务逻辑
```

- **View**: 纯 UI 描述，不包含业务逻辑
- **ViewModel**: 处理 UI 逻辑，通过数据绑定驱动 View
- **Model**: 纯数据结构，不含行为
- **Service**: 业务逻辑（文件操作、配置读写等）

## 数据流

```
用户操作 → ViewModel 命令 → Service 执行 → Model 更新 → UI 刷新
```

示例：点击"整理"按钮
1. View 绑定按钮到 `OrganizeCommand`
2. ViewModel 调用 `FileOrganizer.ScanAndOrganize()`
3. FileOrganizer 读取配置，扫描文件，执行移动
4. 移动结果记录为 `LogEntry`，添加到 `ObservableCollection`
5. UI 自动刷新日志列表和状态栏

## 配置文件设计

位置：`~/.filepilot/config.json`

```json
{
  "watchFolder": "/Users/xxx/Desktop",
  "archiveEnabled": false,
  "archiveDays": 30,
  "mode": "Manual",
  "rules": [
    { "extension": ".jpg", "targetFolder": "图片", "enabled": true },
    { "extension": ".png", "targetFolder": "图片", "enabled": true },
    { "extension": ".pdf", "targetFolder": "文档", "enabled": true }
  ],
  "ignoredFolders": ["图片", "文档", "归档"]
}
```
