using System.Text.Json;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using HwOmniSense.Collector.Monitors;
using HwOmniSense.Collector.Models;
using HwOmniSense.Collector.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var current = Process.GetCurrentProcess();
        var others = Process.GetProcessesByName(current.ProcessName);

        foreach (var p in others)
        {
            if (p.Id != current.Id)
            {
                try 
                { 
                    Console.WriteLine($"[INFO] Matando instância duplicada antiga (PID: {p.Id})...");
                    p.Kill();
                    p.WaitForExit(1000);
                } 
                catch { }
            }
        }

        Console.WriteLine("=== HW OmniSense | Backend Collector v2.1 ===");

        using MsiMonitor msi = new MsiMonitor();
        DatabaseService db = new DatabaseService();

        Console.WriteLine($"[DB] Caminho do Banco: {db.DatabasePath}");

        try { db.Initialize(); } catch (Exception ex) { Console.WriteLine($"[ERRO DB INIT]: {ex.Message}"); }

        string msiPath = @"C:\Program Files (x86)\MSI Afterburner\MSIAfterburner.exe";

        foreach (var arg in args)
        {
            if (arg == "--clean")
            {
                Console.WriteLine("[INFO] Comando --clean recebido.");
                try 
                { 
                    SqliteConnection.ClearAllPools();
                    
                    if (File.Exists(db.DatabasePath)) 
                    {
                        File.Delete(db.DatabasePath);
                        Console.WriteLine($"[SUCESSO] Banco deletado em: {db.DatabasePath}");
                    }
                    
                    db.Initialize();
                } 
                catch (Exception ex) { Console.WriteLine($"[ERRO AO LIMPAR]: {ex.Message}"); }
            }
            else if (arg.EndsWith(".exe") && File.Exists(arg))
            {
                msiPath = arg;
                Console.WriteLine($"[CONFIG] Caminho do Afterburner definido pelo usuário: {msiPath}");
            }
        }

        if (!Process.GetProcessesByName("MSIAfterburner").Any())
        {
            if (File.Exists(msiPath))
            {
                try 
                {
                    Process.Start(msiPath);
                    Console.WriteLine("[INFO] Iniciando MSI Afterburner...");
                    Thread.Sleep(8000); 
                }
                catch { Console.WriteLine("[ERRO] Falha ao abrir Afterburner."); }
            }
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
                    var data = msi.ReadData();

                    // CPU
                    if (data.ContainsKey("CPU temperature")) telemetry.CpuTemp = data["CPU temperature"];
                    if (data.ContainsKey("CPU usage")) telemetry.CpuLoad = data["CPU usage"];
                    if (data.ContainsKey("CPU power")) telemetry.CpuWatts = data["CPU power"];
                    if (data.ContainsKey("CPU clock")) telemetry.CpuMhz = data["CPU clock"];

                    // GPU
                    if (data.ContainsKey("GPU temperature")) telemetry.GpuTemp = data["GPU temperature"];
                    if (data.ContainsKey("GPU usage")) telemetry.GpuLoad = data["GPU usage"];
                    if (data.ContainsKey("Core clock")) telemetry.GpuMhz = data["Core clock"];
                    if (data.ContainsKey("Fan tachometer")) telemetry.FanRpm = data["Fan tachometer"];

                    // RAM
                    if (data.ContainsKey("RAM usage")) telemetry.RamLoad = data["RAM usage"];

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
    }
}