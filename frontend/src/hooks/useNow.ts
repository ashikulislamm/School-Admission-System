"use client";

import { useSyncExternalStore } from "react";
import { serverNow } from "@/lib/clock";

// One shared timer for every countdown on screen. The value is the estimated server time in ms.
// 0 means "not measured yet" (server render); consumers treat it as unknown.
const TICK_MS = 500;

let now = 0;
let timer: ReturnType<typeof setInterval> | null = null;
const listeners = new Set<() => void>();

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  if (timer === null) {
    now = serverNow();
    timer = setInterval(() => {
      now = serverNow();
      listeners.forEach((l) => l());
    }, TICK_MS);
  }
  return () => {
    listeners.delete(listener);
    if (listeners.size === 0 && timer !== null) {
      clearInterval(timer);
      timer = null;
      now = 0; // next mount starts from a fresh reading, never a stale one
    }
  };
}

function getSnapshot(): number {
  if (now === 0) now = serverNow();
  return now;
}

/** Estimated current server time (ms), ticking twice a second while any component uses it. */
export function useNow(): number {
  return useSyncExternalStore(subscribe, getSnapshot, () => 0);
}
