import { useState, useEffect } from "react";
import { YStack, Text, Button, XStack, Input } from "tamagui";
import QRCode from "react-qr-code";
import { Command } from "@tauri-apps/plugin-shell";

export function RemoteConnect({ onClose }: { onClose: () => void }) {
  const [ip, setIp] = useState("Carregando IP...");
  const [remoteUrl, setRemoteUrl] = useState("");

  useEffect(() => {
    const fetchIp = async () => {
      try {
        const cmd = Command.create("cmd", ["/c", "ipconfig"]);
        const output = await cmd.execute();
        const text = output.stdout;
        
        const match = text.match(/IPv4 Address[ .]*: (192\.168\.\d+\.\d+)/);
        
        if (match && match[1]) {
          setIp(match[1]);
          setRemoteUrl(`http://${match[1]}:9090`); 
        } else {
          setIp("Não encontrado. Digite manualmente.");
        }
      } catch (e) {
        setIp("Erro ao buscar IP");
      }
    };
    fetchIp();
  }, []);

  return (
    <YStack 
      f={1} 
      className="glass-panel" 
      p="$6" 
      jc="center" 
      ai="center" 
      gap="$4"
      borderRadius="$6"
    >
      <Text color="$color" fontFamily="$heading" fontSize="$5">Conexão Remota 📱</Text>
      
      <YStack p="$4" bc="white" borderRadius="$4">
        {remoteUrl ? <QRCode value={remoteUrl} size={200} /> : <Text>Gerando...</Text>}
      </YStack>

      <Text color="$gray11" textAlign="center">
        Escaneie para monitorar este PC via Wi-Fi.
        {"\n"}
        Certifique-se que o celular está na mesma rede.
      </Text>
      
      <XStack gap="$2" ai="center">
        <Text color="$gray10">IP Local:</Text>
        <Input 
          value={ip} 
          onChangeText={(t) => { setIp(t); setRemoteUrl(`http://${t}:9090`); }} 
          w={150} 
          bc="$background" 
          color="$color"
        />
      </XStack>

      <Button theme="red" onPress={onClose}>Fechar</Button>
    </YStack>
  );
}