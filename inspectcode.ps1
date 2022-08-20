#Requires -Version 7.0

# This script runs code inspection and opens the results in a web browser.

dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
$resultPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.html')
dotnet jb inspectcode JsonApiDotNetCore.sln --no-build --output="$outputPath" --profile=WarningSeverities.DotSettings --properties:Configuration=Release --severity=WARNING --verbosity=WARN -dsl=GlobalAll -dsl=GlobalPerProduct -dsl=SolutionPersonal -dsl=ProjectPersonal

if ($LASTEXITCODE -ne 0) {
    throw "Code inspection failed with exit code $LASTEXITCODE"
}

[xml]$xml = Get-Content "$outputPath"
if ($xml.report.Issues -and $xml.report.Issues.Project) {
    $xslt = new-object System.Xml.Xsl.XslCompiledTransform;
    $xslt.Load("$pwd/JetBrainsInspectCodeTransform.xslt");
    $xslt.Transform($outputPath, $resultPath);

    Write-Output "Opening results in browser"
    Invoke-Item "$resultPath"
}
