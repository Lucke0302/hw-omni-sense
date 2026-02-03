using System.Runtime.InteropServices;

public class AmdGpuSensor
{
    private const int ADL_OK = 0;

    [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Main_Control_Create(delegate_ADL_Main_Memory_Alloc callback, int iEnumConnectedAdapters);

    [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Main_Control_Destroy();

    [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

    [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);

    [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string AdapterName; // O nome técnico
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string DisplayName; // O nome "bonito" (Windows)
        public int Present;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADLTemperature
    {
        public int Size;
        public int Temperature; 
    }

    public static (string Name, float Temp) GetGpuData()
    {
        string gpuName = "Unknown AMD GPU";
        float gpuTemp = 0.0f;

        try 
        {
            if (ADL_Main_Control_Create(ADL_Main_Memory_Alloc, 1) != ADL_OK) return (gpuName, gpuTemp);

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

                        {
                            ADLTemperature tempStruct = new ADLTemperature();
                            tempStruct.Size = Marshal.SizeOf(tempStruct);

                            if (ADL_Overdrive5_Temperature_Get(info.AdapterIndex, 0, ref tempStruct) == ADL_OK)
                            {
                                gpuName = info.AdapterName.Trim();
                                gpuTemp = tempStruct.Temperature / 1000.0f;
                                
                                if (gpuTemp > 0) break; 
                            }
                        }
                    }
                }
                Marshal.FreeHGlobal(buffer);
            }

            ADL_Main_Control_Destroy();
        }
        catch 
        {
        }
        
        return (gpuName, gpuTemp);
    }
}