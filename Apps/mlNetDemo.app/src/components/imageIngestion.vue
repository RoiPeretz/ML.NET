<template>
  <v-container class="d-flex flex-column">
    <h2>Upload</h2>
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
</template>

<script setup lang="ts">
import type { IDetactionHubClient } from "../hubs/detactionHubClient";
import { ref, inject } from "vue";
import { VFileUpload } from "vuetify/labs/VFileUpload";

const selectedFiles = ref<File[]>();
const detectionHubClient = inject<IDetactionHubClient>("detectionHubClient");

async function startIngest() {
  if (!selectedFiles.value || selectedFiles.value.length === 0) {
    console.warn("No files selected for ingestion.");
    return;
  }

  try {
    for (const file of selectedFiles.value) {
      const reader = new FileReader();

      reader.onload = async function (e) {
        const arrayBuffer = e.target?.result;
        if (!arrayBuffer) {
          console.error("Failed to read file as ArrayBuffer.");
          return;
        }
        if (arrayBuffer instanceof ArrayBuffer) {
          detectionHubClient?.detect(file.name, arrayBuffer, file.type);
        } else {
          console.error("File result is not an ArrayBuffer.");
        }
      };

      reader.readAsArrayBuffer(file);
    }
  } catch (error) {
    console.error("Error while sending files to detectionHubClient:", error);
  } finally {
    selectedFiles.value = [];
  }
}

</script>
