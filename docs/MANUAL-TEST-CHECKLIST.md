# Manual smoke test checklist (Windows)

Run on Windows 10/11 after `dotnet build` or published `cmuxw.exe`.

## Launch and shells

- [ ] App launches without crash
- [ ] New workspace (`Ctrl+N`)
- [ ] PowerShell 7 tab — `Get-Location` works
- [ ] CMD tab — `echo ok`
- [ ] WSL tab (if installed) — `uname -a`
- [ ] Git Bash tab (if installed) — `pwd`

## Terminal behaviour

- [ ] Resize window — prompt/layout stay aligned
- [ ] Long output — scrollback works
- [ ] Select text and copy (`Ctrl+C` with selection)
- [ ] Paste (`Ctrl+V`)
- [ ] Clear scrollback (`Ctrl+K` if bound)

## Panes

- [ ] Split right (`Ctrl+D`)
- [ ] Split down (`Ctrl+Shift+D`)
- [ ] Focus adjacent pane (`Ctrl+Alt+Arrow`)
- [ ] Close pane / surface (`Ctrl+W`)

## Notifications

- [ ] OSC test in terminal: `Write-Host "`e]9;Test alert`a"` (PowerShell) shows unread badge
- [ ] `cmux notify --title "Test" --body "Hello"` (CLI on PATH) increments unread
- [ ] With cmux in background: Windows toast appears (if enabled in Settings → Behavior)
- [ ] Taskbar button flashes (if enabled)
- [ ] `Ctrl+Shift+U` jumps to latest unread

## Persistence and config

- [ ] Settings save to `%USERPROFILE%\.cmux-windows\config.json`
- [ ] Restart app — session layout restores (if enabled)
- [ ] Change default shell in Settings — new pane uses it

## Publish build

- [ ] `.\scripts\publish-win-x64.ps1` completes
- [ ] `publish\cmux-win-x64\cmuxw.exe` runs without installed .NET runtime
