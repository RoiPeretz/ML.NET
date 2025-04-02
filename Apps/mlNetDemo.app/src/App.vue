<template>
  <v-app>
    <router-view />
  </v-app>
</template>

<script lang="ts" setup>
import { DetactionHubClient, type IDetactionHubClient } from './hubs/detactionHubClient';

const detectionHubClient = new DetactionHubClient;
provide<IDetactionHubClient>("detectionHubClient", detectionHubClient);

onMounted(async () => {
  await detectionHubClient.startAaync();
})
</script>
