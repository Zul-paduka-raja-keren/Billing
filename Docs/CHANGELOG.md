# Changelog

All notable changes to Warnet Billing System will be documented here.

## [1.0.0] - 2026-01-11

### Added
- ✅ TCP Server/Client architecture
- ✅ Real-time billing engine
- ✅ Client lock system with keyboard blocking
- ✅ Full-screen lock screen UI
- ✅ Timer display overlay
- ✅ Login screen (guest + member mode)
- ✅ Server dashboard with DataGrid
- ✅ Start/Pause/Resume/Extend session controls
- ✅ Payment dialog (Cash/QRIS/E-wallet UI)
- ✅ Client management window
- ✅ Reports window with daily/monthly stats
- ✅ Pricing management window
- ✅ Settings window
- ✅ SQLite database integration
- ✅ Automatic database backup
- ✅ Heartbeat monitoring
- ✅ Idle detection
- ✅ Process monitoring (blacklist)
- ✅ Network helper utilities
- ✅ Logger system
- ✅ Deployment scripts (BAT & PowerShell)

### Technical Details
- **Framework:** .NET 8.0 + WPF
- **Database:** SQLite 3 with Dapper ORM
- **Network:** TCP Sockets (port 9999)
- **UI:** MaterialDesign + Custom dark theme

---

## [Roadmap v1.1] - Planned

### To Be Added
- [ ] Midtrans payment integration (working)
- [ ] Web Dashboard (Blazor Server)
- [ ] WhatsApp notifications
- [ ] Membership system with discounts
- [ ] Auto top-up via balance
- [ ] Facial recognition login
- [ ] Game hour tracking
- [ ] Multi-language support

### To Be Improved
- [ ] Better error handling
- [ ] Unit tests coverage
- [ ] Performance optimization
- [ ] UI/UX refinements
- [ ] Documentation expansion

---

## [Future v2.0] - Vision

- Multi-location support (cloud sync)
- Mobile app (iOS/Android)
- Advanced analytics & reporting
- AI-powered recommendations
- API for third-party integration