import { config } from './tamagui.config'

export type Conf = typeof config

declare module 'tamagui' {
  interface TamaguiCustomConfig extends Conf {}
  
  interface AnimationKeys {
    bouncy: any
    smooth: any
    quick: any
  }
}