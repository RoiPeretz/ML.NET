<template>
  <v-container>
    <v-form>
      <v-row>
        <v-col cols="9">
          <v-text-field
            v-model="searchQuery"
            label="Search for an object or using a keyword"
            outlined
            :rules="[searchRule]"
          />
        </v-col>
        <v-col cols="3">
          <v-btn
            color="primary"
            block
            style="height: 56px;"
            :disabled="!isSearchValid"
            @click="performSearch"
          >
            Search
          </v-btn>
        </v-col>
      </v-row>
    </v-form>
    <v-list v-if="results.length">
      <v-list-item
        v-for="(result, index) in results"
        :key="index"
      >
        <v-card
          class="mx-auto"
          max-width="344"
        >
          <v-img
            height="200px"
            :src="getFileUrl(result.fileName)"
          />

          <v-card-title>
            File: {{ result.fileName }}
          </v-card-title>
          <v-card-subtitle>
            Detection Time: {{ result.detectionTimeMilliseconds }} ms
          </v-card-subtitle>
          <v-card-subtitle>
            Source: {{ result.detectionSource }}
          </v-card-subtitle>
          <v-card-actions>
            <v-btn
              color="primary"
              text
              @click="toggleExpansion(index)"
            >
              {{ expandedIndex === index ? 'Hide Details' : 'Show Details' }}
            </v-btn>
            <v-spacer />
            <v-btn
              :icon="expandedIndex === index ? 'mdi-chevron-up' : 'mdi-chevron-down'"
              @click="toggleExpansion(index)"
            />
          </v-card-actions>
          <v-expand-transition>
            <div v-show="expandedIndex === index">
              <v-divider />
              <v-card-text>
                <strong>Detected Objects:</strong>
                <ul>
                  <li
                    v-for="(object, objIndex) in result.detectedObjects"
                    :key="objIndex"
                  >
                    {{ object.label }} ({{ object.color }}): {{ object.additionalInfo }}
                  </li>
                </ul>
              </v-card-text>
            </div>
          </v-expand-transition>
        </v-card>
      </v-list-item>
    </v-list>
    <v-alert
      v-if="error"
      type="error"
    >
      {{ error }}
    </v-alert>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import type { ImageDetectionResult } from "../models/imageDetectionResult";
import { type IBffHubClient } from '../hubs/detactionHubClient';

const error = ref<string>("");
const searchQuery = ref<string>("");
const expandedIndex = ref<number | null>(null);
const results = ref<ImageDetectionResult[]>([]);
const bffHubClient = inject<IBffHubClient>("bffHubClient");
const isSearchValid = computed(() => !!searchQuery.value);
const searchRule = (value: string) => !!value || "Search query cannot be empty";

const getFileUrl = (fileName: string) => {
  return `http://localhost:9000/assets/${fileName}`;
};

function toggleExpansion(index: number) {
  expandedIndex.value = expandedIndex.value === index ? null : index;
}

async function performSearch() {
  if (!searchQuery.value) {
    error.value = "Please enter a search query.";
    return;
  }

  results.value = await bffHubClient?.Query(searchQuery.value) || [];
  console.log("Search results:", results.value);
  await nextTick();
}
</script>

<style scoped>
</style>
