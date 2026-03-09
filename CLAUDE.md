# TrustSync - Claude Code Instructions

## Project Overview
TrustSync is an Avalonia UI desktop accounting app (.NET 8, MVVM, SQLite).

## Version Management
The app version MUST be consistent across ALL these locations:
1. `src/TrustSync.Desktop/TrustSync.Desktop.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
2. `src/TrustSync.Desktop/Views/Auth/LoginView.axaml` — footer `Text="v{VERSION}"`
3. `installer.iss` — `AppVersion={VERSION}`

When bumping the version, update ALL three files. Use semantic versioning (e.g. 1.2.0 → 1.3.0).

## Build & Release Process
When asked to commit, push, or release, ALWAYS follow these steps:

### 1. Build the app
```bash
dotnet publish src/TrustSync.Desktop/TrustSync.Desktop.csproj -c Release -r win-x64 --self-contained -o publish
```

### 2. Build the Windows installer
Requires Inno Setup (installed at user-local path):
```bash
"/c/Users/yousi/AppData/Local/Programs/Inno Setup 6/ISCC.exe" installer.iss
```
This produces: `installer_output/TrustSync-Setup.exe`

### 3. Commit & push
- Commit all changes
- Push to origin

### 4. GitHub Release (when asked)
```bash
gh release create v{VERSION} installer_output/TrustSync-Setup.exe --title "TrustSync v{VERSION}" --notes "Release notes here"
```

## Key Paths
- Solution: `TrustSync.sln`
- Desktop app: `src/TrustSync.Desktop/`
- Installer config: `installer.iss`
- Installer output: `installer_output/TrustSync-Setup.exe`
- Themes: `src/TrustSync.Desktop/Themes/`
- Auth views: `src/TrustSync.Desktop/Views/Auth/`

## Architecture
- `TrustSync.Domain` — Entities
- `TrustSync.Application` — Services, interfaces
- `TrustSync.Infrastructure` — EF Core, persistence
- `TrustSync.Desktop` — Avalonia UI, ViewModels, Views

## Theme System
- Light/Dark themes defined in `Themes/Core/Colors.axaml` using `ThemeDictionaries`
- All colors use `DynamicResource` for theme switching
- Logo switches via `LogoDarkOpacity`/`LogoWhiteOpacity` resources
- Theme persisted in AppSettings DB, applied via `App.ApplyTheme()`
