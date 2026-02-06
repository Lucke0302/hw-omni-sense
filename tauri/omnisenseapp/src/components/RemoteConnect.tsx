import { useState, useEffect } from "react";
import { YStack, Text, Button, XStack, Input } from "tamagui";
import QRCode from "react-qr-code";
import { invoke } from "@tauri-apps/api/core";

export function RemoteConnect({ onClose }: { onClose: () => void }) {
  const [ip, setIp] = useState("Carregando IP...");
  const [remoteUrl, setRemoteUrl] = useState("");

  useEffect(() => {
    const fetchIp = async () => {
      try {
        const foundIp = await invoke("get_local_ip") as string;
        
        console.log("IP do Rust:", foundIp);
        
        if (foundIp && foundIp !== "127.0.0.1") {
          setIp(foundIp);
          setRemoteUrl(`http://${foundIp}:9090`); 
        } else {
          setIp("Não detectado. Digite abaixo:");
        }
      } catch (e) {
        console.error("Erro Rust:", e);
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
      zIndex={1000}
    >
      <Text color="$color" fontFamily="$heading" fontSize="$5">Conexão Remota 📱</Text>
      
      <YStack p="$4" bc="white" borderRadius="$4">
        {remoteUrl ? <QRCode value={remoteUrl} size={200} /> : <Text color="black">...</Text>}
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
          w={200} 
          bc="$background" 
          color="$color"
        />
      </XStack>

      <Button theme="red" onPress={onClose}>Fechar</Button>
    </YStack>
  );
}