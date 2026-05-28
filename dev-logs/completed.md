# 已完成开发事项

## 2026-05-27

### Step 0-2: 项目初始化 + 数据模型 + 配置服务
- 创建 Avalonia MVVM 项目，配置 .NET 8.0 + CommunityToolkit.Mvvm
- Models: Rule.cs, LogEntry.cs, AppConfig.cs（含 30 条默认规则）
- Services/ConfigService.cs — JSON 配置 ~/.filepilot/config.json，线程安全
- 文档目录 docs/、开发日志 dev-logs/、CLAUDE.md
- 编译成功

### Step 3: 文件整理核心
- Services/ConflictResolver.cs — 文件名冲突自动重命名（file.jpg → file(1).jpg）
- Services/FileOrganizer.cs — 扫描/分类/移动/归档
- 编译成功

### Step 4: 主窗口 UI 外壳
- 工具栏 + 左右分栏 + 状态栏
- 毛玻璃效果（macOS/Windows 自适应）
- macOS Sonoma 风格 + Windows 适配
- 编译成功

### Step 5: 规则管理 UI
- 添加/删除/编辑规则，即时保存到配置文件
- 重复扩展名检测
- 编译成功

### Step 6-7: 手动整理 + 操作日志
- Organize 按钮连接 FileOrganizer，异步执行不阻塞 UI
- 操作日志实时显示（绿色成功/红色失败）
- 整理完成后显示统计（移动 X 个，跳过 Y 个）
- 编译成功

### Step 9: 撤销功能（紧急插队 → 完整实现）
- Services/UndoService.cs — 撤销栈 + 逐条撤销 + 批量撤销
- 每次成功移动自动记录到 `Stack<LogEntry>`
- 状态栏显示 `可撤回 X 次`

**单次撤回**（"撤回最近一次归类"按钮，位于操作日志右上角）：
- `UndoLast()` 弹出栈顶，文件移回原位，`IgnoreNext()` 防止自动监视反弹
- 日志显示"已撤回最近一次归类"，带绿色圆点

**多次撤回模式**（"撤回多次归类"按钮，操作日志右上角）：
- 点击后进入多选模式，`IsMultiUndoMode` 切换按钮组显示（全选/撤回选中/取消）
- 可撤销日志右上角出现空心圆圈（`CanSelectForUndo = IsUndoable`），点击圆圈绿色填充
- "全选"一键选中所有可撤销条目，"撤回选中"执行批量撤销
- `UndoMultiple(HashSet<LogEntry>)`：弹出全部栈条目 → 撤销选中的 → 其余按原序推回
- 撤销成功后日志显示"已撤回选中的 X 次归类"
- "取消"退出多选，清空选中状态

**可撤销判定（`IsUndoable`）**：
- 仅限 `Success && !IsInfo && ActionLabel == "成功归类" && !HasBeenUndone`
- 已跳过、info 条目、撤回结果条目、已被撤销过的条目均不显示选择圆圈

**代码改动**：
- `LogEntry` 继承 `ObservableObject`，新增 `IsSelectedForUndo`、`CanSelectForUndo`、`HasBeenUndone`、`IsUndoable`、`SelectionCircleBg`/`SelectionCircleBorder`
- `UndoService` 提取 `UndoEntry()` 私有方法（`UndoLast` 和 `UndoMultiple` 共用），撤销成功时标记 `entry.HasBeenUndone = true`
- 删除工具栏旧"撤销"按钮，撤回到操作日志面板头部
- `OnToggleUndoSelection` 点击处理（code-behind）
- ActionLabel 统一从"成功分类"改为"成功归类"
- 编译成功

### Bug 修复
- **严重 Bug**: FileOrganizer.ScanFiles() 误扫描子文件夹一层深度，导致用户子文件夹内文件被拖出
  - 修复：改为仅扫描桌面根目录的直接文件
  - 已通过恢复脚本将文件移回

