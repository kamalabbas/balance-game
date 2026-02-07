param(
    [Parameter(Mandatory = $true)]
    [string] $ActivationKey,

    [Parameter(Mandatory = $false)]
    [string] $MachineCode
)

$ErrorActionPreference = 'Stop'

function Base64UrlDecode([string] $text) {
    $padded = $text.Replace('-', '+').Replace('_', '/')
    switch ($padded.Length % 4) {
        2 { $padded += '==' }
        3 { $padded += '=' }
    }
    return [Convert]::FromBase64String($padded)
}

if ([string]::IsNullOrWhiteSpace($ActivationKey)) {
    throw 'ActivationKey is empty.'
}

$ActivationKey = $ActivationKey.Trim()
$parts = $ActivationKey.Split('.')
if ($parts.Length -ne 3) {
    throw 'Activation key format invalid (expected 3 dot-separated parts).'
}

$prefix = $parts[0]
$payloadB64u = $parts[1]

$payloadBytes = Base64UrlDecode $payloadB64u
$payloadJson = [System.Text.Encoding]::UTF8.GetString($payloadBytes)

Write-Host "Prefix : $prefix"
Write-Host "Payload : $payloadJson"

try {
    $payload = $payloadJson | ConvertFrom-Json
    Write-Host "Product : $($payload.product)"
    Write-Host "Machine : $($payload.machine)"
    Write-Host "IssuedAt: $($payload.issuedAt)"
    Write-Host "Expires : $($payload.expiresAt)"
} catch {
    Write-Host 'Payload is not valid JSON.'
}

if (-not [string]::IsNullOrWhiteSpace($MachineCode)) {
    $MachineCodeNorm = ($MachineCode -replace '\s','').ToUpperInvariant()
    Write-Host "MachineCode (input) : $MachineCodeNorm"
    try {
        if ($payload.machine -ne $MachineCodeNorm) {
            Write-Host 'MISMATCH: payload.machine != MachineCode'
        } else {
            Write-Host 'MATCH: payload.machine == MachineCode'
        }
    } catch {
        # ignore
    }
}
