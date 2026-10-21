# Document Viewer

Windows desktop app (WPF, .NET 10) that finds documents in the EBA API and shows their PDFs. It is started by Pantheon with search arguments.

## Run

```
DocumentViewer.exe --endpoint https://eba.example.com --token <token> --acKey 12345
```

The app is published self-contained, so no .NET installation is needed to run it.
Only one instance runs. A later launch passes its arguments to the running window and exits.
Exit code `1` means the start failed (invalid arguments or connection data).

## Arguments

Every argument is `--name value`, all are optional.

| Argument | Meaning |
|---|---|
| `--endpoint` | EBA address, must be `https://` |
| `--token` | API access token |
| `--DocType` | Document type, default `racuni` |
| `--acIssuer`, `--acKey`, `--acDoc1` | Search by issuer, key, linked document |
| `--acDept`, `--acCostDRV` | Department, cost driver (several values separated by `;`) |
| `--adDate` | Date range `from;to`, format `yyyy-MM-dd`, both required |
| `--EbaID` | Exact documents, IDs separated by `;`. Other filters are ignored |

## Configuration

Endpoint and token can also come from `appsettings.json` or user secrets (for development):
`EBA_ENDPOINT`, `API_TOKEN`, or `USER` and `PASS` (Basic authentication when there is no token).
Arguments take priority.

## Using the viewer

- All matching documents are loaded, the 100 newest are listed.
- Zoom: `Ctrl` + `+` / `-` / `0` or `Ctrl` + mouse wheel.
- The search button opens a form with the current filters.

## Build

```
dotnet build
```

Requires the .NET 10 SDK on Windows.

## Installer

A single `DocumentViewer-Setup.exe` installs the app for the current user without admin rights.
It always installs to `%LOCALAPPDATA%\Programs\DocumentViewer`, so Pantheon can start
`%LOCALAPPDATA%\Programs\DocumentViewer\DocumentViewer.exe`.

Requires [Inno Setup 6](https://jrsoftware.org/isdl.php). Build with:

```
.\installer\build.ps1
```

This publishes the app (self-contained, win-x64) into `publish\` and creates `installer\output\DocumentViewer-Setup.exe`.
Running a newer setup upgrades the installed app and keeps an edited `appsettings.json`.
Raise `AppVersion` in `installer\DocumentViewer.iss` for each release, and never change `AppId`.

## Notes

- Documents are kept in memory only, nothing is written to disk.
- The server certificate is not validated, because test and production servers use different certificates.
- The token given as an argument is visible in the process list of the machine.
- A document that fails to load on its own is skipped, a connection or access error stops the whole load and shows a message with a Retry button.
