import { YStack, XStack, Text, Separator, styled, Button } from "tamagui"; 

interface StatCardProps {
  title: string;
  temp?: number | null;
  load?: number | null;
  frequency?: number | null;
  voltage?: number | null;

  healthStatus?: string;
  
  tempColor?: string;
  loadColor: string;
  isCritical?: boolean;
  onClick?: () => void;

  isStressing?: boolean;
  onStressToggle?: () => void;
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

export function StatCard({
  title, temp, load, frequency, voltage,
  healthStatus, tempColor, loadColor, isCritical, onClick,
  isStressing, onStressToggle
}: StatCardProps) {

  const formatValue = (val: number | null | undefined, unit: string, digits = 0) => 
    val !== undefined && val !== null ? `${val.toFixed(digits)}${unit}` : "--";

  return (
    <CardFrame 
      onPress={onClick} 
      className="smooth-transition glass-panel"
      borderColor={isCritical ? "$red10" : "$borderColor"}
      backgroundColor="$cardBg"
    >
        {/* --- CABEÇALHO --- */}
        <XStack jc="space-between" ai="center" mb="$2">
          <XStack ai="center" gap="$2">
            <Text fontWeight="bold" fontSize="$4" color="$gray11" fontFamily="$heading">
              {title}
            </Text>
            
            {isStressing && (
              <YStack 
                bg="$red4" px="$2" py="$1" borderRadius="$4" 
                enterStyle={{ opacity: 0, scale: 0.5 }}
              >
                 <Text fontSize="$2" fontWeight="bold" color="$red11">🔥 TESTE</Text>
              </YStack>
            )}
          </XStack>

          <XStack>
             {isCritical && <Text fontSize="$4">⚠️</Text>}
          </XStack>
        </XStack>
        
        <Separator borderColor="$gray6" mb="$3" />
        
        {/* --- DADOS --- */}
        <YStack gap="$2">
            <XStack jc="space-between" ai="center">
            <Text color="$gray10" fontSize="$3">Temp:</Text>
            <Text fontFamily="$tech" fontSize="$4" color={tempColor || "$gray11"} fontWeight="700">
                {formatValue(temp, "°C")}
            </Text>
            </XStack>

            <XStack jc="space-between" ai="center">
            <Text color="$gray10" fontSize="$3">Uso:</Text>
            <Text fontFamily="$tech" fontSize="$4" color={loadColor} fontWeight="700">
                {formatValue(load, "%", 0)}
            </Text>
            </XStack>

            <XStack jc="space-between" ai="center">
                <Text color="$gray10" fontSize="$3">Clock:</Text>
                <Text fontFamily="$tech" fontSize="$3" color="$gray11">
                {formatValue(frequency, " MHz", 0)}
                </Text>
            </XStack>

            {/* Voltage */}
            {voltage !== undefined && voltage !== null && (
                <XStack jc="space-between" ai="center">
                    <Text color="$gray10" fontSize="$3">Volt:</Text>
                    <Text fontFamily="$tech" fontSize="$3" color="$gray11">
                    {formatValue(voltage, " V", 2)}
                    </Text>
                </XStack>
            )}
        </YStack>

        {onStressToggle && (
            <Button 
                size="$2" 
                mt="$3" 
                theme={isStressing ? "red" : "gray"} 
                onPress={(e) => {
                    e.stopPropagation(); 
                    onStressToggle();
                }}
                icon={isStressing ? undefined : <Text>⚡</Text>}
                chromeless={!isStressing} 
                borderWidth={1}
            >
                {isStressing ? "Parar Teste ⏹️" : "Estressar"}
            </Button>
        )}

    </CardFrame>
  );
}