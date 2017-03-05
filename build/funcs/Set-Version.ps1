$baseVersion = $(cat VERSION)
$version = [version]$baseVersion
$build = $env:APPVEYOR_BUILD_NUMBER
$major = $version.Major
$branch = $env:APPVEYOR_REPO_BRANCH

if ($env:APPVEYOR_REPO_TAG -eq $true) {
    $version = $env:APPVEYOR_REPO_TAG_NAME
    $buildVersionPrefix = $version
    $nugetVersion = $version
} else {
    # $buildsForBranch = "/api/projects/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_SLUG/history?recordsNumber=4000&branch=$branch"
    # $lastBuild = (Invoke-WebRequest -Uri $buildsForBranch) | ConvertFrom-Json
    # $lastBuildVersion = $lastBuild.build.version

    if ($branch -eq 'master') {
        $buildVersionSuffix = "ci"
    }
    else {
        $buildVersionSuffix = "b-$branch"
    }
    $buildVersionPrefix = "$baseVersion-$buildVersionSuffix"
    $version = "$buildVersionPrefix+$build"

    
    $nugetBuild = "-$($build | % PadLeft 4 '0')"
    $nugetSuffix = $buildVersionSuffix.Substring([math]::min(15, $buildVersionSuffix.Length))
    
    $nugetVersion = "$buildVersionPrefix$nugetSuffix$nugetBuild"
}
$env:SEB_VERSION_BASE = $baseVersion
$env:SEB_VERSION_PREFIX = $buildVersionPrefix
$env:NUGET_VERSION = $nugetVersion
Write-Host "Version '$version', base '$baseVersion', prefix '$env:SEB_VERSION_PREFIX"

Set-AppveyorBuildVariable -Name "AssemblyMajor" -Value "$major"
Update-AppVeyorBuild -Version "$version"

ruby bin/chandler push