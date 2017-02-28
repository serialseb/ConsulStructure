if (-not $env:RUBY_VERSION) {
    $env:RUBY_VERSION = "21"
}
$env:CHANDLER_GITHUB_API_TOKEN=$env:GITHUB_TOKEN
$env:PATH = "C:\Ruby$env:RUBY_VERSION\bin;$env:PATH"
$env:SEB_SLN = (Get-ChildItem "$env:APPVEYOR_BUILD_FOLDER/src/*.sln").ToString()
$env:SEB_PROJECT_NAME = $env:APPVEYOR_PROJECT_NAME