param ($Solution)

$buildCmd = "C:\Program Files (x86)\MSBuild\14.0\bin\msbuild.exe"
$buildArgs = @(
    $solution,
    "/l:C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll",
    "/m",
    "/p:UseSharedCompilation=false",
    "/p:Configuration=$env:CONFIGURATION",
    "/p:Platform=$env:PLATFORM")
if ($env:APPVEYOR_REPO_TAG -eq $false) {
  & $buildCmd $buildArgs
  return
} else {
  "Building project with Coverity Scan..."
  cov-build --dir cov-int $buildCmd $buildArgs;
}