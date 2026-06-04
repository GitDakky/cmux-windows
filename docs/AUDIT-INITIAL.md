# cmux-windows — Initial audit (2026-06-05)

**Fork:** https://github.com/GitDakky/cmux-windows  
**Upstream:** https://github.com/mkurman/cmux-windows (v1.0.6 in `Cmux.csproj`)  
**Audit environment:** macOS (darwin); WPF/ConPTY builds require Windows + .NET 10 SDK.

---

## Executive summary

`cmux-windows` is a **mature, feature-rich** WPF terminal multiplexer—not a stub. It already implements most CMUX-style workflows: vertical workspace sidebar, horizontal surface tabs, split panes, OSC-based agent notifications, named-pipe CLI, session persistence, command logs, session vault, and substantial terminal emulation (VT parser, scrollback, OSC 133).

Upstream **CI on `main` is green** (Windows, .NET 10). Local audit machine had **.NET 8 only** and **cannot compile WPF** on macOS—see [SETUP.md](./SETUP.md).

**Recommendation:** Stabilise and document rather than rewrite. Gaps vs official macOS CMUX are mainly **Ghostty rendering**, **in-app browser depth**, **SSH workspaces**, **agent hooks/resume**, **taskbar flash**, and **release zip naming**—not core multiplexer mechanics.

---

## 1. Project structure

| Path | Role |
|------|------|
| `src/Cmux/` | WPF host (`cmuxw.exe`), views, controls, MVVM, toast helper |
| `src/Cmux.Core/` | ConPTY, VT/OSC, models, persistence, IPC, services |
| `src/Cmux.Cli/` | `cmux` CLI over named pipes |
| `src/Cmux.Daemon/` | Background session/daemon (pipe server, session manager) |
| `tests/Cmux.Tests/` | xUnit + FluentAssertions (terminal core, split tree, OSC) |
| `.github/workflows/ci.yml` | Build (Debug/Release), test, publish artifacts on `main` |
| `assets/` | Icon, screenshots |

**Solution:** `Cmux.sln` — 5 projects, shared `Directory.Build.props` (nullable, warnings as errors, LangVersion 14).

**Target:** `net10.0-windows10.0.17763.0` (app), `net10.0-windows` (core). **Requires .NET 10 SDK.**

---

## 2. WPF application architecture

- **Entry:** `App.xaml.cs` — loads settings, wires `NotificationService` → `ToastNotificationHelper`, named pipe server for CLI.
- **Shell:** `MainWindow.xaml` — chromeless window (`WindowChrome`), transparent background, custom title bar; hosts sidebar + surface tab bar + `SplitPaneContainer`.
- **MVVM:** `MainViewModel`, `WorkspaceViewModel`, `SurfaceViewModel` — workspace/surface/pane lifecycle, IPC command dispatch on UI thread.
- **Controls:** `TerminalControl` (rendering + input), `SplitPaneContainer`, `CommandPalette`, `NotificationPanel`, `SurfaceTabBar`, `WorkspaceSidebarItem`, optional `BrowserControl` (WebView2).
- **Extra UI:** Settings, Session Vault, History, Logs, Color picker, Snippet picker.
- **Heavy service:** `AgentRuntimeService.cs` (~2.7k+ lines) — built-in chat agent, MCP, tools (optional; `AgentSettings.Enabled` defaults **false**).

**Observation:** App scope exceeds “terminal multiplexer only”; agent runtime is optional but increases maintenance surface.

---

## 3. ConPTY terminal implementation

| Component | Purpose |
|-----------|---------|
| `ConPtyInterop.cs` | P/Invoke `CreatePseudoConsole`, resize, close |
| `PseudoConsole.cs` | Pipe pair + ConPTY handle lifecycle |
| `TerminalProcess.cs` | `CreateProcess` with `PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE` |
| `TerminalSession.cs` | Orchestrates read loop, resize, shell process |
| `VtParser.cs` | CSI/ESC/OSC parsing |
| `TerminalBuffer.cs` | Screen + alternate buffer, scroll regions |
| `ScrollbackBuffer.cs` | Configurable line cap (`ScrollbackLines`, default 10k) |
| `OscHandler.cs` | Title, cwd (OSC 7), notifications (9/99/777), shell integration (133) |
| `TerminalSelection.cs` | Selection + copy text extraction |
| `UrlDetector.cs` | Ctrl+click URLs |

