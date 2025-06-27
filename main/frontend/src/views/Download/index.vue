<template>
  <div>
    <ul>
      <FileTreeResponse :node="treeData" />
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { apiGetFileTree, FileTreeResponse } from '@/resource/download';

const treeData = ref<FileTreeResponse | null>(null);

onMounted(async () => {
  try {
    const response = await apiGetFileTree();
    if (response) {
      treeData.value = response;
      console.log(response);
    }
  } catch (error) {
    console.error(error);
  }
});
</script>

<style scoped></style>
