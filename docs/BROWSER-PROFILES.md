# Browser profiles

cmux-windows supports **browser surfaces** alongside terminal surfaces. Each browser surface uses a named **profile** that controls sandboxing, persistence, and whether pages render inside WebView2 or an external browser.

## Profile kinds

| Kind | Behaviour |
|------|-----------|
| `embeddedIsolated` | WebView2 with a dedicated user-data folder per workspace/surface (agent sandbox). |
| `embeddedPersistent` | WebView2 with a shared user-data folder for the profile id (project logins/cookies). |
| `external` | Launches the system default browser or a configured executable; no embedded script API. |

User-data folders default to:

`%USERPROFILE%\.cmux-windows\browser-profiles\`

## Configuration

Profiles live in `%USERPROFILE%\.cmux-windows\config.json`:

```json
{
  "browser": {
    "defaultProfileId": "isolated-default",
    "profiles": [
      {
        "id": "isolated-default",
        "name": "Isolated (agent sandbox)",
        "kind": "embeddedIsolated",
        "scopeToSurface": true,
        "startUrl": "about:blank",
        "isDefault": true
      }
    ]
  }
}
```

Choose the default profile in **Settings → Browser**.

## UI

- **Surface → New Browser Surface** (command palette: “New Browser Surface”)
- Browser surfaces show the WebView2 toolbar (back/forward/reload/address bar)
- Terminal split toolbar is hidden on browser surfaces

## CLI / IPC

```bash
cmux surface create --kind browser --startUrl https://example.com
cmux browser profiles
cmux browser navigate --url https://example.com
cmux browser url
cmux browser eval --script "document.title"
cmux browser snapshot
cmux browser click --selector "#submit"
cmux browser fill --selector "#q" --value "cmux"
```

Pipe commands: `BROWSER.CREATE`, `BROWSER.NAVIGATE`, `BROWSER.EVAL`, `BROWSER.URL`, `BROWSER.SNAPSHOT`, `BROWSER.CLICK`, `BROWSER.FILL`, `BROWSER.PROFILES`.

Optional targeting args: `workspaceId`, `surfaceId`, `surfaceName`.

## Session persistence

Browser surfaces are restored from `%LOCALAPPDATA%\cmux\session.json` with `kind`, `browserProfileId`, and last URL fields.

## Agent notes

- Embedded profiles expose navigate/eval/snapshot/click/fill over named pipes.
- External profiles open URLs outside cmux; IPC script commands return an error.
- Isolated profiles prevent cookie/session leakage between agent runs on different surfaces.
