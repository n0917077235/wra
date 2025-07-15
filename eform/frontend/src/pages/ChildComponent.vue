<template>
    <div>
      <h1>Child Component</h1>
      <p>Table Type: {{ tableType }}</p>
      <p>Record Time: {{ recordTime }}</p>
      <!-- Render child details here -->
    </div>
  </template>
  
  <script>
  import { useRoute } from 'vue-router';
  
  export default {
    setup() {
      const route = useRoute();
      const tableType = route.params.tableType;
      const recordTime = route.params.recordTime;
  
      return {
        tableType,
        recordTime,
      };
    },
  };
  </script>
  
  <style scoped>
  /* Add any necessary styles */
  </style>
  