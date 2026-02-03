using System.Text.Json;
using HwOmniSense.Collector.Monitors;
using HwOmniSense.Collector.Models;
using HwOmniSense.Collector.Services;

Console.WriteLine("=== HW OmniSense | Backend Collector v2.0 (Clean Architecture) ===");

using MsiMonitor msi = new MsiMonitor();
DatabaseService db = new DatabaseService();

try 
{ 
    db.Initialize(); 
    Console.WriteLine("[INFO] Banco de Dados SQLite inicializado.");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERRO] Falha no Banco: {ex.Message}");
}
bool msiConnected = false;

try
{
    msi.Connect();
    msiConnected = true;
    Console.WriteLine("[INFO] Conectado ao MSI Afterburner com sucesso!");
}
catch
{
    Console.WriteLine("[AVISO] MSI Afterburner não encontrado. Ativando Modo Simulação.");
}

// Variáveis de Simulação
Random rng = new Random();
float simTemp = 45.0f;

int dbCounter = 0;

while (true)
{
    var telemetry = new HardwareTelemetry();

    if (msiConnected)
    {
        try
        {
            var dados = msi.ReadData();

            // CPU
            if (dados.ContainsKey("CPU temperature")) telemetry.CpuTemp = dados["CPU temperature"];
            if (dados.ContainsKey("CPU usage")) telemetry.CpuLoad = dados["CPU usage"];
            if (dados.ContainsKey("CPU power")) telemetry.CpuWatts = dados["CPU power"];
            if (dados.ContainsKey("CPU clock")) telemetry.CpuMhz = dados["CPU clock"];

            // GPU
            if (dados.ContainsKey("GPU temperature")) telemetry.GpuTemp = dados["GPU temperature"];
            if (dados.ContainsKey("GPU usage")) telemetry.GpuLoad = dados["GPU usage"];
            if (dados.ContainsKey("Core clock")) telemetry.GpuMhz = dados["Core clock"];
            if (dados.ContainsKey("Fan tachometer")) telemetry.FanRpm = dados["Fan tachometer"];

            // RAM
            if (dados.ContainsKey("RAM usage")) telemetry.RamLoad = dados["RAM usage"];

        }
        catch
        {
            msiConnected = false;
        }
    }
    else
    {
        // --- MODO SIMULAÇÃO (Testes) ---
        simTemp += (float)(rng.NextDouble() * 4 - 2);
        simTemp = Math.Clamp(simTemp, 40, 90);
        
        telemetry.GpuTemp = (float)Math.Round(simTemp, 1);
        telemetry.CpuTemp = (float)Math.Round(simTemp - 10, 1);
        telemetry.CpuWatts = rng.Next(15, 65);
        telemetry.GpuLoad = rng.Next(20, 100);
        telemetry.GpuName = "Simulation Mode";
        telemetry.IsSimulation = true;
        
        try { msi.Connect(); msiConnected = true; } catch {}
    }

    dbCounter++;
    if (dbCounter >= 5)
    {
        Task.Run(() => db.SaveTelemetry(telemetry)); 
        dbCounter = 0;
    }

    string json = JsonSerializer.Serialize(telemetry);
    Console.WriteLine(json);

    Thread.Sleep(1000);
}