namespace Server.Services;

using System;
using System.Data.SQLite;
using System.Collections.Generic;
using Dapper;
using Shared.Models;

public class DatabaseManager
{
    private string _connectionString;
    
    public DatabaseManager(string dbPath = "Data/billing.db")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath};Version=3;";
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        
        // Table: clients
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS clients (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                ip_address TEXT,
                mac_address TEXT,
                is_online BOOLEAN DEFAULT 0,
                last_seen DATETIME,
                total_sessions INTEGER DEFAULT 0,
                total_spent REAL DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ");
        
        // Table: users
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                email TEXT,
                phone TEXT,
                balance REAL DEFAULT 0,
                points INTEGER DEFAULT 0,
                is_member BOOLEAN DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ");
        
        // Table: sessions
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS sessions (
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
            )
        ");
        
        // Table: payments
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS payments (
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
            )
        ");
        
        // Table: pricing
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS pricing (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                rate_per_hour REAL NOT NULL,
                duration_minutes INTEGER,
                is_package BOOLEAN DEFAULT 0,
                discount_percentage REAL DEFAULT 0,
                is_active BOOLEAN DEFAULT 1,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ");
        
        // Insert default pricing
        conn.Execute(@"
            INSERT OR IGNORE INTO pricing (id, name, rate_per_hour, duration_minutes, is_package) 
            VALUES 
            (1, 'Regular', 5000, 60, 0),
            (2, 'Paket 3 Jam', 12000, 180, 1),
            (3, 'Paket Malam', 15000, 420, 1)
        ");
        
        // Table: logs
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS logs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                level TEXT NOT NULL,
                message TEXT NOT NULL,
                source TEXT,
                timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ");
    }
    
    // Client operations
    public void UpsertClient(Client client)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Execute(@"
            INSERT INTO clients (id, name, ip_address, is_online, last_seen)
            VALUES (@Id, @Name, @IpAddress, @IsOnline, @LastSeen)
            ON CONFLICT(id) DO UPDATE SET
                name = @Name,
                ip_address = @IpAddress,
                is_online = @IsOnline,
                last_seen = @LastSeen
        ", client);
    }
    
    public List<Client> GetAllClients()
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.Query<Client>("SELECT * FROM clients ORDER BY name").AsList();
    }
    
    // Session operations
    public int SaveSession(Session session)
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.QuerySingle<int>(@"
            INSERT INTO sessions (client_id, user_id, start_time, end_time, 
                duration_minutes, rate_per_hour, total_cost, payment_method, status)
            VALUES (@ClientId, @UserId, @StartTime, @EndTime, @DurationMinutes, 
                @RatePerHour, @TotalCost, @PaymentMethod, @Status);
            SELECT last_insert_rowid();
        ", session);
    }
    
    public void UpdateSession(Session session)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Execute(@"
            UPDATE sessions SET
                end_time = @EndTime,
                duration_minutes = @DurationMinutes,
                total_cost = @TotalCost,
                status = @Status
            WHERE id = @Id
        ", session);
    }
    
    public List<Session> GetTodaySessions()
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.Query<Session>(@"
            SELECT * FROM sessions 
            WHERE DATE(start_time) = DATE('now')
            ORDER BY start_time DESC
        ").AsList();
    }
    
    // Revenue
    public decimal GetDailyRevenue()
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.QuerySingleOrDefault<decimal>(@"
            SELECT COALESCE(SUM(total_cost), 0) 
            FROM sessions 
            WHERE DATE(start_time) = DATE('now')
        ");
    }
    
    public decimal GetMonthlyRevenue()
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.QuerySingleOrDefault<decimal>(@"
            SELECT COALESCE(SUM(total_cost), 0) 
            FROM sessions 
            WHERE strftime('%Y-%m', start_time) = strftime('%Y-%m', 'now')
        ");
    }
    
    // Pricing
    public List<Pricing> GetActivePricing()
    {
        using var conn = new SQLiteConnection(_connectionString);
        return conn.Query<Pricing>("SELECT * FROM pricing WHERE is_active = 1").AsList();
    }
}