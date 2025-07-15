
<template>
    <br/>
    <br/>
    <br/>
    <br/>
    <v-row style="font-size: 22px; color:darkorange">
<br/>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;查詢案件
</v-row>
<p>Table Type: {{ tableType }}</p>
    <p>Record Time: {{ recordTime }}</p>
<br/>
    <div style="margin-left: 15px;">
      <v-btn @click="goBack" >回上頁</v-btn>
      <br/>
    </div>
    
      <div class="container">
        
        <br/>
        <h3>{{ formName }}</h3>
         
    <br/>
    <br/>
  
     
          <v-text-field style="width: 300px;"
            v-model="formDate"
            label="資料日期"
            type="date"
            @input="formatDate"
          ></v-text-field>
          <v-combobox
          v-model="stationName"
          label="站台"
          :items="itemStations"
          item-title="text"
          item-value="value"
          variant="solo"
          class="pa-1"
          style="width: 300px;"
          @update:modelValue="onStationChange"
          return-object
      ></v-combobox>
      <v-combobox
          v-model="maintainLocation"
          label="維護保養地點"
          :items="itemLocations"
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
                  @click="loadFormAndData" 
                  class="load-button"
                  color="#5865f2"
                  size="medium"
                  variant="flat" >載入表格及資料
          </v-btn>
        <br/>
        <br/>
        <form @submit.prevent="submitForm">
          
          <div v-if="formFields && formFields.fields && formFields.fields.length > 0">
            <!-- Render form fields dynamically -->
            <div>
            <div v-for="field in formFields.fields" :key="field.names">
              <template v-if="field.type === 'text'">
                <v-text-field style="width:300px;"
                  v-model="formData[field.names[0]]"
                  hide-details="auto"
                  :label="field.label"
                ></v-text-field>
                <br/>
              </template>
              <template v-else-if="field.type === 'ddlstation'">
                <v-combobox
                  v-model="formData[field.names[0]]"
                  :label="field.label"
                  :items="field.options"
                  style="width: 250px;"
                ></v-combobox>
              </template>
              <template v-else-if="field.type === 'category'">
                <v-combobox
                  v-model="formData[field.names[0]]"
                  :label="field.label"
                  :items="field.options"
                  style="width: 250px;"
                ></v-combobox>
                <v-btn 
                  @click="loadData" 
                  class="load-button"
                  color="#5865f2"
                  size="medium"
                  variant="flat" >載入資料</v-btn>
                
              </template>
              <template v-else-if="field.type === 'checkbox'">
                <v-checkbox :label="field.label" v-model="formData[field.names[0]]" style="width:250px;">
                </v-checkbox>
              </template>
              <template v-else-if="field.type === 'select'" >
                <v-combobox v-model="formData[field.names[0]]"
                  :items="field.options" :label="field.label" style="width:250px;">
                </v-combobox>
              </template>
              <template v-else-if="field.type === 'table'">
              <table>
                  <thead>
                  <tr>
                      <!-- Render table headers -->
                      <th v-for="header in field.headers" :key="header">{{ header }}</th>
                  </tr>
                  </thead>
                  <tbody>
                  <!-- Iterate over table data and render rows -->
                  <tr v-for="(row, index) in field.data" :key="index">
                      <!-- Iterate over row data and render columns -->
                      <td v-for="(cell, cellIndex) in row" :key="cellIndex">{{ cell }}</td>
                  </tr>
                  </tbody>
              </table>
              </template>
              <template v-else-if="field.type === 'multiselect'">
                <v-row>
                  <div class="label-container">
                <label >{{ field.label }}:</label>
                  </div>
                  <div class="field-container">
                  <div v-for="(row, rowIndex) in field.headers" :key="rowIndex" class="text-field-wrapper">
                <div>
                  <v-combobox v-model="formData[field.names[rowIndex]]" hide-details="auto"
                    :label="field.headers[rowIndex]" auto-grow
                    :items="field.options">
                  </v-combobox>
                </div>
              </div>
            </div>
            
            </v-row>
                <!--select :name="field.name" v-model="formData[field.name]">
                  <option v-for="option in field.options" :key="option" :value="option">{{ option }}</option>
                </select-->
                <br/>
            <br/>
              </template>
              <template v-else-if="field.type === 'multitext'">
                <br/>
                  <v-row>
                  <div class="label-container">
                    <label>{{ field.label }}:</label>
  
                  </div>
                  <div class="field-container">
                  <div v-for="(row, rowIndex) in field.headers" :key="rowIndex">
                    <v-text-field class="text-field-wrapper"
                      v-model="formData[field.names[rowIndex]]"
                      hide-details="auto"
                      :label="field.headers[rowIndex]"
                    ></v-text-field>
                  </div>
                </div>
                </v-row>
                <br/>
               </template>
              <template v-else-if="field.type === 'textarea'">
                <br/>
                <v-row>
                  <v-col cols="11" md="8" >
                    <v-textarea 
                      v-model="formData[field.names[0]]"
                      :label="field.label"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <br/>
              </template>
              <template v-else-if="field.type === 'label'">
                <br/>
                  <div>
                    {{ field.label }}
                  </div>
                  <br/>
              </template>
              <template v-else-if="field.type === 'date'">
                <v-text-field style="width: 300px;"
            v-model="formData[field.names[0]]"
            :label="field.label"
            type="date"
            @input="formatDate"
          ></v-text-field>
  </template>
  <template v-else-if="field.type === 'endform'">
    <v-row>
            <v-col v-for="(item, index) in extraItems" :key="index" cols="8">
              <template v-if="item.fieldName !== ''">
                <v-text-field v-model="formData[item.fieldName]" :label="item.itemName" outlined class="narrow-field"></v-text-field>
              </template>
              <template v-else>
                <v-label>{{ item.itemName }}</v-label>
              </template>
              
            </v-col>
            
          </v-row>
          
          <br/>
          <br/>
          <!--input type="file" name="上傳照片" @change="handleFileUpload"-->
          
          <h4 class="font-weight-light pink--text">照片檔上傳</h4>
          <br/>
          
          <v-row>
              <v-file-input accept="image/*" 
                  label="選取圖檔"
                  prepend-icon="mdi-image"
                  multiple chips color="pink"
                  v-model="upFiles"
                  @change="addImages">
                </v-file-input>
          </v-row>
                
                  <br/>
                  <!--v-row>
                    <v-col cols="9">
          <div v-for="(image, key) in images" :key="key">
              <div class="image-holder">
                <img v-bind:ref="'image'" :alt="'ImageOld' + key" :src="image.url" class="img-fluid"/>
                <v-btn icon @click="removeImage3(key)">
                    <v-icon color="red">mdi-close</v-icon>
                  </v-btn>
              </div>
            </div>
            </v-col>
          </v-row-->
          <v-row>  
            <v-col cols="9">
        <v-row v-for="(image, key) in images" :key="key">
          <v-col>
            <div class="image-holder">
              <img v-bind:ref="'image'" alt="" :src="image.url" class="img-fluid"/>
              <v-btn icon @click="removeImage2(key)">
                  <v-icon color="red">mdi-close</v-icon>
              </v-btn>
              
            </div>
          </v-col>
        </v-row>
      </v-col>
      </v-row>
          
              <!--v-row>
                  <v-col sm="4" v-for="(file,f) in files" :key="f">
                      {{file.name}}
                      <v-btn icon @click="removeImage(f)">
              <v-icon color="red">mdi-close</v-icon>
            </v-btn>
                      <img :ref="'file'" :src="file.url" class="img-fluid" :title="'file' + f" />
                  </v-col>
              </v-row-->
          <br/>
          <br/>
          <v-row>
      <v-col cols="12" md="4">
        <v-combobox label="主管" :items="itemsManager" v-model="manager" item-title="name"
          item-value="id"></v-combobox>
      </v-col>
      <v-col cols="12" md="4">
        <v-combobox label="維護人員" :items="itemsMaintainer" v-model="maintainer" chips item-title="name"
          item-value="id"
          multiple></v-combobox>
      </v-col>
    </v-row>
          <br/>
          <br/>
          <v-row>
            <input type="hidden" v-model="guid" />
          <button type="submit" class="custom-button">儲存資料</button>
          </v-row>
          <br/>
          <br/>
          <br/>
  </template>
  
              <!--template v-else-if="field.type === 'uploadfile'">
                  <input type="file" :name="field.names[0]" @change="handleFileUpload">
              </template-->
  
              
  
              <!--template v-else-if="field.type === 'submit'">
            <button type="submit" class="custom-button">{{ field.label }}</button>
          </template-->
          
              <template>
                
  </template>
  
              <!-- Add more cases for other field types (radio, button, etc.) -->
            </div>
          </div>
          </div>
          <div v-else>
            <!--x
            <p>無表格輸入格式資料</p>
            -->
          </div>
          <br/>
          <br/>
          
        </form>
      
      </div>
  
    </template>

  <style>
  
  .label-container {
    display: inline-block;
    margin-left: 15px;
    vertical-align: middle;
    min-height: 50px;
    margin-top: 0px;
    width: 300px;
  }
  
  .field-container {
    display: inline-flex;
    flex-wrap: wrap;
    margin-left: 10px;
  }
  
  .text-field-wrapper {
    flex: 1;
    min-width: 180px;
    margin-right: 10px;
    min-height: 100px;
  }
  
  @media (max-width: 600px) {
    .label-container {
    display: inline-block;
    margin-left: 15px;
    vertical-align: middle;
    height: 60px;
    margin-top: 30px;
    width: 500px;
  }
    .text-field-wrapper {
      flex: 0 1 30%; 
      margin-bottom: 20px;
      vertical-align: middle;
      height: 60px;
    }
  }
  
  
  
  .combobox1 {
    flex: 1; 
  }
  
  .container {
    /*display: inline-block;*/
    /*position: absolute;*/
    margin-left:15px;
    z-index: 100;
    width: 100%;
    height: 100vh;
  }
  
  .custom-select {
    width: 180px; 
    height: 80px;
    justify-content: flex-end;
  }
  
  .custom-button.v-btn  {
    background-color: #4CAF50; 
    border: none;
    color: white; 
    padding: 15px 32px; 
    text-align: center; 
    text-decoration: none; 
    display: inline-block;
    font-size: 16px;
    margin: 4px 2px;
    cursor: pointer;
    min-width:300px;
    max-height:50px;
  }
  
  .custom-button  {
    background-color: #4269e7;
    border: none;
    color: white; 
    padding: 15px 32px; 
    text-align: center; 
    text-decoration: none; 
    display: inline-block;
    font-size: 16px;
    margin: 4px 2px;
    cursor: pointer;
    min-width:300px;
    max-height:50px;
  }
  
  .load-button {
    background-color: #4CFFFF;
    border: none;
    color: white; 
    padding: 15px 32px; 
    text-align: center; 
    text-decoration: none; 
    display: inline-block;
    font-size: 16px;
    margin: 4px 2px;
    cursor: pointer;
    min-width:300px;
    max-height:60px;
  }
  
  
   
  .narrow-field {
    max-width: 500px; 
  }
   
  .img-fluid {
      width: 50%;
  }
  
  
  .image-holder {
    float: left;
  }
  
  </style>
  
  
    <script>
  
    import axios from 'axios';
    import {reactive, toRefs,onBeforeMount,onMounted,ref,watch} from 'vue'
    import vuetify from '@/plugins/vuetify';
    //import { defineConfig, loadEnv } from 'vite';
    //const API_ENDPOINT = process.env.VITE_APP_API_ENDPOINT;
    const value = ref('foo');
    const env = import.meta.env;
    const apiUrl = window.config.API_URL; //(env.VITE_APP_API_ENDPOINT);
    let imageId=100;
    const selectedMaintain = ref([]);
    async function uploadFiles(files,formId,stationId,date,maintainLocation) {
      console.log('formId',formId);
      console.log('stationId',stationId);
      console.log('date',date);
          try {
            let formData2 = new FormData();
  
            console.log('files',files.length);
            //for (let i = 0; i < files.length; i++) {
            //  formData2.append('files', files[i]);
            //}
            
            for (let i = 0; i < files.length; i++) {
              //if (files[i]==null)
              //   continue;
              if (files[i] instanceof File)
              {
                try
                {
                  console.log('image',files[i])
                  formData2.append('files', files[i]);
                }
                catch(e)
                {
                  console.log('add file:'+e);
                }
              }
              else
              {
                console.log('not file',files[i]);
                formData2.append('files', files[i]);
              }
            }
            if (files.length > 0)
            {
              formData2.append('formId',formId);
              formData2.append('stationId',stationId);
              formData2.append('date',date);
              formData2.append('maintainLocation',maintainLocation);
              // Send the FormData object to the server
              //formData2.append(this.formData);
              const response = await axios.post(`${apiUrl}/api/forms/files`, formData2);
    
              // Handle the response
              console.log('Files uploaded successfully:', response.data);
              //this.files=[];
              
              console.log('files[2]',files.length);
              var returnData=response.data.split(':');
              if (returnData.length >=2)
              {
                console.log('return',returnData[1]);
                //for (let i = 0; i < files.length; i++)
                  //files[i]=null;
                
                return returnData[1];
              }
              else
              {
                return '';
              }
            }
            else
            {
              return '';
            }
          } catch (error) {
            console.error('Error uploading files:', error);
            throw error; // Rethrow the error for handling in the calling code
          }
          finally
          {
            //console.log('upFiles',this.upFiles.length);
            //  this.upFiles=[];
          }
    }
  
    
  
  async function asyncUpload(files,formId,stationId,date) {
    console.log('upload files:',files);
    const result = await uploadFiles(files,formId,stationId,date);
    console.log('asyncUpload',result); // will print "Hello world!"
    guid=result.data;
  }
  
  
   
  
    export default {
      inheritAttrs:false,
      setup() {
        const route = useRoute();
        const tableType = route.params.tableType;
        const recordTime = route.params.recordTime;

        return {
          tableType,
          recordTime,
        };
      },
      /*
      props: {
        tableType: {
          type: String,
          required: true,
        },
        recordTime: {
          type: String,
          required: true,
        },
      },
      */
      /*
      seetup() {
        const route = useRoute()
        tableType = route.params.tableType;
        console.log('tableType',route.params.tableType);
        recordTime = route.params.recordTime;
        console.log('rt',recordTime);
      },
      */
      watch: {
        
      },
      data() {
        //console.log('tabletype',tableType);
        //console.log('rt',recordTime);
        let d=new Date();
        d.setHours(d.getHours() + 8);
              
        return {
          formName: '',    
          formFields: null, 
          formData: {},
          items: [], 
          itemStations:[],
          itemLocations:[],
          itemCategories:[],
          selectedOptions: [] ,
          formDate: d.toISOString().substr(0, 10),
          selectedDate:d.toISOString().substr(0, 10),
          dataLoaded: false,
          loadedData: null,
          stationName:null,
          maintainLocation:null,
          category:null,
          extraItems:null,
          files: [],
          readers: [],
          productImages: [],
          guid:'',
          images: [],
          upFiles:[],
          itemsManager:[],
          itemsMaintainer:ref([]),
          manager:[],
          maintainer:[],
          canvasWidth: 1280, 
          canvasHeight: 720,
          //resizedImage: null,
          image: null,
          imageUrl: ''
        };
      },
      components: {
      
      // other components
    } ,
      async created() {
      //const data = null;
      //this.items = data.items;
      //console.log(data.items);
      //this.selectedOptions = new Array(data.items.length).fill('');
      //console.log( this.selectedOptions);
     },
      mounted() {
        //this.fetchFormFields(); 
        //this.loadPreData();
        this.formDate=this.recordTime;
        this.loadPreData();
      },
      methods: {
        resizeAndAddText(image) {
          const canvas = this.$refs.canvas;
          const ctx = canvas.getContext('2d');
          
          // Resize the image to fit the canvas
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          ctx.drawImage(image, 0, 0, this.canvasWidth, this.canvasHeight);
          
          // Add the current date
          const today = new Date();
          const dateString = today.toISOString().split('T')[0]; // YYYY-MM-DD format
          ctx.font = '20px Arial';
          ctx.fillStyle = 'white';
          ctx.textAlign = 'right';
          ctx.fillText(dateString, this.canvasWidth - 10, this.canvasHeight - 10);
          
          // Convert canvas to image URL
          this.resizedImage = canvas.toDataURL('image/jpeg');
        },
        findUploadFileIndex(imageIndex) {
          const image = this.images[imageIndex];
          const uploadedFileIndex = this.files.findIndex(file => {
            return file.name + file.size === image.id; // Match the unique identifier
          });
          return uploadedFileIndex;
        },
        handleFileUpload() {
        const files = this.files;
        for (let file of files) {
          if (file instanceof File) {
            const reader = new FileReader();
            reader.onload = (e) => {
              
              this.images.push({
                url: e.target.result,
                id: null // You can add other metadata if needed
              });
            };
            reader.readAsDataURL(file);
          } else {
            console.error('The uploaded file is not of type File:', file);
          }
        }
      },
        addImages2(){
              console.log('files', this.files)
              this.files.forEach((file, f) => {
                  this.readers[f] = new FileReader();
                  this.readers[f].onloadend = (e) => {
                      let fileData = this.readers[f].result
                      let imgRef = this.$refs.file[f]
                      imgRef.src = fileData
                      console.log(fileData)
                      // send to server here...
                  }
  
                  this.readers[f].readAsDataURL(this.files[f]);
              });
              console.log('images.length',this.images.length);
        },
        
        addImages(e) {
          this.files=this.upFiles;
          console.log('call addImages');
          console.log('files count:',this.files.length);
          console.log('files',this.files);
          let selectedFiles = e.target.files;
          this.productImages=[];
          for (let i = 0; i < selectedFiles.length; i++) {
            this.productImages.push(selectedFiles[i]);
            //this.images.push(selectedFiles[i]);
          }
          this.applyImage4();
          //this.upFiles=null;
        },
        applyImage4(){
          console.log('call applyImages4');
          console.log('productImages',this.images.length);
          console.log(this.images);
          //this.images=[];
          for (let i = 0; i < this.productImages.length; i++) {
            if (this.productImages[i] instanceof File) {
              let reader = new FileReader();
              console.log(this.productImages[i].name,this.productImages[i].size);
              reader.onload = (e) => {
                const imageData = e.target.result;
                const image = new Image();
                //this.$refs.image[i].src = reader.result;
                image.onload=()=>{
                  const canvas = document.createElement('canvas');
                  canvas.width = this.canvasWidth;
                  canvas.height = this.canvasHeight;
                  const ctx = canvas.getContext('2d');
                  ctx.drawImage(image, 0, 0, canvas.width, canvas.height);
                  //const base64Image = canvas.toDataURL();
                  //console.log('base64',base64Image);
                  //this.imageUrl = base64Image.replace(/[^,]+/g, '');
                  //console.log('imageUrl',this.imageUrl);

                  const today = new Date();
                  const dateString = today.toISOString().split('T')[0]; // YYYY-MM-DD format
                  ctx.font = '24px Arial';
                  ctx.fillStyle = 'red';
                  ctx.textAlign = 'right';
                  ctx.fillText(dateString, this.canvasWidth - 20, this.canvasHeight - 20);
                  //this.addTimestamp(base64Image);
                  this.imageUrl = canvas.toDataURL();
                  this.images.push({
                    url: this.imageUrl,//e.target.result,
                    id: this.productImages[i].name + this.productImages[i].size, 
                    tag:'new'
                  });
                  imageId++;
                };
                image.src = imageData;

                
              };
              
              //console.log(i+':'+this.productImages[i].name);
              reader.readAsDataURL(this.productImages[i]);
            }
          }
        },
        addTimestamp(imageUrl) {
          const timestamp = new Date().toISOString().slice(0, 10).replace(/-/g, '');
          const splitImageUrl = imageUrl.split(',');
          const base64Image = `${splitImageUrl[0]}, ${timestamp} ${splitImageUrl[1]}`;
          this.imageUrl = base64Image;
        },
        applyImage() {
          console.log('productImages',this.productImages.length);
          console.log(this.productImages);
          for (let i = 0; i < this.productImages.length; i++) {
            let reader = new FileReader();
            
            reader.onloadend = (e) => {
              //this.$refs.image[i].src = reader.result;
              let fileData = reader.result;
                      let imgRef = this.$refs.image[i];
                      imgRef.src = fileData;//+'?t='+new Date().toISOString();
              //console.log('imgRef.src',imgRef.src);
              imgRef.alt='imgNew'+imageId.toString();
              imageId++;
            };
            
            console.log(i+':'+this.productImages[i].name);
            reader.readAsDataURL(this.productImages[i]);
          }
          
        },
        applyImage3() {
          console.log('images',this.images.length);
          console.log(this.images);
          for (let i = 0; i < this.images.length; i++) {
            
          }
        },
        
        /*
        addImages(event) {
          for (let i = 0; i < event.target.files.length; i++) {
            const file = event.target.files[i];
            this.files.push({ name: file.name, url: URL.createObjectURL(file) });
          }
        },
        */
        removeImage(image, index) {
        console.log(this.productImages);
        this.productImages.splice(index, 1);
        this.applyImage();
        //this.$refs.image[index].src = "" // You are hidding the 3rd one that is now in index 1.
        },
        removeImage2(index) {
          console.log('index:'+index);
          //this.files.splice(index, 1);
          //this.productImages.splice(index, 1);
          console.log('index',index);
          console.log('image id',this.images[index].id);
          if (this.images[index].tag==='old')
          {
            axios.post(`${apiUrl}/api/forms/removeImage?imageId=${this.images[index].id.toString()}`)
            .then(response => {
              // Handle success response
              console.log('Response:', response.data);
              this.images.splice(index, 1);
              console.log('image length:'+this.images.length);
              // Reset form data if needed
              //this.formData = {};
              console.log('刪除完成');
              alert('刪除完成');
            })
            .catch(error => {
              // Handle error
              console.error('刪除錯誤:', error);
              alert('刪除錯誤:'+error);
            });
          }
          else
          {
            const uploadedFileIndex = this.findUploadFileIndex(index);
            if (uploadedFileIndex !== -1) {
              this.files.splice(uploadedFileIndex, 1);
            }
            this.images.splice(index, 1);
          }
          //this.applyImage4();
          //this.$refs.image[index].src = ""
        },
        removeImage3(index) {
          console.log('index:'+index);
          //this.files.splice(index, 1);
          this.images.splice(index, 1);
          console.log('image length:'+this.images.length);
          this.applyImage3();
          
          //this.$refs.image[index].src = ""
        },
        goBack() {
          location.reload();
        },
        async loadPreData()
        {
              
                //console.log('api:'+ `${apiUrl}/api/forms/getstationcategory`);
                //axios.get(`${apiUrl}/api/forms/getstationcategory`)
                //console.log('api',`${apiUrl}/api/forms/getstation`);
                axios.get(`${apiUrl}/api/forms/getstation`)
                .then(response => {
                  //console.log('stations',response.data.lstStations);
                    if (response.data && response.data.lstStations) {
                      //this.itemCategories = response.data.lstCategories;
                      this.itemStations = response.data.lstStations;
                      
                      //console.log('stations', response.data);
                      //console.log(this.itemStations);
                      //console.log(this.itemCategories);
                      //console.log(this.itemCategories);
                      let d=new Date();
                      d.setHours(d.getHours() + 8);
                      this.formData['formDate']=d.toISOString().substr(0, 10);
                    } else {
                      console.error('Invalid response format', response);
                    }
                })
                .catch(error => {
                  console.error('載入站台及類型錯誤:', error);
                  alert('載入站台及類型錯誤:'+ error);
                });
                
            
        },
        async fetchFormFields() {
          try {
            let d=new Date();
              d.setHours(d.getHours() + 8);
            const response = await axios.get(`${apiUrl}/api/forms/${this.formId}`);
            this.formName = response.data.name;   
            this.formFields = JSON.parse(response.data.definition); 
            console.log('Form data:', this.formData);
            console.log(this.formFields);
            console.log(this.formFields.fields);
            this.formData['formDate']=d.toISOString().substr(0, 10);
          } catch (error) {
            console.error('Error fetching form fields:', error);
          }
        },
        arrayBufferToBase64(buffer) {
          let binary = '';
          console.log('buffer len',buffer.length);
          console.log('buffer',buffer);
          const bytes = new Uint8Array(buffer);
          console.log('Bytes:', bytes);
          console.log('bytes len',bytes.byteLength);
          for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
          }
          
          return window.btoa(binary);
        },
        stringToUint8Array(base64String) {
        const binaryString = window.atob(base64String); // Decode base64 string
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
          bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes.buffer;
      },
        async loadFormAndData() {
          try {
            console.log('loadFormAndData');
            this.formData={};
            this.productImages=[];
            this.images=[];
            //console.log('formanddata',`${apiUrl}/api/forms/getForm?tabletype=${this.category.value}&stationid=${this.stationName.value}&location=${this.maintainLocation.value}&date=${this.formDate}`);
            if (this.category.value===undefined || this.maintainLocation.value===undefined)
              return;
            const response = await axios.get(`${apiUrl}/api/forms/getForm?tabletype=${this.category.value}&stationid=${this.stationName.value}&location=${this.maintainLocation.value}&date=${this.formDate}`); 
            this.formName = response.data.name;   
            this.formFields = JSON.parse(response.data.definition);
            console.log('formfields',this.formFields);
            console.log('formfiedsdata',this.formFields.fields);
            
            
            console.log('form date',response.data.date);
            this.formDate=response.data.date.substr(0, 10); 
            //console.log(response.data);
            this.extraItems =  response.data.lstExtraItem;
            this.itemsMaintainer = response.data.lstMaintainer;
            this.itemsManager = response.data.lstManager;
            console.log('LastDate',response.data.lastDate);
            
            
            const result0=this.formFields.fields.find(item => item.names.includes('LastDate'));
            console.log('result',result0);
            console.log('result',result0.names[0]);
            if (result0!=null)
            {
               //const indices = this.formFields.fields.map((item, index) => index).filter(i => this.formFields.fields[i] === result0);
               //console.log('indices',indices);
               //console.log('lastdate',this.formFields.fields[indices].names[0]);
                                 //console.log('lastdata length', response.data.lastDate.length);
               if (response.data.lastDate.length >= 10)
                   this.formData[result0.names[0]]=response.data.lastDate.substr(0,10);
            
            }
            
            const result1=this.formFields.fields.find(item => item.names.includes('CurrDate'));
            if (result1!=null)
            {
               //const indices = this.formFields.fields.map((item, index) => index).filter(i => this.formFields.fields[i] === result1);
               //console.log('indices',indices);
               //console.log('lastdate',this.formFields.fields[indices].names[0]);
               this.formData[result1.names[0]]=new Date().toISOString().substr(0, 10);
            }
            //console.log(response.data.lstImageData);
            //this.images=response.data.lstImageData.map(image=>({
            //  url: `data:image/jpeg;base64,${this.arrayBufferToBase64(image.image)}`,
            //  id: image.id // Assume your API provides an ID for each image
            //}));
            const dbImages=response.data.lstImageData.map(image=>({
              url: `data:image/jpeg;base64,${image.image}`,
              //url: `data:image/jpeg;base64,${image.image}`,
              id: image.id, // Assume your API provides an ID for each image
              tag:'old',
            }));
  
            this.images = [...this.images,...dbImages];
            //console.log('image',this.images);
            //console.log('length:'+response.data.lstFormData.length);
              console.log(response.data.lstFormData);
            for(var i=0; i < response.data.lstFormData.length;i++)
            {
              console.log(i+','+response.data.lstFormData[i].fieldName+','+response.data.lstFormData[i].fieldValue);
              if (response.data.lstFormData[i].fieldName==='manager')
              {
                this.manager=[];
                for(var j=0;j<this.itemsManager.length;j++)
                {
                  console.log('db fieldvalue',response.data.lstFormData[i].fieldValue);
                  console.log('itemManager',this.itemsManager[j]);
                  if (this.itemsManager[j].id===response.data.lstFormData[i].fieldValue)
                  {
                    console.log('manager name',this.itemsManager[j].name);
                    this.manager.push(this.itemsManager[j]);
                    break;
                  }
                }
              }
              else if (response.data.lstFormData[i].fieldName==='maintainer')
              {
                //this.maintainer=response.data.lstFormData[i].fieldValue;
                var selected=response.data.lstFormData[i].fieldValue.split(',');
                this.maintainer=[];
                for(var j=0;j<this.itemsMaintainer.length;j++)
                {
                  for(var k=0;k<selected.length;k++)
                  {
                    if (this.itemsMaintainer[j].id===selected[k])
                    {
                      console.log(this.itemsMaintainer[j].id, selected[k]);
                      this.maintainer.push(this.itemsMaintainer[j]);
                      break;
                    }
                  }
                }
                console.log('finished');
                //selectedMaintain.value=response.data.lstFormData[i].fieldValue;
              }
              else
              {
                this.formData[response.data.lstFormData[i].fieldName]=response.data.lstFormData[i].fieldValue;
              }
              //console.log('formfield:',this.formData[response.data.lstFormData[i].fieldName]);    
              
            }
            //this.maintainer=selectedMaintain;
            this.dataLoaded = true;
            
          } catch (error) {
            console.error('載入資料發生錯誤:', error);
          }
        },
        
        uploadImage() {
        if (this.resizedImage) {
            const byteString = atob(this.resizedImage.split(',')[1]);
            const mimeString = this.resizedImage.split(',')[0].split(':')[1].split(';')[0];
            const ab = new ArrayBuffer(byteString.length);
            const ia = new Uint8Array(ab);
            for (let i = 0; i < byteString.length; i++) {
              ia[i] = byteString.charCodeAt(i);
            }
            const blob = new Blob([ab], { type: mimeString });

            // Create FormData object
            const formData = new FormData();
            formData.append('file', blob, 'resized_image.jpg');

            // Send the image to the server
            axios.post('YOUR_UPLOAD_URL', formData, {
              headers: {
                'Content-Type': 'multipart/form-data',
              },
            })
            .then(response => {
              console.log('Image uploaded successfully:', response.data);
            })
            .catch(error => {
              console.error('Error uploading image:', error);
            });
          }
        },
        
        async submitForm() {
          console.log(this.formFields.fields);
          //console.log(this.formFields.fields[0]);
          //console.log(this.formFields.fields[0].names[0]);
          console.log(this.formData[this.formFields.fields[0].names[0]]);
          console.log('Form data:', this.formData);
          console.log(JSON.stringify(this.formData));
          
  
          //const data = await asyncUpload(this.productImages,this.category.value,this.stationName.value,this.formDate);
          //this.formData[guid]=guid;
          console.log('files:',this.files);
          var response = await uploadFiles(this.files,this.category.value,this.stationName.value,this.formDate,this.maintainLocation)
          /*
            .then(response => {
              console.log('guid 0:', response.data);
              this.guid=response.data;
              console.log('guid 1',this.guid);
              this.formData[guid].this.guid;
              console.log('formData[guid]:',  this.formData[guid]);
            // Handle successful response
            }) 
            .catch(error => {
              // Error uploading files
              console.log("upload error:"+error);
            });
          */
          console.log('guid 1',response);
          if (response!=='')
          {
            this.guid=response;
          }
          console.log('guid 2',this.guid);
          const postData = {
          formData: JSON.stringify(this.formData),//this.formData,
          category: this.category.value,
          date:this.formDate,
          station:this.stationName.value,
          guid:this.guid,
          manager:this.manager,
          maintainer:this.maintainer,
          maintainLocation:this.maintainLocation.value,
          //files:this.productImages,
          };

          console.log('postData',postData);
          
          /*
          for (let i = 0; i < this.productImages.length; i++) {
            try
            {
              console.log('image',this.productImages[i])
              postData.files.push('files', this.productImages[i]);
            }
            catch(e)
            {
              console.log('add file:'+e);
            }
          }
          */
          //console.log('filesLength',postData.files.length);
           axios.post(`${apiUrl}/api/forms/savedata`, postData)
          .then(response => {
            // Handle success response
            console.log('Response:', response.data);
            // Reset form data if needed
            //this.formData = {};
            console.log('存檔完成');
            alert('存檔完成');
            console.log('upFiles',this.upFiles.length);
            this.upFiles=[];
            this.files=[];
            console.log('upFiles',this.upFiles.length);
          })
          .catch(error => {
            // Handle error
            console.error('存檔錯誤:', error);
            alert('存檔錯誤:'+error);
          });
        },
        formatDate() {
          console.log('formateDate');
          if (this.selectedDate) {
            if (this.selectedDate === "NaN-NaN-NaN")
            {
              let d=new Date();
              d.setHours(d.getHours() + 8);
              return d.toISOString().substr(0, 10);
            }
            const date = new Date(this.selectedDate);
            this.selectedDate = `${date.getFullYear()}-${(date.getMonth() + 1)
              .toString()
              .padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
          }
        },
        onStationChange(selectedStation) {
          console.log('onchange',selectedStation.value);
          console.log('stationChange',`${selectedStation.value}`);
          this.itemLocations=[];
          this.itemCategories=[];
          //this.formData=null;
          this.maintainLocation=null;
          this.category=null;
          if (selectedStation.value===undefined)
            return;

          axios.post(`${apiUrl}/api/forms/getLocationAndType?stationId=${selectedStation.value}`)
            .then(response => {
              // Handle success response
              console.log('Response:', response.data);
              
              this.itemLocations=response.data.lstLocations;
              this.itemCategories=response.data.lstTypes;
            })
            .catch(error => {
              // Handle error
              //console.error('刪除錯誤:', error);
              alert('讀取維護地點及表格種類錯誤:'+error);
            });
          //const station = this.stations.find(st => st.ST === selectedStation.ST);
          //this.locationOptions = station.Locations.map(location => location.Location);
        }
      },
      computed: {
      },
      
    };
  
    
    </script>
    