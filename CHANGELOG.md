# Changelog

All notable changes to **[GitDakky/cmux-windows](https://github.com/GitDakky/cmux-windows)** are documented here.

This is the maintained Windows distribution of cmux. It was forked from [mkurman/cmux-windows](https://github.com/mkurman/cmux-windows) and extended for production use (notifications, packaging, CI, and agent workflows). Conceptual inspiration also comes from macOS [cmux](https://github.com/manaflow-ai/cmux).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.8] - 2026-06-05

### Added

- `NotificationSource.Idle` for heuristic idle attention (distinct from CLI/OSC).
- Shared `PipeEncoding` for named-pipe UTF-8 (no BOM).
- Settings save failure logging to `%LOCALAPPDATA%\cmux-windows\settings.log`.
- CI/release verification that `cmux-daemon.exe` is present beside `cmuxw.exe`.

### Changed

- **GitDakky** release zip now ships `cmux-daemon.exe` in the `app\` folder (session persistence daemon).
- Release workflow runs `dotnet test` before publishing assets.
- Idle detection marks non-agent and stale panes as handled so WMI is not polled every 10 seconds.

### Fixed

- Nullable `ProcessId` when calling `AgentDetector` for idle attention.
- Duplicate `Utf8NoBom` encoding constant on named-pipe client (build error on .NET 10).

## [1.0.7] - 2026-06-04

### Added

- Pane/tab unread attention indicators.
- Agent idle detection (no-output heuristic, optional agent-only filter).
- Taskbar flash and Windows toast preferences (Settings → Behavior).
- Config path `%USERPROFILE%\.cmux-windows\config.json` with legacy `%LOCALAPPDATA%\cmux\settings.json` fallback.
- GitHub Release workflow on `v*` tags.
- `scripts/publish-single-file.ps1`, `scripts/smoke-test.ps1`, `scripts/install-agent-hooks.ps1`.
- Unit tests: settings, notifications, pane activity.
- Docs: MVP plan, manual test checklist, roadmap, known limitations.

### Changed

- Terminal resize debounced (50 ms) before ConPTY resize.
- Named pipe server/client: UTF-8 without BOM, connect/read timeouts, pipe dispose on timeout.

### Fixed

- `WriteLineAsync` overload for .NET 10 named-pipe responses.

## [1.0.6] and earlier (upstream baseline)

Features present before the GitDakky MVP pass (from upstream lineage), including:

- ConPTY multiplexer, workspaces, surfaces, splits.
- Session persistence daemon (when `cmux-daemon.exe` is deployed).
- OSC notifications, command logs, Session Vault, shell detection.
- Per-pane shell selection, default shell application.

See [docs/AUDIT-INITIAL.md](docs/AUDIT-INITIAL.md) for the pre-fork audit snapshot.

## Links

| Version | Release |
|---------|---------|
| **Latest** | [v1.0.8](https://github.com/GitDakky/cmux-windows/releases/tag/v1.0.8) |
| Previous | [v1.0.7](https://github.com/GitDakky/cmux-windows/releases/tag/v1.0.7) |

[1.0.8]: https://github.com/GitDakky/cmux-windows/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/GitDakky/cmux-windows/compare/v1.0.6...v1.0.7
