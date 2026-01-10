using System.Data.SQLite;
using Dapper;

public class DatabaseManager
{
    private string _connectionString;
    
    public DatabaseManager(string dbPath = "billing.db")
    {
        _connectionString = $"Data Source={dbPath};Version=3;";
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using (var conn = new SQLiteConnection(_connectionString))
        {
            conn.Open();
            
            // Table clients
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS clients (
                    id TEXT PRIMARY KEY,
                    name TEXT,
                    ip_address TEXT,
                    last_seen DATETIME
                )
            ");
            
            // Table sessions
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS sessions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    client_id TEXT,
                    start_time DATETIME,
                    end_time DATETIME,
                    duration_minutes INTEGER,
                    total_cost REAL,
                    FOREIGN KEY(client_id) REFERENCES clients(id)
                )
            ");
            
            // Table pricing
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS pricing (
                    id INTEGER PRIMARY KEY,
                    name TEXT,
                    rate_per_hour REAL,
                    duration_minutes INTEGER
                )
            ");
            
            // Insert default pricing
            conn.Execute(@"
                INSERT OR IGNORE INTO pricing VALUES 
                (1, 'Regular', 5000, 60),
                (2, 'Paket 3 Jam', 12000, 180),
                (3, 'Paket Malam', 15000, 420)
            ");
        }
    }
    
    public void SaveSession(ActiveSession session)
    {
        using (var conn = new SQLiteConnection(_connectionString))
        {
            conn.Execute(@"
                INSERT INTO sessions (client_id, start_time, end_time, duration_minutes, total_cost)
                VALUES (@ClientId, @StartTime, @EndTime, @Duration, @Cost)
            ", new
            {
                session.ClientId,
                session.StartTime,
                EndTime = DateTime.Now,
                Duration = (DateTime.Now - session.StartTime).TotalMinutes,
                Cost = session.TotalCost
            });
        }
    }
    
    public decimal GetDailyRevenue()
    {
        using (var conn = new SQLiteConnection(_connectionString))
        {
            return conn.QuerySingle<decimal>(@"
                SELECT COALESCE(SUM(total_cost), 0) 
                FROM sessions 
                WHERE DATE(start_time) = DATE('now')
            ");
        }
    }
}