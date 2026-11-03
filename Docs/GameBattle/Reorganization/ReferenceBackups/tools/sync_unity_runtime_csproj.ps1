param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = "Unity\AbilityKit.Demo.Moba.Runtime.csproj",

    [Parameter(Mandatory = $false)]
    [string]$SourceRoot = "Unity\Assets\Scripts\GameBattle\Gameplay\Moba\Runtime",

    [Parameter(Mandatory = $false)]
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'

function Resolve-RepoPath([string]$Path) {
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Get-RelativePath([string]$BasePath, [string]$TargetPath) {
    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri($baseFullPath)
    $targetUri = New-Object System.Uri($targetFullPath)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

$projectFullPath = Resolve-RepoPath $ProjectPath
$sourceRootFullPath = Resolve-RepoPath $SourceRoot
$unityRoot = Split-Path -Parent $projectFullPath

if (!(Test-Path $projectFullPath)) {
    throw "Project file not found: $projectFullPath"
}

if (!(Test-Path $sourceRootFullPath)) {
    throw "Source root not found: $sourceRootFullPath"
}

$projectText = [System.IO.File]::ReadAllText($projectFullPath)
$existing = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($match in [regex]::Matches($projectText, '<Compile\s+Include="([^"]+)"\s*/>')) {
    [void]$existing.Add($match.Groups[1].Value)
}

$sourceFiles = Get-ChildItem -Path $sourceRootFullPath -Filter '*.cs' -Recurse -File |
    Sort-Object FullName

$missing = New-Object 'System.Collections.Generic.List[string]'
foreach ($file in $sourceFiles) {
    $relative = Get-RelativePath $unityRoot $file.FullName
    if (!$existing.Contains($relative)) {
        $missing.Add($relative)
    }
}

if ($missing.Count -eq 0) {
    Write-Host "Unity runtime csproj is in sync: $ProjectPath" -ForegroundColor Green
    exit 0
}

Write-Host ("Missing Compile Include entries in {0}:" -f $ProjectPath) -ForegroundColor Yellow
foreach ($include in $missing) {
    Write-Host "  $include"
}

if (!$Apply) {
    Write-Host "Run with -Apply to insert missing entries." -ForegroundColor Yellow
    exit 2
}

$compileItemGroup = [regex]::Match($projectText, '(?s)<ItemGroup>\s*(?:<Compile\s+Include="[^"]+"\s*/>\s*)+')
if (!$compileItemGroup.Success) {
    throw "Could not find Compile ItemGroup in $ProjectPath"
}

$insertAt = $projectText.IndexOf('</ItemGroup>', $compileItemGroup.Index)
if ($insertAt -lt 0) {
    throw "Could not find end of Compile ItemGroup in $ProjectPath"
}

$lines = foreach ($include in $missing) {
    "    <Compile Include=`"$include`" />"
}
$insertText = ($lines -join "`r`n") + "`r`n"
$updated = $projectText.Insert($insertAt, $insertText)

$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($projectFullPath, $updated, $utf8Bom)

Write-Host "Inserted $($missing.Count) Compile Include entries into $ProjectPath" -ForegroundColor Green
