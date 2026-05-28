# FilePilot — 执行步骤

## Step 0: 项目初始化 ✅
**完成时间**: 2026-05-27
- [x] 安装 Avalonia 模板，创建 MVVM 项目，配置 .NET 8.0
- [x] 创建项目结构、文档、开发日志、CLAUDE.md
- [x] dotnet build 零错误零警告

## Step 1: 核心数据模型 ✅
- [x] Models/Rule.cs, LogEntry.cs, AppConfig.cs（含 30 条默认规则）

## Step 2: 配置服务 ✅
- [x] Services/ConfigService.cs — JSON 配置读写、线程安全、深拷贝

## Step 3: 文件整理核心 ✅
- [x] Services/ConflictResolver.cs + FileOrganizer.cs — 扫描/分类/移动/归档

## Step 4: 主窗口 UI 外壳 ✅
- [x] 工具栏 + 左右分栏 + 状态栏 + 毛玻璃 + 跨平台适配

## Step 5: 规则管理 UI ✅
- [x] 增删改规则 + 即时保存 + 重复检测

## Step 6: 手动整理按钮 ✅
- [x] Organize 连接 FileOrganizer + 异步执行 + 统计显示

## Step 7: 操作日志面板 ✅
- [x] 实时日志显示 + 状态指示灯

## Step 9: 撤销功能 ✅
**完成时间**: 2026-05-27
- [x] Services/UndoService.cs — 撤销栈（`Stack<LogEntry>`），每次成功移动自动记录
- [x] 双模式撤销按钮：位于操作日志面板右上角
  - "撤回最近一次归类"：弹出栈顶条目，调用 `UndoLast()` 逐条撤销
  - "撤回多次归类"：进入多选模式，可批量勾选后执行 `UndoMultiple()`
- [x] 多选模式交互：
  - 点击"撤回多次归类"后，可撤销日志右上角出现空心圆圈（灰色边框 `#9CA3AF`）
  - 点击圆圈切换选中状态（绿色填充 `#22C55E`）
  - "全选"按钮一键选中所有可撤销条目，"撤回选中"执行批量撤销后退出多选模式
  - "取消"退出多选模式并清空所有选中状态
- [x] 可撤销判定（`IsUndoable`）：仅限 `Success && !IsInfo && ActionLabel == "成功归类" && !HasBeenUndone`
  - 已跳过的条目、信息条目（模式切换日志）、撤回结果日志、已被撤销过的条目均不显示选择圆圈
- [x] LogEntry 升级为 `ObservableObject`，新增 `IsSelectedForUndo`、`CanSelectForUndo`、`HasBeenUndone`、`IsUndoable`、`SelectionCircleBg`、`SelectionCircleBorder`
- [x] UndoService 重构：提取核心逻辑到 `UndoEntry()` 私有方法，`UndoLast()` 和 `UndoMultiple()` 共用
- [x] `UndoMultiple()` 弹出全部栈条目，撤销选中的，其余按原序推回
- [x] 撤销成功后 `_fileWatcher.IgnoreNext()` 阻止被实时监视反弹
- [x] 撤销日志区分显示："已撤回最近一次归类" / "已撤回选中的 X 次归类"
- [x] 状态栏显示 `可撤回 X 次`

## Bug 修复 ✅
- [x] FileOrganizer 不再扫描子文件夹（仅桌面根目录直接文件）

## Step 8: 实时文件监视 ✅
**完成时间**: 2026-05-27
- [x] Services/FileWatcherService.cs — 封装 FileSystemWatcher，500ms 防抖 + 一次性忽略列表
- [x] `IsDirectChild()` 校验防止 macOS FSEvents 误报子文件夹事件
- [x] FileOrganizer.OrganizeSingleFile() 防御性校验，双重保护
- [x] 手动模式 / 自动模式两个并列按钮，圆形指示灯（选中绿/未选中灰）
- [x] 启动时不自动进入任何模式，须用户手动选择
- [x] 点击“自动模式” → 应用内确认弹窗 → 同意后执行整理 + 实时监视
- [x] 撤回文件自动加入忽略列表，避免被实时监视反弹
- [x] 模式切换写入日志（灰色圆点，不可撤销的信息条目）

## Step 10: 设置对话框 ✅（已简化为监视文件夹选择器）
- [x] 删除了未实现的设置弹窗，改为"整理规则"标题右侧内联显示监视文件夹
- [x] "浏览"按钮实时更换监视文件夹，即时写入配置

## Step 10.5: 源文件夹/目标文件夹架构重构 ✅
**完成时间**: 2026-05-27
- [x] `AppConfig` 删除 `WatchFolder`，新增 `SourceFolder` + `DestinationFolder`
- [x] `FileOrganizer` 扫描源文件夹 → 归类到目标文件夹的子目录
- [x] 左侧面板：源/目标文件夹选择器
- [x] 规则表头"目标文件夹"改名"归类去向"，TargetFolder 改为绿色只读徽章
- [x] 添加规则表单 TargetFolder 改为 ComboBox（代码后置设置 ItemsSource，避免 DataTemplate 绑定问题）
- [x] `RefreshTargetFolders()` 扫描目标文件夹的直接子目录填充 ComboBox 选项
- [x] 编译通过

## Step 10.6: UI 回退与简化 ✅
**完成时间**: 2026-05-27
- [x] 放弃 ComboBox/Flyout 列表选择器方案（Avalonia 下多次尝试均无法正常显示下拉）
- [x] 回退到 TextBox 手动输入扩展名和目标文件夹名
- [x] 恢复 30 条默认规则（AppConfig.CreateDefault()）
- [x] 恢复 Desktop 默认源文件夹路径
- [x] 规则行目标文件夹可编辑 TextBox（替代绿色只读徽章）
- [x] 编译通过

## Step 10.7: UI 文本与布局优化 ✅
- [x] "目标文件夹" → "目标文件夹完整名称"（规则表头 + 添加表单 Watermark）
- [x] "整理规则" → "归类规则"（左侧面板标题）
- [x] "桌面整理" → "发起归类"（按钮文本）
- [x] "发起归类"按钮从工具栏移至"归类规则"标题右侧，样式与撤回按钮统一
- [x] `Rule.Enabled` 默认为 `false`（ToggleSwitch 默认关闭）
- [x] `DestinationFolder` 每次启动清空（不持久化到 config.json）
- [x] 编译通过

## Step 11: 系统通知与 UI 打磨 ✅
**完成时间**: 2026-05-28
- [x] Services/NotificationService.cs — 零依赖跨平台通知（macOS osascript + Windows PowerShell Toast）
- [x] 组织完成、撤回成功时弹出系统通知
- [x] "发起归类"按钮组织过程中显示"整理中..."并禁用
- [x] 状态栏 2px 蓝色不定进度条
- [x] 规则列表空状态占位
- [x] 操作日志空状态占位
- [x] 模式守卫：未选模式时禁止归类，状态栏三层提示
- [x] 规则自动排序（启用置顶）+ 手动添加自动启用
- [x] 规则行/表头/按钮/添加栏样式优化
- [x] 窗口尖角化，去除毛玻璃与外层阴影
- [x] 国际化支持：中/英文自动切换，所有 UI 文本本地化
