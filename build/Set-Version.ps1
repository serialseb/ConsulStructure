$version = [version]$(cat VERSION)
$build = $env:APPVEYOR_BUILD_NUMBER
$major = $version.Major

$branch = $env:APPVEYOR_REPO_BRANCH

if ($branch -eq 'master') {
    $version = "$version-ci-$build"
} elseif ($env:APPVEYOR_REPO_TAG) {
    $version = $env:APPVEYOR_REPO_TAG_NAME
}
else {
    $version = "$version-br-$branch-$build"
}

Set-AppveyorBuildVariable -Name "AssemblyMajor" -Value "$major"
Update-AppVeyorBuild -Version "$version"

gem install chandler --no-ri --no-rdoc
set-content ~/.netrc "machine api.github.com login $env:github_username password $env:github_password" -encoding ascii
cp ~/.netrc ~/_netrc
chandler push