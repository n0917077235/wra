<template>
    
   <v-responsive
    class="mx-auto"
    max-width="1920"
  >
<br/>
<br/>
<br/>
<br/>
<v-row style="font-size: 22px; color:darkorange">
<br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;預定進度管理
</v-row>
<br/>
<br/>

<v-form ref="form" v-model="valid" @submit.prevent="submitForm">
<v-row>
    <v-col cols="12" sm="6">
        <v-select
              v-model="scheduleFormData.maintainType"
              :items="maintainTypes"
              item-text="title"
              item-value="key"
              label="選擇維護項目"
              
            ></v-select>
    </v-col>
</v-row>
<v-row>
    <v-col cols="12" sm="6">
        <v-text-field
            v-model="scheduleFormData.startYearMonth"
            label="啟始年/月(yyyy/MM)"
            :rules="[validateDate]"
            required
            type="input">
        </v-text-field>
    </v-col>
    <v-col>
        <v-btn color="darkgreen" @click="generateDates" style="background-color: cornflowerblue; min-width:100px; max-width: 200px; margin-right: 10px; margin-top: 20px;"  >確認</v-btn>
    </v-col>
</v-row>


<v-row>
    <v-col>
        <!--v-select multiple
        :items="yearMonths" label="選擇維護年/月" class="mx-2"></v-select-->
        <v-list style="height: 300px; max-width:400px; " >
            <v-list-item v-for="date in yearMonths" :key="date" style="height: 30px;">
                <v-row align="center" >
            <v-col cols="auto">
              <v-checkbox style="margin-top:15px;"
                v-model="scheduleFormData.selectedDates"
                :value="date"
              ></v-checkbox>
            </v-col>
            <v-col>
              <v-list-item style="font-size:larger; margin-top: 0px;">
                <v-list-item-title>{{ date }}</v-list-item-title>
              </v-list-item>
            </v-col>
          </v-row>
        </v-list-item>
      </v-list>
    </v-col>
</v-row>
<v-row style="max-width:400px;">
    <v-col>
        <v-btn color="white" @click="selectall" style="background-color: cornflowerblue; min-width:150px; max-width: 200px; margin-right: 10px; margin-top: 20px;">
            全選
        </v-btn>
    </v-col>
    <v-col>
        <v-btn color="white" @click="unselectall" style="background-color: cornflowerblue; min-width:150px; max-width: 200px; margin-right: 10px; margin-top: 20px;">
            取消全選
        </v-btn>
    </v-col>
</v-row>
<v-row style="max-width:400px;">
    <v-col>
        <v-btn color="red" @click="deleteSchedule" style="background-color: cornflowerblue; min-width:150px; max-width: 200px; margin-right: 10px; margin-top: 20px;">
            刪除
        </v-btn>
    </v-col>
    <v-col>
        <v-btn color="darkblue" type="submit" style="background-color: cornflowerblue; min-width:150px; max-width: 200px; margin-right: 10px; margin-top: 20px;">
            儲存
        </v-btn>
    </v-col>
</v-row>
</v-form>
   </v-responsive>

</template>

<script>

import axios from 'axios';
import {reactive, computed, toRefs,onBeforeMount,onMounted,ref,watch} from 'vue'
import vuetify from '@/plugins/vuetify';
const env = import.meta.env;
const apiUrl = window.config.API_URL; //(env.VITE_APP_API_ENDPOINT);
const button = ref();
console.log('url1',`${apiUrl}`);
console.log('url2',`${apiUrl}/api/forms/savescheduledata`);
export default {
    //inheritAttrs:false,
    computed:{
        
    },
    mounted(){


        console.log('url',`${apiUrl}/api/forms/savescheduledata`);

    },
    data() {
      return {
        
        
        
        maintainTypes: [
        
            //{ title: '請選擇', key: '-1' },
            { title: '影像監視系統', key: '0' },
            { title: '水門水位計系統', key: '1' },
            { title: '水門開度計系統', key: '2' },
            { title: '無線電系統', key: '3' },
            { title: '光纖傳輸系統', key: '4' },
            { title: '避雷接地系統', key: '5' },
            { title: '員山子分洪監測系統', key: '6' },
            { title: '堤防安全監測系統', key: '7' },
        ],
        scheduleFormData:{
            selectedDates: [],
            maintainType:null,
            startYearMonth:null,
        },
        yearMonths:[],
        valid: false,
        
      };
    },
    methods:{
        async submitForm() {
            console.log('submitForm');
            console.log('form.data',this.scheduleFormData);
          const postData = {
          scheduleFormData: JSON.stringify(this.scheduleFormData),//this.formData,
          //files:this.productImages,
          };

          console.log('postData',postData);
          
          
          //console.log('filesLength',postData.files.length);
          const endpoint = "/api/forms/savescheduledata";
          const url = `${apiUrl}${endpoint}`;

          console.log('url3',`${apiUrl}/api/forms/savescheduledata`);
          console.log('url4',url);
           axios.post(`${apiUrl}/api/forms/savescheduledata`, postData)
          .then(response => {
            // Handle success response
            console.log('Response:', response.data);
            // Reset form data if needed
            //this.formData = {};
            console.log('存檔完成');
            alert('存檔完成');
          })
          .catch(error => {
            // Handle error
            console.error('存檔錯誤:', error);
            alert('存檔錯誤:'+error);
          });
        },
        
        validateDate(value) {
            const pattern = /^\d{4}\/(0[1-9]|1[0-2])$/;
            return pattern.test(value) || '日期格式 yyyy/MM';
        },
        async generateDates() {
        const form = this.$refs.form;
        if (form && form.validate()) {
            const [year, month] = this.scheduleFormData.startYearMonth.split('/').map(Number);
            const dates = [];
            let currentYear = year;
            let currentMonth = month;

            for (let i = 0; i < 12; i++) {
                dates.push(`${currentYear}/${String(currentMonth).padStart(2, '0')}`);
                currentMonth++;
                if (currentMonth > 12) {
                    currentMonth = 1;
                    currentYear++;
                }
            }

            this.yearMonths = dates;

            const response = await axios.get(`${apiUrl}/api/forms/getscheduledata?startyearmonth=${this.scheduleFormData.startYearMonth}&maintaintype=${this.scheduleFormData.maintainType}`);
            console.log('selected',response.data);
            this.scheduleFormData.selectedDates= response.data;
        } else {
            alert('請輸入正確的年/月格式 yyyy/MM ');
        }
        },
        async deleteSchedule()
        {
            const response = await axios.delete(`${apiUrl}/api/forms/deletescheduledata?startyearmonth=${this.scheduleFormData.startYearMonth}&maintaintype=${this.scheduleFormData.maintainType}`);
            console.log('deleted',response.data);
            if (response.status===200)
            {
                if (response.data > 0)
                {
                    this.scheduleFormData.selectedDates=[];
                    this.scheduleFormData.startYearMonth=null;
                    this.scheduleFormData.maintainType=null;
                    alert('刪除成功');
                }
                else
                {
                    alert('刪除失敗');
                }
            }
            else
            {
                alert(response.data);
            }
        },
        selectall() {
            this.scheduleFormData.selectedDates=[];
            for(var i=0;i<this.yearMonths.length;i++)
            {
                this.scheduleFormData.selectedDates.push(this.yearMonths[i]);
            }
        },
        unselectall(){
            this.scheduleFormData.selectedDates=[];
        }        
    },
}

</script>