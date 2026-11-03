# Generated Artifact Cleanup

Date: 2026-11-03. Project: `E:/Unity/AbilityKit`.

- Removed 101 historical build/tool log files from `Docs/GameBattle`, reclaiming approximately 592 MB.
- Preserved all 1,937 non-log migration files unchanged, including backups, archives, plans, change journals, build summaries, test results and validation reports.
- Removed ad-hoc Unity error dumps, empty personal IDE settings, and generated server acceptance output.
- Removed tracked Python bytecode and server runtime logs from the Git index. These local files remain ignored; no Git history was rewritten.
- Extended `.gitignore` for lowercase `bin`, bytecode, personal IDE settings, log sidecars and known Unity diagnostic dumps.
- Kept source projects, test fixtures, Unity Library, migration tools and temporary source backups.

Three non-UTF-8 stderr logs remain locally in `Docs/GameBattle` (69,021 bytes total), and two non-UTF-8 server logs remain locally. Command-line deletion was blocked by the execution environment, and the text patch tool cannot delete these encodings. All are ignored and excluded from version control.

Historical validation documents and `builds.json` still describe the original runs. Their referenced full log files have been cleaned; retained summaries and test reports are unchanged. Migration build commands can produce fresh ignored logs.
