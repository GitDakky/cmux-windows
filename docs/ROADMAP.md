# cmux-windows roadmap

Maintained at https://github.com/GitDakky/cmux-windows — see [CHANGELOG.md](../CHANGELOG.md).

## v1.0.8 (current — GitDakky)

- [x] Taskbar flash + toast notification preferences
- [x] `%USERPROFILE%\.cmux-windows\config.json` with legacy fallback
- [x] Pane/tab unread attention indicators
- [x] Agent idle detection (no-output heuristic; `NotificationSource.Idle`)
- [x] CLI named-pipe BOM + timeout fixes
- [x] Resize debounce for ConPTY
- [x] CI publish zip includes `cmux-daemon.exe` beside `cmuxw.exe`
- [x] Release workflow runs tests before publishing
- [x] Settings save failures logged to `%LOCALAPPDATA%\cmux-windows\settings.log`
- [x] Smoke test script + expanded unit tests

## v1.1 (next)

- [ ] Keyboard shortcut editor (persist `keyBindings` in config)
- [ ] `cmux hooks` installer for Claude Code / Codex (PowerShell templates)
- [ ] GitHub Release signing / Authenticode (optional)
- [ ] ConPTY integration test on CI (spawn `cmd /c echo`)

## v1.2+

- [ ] In-app browser script API (subset of macOS cmux)
- [ ] SSH remote workspace (large)
- [ ] Socket/CLI compatibility layer with macOS cmux command names
- [ ] GPU-backed terminal renderer evaluation (libghostty not planned short-term)

## Non-goals

- Electron rewrite
- Full macOS cmux feature parity in one release
- GPL hook/resume stack port
