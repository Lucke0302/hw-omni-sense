import { useEffect, useState } from "react";
import { Command, Child } from "@tauri-apps/plugin-shell";
import { open } from "@tauri-apps/plugin-dialog";
import { YStack, Text, H2, Card, XStack, Separator, Button } from "tamagui";

interface TelemetryData {
  CpuTemp: number; CpuLoad: number; GpuTemp: number; GpuLoad: number; IsSimulation: boolean;
}

// --- VARIÁVEL GLOBAL (SINGLETON) ---
// Ela mora FORA da função App. O React pode reiniciar o App à vontade,
// mas essa variável continua viva e única.
let activeSidecar: Child | null = null;

function App() {
  const [data, setData] = useState<TelemetryData | null>(null);
  const [status, setStatus] = useState("Inicializando...");

  // Função centralizada para iniciar o Sidecar
  const spawnSidecar = async (customPath?: string, cleanDb = false) => {
    // 1. REGRA DE OURO: Antes de criar um novo, OBRIGATORIAMENTE matamos o velho
    // Como a variável é global, isso funciona mesmo entre re-renderizações.
    if (activeSidecar) {
      try {
        console.log("Matando processo antigo antes de iniciar novo...");
        await activeSidecar.kill();
      } catch (e) {
        console.error("Erro ao matar anterior (pode já estar morto):", e);
      }
      activeSidecar = null;
    }

    try {
      const args = [];
      if (cleanDb) args.push("--clean");
      if (customPath) args.push(customPath);

      setStatus(`Iniciando... ${cleanDb ? "(Limpando)" : ""}`);
      
      const command = Command.sidecar("binaries/hw-omnisense-collector", args);

      // Ouvintes de eventos
      command.stdout.on("data", (line: string) => {
        try {
          const parsed = JSON.parse(line);
          setData(parsed);
          setStatus(parsed.IsSimulation ? "Modo Simulação ⚠️" : "Monitorando 🟢");
        } catch { }
      });

      command.stderr.on("data", (line: string) => console.error(`Erro C#: ${line}`));
      
      // 2. Nasce o novo processo
      const child = await command.spawn();
      
      // 3. Registra ele na variável global
      activeSidecar = child;
      console.log("Novo processo registrado (PID):", child.pid);

    } catch (err) {
      setStatus(`Erro Fatal: ${err}`);
    }
  };

  useEffect(() => {
    // Ao iniciar, pega o caminho salvo e roda
    const savedPath = localStorage.getItem("afterburner_path") || undefined;
    
    // Um pequeno delay ajuda a evitar condições de corrida (Race Conditions) no Windows
    const timer = setTimeout(() => {
        spawnSidecar(savedPath);
    }, 100);

    // Cleanup: Quando fechar a janela totalmente
    return () => {
        clearTimeout(timer);
        // Não matamos aqui no cleanup do useEffect para evitar o pisca-pisca do Strict Mode.
        // Deixamos o spawnSidecar cuidar de matar o anterior se for necessário.
    };
  }, []);

  const handleConfig = async () => {
    try {
      const file = await open({
        multiple: false,
        filters: [{ name: 'Executável', extensions: ['exe'] }]
      });
      if (file) {
        const filePath = Array.isArray(file) ? file[0] : file;
        if (filePath) {
            localStorage.setItem("afterburner_path", filePath);
            spawnSidecar(filePath);
        }
      }
    } catch (e) { console.error(e); }
  };

  const handleClean = () => {
    const savedPath = localStorage.getItem("afterburner_path") || undefined;
    spawnSidecar(savedPath, true);
  };

  return (
    <YStack f={1} bc="$background" ai="center" jc="center" p="$4" gap="$4">
      <H2 color="$color">OmniSense</H2>
      <Text color="$gray10" fontSize="$2">{status}</Text>

      <Card w={300} p="$4" borderWidth={1} borderColor="$borderColor">
        <YStack gap="$2">
          <Text fontWeight="\bold\">Processador (CPU)</Text>
          <Separator />
          <XStack jc="space-between">
            <Text>Temp:</Text>
            <Text color="$red10" fontWeight="bold">{data?.CpuTemp?.toFixed(1) || "--"} °C</Text>
          </XStack>
           <XStack jc="space-between">
            <Text>Uso:</Text>
            <Text color="$blue10">{data?.CpuLoad?.toFixed(1) || "--"} %</Text>
          </XStack>
        </YStack>
      </Card>

      <Card w={300} p="$4" borderWidth={1} borderColor="$borderColor">
        <YStack gap="$2">
          <Text fontWeight="\bold\">Placa de Vídeo (GPU)</Text>
          <Separator />
          <XStack jc="space-between">
            <Text>Temp:</Text>
            <Text color="$red10" fontWeight="bold">{data?.GpuTemp?.toFixed(1) || "--"} °C</Text>
          </XStack>
          <XStack jc="space-between">
            <Text>Uso:</Text>
            <Text color="$blue10">{data?.GpuLoad?.toFixed(1) || "--"} %</Text>
          </XStack>
        </YStack>
      </Card>

      <XStack gap="$3">
        <Button size="$3" onPress={handleConfig}>⚙️ Configurar</Button>
        <Button size="$3" theme="red" onPress={handleClean}>🧹 Limpar</Button>
      </XStack>
    </YStack>
  );
}

export default App;