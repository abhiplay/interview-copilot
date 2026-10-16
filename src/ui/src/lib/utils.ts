import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}

export const OVERLAY_OPACITY = { min: 0.3, max: 1, default: 0.85 } as const;

/** FR-17: keep the overlay readable and never fully invisible. */
export function clampOpacity(value: number): number {
  if (Number.isNaN(value)) {
    return OVERLAY_OPACITY.default;
  }
  return Math.min(OVERLAY_OPACITY.max, Math.max(OVERLAY_OPACITY.min, value));
}
