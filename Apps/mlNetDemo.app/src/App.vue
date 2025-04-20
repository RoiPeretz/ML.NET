<template>
  <v-app>
    <router-view />
  </v-app>
</template>

<script lang="ts" setup>
import { BffHubClient, type IBffHubClient } from './hubs/detactionHubClient';
import type { IngestionStatusEvent } from './models/ingestionStatusEvent';
import { useAppStore } from './stores/app';

const store = useAppStore();
const bffHubClient: IBffHubClient = new BffHubClient;
provide<IBffHubClient>("bffHubClient", bffHubClient);

bffHubClient.ingestionStatusAdded.subscribe((status) => {
  const modelName = status.modelName || '';
  const events: IngestionStatusEvent[] = store.currentStatus.get(modelName) || [];
  events.push(status);
});

onMounted(async () => {
  await bffHubClient.startAaync();
  
  const kvps: Record<string, IngestionStatusEvent[]> = await bffHubClient.getCurretStatus();
  Object.entries(kvps).forEach(([key, value]: [string, IngestionStatusEvent[]]) => {
    const events = store.currentStatus.get(key);
    if (!events) {
      store.currentStatus.set(key, value);
    } else {
      events.push(...value);
    }
  });
 
});
</script>
