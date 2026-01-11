# Warnet Billing System - Deployment Guide

## Prerequisites

1. Windows 10/11 (64-bit)
2. .NET 8.0 Runtime (download from microsoft.com/dotnet)
3. Network connectivity between server and clients
4. Administrator access for initial setup

## Server Installation

### Automatic Deployment
```batch
# Run from project root
Scripts\deploy-server.bat
```

### Manual Installation
1. Build the project:
   ```batch
   cd Server
   dotnet publish -c Release -r win-x64
   ```

2. Copy output to `C:\WarnetBilling\Server\`

3. Edit `config.json`:
   ```json
   {
     "server": {
       "port": 9999
     },
     "database": {
       "path": "Data/billing.db"
     }
   }
   ```

4. Run `Server.exe`

5. Allow firewall exception when prompted

## Client Installation

### Per-PC Installation
```batch
# On each client PC
Scripts\deploy-client.bat
```

### Mass Deployment (via PowerShell)
```powershell
# Edit Scripts/update-clients.ps1 with your client IPs
.\Scripts\update-clients.ps1
```

### Client Configuration
Edit `config.json` on each client:
```json
{
  "server": {
    "ip": "192.168.1.100",  // Server IP
    "port": 9999
  },
  "client": {
    "name": "PC-01"  // Unique name for each PC
  }
}
```

## Network Setup

1. **Server PC**:
   - Set static IP (e.g., 192.168.1.100)
   - Open port 9999 in firewall:
     ```batch
     netsh advfirewall firewall add rule name="Warnet Billing" dir=in action=allow protocol=TCP localport=9999
     ```

2. **Client PCs**:
   - Can use DHCP
   - Must be on same subnet as server

## Testing

1. Start server on operator PC
2. Check status: Should show "🟢 Server: Running"
3. Start client on test PC
4. Client should appear in server dashboard
5. Try starting a session

## Troubleshooting

### Client can't connect
- Check firewall on server
- Verify IP address in client config.json
- Ping server from client: `ping 192.168.1.100`

### Database errors
- Ensure Data folder has write permissions
- Check disk space

### Performance issues
- Limit to 50 concurrent clients
- Use wired network (not WiFi)

## Backup Strategy

1. **Automatic**: Enabled by default (daily at midnight)
2. **Manual**: Run `Scripts\backup-database.bat`
3. **Location**: `C:\WarnetBilling\Server\Data\Backups\`

## Updates

To update all clients:
```powershell
.\Scripts\update-clients.ps1
```

## Security Recommendations

1. Change default admin password
2. Enable Windows Firewall
3. Use strong WiFi password
4. Regular database backups
5. Keep Windows updated
