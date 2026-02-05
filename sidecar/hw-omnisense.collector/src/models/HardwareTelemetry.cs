namespace HwOmniSense.Collector.Models;

public class HardwareTelemetry
{
    //GPU
    public string GpuName { get; set; } = "Unknown";
    public float GpuTemp { get; set; }
    public float GpuLoad { get; set; }
    public float GpuMhz { get; set; }
    public float GpuVolt { get; set; }
    public float FanRpm { get; set; } 
    
    // CPU
    public float CpuTemp { get; set; }
    public float CpuLoad { get; set; }
    public float CpuWatts { get; set; }
    public float CpuMhz { get; set; }
    public float CpuVolt { get; set; }
    public List<float> CpuCoreTemps { get; set; } = new List<float>();
    public float CpuHotspotDelta { get; set; }
    
    // RAM
    public float RamUsed { get; set; } 
    public float RamTotal { get; set; }
    public float RamMhz { get; set; } 
    public float RamVolt { get; set; }
    public float RamTemp { get; set; } 
    

    public bool IsSimulation { get; set; } = false;
    public string AiSuggestion { get; set; } = ""; 
}