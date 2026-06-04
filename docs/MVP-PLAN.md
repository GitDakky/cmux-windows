# cmux-windows — Staged MVP plan

Fork: https://github.com/GitDakky/cmux-windows

---

## Stage 0 — Bootstrap (current)

- [x] Fork and clone
- [x] Initial audit ([AUDIT-INITIAL.md](./AUDIT-INITIAL.md))
- [x] Pin SDK via `global.json`
- [x] CI workflow: zip artefact + `workflow_dispatch`
- [ ] CI green on fork (enable Actions on GitHub if needed)
- [ ] Windows dev machine: install .NET 10 SDK, build, run [MANUAL-TEST-CHECKLIST.md](./MANUAL-TEST-CHECKLIST.md)

---

## Stage 1 — MVP (must ship)

Goal: **Reliable daily-driver terminal multiplexer** for AI agent workflows on Windows.

### 1.1 Build & launch

- Clean `dotnet build` / `dotnet test` on Windows CI
- Document .NET 10 + WebView2 runtime prerequisites
- Fix any Release warnings-as-errors regressions

### 1.2 Terminal correctness

- Verify ConPTY: PowerShell 7, Windows PowerShell, CMD, WSL, Git Bash
- Resize window and panes — no corrupted buffer or stuck cursor
- Scrollback to configured limit
- Copy/paste (selection + bracketed paste setting)
- Apply `DefaultShell` / shell profiles on **new** sessions (upstream fix merged in main)

### 1.3 Multiplexer UX

- Workspace + surface tabs stable (create/close/switch)
- Split right/down, focus adjacent pane, close pane
- Keyboard shortcuts match README (audit gaps vs Settings keybinding overrides)

### 1.4 Notifications (agent attention)

- [x] OSC 9/99/777 → unread state + notification panel
- [x] `cmux notify` from hooks
- [x] Windows toast when app unfocused (setting)
- [x] Taskbar flash optional (Win32 `FlashWindow`)
- [x] Config toggles: toast on/off, flash on/off, focus-only

### 1.5 Configuration

- [x] Primary path `%USERPROFILE%\.cmux-windows\config.json` (legacy fallback)
- [x] Example `docs/config.example.json`

### 1.6 Packaging

- Script: `scripts/publish-win-x64.ps1` → `publish/cmux-win-x64/`
- Single-file optional artefact
- CI: zip `cmux-windows-v{version}-win-x64.zip` on tag or release workflow

### 1.7 Documentation

- README: current status, limitations, roadmap, agent hook examples (Claude Code / Codex)
- Manual test checklist
- Merge upstream pipe fixes if still open

### 1.8 Tests

- Keep expanding **Core** unit tests (config load/save round-trip, settings defaults)
- Windows-only integration test job (optional): spawn `cmd /c echo` via ConPTY

---

## Stage 2 — Post-MVP

| Item | Priority |
|------|----------|
| Merge CLI pipe timeout + BOM fixes | High |
| Config path alias `.cmux-windows` | Medium |
| Agent idle heuristic (no OSC) | Medium |
| Customizable keybindings in UI | Medium |
| Signed release + GitHub Releases | Medium |
| Browser pane parity (basic automation) | Low |
| SSH workspace | Low (large) |
| macOS cmux socket command subset | Low |

---

## Stage 3 — Non-goals (for now)

- Rewriting in Electron or Rust
- Full libghostty port
- GPL macOS cmux hook/resume parity in v1

---

## MVP acceptance checklist

Manual (Windows 11 VM or host):

1. Launch `cmuxw.exe`
2. New workspace → PowerShell tab → run `Get-Location`
3. New surface → CMD → `echo ok`
4. WSL tab if installed
5. Split right + down, focus with `Ctrl+Alt+Arrows`
6. Resize window — prompt aligns
7. Long output — scrollback works
8. Select/copy, paste
9. Trigger `cmux notify` — badge + toast
10. `dotnet publish` win-x64 self-contained — run published exe

Automated:

- `dotnet test` passes on CI
- Publish job produces artefact

---

## Implementation order (matches user priority)

A → B → C → D → E → F → G → H → I → J (build, crashes, ConPTY, tabs/panes, shells, notifications, shortcuts, packaging, tests, docs)
