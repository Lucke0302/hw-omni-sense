using Microsoft.Data.Sqlite;
using HwOmniSense.Collector.Models;
using System;
using System.IO;

namespace HwOmniSense.Collector.Services;

public class DatabaseService
{
    private const string DbName = "hw-omnisense.db"; 
    private string _connectionString;
    
    public string DatabasePath { get; private set; } 

    public DatabaseService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string folder = Path.Combine(appData, "OmniSense");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // 3. Define o caminho final
        DatabasePath = Path.Combine(folder, DbName);
        _connectionString = $"Data Source={DatabasePath}";
    }

    public void Initialize()
    {
        string dir = Path.GetDirectoryName(DatabasePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string createTableCmd = @"
            CREATE TABLE IF NOT EXISTS telemetry (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                gpu_temp REAL,
                gpu_load REAL,
                cpu_temp REAL,
                cpu_load REAL,
                cpu_watts REAL,
                cpu_delta REAL,
                is_gaming INTEGER
            );
            CREATE INDEX IF NOT EXISTS idx_timestamp ON telemetry(timestamp);
        ";

        using var command = new SqliteCommand(createTableCmd, connection);
        command.ExecuteNonQuery();

        try 
        {
            var alterCmd = new SqliteCommand("ALTER TABLE telemetry ADD COLUMN cpu_delta REAL DEFAULT 0", connection);
            alterCmd.ExecuteNonQuery();
        } 
        catch { }
    }

    public void SaveTelemetry(HardwareTelemetry data)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        float delta = 0;
        if (data.CpuCoreTemps.Count > 0)
        {
            float max = data.CpuCoreTemps.Max();
            float avg = data.CpuCoreTemps.Average();
            delta = max - avg;
        }

        string insertCmd = @"
            INSERT INTO telemetry (timestamp, gpu_temp, gpu_load, cpu_temp, cpu_load, cpu_watts, cpu_delta, is_gaming)
            VALUES (@time, @gt, @gl, @ct, @cl, @cw, @cd, @ig)";

        using var command = new SqliteCommand(insertCmd, connection);
        
        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@gt", data.GpuTemp);
        command.Parameters.AddWithValue("@gl", data.GpuLoad);
        command.Parameters.AddWithValue("@ct", data.CpuTemp);
        command.Parameters.AddWithValue("@cl", data.CpuLoad);
        command.Parameters.AddWithValue("@cw", data.CpuWatts);
        command.Parameters.AddWithValue("@ig", data.GpuLoad > 80 ? 1 : 0);
        command.Parameters.AddWithValue("@cd", delta);

        command.ExecuteNonQuery();
    }

    public Dictionary<string, double> GetWeeklyThermalDelta()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = new SqliteCommand(@"
            SELECT strftime('%Y-%W', timestamp) as week, AVG(cpu_delta) as avg_delta
            FROM telemetry 
            WHERE is_gaming = 1 AND timestamp > date('now', '-2 months')
            GROUP BY week
            ORDER BY week ASC", connection);

        var result = new Dictionary<string, double>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string week = reader.GetString(0);
            double delta = reader.GetDouble(1);
            result[week] = delta;
        }
        return result;
    }

    // Limpeza Manual
    public void ClearAllHistory()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM telemetry; VACUUM;", connection);
        command.ExecuteNonQuery();
    }

    // Limpeza de Longo Prazo
    public void PruneOldData(int monthsToKeep = 12)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        string sql = $"DELETE FROM telemetry WHERE timestamp < date('now', '-{monthsToKeep} months')";
        
        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public (double OldAvg, double CurrentAvg) GetHealthComparison()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Média de 3 meses atrás
        var cmdOld = new SqliteCommand(@"
            SELECT AVG(gpu_temp) FROM telemetry 
            WHERE is_gaming = 1 
            AND timestamp BETWEEN date('now', '-90 days') AND date('now', '-83 days')", connection);
        
        // Média dos últimos 7 dias
        var cmdNew = new SqliteCommand(@"
            SELECT AVG(gpu_temp) FROM telemetry 
            WHERE is_gaming = 1 
            AND timestamp > date('now', '-7 days')", connection);

        double oldVal = Convert.ToDouble(cmdOld.ExecuteScalar() is DBNull ? 0 : cmdOld.ExecuteScalar());
        double newVal = Convert.ToDouble(cmdNew.ExecuteScalar() is DBNull ? 0 : cmdNew.ExecuteScalar());

        return (oldVal, newVal);
    }
}