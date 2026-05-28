# FilePilot

A cross-platform desktop file organization tool that automatically sorts files by extension-based rules.

## Features

- **Rule-based organization** — Define "extension → target folder" mappings and organize with one click
- **Two modes** — Manual mode (organize on demand) / Auto mode (real-time file watching)
- **Operation log** — Tracks every file move with single and batch undo support
- **Cross-platform** — Native experience on macOS and Windows, auto-detects system language (English/Chinese)
- **System notifications** — Native notifications on organize completion or undo
- **Safe by design** — Move only, never delete; every operation is undoable

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8.0 |
| UI Framework | Avalonia UI 11.2 |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| Config | System.Text.Json (`~/.filepilot/config.json`) |
| File Watching | System.IO.FileSystemWatcher |
| Notifications | macOS osascript / Windows PowerShell Toast |

## Project Structure

```
FilePilot/
├── FilePilot.App/
│   ├── Models/           # Rule, LogEntry, AppConfig
│   ├── ViewModels/       # MainWindowViewModel, ViewModelBase
│   ├── Views/            # MainWindow (AXAML + code-behind)
│   ├── Services/         # ConfigService, FileOrganizer, FileWatcherService,
│   │                     # UndoService, ConflictResolver, NotificationService, LocService
│   ├── App.axaml
│   └── Program.cs
├── docs/                 # Requirements, technical design, execution steps
├── dev-logs/             # Development logs
└── FilePilot.sln
```

## Build & Run

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project FilePilot.App
```

## Publishing

### Single-file .exe (Windows)

```bash
dotnet publish FilePilot.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:DebugType=none
```

### Single-file app (macOS)

```bash
dotnet publish FilePilot.App -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -p:DebugType=none
```

Outputs a single executable — no .NET Runtime required on the target machine.

## Configuration

Runtime config is stored at `~/.filepilot/config.json`, containing rule lists, source/destination folder paths, and run mode. A default config is auto-generated on first launch.
