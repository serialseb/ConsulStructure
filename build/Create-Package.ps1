  $releaseNotes = 'Pre-release, see https://github.com/serialseb/ConsulStructure/blob/master/CHANGELOG.md for the latest details'
  echo "Repo tag: $env:APPVEYOR_REPO_TAG"
  $nuspecPath = "$($env:APPVEYOR_BUILD_FOLDER)\ConsulStructure.nuspec"
  $nuspec = [xml](Get-Content $nuspecPath)
  if ($env:APPVEYOR_REPO_TAG -eq $true) {
    $uri = "https://api.github.com/repos/serialseb/ConsulStructure/releases/tags/$($env:APPVEYOR_REPO_TAG_NAME)?access_token=$($env:GITHUB_TOKEN)"
    echo "Loading from $uri"
    $releaseNotes = (Invoke-WebRequest -Uri $uri | convertfrom-json).body.Replace('"','\"')
    echo "Release notes: $releaseNotes"
    $nuspec.package.metadata.releaseNotes = $releaseNotes
  }
  echo "Switching license to $env:APPVEYOR_REPO_COMMIT"
  $nuspec.package.metadata.licenseUrl = "https://github.com/serialseb/ConsulStructure/tree/$env:APPVEYOR_REPO_COMMIT/LICENSE.md"
  $nuspec.Save($nuspecPath)
  echo "Saved NuSpec at $nuspecPath"
  nuget pack ConsulStructure.nuspec -version $env:APPVEYOR_BUILD_VERSION -basepath $env:APPVEYOR_BUILD_FOLDER/src/ConsulStructure/