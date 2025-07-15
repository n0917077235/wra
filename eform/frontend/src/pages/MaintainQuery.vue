<br/>
<br/>
<br/>
<br/>
<br/>

<template>
  <br/>
<br/>
<br/>
<br/>
<br/>
  <v-container>
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field style="width: 300px;"
            v-model="startDate"
            label="資料啟始日期"
            type="date"
            @input="formatDate(startDate)"
          ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field style="width: 300px;"
            v-model="endDate"
            label="資料結束日期"
            type="date"
            @input="formatDate(endDate)"
          ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-btn @click="loadData">查詢資料</v-btn>
      </v-col>
    </v-row>
    <br/>
    <br/>
    <v-text-field
      v-model="search"
      label="搜尋"
      class="mb-4"
    ></v-text-field>
    <v-container>
    <v-data-table
      :headers="headers"
      :items="filteredItems"
      item-value="id"
      class="elevation-1"
      itemsPerPage=10
        items-per-page-text="每頁顯示筆數"
        no-data-text="無資料"
        loading-text="載入中"
        :pageText="'{0}-{1} 之 {2}'">
        <!--template v-slot:body="{ items, headers }">
            <tbody>
                <tr v-for="item in items" :key="item.id">
                  <div>
                          <td>{{ item.recordTime }}</td>
                          <td> {{ item.stationId }}</td>
                            <td>{{ item.stationName }}</td>
                          
                        </div>
                      
                </tr>
            </tbody>
        </template-->
      <template v-slot:body="{ items }">
        
          <template  v-for="item in items" :key="item.id" >
            <tr style="width: 100%;" >
              <td v-if="item.lstHistoryDetail && item.lstHistoryDetail.length" @click="toggleExpand(item) ">
                <v-icon>{{ expanded.includes(item.id) ? 'mdi-minus' : 'mdi-plus' }}</v-icon>
              </td>
              <td v-else></td>
              <td >{{ item.recordTime }}</td>
              <td >{{ item.stationId }}</td>
              <td >{{ item.stationName }}</td>
              
            </tr>
        
            <template v-if="expanded.includes(item.id)">
            <tr>
                <td colspan="4">
                  <v-data-table :headers="childHeaders" :items="paginatedChildren(item)" item-value="id" class="elevation-1" :hide-default-footer="true">
                    <template v-slot:body="{ items }">
                        <template v-for="child in items" :key="child.id" >
                          <tr @click="redirectToDetail(child,item.recordTime)" style="cursor: pointer;">
                          <td>{{ child.id }}</td>
                          <td>{{ child.tableTypeName }}</td>
                        </tr>
                        </template>
                      
                    </template>
                  </v-data-table>
                </td>
              </tr>
              <tr>
                <td colspan="4">
                  
                  <v-pagination
                    v-model:page="pagination[item.id].page"
                    :length="totalPages(item)"
                    :total-visible="5"
                    
                    @update:model-value="(page) => handlePageChangeInput(item.id, page)"
                    @next="handlePageChangeNext(item)"
                    @prev="handlePageChangePrev(item)"
                  ></v-pagination>
                  
             
                </td>
              </tr>
              
            </template>
          </template>
        
  
      </template>
    </v-data-table>
  </v-container>

  </v-container>
</template>

<script>
import { ref, computed } from 'vue';
import axios from 'axios';
import router from "../router"; 
import ChildComponent from './MaintainFormHistory.vue';

import { useRoute, useRouter } from 'vue-router';

const env = import.meta.env;
const apiUrl = window.config.API_URL; //(env.VITE_APP_API_ENDPOINT);

