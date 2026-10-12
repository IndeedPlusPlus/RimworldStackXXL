The project targets .NET Framework 4.7.2 and RimWorld 1.5/1.6/1.7. Install the
.NET Framework 4.7.2 Developer Pack and MSBuild (Visual Studio Build Tools), or
use `dotnet msbuild` with an installed .NET SDK and the targeting pack.

Copy these files from RimWorld's `RimWorldWin64_Data/Managed` directory here:

- `Assembly-CSharp.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.IMGUIModule.dll`
- `UnityEngine.TextRenderingModule.dll`

Alternatively, point the build at a RimWorld 1.5 or 1.6 Managed directory:

```powershell
dotnet msbuild RimworldStackXXL/StackXXL.csproj -p:Configuration=Release -p:RimWorldManagedDir="C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed"
```

Run that command from the repository root. Build against RimWorld 1.5 for the
shared assembly, and verify it against 1.6 and 1.7 before release. The build writes
`StackXXL.dll` to `Resources/v1.5+/Assemblies`. Game and Unity assemblies are
compile references only and are never copied into the mod.

The shared DLL uses RimWorld 1.5 game references and Unity references from
RimWorld 1.5 or 1.6. Keep that build as the shared package assembly. RimWorld 1.7
loads it through compatibility support for the earlier Unity APIs; use the
earlier references to build the shared DLL.

No external mod libraries are required. Settings use RimWorld's built-in mod
settings window.
