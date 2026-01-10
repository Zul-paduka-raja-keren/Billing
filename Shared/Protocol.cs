public class Message
{
    public string Type { get; set; } // "LOCK", "UNLOCK", "TIME_UPDATE", dll
    public object Data { get; set; }
}

// Message types:
// SERVER -> CLIENT:
// - LOCK: Kunci layar
// - UNLOCK: Buka layar + waktu awal
// - TIME_UPDATE: Update waktu tersisa
// - SHUTDOWN: Matikan PC (opsional)

// CLIENT -> SERVER:
// - HEARTBEAT: Masih hidup
// - IDLE_DETECTED: User idle 10 menit
// - REQUEST_EXTEND: User minta tambah waktu (dari client UI)