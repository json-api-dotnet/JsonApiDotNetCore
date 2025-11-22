#Requires -Version 7.4
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# This script runs code inspection and opens the results in a web browser.

dotnet tool restore

$solutionFile = 'JsonApiDotNetCore.slnx'
$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
$resultPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.html')

dotnet jb inspectcode --version
dotnet jb inspectcode $solutionFile --dotnetcoresdk=$(dotnet --version) --build --output="$outputPath" --format="xml" --profile=WarningSeverities.DotSettings --properties:Configuration=Release --properties:RunAnalyzers=false --severity=WARNING --verbosity=WARN -dsl=GlobalAll -dsl=GlobalPerProduct -dsl=SolutionPersonal -dsl=ProjectPersonal

[xml]$xml = Get-Content "$outputPath"
if ($xml.report.Issues -and $xml.report.Issues.Project) {
    $xslt = new-object System.Xml.Xsl.XslCompiledTransform;
    $xslt.Load("$pwd/JetBrainsInspectCodeTransform.xslt");
    $xslt.Transform($outputPath, $resultPath);

    Write-Output "Opening results in browser"
    Invoke-Item "$resultPath"
}
