param(
    [string]$Root = (Get-Location).Path,
    [ValidateSet('Project', 'Assets')]
    [string]$Scope = 'Project'
)

$Root = (Resolve-Path -LiteralPath $Root).Path

$excludeFragments = @(
    '\\Library\\',
    '\\Temp\\',
    '\\obj\\',
    '\\.git\\',
    '\\Logs\\',
    '\\Build\\',
    '\\Builds\\',
    '\\UserSettings\\'
)

$exts = @('.cs', '.md')

$scanPath = if ($Scope -eq 'Assets') { Join-Path $Root 'Assets' } else { $Root }

$files = Get-ChildItem -LiteralPath $scanPath -Recurse -File | Where-Object {
    $full = $_.FullName
    ($exts -contains $_.Extension) -and (-not ($excludeFragments | Where-Object { $full -like "*$_*" }))
}

$byExt = $files | Group-Object Extension | ForEach-Object {
    $lineCount = 0
    foreach ($f in $_.Group) {
        $lineCount += ([System.IO.File]::ReadLines($f.FullName) | Measure-Object).Count
    }

    [pscustomobject]@{
        Extension = $_.Name
        Files     = $_.Count
        Lines     = $lineCount
    }
} | Sort-Object Extension

$byExt | Format-Table -AutoSize

$totalLines = ($byExt | Measure-Object -Property Lines -Sum).Sum
$totalFiles = ($byExt | Measure-Object -Property Files -Sum).Sum

"Total files: $totalFiles"
"Total lines: $totalLines"
