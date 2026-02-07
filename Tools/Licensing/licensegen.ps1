param(
    [Parameter(Mandatory = $false)]
    [string] $MachineCode,

    [Parameter(Mandatory = $false)]
    [string] $ExpiresAt,

    [Parameter(Mandatory = $false)]
    [switch] $InitKeys,

    [Parameter(Mandatory = $false)]
    [switch] $ExportPublicKeyToUnity

    ,
    [Parameter(Mandatory = $false)]
    [string] $OutFile,

    [Parameter(Mandatory = $false)]
    [switch] $FromClipboard
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$keysDir = Join-Path $root 'keys'
$privatePath = Join-Path $keysDir 'private_key.xml'
$publicPath = Join-Path $keysDir 'public_key.xml'

$unityPublicKeyPath = Resolve-Path -LiteralPath (Join-Path $root '..\..\Assets\Resources\license_public_key.xml')

function Ensure-Keys {
    if (!(Test-Path $keysDir)) { New-Item -ItemType Directory -Path $keysDir | Out-Null }

    if ((Test-Path $privatePath) -and (Test-Path $publicPath)) {
        return
    }

    Write-Host 'Generating new RSA-2048 keypair...'
    $rsa = [System.Security.Cryptography.RSA]::Create(2048)

    # Export as XML (compatible with RSACryptoServiceProvider.FromXmlString)
    $privateXml = $rsa.ToXmlString($true)
    $publicXml = $rsa.ToXmlString($false)

    Set-Content -Path $privatePath -Value $privateXml -Encoding UTF8
    Set-Content -Path $publicPath -Value $publicXml -Encoding UTF8

    Write-Host "Saved private key: $privatePath"
    Write-Host "Saved public key : $publicPath"
}

function Base64UrlEncode([byte[]] $bytes) {
    $b64 = [Convert]::ToBase64String($bytes)
    $b64 = $b64.TrimEnd('=')
    $b64 = $b64.Replace('+','-').Replace('/','_')
    return $b64
}

Ensure-Keys

if ($ExportPublicKeyToUnity) {
    $pub = Get-Content -Path $publicPath -Raw
    Set-Content -Path $unityPublicKeyPath -Value $pub -Encoding UTF8
    Write-Host "Exported public key to: $unityPublicKeyPath"
}

if ($InitKeys) {
    Write-Host 'Keys initialized.'
    exit 0
}

if ([string]::IsNullOrWhiteSpace($MachineCode) -and $FromClipboard) {
    try {
        $MachineCode = (Get-Clipboard -Raw)
    } catch {
        # ignore
    }
}

if ([string]::IsNullOrWhiteSpace($MachineCode)) {
    $MachineCode = Read-Host 'Paste MachineCode'
}

if ([string]::IsNullOrWhiteSpace($MachineCode)) {
    Write-Host 'No MachineCode provided.'
    Write-Host 'Tip: copy the MachineCode then run: .\licensegen.ps1 -FromClipboard'
    exit 2
}

# Normalize machine code (Unity outputs uppercase Base32; clipboard/file copies may include newlines/spaces)
$MachineCode = ($MachineCode -replace '\s', '').ToUpperInvariant()

$product = 'rollaball'
$issuedAt = [DateTime]::UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")

# expiresAt is optional; keep field present for future expiry support
$expiresAtValue = ''
if (-not [string]::IsNullOrWhiteSpace($ExpiresAt)) {
    $expiresAtValue = $ExpiresAt
}

$payloadObj = [ordered]@{
    product  = $product
    machine  = $MachineCode
    issuedAt = $issuedAt
    expiresAt = $expiresAtValue
}

$payloadJson = ($payloadObj | ConvertTo-Json -Compress)
$payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($payloadJson)

$privateXml = Get-Content -Path $privatePath -Raw
$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider
$rsa.FromXmlString($privateXml)

$signature = $rsa.SignData($payloadBytes, [System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'))

$payloadB64u = Base64UrlEncode $payloadBytes
$sigB64u = Base64UrlEncode $signature

$key = "ROLLABALL1.$payloadB64u.$sigB64u"

Write-Host 'Activation Key (also copied to clipboard):'
Write-Host $key

if ([string]::IsNullOrWhiteSpace($OutFile)) {
    # Always write next to this script for predictable copy/paste
    $OutFile = Join-Path $root 'license.key'
}

if (-not [string]::IsNullOrWhiteSpace($OutFile)) {
    $outPath = Resolve-Path -LiteralPath (Split-Path -Parent $OutFile) -ErrorAction SilentlyContinue
    if (-not $outPath) {
        $parent = Split-Path -Parent $OutFile
        if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    }
    Set-Content -Path $OutFile -Value $key -Encoding ASCII
    Write-Host "Wrote license file: $OutFile"
}

try {
    Set-Clipboard -Value $key
} catch {
    # ignore
}
