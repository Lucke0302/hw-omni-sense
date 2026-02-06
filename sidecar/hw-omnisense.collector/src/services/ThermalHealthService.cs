using System;

namespace HwOmniSense.Collector.Services;

public interface IThermalComponent
{
    string Name { get; }
    float CurrentTemp { get; }
    float Delta { get; }
    float BaselineDelta { get; }
}

public class ThermalHealthService
{
    private const float TEMP_WARNING_THRESHOLD = 85.0f;
    private const float MAX_SAFE_DELTA = 18.0f;
    private const float DEGRADATION_THRESHOLD = 10.0f;

    public (string Status, string Message) Analyze(string componentName, float temp, float currentDelta, float baselineDelta)
    {
        if (currentDelta < 1.0f && temp > TEMP_WARNING_THRESHOLD)
        {
            return ("WARNING", $"Temp alta ({temp:F0}°C) uniforme. Verificar Cooler/Pasta.");
        }

        if (currentDelta > MAX_SAFE_DELTA)
        {
            return ("CRITICAL", $"Delta alto ({currentDelta:F1}°C). Possível má aplicação ou degradação da pasta.");
        }

        if (baselineDelta > 0 && (currentDelta - baselineDelta) > DEGRADATION_THRESHOLD)
        {
            return ("WARNING", $"Degradação detectada (+{(currentDelta - baselineDelta):F1}°C vs Histórico). Planeje troca da pasta.");
        }

        if (temp > 95)
        {
            return ("CRITICAL", "Temperatura crítica! Risco de throttling.");
        }

        return ("OK", "Saudável");
    }
}