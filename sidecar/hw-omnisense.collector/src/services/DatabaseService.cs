using Microsoft.Data.Sqlite;
using HwOmniSense.Collector.Models;

namespace HwOmniSense.Collector.Services;

public class DatabaseService
{
    private const string DbName = "hwsense.db";
    private string _connectionString;

    public DatabaseService()
    {
        // Define o caminho do DB para rodar junto com o executável
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbName);
        _connectionString = $"Data Source={dbPath}";
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Cria a tabela se não existir
        // Salvamos: Data, Temps, Loads e Potência
        string createTableCmd = @"
            CREATE TABLE IF NOT EXISTS telemetry (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                gpu_temp REAL,
                gpu_load REAL,
                cpu_temp REAL,
                cpu_load REAL,
                cpu_watts REAL,
                is_gaming INTEGER
            );
            
            -- Cria um índice para as consultas de data ficarem rápidas
            CREATE INDEX IF NOT EXISTS idx_timestamp ON telemetry(timestamp);
        ";

        using var command = new SqliteCommand(createTableCmd, connection);
        command.ExecuteNonQuery();
    }

    public void SaveTelemetry(HardwareTelemetry data)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string insertCmd = @"
            INSERT INTO telemetry (timestamp, gpu_temp, gpu_load, cpu_temp, cpu_load, cpu_watts, is_gaming)
            VALUES (@time, @gt, @gl, @ct, @cl, @cw, @ig)";

        using var command = new SqliteCommand(insertCmd, connection);
        
        // Parâmetros (Evita SQL Injection e erros de formatação)
        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@gt", data.GpuTemp);
        command.Parameters.AddWithValue("@gl", data.GpuLoad);
        command.Parameters.AddWithValue("@ct", data.CpuTemp);
        command.Parameters.AddWithValue("@cl", data.CpuLoad);
        command.Parameters.AddWithValue("@cw", data.CpuWatts);
        
        // Se a carga da GPU for alta (>80%), marcamos como 'Jogando' (1)
        command.Parameters.AddWithValue("@ig", data.GpuLoad > 80 ? 1 : 0);

        command.ExecuteNonQuery();
    }

    // Limpeza Manual
    public void ClearAllHistory()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM telemetry; VACUUM;", connection); // VACUUM compacta o arquivo
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