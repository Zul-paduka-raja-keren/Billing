# Database Schema Documentation

## Overview

Database: **SQLite 3**  
File: `billing.db`  
ORM: **Dapper**

---

## Tables

### 1. clients

Stores PC/client information

```sql
CREATE TABLE clients (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    ip_address TEXT,
    mac_address TEXT,
    is_online BOOLEAN DEFAULT 0,
    last_seen DATETIME,
    total_sessions INTEGER DEFAULT 0,
    total_spent REAL DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Columns:**
- `id`: Unique client identifier (e.g., "PC-ABC-1234")
- `name`: Display name (e.g., "PC-01")
- `ip_address`: Client IP (nullable)
- `mac_address`: Client MAC address (nullable)
- `is_online`: Connection status
- `last_seen`: Last heartbeat timestamp
- `total_sessions`: Lifetime session count
- `total_spent`: Lifetime revenue from client
- `created_at`: Registration timestamp

---

### 2. users

Stores member accounts

```sql
CREATE TABLE users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    balance REAL DEFAULT 0,
    points INTEGER DEFAULT 0,
    is_member BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Columns:**
- `id`: Auto-increment user ID
- `username`: Unique username
- `password_hash`: SHA256 hashed password
- `email`: Email address (nullable)
- `phone`: Phone number (nullable)
- `balance`: Account balance (for prepaid)
- `points`: Loyalty points
- `is_member`: Premium member flag
- `created_at`: Registration date

---

### 3. sessions

Billing session records

```sql
CREATE TABLE sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    client_id TEXT NOT NULL,
    user_id INTEGER,
    start_time DATETIME NOT NULL,
    end_time DATETIME,
    duration_minutes INTEGER,
    rate_per_hour REAL,
    total_cost REAL,
    payment_method TEXT DEFAULT 'cash',
    status TEXT DEFAULT 'active',
    notes TEXT,
    FOREIGN KEY(client_id) REFERENCES clients(id),
    FOREIGN KEY(user_id) REFERENCES users(id)
);
```

**Columns:**
- `id`: Auto-increment session ID
- `client_id`: Reference to clients table
- `user_id`: Reference to users table (nullable for guest)
- `start_time`: Session start timestamp
- `end_time`: Session end timestamp (null if active)
- `duration_minutes`: Total duration
- `rate_per_hour`: Hourly rate applied
- `total_cost`: Final cost
- `payment_method`: cash/qris/gopay/ovo
- `status`: active/paused/completed
- `notes`: Additional notes (nullable)

**Status Values:**
- `active`: Currently running
- `paused`: Temporarily paused
- `completed`: Finished and paid

---

### 4. payments

Payment transaction records

```sql
CREATE TABLE payments (
    order_id TEXT PRIMARY KEY,
    client_id TEXT,
    user_id INTEGER,
    amount REAL NOT NULL,
    method TEXT NOT NULL,
    status TEXT DEFAULT 'pending',
    qr_code_url TEXT,
    deep_link TEXT,
    midtrans_transaction_id TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    completed_at DATETIME,
    FOREIGN KEY(client_id) REFERENCES clients(id),
    FOREIGN KEY(user_id) REFERENCES users(id)
);
```

**Columns:**
- `order_id`: Unique order ID (e.g., "ORDER-20260111-PC01")
- `client_id`: Reference to clients
- `user_id`: Reference to users (nullable)
- `amount`: Payment amount in Rupiah
- `method`: cash/qris/gopay/ovo/dana
- `status`: pending/success/failed
- `qr_code_url`: QRIS QR code URL (Midtrans)
- `deep_link`: E-wallet deep link
- `midtrans_transaction_id`: Midtrans reference
- `created_at`: Payment creation time
- `completed_at`: Payment completion time

---

### 5. pricing

Pricing packages

```sql
CREATE TABLE pricing (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    rate_per_hour REAL NOT NULL,
    duration_minutes INTEGER,
    is_package BOOLEAN DEFAULT 0,
    discount_percentage REAL DEFAULT 0,
    is_active BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Default Values:**
```sql
(1, 'Regular', 5000, 60, 0, 0, 1)
(2, 'Paket 3 Jam', 12000, 180, 1, 0, 1)
(3, 'Paket Malam', 15000, 420, 1, 0, 1)
```

---

### 6. logs

System logs

```sql
CREATE TABLE logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    level TEXT NOT NULL,
    message TEXT NOT NULL,
    source TEXT,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Log Levels:**
- `INFO`: General information
- `WARNING`: Warning messages
- `ERROR`: Error messages

---

## Indexes

```sql
CREATE INDEX idx_sessions_client ON sessions(client_id);
CREATE INDEX idx_sessions_date ON sessions(start_time);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_logs_timestamp ON logs(timestamp);
```

---

## Common Queries

### Daily Revenue
```sql
SELECT COALESCE(SUM(total_cost), 0) 
FROM sessions 
WHERE DATE(start_time) = DATE('now');
```

### Monthly Revenue
```sql
SELECT COALESCE(SUM(total_cost), 0) 
FROM sessions 
WHERE strftime('%Y-%m', start_time) = strftime('%Y-%m', 'now');
```

### Top Clients
```sql
SELECT client_id, COUNT(*) as sessions, SUM(total_cost) as revenue
FROM sessions 
GROUP BY client_id 
ORDER BY revenue DESC 
LIMIT 10;
```

### Active Sessions
```sql
SELECT * FROM sessions 
WHERE status = 'active';
```

---

## Backup Strategy

**Location:** `Data/Backups/`  
**Frequency:** Daily (midnight)  
**Retention:** 7 days  
**Format:** `billing_YYYY-MM-DD_HHmmss.db`

**Manual Backup:**
```batch
Scripts\backup-database.bat
```