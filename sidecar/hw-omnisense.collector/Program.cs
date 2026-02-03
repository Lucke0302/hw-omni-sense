using System.Text.Json;

Console.WriteLine("=== HW OmniSense | MSI Afterburner Client (Discovery Mode) ===");

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
    Console.WriteLine("[AVISO] MSI Afterburner não encontrado. Certifique-se que ele está aberto.");
}

while (true)
{
    var telemetry = new HardwareTelemetry();

    if (msiConnected)
    {
        try
        {
            var dados = msi.ReadData();


            Console.WriteLine("\n--- SENSORES ENCONTRADOS ---");
            foreach (var sensor in dados)
            {
                Console.WriteLine($"'{sensor.Key}': {sensor.Value}");
            }
            Console.WriteLine("----------------------------\n");

            foreach (var kvp in dados)
            {
                string key = kvp.Key.ToLower();

                if (key.Contains("gpu") && key.Contains("temp") && !key.Contains("limit")) 
                    telemetry.GpuTemp = kvp.Value;
                
                if (key.Contains("gpu") && (key.Contains("usage") || key.Contains("uso"))) 
                    telemetry.GpuLoad = kvp.Value;

                if (key.Contains("cpu") && key.Contains("temp")) 
                    telemetry.CpuTemp = kvp.Value;
                    
                if (key.Contains("cpu") && (key.Contains("usage") || key.Contains("uso") || key.Contains("total"))) 
                    telemetry.CpuLoad = kvp.Value;
            }
            
            telemetry.GpuName = "MSI Afterburner Source";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler dados: {ex.Message}");
            msiConnected = false;
        }
    }
    else
    {
        try { msi.Connect(); msiConnected = true; } catch { Thread.Sleep(1000); }
    }

    string json = JsonSerializer.Serialize(telemetry);
    Console.WriteLine($">>> JSON FINAL: {json}");

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