# ✅ **CHECKLIST POST-DEVELOPMENT**

## 📋 **Phase 1: Pre-Deployment Checklist**

### **1. Development Environment Setup**
- [ ] Install Visual Studio 2022 (Community/Professional)
- [ ] Install .NET 8.0 SDK
- [ ] Install Git (for version control)
- [ ] Clone/download project repository
- [ ] Verify all NuGet packages restored successfully

### **2. Build & Compile**
```bash
# Open billing.sln in Visual Studio
# Or via CLI:
cd warnet-billing
dotnet restore
dotnet build -c Release
```
- [ ] Build solution tanpa error
- [ ] Verify output folders exist:
  - `Server/bin/Release/net8.0-windows/`
  - `Client/bin/Release/net8.0-windows/`
- [ ] Test run Server.exe locally
- [ ] Test run Client.exe locally

### **3. Configuration Files**
- [ ] **Server/config.json**
  ```json
  {
    "server": { "port": 9999 },
    "database": { "path": "Data/billing.db" },
    "pricing": { "default_rate_per_hour": 5000 }
  }
  ```
  ✏️ Sesuaikan: Port, harga default

- [ ] **Client/config.json** (untuk setiap PC)
  ```json
  {
    "server": { 
      "ip": "192.168.1.100",  // ⚠️ PENTING: Ganti dengan IP server!
      "port": 9999 
    },
    "client": { "name": "PC-01" }  // ⚠️ Unique untuk tiap PC
  }
  ```
  ✏️ Sesuaikan: Server IP, Client name

---

## 🖥️ **Phase 2: Server Deployment**

### **4. Server PC Setup**
- [ ] Pilih PC untuk server (operator)
- [ ] Pastikan Windows 10/11 (64-bit)
- [ ] Install .NET 8.0 Runtime (jika belum ada SDK)
- [ ] Set **static IP address**
  ```
  Control Panel → Network → Properties → IPv4
  Example: 192.168.1.100
  ```
- [ ] Disable sleep/hibernate mode
- [ ] Pastikan disk space cukup (min 10GB)