**Strengths:** Real ConPTY (not mock terminal); tests cover parser, buffer resize, OSC notifications, split tree.

**Risks (needs Windows validation):**

- Resize path: `ResizePseudoConsole` + WPF layout must stay in sync on drag-resize.
- Unicode/emoji/colour fidelity vs Ghostty (custom renderer, not GPU libghostty).
- `AgentDetector` uses **WMI** (`Win32_Process`) — Windows-only, best-effort child process names.

---

## 4. Pane / tab / session model

- **Workspace** — sidebar item; git branch, cwd, notification badge text.
- **Surface** — tab within workspace (like cmux “surface”).
- **Pane** — leaf in `SplitNode` tree (`Vertical` = split right, `Horizontal` = split down per README).
- **Persistence:** `%LOCALAPPDATA%\cmux\session.json` — window geometry, workspace/surface/pane layout, cwd; auto-save interval configurable.
- **Transcripts:** capture on close/clear → Session Vault.
- **Daemon:** `Cmux.Daemon` for extended session management (separate executable; verify deployment story in MVP).

---

## 5. CLI and automation

**`Cmux.Cli`** — named pipe client (`DaemonClient` / `NamedPipeClient`):

- `cmux notify --title … --body …`
- `cmux workspace list|create|select|next|prev`
- `cmux surface create|next|prev`
- `cmux split right|down`
- `cmux status`

**Server:** `MainViewModel.HandlePipeCommand` — NOTIFY, WORKSPACE.*, SURFACE.*, SPLIT.*, PANE.* (list/focus/write/read).

**Gap vs macOS cmux:** No documented Unix socket parity; no `cmux hooks setup`, `cmux ssh`, browser automation API, or `cmux.json` project commands. Pipe protocol is custom JSON-ish, not necessarily compatible with macOS cmux CLI.

**Known upstream fixes (open PRs on fork parent):** CLI hang on unresponsive app; named-pipe UTF-8 BOM + `FlushFileBuffers` deadlock — worth merging when stabilising.

---

## 6. Configuration

| Item | Location |
|------|----------|
| Settings | `%LOCALAPPDATA%\cmux\settings.json` (camelCase JSON) |
| Session | `%LOCALAPPDATA%\cmux\session.json` |
| Secrets | `SecretStoreService` (DPAPI-related package) |

**`CmuxSettings` includes:** font, theme, scrollback, default shell/args, shell profiles, keybindings map, agent block, retention, notification-related prefs via `AgentSettings` / UI.

