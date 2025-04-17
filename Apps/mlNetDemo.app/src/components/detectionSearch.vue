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
      <v-list-item-group>
        <v-list-item
          v-for="(result, index) in results"
          :key="index"
        >
          <v-card
            class="mb-4 mx-auto"
            max-width="600"
          >
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
      </v-list-item-group>
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

const error = ref<string>("");
const searchQuery = ref<string>("");
const expandedIndex = ref<number | null>(null);
const results = ref<ImageDetectionResult[]>([]);
const isSearchValid = computed(() => !!searchQuery.value);
const searchRule = (value: string) => !!value || "Search query cannot be empty";

function toggleExpansion(index: number) {
  expandedIndex.value = expandedIndex.value === index ? null : index;
}

function performSearch() {
  if (!searchQuery.value) {
    error.value = "Please enter a search query.";
    return;
  }

  results.value = [
    {
      fileName: "image1.jpg",
      detectionTimeMilliseconds: 123.45,
      detectionSource: "Camera",
      detectedObjects: [
        { label: "Car", color: "Red", additionalInfo: "Parked" },
        { label: "Person", color: "Blue", additionalInfo: "Walking" },
      ],
    },
    {
      fileName: "image2.jpg",
      detectionTimeMilliseconds: 98.76,
      detectionSource: "Drone",
      detectedObjects: [
        { label: "Tree", color: "Green", additionalInfo: "Oak" },
      ],
    },
  ];
  error.value = "";
}
</script>

<style scoped>
</style>
