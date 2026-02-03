using LibreHardwareMonitor.Hardware;
using System.Security.Principal;
using System.Management;
using System.Runtime.Versioning;
using System.Text.Json; // Importante para o JSON final

if (!IsAdministrator())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERRO CRÍTICO: Execute como ADMINISTRADOR.");
    Console.ResetColor();
    return;
}

var computer = new Computer
{
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMemoryEnabled = true,
    IsMotherboardEnabled = true, 
    IsControllerEnabled = true,
    IsStorageEnabled = true
};

try { computer.Open(); } catch {}

Console.WriteLine("=== HW OmniSense | Backend Collector v1.3 (ADL Priority) ===");

Random random = new Random();

while (true)
{
    Console.Clear();
    Console.WriteLine($"=== Leitura em: {DateTime.Now} ===\n");

    var telemetryData = new HardwareTelemetry();

    var amdData = AmdGpuSensor.GetGpuData();
    if (amdData.Temp > 0)
    {
        telemetryData.GpuName = amdData.Name;
        telemetryData.GpuTemp = amdData.Temp;
        telemetryData.GpuHotSpot = amdData.Temp + 12;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[ADL] Sucesso: {amdData.Name} @ {amdData.Temp}°C");
        Console.ResetColor();
    }

    foreach (var hardware in computer.Hardware)
    {
        readHardware(hardware, telemetryData); 
    }
    
    // 4. FALLBACK PARA GPU (Se ADL e LHM falharem)
    if (telemetryData.GpuTemp == 0)
    {
        telemetryData.GpuTemp = 50.0f;
        Console.WriteLine("[AVISO] GPU sem leitura. Usando Mock.");
    }

    Thread.Sleep(2000); // 2 segundos
}


static void readHardware(IHardware hardware, HardwareTelemetry data)
{
    try { hardware.Update(); } catch {}

    Console.WriteLine($"\n>>> {hardware.Name} ({hardware.HardwareType}) <<<");

    foreach (var sensor in hardware.Sensors)
    {
        float valor = sensor.Value.GetValueOrDefault();
        
        if (hardware.HardwareType == HardwareType.Cpu)
        {
            data.CpuName = hardware.Name;
            if (sensor.SensorType == SensorType.Temperature) 
            {
                if (valor > data.CpuTemp) data.CpuTemp = valor;
            }
            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total")) 
                data.CpuLoad = valor;
        }

        if (hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuNvidia)
        {
            if (data.GpuTemp == 0 && valor > 0 && sensor.SensorType == SensorType.Temperature)
            {
                data.GpuTemp = valor;
                data.GpuName = hardware.Name;
            }
        }

        if (sensor.SensorType == SensorType.Temperature || sensor.SensorType == SensorType.Load)
        {
             if (valor == 0) Console.ForegroundColor = ConsoleColor.DarkYellow;
             Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {valor:0.0}");
             Console.ResetColor();
        }
    }

    foreach (var subHardware in hardware.SubHardware)
    {
        readHardware(subHardware, data);
    }
}

static bool IsAdministrator()
{
    var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

class HardwareTelemetry
{
    public string GpuName { get; set; } = "Unknown";
    public float GpuTemp { get; set; }
    public float GpuHotSpot { get; set; }
    public float GpuLoad { get; set; }
    
    public string CpuName { get; set; } = "Unknown";
    public float CpuTemp { get; set; }
    public float CpuLoad { get; set; }
}