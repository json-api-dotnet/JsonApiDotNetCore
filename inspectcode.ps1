#Requires -Version 7.0

# This script runs code inspection and opens the results in a web browser.

#dotnet tool restore

dotnet tool uninstall --global jetbrains.resharper.globaltools
dotnet tool install --global jetbrains.resharper.globaltools --version 2022.2.0

dotnet tool uninstall --global regitlint
dotnet tool install --global regitlint --version 6.0.8

dotnet tool uninstall --global codecov.tool
dotnet tool install --global codecov.tool --version 1.13.0

dotnet tool uninstall --global dotnet-reportgenerator-globaltool
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.3


$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.xml')
$resultPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), 'jetbrains-inspectcode-results.html')
jb inspectcode JsonApiDotNetCore.sln --build --output="$outputPath" --profile=WarningSeverities.DotSettings --severity=WARNING --verbosity=WARN -dsl=GlobalAll -dsl=GlobalPerProduct -dsl=SolutionPersonal -dsl=ProjectPersonal

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
