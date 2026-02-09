import { useEffect, useState } from "react";
import { sendNotification } from "@tauri-apps/plugin-notification";
import { load } from "@tauri-apps/plugin-store"; 
import { YStack, XStack, Text, Button, ScrollView, Popover, Input, Label, Separator } from "tamagui";

interface NotificationLog {
  id: number;
  time: string;
  title: string;
  message: string;
  type: 'cpu' | 'gpu';
}

interface Props {
  cpuTemp: number;
}

export function NotificationManager({ cpuTemp }: Props) {
  const [cpuLimit, setCpuLimit] = useState(85);
  const [inputValue, setInputValue] = useState("85");
  
  const [cooldown, setCooldown] = useState(0);
  const [logs, setLogs] = useState<NotificationLog[]>([]);

  useEffect(() => {
    const loadSettings = async () => {
      try {
        const store = await load('settings.json', { 
            autoSave: true,
            defaults: { cpu_limit: 85 } 
        });
        
        const savedLimit = await store.get<number>('cpu_limit');
        
        if (savedLimit) {
          setCpuLimit(savedLimit);
          setInputValue(savedLimit.toString());
        }
      } catch (e) {
        console.error("Erro ao carregar configs:", e);
      }
    };
    loadSettings();
  }, []);

  const handleSave = async () => {
    const val = Number(inputValue);
    
    if (!isNaN(val) && val > 40) {
        setCpuLimit(val);
        
        try {
            const store = await load('settings.json', { 
                autoSave: true,
                defaults: { cpu_limit: 85 }
            });
            await store.set('cpu_limit', val);
            await store.save();

            console.log("Salvo com sucesso:", val);
        } catch (e) { console.error(e); }
    } else {
        setInputValue(cpuLimit.toString());
    }
  };

  useEffect(() => {
    const now = Date.now();
    
    if (now - cooldown < 300000) return; 

    if (cpuTemp > cpuLimit) {
      triggerNotification("CPU Quente! 🔥", `A CPU atingiu ${cpuTemp}°C. Verifique a refrigeração.`, 'cpu');
    }

  }, [cpuTemp, cpuLimit, cooldown]);

  const triggerNotification = async (title: string, body: string, type: 'cpu' | 'gpu') => {
    try {
      await sendNotification({ title, body });
    } catch (e) { console.error("Erro ao notificar:", e); }

    const newLog: NotificationLog = {
      id: Date.now(),
      time: new Date().toLocaleTimeString(),
      title,
      message: body,
      type
    };

    setLogs(prev => [newLog, ...prev].slice(0, 50));
    setCooldown(Date.now());
  };

  const hasChanges = inputValue !== cpuLimit.toString();

  return (
<Popover placement="bottom-end" size="$4" allowFlip>
      <Popover.Trigger asChild>
        <Button size="$3" circular theme={logs.length > 0 ? "red" : "gray"}>
            <Text fontSize={18}>{logs.length > 0 ? "⚠️" : "🔔"}</Text>
        </Button>
      </Popover.Trigger>

      <Popover.Content 
        borderWidth={1} borderColor="$borderColor" 
        enterStyle={{ y: -10, opacity: 0 }} 
        exitStyle={{ y: -10, opacity: 0 }} 
        elevate w={300} p="$0"
        backgroundColor="$background"
      >
        <Popover.Arrow borderWidth={1} borderColor="$borderColor" />

        <YStack>
          {/* CABEÇALHO */}
          <XStack p="$3" jc="space-between" ai="center" bc="$backgroundHover">
            <Text fontFamily="$heading" fontWeight="bold" fontSize="$3">Configuração de Alerta</Text>
            <Popover.Close asChild>
                <Button 
                    size="$2" 
                    chromeless 
                    hoverStyle={{ backgroundColor: '$red5' }}
                    icon={
                        <Text 
                            fontSize={14} 
                            fontWeight="bold"
                            color="$white"
                        >
                            ✕
                        </Text>
                    } 
                />
            </Popover.Close>
          </XStack>
          
          <Separator />

          <YStack p="$3" gap="$3">
            
            {/* ÁREA DE CONFIGURAÇÃO */}
            <YStack bc="$gray3" p="$3" borderRadius="$4" gap="$2">
                
                <Label color="$gray11" fontSize={11} fontWeight="bold">NOTIFICAR ACIMA DE:</Label>
                
                <XStack ai="center" jc="space-between" gap="$2">
                    <XStack w={50} ai="center" gap="$1" bc="$background" borderRadius="$3" px="$2" borderWidth={1} borderColor="$borderColor" h={35}>
                        <Input 
                            unstyled
                            flex={1}
                            h="100%"
                            keyboardType="numeric" 
                            maxLength={3}
                            value={inputValue} 
                            onChangeText={setInputValue} 
                            textAlign="right"
                            fontWeight="bold"
                            color="$color"
                            fontSize={14}
                        />
                        <Text color="$gray11" fontSize={12} pt={2}>°C</Text>
                    </XStack>

                    <Button 
                        size="$3" 
                        f={1} 
                        theme={hasChanges ? "green" : "gray"} 
                        disabled={!hasChanges}
                        onPress={handleSave}
                    >
                        {hasChanges ? "Salvar Alteração" : "Salvo"}
                    </Button>
                </XStack>
            </YStack>

            {/* LISTA DE LOGS */}
            <YStack>
                <XStack jc="space-between" ai="center" mb="$2" mt="$2">
                    <Text fontSize={11} color="$gray10" fontWeight="bold">HISTÓRICO</Text>
                    {logs.length > 0 && (
                        <Button size="$2" theme="red" variant="outlined" onPress={() => setLogs([])} chromeless>
                            Limpar
                        </Button>
                    )}
                </XStack>

                <ScrollView maxHeight={180} bc="$gray2" borderRadius="$3" p="$2">
                    {logs.length === 0 ? (
                    <YStack ai="center" jc="center" h={60}>
                        <Text fontSize={32} mb="$1">😴</Text>
                        <Text color="$gray9" fontSize={12}>Nenhum alerta recente.</Text>
                    </YStack>
                    ) : (
                    <YStack gap="$2">
                        {logs.map(log => (
                        <YStack key={log.id} bc="$background" p="$2" borderRadius="$3" borderLeftWidth={3} borderLeftColor="$red9">
                            <XStack jc="space-between">
                            <Text fontWeight="bold" fontSize={10}>{log.title}</Text>
                            <Text color="$gray9" fontSize={9}>{log.time}</Text>
                            </XStack>
                            <Text color="$gray11" fontSize={10} mt={1}>{log.message}</Text>
                        </YStack>
                        ))}
                    </YStack>
                    )}
                </ScrollView>
            </YStack>

          </YStack>
        </YStack>
      </Popover.Content>
    </Popover>
  );
}