# Flow Launcher Plugin: Insert

Small, fast Flow Launcher plugin for inserting or generating text on demand.
Use it as a starting point or tailor it to your workflow.

## Features
- Invoke via a keyword and pass arguments
- Return actionable results (copy to clipboard, paste, open, etc.)
- Lightweight and easy to extend
- Works offline

Update this section with your plugin’s actual behavior and examples.

## Requirements
- Flow Launcher (latest)
- .NET SDK (use the version declared in global.json or the project’s .csproj)

## Install
- From Flow Launcher:
    - If published: open Flow Launcher → Settings → Plugins → Install and search for “Insert”
- Manual:
    - Build Release and copy the output folder to:
        - %APPDATA%\FlowLauncher\Plugins\Flow.Launcher.Plugin.Insert

## Usage
- Trigger with your keyword (configure in plugin.json or Flow settings)
- Example queries:
    - <keyword> <your text or command>
Replace with real examples once finalized.

## Configure
- plugin.json controls:
    - Name, Description, Author, Version
    - ActionKeyword (the trigger), Language, IcoPath, etc.
- Plugin settings can be surfaced via Flow Launcher’s Settings UI if implemented

## Build
- git clone <repo>
- dotnet restore
- dotnet build -c Release

## Debug
- Set Flow Launcher as the startup program or attach to Flow.Launcher.exe
- Use portable mode for a clean dev sandbox:
    - Create a portable “Data” folder next to Flow.Launcher.exe, then place your plugin under Data\Plugins
- Optionally link your bin output to the portable Plugins folder for rapid iteration

## Release
- Update version in plugin.json
- Build Release
- Package plugin folder (include plugin.json, icons, binaries)
- Publish to the Flow Launcher Plugin Store if desired

## Roadmap
- [ ] Document commands and results
- [ ] Add settings UI (if needed)
- [ ] Add tests
- [ ] Provide localized strings

## Contributing
Issues and pull requests are welcome. Keep changes small and focused.

## License
Specify your license (e.g., MIT) in a LICENSE file and reference it here.