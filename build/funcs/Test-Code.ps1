    $xunit = (Resolve-Path "src/packages/xunit.runner.console.*/tools/xunit.console.exe").ToString()
    $opencover = (Resolve-Path "src/packages/OpenCover.*/tools/OpenCover.Console.exe").ToString()
    $coveralls = (Resolve-Path "src/packages/coveralls.net.*/tools/csmacnz.Coveralls.exe").ToString()
    & $opencover -register:user -target:$xunit  -returntargetcode "-targetargs:""src\$env:SEB_PROJECT_NAME\bin\$env:CONFIGURATION\$env:SEB_PROJECT_NAME.dll"" -noshadow -appveyor" -filter:"+[*]* -[*]Json.*" -output:opencoverCoverage.xml  -searchdirs:"src\$env:SEB_PROJECT_NAME\bin\$env:CONFIGURATION\"
    & $coveralls --opencover -i opencoverCoverage.xml --repoToken $env:COVERALLS_REPO_TOKEN --commitId $env:APPVEYOR_REPO_COMMIT --commitBranch $env:APPVEYOR_REPO_BRANCH --commitAuthor $env:APPVEYOR_REPO_COMMIT_AUTHOR --commitEmail $env:APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL --commitMessage $env:APPVEYOR_REPO_COMMIT_MESSAGE --jobId $env:APPVEYOR_JOB_ID
