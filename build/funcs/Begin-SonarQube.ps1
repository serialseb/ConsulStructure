$sonarBuildId = "com:github:$($env:APPVEYOR_REPO_NAME.Replace("/", ":"))"
$branchParam = ""

if ($env:APPVEYOR_REPO_BRANCH -ne "master") { $branchParam = "/d:sonar.branch=$env:APPVEYOR_REPO_BRANCH" }

Write-Host "SONARQUBE: set branch parameter to '$branchParam'"
MSBuild.SonarQube.Runner.exe begin `
    /k:"$sonarBuildId" `
    /n:"$env:SEB_PROJECT_NAME" `
    /v:$env:SEB_VERSION_BASE `
    /d:sonar.cs.opencover.reportsPaths="opencoverCoverage.xml" `
    /d:sonar.cs.xunit.reportsPaths=="XUnitResults.xml" `
    /d:"sonar.host.url=https://sonarqube.com" `
    /d:"sonar.login=$env:SONARQUBE_TOKEN" $branchParam
