using System.Text.Json;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;
using System.Net;
using System.Text;
using HwOmniSense.Collector.Monitors;
using HwOmniSense.Collector.Models;
using HwOmniSense.Collector.Services;

internal class Program
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX() { this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    
    private static string _latestJsonData = "{}";

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

        Task.Run(() => StartWebServer());

        using MsiMonitor msi = new MsiMonitor();
        DatabaseService db = new DatabaseService();

        Console.WriteLine($"[DB] Caminho do Banco: {db.DatabasePath}");

        try { db.Initialize(); } catch (Exception ex) { Console.WriteLine($"[ERRO DB INIT]: {ex.Message}"); }

        float systemRamTotalMb = 0;
        try 
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                systemRamTotalMb = memStatus.ullTotalPhys / (1024f * 1024f);
                Console.WriteLine($"[INFO] RAM Total Detectada: {systemRamTotalMb:F0} MB");
            }
        }
        catch { systemRamTotalMb = 16384; }

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

        bool hasPrintedKeys = false;

        // Variáveis de Simulação
        Random rng = new Random();
        float simTemp = 45.0f;

        int dbCounter = 0;

        while (true)
        {
            var telemetry = new HardwareTelemetry();
            telemetry.RamTotal = systemRamTotalMb;

            if (msiConnected)
            {
                try
                {
                    var data = msi.ReadData();

                    // LOG DE SENSORES
                    if (!hasPrintedKeys && data.Count > 0)
                    {
                        Console.WriteLine("\n[DEBUG] --- SENSORES ENCONTRADOS NO MSI ---");
                        foreach (var key in data.Keys)
                        {
                            Console.WriteLine($"[SENSOR] Nome: '{key}' | Valor: {data[key]}");
                        }
                        Console.WriteLine("-------------------------------------------\n");
                        hasPrintedKeys = true;
                    }

                    float GetValue(string[] keys) 
                    {
                        foreach(var key in keys) 
                        {
                            if(data.ContainsKey(key)) 
                            {
                                float val = data[key];
                                if (val > 1000000 || val < -1000000) return 0;
                                return val;
                            }
                        }
                        return 0;
                    }

                    // CPU
                    telemetry.CpuTemp = GetValue(new[] { "CPU temperature", "CPU Package", "Package temperature", "CPU" });
                    telemetry.CpuLoad = GetValue(new[] { "CPU usage", "CPU usage, %", "CPU Total" });
                    telemetry.CpuWatts = GetValue(new[] { "CPU power", "Package power", "CPU Package Power" });
                    
                    // CPU Clock
                    telemetry.CpuMhz = GetValue(new[] { "CPU clock", "Core clock", "CPU frequency", "CPU1 clock" });
                    
                    // CPU Volt
                    telemetry.CpuVolt = GetValue(new[] { "CPU voltage", "Vcore", "CPU VCORE", "Voltage", "VID", "Core 0 VID", "CPU Core Voltage" }); 

                    // GPU
                    telemetry.GpuTemp = GetValue(new[] { "GPU temperature", "GPU1 temperature", "GPU" });
                    telemetry.GpuLoad = GetValue(new[] { "GPU usage", "GPU1 usage", "GPU usage, %" });
                    
                    // GPU Clock
                    telemetry.GpuMhz = GetValue(new[] { "GPU clock", "GPU1 clock", "Core clock", "Graphics clock", "GPU Core Clock" });
                    
                    // GPU Volt
                    telemetry.GpuVolt = GetValue(new[] { "GPU voltage", "GPU1 voltage", "VDDC", "GPU Core Voltage", "Power" });

                    // RAM
                    telemetry.RamUsed = GetValue(new[] { "RAM usage", "Memory usage" }); 
                    
                    // RAM Clock
                    telemetry.RamMhz = GetValue(new[] { "RAM clock", "Memory clock", "DRAM Frequency" }); 
                    
                    // RAM Volt
                    telemetry.RamVolt = GetValue(new[] { "RAM voltage", "DRAM Voltage", "DIMM Voltage", "Mem Voltage" });
                    telemetry.RamTemp = GetValue(new[] { "RAM temperature", "DIMM temperature", "DRAM Temp" });
                    
                    telemetry.CpuCoreTemps.Clear();
                    for (int i = 1; i <= 32; i++)
                    {
                        string[] coreKeys = { $"CPU{i} temperature", $"CPU {i} temperature", $"Core{i} temperature" };
                        float coreVal = GetValue(coreKeys);
                        
                        if (coreVal > 0) telemetry.CpuCoreTemps.Add(coreVal);
                    }

                    if (telemetry.CpuCoreTemps.Count == 0 && telemetry.CpuTemp > 0)
                    {
                        telemetry.CpuCoreTemps.Add(telemetry.CpuTemp);
                    }

                    telemetry.IsSimulation = false;
                }
                catch
                {
                    msiConnected = false;
                }
            }
            else
            {
                // --- MODO SIMULAÇÃO (Testes) ---
                telemetry.RamUsed = rng.Next(4000, (int)systemRamTotalMb - 2000);
                telemetry.RamMhz = 3200;
                telemetry.RamTemp = 35 + rng.Next(0, 10);
                telemetry.RamVolt = 1.35f;

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
            _latestJsonData = json;

            Console.WriteLine(json);

            Thread.Sleep(10000);
        }
    }
    private static void StartWebServer()
    {
        try
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:9090/"); 
            listener.Start();
            Console.WriteLine("[SERVER] Servidor Remoto rodando na porta 9090");

            while (true)
            {
                var context = listener.GetContext();
                var response = context.Response;

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Content-Type", "application/json");

                byte[] buffer = Encoding.UTF8.GetBytes(_latestJsonData);
                
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVER ERROR]: {ex.Message}");
            Console.WriteLine("Tente rodar o app como Administrador para liberar a porta 9090.");
        }
    }
}