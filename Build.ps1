function CheckLastExitCode {
    param ([int[]]$SuccessCodes = @(0), [scriptblock]$CleanupScript=$null)

    if ($SuccessCodes -notcontains $LastExitCode) {
        throw "Executable returned exit code $LastExitCode"
    }
}

function RunInspectCode {
    $outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
    # passing --build instead of --no-build as workaround for https://youtrack.jetbrains.com/issue/RSRP-487054
    dotnet jb inspectcode JsonApiDotNetCore.sln --no-build --output="$outputPath" --properties:Configuration=Release --severity=WARNING --verbosity=VERBOSE -dsl=GlobalAll -dsl=GlobalPerProduct -dsl=SolutionPersonal -dsl=ProjectPersonal
    CheckLastExitCode

    [xml]$xml = Get-Content "$outputPath"
    if ($xml.report.Issues -and $xml.report.Issues.Project) {
        foreach ($project in $xml.report.Issues.Project) {
            if ($project.Issue.Count -gt 0) {
                $project.ForEach({
                    Write-Output "`nProject $($project.Name)"
                    $failed = $true

                    $_.Issue.ForEach({
                        $issueType = $xml.report.IssueTypes.SelectSingleNode("IssueType[@Id='$($_.TypeId)']")
                        $severity = $_.Severity ?? $issueType.Severity

                        Write-Output "[$severity] $($_.File):$($_.Line) $($_.Message)"
                    })
                })
            }
        }

        if ($failed) {
            throw "One or more projects failed code inspection.";
        }
    }
}

function RunCleanupCode {
    # When running in cibuild for a pull request, this reformats only the files changed in the PR and fails if the reformat produces changes.

    if ($env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT) {
        Write-Output "Running code cleanup on changed files in pull request"

        # In the past, we used $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT for the merge commit hash. That is the pinned hash at the time the build is enqueued.
        # When a force-push happens after that, while the build hasn't yet started, this hash becomes invalid during the build, resulting in a lookup error.
        # To prevent failing the build for unobvious reasons we use HEAD, which is always the latest version.
        $mergeCommitHash = git rev-parse "HEAD"
        $targetCommitHash = git rev-parse "$env:APPVEYOR_REPO_BRANCH"

        dotnet regitlint -s JsonApiDotNetCore.sln --print-command --disable-jb-path-hack --jb --profile='\"JADNC Full Cleanup\"' --jb --properties:Configuration=Release --jb --verbosity=WARN -f commits -a $mergeCommitHash -b $targetCommitHash --fail-on-diff --print-diff
        CheckLastExitCode
    }
}

dotnet tool restore
CheckLastExitCode

dotnet build -c Release
CheckLastExitCode

RunInspectCode
RunCleanupCode
