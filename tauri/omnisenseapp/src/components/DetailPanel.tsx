import { YStack, XStack, Text, H2, Button, Separator, ScrollView } from "tamagui";
import { getLoadColor, getTempColor } from "../utils/status";

interface DetailPanelProps {
  type: 'CPU' | 'GPU' | 'RAM';
  onBack: () => void;
  data: any;
}

export function DetailPanel({ type, onBack, data }: DetailPanelProps) {
  
  const fakeCores = Array.from({ length: type === 'GPU' ? 1 : 12 }, (_, i) => ({
    id: i,
    load: Math.random() * 100,
    temp: 40 + Math.random() * 40
  }));

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
          <Text color="$gray10" fontFamily="$body">Análise Profunda em Tempo Real</Text>
        </YStack>
        <Button size="$3" theme="red" onPress={onBack}>
          X Fechar
        </Button>
      </XStack>

      <Separator borderColor="$gray8" />

      <ScrollView f={1}>
        <XStack flexWrap="wrap" gap="$3" jc="center" pb="$4">
          {fakeCores.map((core) => (
            <YStack 
              key={core.id} 
              w={140} 
              p="$3" 
              className="glass-panel"
              borderWidth={1}
              borderColor="$gray5"
              gap="$1"
            >
              <Text color="$gray11" fontWeight="bold" fontSize="$3">
                {type === 'GPU' ? 'Core Principal' : `Núcleo #${core.id + 1}`}
              </Text>
              
              <Separator my="$2" />
              
              <XStack jc="space-between">
                <Text color="$gray10" fontSize="$2">Uso</Text>
                <Text fontFamily="$tech" color={getLoadColor(core.load)} fontWeight="bold">
                  {core.load.toFixed(0)}%
                </Text>
              </XStack>
              
              <XStack jc="space-between">
                <Text color="$gray10" fontSize="$2">Temp</Text>
                <Text fontFamily="$tech" color={getTempColor(core.temp)} fontWeight="bold">
                  {core.temp.toFixed(0)}°C
                </Text>
              </XStack>

              <YStack h={4} w="100%" bc="$gray4" mt="$2" borderRadius="$10" overflow="hidden">
                <YStack h="100%" w={`${core.load}%`} bc={getLoadColor(core.load)} />
              </YStack>
            </YStack>
          ))}
        </XStack>
      </ScrollView>

      <XStack p="$4" bc="$backgroundTransparent" borderRadius="$4" jc="space-around">
        <YStack ai="center">
           <Text color="$gray10">Média Térmica</Text>
           <H2 fontFamily="$tech" color={getTempColor(type === 'CPU' ? data?.CpuTemp : data?.GpuTemp)}>
             {type === 'CPU' ? data?.CpuTemp?.toFixed(1) : data?.GpuTemp?.toFixed(1) || "--"} °C
           </H2>
        </YStack>
        <Separator vertical />
        <YStack ai="center">
           <Text color="$gray10">Carga Total</Text>
           <H2 fontFamily="$tech" color={getLoadColor(type === 'CPU' ? data?.CpuLoad : data?.GpuLoad)}>
             {type === 'CPU' ? data?.CpuLoad?.toFixed(1) : data?.GpuLoad?.toFixed(1) || "--"} %
           </H2>
        </YStack>
      </XStack>
    </YStack>
  );
}