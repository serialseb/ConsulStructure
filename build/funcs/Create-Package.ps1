$repo = $env:APPVEYOR_REPO_NAME

function Get-GitHubRepo([string]$uri="") {
    if ($uri -ne "" -and -not $uri.StartsWith("/")) { $uri = "/$uri" }
    $finalUri = "https://api.github.com/repos/$repo$uri" + '?access_token=' + "$($env:GITHUB_TOKEN)"
    write-host "Issuing request to $finalUri"
    Invoke-WebRequest -Uri $finalUri | ConvertFrom-Json
}

$releaseNotes = 'Pre-release, see https://github.com/serialseb/ConsulStructure/blob/master/CHANGELOG.md for the latest details'
$repoInfo = Get-GitHubRepo

if ($env:APPVEYOR_REPO_TAG -eq $true) {
    $releaseNotes = (Get-GitHubRepo "releases/tags/$($env:APPVEYOR_REPO_TAG_NAME)").body.Replace('"','\"').Replace('### ', '')
}

$authors =  (git shortlog -sn | ? { $_ -match '^\s*(?<count>\d+)\s*(?<author>.*)$' } | % {"$($matches["author"]) ($($matches["count"]))" }) -join ', '
$description = $repoInfo.description
$licenseUrl = "https://github.com/$($env:APPVEYOR_REPO_NAME)/tree/$env:APPVEYOR_REPO_COMMIT/LICENSE.md"
$projectUrl = "https://github.com/$($env:APPVEYOR_REPO_NAME)/"

write-host "Release notes: $releaseNotes"
write-host "License URL: $licenseUrl"
write-host "Authors: $authors"
write-host "Project URL: $projectUrl"

& nuget pack $nuspecPath.nuspec `
    -Properties releaseNotes="$releaseNotes"`;`
                authors="$authors"`;`
                licenseUrl="$licenseUrl"`;`
                projectUrl="$projectUrl"`;`
                description="$description"`;`
    -version $env:NUGET_VERSION `
    -basepath $env:APPVEYOR_BUILD_FOLDER/src/$env:SEB_PROJECT_NAME/

Push-AppveyorArtifact *.nupkg