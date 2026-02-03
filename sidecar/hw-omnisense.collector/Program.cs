using LibreHardwareMonitor.Hardware;
using System.Security.Principal;
using System.Management;
using System.Runtime.Versioning;

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

computer.Open();

Console.WriteLine("=== HW OmniSense | Backend Collector v1.2 (WMI Fallback) ===");
Console.WriteLine("Pressione Ctrl+C para parar.");

while (true)
{
    Console.Clear();
    Console.WriteLine($"=== Leitura em: {DateTime.Now} ===\n");

    var telemetryData = new HardwareTelemetry();

    foreach (var hardware in computer.Hardware)
    {
        readHardware(hardware, telemetryData); 
    }

    if (telemetryData.GpuTemp == 0)
    {
        Console.WriteLine("\n[AVISO] Leitura direta bloqueada. Tentando via WMI...");
        
        float wmiTemp = readHardwareWMI();
        
        if (wmiTemp > 0)
        {
            telemetryData.GpuTemp = wmiTemp;
            telemetryData.GpuName = "GPU/System (Recuperado via WMI)";
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"SUCCESS: WMI retornou {wmiTemp:0.0}°C");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FALHA: WMI também não retornou dados ou não é suportado.");
            Console.ResetColor();
        }
    }

    Thread.Sleep(20000);
}


static void readHardware(IHardware hardware, HardwareTelemetry data)
{
    hardware.Update();

    Console.WriteLine($"\n>>> {hardware.Name} ({hardware.HardwareType}) <<<");

    foreach (var sensor in hardware.Sensors)
    {
        if (sensor.SensorType == SensorType.Temperature || sensor.SensorType == SensorType.Load || sensor.SensorType == SensorType.Fan)
        {
            string unit = sensor.SensorType switch { SensorType.Temperature => "°C", SensorType.Load => "%", SensorType.Fan => "RPM", _ => "" };
            float valor = sensor.Value.GetValueOrDefault();

            if (hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuIntel)
            {
                data.GpuName = hardware.Name;
                if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core")) data.GpuTemp = valor;
            }

            if (valor == 0)
            {
                 Console.ForegroundColor = ConsoleColor.DarkYellow;
                 Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {valor:0.0}{unit} (Zerado)");
                 Console.ResetColor();
            }
            else
            {
                if (sensor.SensorType == SensorType.Temperature && valor > 80) Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {valor:0.0}{unit}");
                Console.ResetColor();
            }
        }
    }

    foreach (var subHardware in hardware.SubHardware)
    {
        readHardware(subHardware, data);
    }
}

[SupportedOSPlatform("windows")]
static float readHardwareWMI()
{
    try
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

        foreach (ManagementObject obj in searcher.Get())
        {
            double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"]);
            double tempCelsius = (tempKelvin / 10.0) - 273.15;

            if (tempCelsius > 0) return (float)tempCelsius;
        }
    }
    catch (Exception)
    {
    }
    return 0.0f;
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
    public float GpuLoad { get; set; }
}