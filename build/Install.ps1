echo "Installing SSL Cert file to $env:SSL_CERT_FILE"
mkdir c:\ca
iwr http://curl.haxx.se/ca/cacert.pem -outfile $env:SSL_CERT_FILE

choco install "msbuild-sonarqube-runner" -y

$env:SEB_FUNCS = Join-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) -Child "funcs"

& $env:SEB_FUNCS/Set-EnvVars.ps1
& $env:SEB_FUNCS/Install-Chandler.ps1
& $env:SEB_FUNCS/Set-Version.ps1
