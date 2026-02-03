namespace HwOmniSense.Collector.Models;

public class HardwareTelemetry
{
    //GPU
    public string GpuName { get; set; } = "Unknown";
    public float GpuTemp { get; set; }
    public float GpuLoad { get; set; }
    public float GpuMhz { get; set; }
    public float FanRpm { get; set; } 
    
    // CPU
    public float CpuTemp { get; set; }
    public float CpuLoad { get; set; }
    public float CpuWatts { get; set; }
    public float CpuMhz { get; set; }
    
    // RAM
    public float RamLoad { get; set; } //mb
    

    public bool IsSimulation { get; set; } = false;
    public string AiSuggestion { get; set; } = ""; // Preparando para o Helper
}