export const COLORS = {
  silver: "$gray9",
  green: "$green9",
  lime: "$green11",
  yellow: "$yellow10",
  orange: "$orange10",
  red: "$red10",
  critical: "$red11",
};

export function getLoadColor(load: number | undefined): string {
  if (load === undefined) return COLORS.silver;


  if (load < 20) return COLORS.green;
  if (load < 40) return COLORS.lime;
  if (load < 60) return COLORS.yellow;
  if (load < 80) return COLORS.orange;
  return COLORS.red;
}

export function getTempColor(temp: number | undefined): string {
  if (temp === undefined) return COLORS.silver;

  if (temp < 45) return COLORS.silver;
  if (temp < 60) return COLORS.yellow;
  if (temp < 75) return COLORS.orange;
  if (temp < 85) return COLORS.red;
  return COLORS.critical;
}

export function isCriticalState(temp: number | undefined): boolean {
  if (!temp) return false;
  return temp > 85; 
}