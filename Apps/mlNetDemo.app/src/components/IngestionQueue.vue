<template>
  <v-container class="d-flex flex-column">
    <h2>Queue</h2>
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
              <v-list-item-title>
                {{ getEventTitle(item) }}
              </v-list-item-title>
              <v-list-item-subtitle>
                {{ getDateTime(item) }}
              </v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-container>
</template>

<script setup lang="ts">
import type { IngestionStatusEvent } from "../models/ingestionStatusEvent";
import { useAppStore } from "../stores/app";

const store = useAppStore();

function getEventTitle(event: IngestionStatusEvent): string {
  let result = event.message ?? "";

  if (event.modelName) {
    result += ": " + event.modelName;
  }

  if (event.detectionTime) {
    return (result += " (" + msToHumanReadable(event.detectionTime) + ")");
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
    parts.push(`${days} day${days !== 1 ? "s" : ""}`);
  }
  if (hours > 0) {
    parts.push(`${hours} hour${hours !== 1 ? "s" : ""}`);
  }
  if (minutes > 0) {
    parts.push(`${minutes} minute${minutes !== 1 ? "s" : ""}`);
  }
  if (seconds > 0 || parts.length === 0) {
    parts.push(`${seconds} second${seconds !== 1 ? "s" : ""}`);
  }

  return parts.join(", ");
}
</script>
