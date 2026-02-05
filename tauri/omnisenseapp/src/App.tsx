import { useEffect, useState } from "react";
import { Command, Child } from "@tauri-apps/plugin-shell";
import { open } from "@tauri-apps/plugin-dialog";
import { YStack, Text, H2, XStack, Button } from "tamagui";
import { StatCard } from "./components/StatCard";
import { getLoadColor, getTempColor, isCriticalState } from "./utils/status";
import { DetailPanel } from "./components/DetailPanel";
import { check } from '@tauri-apps/plugin-updater';
import { relaunch } from '@tauri-apps/plugin-process';
import { RemoteConnect } from "./components/RemoteConnect";

interface TelemetryData {
  CpuTemp: number; 
  CpuLoad: number; 
  CpuMhz: number;
  CpuVolt: number;
  
  GpuTemp: number; 
  GpuLoad: number; 
  GpuMhz: number;
  GpuVolt: number;
  
  IsSimulation: boolean;
  CpuCoreTemps?: number[]; 
  CpuHotspotDelta?: number; 
  
  RamUsed: number; 
  RamTotal: number; 
  RamMhz: number; 
  RamTemp: number;
  RamVolt: number; 
}

let activeSidecar: Child | null = null;

function App() {
  const [data, setData] = useState<TelemetryData | null>(null);
  const [status, setStatus] = useState("Inicializando...");
  const [showRemote, setShowRemote] = useState(false);

  const [selectedView, setSelectedView] = useState<'CPU' | 'GPU' | 'RAM' | null>(null);

  const spawnSidecar = async (customPath?: string, cleanDb = false) => {
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
              if (document.hidden) return; 

              const parsed = JSON.parse(line);
              setData(parsed);
              setStatus(parsed.IsSimulation ? "Modo Simulação ⚠️" : "Monitorando 🟢");
          } catch { }
      });

      command.stderr.on("data", (line: string) => console.error(`Erro C#: ${line}`));
      
      const child = await command.spawn();
      
      activeSidecar = child;
      console.log("Novo processo registrado (PID):", child.pid);

    } catch (err) {
      setStatus(`Erro Fatal: ${err}`);
    }
  };

  useEffect(() => {
    const savedPath = localStorage.getItem("afterburner_path") || undefined;
    
    const timer = setTimeout(() => {
        spawnSidecar(savedPath);
    }, 100);


    return () => {
        clearTimeout(timer);
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

  useEffect(() => {
    const checkForUpdates = async () => {
      try {
        const update = await check();
        if (update?.available) {
          const yes = confirm(`Atualização ${update.version} disponível! Baixar agora?`);
          if (yes) {
            await update.downloadAndInstall();
            await relaunch();
          }
        }
      } catch (error) {
        console.error("Erro ao buscar update:", error);
      }
    };

    checkForUpdates();
  }, []);

  const ramPercentage = (data?.RamUsed && data?.RamTotal) 
    ? (data.RamUsed / data.RamTotal) * 100 
    : 0;

return (
        <YStack f={1} p="$4" gap="$4">
          <div className="cyber-grid-bg" />
          <div className="vignette" />
          
          {selectedView ? (
            <DetailPanel type={selectedView} data={data} onBack={() => setSelectedView(null)} />
          ) : (
            <>
              <YStack ai="center" mb="$2">
                <H2 color="$color" fontFamily="$heading">OmniSense</H2>
                <Text color="$gray10" fontSize="$2" fontFamily="$body">{status}</Text>
              </YStack>

              <XStack flexWrap="wrap" gap="$4" jc="center" width="100%">
                
                {/* CPU CARD */}
                <StatCard 
                  title="CPU" 
                  temp={data?.CpuTemp} 
                  load={data?.CpuLoad}
                  frequency={data?.CpuMhz} 
                  voltage={data?.CpuVolt}
                  tempColor={getTempColor(data?.CpuTemp)}
                  loadColor={getLoadColor(data?.CpuLoad)}
                  isCritical={isCriticalState(data?.CpuTemp)}
                  onClick={() => setSelectedView('CPU')}
                />

                {/* GPU CARD */}
                <StatCard 
                  title="GPU" 
                  temp={data?.GpuTemp} 
                  load={data?.GpuLoad} 
                  frequency={data?.GpuMhz} 
                  voltage={data?.GpuVolt}
                  tempColor={getTempColor(data?.GpuTemp)}
                  loadColor={getLoadColor(data?.GpuLoad)}
                  isCritical={isCriticalState(data?.GpuTemp)}
                  onClick={() => setSelectedView('GPU')}
                />

                {/* RAM CARD */}
                <StatCard 
                  title="RAM" 
                  temp={data && data.RamTemp > 0 ? data.RamTemp : undefined} 
                  load={ramPercentage > 0 ? ramPercentage : undefined}
                  frequency={data?.RamMhz}
                  voltage={data?.RamVolt} 
                  loadColor={getLoadColor(ramPercentage)} 
                  onClick={() => setSelectedView('RAM')}
                />

              </XStack>

              <XStack gap="$3" mt="auto" jc="center">
                <Button size="$3" onPress={handleConfig}>⚙️ Config</Button>
                <Button size="$3" theme="red" onPress={handleClean}>🧹 Limpar</Button>
              </XStack>
            </>
          )}
        </YStack>
      );
}

export default App;