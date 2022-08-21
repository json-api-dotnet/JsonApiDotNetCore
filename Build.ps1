function CheckLastExitCode {
    param ([int[]]$SuccessCodes = @(0), [scriptblock]$CleanupScript=$null)

    if ($SuccessCodes -notcontains $LastExitCode) {
        throw "Executable returned exit code $LastExitCode"
    }
}

function RunInspectCode {
    $outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
    dotnet jb inspectcode JsonApiDotNetCore.sln --no-build --output="$outputPath" --properties:Configuration=Release --severity=WARNING --verbosity=VERBOSE -dsl=GlobalAll -dsl=GlobalPerProduct -dsl=SolutionPersonal -dsl=ProjectPersonal
    CheckLastExitCode

    Write-Output "InspectCode execution has completed."
}

dotnet tool restore
CheckLastExitCode

dotnet build -c Release
CheckLastExitCode

RunInspectCode
