
<template>


    <v-container>
    
        <br/>
  <br/>
  <br/>
  <br/>

  <v-row style="font-size: 22px; color:darkorange">
<br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;備註欄管理
</v-row>
<br/>
<br/>
        
        <v-combobox
        v-model="stationName"
        label="維護保養地點"
        :items="itemStations"
        item-title="text"
        item-value="value"
        variant="solo"
        class="pa-1"
        style="width: 300px;"
        return-object
    ></v-combobox>
        <v-combobox
                v-model="category"
                label="維護保養項目"
                :items="itemCategories"
                item-title="text"
                item-value="value"
                style="width: 300px;"
                
              ></v-combobox>
        <v-btn 
                @click="fetchData" 
                class="load-button"
                color="#5865f2"
                size="medium"
                variant="flat" >載入表格及資料
        </v-btn>
        <br/>
        <br/>
      <v-data-table
        :headers="headers"
        :items="items"
        item-key="itemName"
        class="elevation-1"
        height="350"
        fixed-header
        items-per-page-text="每頁顯示筆數"
        no-data-text="無資料"
        loading-text="載入中"
        :pageText="'{0}-{1} 之 {2}'"
        @click:row="(item,row) => rowClick(item,row)" 
        :item-class="() => 'getRowClass'"
        single-select
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title>管理表單備註欄</v-toolbar-title>
            <v-divider class="mx-4" inset vertical></v-divider>
            <v-spacer></v-spacer>
            <v-dialog v-model="dialog" max-width="600px">
              <template v-slot:activator="{ props }">
                <v-btn color="primary" dark class="mb-2" v-bind="props">
                  新增
                </v-btn>
              </template>
              <v-card>
                <v-card-title>
                  <span class="headline">{{ formTitle }}</span>
                </v-card-title>
                <v-card-text>
                  <v-container>
                    <v-row>
                      <v-col
                        v-for="(header, index) in headers"
                        :key="index"
                        cols="12"
                        sm="6"
                        md="4"
                      >
                        <v-text-field
                          v-model="editedItem[header.key]"
                          :label="header.title"
                          v-if="header.key !== 'actions'"
                        ></v-text-field>
                      </v-col>
                    </v-row>
                  </v-container>
                </v-card-text>
                <v-card-actions>
                  <v-spacer></v-spacer>
                  <v-btn color="blue darken-1" text @click="closeDialog">
                    取消
                  </v-btn>
                  <v-btn color="blue darken-1" text @click="save">
                    儲存
                  </v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
          </v-toolbar>
        </template>
        <template v-slot:[`item.actions`]="{ item }">
          <v-icon small @click="editItem(item)">mdi-pencil</v-icon>
          <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-container>
  </template>
  
  <script>
  import axios from 'axios';
  import {reactive, toRefs,onBeforeMount,onMounted,ref,watch} from 'vue'
  import vuetify from '@/plugins/vuetify';
  const env = import.meta.env;
  const apiUrl = window.config.API_URL; //(env.VITE_APP_API_ENDPOINT);
  

  export default {
    mounted() {
      //this.fetchFormFields(); 
      //this.loadPreData();
      this.loadPreData();
    },
    data() {
      return {
        stationName:null,
        category:null,
        dialog: false,
        itemStations:[],
        itemCategories:[],
        selectedRows: [],
        headers: [
          { title: '欄位名稱', key: 'itemName' },
          { title: '元件英文名稱', key: 'fieldName' },
          { title: '顯示順序', key: 'seq' },
          { title: '編輯/刪除', key: 'actions', sortable: false }
          //{ title: 'PHONE', key: 'phone' },
          //{ title: 'Actions', key: 'actions', sortable: false }
        ],
        items: [],
        editedIndex: -1,
        editedItem: {
          itemName: '',
          fieldName: '',
          seq: '',
          sequence:'',
        },
        defaultItem: {
          itemName: '',
          fieldName: '',
          seq: '',
          sequence:'',
        }
      };
    },
    
   
    computed: {
      formTitle() {
        return this.editedIndex === -1 ? '新增項目' : '編輯項目';
      }
    },
    watch: {
      dialog(val) {
        val || this.closeDialog();
      }
    },
    methods: {
        async loadPreData()
        {
            try {
                
                
                console.log('api:'+ `${apiUrl}/api/forms/getstationcategory`);
                const response = await axios.get(`${apiUrl}/api/forms/getstationcategory`);
                console.log(response);
                console.log(response.data);
                this.itemStations = response.data.lstStations ;
                this.itemCategories = response.data.lstCategories;
                console.log('stations', response.data);
                console.log(this.itemStations);
                //console.log(this.itemCategories);
                //console.log(this.itemCategories);
                //let d=new Date();
                //d.setHours(d.getHours() + 8);
                //this.formData['formDate']=d.toISOString().substr(0, 10);
            } catch (error) {
                console.error('載入站台及類型錯誤:', error);
                alert('載入站台及類型錯誤:'+ error);
            }
        },
        async loadFormAndData()
        {
            try
            {

            }
            catch(e)
            {
                
            }
        },
      async fetchData() {
        try {
            console.log('call fetchData',`${apiUrl}/api/forms/getitems?station=${this.stationName.value}&category=${this.category.value}`);
          const response = await axios.get(`${apiUrl}/api/forms/getitems?station=${this.stationName.value}&category=${this.category.value}`);
          this.items = response.data;
          console.log(this.items);
        } catch (error) {
          console.error(error);
        }
      },
      editItem(item) {
        this.editedIndex = this.items.indexOf(item);
        this.editedItem = Object.assign({}, item);
        this.dialog = true;
      },
      async deleteItem(item) {
        const index = this.items.indexOf(item);
        if (confirm('確認刪除['+item.itemName+']?')) {
          try {
            //const response= await axios.delete(`${apiUrl}/api/forms/deleteitem?station=${this.stationName.value}&category=${this.category.value}&itemname=${item.itemName}&seq=${item.seq}`);
            console.log("seq:",item.sequence);
            const response= await axios.delete(`${apiUrl}/api/forms/item/delete?sequence=${item.sequence}`);
            if (response.status===200)
            {
              this.items.splice(index, 1);
            }
          } catch (error) {
            console.error(error);
          }
        }
      },
      closeDialog() {
        this.dialog = false;
        this.editedItem = Object.assign({}, this.defaultItem);
        this.editedIndex = -1;
      },
      async save() {
        try {
          if (this.editedIndex > -1) {
            console.log(`${apiUrl}/api/forms/item/edit/${this.editedItem.sequence}`);
            const response=await axios.post(`${apiUrl}/api/forms/item/edit/${this.editedItem.sequence}`,this.editedItem);
            if (response.status==200)
              Object.assign(this.items[this.editedIndex], this.editedItem);
          } else {
            console.log('Sending request to:', `/api/forms/item/add/${this.stationName.value}/${this.category.value}`);
            console.log('Request payload:', this.editedItem);
            const response = await axios.post(`${apiUrl}/api/forms/item/add/${this.stationName.value}/${this.category.value}`, this.editedItem,{
              headers: {
                'Content-Type': 'application/json'
              }
            });
            this.items.push(response.data);
          }
          this.closeDialog();
        } catch (error) {
          console.error(error);
        }
      },
      getRowClass(item) {
        console.log('getRowClass');
        console.log('item',item);
        return this.selectedRows.includes(item.id) ? 'selected-row' : '';
      },
      rowClick: function (item, row ) {      
        console.log('row',row);
        //console.log('item',item.value);
        //return;
        const id = row.item.itemName // just temporary
        console.log('item',row.item.itemName);
        const rowNo = this.selectedRows.indexOf(id)
        if(rowNo == -1) {
          this.selectedRows.push(id)
          //row.select(true);
        } else {
          this.selectedRows.splice(rowNo, 1)
          ///row.select(false);
        }
        console.log(this.selectedRows)
      },
      log: function (event) {      
        console.log(event)
      }
    },
    created() {
      //this.fetchData();
    }
  };
  </script>
  
  <style scoped>
  .label-container {
    display: inline-block;
    margin-left: 15px;
    vertical-align: top;
  }
  
  .field-container {
    display: inline-flex;
    flex-wrap: wrap;
    margin-left: 10px;
  }
  
  .text-field-wrapper {
    flex: 1;
    min-width: 100px;
    margin-right: 10px;
  }
  
  @media (max-width: 600px) {
    .text-field-wrapper {
      flex: 0 1 45%;
      margin-bottom: 10px;
    }
  }
  .selected-row {
  background-color: #e0f7fa; /* Change to your desired color */
}
tr.v-data-table__selected {
  background: #7d92f5 !important;
}
  </style>
  