**User task asked for `%USERPROFILE%\.cmux-windows\config.json`:** **Not implemented** — uses `%LOCALAPPDATA%\cmux\` instead. MVP should either document current path or add alias/symlink support.

**Ghostty:** `GhosttyConfigReader.cs` — reads Ghostty config for theme import (alignment with reference Ghostty workflow).

---

## 7. Notifications and agent attention

| Mechanism | Status |
|-----------|--------|
| OSC 9 / 99 / 777 | Implemented (`OscHandler` + tests) |
| `cmux notify` CLI | Implemented |
| In-app notification panel | `NotificationPanel`, unread counts |
| Windows toast | `ToastNotificationHelper` (Toolkit.Uwp.Notifications) |
| Tab/pane visual attention | Sidebar/tab metadata (verify “blue ring” parity on Windows) |
| Taskbar flash | **Not found** in codebase |
| Process-based agent detect | `AgentDetector` — Claude, Codex, Aider, Copilot, Cursor, Cline, Windsurf |
| Idle/prompt detection | OSC 133 markers; no full idle-heuristic audit |
| Built-in agent chat | Large optional subsystem |

**vs macOS cmux:** Core OSC + CLI path matches intent. Missing: hook installer, resume commands, richer notification UX (rings), taskbar flash.

**vs wmux:** wmux is Electron+xterm.js with passive Claude observation + CDP browser; different stack, stronger “visibility” marketing—not comparable implementation.

---

## 8. Packaging and release

- **CI publish** (on `main`): self-contained `win-x64` app + CLI to GitHub Actions artifacts (`cmux-windows-x64`, `cmux-cli-windows-x64`).
- **README** documents framework-dependent, self-contained, and single-file publish commands.
- **Gaps:** No versioned `cmux-windows-vX.Y.Z-win-x64.zip` release asset in workflow; no combined zip of app+CLI; releases may be manual on upstream.

---

## 9. Test coverage

**`Cmux.Tests`:** Strong coverage of **terminal core** (VT, buffer, OSC, selection, URL, split tree, agent conversation JSONL parsing). **No WPF UI tests** (expected). **No ConPTY integration tests** on CI (would need Windows-specific harness).

**Manual checklist:** Not in repo yet — add under `docs/MANUAL-TEST-CHECKLIST.md` (MVP deliverable).

---

## 10. Known broken / incomplete areas

| Area | Notes |
|------|------|
| `ValueConverters.cs` | Several `ConvertBack` throw `NotImplementedException` (WPF one-way converters—OK if never two-way) |
| macOS/Linux dev | Cannot build/run WPF app |
| Config path | Differs from task spec (`.cmux-windows`) |
| Taskbar flash | Missing |
| macOS cmux feature parity | SSH, browser script API, hooks, Sparkle updates, GPU Ghostty |
| CLI compatibility | Windows pipe ≠ macOS socket |
| Open upstream PRs | Pipe timeout, BOM deadlock |
| Agent runtime | Off by default; large codepath if enabled |

---

## 11. Comparison matrix (cmux / wmux / cmux-windows)

| Feature | manaflow-ai/cmux | wmux | cmux-windows |
|---------|------------------|------|----------------|
| Stack | Swift + libghostty | Electron + xterm.js | C# WPF + ConPTY |
| Vertical workspace tabs | Yes | Yes (sidebar) | Yes |
| Surface tabs | Yes | Yes | Yes |
| Split panes | Yes | Yes | Yes |
| OSC agent notifications | Yes | Hooks/observation | Yes (9/99/777) |
| `cmux notify` | Yes | N/A | Yes |
| In-app browser | Yes (scriptable) | Yes (CDP) | WebView2 control (depth TBD) |
| Ghostty config | Yes | No | Partial reader |
| SSH workspace | Yes | No | No |
| Session restore + agent resume | Yes + hooks | Limited | Layout + cwd; no `cmux hooks` |
| Windows toast | N/A | N/A | Yes |
| Taskbar flash | N/A | N/A | No |
| GPU terminal | Yes | xterm.js | Custom VT renderer |
| License | GPL | AGPL (wmux) | MIT |

---

## 12. Build verification (this audit)

| Step | Result |
|------|--------|
| Fork `mkurman/cmux-windows` | Done → GitDakky/cmux-windows |
| Clone to workspace | Done |
| `dotnet build` on macOS (.NET 8) | **FAIL** — NETSDK1045 needs .NET 10 |
| Run WPF locally | **N/A** on macOS |
| Upstream CI `main` | **SUCCESS** (2026-03-31) |

**Next verification:** Push commit to fork; confirm GitHub Actions build+test on `windows-latest` with .NET 10.

---

## 13. Assumptions

1. Production target remains **Windows 10 17763+** / Windows 11.
2. .NET 10 is acceptable (README + csproj); LTS downgrade is optional if enterprises require .NET 8.
3. Config location can stay `%LOCALAPPDATA%\cmux` if documented clearly.
4. “CMUX parity” means **workflow** parity, not binary/API compatibility with macOS cmux.

---

*Next document: [MVP-PLAN.md](./MVP-PLAN.md)*
