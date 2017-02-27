if ($env:APPVEYOR_REPO_TAG -eq $true) {
  nuget.exe install PublishCoverity -ExcludeVersion
  PublishCoverity\PublishCoverity.exe compress -o coverity.zip -i cov-int
  $version = $env:APPVEYOR_BUILD_VERSION
  .\PublishCoverity\PublishCoverity.exe publish `
    -t "$env:coverity_token" `
    -r "$env:APPVEYOR_REPO_NAME" `
    -z coverity.zip `
    -d "AppVeyor Tagged Build($env:APPVEYOR_BUILD_VERSION)." `
    --codeVersion "$version"
}