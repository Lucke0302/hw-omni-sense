using System.Diagnostics;

namespace HwOmniSense.Collector.Services;

public class StressTestService
{
    private CancellationTokenSource? _cpuToken;
    private CancellationTokenSource? _ramToken;
    private List<byte[]> _ramHog = new();

    // --- CPU STRESS ---
    public void StartCpuStress()
    {
        if (_cpuToken != null && !_cpuToken.IsCancellationRequested) return;

        _cpuToken = new CancellationTokenSource();
        var token = _cpuToken.Token;
        int coreCount = Environment.ProcessorCount;

        Console.WriteLine($"[STRESS] Iniciando estresse de CPU em {coreCount} threads (Prioridade: Abaixo do Normal)...");

        for (int i = 0; i < coreCount; i++)
        {
            Thread t = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    double x = 0.0001;
                    for (int j = 0; j < 10000; j++)
                    {
                        x = Math.Sqrt(x + j) * Math.Sin(x);
                    }

                    Thread.Sleep(0);
                }
            });

            t.IsBackground = true;
            
            t.Priority = ThreadPriority.Lowest; 
            
            t.Start();
        }
    }

    public void StopCpuStress()
    {
        if (_cpuToken != null)
        {
            _cpuToken.Cancel();
            _cpuToken = null;
            Console.WriteLine("[STRESS] CPU Stress parado.");
        }
    }

    // --- RAM STRESS ---
    public void StartRamStress()
    {
        if (_ramToken != null && !_ramToken.IsCancellationRequested) return;

        _ramToken = new CancellationTokenSource();
        var token = _ramToken.Token;
        _ramHog.Clear();

        Console.WriteLine("[STRESS] Iniciando alocação de RAM...");

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] chunk = new byte[100 * 1024 * 1024]; 
                    
                    for (int i = 0; i < chunk.Length; i += 4096) chunk[i] = 1; 
                    
                    _ramHog.Add(chunk);
                    await Task.Delay(500);
                }
                catch (OutOfMemoryException)
                {
                    await Task.Delay(1000);
                }
            }
        }, token);
    }

    public void StopRamStress()
    {
        if (_ramToken != null)
        {
            _ramToken.Cancel();
            _ramToken = null;
            
            _ramHog.Clear();
            GC.Collect();
            Console.WriteLine("[STRESS] RAM liberada.");
        }
    }
}