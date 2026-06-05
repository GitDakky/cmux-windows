# Development setup

## Repository (GitDakky)

- **Maintained repo:** https://github.com/GitDakky/cmux-windows
- **Releases:** https://github.com/GitDakky/cmux-windows/releases
- **Upstream lineage:** https://github.com/mkurman/cmux-windows

```powershell
git clone https://github.com/GitDakky/cmux-windows.git
cd cmux-windows
```

## Requirements (Windows)

| Requirement | Notes |
|-------------|--------|
| Windows 10 build 17763+ or Windows 11 | ConPTY minimum |
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | Required by all projects; `global.json` pins 10.0.100+ |
| WebView2 Runtime | For in-app browser panes (usually preinstalled on Win11) |
| Visual Studio 2022 or Build Tools | Optional; CLI build is sufficient |

## macOS / Linux (audit only)

- **WPF and ConPTY do not build** on non-Windows OS.
- Use **GitHub Actions** (`CI` workflow) or a Windows VM for compile/test.
- Core logic tests could run on Windows CI only today because `Cmux.Core` targets `net10.0-windows`.

## Install .NET 10 SDK

**Windows (winget):**

```powershell
winget install Microsoft.DotNet.SDK.10
```

**macOS (audit machine — for restore only, not WPF build):**

```bash
# Example; WPF project still won't compile on macOS
brew install --cask dotnet-sdk@10
```

Verify:

```powershell
dotnet --version
# Expect 10.0.x
```

## Build

```powershell
dotnet restore Cmux.sln
dotnet build Cmux.sln -c Debug
dotnet test Cmux.sln -c Debug --no-build
```

## Run (dev)

```powershell
dotnet run --project src/Cmux/Cmux.csproj -c Debug
```

## Publish (release artefact)

```powershell
.\scripts\publish-win-x64.ps1
```

Output: `publish/cmux-win-x64/cmuxw.exe` (self-contained).

## Updates

cmux checks **[GitHub Releases](https://github.com/GitDakky/cmux-windows/releases)** for a newer `cmux-windows-v*-win-x64.zip` (enabled by default on startup, configurable under **Settings → Behavior**).

When an update is available, a dialog offers **Update now** or **Remind me later**. Installing downloads the official zip, stages it under `%LOCALAPPDATA%\cmux-windows\updates\`, and applies it after you confirm restart — cmux closes briefly, replaces `app\` and `cli\` files, then relaunches.

Manual check: **Settings → About → Check for updates now**.

## Setup issues logged (2026-06-05 audit)

| Issue | Mitigation |
|-------|------------|
| Only .NET 8 installed | Install .NET 10 SDK |
| Building on macOS | Use Windows CI or VM |
| Config path `%LOCALAPPDATA%\cmux` vs `.cmux-windows` | Documented in audit; alias planned MVP |
| WebView2 missing on older Win10 | Install [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) |
