# cmux-windows — Staged MVP plan

**GitDakky distribution:** https://github.com/GitDakky/cmux-windows  
**Changelog:** [../CHANGELOG.md](../CHANGELOG.md)

**Status: MVP complete** (automated CI green; manual checklist for Windows host validation).

---

## Stage 0 — Bootstrap

- [x] Fork and clone
- [x] Initial audit ([AUDIT-INITIAL.md](./AUDIT-INITIAL.md))
- [x] Pin SDK via `global.json`
- [x] CI workflow: zip artefact + `workflow_dispatch`
- [x] CI green on GitDakky `main`
- [ ] Manual run of [MANUAL-TEST-CHECKLIST.md](./MANUAL-TEST-CHECKLIST.md) on a Windows machine

---

## Stage 1 — MVP

### 1.1 Build & launch

- [x] `dotnet build` / `dotnet test` on Windows CI
- [x] Document .NET 10 + WebView2 ([SETUP.md](./SETUP.md))
- [x] Release warnings-as-errors passing on CI

### 1.2 Terminal correctness

- [x] Shell detection (PWsh, PowerShell, CMD, WSL, Git Bash)
- [x] Resize debounce → ConPTY resize coalescing
- [x] Scrollback, copy/paste, default shell settings (existing + docs)

### 1.3 Multiplexer UX

- [x] Workspaces, surfaces, splits (existing)
- [x] Pane/tab unread indicators

### 1.4 Notifications

- [x] OSC 9/99/777, `cmux notify`, toast, taskbar flash, config toggles
- [x] Agent idle detection

### 1.5 Configuration

- [x] `%USERPROFILE%\.cmux-windows\config.json` + legacy fallback
- [x] Example config

### 1.6 Packaging

- [x] `scripts/publish-win-x64.ps1`
- [x] `scripts/publish-single-file.ps1`
- [x] CI zip `cmux-windows-v{version}-win-x64.zip`
- [x] `.github/workflows/release.yml` on version tags

### 1.7 Documentation

- [x] README agent hooks, config, limitations
- [x] Manual test checklist
- [x] Upstream pipe fixes merged (BOM + CLI timeout)

### 1.8 Tests

- [x] Settings, notification, pane activity unit tests
- [x] `scripts/smoke-test.ps1`

---

## Post-MVP

See [ROADMAP.md](./ROADMAP.md) and [KNOWN-LIMITATIONS.md](./KNOWN-LIMITATIONS.md).
