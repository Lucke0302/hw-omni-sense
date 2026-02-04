import React from "react";
import ReactDOM from "react-dom/client";
import { TamaguiProvider } from 'tamagui';
import config from './tamagui.config';
import App from "./App";
import './App.css';

import '@tamagui/core/reset.css' 

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <TamaguiProvider config={config} defaultTheme="dark">
      <App />
    </TamaguiProvider>
  </React.StrictMode>,
);