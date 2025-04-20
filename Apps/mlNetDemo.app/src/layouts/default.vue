<template>
  <v-container>
    <v-tabs
      v-model="tab"
      background-color="primary"
      dark
    >
      <v-tab :to="{ name: 'ImageIngestionPage' }">
        Image Ingestion
      </v-tab>
      <v-tab :to="{ name: 'DetectionSearchPage' }">
        Detection Search
      </v-tab>
    </v-tabs>
    <v-divider />

    <v-main>
      <router-view />
    </v-main>
  </v-container>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const tab = ref(route.path === '/detectionSearchPage' ? 'DetectionSearchPage' : 'ImageIngestionPage');

if (typeof route.path === 'string') {
  tab.value = route.path === '/detectionSearchPage' ? 'DetectionSearchPage' : 'ImageIngestionPage';
}
</script>