export default {
  components: { ChildComponent },
  
  data(){
    /*
    var d1=new Date();
    d1=d1.setDate(d1.getDate()-14);
    d1=new Date(d1);
    d1=d1.setHours(d1.getHours() + 8);
    console.log('d1',new Date(d1).toISOString().substr(0, 10));

    var d2=new Date();
    d2=d2.setHours(d2.getHours() + 8);
    return {
                  
      startDate: new Date(d1).toISOString().substr(0, 10),
      endDate: new Date(d2).toISOString().substr(0, 10),
      tableData: [],
    };
    */
  },
  
  setup() {
    const search = ref('');
    
    var d1=new Date();
    d1=d1.setDate(d1.getDate()-14);
    d1=new Date(d1);
    d1=d1.setHours(d1.getHours() + 8);
    console.log('d1',new Date(d1).toISOString().substr(0, 10));

    var d2=new Date();
    d2=d2.setHours(d2.getHours() + 8);
    
    const headers = ref([
      { title: '+/-', key: 'expend',width:'10%' },
      { title: '維護日期', key: 'recordTime',width:'40%'},
      { title: '站碼', key: 'stationId',width:'20%' },
      { title: '站名', key: 'stationName',width:'30%'},
    ]);

    const childHeaders = ref([
      { title: 'ID', key: 'id',width:'20%' },
      { title: '維護類型', key: 'tableTypeName',width:'40%' },
    ]);

    const startDate = ref(new Date(d1).toISOString().substr(0, 10));
    const endDate = ref(new Date(d2).toISOString().substr(0, 10));
    const items = ref([]);
    
    const expanded = ref([]);
    const pagination = ref({});
    
    const itemsPerPage = 2;

    const loadData = async () => {
      try {
        console.log('loadData');
        const response = await axios.get(`${apiUrl}/api/forms/maintainquery?startDate=${startDate.value}&endDate=${endDate.value}`);
        items.value = response.data.lstHistoryHeader;
        console.log('items',items.value);
        
        items.value.forEach(item => {
          console.log('child',item.lstHistoryDetail);
          pagination.value[item.id] = {
            page: 1,
            itemsPerPage: 5,
            children: item.lstHistoryDetail,  // Directly use fetched children data
          };
        });
      } catch (error) {
        console.error('Error fetching items:', error);
      }
    };
    const redirectToDetail = (child, recordTime) => {
      console.log('call history query');
      route.push({
        name: 'MaintainFormHistory',
        params: { tableType: child.tableType, recordTime: recordTime },
      });
    };
    
    /*
    const toggleExpand = (item) => {
      const index = expanded.value.indexOf(item.id);
      if (index === -1) {
        expanded.value.push(item.id);
        if (!pagination.value[item.id]) {
          pagination.value[item.id] = {
            page: 1,
            itemsPerPage,
          };
        }
      } else {
        expanded.value.splice(index, 1);
      }
    };

    */

    const toggleExpand = (item) => {
      const index = expanded.value.indexOf(item.id);
      if (index === -1) {
        expanded.value.push(item.id);
      } else {
        expanded.value.splice(index, 1);
      }
    };

    const paginatedChildren = (item) => {
      const pag = pagination.value[item.id];
      if (!pag) {
        return [];
      }
      const start = (pag.page - 1) * pag.itemsPerPage;
      const end = start + pag.itemsPerPage;
      return pag.children.slice(start, end);
    };

    const totalPages = (item) => {
      return Math.ceil(item.lstHistoryDetail.length / pagination.value[item.id].itemsPerPage);
    };

    const handlePageChange = (item,page) => {
      // Ensure the pagination state updates reactively
      //console.log('child page [0]',item.id);
      //console.log('child page [1]',pagination.value[item.id]);
      //console.log('page',{ ...pagination.value[item.id] });
      //console.log('page [2]', pagination.value);
      console.log('child page [0]:',pagination.value[item.id].page);
      console.log('child page [1]:',page);
      pagination.value[item.id].page= page;
      //pagination.value[item.id].page++;
      console.log('child page [2]',pagination.value[item.id]);
    };
    const handlePageChangeNext = (item) => {
      // Ensure the pagination state updates reactively
      console.log('page [next]', pagination.value);
      //pagination.value[item.id] = { ...pagination.value[item.id] };
      var pages=totalPages(item);
      console.log('page ',pagination);
      //console.log('page ',pagination.value);
      //console.log('page ',pagination.value.length);
      if (pagination.value[item.id].page+1 < pages+1)
      {
        pagination.value[item.id].page++;
      }
      //console.log('child page [2]',pagination.value[item.id]);
    };

    const handlePageChangePrev = (item) => {
      // Ensure the pagination state updates reactively
      console.log('page [prev]', pagination.value);
      //pagination.value[item.id] = { ...pagination.value[item.id] };
      //pagination.value[item.id].page++;
      if (pagination.value[item.id].page-1 >= 1)
      {
        pagination.value[item.id].page--;
      }
    };

    

    const filteredItems = computed(() => {
      if (!search.value) {
        return items.value;
      }
      return items.value
        .map(item => ({
          ...item,
          children: item.children.filter(child => {
            return child.name.toLowerCase().includes(search.value.toLowerCase()) ||
                   child.category.toLowerCase().includes(search.value.toLowerCase()) ||
                   child.date.includes(search.value);
          })
        }))
        .filter(item => {
          return item.name.toLowerCase().includes(search.value.toLowerCase()) ||
                 item.category.toLowerCase().includes(search.value.toLowerCase()) ||
                 item.date.includes(search.value) ||
                 item.children.length;
        });
    });

    const route = useRouter();

    return {
      search,
      headers,
      filteredItems,
      childHeaders,
      expanded,
      pagination,
      toggleExpand,
      paginatedChildren,
      totalPages,
      handlePageChange,
      handlePageChangeNext,
      handlePageChangePrev,
      loadData,
      startDate,
      endDate,
      redirectToDetail,
      
    };
  },
  methods:{
    methods: {
    fireEvent() {
      this.$emit('childEvent', { data: 'Hello from child!' });
    },
  },
    onMounted:{
      
    },
    handlePageChangeInput(id,page) {

      //console.log('child page',page);
      // Ensure the pagination state updates reactively
      //console.log('obj1',Object.values(this.pagination));
      
      //console.log('currentItem',currentItem);
      console.log('page value',this.pagination[id].page);
      console.log('id',id);
      this.pagination[id].page = page;
    },
    async loadData2 () {
      console.log('startdate',this.startDate);
      console.log('enddate',this.endDate);
      try
      {
          console.log('url',`${apiUrl}/api/maintainquery?startDate=${this.startDate}&endDate=${this.endDate}`);
          const response = await axios.get(`${apiUrl}/api/forms/maintainquery?startDate=${this.startDate}&endDate=${this.endDate}`);
          items=response.data;
          console.log('items',items);
          //
          //items.value = data;
      }
      catch(e)
      {
          alert(e);
      }

    },
    goToDetails(id){
      // Navigate to the details page
      this.$router.push({ name: 'DetailsPage', params: { id } });
    },
    formatDate(selectedDate) {
      
      if (selectedDate) {
        if (selectedDate === "NaN-NaN-NaN")
        {
          let d=new Date();
          d.setHours(d.getHours() + 8);
          console.log('formateDate 1',d.toISOString().substr(0, 10));
          return d.toISOString().substr(0, 10);
        }
        const date = new Date(selectedDate);
        selectedDate = `${date.getFullYear()}-${(date.getMonth() + 1)
          .toString()
          .padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
          console.log('formateDate 2 ',selectedDate);
        return selectedDate;
      }
    },

  },
};
</script>

<style>


.grid-container {
  display: inline-grid;
  grid-template-columns: auto;
  width: 100%;
}
</style>

