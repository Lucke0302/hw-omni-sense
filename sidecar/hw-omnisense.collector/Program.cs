using LibreHardwareMonitor.Hardware;

var computer = new Computer
{
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMemoryEnabled = true,
    IsMotherboardEnabled = true 
};

computer.Open();

Console.WriteLine("=== HW OmniSense | Backend Collector v1.0 ===");
Console.WriteLine("Monitorando hardware... (Pressione Ctrl+C para parar)");

while (true)
{
    foreach (var hardware in computer.Hardware)
    {
        hardware.Update();

        Console.WriteLine($"\n>>> {hardware.Name} <<<");

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.Value.HasValue)
            {
                string unit = sensor.SensorType == SensorType.Temperature ? "°C" : (sensor.SensorType == SensorType.Load ? "%" : "");
                Console.WriteLine($"  [{sensor.SensorType}] {sensor.Name}: {sensor.Value.Value:0.0}{unit}");
            }
        }
    }

    Thread.Sleep(20000);
    Console.Clear();
    Console.WriteLine("=== HW OmniSense | Rodando em .NET 9.0 ===");
}