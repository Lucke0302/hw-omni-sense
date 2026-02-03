using System.Runtime.InteropServices;

public class AmdGpuSensor
{
    private const int ADL_OK = 0;

    [DllImport("atiadlxy.dll", EntryPoint = "ADL_Main_Control_Create", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Main_Control_Create64(delegate_ADL_Main_Memory_Alloc callback, int iEnumConnectedAdapters);

    [DllImport("atiadlxx.dll", EntryPoint = "ADL_Main_Control_Create", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Main_Control_Create32(delegate_ADL_Main_Memory_Alloc callback, int iEnumConnectedAdapters);

    [DllImport("atiadlxy.dll", EntryPoint = "ADL_Main_Control_Destroy", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Main_Control_Destroy();

    [DllImport("atiadlxy.dll", EntryPoint = "ADL_Adapter_NumberOfAdapters_Get", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

    [DllImport("atiadlxy.dll", EntryPoint = "ADL_Adapter_AdapterInfo_Get", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);

    [DllImport("atiadlxy.dll", EntryPoint = "ADL_Overdrive5_Temperature_Get", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Overdrive5_Temperature_Get(int adapterIndex, int thermalControllerIndex, ref ADLTemperature temperature);

    private delegate IntPtr delegate_ADL_Main_Memory_Alloc(int size);
    private static IntPtr ADL_Main_Memory_Alloc(int size) => Marshal.AllocHGlobal(size);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ADLAdapterInfo
    {
        public int Size;
        public int AdapterIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)] public string UDID;
        public int BusNumber;
        public int DeviceNumber;
        public int FunctionNumber;
        public int VendorID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string DisplayName;
        public int Present;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADLTemperature { public int Size; public int Temperature; }

    public static (string Name, float Temp) GetGpuData()
    {
        try
        {
            int result = ADL_Main_Control_Create64(ADL_Main_Memory_Alloc, 1);
            if (result != ADL_OK)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[ADL DEBUG] Falha ao iniciar ADL 64-bit. Código: {result}");
                Console.ResetColor();
                return ("Erro ADL Init", 0);
            }
        }
        catch (DllNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[ADL DEBUG] ERRO CRÍTICO: 'atiadlxy.dll' (Driver AMD) não encontrada no Windows.");
            Console.WriteLine("Você tem drivers da AMD instalados neste PC?");
            Console.ResetColor();
            return ("DLL Missing", 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADL DEBUG] Erro desconhecido ao carregar DLL: {ex.Message}");
            return ("Erro DLL", 0);
        }

        try
        {
            int numAdapters = 0;
            ADL_Adapter_NumberOfAdapters_Get(ref numAdapters);
            
            if (numAdapters > 0)
            {
                int size = Marshal.SizeOf(typeof(ADLAdapterInfo));
                IntPtr buffer = Marshal.AllocHGlobal(size * numAdapters);
                
                if (ADL_Adapter_AdapterInfo_Get(buffer, size * numAdapters) == ADL_OK)
                {
                    for (int i = 0; i < numAdapters; i++)
                    {
                        IntPtr currentPtr = new IntPtr(buffer.ToInt64() + (i * size));
                        ADLAdapterInfo info = Marshal.PtrToStructure<ADLAdapterInfo>(currentPtr);

                        if (info.VendorID == 1002 && info.Present != 0)
                        {
                            ADLTemperature tempStruct = new ADLTemperature();
                            tempStruct.Size = Marshal.SizeOf(tempStruct);

                            if (ADL_Overdrive5_Temperature_Get(info.AdapterIndex, 0, ref tempStruct) == ADL_OK)
                            {
                                float temp = tempStruct.Temperature / 1000.0f;
                                if (temp > 0)
                                {
                                    ADL_Main_Control_Destroy();
                                    return (info.AdapterName.Trim(), temp);
                                }
                            }
                        }
                    }
                }
                Marshal.FreeHGlobal(buffer);
            }
            
            ADL_Main_Control_Destroy();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADL DEBUG] Erro durante a leitura: {ex.Message}");
        }

        return ("No AMD GPU", 0);
    }
}