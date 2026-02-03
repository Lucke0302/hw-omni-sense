using LibreHardwareMonitor.Hardware;
using System.Security.Principal;

if (!IsAdministrator())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERRO CRÍTICO: Este programa PRECISA rodar como ADMINISTRADOR para ler temperaturas.");
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

Console.WriteLine("=== HW OmniSense | Backend Collector v1.1 ===");
Console.WriteLine("Pressione Ctrl+C para parar.");

while (true)
{
    Console.Clear();
    Console.WriteLine($"=== Leitura em: {DateTime.Now} ===\n");

    foreach (var hardware in computer.Hardware)
    {
        readHardware(hardware); 
    }

    Thread.Sleep(20000);
}

static void readHardware(IHardware hardware)
{
    hardware.Update();

    Console.WriteLine($"\n>>> {hardware.Name} ({hardware.HardwareType}) <<<");

    foreach (var sensor in hardware.Sensors)
    {
        if (sensor.SensorType == SensorType.Temperature || sensor.SensorType == SensorType.Load || sensor.SensorType == SensorType.Fan)
        {
            string unit = sensor.SensorType switch
            {
                SensorType.Temperature => "°C",
                SensorType.Load => "%",
                SensorType.Fan => "RPM",
                _ => ""
            };

            if (sensor.Value.GetValueOrDefault() == 0)
            {
                 Console.ForegroundColor = ConsoleColor.DarkYellow;
                 Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {sensor.Value.GetValueOrDefault():0.0}{unit} (Zerado)");
                 Console.ResetColor();
            }
            else
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value > 80) Console.ForegroundColor = ConsoleColor.Red;
                
                Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {sensor.Value.GetValueOrDefault():0.0}{unit}");
                Console.ResetColor();
            }
        }
    }

    foreach (var subHardware in hardware.SubHardware)
    {
        readHardware(subHardware);
    }
}

static bool IsAdministrator()
{
    var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}