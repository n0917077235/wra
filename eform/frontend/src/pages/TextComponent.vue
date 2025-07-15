<template>
    <div>
      <v-text-field :v-model="value"></v-text-field>
    </div>
  </template>
  
  <script>
  export default {
    props: ['value'],
  };
  </script>
  