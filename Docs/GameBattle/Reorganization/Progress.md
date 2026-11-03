# GameBattle Reorganization

Date: 2026-10-26.

Historical directory-reorganization checkpoint. Framework assembly identities
were subsequently consolidated into GameBattle.Runtime, GameBattle.Unity, and
GameBattle.Editor. Current evidence is in ../AssemblyConsolidation/Validation.md.
Do not rerun this stage's identity-preserving migration against the new layout.

Scope: directory ownership, path references, assembly boundaries, and the minimal
engine adapter separation required for a Unity-free Runtime. Combat algorithms,
serialized schemas, namespaces, and existing assembly identities are retained.

## Stages

- [x] Inventory and baseline hashes captured in `plan.json`.
- [x] Relocate files to Editor, Runtime, Unity, and Example.
- [x] Repair source projects, asset paths, tools, and assembly references.
- [x] Enforce engine-free Runtime boundaries.
- [x] Verify hashes, GUIDs, ownership, project inputs, and generated-project compilation.

## Verification Results

- All 15,760 baseline files accounted for; original metadata bytes and assembly
  identities preserved.
- Runtime source engine-API checks and framework-to-Example dependency checks pass.
- All 68 Runtime assemblies built; binary inspection found no direct Unity
  assembly references.
- MOBA view/editor, default Assembly-CSharp/Assembly-CSharp-Editor, Shooter Runtime,
  Tiny view, and the three optional network transports build with zero errors.
- Seven engine-free environment/record-path checks pass.
- Live Unity reimport, Console, Play Mode, and player build remain unverified:
  the HTTP MCP server is running, but the editor has not connected.
- Temporary editor migration helper removed; no editor refresh/reload lock is
  left by this migration.

## Constraints

- No overwrite or deletion of user source changes.
- Existing `.meta` bytes and GUIDs remain unchanged.
- Metadata for merged grouping folders and historical package documentation is
  retained under this directory's Archive, outside the Unity source tree.
- MOBA, Shooter, and Tiny code, resources, tooling, and tests are under Example.
- Runtime/Buff is an explicit ownership boundary. Existing MOBA-specific Buff
  implementations stay in Example; this relocation does not invent a generic Buff
  framework or add global Buff functionality.
- Networking, hosting, replay, synchronization, and diagnostics remain separate
  submodules under Runtime/Battle until their internal design is reviewed.
- The .NET `src` projects remain build/test entry points; their linked Unity source
  paths are updated, not duplicated.
