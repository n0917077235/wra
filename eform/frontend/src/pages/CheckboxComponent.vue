<template>
    <div>
      <v-checkbox :v-model="checked">{{ label }}</v-checkbox>
    </div>
  </template>
  
  <script>
  export default {
    props: ['value', 'label'],
    data() {
      return {
        checked: false,
      };
    },
  };
  </script>
  