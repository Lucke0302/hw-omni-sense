import { useEffect, useState } from "react";
import { sendNotification } from "@tauri-apps/plugin-notification";
import { YStack, XStack, Text, Button, ScrollView, Popover, Input, Label } from "tamagui";
import { Bell, AlertTriangle, Trash } from "@tamagui/lucide-icons";

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
  const [cpuLimit, setCpuLimit] = useState(70);
  const [cooldown, setCooldown] = useState(0);
  
  const [logs, setLogs] = useState<NotificationLog[]>([]);

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
    } catch (e) {
      console.error("Erro ao notificar:", e);
    }

    const newLog: NotificationLog = {
      id: Date.now(),
      time: new Date().toLocaleTimeString(),
      title,
      message: body,
      type
    };

    setLogs(prev => [newLog, ...prev].slice(0, 50))
    setCooldown(Date.now());
  };

  return (
    <Popover placement="bottom">
        <Popover.Trigger asChild>
        <Button size="$3" circular chromeless>
            <Text fontSize={20}>{logs.length > 0 ? "⚠️" : "🔔"}</Text>
        </Button>
      </Popover.Trigger>

      <Popover.Content borderWidth={1} borderColor="$borderColor" enterStyle={{ y: -10, opacity: 0 }} exitStyle={{ y: -10, opacity: 0 }} elevate w={350}>
        <Popover.Arrow borderWidth={1} borderColor="$borderColor" />

        <YStack p="$3" gap="$3">
          <XStack jc="space-between" ai="center">
            <Text fontFamily="$heading" fontWeight="bold">Central de Alertas</Text>
            <Button size="$2" theme="red" onPress={() => setLogs([])} chromeless>
                🗑️ Limpar
            </Button>
          </XStack>

          <XStack ai="center" gap="$2" bc="$backgroundHover" p="$2" borderRadius="$3">
            <Label f={1} color="$gray10" fontSize={12}>Alertar CPU acima de:</Label>
            <Input 
              w={60} 
              keyboardType="numeric" 
              value={cpuLimit.toString()} 
              onChangeText={t => setCpuLimit(Number(t))} 
              bc="$background"
            />
            <Text>°C</Text>
          </XStack>

          <ScrollView maxHeight={300}>
            {logs.length === 0 ? (
              <Text color="$gray10" textAlign="center" py="$4">Nenhum alerta registrado.</Text>
            ) : (
              <YStack gap="$2">
                {logs.map(log => (
                  <YStack key={log.id} bc="$gray4" p="$2" borderRadius="$3" borderLeftWidth={4} borderLeftColor="$red10">
                    <XStack jc="space-between">
                      <Text fontWeight="bold" fontSize={12}>{log.title}</Text>
                      <Text color="$gray10" fontSize={10}>{log.time}</Text>
                    </XStack>
                    <Text color="$gray11" fontSize={11}>{log.message}</Text>
                  </YStack>
                ))}
              </YStack>
            )}
          </ScrollView>
        </YStack>
      </Popover.Content>
    </Popover>
  );
}