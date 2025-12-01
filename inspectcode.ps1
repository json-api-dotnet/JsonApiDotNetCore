#Requires -Version 7.4
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# This script runs code inspection and opens the results in a web browser.

dotnet tool restore

$solutionFile = 'JsonApiDotNetCore.sln'
$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
$resultPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.html')

dotnet jb inspectcode --version
dotnet jb inspectcode $solutionFile --build --no-updates --dotnetcoresdk=$(dotnet --version) --output="$outputPath" --format="xml" --settings=WarningSeverities.DotSettings --properties:Configuration=Release --properties:RunAnalyzers=false --properties:NuGetAudit=false --severity=WARNING --verbosity=WARN --disable-settings-layers=GlobalAll --disable-settings-layers=GlobalPerProduct --disable-settings-layers=SolutionPersonal --disable-settings-layers=ProjectPersonal

[xml]$xml = Get-Content "$outputPath"
if ($xml.report.Issues -and $xml.report.Issues.Project) {
    $xslt = new-object System.Xml.Xsl.XslCompiledTransform;
    $xslt.Load("$pwd/JetBrainsInspectCodeTransform.xslt");
    $xslt.Transform($outputPath, $resultPath);

    Write-Output "Opening results in browser"
    Invoke-Item "$resultPath"
}