### Step 8: 实时文件监视
- Services/FileWatcherService.cs — 封装 FileSystemWatcher，500ms 防抖
- 手动模式 / 自动模式两个并列按钮，带圆形指示灯（灰/绿）
- 启动时两个按钮均为未选中状态（灰色），不自动进入任何模式
- 点击“自动模式”弹出应用内确认弹窗（遮罩层 + 居中白色对话框）
- 同意后执行一次桌面整理 + 开始实时监视
- 新文件自动整理，日志和 UI 实时更新
- 编译成功

### Step 10: 设置对话框 → 简化重构
- 删除了设置弹窗（归档设置、忽略文件夹等功能未实现或不必要），改为直接在"整理规则"标题右侧显示监视文件夹路径
- 只读文本框显示当前路径 + "浏览"按钮调用原生文件夹选择器，更换后即时保存并重启 Watcher（若处于自动模式）
- 编译成功

### Step 8 迭代：安全修复 + UI 打磨
- **安全修复**: FileWatcherService 新增 `IsDirectChild()` 校验，防止 macOS FSEvents 误报子文件夹事件
- **安全修复**: FileOrganizer.OrganizeSingleFile() 增加防御性校验，双重保护子文件夹不被触碰
- **撤回保护**: FileWatcherService 新增 `IgnoreNext()` 一次性忽略列表，撤回后文件不再被自动归档
- **确认弹窗**: 从独立 Window 改为应用内遮罩层弹窗，始终显示在主窗口上层
- **按钮改名**: “开始整理” → “桌面整理”
- **日志区分**: LogEntry 新增 `ActionLabel` 属性，区分“成功分类”/“已跳过”/“成功撤回”/“撤回失败”
- **模式日志**: LogEntry 新增 `IsInfo` 类型（灰色圆点），切换模式时记录“当前模式：手动/自动模式”
- **状态栏简化**: 删除左下角状态文字，文件统计信息居中显示
- `LogEntry.Info()` 静态工厂方法、`DisplayText` 组合显示属性
- 编译成功

### 源文件夹/目标文件夹架构重构
- 删除 `AppConfig.WatchFolder`，拆分为 `SourceFolder` + `DestinationFolder`
- `FileOrganizer` 扫描源文件夹 → 归类到目标文件夹的对应子目录
- 左侧面板重新设计：
  - 源文件夹行（只读 TextBox + 浏览按钮）
  - 目标文件夹行（只读 TextBox + 浏览按钮）
  - 规则表头"目标文件夹"改名为"归类去向"
  - 规则列表 TargetFolder 改为绿色只读徽章
  - 添加规则表单中 TargetFolder 改为 ComboBox，选项来自目标文件夹的直接子目录
- `ChangeSourceFolder()` 自动模式下重启 watcher，`ChangeDestinationFolder()` 刷新子文件夹列表
- `RefreshTargetFolders()` 扫描目标文件夹的直接子目录，直接填充 `TargetFolderOptions`
- 编译成功

### ComboBox 绑定修复（多次迭代）
- **问题**：DataTemplate 内的 ComboBox 无法通过 ElementName 绑定访问父级 ViewModel 的 `TargetFolderOptions`（编译绑定下 DataContext 类型为 object）
- **尝试的方案**：
  - `Loaded` 事件 → DataContext 尚未赋值（在 `App.axaml.cs` 中构造后才设置）
  - `DataContextChanged` 事件 → 时序仍不可靠
  - `DropDownOpened` 事件 → 过于滞后
  - `x:CompileBindings="False"` 反射绑定 → 卡顿且仍无法解析
- **最终方案**：
  - 规则行恢复绿色只读徽章（不在 DataTemplate 内放 ComboBox）
  - 添加表单 ComboBox 通过 `x:Name` + 代码后置 `DataContextChanged` 事件直接赋值 `ItemsSource`
  - Window 构造函数中 `DataContextChanged += OnDataContextChanged`，DataContext 就绪后 `AddFormComboBox.ItemsSource = vm.TargetFolderOptions`
- 编译成功

