# Stack XXL

Stack XXL increases RimWorld item stack limits using configurable XL and XXL
multipliers. This version supports RimWorld 1.3 through 1.7 and uses the game's
built-in mod settings window. It has no external mod dependencies.

Set the multipliers and choose Default, XL, or XXL for each item category under
Options > Mod settings > Stack XXL. Restart RimWorld after changing settings.

On the first launch, existing Stack XXL settings are imported from the old
HugsLib settings file when available. Once native settings have been saved,
they take precedence. The shared legacy settings file is left untouched.

## Build and install

See [the build reference instructions](RimworldStackXXL/Libraries/README.md)
for the required game assemblies and build commands. Build both DLLs: against
1.3 for RimWorld 1.3, and against 1.5 for RimWorld 1.4 through 1.7. Check all
supported versions before publishing.

The `RimworldStackXXL/Resources` directory is the mod package. After building,
copy its contents into a `StackXXL` directory under RimWorld's `Mods` directory.
RimWorld 1.3 loads `v1.3/Assemblies`; RimWorld 1.4 through 1.7 load
`v1.5+/Assemblies`. The latter folder keeps its existing name for compatibility
with the previous package layout. `LoadFolders.xml` selects exactly one DLL
alongside the shared languages and metadata.
Avoid carrying over assemblies from previous Stack XXL versions when updating.

Future RimWorld versions should be tested before adding them to
`About/About.xml` and `LoadFolders.xml`.
