<template>
  <v-app>
    <router-view />
  </v-app>
</template>

<script lang="ts" setup>
import { DetactionHubClient, type IDetactionHubClient } from './hubs/detactionHubClient';
import type { IngestionStatusEvent } from './models/ingestionStatusEvent';
import { useAppStore } from './stores/app';

const store = useAppStore();
const detectionHubClient: IDetactionHubClient = new DetactionHubClient;
provide<IDetactionHubClient>("detectionHubClient", detectionHubClient);

detectionHubClient.ingestionStatusAdded.subscribe((status) => {
  const modelName = status.modelName || '';
  const events: IngestionStatusEvent[] = store.currentStatus.get(modelName) || [];
  events.push(status);
});

onMounted(async () => {
  await detectionHubClient.startAaync();
  
  const kvps: Record<string, IngestionStatusEvent[]> = await detectionHubClient.getCurretStatus();
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
