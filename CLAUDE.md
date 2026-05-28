# CLAUDE.md — FilePilot 项目助手指引

## 项目概述
FilePilot 是一款跨平台（macOS + Windows）桌面文件自动整理工具，使用 C# + Avalonia UI 构建。
帮助用户自动分类、归档桌面文件，保持桌面整洁。

## 项目文档
所有设计文档位于 [docs/](docs/) 目录：
- [功能需求](docs/requirements.md) — 完整的功能需求列表
- [技术设计](docs/technical-design.md) — 架构、技术栈、配置文件设计
- [执行步骤](docs/execution-steps.md) — 逐步开发计划及验证标准

## 开发日志
位于 [dev-logs/](dev-logs/) 目录：
- [已完成事项](dev-logs/completed.md) — 带日期标记的完成记录
- [待办事项](dev-logs/todo.md) — 当前和计划中的任务

## 关键文件路径
- 解决方案：[FilePilot.sln](FilePilot.sln)
- 主项目：[FilePilot.App/FilePilot.App.csproj](FilePilot.App/FilePilot.App.csproj)
- 程序入口：[FilePilot.App/Program.cs](FilePilot.App/Program.cs)
- MVVM 基类：[FilePilot.App/ViewModels/ViewModelBase.cs](FilePilot.App/ViewModels/ViewModelBase.cs)
- 主窗口：[FilePilot.App/Views/MainWindow.axaml](FilePilot.App/Views/MainWindow.axaml)

## 工作准则

### 开发流程
1. 严格按执行步骤逐步开发，每步完成后用户验证
2. 每步结束后更新 [dev-logs/completed.md](dev-logs/completed.md) 和 [dev-logs/todo.md](dev-logs/todo.md)
3. 每步必须满足：`dotnet build` 零错误 + `dotnet run` 可启动

### 代码规范
- 使用 CommunityToolkit.Mvvm 的 `[ObservableProperty]`, `[RelayCommand]` 属性
- ViewModel 继承 `ViewModelBase`（已内置 ObservableObject）
- 业务逻辑放在 Services/ 中，不放在 ViewModel 中
- 配置读写统一通过 ConfigService，不直接访问文件
- 文件移动操作必须通过 FileOrganizer，确保可追踪和可撤销

### 禁止事项
- 不删除文件（只移动），这是核心安全约束
- 不在 View 中写业务逻辑
- 不使用数据库，所有持久化走 JSON 配置文件
- 不硬编码文件路径，统一从配置读取

### 常用命令
```bash
# 编译项目
dotnet build

# 运行应用
dotnet run --project FilePilot.App

# 添加 NuGet 包
dotnet add FilePilot.App package <PackageName>
```

### 配置文件位置
运行时配置存储在 `~/.filepilot/config.json`
