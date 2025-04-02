<!-- eslint-disable vue/multiline-html-element-content-newline -->
<!-- eslint-disable vue/html-indent -->
<template>
  <v-row>
    <v-col>
      <v-container class="d-flex flex-column">
        <h2> Upload </h2> 
        <v-file-upload 
          v-model="selectedFiles"
          multiple
          clearable
          density="default" 
        />
        <v-btn
          v-show="selectedFiles && selectedFiles?.length > 0"
          color="primary"
          append-icon="mdi-arrow-right-circle"
          @click="startIngest()"
        >
          Start
        </v-btn>
      </v-container>
    </v-col>
    <v-col>
      <v-container class="d-flex flex-column">
        <h2> Queue </h2> 
        <v-expansion-panels>
          <v-expansion-panel
            v-for="kvp in store.$state.currentStatus"
            :key="kvp[0]"
            :title="kvp[0]"
          >
            <v-expansion-panel-text>
              <v-list
                v-for="(item, index) in kvp[1]"
                  :key="index"
              >
              <v-list-item>
                <v-list-item-title>{{ getEventTitle(item) }}</v-list-item-title>
                <v-list-item-subtitle>{{ getDateTime(item) }}</v-list-item-subtitle>
                  </v-list-item>
                </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>
      </v-container>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import type { IDetactionHubClient } from '@/hubs/detactionHubClient';
import type { IngestionStatusEvent } from '@/models/ingestionStatusEvent';
import { useAppStore } from '@/stores/app';
import { VFileUpload } from 'vuetify/labs/VFileUpload'

const store = useAppStore();
const selectedFiles = ref<File[]>();
const detectionHubClient = inject<IDetactionHubClient>('detectionHubClient');

async function startIngest() {
  if (!selectedFiles.value || selectedFiles.value.length === 0) {
    console.warn('No files selected for ingestion.');
    return;
  }

  try {
    for (const file of selectedFiles.value) {
        const reader = new FileReader();
        
        reader.onload =  async function(e) {
            const arrayBuffer = e.target?.result;
            if (!arrayBuffer) {
              console.error('Failed to read file as ArrayBuffer.');
              return;
            }
            if (arrayBuffer instanceof ArrayBuffer) {
              detectionHubClient?.detect(file.name, arrayBuffer, file.type);
            } else {
              console.error('File result is not an ArrayBuffer.');
            }
        };
        
        reader.readAsArrayBuffer(file);
      }
   
  } catch (error) {
    console.error('Error while sending files to detectionHubClient:', error);
  } finally {
    selectedFiles.value = [];
  }
}

function getEventTitle(event: IngestionStatusEvent): string {
  let result = event.message ?? "";

  if (event.modelName) {
     result += ": " + event.modelName;
  }

  if (event.detectionTime) {
     return result += " (" + msToHumanReadable(event.detectionTime) + ")";
  }
  return result;
}

function getDateTime(event: IngestionStatusEvent): string {
  const date = new Date(event.creationDate);
  return date.toLocaleString();
}

function msToHumanReadable(ms: number): string {
  let remaining = ms;

  const days = Math.floor(remaining / 86400000);
  remaining %= 86400000;

  const hours = Math.floor(remaining / 3600000);
  remaining %= 3600000;

  const minutes = Math.floor(remaining / 60000);
  remaining %= 60000;

  const seconds = Math.floor(remaining / 1000);

  const parts: string[] = [];
  if (days > 0) {
    parts.push(`${days} day${days !== 1 ? 's' : ''}`);
  }
  if (hours > 0) {
    parts.push(`${hours} hour${hours !== 1 ? 's' : ''}`);
  }
  if (minutes > 0) {
    parts.push(`${minutes} minute${minutes !== 1 ? 's' : ''}`);
  }
  // Include seconds if they're nonzero, or if no other parts exist.
  if (seconds > 0 || parts.length === 0) {
    parts.push(`${seconds} second${seconds !== 1 ? 's' : ''}`);
  }

  return parts.join(', ');
}
</script>
