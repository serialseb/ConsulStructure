$sonarBuildId = "com:github:$($env:APPVEYOR_REPO_NAME.Replace("/", ":"))"
MSBuild.SonarQube.Runner.exe begin /k:"$sonarBuildId" /n:"$env:SEB_PROJECT_NAME" /v:$env:SEB_VERSION_PREFIX /d:sonar.cs.opencover.reportsPaths="opencoverCoverage.xml" /d:sonar.cs.xunit.reportsPaths=="XUnitResults.xml" /d:"sonar.host.url=https://sonarqube.com" /d:"sonar.login=$env:SONARQUBE_TOKEN"
