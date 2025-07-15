<template>
    
  
 <br/>
 <br/>
 <br/>
 <br/>
 <v-row style="font-size: 22px; color:darkorange">
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;維護資料查詢
 </v-row>
 <v-row>
     <v-col cols="4" sm="6">
         <v-text-field
             v-model="ym"
             label="維護年度/月份(YYYY/MM)"
             :rules="[validateDate]"
             required
             type="input">
         </v-text-field>
     </v-col>
 </v-row>
<v-row>
    <v-col cols="4" sm="6">
        <v-btn color="darkgreen" @click="maintainQuery" style="background-color: cornflowerblue; min-width:100px; margin-left:10px; "  >查&nbsp;&nbsp;詢</v-btn>
    </v-col>
</v-row>
<v-row>   
     <v-col cols="4" sm="6">
        <v-text-field
      v-model="search"
      label=" 搜&nbsp;&nbsp;尋"
      clearable
    ></v-text-field>
     </v-col>
 </v-row>
 
 
 <v-data-table 
 :items="filteredItems"
 :headers="headers"
 item-key="row"
  :items-per-page="10"
  no-data-text="無資料"
  fixed-header
  items-per-page-text="每頁顯示筆數"
   loading-text="載入中"
  :pageText="'{0}-{1} 之 {2}'"
 >
 <template v-slot:item.download="{ item }">
        <v-btn
          icon
          @click="downloadFile(item.fileUrl)"
        >
          <v-icon>mdi-download</v-icon>
        </v-btn>
      </template>
 
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
        filteredItems() {
            return this.items.filter(item => {
                return (
                item.location.toLowerCase().includes(this.search.toLowerCase()) ||
                item.station.toString().toLowerCase().includes(this.search.toLowerCase()) ||
                item.tabletype.toString().toLowerCase().includes(this.search.toLowerCase()))
            });
        },
     },
     mounted(){
 
 
         //console.log('url',`${apiUrl}/api/forms/queryschedule?${this.year}`);
 
     },
     
     data() {
        let d=new Date();
        d.setHours(d.getHours() + 8);
        let ddd=d.toISOString().split('/');
       return {
        search: "",
         items:[],
         yearMonth:ddd[0]+'/'+ddd[1],
         
         headers: [
         
             { title: '序號', key: 'row' ,align: 'center', sortable: true, },
             { title: '年-月-日', key: 'recordtime' ,align: 'center', sortable: true,},
             { title: '站台', key: 'station' ,align: 'center', sortable: true,},
             { title: '維護保養地點', key: 'location' ,align: 'center', sortable: true,},
             { title: '維護保養項目', key: 'tabletype' ,align: 'center', sortable: true,},
             { title: '檔案下載', key: 'download' ,align: 'center', sortable: false,},
         ],
         
         valid: false,
         ym:'',
         
       };
     },
     methods:{
         
       
        validateDate(value) {
            if (value==null || value.length !=7)
            {
                return '年度/月份(格式YYYY/MM)';
            }
            value=value.substring(0,7);
            const pattern = /^(19|20)\d{2}\/(0[1-9]|1[0-2])$/;
              const regex = /^(19|20)\d{2}\/(0[1-9]|1[0-2])$/;
              this.ym=value;
            return pattern.test(value) || '年度/月份(格式YYYY/MM)';
         },
        async maintainQuery() {
            const ym=this.ym.split('T')[0].substring(0,7);
            //console.log(`${apiUrl}/api/maintainquery?yearmonth=${ym}`);
            const response = await axios.get(`${apiUrl}/document/maintainquery?yearmonth=${ym}`);
            //console.log('selected',response.data);
            this.items= response.data;
            //console.log('items',this.items);
         },
        async downloadFile(fileUrl) {
            //console.log('download');
            //console.log('fileurl',fileUrl);
            
            const urls=fileUrl.split(',');
            const encodedLocation = encodeURIComponent(urls[1]);
            //console.log('url length',urls.length);
            const url=`${apiUrl}/document/Download?stationid=${urls[0]}&location=${encodedLocation}&tableType=${urls[2]}&recordtime=${urls[3]}`;
            //console.log(url);
            //alert(url);
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', '');
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
        },
     },
 }
 
 </script>