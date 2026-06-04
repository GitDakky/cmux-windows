# Known limitations

## Platform

- **Windows only** — WPF + ConPTY require Windows 10 build 17763+ or Windows 11.
- **.NET 10 SDK** required to build; published self-contained builds do not require a separate runtime install.

## Terminal emulation

- Custom VT renderer (not libghostty) — some TUI apps may render differently than Ghostty/iTerm.
- Resize is debounced (50 ms); extremely fast layout churn may briefly show misaligned cells until settle.
- Daemon-backed sessions fall back to local ConPTY if the background daemon fails to start.

## Notifications

- Idle detection uses **time since last output** and optional **agent process name** heuristics — not a substitute for OSC notifications from the agent itself.
- `AgentDetector` uses WMI (`Win32_Process`) — requires Windows and may miss renamed binaries.
- Toasts require Windows notification support; may be suppressed by Focus Assist.

## macOS cmux parity gaps

- No `cmux hooks setup` / agent resume commands yet
- No SSH workspace workflow
- No Sparkle auto-update (manual download or GitHub Releases)
- Named pipe CLI (`\\.\pipe\cmux`) — not compatible with macOS Unix socket API

## Optional components

- **WebView2** required for embedded browser panes
- Built-in **Agent chat** panel is off by default (`agent.enabled: false`); large optional subsystem
