# Migration Validation

Date: 2026-10-23.

- 15,467 moved files exist at their mapped destinations.
- Original metadata hashes and assembly names/GUIDs are unchanged.
- Script assembly ownership is preserved; 104 asmref files connect split source directories.
- 5,687 C# files are byte-identical to the pre-move snapshot.
- 44 C# files contain migration path substitutions, without behavior changes.
- Representative MSBuild Compile evaluation succeeds for Core, Ability, Moba.Core and Moba.Presentation.Core; no missing source files.
- Unity-generated Moba.Runtime project build: 0 errors, 554 warnings.
- Unity-generated Moba.View.Runtime project build: 0 errors, 307 warnings.
- Unity-generated Moba.Editor project build: 0 errors, 161 warnings.
- These project builds do not prove Unity AssetDatabase import, Play Mode, editor interaction, or battle equivalence.
- Standalone Moba.Core build is blocked by NETSDK1045: target net10.0, installed SDK 9.0.318. Target frameworks were not changed.
- Repository-wide git diff --check reports pre-existing whitespace in ProjectSettings.asset. User settings were not reformatted.

Detailed file and ownership results are in verification.json. View/editor build logs are retained alongside this report.
