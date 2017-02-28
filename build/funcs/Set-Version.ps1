$version = [version]$(cat VERSION)
$baseVersion = $version
$build = $env:APPVEYOR_BUILD_NUMBER | % PadLeft 4 '0'
$major = $version.Major
$branch = $env:APPVEYOR_REPO_BRANCH

if ($env:APPVEYOR_REPO_TAG){
    $version = $env:APPVEYOR_REPO_TAG_NAME
} else {
    $buildsForBranch = "/api/projects/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_SLUG/history?recordsNumber=4000&branch=$branch"
    $lastBuild = (Invoke-WebRequest -Uri $buildsForBranch) | ConvertFrom-Json
    $lastBuildVersion = $lastBuild.build.version

    if ($branch -eq 'master') {
        $buildVersionPrefix = "$baseVersion-ci-" 
        $version = "$version-ci-$build"
    }
    else {
        $version = "$version-br$branch-$build"
    }
}
Set-AppveyorBuildVariable -Name "AssemblyMajor" -Value "$major"
Update-AppVeyorBuild -Version "$version"

bin/chandler push