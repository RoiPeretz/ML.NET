import type { IngestionStatusEvent } from '@/models/ingestionStatusEvent'
import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', () => {
  const currentStatus = ref<Map<string, IngestionStatusEvent[]>>(new Map<string, IngestionStatusEvent[]>());

  return {
    currentStatus
  }
})
