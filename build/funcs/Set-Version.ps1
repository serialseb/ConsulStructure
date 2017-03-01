$version = [version]$(cat VERSION)
$baseVersion = $version
$build = $env:APPVEYOR_BUILD_NUMBER | % PadLeft 4 '0'
$major = $version.Major
$branch = $env:APPVEYOR_REPO_BRANCH

if ($env:APPVEYOR_REPO_TAG) {
    $version = $env:APPVEYOR_REPO_TAG_NAME
    $buildVersionPrefix = $version
} else {
    $buildsForBranch = "/api/projects/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_SLUG/history?recordsNumber=4000&branch=$branch"
    $lastBuild = (Invoke-WebRequest -Uri $buildsForBranch) | ConvertFrom-Json
    $lastBuildVersion = $lastBuild.build.version

    if ($branch -eq 'master') {
        $buildVersionPrefix = "$version-ci"
        $version = "$buildVersionPrefix-$build"
    }
    else {
        $buildVersionPrefix = "$version-b"
        $version = "$buildVersionPrefix-$branch-$build"
    }
}

Set-AppveyorBuildVariable -Name "AssemblyMajor" -Value "$major"
Update-AppVeyorBuild -Version "$version"

$env:SEB_VERSION_PREFIX = $buildVersionPrefix
Write-Host "Version '{$version}' with prefix {$env:SEB_VERSION_PREFIX}"
ruby bin/chandler push