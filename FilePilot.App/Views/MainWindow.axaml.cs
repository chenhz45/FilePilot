using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FilePilot.App.Models;
using FilePilot.App.ViewModels;

namespace FilePilot.App.Views;

public partial class MainWindow : Window
{
    private Rule? _editingRule;

    public MainWindow()
    {
        InitializeComponent();

        TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
    }

    private void OnDeleteRule(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Rule rule)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.DeleteRule(rule);
            }
        }
    }

    private void OnEditTargetFolder(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not Rule rule) return;

        _editingRule = rule;
        EditTargetFolderBox.Text = rule.TargetFolder;
        EditOverlay.IsVisible = true;

        // Force IME re-init after the TextBox is visible and in the tree
        InputMethod.SetIsInputMethodEnabled(EditTargetFolderBox, false);
        InputMethod.SetIsInputMethodEnabled(EditTargetFolderBox, true);

        EditTargetFolderBox.Focus();
        EditTargetFolderBox.SelectAll();
    }

    private void OnEditBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OnEditOk(sender, e);
        else if (e.Key == Key.Escape)
            OnEditCancel(sender, e);
    }

    private void OnEditOk(object? sender, RoutedEventArgs e)
    {
        if (_editingRule != null)
        {
            _editingRule.TargetFolder = EditTargetFolderBox.Text ?? "";
        }
        EditOverlay.IsVisible = false;
        _editingRule = null;
    }

    private void OnEditCancel(object? sender, RoutedEventArgs e)
    {
        EditOverlay.IsVisible = false;
        _editingRule = null;
    }

    private void OnToggleUndoSelection(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is LogEntry entry)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleUndoSelection(entry);
            }
        }
    }

    private async void OnBrowseSourceFolder(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择源文件夹（需要整理的文件所在位置）",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            vm.ChangeSourceFolder(folders[0].Path.LocalPath);
        }
    }

    private async void OnBrowseDestinationFolder(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择目标文件夹（文件分类后去向的基础路径）",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            vm.ChangeDestinationFolder(folders[0].Path.LocalPath);
        }
    }
}
