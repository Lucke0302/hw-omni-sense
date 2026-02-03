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

        MAHM_SHARED_MEMORY_HEADER header;
        _accessor.Read(0, out header);

        Console.WriteLine($"[MSI DEBUG] Assinatura: {header.Signature:X}");
        
        if (header.Signature != 0x4D41484D && header.Signature != 0xDEADBEEF)
        {
            Console.WriteLine("[ERRO] Assinatura inválida.");
            return result;
        }

        if (header.EntryCount == 0) return result;

        byte[] buffer = new byte[header.EntrySize];

        for (int i = 0; i < header.EntryCount; i++)
        {
            long offset = header.HeaderSize + (i * header.EntrySize);
            
            _accessor.ReadArray(offset, buffer, 0, buffer.Length);

            MAHM_SHARED_MEMORY_ENTRY entry = BytesToStruct<MAHM_SHARED_MEMORY_ENTRY>(buffer);

            string cleanName = entry.SrcName.Trim('\0');
            
            if (!string.IsNullOrEmpty(cleanName))
            {
                result[cleanName] = entry.Data;
            }
        }

        return result;
    }

    private static T BytesToStruct<T>(byte[] data) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}