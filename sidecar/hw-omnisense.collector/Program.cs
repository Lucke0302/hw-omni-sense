using System.Text.Json;

Console.WriteLine("=== HW OmniSense | MSI Afterburner Client ===");

MsiMonitor msi = new MsiMonitor();
bool msiConnected = false;

try
{
    msi.Connect();
    msiConnected = true;
    Console.WriteLine("[INFO] Conectado ao MSI Afterburner com sucesso!");
}
catch
{
    Console.WriteLine("[AVISO] MSI Afterburner não encontrado. Usando SIMULAÇÃO.");
}

Random rng = new Random();
float simTemp = 45.0f;

while (true)
{
    var telemetry = new HardwareTelemetry();

    if (msiConnected)
    {
        try
        {
            var dados = msi.ReadData();
            
            if (dados.ContainsKey("GPU temperature")) telemetry.GpuTemp = dados["GPU temperature"];
            if (dados.ContainsKey("GPU1 temperature")) telemetry.GpuTemp = dados["GPU1 temperature"]; // As vezes vem com número
            
            if (dados.ContainsKey("GPU usage")) telemetry.GpuLoad = dados["GPU usage"];
            if (dados.ContainsKey("GPU1 usage")) telemetry.GpuLoad = dados["GPU1 usage"];
            
            if (dados.ContainsKey("CPU temperature")) telemetry.CpuTemp = dados["CPU temperature"];
            if (dados.ContainsKey("CPU usage")) telemetry.CpuLoad = dados["CPU usage"];
            
            telemetry.GpuName = "MSI Afterburner Source";
        }
        catch
        {
            msiConnected = false;
        }
    }
    else
    {
        simTemp += (float)(rng.NextDouble() * 4 - 2);
        simTemp = Math.Clamp(simTemp, 40, 90);
        
        telemetry.GpuTemp = (float)Math.Round(simTemp, 1);
        telemetry.CpuTemp = (float)Math.Round(simTemp - 10, 1);
        telemetry.GpuLoad = rng.Next(20, 100);
        telemetry.GpuName = "Simulation Mode";
        telemetry.IsSimulation = true;
        
        try { msi.Connect(); msiConnected = true; } catch {}
    }

    string json = JsonSerializer.Serialize(telemetry);
    Console.WriteLine(json);

    Thread.Sleep(10000);
}

class HardwareTelemetry
{
    public string GpuName { get; set; } = "Unknown";
    public float GpuTemp { get; set; }
    public float GpuLoad { get; set; }
    public float CpuTemp { get; set; }
    public float CpuLoad { get; set; }
    public bool IsSimulation { get; set; } = false;
}