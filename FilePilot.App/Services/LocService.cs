using System;
using System.Globalization;

namespace FilePilot.App.Services;

public static class Loc
{
    private static readonly bool IsZh =
        CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

    // ── Toolbar ──
    public static string ManualMode => IsZh ? "手动模式" : "Manual Mode";
    public static string AutoMode => IsZh ? "自动模式" : "Auto Mode";

    // ── Left Panel: Title ──
    public static string RulePanelTitle => IsZh ? "归类规则" : "Rules";
    public static string Organize => IsZh ? "发起归类" : "Organize";
    public static string Organizing => IsZh ? "整理中..." : "Organizing...";

    // ── Left Panel: Folders ──
    public static string SourceFolder => IsZh ? "源文件夹" : "From Folder";
    public static string DestinationFolder => IsZh ? "目标文件夹" : "To Folder";
    public static string Browse => IsZh ? "浏览" : "Browse";

    // ── Left Panel: Rules Table Header ──
    public static string ExtensionHeader => IsZh ? "扩展名" : "Extension";
    public static string TargetFolderHeader => IsZh ? "目标文件夹完整名称" : "Target Folder";
    public static string EnableHeader => IsZh ? "启用" : "Enable";

    // ── Left Panel: Add Rule Form ──
    public static string ExtWatermark => IsZh ? ".ext" : ".ext";
    public static string TargetFolderWatermark => IsZh ? "目标文件夹完整名称" : "Target folder name";
    public static string EditTargetFolderTitle => IsZh ? "输入目标文件夹完整名称" : "Enter target folder name";
    public static string AddRule => IsZh ? "添加" : "Add";

    // ── Left Panel: Empty State ──
    public static string NoRules => IsZh ? "暂无归类规则" : "No rules yet";
    public static string NoRulesHint => IsZh
        ? "请在下方添加扩展名与目标文件夹的对应规则"
        : "Add extension and target folder rules below";

    // ── Right Panel ──
    public static string LogPanelTitle => IsZh ? "操作日志" : "Operation Log";
    public static string UndoLastLabel => IsZh ? "撤回最近一次归类" : "Undo Last";
    public static string UndoMultipleLabel => IsZh ? "撤回多次归类" : "Undo Multiple";
    public static string SelectAll => IsZh ? "全选" : "Select All";
    public static string UndoSelected => IsZh ? "撤回选中" : "Undo Selected";
    public static string Cancel => IsZh ? "取消" : "Cancel";
    public static string NoLogs => IsZh ? "暂无操作记录" : "No records yet";
    public static string NoLogsHint => IsZh
        ? "执行归类后，操作记录将显示在这里"
        : "Operation records will appear here after organizing";

    // ── Edit Overlay ──
    public static string OkButton => IsZh ? "确定" : "OK";

    // ── Confirm Dialog ──
    public static string Confirm => IsZh ? "同意" : "Confirm";
    public static string AutoModeConfirmMsg => IsZh
        ? "开启实时模式后，会执行一次桌面整理"
        : "Enabling auto mode will organize files in the source folder once";

