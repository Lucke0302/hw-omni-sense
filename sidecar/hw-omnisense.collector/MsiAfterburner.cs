using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

public class MsiMonitor : IDisposable
{
    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;
    private const string MapName = "MAHMSharedMemory";

    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MAHM_SHARED_MEMORY_HEADER
    {
        public uint Signature;
        public uint Version;
        public uint HeaderSize;
        public uint EntryCount;
        public uint EntrySize;
        public uint Time;
        public uint GpuEntryCount;
        public uint GpuEntrySize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MAHM_SHARED_MEMORY_ENTRY
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string SrcName; 
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string SrcUnits;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string LocalizedSrcName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string LocalizedSrcUnits;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string RecommendedFormat;
        public float Data;
        public float MinLimit;
        public float MaxLimit;
        public uint Flags;
        public uint Gpu;
        public uint SrcId;
    }

    public void Connect()
    {
        try
        {
            _mmf = MemoryMappedFile.OpenExisting(MapName);
            _accessor = _mmf.CreateViewAccessor();
        }
        catch (FileNotFoundException)
        {
            throw new Exception("MSI Afterburner não está rodando.");
        }
    }

    public Dictionary<string, float> ReadData()
    {
        var result = new Dictionary<string, float>();

        if (_accessor == null) return result;

        // Tenta ler o cabeçalho
        MAHM_SHARED_MEMORY_HEADER header;
        _accessor.Read(0, out header);

        Console.WriteLine($"[MSI DEBUG] Assinatura: {header.Signature:X} (Esperado: DEADBEEF)");
        Console.WriteLine($"[MSI DEBUG] Versão: {header.Version} | Sensores: {header.EntryCount}");
        Console.WriteLine($"[MSI DEBUG] Tamanho Header: {header.HeaderSize} | Tamanho Entrada: {header.EntrySize}");

        if (header.Signature != 0xDEADBEEF) 
        {
            Console.WriteLine("[ERRO CRÍTICO] Assinatura de memória inválida! O Afterburner pode estar em outra versão ou travado.");
            return result;
        }

        if (header.EntryCount == 0)
        {
            Console.WriteLine("[AVISO] MSI Afterburner conectado, mas reportando ZERO sensores.");
            Console.WriteLine("DICA: Abra as configurações do Afterburner > Monitoramento e garanta que os gráficos estão ativos.");
            return result;
        }

        for (int i = 0; i < header.EntryCount; i++)
        {
            long offset = header.HeaderSize + (i * header.EntrySize);
            
            MAHM_SHARED_MEMORY_ENTRY entry;
            _accessor.Read(offset, out entry);

            string cleanName = entry.SrcName.Trim('\0');
            
            if (!string.IsNullOrEmpty(cleanName))
            {
                result[cleanName] = entry.Data;
            }
        }

        return result;
    }

    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}