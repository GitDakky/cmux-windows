# cmux for Windows (GitDakky)

A dark, keyboard-first terminal multiplexer for Windows, built with WPF and ConPTY. Optimised for AI coding agents (Claude Code, Codex, Cursor, and similar tools) with OSC notifications, idle attention, and a scriptable CLI.

**Repository:** https://github.com/GitDakky/cmux-windows  
**Latest release:** https://github.com/GitDakky/cmux-windows/releases/latest  
**Changelog:** [CHANGELOG.md](CHANGELOG.md)

---

## GitDakky distribution

This repo is the **maintained Windows build** under [GitDakky](https://github.com/GitDakky). It is no longer a throwaway fork: it ships CI-tested releases, documented config, and agent-notification workflows.

| | |
|---|---|
| **Clone / build from** | `https://github.com/GitDakky/cmux-windows.git` |
| **Download (recommended)** | [GitHub Releases](https://github.com/GitDakky/cmux-windows/releases) — `cmux-windows-v{version}-win-x64.zip` |
| **Lineage** | Forked from [mkurman/cmux-windows](https://github.com/mkurman/cmux-windows); conceptually aligned with macOS [cmux](https://github.com/manaflow-ai/cmux) |

**Current release: v1.0.8** — ConPTY multiplexer, pane/tab attention, OSC + idle + CLI notifications, taskbar flash, config at `%USERPROFILE%\.cmux-windows\config.json`, publish zip with `cmuxw.exe`, `cmux-daemon.exe`, and `cmux` CLI. Requires **Windows 10 (17763+)** or **Windows 11**. Build from source needs the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

| Doc | Purpose |
|-----|---------|
| [CHANGELOG.md](CHANGELOG.md) | Version history (GitDakky releases) |
| [docs/AUDIT-INITIAL.md](docs/AUDIT-INITIAL.md) | Initial codebase audit |
| [docs/MVP-PLAN.md](docs/MVP-PLAN.md) | MVP scope and completion status |
| [docs/SETUP.md](docs/SETUP.md) | SDK, build, publish |
| [docs/config.example.json](docs/config.example.json) | Example settings |
| [docs/KNOWN-LIMITATIONS.md](docs/KNOWN-LIMITATIONS.md) | Platform and parity limits |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Post-MVP roadmap |
| [docs/MANUAL-TEST-CHECKLIST.md](docs/MANUAL-TEST-CHECKLIST.md) | Interactive validation on Windows |

---

## Install (release build)

1. Open [Releases](https://github.com/GitDakky/cmux-windows/releases/latest) and download `cmux-windows-v1.0.8-win-x64.zip` (or the latest version).
2. Extract the zip. You will get two folders:
   - **`app\`** — `cmuxw.exe` (main app) and `cmux-daemon.exe` (session persistence)
   - **`cli\`** — `cmux.exe` (automation / hooks)
3. Run `app\cmuxw.exe`. Optionally add `cli\` to your `PATH` for `cmux notify` and workspace commands.
4. SmartScreen may warn on unsigned binaries — use **More info → Run anyway** if you trust this build.

No separate .NET runtime install is required (self-contained publish).

---

## Why / Who / What / How

| Why (problem) | Who (for) | What (feature) | How to use |
|---|---|---|---|
| You lose context across projects and shells | Developers juggling many repos/tasks | **Workspaces + surfaces (tabs)** | `Ctrl+N` new workspace, `Ctrl+T` new surface, switch with `Ctrl+1..9` |
| One terminal is never enough | CLI-heavy users, agent workflows | **Split panes** (right/down) | `Ctrl+D` split right, `Ctrl+Shift+D` split down, `Ctrl+Alt+Arrow` focus pane |
| You miss important agent outputs | AI-assisted coding users (Claude/Codex/etc.) | **OSC notifications + unread tracking** | `Ctrl+I` open notifications, `Ctrl+Shift+U` jump to latest unread |
| You need auditability of executed commands | Security-conscious / debugging workflows | **Command logs + history picker** | `Ctrl+Shift+L` logs, `Ctrl+Alt+H` command history, insert/run from UI |
| You want full session recall after crashes/restarts | Long-running sessions | **Session persistence + transcript capture** | Auto restore on startup + open **Session Vault** (`Ctrl+Shift+V`) |
| You want searchable output history like Termius vault | Anyone reviewing terminal sessions | **Session Vault browser** | Open vault, filter captures, preview transcript, copy/open file |
| You need dark theme consistency and personalization | Users who care about UX/readability | **Dark UI + terminal theme customization** | Settings (`Ctrl+,`) for colors/font/cursor + workspace accents |
| You want quick actions without mouse hunting | Keyboard-first power users | **Command palette + shortcuts** | `Ctrl+Shift+P` command palette, menu mirrors key flows |
| You need automation from scripts/tools | Integrators/agent hooks | **Named pipe CLI API** (`cmux`) | `cmux notify`, `cmux workspace`, `cmux split`, `cmux status` |

---

## Core capabilities

- Native **ConPTY terminal emulation** (real Windows terminal backend)
- Workspace sidebar with metadata (git branch, cwd, notifications)
- Multi-surface tabs and split-pane layout management
- Notification ingestion (OSC 9/99/777) for coding agents
- Idle attention when agent panes stop producing output (configurable)
- Command logs/history with filtering and quick replay
- Terminal transcript capture + Session Vault browsing
- Persistent sessions via `cmux-daemon.exe` (bundled in release zip)
- Dark desktop UI with keyboard-first navigation

---

## Screenshots

<details>
  <summary>Open screenshots</summary>

  <p><strong>Main workspace view</strong></p>
  <img src="assets/screenshots/1.jpg" alt="cmux main workspace" width="1000" />

  <p><strong>Snippets panel</strong></p>
  <img src="assets/screenshots/2.jpg" alt="cmux snippets panel" width="700" />

  <p><strong>Command logs window</strong></p>
  <img src="assets/screenshots/3.jpg" alt="cmux command logs" width="1000" />
</details>

---

## Build and run (Windows)

### Quick verify

```powershell
git clone https://github.com/GitDakky/cmux-windows.git
cd cmux-windows
.\scripts\smoke-test.ps1
# optional: also publish artefacts
.\scripts\smoke-test.ps1 -Publish
```

### Requirements

- Windows 10 build 17763+ or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (browser panes)
- Optional: Visual Studio 2022 / Build Tools

### Dev run

```powershell
dotnet build Cmux.sln -c Debug
dotnet run --project src/Cmux/Cmux.csproj -c Debug
```

### Publish (matches CI release layout)

```powershell
.\scripts\publish-win-x64.ps1
```

Outputs under `publish\cmux-win-x64\` (`cmuxw.exe`, `cmux-daemon.exe`) and `publish\cmux-cli-win-x64\` (`cmux.exe`), plus versioned zip `publish\cmux-windows-v{version}-win-x64.zip`.

---

## Build `.exe` variants (advanced)

### 1) Framework-dependent `.exe` (smallest output)

```powershell
dotnet publish src/Cmux/Cmux.csproj -c Release -r win-x64 --self-contained false -o publish/cmux-win-x64
```

Output: `publish/cmux-win-x64/cmuxw.exe` (requires .NET runtime on target machine).

### 2) Self-contained folder (same as release script)

Use `.\scripts\publish-win-x64.ps1` — includes daemon and CLI.

### 3) Single-file self-contained `.exe` (portable)

```powershell
.\scripts\publish-single-file.ps1
```

Output: `publish/cmux-win-x64-single/cmuxw.exe` (daemon not single-file; use full publish for persistence).

> WebView2-backed browser panes may require the [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) on the target system.

---

## First 5 minutes (how to use)

1. Launch `app\cmuxw.exe` (or dev build `cmuxw.exe`)
2. `Ctrl+N` to create a workspace for your repo
3. `Ctrl+T` to create additional surfaces (tabs)
4. Split panes with `Ctrl+D` / `Ctrl+Shift+D`
5. Open command palette with `Ctrl+Shift+P` for quick actions
6. Open logs with `Ctrl+Shift+L`
7. Open Session Vault with `Ctrl+Shift+V`
8. Open settings with `Ctrl+,` and tune terminal theme/font/cursor and notification behaviour

---

## Keyboard shortcuts

### Workspaces

| Shortcut | Action |
|---|---|
| `Ctrl+N` | New workspace |
| `Ctrl+1..8` | Jump to workspace 1..8 |
| `Ctrl+9` | Jump to last workspace |
| `Ctrl+Shift+W` | Close workspace |
| `Ctrl+Shift+R` | Rename workspace |
| `Ctrl+B` | Toggle sidebar |

### Surfaces (tabs)

| Shortcut | Action |
|---|---|
| `Ctrl+T` | New surface |
| `Ctrl+W` | Close surface |
| `Ctrl+Shift+]` | Next surface |
| `Ctrl+Shift+[` | Previous surface |
| `Ctrl+Tab` / `Ctrl+Shift+Tab` | Cycle surfaces |

### Panes

| Shortcut | Action |
|---|---|
| `Ctrl+D` | Split right |
| `Ctrl+Shift+D` | Split down |
| `Ctrl+Alt+Arrow` | Focus adjacent pane |
| `Ctrl+Shift+Z` | Zoom/unzoom pane |

### Productivity

| Shortcut | Action |
|---|---|
| `Ctrl+Shift+P` | Command palette |
| `Ctrl+Shift+F` | Search overlay |
| `Ctrl+Shift+L` | Command logs |
| `Ctrl+Shift+V` | Session vault |
| `Ctrl+Alt+H` | Command history picker |
| `Ctrl+,` | Settings |

---

## Configuration

Settings are stored as JSON:

| Path | Role |
|------|------|
| `%USERPROFILE%\.cmux-windows\config.json` | **Primary** (read/write after save) |
| `%LOCALAPPDATA%\cmux\settings.json` | Legacy (read-only fallback if user file missing) |

Example: [docs/config.example.json](docs/config.example.json). Open **Settings** (`Ctrl+,`) → **Behavior** for toast, taskbar flash, and idle detection.

---

## Agent notification setup

cmux-windows detects agent attention through:

1. **OSC sequences** (Claude Code, Codex, and others that emit OSC 9/99/777)
2. **`cmux notify`** CLI (named pipe to the running app)
3. **Idle heuristic** — no output for a configured interval (optional agent-only filter)
4. **Process heuristics** — child processes matching `claude`, `codex`, `cursor`, etc. (sidebar labels)

### Claude Code / Codex hooks (PowerShell)

```powershell
# Notify cmux when a script needs attention (cmux CLI on PATH)
cmux notify --title "Claude Code" --body "Waiting for your input"
```

Ensure the cmux app is running and `cmux.exe` from the release `cli\` folder is on `PATH`.

Optional installer for PowerShell hook helpers:

```powershell
.\scripts\install-agent-hooks.ps1
# then in hooks: Send-CmuxAgentNotify -Title "Claude Code" -Body "Waiting for input"
```

### In-app behaviour

- Unread badges on workspace sidebar, surfaces, and panes
- Notification panel (`Ctrl+I`)
- `Ctrl+Shift+U` — jump to latest unread
- Optional **Windows toast** and **taskbar flash** when cmux is in the background (Settings → Behavior)

---

## CLI usage

```powershell
cmux notify --title "Claude Code" --body "Waiting for input"
cmux workspace list
cmux workspace create --name "My Project"
cmux workspace select --index 0
cmux surface create
cmux split right
cmux split down
cmux status
```

Pipe endpoint: `\\.\pipe\cmux` (Windows named pipe; not compatible with macOS cmux Unix sockets).

---

## Architecture (high level)

```text
src/
  Cmux/         WPF desktop app (views, controls, themes)
  Cmux.Core/    terminal engine, models, services, persistence, IPC
  Cmux.Cli/     command-line client for automation
  Cmux.Daemon/  background session persistence (cmux-daemon.exe)
tests/
  Cmux.Tests/   unit tests
```

---

## Contributing

Issues and pull requests: https://github.com/GitDakky/cmux-windows/issues

For upstream comparison or merging fixes back to [mkurman/cmux-windows](https://github.com/mkurman/cmux-windows), see [docs/AUDIT-INITIAL.md](docs/AUDIT-INITIAL.md).

---

## License

MIT