    // ── Status / Messages ──
    public static string SelectFoldersFirst => IsZh
        ? "请先选择源文件夹和目标文件夹"
        : "Please select source and destination folders first";
    public static string SelectModeFirst => IsZh
        ? "请选择手动模式或自动模式后方可进行归类"
        : "Select a mode to start organizing";
    public static string NoUndoAvailable => IsZh ? "没有可撤回的操作" : "No operations to undo";
    public static string UndoFailed => IsZh ? "撤回失败" : "Undo failed";
    public static string UndoLastSuccess => IsZh ? "已撤回最近一次归类" : "Last organize undone";
    public static string SelectUndoOps => IsZh ? "请选择要撤回的归类操作" : "Select operations to undo";
    public static string MultiUndoCancelled => IsZh ? "已取消多次撤回" : "Multi-undo cancelled";
    public static string NoUndoSelected => IsZh ? "未选择任何可撤回的操作" : "No operations selected";
    public static string ScanningFiles => IsZh ? "正在扫描文件..." : "Scanning files...";
    public static string NoFilesToOrganize => IsZh ? "未找到需要整理的文件" : "No files to organize";
    public static string ExtensionRequired => IsZh ? "请输入扩展名" : "Please enter an extension";
    public static string TargetFolderRequired => IsZh ? "请输入目标文件夹" : "Please enter a target folder";
    public static string RuleExists(string ext) => IsZh ? $"规则 {ext} 已存在" : $"Rule {ext} already exists";
    public static string RuleAdded(string ext, string folder) => IsZh ? $"已添加规则: {ext} → {folder}" : $"Rule added: {ext} → {folder}";
    public static string RuleDeleted(string ext, string folder) => IsZh ? $"已删除规则: {ext} → {folder}" : $"Rule deleted: {ext} → {folder}";
    public static string SourceFolderChanged(string path) => IsZh ? $"源文件夹已更改为: {path}" : $"Source folder changed to: {path}";
    public static string DestFolderChanged(string path) => IsZh ? $"目标文件夹已更改为: {path}" : $"Destination folder changed to: {path}";
    public static string AutoModeLog => IsZh ? "当前模式：自动模式" : "Mode: Auto";
    public static string ManualModeLog => IsZh ? "当前模式：手动模式" : "Mode: Manual";
    public static string SwitchingToAuto => IsZh ? "已切换到自动模式，正在整理现有文件..." : "Switched to auto mode, organizing...";
    public static string RulesLoaded(int count) => IsZh ? $"已加载 {count} 条规则" : $"{count} rules loaded";
    public static string OrganizeDone(int moved, int skipped, int undoCount) => IsZh
        ? $"整理完成: 移动 {moved} 个, 跳过 {skipped} 个 | 可撤回 {undoCount} 次"
        : $"Done: {moved} moved, {skipped} skipped | {undoCount} undoable";
    public static string FileMoved(string name) => IsZh ? $"已移动: {name}" : $"Moved: {name}";
    public static string FileSkipped(string name, string? err) => IsZh ? $"跳过: {name} — {err}" : $"Skipped: {name} — {err}";
    public static string AutoOrganized(string name) => IsZh ? $"已自动整理: {name}" : $"Auto-organized: {name}";
    public static string UndoCount(int count) => IsZh ? $"可撤回 {count} 次" : $"{count} undoable";
    public static string UndoMultipleSuccess(int count) => IsZh ? $"已撤回选中的 {count} 次归类" : $"Undone {count} operations";
    public static string UndoFailedWithError(string? err) => IsZh ? $"撤回失败: {err}" : $"Undo failed: {err}";
    public static string OrganizeError(string msg) => IsZh ? $"整理出错: {msg}" : $"Organize error: {msg}";

    // ── Notifications ──
    public static string NotifUndoTitle => IsZh ? "FilePilot 撤回" : "FilePilot Undo";
    public static string NotifUndoLastBody => IsZh ? "已撤回最近一次归类" : "Last organize undone";
    public static string NotifUndoMultiBody(int n) => IsZh ? $"已撤回 {n} 次归类" : $"{n} operations undone";
    public static string NotifOrganizeTitle => IsZh ? "FilePilot 整理完成" : "FilePilot Organize";
    public static string NotifOrganizeBody(int moved, int skipped) => IsZh
        ? $"成功移动 {moved} 个文件" + (skipped > 0 ? $"，跳过 {skipped} 个" : "")
        : $"Successfully moved {moved} files" + (skipped > 0 ? $", {skipped} skipped" : "");

    // ── Log Entry Labels ──
    public static string LogLabelSuccess => IsZh ? "成功归类" : "Organized";
    public static string LogLabelSkipped => IsZh ? "已跳过" : "Skipped";

    // ── FileOrganizer ──
    public static string SourceFolderNotExist => IsZh ? "源文件夹不存在" : "Source folder does not exist";
    public static string FileNotInRoot => IsZh ? "文件不在源文件夹根目录" : "File not in source folder root";
    public static string NoMatchingRule(string ext) => IsZh ? $"没有匹配的规则: {ext}" : $"No matching rule: {ext}";
    public static string ArchiveDir => IsZh ? "归档" : "Archive";
}
