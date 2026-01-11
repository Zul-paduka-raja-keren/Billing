# Troubleshooting Guide

Common issues and solutions for Warnet Billing System.

---

## 🔴 Client Can't Connect

### Symptoms
- Client shows "Terputus dari server"
- "Server: Disconnected" status

### Solutions

1. **Check Server Status**
   ```
   - Pastikan Server.exe running di operator PC
   - Check status di dashboard: harus "🟢 Server: Running"
   ```

2. **Verify IP Configuration**
   ```
   - Buka Client/config.json
   - Pastikan "ip" sesuai dengan IP server
   - Test: ping [SERVER_IP] dari client PC
   ```

3. **Firewall Check**
   ```batch
   # Di server PC, buka port 9999
   netsh advfirewall firewall add rule name="Warnet Billing" dir=in action=allow protocol=TCP localport=9999
   ```

4. **Network Issues**
   ```
   - Pastikan client & server di subnet yang sama
   - Check kabel LAN / WiFi connection
   - Restart router jika perlu
   ```

---

## 🔴 Database Locked

### Symptoms
- Error: "Database is locked"
- Server crash saat save data

### Solutions

1. **Close Duplicate Instances**
   ```
   - Buka Task Manager
   - End semua process "Server.exe"
   - Hapus file: billing.db-shm dan billing.db-wal
   ```

2. **Check File Permissions**
   ```
   - Right-click billing.db > Properties > Security
   - Pastikan user punya Full Control
   ```

3. **Restore from Backup**
   ```batch
   # Jika database corrupt
   cd Data\Backups
   copy billing_[latest].db ..\billing.db
   ```

---

## 🔴 Session Not Starting

### Symptoms
- Click "Start" tapi client tetap locked
- No response dari client

### Solutions

1. **Check Client Logs**
   ```
   - Buka: Client\Logs\app_YYYY-MM-DD.log
   - Cari error message
   ```

2. **Restart Client**
   ```
   - Close Client.exe
   - Restart dari desktop shortcut
   ```

3. **Check Network Latency**
   ```cmd
   ping [SERVER_IP] -t
   # Latency harus <50ms
   ```

4. **Verify BillingEngine**
   ```
   - Check server logs
   - Pastikan tidak ada exception
   ```

---

## 🔴 High CPU Usage

### Symptoms
- Server.exe menggunakan >50% CPU
- UI freezing / lag

### Solutions

1. **Reduce Client Count**
   ```
   - Max recommended: 50 clients
   - Jika >50, consider upgrade hardware
   ```

2. **Optimize Timer Interval**
   ```csharp
   // Di BillingEngine.cs, ubah dari 1000ms ke 2000ms
   _ticker = new Timer(2000);
   ```

3. **Close Unnecessary Apps**
   ```
   - Task Manager > End unnecessary processes
   - Disable auto-updates
   ```

4. **Database Cleanup**
   ```sql
   -- Hapus log lama (>30 hari)
   DELETE FROM logs WHERE timestamp < datetime('now', '-30 days');
   
   -- Vacuum database
   VACUUM;
   ```

---

## 🔴 Lock Screen Not Working

### Symptoms
- User bisa close lock screen
- Alt+F4 / Windows key masih works

### Solutions

1. **Run as Administrator**
   ```
   - Right-click Client.exe
   - Run as Administrator
   - Set permanent: Properties > Compatibility > Run as admin
   ```

2. **Enable System Locker**
   ```csharp
   // Di config.json (client)
   {
     "features": {
       "disable_task_manager": true,
       "block_keyboard_shortcuts": true
     }
   }
   ```

3. **Registry Fix**
   ```batch
   # Manual disable task manager
   reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System /v DisableTaskMgr /t REG_DWORD /d 1 /f
   ```

---

## 🔴 Payment Integration Failed

### Symptoms
- QRIS tidak muncul
- Error saat generate QR

### Solutions

1. **Check Midtrans Config**
   ```json
   {
     "payment": {
       "midtrans_server_key": "YOUR_KEY_HERE",  // Must be valid
       "is_production": false  // Use sandbox untuk testing
     }
   }
   ```

2. **Test Connectivity**
   ```cmd
   curl https://api.midtrans.com/ping
   ```

3. **Verify Webhook URL**
   ```
   - Pastikan webhook URL accessible dari internet
   - Use ngrok untuk testing: ngrok http 5000
   ```

---

## 🔴 Time Not Updating

### Symptoms
- Timer stuck / tidak countdown
- RemainingTime tidak berubah

### Solutions

1. **Check BillingEngine Status**
   ```csharp
   // Pastikan timer running
   _ticker.Start();
   ```

2. **Verify Network Messages**
   ```
   - Check server logs: "TIME_UPDATE" messages
   - Client harus receive message setiap 5 detik
   ```

3. **Restart Session**
   ```
   - Stop session
   - Start ulang dari dashboard
   ```

---

## 🔴 Auto-Start Not Working

### Symptoms
- Client tidak auto-start saat boot
- Harus manual run Client.exe

### Solutions

1. **Check Registry**
   ```batch
   reg query HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v WarnetClient
   ```

2. **Re-deploy Client**
   ```batch
   Scripts\deploy-client.bat
   ```

3. **Task Scheduler Alternative**
   ```
   - Control Panel > Task Scheduler
   - Create Basic Task
   - Trigger: At log on
   - Action: Start Client.exe
   ```

---

## 🔴 Reports Not Loading

### Symptoms
- Empty data grid
- Revenue shows Rp 0

### Solutions

1. **Check Database Connection**
   ```csharp
   // Test query di SQLite browser
   SELECT * FROM sessions;
   ```

2. **Verify Date Filter**
   ```sql
   -- Manual query
   SELECT * FROM sessions WHERE DATE(start_time) = DATE('now');
   ```

3. **Rebuild Database**
   ```
   - Backup current billing.db
   - Delete billing.db
   - Restart server (will recreate schema)
   - Restore data from backup
   ```

---

## 📞 Getting Help

Jika masih ada masalah:

1. **Collect Logs**
   ```
   - Server/Logs/app_*.log
   - Server/Logs/errors_*.log
   - Client/Logs/app_*.log
   ```

2. **System Info**
   ```
   - Windows version
   - .NET version: dotnet --version
   - RAM & CPU specs
   - Network topology
   ```

3. **Contact Support**
   - Email: support@warnetbilling.com
   - GitHub Issues: [link]
   - Discord: [link]

---

## 🛡️ Prevention Tips

1. ✅ Regular database backups (automated)
2. ✅ Update Windows & .NET runtime
3. ✅ Use static IP untuk server
4. ✅ Monitor logs daily
5. ✅ Test payment integration di sandbox dulu
6. ✅ Keep client count ≤50
7. ✅ Restart server weekly
8. ✅ Clean database monthly