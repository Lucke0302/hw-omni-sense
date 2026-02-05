import { YStack, XStack, Text, Separator, styled } from "tamagui"; 

interface StatCardProps {
  title: string;
  temp?: number | null;
  load?: number | null;
  frequency?: number | null;
  voltage?: number | null;
  
  tempColor?: string;
  loadColor: string;
  isCritical?: boolean;
  onClick?: () => void;
}

const CardFrame = styled(YStack, {
  f: 1,
  minWidth: 220,
  maxWidth: 300,
  p: "$4",
  borderRadius: "$4",
  backgroundColor: "transparent", 
  y: 0,
  opacity: 1,
  scale: 1, 
  cursor: 'pointer',
  shadowColor: "#000000", 
  shadowRadius: 5,
  shadowOffset: { width: 0, height: 2 },
  shadowOpacity: 0.1,
  hoverStyle: {
    scale: 1.05,
    y: -5,
    borderColor: "$blue8", 
    shadowRadius: 15,
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.3,
  },
  pressStyle: { scale: 0.98, y: 0 }
});

export function StatCard({ title, temp, load, frequency, voltage, tempColor, loadColor, isCritical, onClick }: StatCardProps) {
  
  const formatValue = (val: number | undefined | null, unit: string, decimals = 1) => {
    if (val === undefined || val === null || (val === 0 && unit !== "%")) return "--";
    
    return `${val.toFixed(decimals)} ${unit}`;
  };

  return (
    <CardFrame 
      className="smooth-transition glass-panel"
      borderWidth={isCritical ? 2 : 1}
      borderColor={isCritical ? "$red10" : "$borderColor"}
      scale={isCritical ? 1.05 : 1}
      onPress={onClick}
    >
      <YStack gap="$2"> 
        <XStack jc="space-between" ai="center" mb="$2">
          <Text fontWeight="bold" fontSize="$4" color="$gray11" fontFamily="$heading">
            {title}
          </Text>
          {isCritical && <Text fontSize="$4">🔥</Text>}
        </XStack>
        
        <Separator borderColor="$gray6" />
        
        <XStack jc="space-between" ai="center">
          <Text color="$gray10" fontSize="$3">Temp:</Text>
          <Text fontFamily="$tech" fontSize="$4" color={tempColor || "$gray11"} fontWeight="700">
            {formatValue(temp, "°C")}
          </Text>
        </XStack>

        <XStack jc="space-between" ai="center">
          <Text color="$gray10" fontSize="$3">Uso:</Text>
          <Text fontFamily="$tech" fontSize="$4" color={loadColor} fontWeight="700">
            {formatValue(load, "%", 1)}
          </Text>
        </XStack>

        <XStack jc="space-between" ai="center">
            <Text color="$gray10" fontSize="$3">Clock:</Text>
            <Text fontFamily="$tech" fontSize="$3" color="$gray11">
            {formatValue(frequency, "MHz", 0)}
            </Text>
        </XStack>

        <XStack jc="space-between" ai="center">
            <Text color="$gray10" fontSize="$3">Volt:</Text>
            <Text fontFamily="$tech" fontSize="$3" color="$yellow10">
            {formatValue(voltage, "V", 2)}
            </Text>
        </XStack>

      </YStack>
    </CardFrame>
  );
}