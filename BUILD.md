# Building

Requires .NET SDK, .NET Framework 4.8 targeting pack, Elin and AutoExplore installed.
Set `ELIN_DIR` to the Elin installation and `AUTOEXPLORE_DLL` to the installed
`Elin_AutoExplore.dll`, then run:

```text
dotnet build AutoExploreShearing.csproj -c Release -p:Platform=AnyCPU
```

Output: `bin/Release/net48/AutoExploreShearing.dll`.
No game or dependency assemblies are redistributed. Builds do not install.
Copy this DLL, `package.xml` and `preview.jpg` into an Elin Package subfolder
for manual installation. Use only one enabled copy of this extension.

The stable package ID is `gharkz.autoexploreshearing`. The display name is
Auto-Explore Shearing Extension. AutoExplore is required at runtime; its loader
dependency is soft because Elin gives each package a separate chainloader.
The extension validates the initialized dependency and targeting methods on Start.
Two postfixes add shearing targets to the early-exit classifier and full-search
harvest list. Native shearing runs as a child job to avoid AutoAct takeover.
Target eligibility does not require visibility. Native movement, products and
progress are retained. Stamina checks preserve a positive reserve including wide
shears. Failed stationary targets are skipped for ten seconds before retrying.

The icon is drawn by `build_icon.ps1` using Windows System.Drawing and Segoe UI.
