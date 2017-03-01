$repo = $env:APPVEYOR_REPO_NAME

function Get-GitHubRepo([string]$uri="") {
    if ($uri -ne "" -and -not $uri.StartsWith("/")) { $uri = "/$uri" }
    $finalUri = "https://api.github.com/repos/$repo$uri?access_token=$($env:GITHUB_TOKEN)"
    write-host "Issuing request to $finalUri"
    Invoke-WebRequest -Uri $finalUri | ConvertFrom-Json
}

$releaseNotes = 'Pre-release, see https://github.com/serialseb/ConsulStructure/blob/master/CHANGELOG.md for the latest details'

$nuspecPath = "$($env:APPVEYOR_BUILD_FOLDER)\$($env:SEB_PROJECT_NAME).nuspec"
$nuspec = [xml](Get-Content $nuspecPath)

if ($env:APPVEYOR_REPO_TAG -eq $true) {
    $releaseNotes = (Get-GitHubRepo "releases/tags/$($env:APPVEYOR_REPO_TAG_NAME)").body.Replace('"','\"').Replace('### ', '')
    if ($releaseNotes){ 
        $nuspec.package.metadata.releaseNotes = $releaseNotes
    }
}
write-host "Release notes: $releaseNotes"

$repoInfo = Get-GitHubRepo
write-host "Switching license to $env:APPVEYOR_REPO_COMMIT"
if ($repoInfo.description) {
    $nuspec.package.metadata.description = $repoInfo.description
}
$nuspec.package.metadata.licenseUrl = "https://github.com/$($env:APPVEYOR_REPO_NAME)/tree/$env:APPVEYOR_REPO_COMMIT/LICENSE.md"
$nuspec.Save($nuspecPath)
write-host "Saved NuSpec at $nuspecPath"
nuget pack $nuspecPath.nuspec -version $env:APPVEYOR_BUILD_VERSION -basepath $env:APPVEYOR_BUILD_FOLDER/src/$env:SEB_PROJECT_NAME/
Push-AppveyorArtifact *.nupkg