### **5. Deploy Server Application**
```batch
# Option 1: Automatic
Scripts\deploy-server.bat

# Option 2: Manual
xcopy /E /I Server\bin\Release\net8.0-windows\publish C:\WarnetBilling\Server\
```
- [ ] Run deployment script
- [ ] Verify files copied to `C:\WarnetBilling\Server\`
- [ ] Edit `C:\WarnetBilling\Server\config.json`
- [ ] Create desktop shortcut (auto by script)

### **6. Firewall Configuration**
```batch
# Allow port 9999
netsh advfirewall firewall add rule name="Warnet Billing" dir=in action=allow protocol=TCP localport=9999
```
- [ ] Run command as Administrator
- [ ] Test firewall rule: `netsh advfirewall firewall show rule name="Warnet Billing"`
- [ ] Disable Windows Defender jika perlu (untuk testing)

### **7. Database Initialization**
- [ ] Run `Server.exe` first time
- [ ] Verify `Data/billing.db` created
- [ ] Check tables created (use DB Browser for SQLite)
- [ ] Verify default pricing inserted
- [ ] Test database write permissions

### **8. Server Testing**
- [ ] Run `Server.exe`
- [ ] Check status: "🟢 Server: Running"
- [ ] Verify port listening: `netstat -an | findstr 9999`
- [ ] Check logs: `Logs/app_YYYY-MM-DD.log`
- [ ] Leave server running

---

## 💻 **Phase 3: Client Deployment**

### **9. Network Planning**
- [ ] Document IP addresses untuk setiap PC:
  ```
  Server:  192.168.1.100
  PC-01:   192.168.1.101
  PC-02:   192.168.1.102
  PC-03:   192.168.1.103
  ... dst
  ```
- [ ] Test ping dari client ke server
- [ ] Pastikan semua PC di subnet yang sama

### **10. Deploy Client Application (Per PC)**
```batch
# On each client PC
Scripts\deploy-client.bat
```
- [ ] Run deployment script on each PC
- [ ] Edit `C:\WarnetBilling\Client\config.json` untuk setiap PC:
- [ ] Set `server.ip` = IP server
- [ ] Set `client.name` = unique name (PC-01, PC-02, dst)
- [ ] Verify auto-start registry key created
- [ ] Test run `Client.exe`

### **11. Client Testing**
- [ ] Run Client.exe on one test PC
- [ ] Check lock screen muncul
- [ ] Check status: "Terhubung ke server"
- [ ] Verify client muncul di server dashboard
- [ ] Test login (guest mode)
- [ ] Check keyboard shortcuts blocked (Alt+F4, Win key)

### **12. Mass Deployment (Optional)**
```powershell
# Edit Scripts/update-clients.ps1 with all client IPs
.\Scripts\update-clients.ps1
```
- [ ] Edit `update-clients.ps1` dengan list IP client
- [ ] Enable file sharing on all client PCs
- [ ] Run PowerShell script as Administrator
- [ ] Verify all clients updated successfully

---

## 🧪 **Phase 4: Testing & Validation**

### **13. Connectivity Testing**
- [ ] Start server
- [ ] Start 2-3 test clients
- [ ] All clients appear in dashboard
- [ ] Check heartbeat (no timeout warnings)
- [ ] Test disconnect/reconnect (stop client, restart)

### **14. Billing Flow Testing**
- [ ] **Start Session**:
  - [ ] Click "▶️ Start" di dashboard
  - [ ] Select pricing package
  - [ ] Client unlocked successfully
  - [ ] Timer counting down
  - [ ] Cost calculating correctly

- [ ] **Pause/Resume**:
  - [ ] Click "⏸️ Pause"
  - [ ] Client locked again
  - [ ] Timer paused
  - [ ] Click "▶️ Resume"
  - [ ] Client unlocked, timer continues

- [ ] **Extend Time**:
  - [ ] Click "⏱️ +30min"
  - [ ] Time added correctly
  - [ ] Client notified

- [ ] **Payment**:
  - [ ] Click "💳 Bayar"
  - [ ] Select Cash
  - [ ] Payment recorded

- [ ] **Stop Session**:
  - [ ] Click "⏹️ Stop"
  - [ ] Client locked
  - [ ] Session saved to database
  - [ ] Revenue updated

### **15. Security Testing**
- [ ] Test keyboard shortcuts blocked:
  - [ ] Alt+F4 → blocked
  - [ ] Windows Key → blocked
  - [ ] Ctrl+Alt+Del → (cannot fully block, tapi minimize impact)
- [ ] Test process monitor (if enabled):
  - [ ] Run cheat engine → auto-killed
- [ ] Test idle detection:
  - [ ] No activity for 10 min → notification
- [ ] Test close button → disabled

### **16. Database Testing**
- [ ] Check sessions recorded correctly
- [ ] Verify revenue calculations
- [ ] Test daily/monthly reports
- [ ] Run manual backup: `Scripts\backup-database.bat`
- [ ] Verify backup file created in `Data/Backups/`

### **17. Performance Testing**
- [ ] Start 10+ concurrent sessions
- [ ] Monitor CPU usage (should be <20%)
- [ ] Monitor RAM usage
- [ ] Check network bandwidth
- [ ] No lag or freezing
- [ ] All timers update smoothly

---

## 🛡️ **Phase 5: Security & Hardening**

### **18. Server Security**
- [ ] Change default admin password (jika ada)
- [ ] Enable Windows Firewall
- [ ] Disable unnecessary services
- [ ] Regular Windows updates
- [ ] Antivirus configured (whitelist Server.exe)

### **19. Client Security**
- [ ] Run as Administrator (auto-start)
- [ ] Disable Task Manager (optional, via registry)
- [ ] Block USB ports (optional, via policy)
- [ ] Configure process blacklist in config.json
- [ ] Regular Windows updates

### **20. Network Security**
- [ ] Change WiFi password (if using WiFi)
- [ ] Hide SSID
- [ ] Use strong WPA2/WPA3
- [ ] Consider MAC address filtering
- [ ] Separate guest network jika ada

---

## 📊 **Phase 6: Operations Setup**

### **21. Backup Strategy**
- [ ] Auto backup enabled (default: daily midnight)
- [ ] Test manual backup
- [ ] Document backup location: `C:\WarnetBilling\Server\Data\Backups\`
- [ ] Setup external backup (USB/Cloud) - **PENTING!**
- [ ] Schedule: Copy backups ke external drive weekly
- [ ] Test restore from backup

### **22. Monitoring Setup**
- [ ] Check logs daily: `Logs/app_*.log`
- [ ] Monitor disk space weekly
- [ ] Review error logs
- [ ] Setup alerts (optional, via email/WhatsApp)

### **23. Operator Training**
- [ ] Train operator on:
  - [ ] Starting/stopping sessions
  - [ ] Payment processing
  - [ ] Handling disconnections
  - [ ] Basic troubleshooting
  - [ ] Daily closing procedure
- [ ] Create operator manual (print dari README.md)
- [ ] Setup support contact (your phone/email)

---

## 📱 **Phase 7: Optional Features**

### **24. Web Dashboard (Optional)**
```bash
cd WebDashboard
dotnet run
# Access: http://SERVER_IP:5000
```
- [ ] Test web dashboard access
- [ ] Check mobile responsive design
- [ ] Note: Feature masih simplified

### **25. Payment Integration (Optional)**
- [ ] Register Midtrans account: https://midtrans.com
- [ ] Get Server Key & Client Key
- [ ] Add to `config.json`:
  ```json
  "payment": {
    "midtrans_server_key": "YOUR_KEY",
    "is_production": false
  }
  ```
- [ ] Test QRIS generation (sandbox mode)
- [ ] Test payment webhook
- [ ] Go live after successful testing

---

## 📝 **Phase 8: Documentation**

### **26. System Documentation**
- [ ] Document all PC names and IPs
- [ ] Document network topology
- [ ] Document pricing packages
- [ ] Create troubleshooting guide for operator
- [ ] Print quick reference card

### **27. Maintenance Schedule**
- [ ] **Daily**:
  - [ ] Check server running
  - [ ] Review logs for errors
  - [ ] Verify auto-backup ran
  
- [ ] **Weekly**:
  - [ ] Copy backups to external drive
  - [ ] Clear old logs (>30 days)
  - [ ] Check disk space
  - [ ] Update clients if needed
  
- [ ] **Monthly**:
  - [ ] Review revenue reports
  - [ ] Database vacuum (cleanup)
  - [ ] Windows updates
  - [ ] Security audit

---

## 🚀 **Phase 9: Go Live!**

### **28. Soft Launch (1-2 weeks)**
- [ ] Start with limited clients (5-10 PC)
- [ ] Monitor closely
- [ ] Collect operator feedback
- [ ] Fix any issues found
- [ ] Adjust pricing if needed

### **29. Full Launch**
- [ ] Deploy to all clients
- [ ] Announce to customers
- [ ] Monitor first week closely
- [ ] Collect customer feedback
- [ ] Optimize based on usage patterns

---

## 🔧 **Phase 10: Ongoing Maintenance**

### **30. Regular Updates**
- [ ] Check for .NET runtime updates
- [ ] Update application (via `update-clients.ps1`)
- [ ] Test updates on 1 PC first
- [ ] Deploy to all PCs
- [ ] Document version changes

### **31. Issue Tracking**
- [ ] Create issue log (Excel/Google Sheets)
- [ ] Track:
  - Date, Issue, PC affected, Resolution, Time
- [ ] Review monthly for patterns
- [ ] Implement preventive measures

---

## ✅ **Final Pre-Launch Checklist**

### **Critical Items:**
- [ ] Server running without errors
- [ ] All clients connected successfully
- [ ] Billing flow tested end-to-end
- [ ] Payment processing works
- [ ] Database backups working
- [ ] Logs being created
- [ ] Operator trained
- [ ] Emergency contact ready
- [ ] Backup server plan (if main fails)

### **Nice to Have:**
- [ ] Web dashboard accessible
- [ ] Payment integration (QRIS/E-wallet)
- [ ] WhatsApp notifications
- [ ] Membership system
- [ ] Customer feedback form

---

## 📞 **Support Contacts**

**Developer Support:**
- 📧 Email: sandis2320@gmail.com
- 🐛 GitHub Issues: [Create Issue](https://github.com/Zul-paduka-raja-keren/warnet-billing/issues)

**Resources:**
- 📚 Docs: Check `/Docs` folder
- 🎥 Tutorial: (coming soon)
- 💬 Community: (Discord/Telegram - coming soon)

---

## 🎉 **Ready to Go Live?**

Jika semua checklist di atas ✅, maka sistem Anda **SIAP PRODUCTION!**

**Good luck with your warnet billing system!** 🚀🎮

---

**Last Updated:** January 11, 2026  
**Version:** 1.0.0  
**Status:** Production Ready ✅