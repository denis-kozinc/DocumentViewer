; Per-user installation of Document Viewer into AppData, no admin rights needed.
; Build with installer\build.ps1, it publishes the app into ..\publish first.

[Setup]
; Never change the AppId, Windows uses it to recognize an upgrade of the same app
AppId={{71448BD5-FB3C-43FB-909F-2E953237AAC9}
AppName=Document Viewer
AppVersion=1.1.0
AppPublisher=Softeh d.o.o.

; Fixed target folder: %LOCALAPPDATA%\Programs\DocumentViewer
DefaultDirName={autopf}\DocumentViewer
DisableDirPage=yes
UsePreviousAppDir=no
DisableProgramGroupPage=yes

PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

OutputDir=output
OutputBaseFilename=DocumentViewer-Setup
Compression=lzma2
SolidCompression=yes

SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\DocumentViewer.exe

; Close the running app when upgrading
CloseApplications=yes

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Excludes: "*.pdb,appsettings.json"; Flags: recursesubdirs ignoreversion
; Not overwritten on upgrade, the user may have edited it
Source: "..\publish\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist

[Icons]
Name: "{autoprograms}\Document Viewer"; Filename: "{app}\DocumentViewer.exe"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\DocumentViewer.exe"; ValueType: string; ValueData: "{app}\DocumentViewer.exe"; Flags: uninsdeletekey
