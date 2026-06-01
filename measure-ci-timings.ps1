param(
    [int]$Iterations = 10,
    [string]$ResultFile = "$PSScriptRoot\ci-timings.csv"
)

$PR2014  = 2014  # replace WebApplicationFactory
$PR2018  = 2018  # master baseline
$Repo    = "json-api-dotnet/JsonApiDotNetCore"

function Get-LatestRunIdForPR {
    param([int]$PrNumber)
    $headSha = (gh pr view $PrNumber --repo $Repo --json headRefOid | ConvertFrom-Json).headRefOid
    $runs = (gh api "repos/$Repo/actions/runs?event=pull_request&head_sha=$headSha" | ConvertFrom-Json).workflow_runs
    $run = $runs | Where-Object { $_.name -eq "Build" } | Sort-Object -Property created_at -Descending | Select-Object -First 1
    if (-not $run) {
        Write-Error "No 'Build' workflow run found for PR #$PrNumber (SHA: $headSha)"
        exit 1
    }
    return [string]$run.id
}

Write-Host "Looking up latest CI run IDs..."
$Run2014 = Get-LatestRunIdForPR $PR2014
$Run2018 = Get-LatestRunIdForPR $PR2018
Write-Host "PR #$PR2014 -> run $Run2014"
Write-Host "PR #$PR2018 -> run $Run2018"

"Iteration,PR,OS,DurationSec" | Out-File $ResultFile -Encoding utf8
Write-Host "Results will be saved to: $ResultFile"

function Wait-ForBothCompleted {
    param([bool]$AfterRerun)
    if ($AfterRerun) {
        # Give GitHub ~2 minutes to transition the re-runs out of "completed"
        Write-Host "$(Get-Date -Format 'HH:mm:ss') Waiting 2 min for re-runs to start..."
        Start-Sleep 120
    }
    $done = @{ "2014" = $false; "2018" = $false }
    $ids  = @{ "2014" = $Run2014; "2018" = $Run2018 }
    do {
        foreach ($pr in "2014","2018") {
            if ($done[$pr]) { continue }
            $status = (gh run view $ids[$pr] --repo $Repo --json status | ConvertFrom-Json).status
            Write-Host "$(Get-Date -Format 'HH:mm:ss') [PR$pr] $status"
            if ($status -eq "completed") { $done[$pr] = $true }
        }
        if (-not ($done["2014"] -and $done["2018"])) { Start-Sleep 60 }
    } while (-not ($done["2014"] -and $done["2018"]))
}

function Get-TestDurations {
    param([string]$RunId)
    $jobs = (gh run view $RunId --repo $Repo --json jobs | ConvertFrom-Json).jobs
    $result = @{}
    foreach ($job in $jobs) {
        if ($job.name -notmatch "build-and-test") { continue }
        $os = switch -Regex ($job.name) {
            "ubuntu"  { "ubuntu"  }
            "windows" { "windows" }
            "macos"   { "macos"   }
            default   { "other"   }
        }
        $testStep = $job.steps | Where-Object { $_.name -eq "Test" } | Select-Object -First 1
        if ($testStep -and $testStep.completedAt -and $testStep.startedAt) {
            $result[$os] = [int]([datetime]$testStep.completedAt - [datetime]$testStep.startedAt).TotalSeconds
        }
    }
    return $result
}

for ($iter = 1; $iter -le $Iterations; $iter++) {
    Write-Host ""
    Write-Host "=== Iteration $iter / $Iterations  $(Get-Date -Format 'HH:mm:ss') ==="

    $isRerun = $iter -gt 1
    if ($isRerun) {
        Write-Host "Triggering re-run for PR 2014 (run $Run2014)..."
        gh run rerun $Run2014 --repo $Repo 2>&1
        Write-Host "Triggering re-run for PR 2018 (run $Run2018)..."
        gh run rerun $Run2018 --repo $Repo 2>&1
    }

    Wait-ForBothCompleted -AfterRerun $isRerun

    $d2014 = Get-TestDurations $Run2014
    $d2018 = Get-TestDurations $Run2018

    foreach ($os in "ubuntu","macos","windows") {
        if ($null -ne $d2014[$os]) { "$iter,2014,$os,$($d2014[$os])" | Add-Content $ResultFile -Encoding utf8 }
        if ($null -ne $d2018[$os]) { "$iter,2018,$os,$($d2018[$os])" | Add-Content $ResultFile -Encoding utf8 }
    }

    Write-Host "Iteration $iter results:"
    foreach ($os in "ubuntu","macos","windows") {
        $a = $d2014[$os]; $b = $d2018[$os]
        $diff = if ($a -and $b) { "{0:+#;-#;0}" -f ($a - $b) } else { "?" }
        Write-Host ("  {0,-10}  PR2014={1,4}s   PR2018={2,4}s   diff={3}s" -f $os, $a, $b, $diff)
    }
    Write-Host "ITER_DONE_$iter"
}

Write-Host ""
Write-Host "=== All $Iterations iterations complete ==="
Write-Host "Results saved to: $ResultFile"
Get-Content $ResultFile
Write-Host "ALL_ITERATIONS_COMPLETE"
