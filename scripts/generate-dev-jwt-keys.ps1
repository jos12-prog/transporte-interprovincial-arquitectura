$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$keyDirectory = Join-Path $repoRoot '.secrets/jwt'
New-Item -ItemType Directory -Path $keyDirectory -Force | Out-Null

$rsa = [System.Security.Cryptography.RSA]::Create(2048)
try {
    $privatePath = Join-Path $keyDirectory 'private.pem'
    $publicPath = Join-Path $keyDirectory 'public.pem'
    [System.IO.File]::WriteAllText($privatePath, $rsa.ExportRSAPrivateKeyPem(), [System.Text.Encoding]::ASCII)
    [System.IO.File]::WriteAllText($publicPath, $rsa.ExportRSAPublicKeyPem(), [System.Text.Encoding]::ASCII)
    Write-Output "JWT development key pair generated under .secrets/jwt (private.pem, public.pem)."
}
finally {
    $rsa.Dispose()
}
