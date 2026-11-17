[Setup]
AppName=SAP Logon Lite
AppVersion=1.0
AppPublisher=SAP Logon Lite
DefaultDirName={autopf}\SAP Logon Lite
DefaultGroupName=SAP Logon Lite
OutputDir=output
OutputBaseFilename=SAP-Logon-Setup
SetupIconFile=app.ico
UninstallDisplayIcon={app}\SAP Logon.exe
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

[Files]
Source: "SAP Logon.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\SAP Logon"; Filename: "{app}\SAP Logon.exe"
Name: "{autodesktop}\SAP Logon"; Filename: "{app}\SAP Logon.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SAP Logon.exe"; Description: "Launch SAP Logon"; Flags: nowait postinstall skipifsilent
