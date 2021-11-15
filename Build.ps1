function CheckLastExitCode {
    param ([int[]]$SuccessCodes = @(0), [scriptblock]$CleanupScript=$null)

    if ($SuccessCodes -notcontains $LastExitCode) {
        throw "Executable returned exit code $LastExitCode"
    }
}

function RunInspectCode {
    $outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
    dotnet jb inspectcode JsonApiDotNetCore.sln --no-build --output="$outputPath" --profile=WarningSeverities.DotSettings --properties:Configuration=Release --severity=WARNING --verbosity=WARN -dsl=GlobalAll -dsl=SolutionPersonal -dsl=ProjectPersonal
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

        dotnet regitlint -s JsonApiDotNetCore.sln --print-command --jb --profile --jb --profile='\"JADNC Full Cleanup\"' --jb --properties:Configuration=Release --jb --verbosity=WARN -f commits -a $mergeCommitHash -b $targetCommitHash --fail-on-diff --print-diff
        CheckLastExitCode
    }
}

function ReportCodeCoverage {
    if ($env:APPVEYOR) {
        if ($IsWindows) {
            dotnet codecov -f "**\coverage.cobertura.xml"
        }
    }
    else {
        dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage
    }

    CheckLastExitCode
}

function CreateNuGetPackage {
    if ($env:APPVEYOR_REPO_TAG -eq $true) {
        # Get the version suffix from the repo tag. Example: v1.0.0-preview1-final => preview1-final
        $segments = $env:APPVEYOR_REPO_TAG_NAME -split "-"
        $suffixSegments = $segments[1..2]
        $versionSuffix = $suffixSegments -join "-"
    }
    else {
        # Get the version suffix from the auto-incrementing build number. Example: "123" => "pre-0123".
        if ($env:APPVEYOR_BUILD_NUMBER) {
            $revision = "{0:D4}" -f [convert]::ToInt32($env:APPVEYOR_BUILD_NUMBER, 10)
            $versionSuffix = "pre-$revision"
        }
        else {
            $versionSuffix = "pre-0001"
        }
    }

    if ([string]::IsNullOrWhitespace($versionSuffix)) {
        dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts
    }
    else {
        dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$versionSuffix
    }

    CheckLastExitCode
}

dotnet tool restore
CheckLastExitCode

dotnet build -c Release
CheckLastExitCode

RunInspectCode
RunCleanupCode

# Owing to issues with the Azure Cosmos Emulator for Linux on appveyor, do not run the CosmosDbTests on Linux
if ($isLinux) {
    dotnet test --filter 'FullyQualifiedName!~CosmosDbTests' -c Release --no-build --collect:"XPlat Code Coverage"
    CheckLastExitCode
}
else {
    dotnet test -c Release --no-build --collect:"XPlat Code Coverage"
    CheckLastExitCode
}

ReportCodeCoverage

CreateNuGetPackage
