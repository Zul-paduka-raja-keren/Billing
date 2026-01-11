# Warnet Billing System - API Documentation

## TCP Network Protocol

### Message Format

All messages use JSON format over TCP socket:

```json
{
  "Type": "MESSAGE_TYPE",
  "Data": { /* payload object */ }
}
```

---

## Server → Client Messages

### 1. LOCK
Locks the client screen

```json
{
  "Type": "LOCK",
  "Data": null
}
```

### 2. UNLOCK
Unlocks client and starts session

```json
{
  "Type": "UNLOCK",
  "Data": {
    "remaining": 3600  // seconds
  }
}
```

### 3. TIME_UPDATE
Updates remaining time (sent every 5 seconds)

```json
{
  "Type": "TIME_UPDATE",
  "Data": {
    "remaining": 3595
  }
}
```

### 4. SHUTDOWN
Shuts down client PC

```json
{
  "Type": "SHUTDOWN",
  "Data": null
}
```

### 5. LOGIN_SUCCESS / LOGIN_FAILED
Response to client login

```json
{
  "Type": "LOGIN_SUCCESS",
  "Data": {
    "message": "Welcome, user!"
  }
}
```

---

## Client → Server Messages

### 1. HEARTBEAT
Periodic ping (every 10 seconds)

```json
{
  "Type": "HEARTBEAT",
  "Data": {
    "timestamp": "2026-01-11T10:30:00",
    "status": "alive"
  }
}
```

### 2. CLIENT_INFO
Initial connection info

```json
{
  "Type": "CLIENT_INFO",
  "Data": {
    "name": "PC-01",
    "ip": "192.168.1.101"
  }
}
```

### 3. CLIENT_LOGIN
User login request

```json
{
  "Type": "CLIENT_LOGIN",
  "Data": {
    "client_id": "PC-01",
    "username": "john_doe",
    "password": "****",
    "is_guest": false,
    "ip_address": "192.168.1.101"
  }
}
```

### 4. IDLE_DETECTED
User idle notification

```json
{
  "Type": "IDLE_DETECTED",
  "Data": {
    "idle_duration": 600  // seconds
  }
}
```

---

## Database API

### Client Operations

```csharp
// Insert or update client
void UpsertClient(Client client)

// Get all clients
List<Client> GetAllClients()
```

### Session Operations

```csharp
// Create new session
int SaveSession(Session session)

// Update existing session
void UpdateSession(Session session)

// Get today's sessions
List<Session> GetTodaySessions()
```

### Revenue Operations

```csharp
// Get daily revenue
decimal GetDailyRevenue()

// Get monthly revenue
decimal GetMonthlyRevenue()
```

### Pricing Operations

```csharp
// Get active pricing packages
List<Pricing> GetActivePricing()
```

---

## Payment Integration (Midtrans)

### Create Payment

**Endpoint:** `POST https://api.midtrans.com/v2/charge`

**Headers:**
```
Authorization: Basic {Base64(ServerKey:)}
Content-Type: application/json
```

**Request:**
```json
{
  "transaction_details": {
    "order_id": "ORDER-20260111-PC01",
    "gross_amount": 10000
  },
  "payment_type": "qris",
  "qris": {
    "acquirer": "gopay"
  }
}
```

**Response:**
```json
{
  "status_code": "201",
  "transaction_id": "...",
  "actions": [
    {
      "name": "generate-qr-code",
      "url": "https://api.midtrans.com/qr/..."
    }
  ]
}
```

### Webhook Notification

**Endpoint:** `POST https://your-server.com/webhook/midtrans`

**Payload:**
```json
{
  "order_id": "ORDER-20260111-PC01",
  "transaction_status": "settlement",
  "gross_amount": "10000",
  "signature_key": "..."
}
```

---

## Error Handling

All API operations should implement try-catch:

```csharp
try
{
    await _tcpClient.SendMessageAsync(message);
}
catch (Exception ex)
{
    Logger.Error($"Failed: {ex.Message}");
    // Handle error
}
```