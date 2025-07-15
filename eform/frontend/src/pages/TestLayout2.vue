<template>
    <v-app>
      <v-container>
        <v-row v-for="(row, rowIndex) in processedRows" :key="rowIndex">
          <!-- Headers Row -->
          <v-col v-for="(header, headerIndex) in row.headers" :key="headerIndex" :cols="getColumnWidth(header)">
            <strong>{{ header }}</strong>
          </v-col>
          
          <!-- Fields Row -->
          <v-col v-for="(item, itemIndex) in row.fields" :key="itemIndex" :cols="getColumnWidth(item)">
            <component :is="getComponent(item.type)"
                       :v-model="getFieldValue(item)"
                       :item="item"
            />
          </v-col>
        </v-row>
      </v-container>
    </v-app>
  </template>
  
  <script>
  import { ref, computed } from 'vue';
  import TextComponent from './TextComponent.vue';
  import CheckboxComponent from './CheckboxComponent.vue';
  import LabelComponent from './LabelComponent.vue';
  
  export default {
    components: {
      TextComponent,
      CheckboxComponent,
      LabelComponent,
    },
    setup() {
      const inputRows = ref([
        { 
          row: {
            headers: "檢查標準,實際檢查情況,檢查結果,備註",
            fields: "L:AAAA;C:正常,異常;C:正常,異常;T" 
          },
        },
        { 
          row: {
            headers: "檢查標準,實際檢查情況,檢查結果,備註",
            fields: "L:BBBB;T:M;C:正常,異常;T" 
          }
        }
      ]);
  
      const processedRows = computed(() =>
        inputRows.value.map(rowData => {
          const headers = rowData.row.headers.split(',');
          const fields = rowData.row.fields.split(';').map((item, index) => {
            const [type, ...values] = item.split(':');
            return { type, values: values.join(':').split(','), header: headers[index] };
          });
          return { headers, fields };
        })
      );
  
      const getComponent = (type) => {
        switch (type) {
          case 'L':
            return 'LabelComponent';
          case 'C':
            return 'CheckboxComponent';
          case 'T':
            return 'TextComponent';
          default:
            return 'div';
        }
      };
  
      const getFieldValue = (item) => {
        // Logic to get or set the value based on the item type (L, C, T)
        switch (item.type) {
          case 'L':
            return item.values[0]; // Example: return label value for LabelComponent
          case 'C':
            return item.values.filter(value => value !== '').map(() => false); // Initialize checkbox values
          case 'T':
            return ''; // Initialize text field value
          default:
            return '';
        }
      };
  
      const getColumnWidth = (item) => {
        // Adjust column width based on item type or other conditions as needed
        return item.type === 'C' ? 'auto' : '200px'; // Example: set 'auto' for checkboxes, '200px' for others
      };
  
      return {
        processedRows,
        getComponent,
        getFieldValue,
        getColumnWidth,
      };
    },
  };
  </script>
  
  <style>
  .checkbox-container {
  display: flex; /* Ensures items are laid out in a row */
  align-items: center; /* Aligns items vertically in the center */
}
  </style>
  