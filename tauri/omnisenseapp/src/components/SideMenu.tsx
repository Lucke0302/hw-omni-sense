import { Settings, Smartphone, Trash2, Gauge, X } from "lucide-react";

interface SideMenuProps {
  isOpen: boolean; onClose: () => void; onConfig: () => void; onRemote: () => void;
  onClean: () => void; onOptimize: () => void;
}

export function SideMenu({ 
  isOpen, onClose, onConfig, onRemote, onClean, onOptimize
}: SideMenuProps) {
  
  return (
    <>
      {/* Overlay Escuro */}
      <div 
        className={`menu-overlay ${isOpen ? 'open' : ''}`} 
        onClick={onClose}
      />

      {/* Menu Lateral Deslizante */}
      <div className={`side-menu ${isOpen ? 'open' : ''}`}>
        
        <div className="menu-header">
          <span className="menu-title">Menu</span>
          <button className="menu-btn" style={{padding: 5}} onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <div className="menu-divider" />

        <small style={{color: '#666', textTransform: 'uppercase', fontSize: 12}}>Geral</small>
        
        <button className="menu-btn" onClick={onConfig}>
          <Settings size={20} /> Configurar MSI
        </button>
        
        <button className="menu-btn" onClick={onRemote}>
          <Smartphone size={20} /> Conexão Remota
        </button>
        
        <button className="menu-btn danger" onClick={onClean}>
          <Trash2 size={20} /> Limpar DB
        </button>

        <div className="menu-divider" />
        <small style={{color: '#666', textTransform: 'uppercase', fontSize: 12}}>Performance</small>

        <button className="menu-btn" style={{color: '#3b82f6'}} onClick={onOptimize}>
          <Gauge size={20} /> Otimizador (Breve)
        </button>


        <div style={{marginTop: 'auto', textAlign: 'center', color: '#444', fontSize: 12}}>
          v0.4.1
        </div>
      </div>
    </>
  );
}