### 列表选择器尝试与回退（AutoCompleteBox / ComboBox / Flyout）
- 尝试将扩展名输入改为 AutoCompleteBox → Avalonia 不支持 `Text`/`IsEditable` 属性
- 尝试改用 ComboBox + ItemsSource → 下拉始终无机显示（怀疑是外层 `ClipToBounds` 裁剪或 Avalonia Popup 定位问题）
- 尝试 Button + Flyout + ListBox 模式 → Flyout 同样无显示
- **最终方案**：放弃列表选择器，回退到原始简洁设计 — TextBox 手动输入扩展名和目标文件夹名
- 恢复 30 条默认规则、恢复 Desktop 默认路径、规则行目标文件夹改为可编辑 TextBox
- 编译成功

### UI 文本与布局优化
- "目标文件夹" → "目标文件夹完整名称"（规则列表表头 + 添加表单 Watermark），提示用户输入完整文件夹名
- "整理规则" → "归类规则"（左侧面板标题）
- "桌面整理" → "发起归类"（按钮文本）
- "发起归类"按钮从工具栏移至左侧面板"归类规则"标题右侧，样式与操作日志撤回按钮一致（`#F3F4F6` 灰底 + `10,6` 内边距 + `6` 圆角）
- 编译成功

### 默认行为调整
- `Rule.Enabled` 默认为 `false`（ToggleSwitch 默认关闭）
- `DestinationFolder` 每次启动清空（不持久化），用户需每次手动设置目标文件夹
- 编译成功

### Step 11: 系统通知与 UI 打磨
- Services/NotificationService.cs — 零依赖跨平台系统通知（macOS osascript + Windows PowerShell Toast）
- 通知集成点：手动整理完成、单次撤回、批量撤回（自动模式逐文件不发通知避免打扰）
- "发起归类"按钮绑定 `OrganizeButtonText`（整理中...）+ `IsEnabled` 防重复点击
- 状态栏 2px 蓝色不定进度条（`IsIndeterminate`），组织过程中显示
- 规则列表空状态："暂无归类规则" + 提示文字
- 操作日志空状态："暂无操作记录" + 提示文字
- ViewModel 新增计算属性：`IsNotOrganizing`、`OrganizeButtonText`、`HasNoRules`、`HasNoLogs`
- `RefreshEmptyStates()` 辅助方法，在构造/加载/增删规则/组织/撤回后刷新
- 编译成功

### Step 11 迭代：UI 打磨与交互优化
- **模式守卫**：未选择手动/自动模式时"发起归类"按钮禁用，状态栏三层优先级提示（选文件夹 → 选模式 → 就绪）
- **规则排序**：启用的规则自动置顶，取消启用后掉到下方（`SortRules()` + `SaveRules()`）
- **手动添加规则自动启用**：`AddRule()` 中 `new Rule(ext, folder, enabled: true)`
- **规则行样式**：扩展名宽度 64px 与添加栏对齐，背景改为透明，目标文件夹 TextBox 背景透明
- **ToggleSwitch 清理**：添加 `OffContent=""` `OnContent=""` 去掉 Off/On 标签
- **删除按钮**：✕ 始终可见（`Foreground="#1A1A1A"`），`HorizontalContentAlignment` + `VerticalContentAlignment` 居中
- **表头居中**：`HorizontalAlignment="Center"` → `TextAlignment="Center"`
- **按钮配色统一**：两个"浏览"+"添加"按钮 `#3B82F6`(蓝) → `#F3F4F6`(灰)，与撤回按钮一致
- **添加栏背景**：`#40000000` → `#F3F4F6`，与源文件夹 TextBox 背景统一
- **窗口外观**：圆角 → 尖角（`CornerRadius="0"`），关闭毛玻璃（`TransparencyLevelHint=None`），去除外层圆角阴影
- 编译成功

### 国际化 (i18n) 支持
- Services/LocService.cs — 静态本地化服务，根据系统语言自动选择中/英文（`CultureInfo.CurrentUICulture`）
- 覆盖所有 UI 文本：工具栏、左侧面板、右侧面板、状态栏、通知、日志标签、错误消息
- AXAML 使用 `{x:Static loc:Loc.XXX}` 绑定，ViewModel/LogEntry/FileOrganizer 使用 `Loc.XXX()` 方法
- 系统语言检测：中文 → zh，其余 → en（英文）
- 编译成功

