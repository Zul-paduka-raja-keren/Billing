# Mass update all client PCs via network

$clientIPs = @(
    "192.168.1.101",
    "192.168.1.102",
    "192.168.1.103",
    "192.168.1.104",
    "192.168.1.105"
)

$sourcePath = ".\Client\bin\Release\net8.0-windows\publish"
$destPath = "C$\WarnetBilling\Client"

Write-Host "================================"
Write-Host " Mass Client Update Utility"
Write-Host "================================"
Write-Host ""

foreach ($ip in $clientIPs) {
    Write-Host "Updating $ip..." -ForegroundColor Yellow
    
    try {
        # Test connection
        if (Test-Connection -ComputerName $ip -Count 1 -Quiet) {
            # Copy files
            $remotePath = "\\$ip\$destPath"
            
            if (Test-Path $remotePath) {
                Copy-Item -Path "$sourcePath\*" -Destination $remotePath -Recurse -Force
                Write-Host "✓ $ip updated successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ $ip - Installation path not found" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ $ip - Not reachable" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "✗ $ip - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "================================"
Write-Host " Update Complete!"
Write-Host "================================"
Read-Host "Press Enter to exit"