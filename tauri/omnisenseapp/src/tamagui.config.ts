import { createTamagui, createFont } from 'tamagui'
import { config as configBase } from '@tamagui/config/v3'
import { createAnimations } from '@tamagui/animations-css'

// --- FONTES ---
const interFont = createFont({
  family: 'Inter, sans-serif',
  size: { 1: 12, 2: 14, 3: 16, 4: 18 },
  lineHeight: { 1: 17, 2: 22, 3: 25 },
  weight: { 4: '300', 7: '600' },
  letterSpacing: { 4: 0, 8: -1 },
})

const techFont = createFont({
  family: 'Rajdhani, sans-serif',
  size: { 1: 14, 2: 18, 3: 24, 4: 32, 5: 48 },
  lineHeight: { 1: 18, 2: 24, 3: 30, 4: 40, 5: 60 }, 
  weight: { 4: '500', 7: '700' },
  letterSpacing: { 4: 1 },
})

// --- ANIMAÇÕES ---
const animations = createAnimations({
  bouncy: 'cubic-bezier(0.175, 0.885, 0.32, 1.275) 500ms',
  smooth: 'all 200ms ease-in-out', 
  quick: 'all 100ms ease-in',
})

// --- CONFIG ---
const config = createTamagui({
  ...configBase,
  fonts: {
    body: interFont,
    heading: interFont,
    tech: techFont,
  },
  animations: animations as any,
})

export { config }
export default config