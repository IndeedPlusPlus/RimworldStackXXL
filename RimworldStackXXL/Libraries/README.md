The project targets .NET Framework 4.7.2 and RimWorld 1.3 through 1.7. Install the
.NET Framework 4.7.2 Developer Pack and MSBuild (Visual Studio Build Tools), or
use `dotnet msbuild` with an installed .NET SDK and the targeting pack.

Copy these files from RimWorld's `RimWorldWin64_Data/Managed` directory here:

- `Assembly-CSharp.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.IMGUIModule.dll`
- `UnityEngine.TextRenderingModule.dll`

Build the RimWorld 1.3 DLL using references from RimWorld 1.3:

```powershell
dotnet msbuild RimworldStackXXL/StackXXL.csproj -p:Configuration=Release -p:RimWorldVersion=1.3 -p:RimWorldManagedDir="C:\RimWorld-1.3\RimWorldWin64_Data\Managed"
```

Build the shared RimWorld 1.4 through 1.7 DLL using references from RimWorld 1.5:

```powershell
dotnet msbuild RimworldStackXXL/StackXXL.csproj -p:Configuration=Release -p:RimWorldVersion=1.5 -p:RimWorldManagedDir="C:\RimWorld-1.5\RimWorldWin64_Data\Managed"
```

Run both commands from the repository root. `RimWorldVersion` defaults to `1.5`.
The 1.3 build writes `StackXXL.dll` to `Resources/v1.3/Assemblies`; the shared build
writes to `Resources/v1.5+/Assemblies`. Their intermediate files are kept separate
so one build cannot reuse the other's references. Both DLLs must be included in
the package. Game and Unity assemblies are compile references only and are never
copied into the mod.

The separate 1.3 build is necessary because native UI method signatures changed
in 1.4. `LoadFolders.xml` selects the 1.3 DLL only for RimWorld 1.3 and the shared
DLL for RimWorld 1.4 through 1.7. Verify both builds against their supported game
versions before release.

The shared DLL uses RimWorld 1.5 game references and Unity references from
RimWorld 1.5 or 1.6. Keep that build as the shared package assembly. RimWorld 1.7
loads it through compatibility support for the earlier Unity APIs; use the
earlier references to build the shared DLL.

No external mod libraries are required. Settings use RimWorld's built-in mod
settings window.
