<template>
    
  
 <br/>
 <br/>
 <br/>
 <br/>
 <v-row style="font-size: 22px; color:darkorange">
 <br/>
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;預定進度查詢
 </v-row>
 <br/>
 <br/>
 
 
 
 <v-row>
     <v-col cols="4" sm="6">
         <v-text-field
             v-model="year"
             label="維護年度(yyyy)"
             :rules="[validateDate]"
             required
             type="input">
         </v-text-field>
     </v-col>
     <v-col>
         <v-btn color="darkgreen" @click="scheduleQuery" style="background-color: cornflowerblue; min-width:100px; max-width: 200px; margin-right: 10px; margin-top: 20px;"  >查詢</v-btn>
     </v-col>
 </v-row>
 
 <v-data-table 
 :items="items"
 :headers="headers"
 item-key="row"
 hide-default-footer
  disable-pagination
  :items-per-page="-1"
  no-data-text="無資料"
 >
 
 </v-data-table>
 
 
<br/>
<br/>
<br/>
<br/>
<br/>
 </template>
 
 <script>
 
 import axios from 'axios';
 import {reactive, computed, toRefs,onBeforeMount,onMounted,ref,watch} from 'vue'
 import vuetify from '@/plugins/vuetify';
 const env = import.meta.env;
 const apiUrl = window.config.API_URL; //(env.VITE_APP_API_ENDPOINT);
 const button = ref();
 
 export default {
     //inheritAttrs:false,
     
     computed:{
         
     },
     mounted(){
 
 
         //console.log('url',`${apiUrl}/api/forms/queryschedule?${this.year}`);
 
     },
     
     data() {
        let d=new Date();
        d.setHours(d.getHours() + 8);
              
       return {
         items:[],
         year:d.toISOString().split('-')[0],
         
         headers: [
         
             { title: '序號', key: 'row' ,align: 'center', sortable: false, },
             { title: '年/月', key: 'yearMonth' ,align: 'center', sortable: false,},
             { title: '影像監視系統', key: 'item1',align: 'center', sortable: false, },
             { title: '水門水位計系統', key: 'item2' ,align: 'center', sortable: false,},
             { title: '水門開度計系統', key: 'item3' ,align: 'center', sortable: false,},
             { title: '無線電系統', key: 'item4' ,align: 'center', sortable: false,},
             { title: '光纖傳輸系統', key: 'item5',align: 'center', sortable: false, },
             { title: '避雷接地系統', key: 'item6' ,align: 'center', sortable: false,},
             { title: '員山子分洪監測系統', key: 'item7' ,align: 'center', sortable: false,},
             { title: '堤防安全監測系統', key: 'item8' ,align: 'center', sortable: false,},
         ],
         
         valid: false,
         
       };
     },
     methods:{
         
         
         validateDate(value) {
             const pattern = /^\d{4}\/(0[1-9]|1[0-2])$/;
             return pattern.test(value) || '年度格式 yyyy';
         },
         async scheduleQuery() {
         
       
             const response = await axios.get(`${apiUrl}/api/forms/scheduleQuery?year=${this.year}`);
             console.log('selected',response.data);
             this.items= response.data;
            console.log('items',this.items);
         },
                
     },
 }
 
 </script>