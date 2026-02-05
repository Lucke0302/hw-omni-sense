import { YStack, XStack, Text, H2, Button, Separator, ScrollView } from "tamagui";
import { COLORS, getLoadColor, getTempColor } from "../utils/status";
import { useMemo } from 'react';

interface DetailPanelProps {
  type: 'CPU' | 'GPU' | 'RAM';
  onBack: () => void;
  data: any;
}

interface CoreData {
  id: number;
  load: number;
  temp: number;
  total?: number;
}

export function DetailPanel({ type, onBack, data }: DetailPanelProps) {
  
  const formatValue = (val: number | undefined | null, unit: string, decimals = 0) => {
    if (val === undefined || val === null || (val === 0 && unit !== "%")) return "--";
    return `${val.toFixed(decimals)} ${unit}`;
  };

  const cores = useMemo(() => {
    if (type === 'CPU' && data?.CpuCoreTemps?.length > 0) {
      return data.CpuCoreTemps.map((temp: number, index: number) => ({
        id: index,
        load: data.CpuLoad, 
        temp: temp
      }));
    }

    if (type === 'RAM') {
        return [{
            id: 0,
            load: data?.RamUsed || 0,
            total: data?.RamTotal || 1,
            temp: data?.RamTemp || 0
        }];
    }

    return [{ id: 0, load: data?.GpuLoad || 0, temp: data?.GpuTemp || 0 }];
  }, [data, type]);

  const delta = useMemo(() => {
    if (type !== 'CPU' || cores.length <= 1) return 0;
    const max = Math.max(...cores.map((c: CoreData) => c.temp));
    const avg = cores.reduce((a: number, b: CoreData) => a + b.temp, 0) / cores.length;
    return max - avg;
  }, [cores, type]);

  return (
    <YStack 
      f={1} 
      className="glass-panel" 
      p="$6" 
      gap="$4" 
      enterStyle={{ opacity: 0, scale: 0.9 }}
      exitStyle={{ opacity: 0, scale: 0.9 }}
      m="$4"
      borderRadius="$6"
      borderWidth={1}
      borderColor="$borderColor"
    >
      <XStack jc="space-between" ai="center">
        <YStack>
          <H2 fontFamily="$heading" color="$color">Detalhes: {type}</H2>
          <Text color="$gray10" fontFamily="$body">
            {type === 'RAM' ? `Total do Sistema: ${(data?.RamTotal / 1024)?.toFixed(1)} GB` : 'Análise Profunda em Tempo Real'}
          </Text>
        </YStack>
        <Button size="$3" theme="red" onPress={onBack}>X Fechar</Button>
      </XStack>

      <Separator borderColor="$gray8" />

      <ScrollView f={1}>
        <XStack flexWrap="wrap" gap="$3" jc="center" pb="$4">
          {cores.map((core: CoreData) => {
            
            const isRam = type === 'RAM';
            
            const displayValue = isRam 
                ? formatValue(core.load, "MB") 
                : formatValue(core.load, "%");
                
            const barWidth = isRam && core.total ? (core.load / core.total) * 100 : core.load;

            return (
              <YStack key={core.id} w={140} p="$3" className="glass-panel" borderWidth={1} borderColor="$gray5" gap="$1">
                
                <XStack jc="space-between" ai="center">
                  <Text color="$gray11" fontWeight="bold" fontSize="$3">
                    {type === 'GPU' ? 'GPU Core' : type === 'RAM' ? 'Módulo RAM' : `Núcleo #${core.id + 1}`}
                  </Text>
                  
                  {type === 'CPU' && core.temp === Math.max(...cores.map((c: CoreData) => c.temp)) && (
                    <Text fontSize={10} color={COLORS.red} fontWeight="bold">HOTSPOT 🔥</Text>
                  )}
                </XStack>

                <Separator my="$2" />

                <XStack jc="space-between">
                  <Text color="$gray10" fontSize="$2">Uso</Text>
                  <Text fontFamily="$tech" color={getLoadColor(isRam ? (core.load / (core.total||1))*100 : core.load)} fontWeight="bold" fontSize={isRam ? 11 : 14}>
                    {displayValue}
                  </Text>
                </XStack>
                
                <XStack jc="space-between">
                  <Text color="$gray10" fontSize="$2">Temp</Text>
                  <Text fontFamily="$tech" color={getTempColor(core.temp)} fontWeight="bold">
                    {formatValue(core.temp, "°C")}
                  </Text>
                </XStack>

                <YStack h={4} w="100%" bc="$gray4" mt="$2" borderRadius="$10" overflow="hidden">
                  <YStack 
                    h="100%" 
                    w={`${Math.min(barWidth, 100)}%`} 
                    bc={getLoadColor(isRam ? barWidth : core.load)} 
                  />
                </YStack>

              </YStack>
            );
          })}
        </XStack>
      </ScrollView>

      <XStack p="$4" bc="$backgroundTransparent" borderRadius="$4" jc="space-around">
         
         <YStack ai="center">
           <Text color="$gray10">{type === 'RAM' ? 'Frequência' : 'Média Térmica'}</Text>
           <H2 fontFamily="$tech">
             {type === 'RAM' 
               ? formatValue(data?.RamMhz, "MHz") 
               : formatValue(type === 'CPU' ? data?.CpuTemp : data?.GpuTemp, "°C", 1)
             }
           </H2>
         </YStack>
         
         <Separator vertical />

         <YStack ai="center">
            {type === 'CPU' ? (
                <>
                   <Text color="$gray10">Desvio (Delta)</Text>
                   <H2 fontFamily="$tech" color={delta > 15 ? COLORS.red : COLORS.lime}>
                     {delta > 0 ? formatValue(delta, "°C", 1) : "--"}
                   </H2>
                </>
            ) : (
                <>
                   <Text color="$gray10">Clock</Text>
                   <H2 fontFamily="$tech">
                     {formatValue(type === 'GPU' ? data?.GpuMhz : data?.RamMhz, "MHz", 0)}
                   </H2>
                </>
            )}
         </YStack>

         <Separator vertical />

         <YStack ai="center">
           <Text color="$gray10">Voltagem</Text>
           <H2 fontFamily="$tech" color={COLORS.yellow}>
             {type === 'CPU' ? formatValue(data?.CpuVolt, "V", 2) :
              type === 'GPU' ? formatValue(data?.GpuVolt, "V", 2) :
              formatValue(data?.RamVolt, "V", 2)}
           </H2>
         </YStack>
         
         {type === 'CPU' && (
           <>
             <Separator vertical />
             <YStack ai="center">
               <Text color="$gray10">Clock</Text>
               <H2 fontFamily="$tech">
                 {formatValue(data?.CpuMhz, "MHz", 0)}
               </H2>
             </YStack>
           </>
         )}

      </XStack>
    </YStack>
  );
}