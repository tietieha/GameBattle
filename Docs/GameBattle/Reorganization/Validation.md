# GameBattle Reorganization Validation

Date: 2026-10-26.

Historical checkpoint before assembly consolidation. The 68-assembly binary
report and original identities below describe that stage only. Current framework
assemblies and verification are documented in ../AssemblyConsolidation/Validation.md.

## Completed Scope

The physical source tree now has exactly four root directories: Editor, Runtime,
Unity, and Example. Runtime has the 13 agreed responsibility directories.
MOBA/Shooter/Tiny-specific source, Unity adapters, editor tooling, resources,
and Unity-side tests are under their respective Example directories.

The .NET src projects remain external build/test entry points linked to the new
source locations. Existing combat algorithms and serialized schemas were not
rewritten. Runtime/Buff reserves a shared boundary; its generic implementation
and global/team Buff support are not implemented by this relocation.

## Static Evidence

- plan.json: 15,760 baseline files and 5,731 baseline C# sources.
- verification.json: no missing files, changed original metadata, duplicate
  active GUIDs, lost assembly identities, ownership mismatches, engine API usage
  in Runtime, or Runtime references to Editor/Unity/Example assemblies.
- Source changes are limited to journaled path replacements and the explicitly
  enumerated engine-adapter changes.
- Existing source assembly identities are retained. The Unity-only reload
  listener now belongs to Core.Unity; the Unity diagnostics helper was split
  from the logic diagnostics source into Core.Unity.
- Original folder metadata for merged/empty groups, documentation, licenses,
  and old build caches are retained outside the Unity source tree in Archive.

## Compilation And Tests

Generated Unity project builds completed with zero errors:

- AbilityKit.Demo.Moba.View.Runtime.
- AbilityKit.Demo.Moba.Editor.
- Assembly-CSharp-Editor and its dependency graph, including Assembly-CSharp.
- AbilityKit.Demo.Shooter.Runtime.
- AbilityKit.Demo.Tiny.View.
- AbilityKit.Network.Transport.InMemory.
- AbilityKit.Network.Transport.LiteNet.
- AbilityKit.Network.Transport.WebSocket.

Compiler warnings remain; this migration does not claim a warning-free project.
The first parallel build attempt encountered a shared obj file lock; the final
builds were rerun sequentially. The MemoryPack record adapter's new explicit Core
reference was also corrected before the final successful build.

runtime-binaries.json records all 68 Runtime assemblies inspected with zero
direct Unity assembly references. This is direct-reference verification, not
a claim about every external dependency's Unity packaging or all platform
configurations.

The net9.0 boundary test executable passed seven checks: default and injected
data directory, absolute/null/empty path handling, nested record save/load
roundtrip, and injected warning handling. The repository's net10.0 project
targets were not downgraded.

## Remaining Verification

The local MCP HTTP server is reachable but reports zero connected Unity
instances. The live project's Unity reimport/Console state, Play Mode,
cold-start import, and player builds have not been verified this round.
The temporary editor connection/migration helper was removed.

The broader repository still contains unrelated missing references in the
vendored ET projects. These are outside the GameBattle migration scope.
