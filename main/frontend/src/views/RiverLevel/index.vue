<template>
  <div class="search">
    <!-- 區域選擇器 -->
    <div class="control-panel">
      <el-select v-model="selectedArea" placeholder="請選擇區域" @change="handleAreaChange">
        <el-option
          v-for="area in areaOptions"
          :key="area.value"
          :label="area.label"
          :value="area.value"
        />
      </el-select>
    </div>

    <!-- 地圖容器 -->
    <div ref="mapContainer" class="map-container" style="height: 200px; width: 100%; position: relative;">
      <!-- 信息窗口 -->
      <div v-if="infoWindow.visible" class="info-window">
        <div class="info-window-header">
          <span class="info-window-title">{{ infoWindow.title }}</span>
          <span class="info-window-close" @click="closeInfoWindow">×</span>
        </div>
        <div class="info-window-content">
          <div v-if="infoWindow.sensor">
            <div class="info-item marker-highlight"><span>目前查看:</span> {{ infoWindow.sensor.SensorNameA || `感測器 ${infoWindow.sensor.SensorID}` }}</div>
            <div class="info-item"><span>感測器ID:</span> {{ infoWindow.sensor.SensorID }}</div>
            <div class="info-item"><span>最新水位:</span> {{ infoWindow.sensor.LastValue1 }} {{ infoWindow.sensor.DataUnit }}</div>
            <div class="info-item"><span>更新時間:</span> {{ infoWindow.sensor.LastDataTime }}</div>
            <div class="info-item"><span>狀態:</span> {{ infoWindow.sensor.Status || '正常' }}</div>
          </div>
        </div>
        <!-- 連接線指示當前選中的標記 -->
        <div class="info-window-pointer"></div>
      </div>
    </div>
    
    <!-- 水位折線圖 -->
    <div v-if="chartReady" class="chart-container">
      <h3>水位折線圖</h3>
      <div class="chart-wrapper">
        <line-chart :data="chartData" :options="chartOptions" :key="chartUpdateKey" />
      </div>
    </div>

    <!-- 感測器列表 (已註釋) -->
    <!-- 
    <div v-if="sensors.length > 0" class="sensor-list">
      <h3>感測器列表</h3>
      <el-table :data="sortedSensors" stripe style="width: 100%" class="custom-table">
        <el-table-column prop="SensorID" label="感測器ID" sortable width="100" />
        <el-table-column prop="SensorNameA" label="感測器名稱" width="180" />
        <el-table-column prop="LastDataTime" label="最後資料時間" width="180" />
        <el-table-column label="最新水位" width="120">
          <template #default="scope">
            {{ scope.row.LastValue1 }} {{ scope.row.DataUnit }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="scope">
            <el-button 
              type="primary" 
              size="small" 
              @click="locateSensor(scope.row)"
              :disabled="!scope.row.X || !scope.row.Y">
              定位
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>
    -->
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, defineComponent } from 'vue'
import { ElSelect, ElOption, ElTable, ElTableColumn, ElButton } from 'element-plus'
import RiverLevelAPI from '@/resource/riverlevel'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement, Filler } from 'chart.js'
import { Line } from 'vue-chartjs'

// 宣告 TGOS 全域變數
declare global {
  const TGOS: any
}

// 註冊 ChartJS 組件，包括 Filler 插件用於區域填充
ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement, Filler)

// 自定義插件：確保填充到 Y 軸的最小值
const fillToMinPlugin = {
  id: 'fillToMin',
  beforeDraw: (chart: any) => {
    const { ctx, chartArea, scales } = chart
    if (!chartArea || !scales.y) return
    
    // 獲取 Y 軸的最小值在畫布上的位置
    const minY = scales.y.getPixelForValue(scales.y.min)
    
    // 對每個數據集應用填充
    chart.data.datasets.forEach((dataset: any, i: number) => {
      if (!dataset._meta) return
      
      const meta = Object.values(chart.getDatasetMeta(i))[0] as any
      if (!meta || !meta.data || !meta.data.length) return
      
      // 繪製填充區域
      ctx.save()
      ctx.fillStyle = dataset.backgroundColor || 'rgba(54, 162, 235, 0.2)'
      ctx.beginPath()
      
      // 從第一個點開始
      const firstPoint = meta.data[0]
      ctx.moveTo(firstPoint.x, firstPoint.y)
      
      // 繪製折線
      meta.data.forEach((point: any) => {
        ctx.lineTo(point.x, point.y)
      })
      
      // 繪製到底部的邊界
      const lastPoint = meta.data[meta.data.length - 1]
      ctx.lineTo(lastPoint.x, minY)
      ctx.lineTo(firstPoint.x, minY)
      ctx.closePath()
      ctx.fill()
      ctx.restore()
    })
  }
}

// 註冊自定義插件
ChartJS.register(fillToMinPlugin)

// 引入 LineChart 並重命名為 LineChartComponent
const LineChart = Line

const mapContainer = ref<HTMLElement | null>(null)
const selectedArea = ref('')
const areaOptions = ref<{ value: string; label: string }[]>([])
const sensors = ref<any[]>([])
let map: any = null
let markers: any[] = []
let activeMarker: any = null // 添加用於保存當前活動標記的變數
const chartReady = ref(false)
const chartUpdateKey = ref(0) // 添加用於強制圖表更新的 key
const activeMarkerId = ref(null) // 添加活動標記的ID

// 信息窗口狀態
const infoWindow = ref({
  visible: false,
  title: '',
  content: '',
  sensor: null
})

// 關閉信息窗口
const closeInfoWindow = () => {
  infoWindow.value.visible = false;
  activeMarkerId.value = null;
  
  // 清除活動標記引用
  activeMarker = null;
}

// 按照SensorID排序的感測器列表
const sortedSensors = computed(() => {
  return [...sensors.value].sort((a, b) => {
    // 將SensorID轉換為字串後進行比較
    const idA = String(a.SensorID)
    const idB = String(b.SensorID)
    
    // 如果SensorID是數字形式的字串，嘗試數字排序
    const numA = parseInt(idA)
    const numB = parseInt(idB)
    
    if (!isNaN(numA) && !isNaN(numB)) {
      return numA - numB
    }
    
    // 如果不是純數字或轉換失敗，則按字串排序
    return idA.localeCompare(idB)
  })
})

// 圖表數據和配置
const chartData = computed(() => {
  // 確保返回一個完整的圖表數據結構，無論什麼情況
  const defaultChartData = {
    labels: [],
    datasets: [{
      label: '水位',
      backgroundColor: 'rgba(54, 162, 235, 0.2)', // 填充顏色，將被自定義插件使用
      borderColor: 'rgba(54, 162, 235, 1)',
      borderWidth: 2,
      pointBackgroundColor: 'rgba(54, 162, 235, 1)',
      pointBorderColor: '#fff',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(54, 162, 235, 1)',
      data: [],
      fill: false // 禁用默認填充，使用我們的自定義插件
    }]
  }
  
  try {
    // 如果沒有有效的感測器數據，返回默認結構
    if (!sortedSensors.value || !Array.isArray(sortedSensors.value) || sortedSensors.value.length === 0) {
      console.log('沒有有效的感測器數據，返回默認圖表結構')
      return defaultChartData
    }
    
    // 從排序後的感測器中獲取數據
    const labels = sortedSensors.value.map(sensor => sensor.SensorNameA || sensor.SensorID || '未命名')
    const waterLevels = sortedSensors.value.map(sensor => {
      const value = parseFloat(sensor.LastValue1)
      return isNaN(value) ? 0 : value
    })
    const dataUnit = sortedSensors.value[0]?.DataUnit || ''
    
    // 計算數據中的最小值
    let minDataValue = Math.min(...waterLevels.filter(val => !isNaN(val)))
    minDataValue = Math.floor(minDataValue * 10) / 10 - 0.1 // 向下取整到小數點後一位，再減0.1
    
    return {
      labels,
      datasets: [
        {
          label: `水位 ${dataUnit ? `(${dataUnit})` : ''}`,
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          borderColor: 'rgba(54, 162, 235, 1)',
          borderWidth: 2,
          pointBackgroundColor: 'rgba(54, 162, 235, 1)',
          pointBorderColor: '#fff',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgba(54, 162, 235, 1)',
          data: waterLevels,
          fill: {
            target: 'start',  // 使用 'start' 而不是 'origin'，填充到數據集的底部
            above: 'rgba(54, 162, 235, 0.2)'  // 填充顏色
          }
        }
      ]
    }
  } catch (error) {
    console.error('生成圖表數據時發生錯誤:', error)
    return defaultChartData
  }
})

// 圖表配置選項
const chartOptions = computed(() => {
  // 動態計算數據中的最小值
  let minValue = -0.2 // 默認值，如果無法計算則使用
  
  // 如果有有效的圖表數據，則計算實際的最小值
  if (chartData.value && 
      chartData.value.datasets && 
      chartData.value.datasets.length > 0 &&
      chartData.value.datasets[0].data && 
      chartData.value.datasets[0].data.length > 0) {
    
    // 過濾出有效的數值（非 NaN、非 undefined、非 null）
    const validData = chartData.value.datasets[0].data.filter(val => 
      val !== undefined && val !== null && !isNaN(Number(val))
    ).map(val => Number(val))
    
    if (validData.length > 0) {
      // 找出數據中的最小值
      const dataMin = Math.min(...validData)
      
      // 為了視覺效果，將最小值稍微調低（向下取整到小數點後一位，再減去0.1）
      minValue = Math.floor(dataMin * 10) / 10 - 0.1
      
      console.log('數據最小值:', dataMin, '圖表 Y 軸最小值設置為:', minValue)
    }
  }
  
  return {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        min: minValue, // 使用動態計算的最小值
        title: {
          display: true,
          text: '水位'
        },
        beginAtZero: false, // 確保 Y 軸不會強制從 0 開始
        grace: '5%' // 在軸的底部添加額外空間
      },
      x: {
        title: {
          display: true,
          text: '感測器'
        }
      }
    },
    plugins: {
      tooltip: {
        callbacks: {
          label: function(context: any) {
            return `水位: ${context.parsed.y}`
          }
        }
      },
      legend: {
        display: true,
        position: 'top' as const
      },
      filler: {
        propagate: false // 禁用默認填充，改用我們的自定義插件
      },
      fillToMin: {
        enabled: true // 啟用我們的自定義填充插件
      }
    },
    elements: {
      line: {
        tension: 0.1 // 設為0，使用直線而非圓滑曲線
      }
    }
  }
})

  // 從API獲取區域列表
const fetchAreaList = async () => {
  try {
    const response = await RiverLevelAPI.getAreaList()
    if (response.success && response.data) {
      areaOptions.value = response.data.map(area => ({
        value: area.AreaID,
        label: area.AreaName
      }))
    }
  } catch (error) {
    // 錯誤處理
  }
}

// 處理區域變更
const handleAreaChange = async (value: string) => {
  try {
    // 先將圖表設為未準備好
    chartReady.value = false
    
    // 清除上一次的標記
    clearMarkers()
    
    // 關閉信息窗口
    closeInfoWindow()
    
    // 重置感測器列表
    sensors.value = []
    
    // 如果沒有選擇區域，則返回
    if (!value) {
      return
    }
    
    // 獲取感測器資料
    const response = await RiverLevelAPI.getSensorsByArea(value)
    if (response.success && response.data) {
      sensors.value = response.data
      
      console.log('獲取到的感測器數量:', sensors.value.length)
      
      // 在地圖上標記感測器位置
      markSensorsOnMap(sensors.value)
      
      // 如果有感測器數據，設置圖表為準備好
      if (sensors.value.length > 0) {
        // 完全重置圖表，強制重新渲染
        chartReady.value = false
        
        // 延遲設置以確保 DOM 已更新且 chartData 已計算
        setTimeout(() => {
          // 確保在設置 chartReady 前，chartData 已經準備好並且有有效數據
          const currentChartData = chartData.value
          if (currentChartData && 
              currentChartData.labels && 
              currentChartData.datasets && 
              currentChartData.datasets.length > 0 &&
              currentChartData.datasets[0].data &&
              currentChartData.datasets[0].data.length > 0) {
            console.log('圖表數據已準備好:', currentChartData)
            // 增加 chartUpdateKey 的值，強制圖表組件重新渲染
            chartUpdateKey.value++
            chartReady.value = true
          } else {
            console.warn('圖表數據不完整，不顯示圖表')
          }
        }, 500) // 增加延遲時間到 500ms
      }
    }
  } catch (error) {
    console.error('處理區域變更發生錯誤:', error)
    // 確保在發生錯誤時也清除標記
    clearMarkers()
    // 錯誤時不顯示圖表
    chartReady.value = false
  }
}

// 定位到特定感測器
const locateSensor = (sensor: any) => {
  if (sensor.X && sensor.Y && map) {
    try {
      // 轉換為數值以確保正確處理
      const x = parseFloat(sensor.X)
      const y = parseFloat(sensor.Y)
      map.setCenter(new TGOS.TGPoint(x, y))
      map.setZoom(16)  // 放大以便清楚查看
      
      // 滾動頁面到地圖位置
      if (mapContainer.value) {
        // 使用 scrollIntoView 方法滾動到地圖容器
        mapContainer.value.scrollIntoView({ 
          behavior: 'smooth',  // 平滑滾動
          block: 'start'       // 對齊元素的頂部
        });
      }
      
      // 顯示信息窗口
      infoWindow.value.visible = true;
      infoWindow.value.title = sensor.SensorNameA || `感測器 ${sensor.SensorID}`;
      infoWindow.value.sensor = sensor;
      activeMarkerId.value = sensor.SensorID;
    } catch (error) {
      console.error('定位感測器時發生錯誤:', error)
    }
  }
}

// 在地圖上標記感測器
const markSensorsOnMap = (sensorList: any[]) => {
  if (!map) return
  
  // 只處理有座標的感測器
  const validSensors = sensorList.filter(sensor => sensor.X && sensor.Y)
  
  console.log('有效的感測器數量:', validSensors.length)
  
  if (validSensors.length === 0) return
  
  // 計算所有座標的範圍
  let minX = Infinity, maxX = -Infinity
  let minY = Infinity, maxY = -Infinity
  
  validSensors.forEach(sensor => {
    try {
      const x = parseFloat(sensor.X)
      const y = parseFloat(sensor.Y)
      
      // 更新座標範圍
      minX = Math.min(minX, x)
      maxX = Math.max(maxX, x)
      minY = Math.min(minY, y)
      maxY = Math.max(maxY, y)
      
      // 創建標記點
      const point = new TGOS.TGPoint(x, y)
      
      // 定義標記選項
      const markerOptions = {
        flat: false,
        visible: true,
        label: sensor.SensorNameA,
        labelVisible: false // 初始不顯示標籤
      }
      
      // 創建標記
      const marker = new TGOS.TGMarker(map, point, markerOptions)
      
      // 明確設置標題文字，這將在滑鼠懸停時顯示
      marker.setTitle(sensor.SensorNameA)
      
      // 添加滑鼠懸停事件 - 僅改變游標為可點擊狀態
      TGOS.TGEvent.addListener(marker, "mouseover", function() {
        try {
          // 改變游標為可點擊狀態
          map.getDiv().style.cursor = 'pointer';
        } catch (error) {
          console.error('設置游標錯誤:', error);
        }
      });
      
      // 添加滑鼠離開事件 - 恢復游標
      TGOS.TGEvent.addListener(marker, "mouseout", function() {
        try {
          // 恢復游標
          map.getDiv().style.cursor = '';
        } catch (error) {
          console.error('恢復游標錯誤:', error);
        }
      });
      
      // 添加點擊事件
      TGOS.TGEvent.addListener(marker, "click", function(e) {
        try {
          // 如果有先前活動的標記，恢復其狀態
          if (activeMarker && activeMarker !== marker) {
            // 使用安全的方式恢復標記狀態
            // 不需要任何視覺效果，已移除
          }
          
          // 保存當前活動標記
          activeMarker = marker;
          
          // 先將地圖中心設置為標記位置
          // const markerPosition = marker.getPosition();
          // map.setCenter(markerPosition);
          
          // 適當放大地圖（如果當前縮放級別低於15）
          // if (map.getZoom() < 15) {
          //   map.setZoom(15);
          // }
          
          // 已移除標記視覺效果，不再需要設置選中效果
          
          // 設置信息窗口內容
          infoWindow.value.visible = true;
          infoWindow.value.title = sensor.SensorNameA || `感測器 ${sensor.SensorID}`;
          infoWindow.value.sensor = sensor;
          
          // 將活動標記ID保存起來，以便可以在用戶界面中顯示其連接
          activeMarkerId.value = sensor.SensorID;
        } catch (error) {
          console.error('顯示信息窗口時發生錯誤:', error);
        }
      })
      
      markers.push(marker)
    } catch (error) {
      console.error('建立標記時發生錯誤:', error)
    }
  })
  
  // 根據標記位置自動調整地圖視圖
  try {
    if (validSensors.length > 0) {
      // 計算中心點
      const centerX = (minX + maxX) / 2
      const centerY = (minY + maxY) / 2
      map.setCenter(new TGOS.TGPoint(centerX, centerY))
      
      // 計算適當的縮放級別
      // 根據標記的分佈範圍決定縮放級別
      const spanX = maxX - minX
      const spanY = maxY - minY
      const maxSpan = Math.max(spanX, spanY)
      
      // 輸出最大跨度，方便調試
      console.log('標記分佈的最大跨度:', maxSpan)
      
      // 根據範圍大小決定縮放級別
      let zoomLevel
      if (validSensors.length === 1) {
        // 單個標記時使用較高縮放級別
        zoomLevel = 15  // 從16降低到15，增加可視範圍
      } else {
        // 使用對數函數計算適當的縮放級別
        // TGOS 地圖的縮放級別範圍通常是 0-18
        // 這裡我們將最大跨度轉換為縮放級別 9-15
        
        // 經驗閾值調整：
        // 當 maxSpan = 0.005 時，設置 zoomLevel = 14 (原本15)
        // 當 maxSpan = 0.05 時，設置 zoomLevel = 12 (原本13)
        // 當 maxSpan = 0.5 時，設置 zoomLevel = 9 (原本10)
        
        if (maxSpan < 0.005) {
          zoomLevel = 14  // 從15降低到14，增加可視範圍
        } else if (maxSpan < 0.01) {
          zoomLevel = 13  // 從14降低到13
        } else if (maxSpan < 0.05) {
          zoomLevel = 12  // 從13降低到12
        } else if (maxSpan < 0.1) {
          zoomLevel = 11  // 從12降低到11
        } else if (maxSpan < 0.2) {
          zoomLevel = 10  // 從11降低到10
        } else {
          zoomLevel = 9   // 從10降低到9
        }
        
        // 為了避免縮放太近或太遠，我們限制縮放級別在一個合理範圍內
        zoomLevel = Math.max(9, Math.min(zoomLevel, 14))  // 下限從10降低到9，上限從15降低到14
      }
      
      console.log('計算得到的縮放級別:', zoomLevel)
      map.setZoom(zoomLevel)
      
      // 確保地圖類型設置為航照影像
      map.setMapTypeId(TGOS.TGMapTypeId.IMAGENLSC)
    } else {
      // 如果沒有有效標記，則使用默認視圖（台北市）
      map.setCenter(new TGOS.TGPoint(121.5333, 25.0383))
      map.setZoom(11)
      map.setMapTypeId(TGOS.TGMapTypeId.IMAGENLSC)
    }
  } catch (error) {
    console.error('自動調整地圖視圖時發生錯誤:', error)
    // 出錯時使用默認視圖
    map.setCenter(new TGOS.TGPoint(121.5333, 25.0383))
    map.setZoom(11)
    map.setMapTypeId(TGOS.TGMapTypeId.IMAGENLSC)
  }
}

// 清除所有標記
const clearMarkers = () => {
  markers.forEach(marker => {
    marker.setMap(null)
  })
  markers = []
  
  // 確保清除標記時也關閉信息窗口
  closeInfoWindow()
  activeMarkerId.value = null
}

function initMap() {
  if (!mapContainer.value) return

  // 初始化 TGOS 地圖，設定為航照影像
  map = new TGOS.TGOnlineMap(
    mapContainer.value,
    TGOS.TGCoordSys.EPSG3857,
    { 
      mapTypeControl: false,  // 關閉地圖類型切換控制項
      scaleControl: false,    // 關閉比例尺控制項
      navigationControl: false, // 關閉導航控制項
      disableDefaultUI: true  // 禁用所有默認UI元素
    }
  )
  
  // 設置初始位置（台北市）
  map.setCenter(new TGOS.TGPoint(121.5333, 25.0383)) // 台北市中心點
  map.setZoom(11)  // 縮小視角以顯示整個台北市
  
  // 設定地圖類型
  map.setMapTypeId(TGOS.TGMapTypeId.IMAGENLSC)
}

onMounted(() => {
  // 獲取區域列表
  fetchAreaList()

  // 確保 TGOS API 已載入
  if (typeof TGOS !== 'undefined') {
    initMap()
  } else {
    // 等待 API 載入
    const checkTGOS = setInterval(() => {
      if (typeof TGOS !== 'undefined') {
        clearInterval(checkTGOS)
        initMap()
      }
    }, 100)
  }
})
</script>

<style lang="scss" scoped>
.search {
  width: 100%;
}

.search-tabs {
  margin-bottom: 0;
  align-items: flex-start;
  padding-left: 12px;
}

.search-main {
  background: #fff;
  border-radius: 0px 12px 0px 0px;
  box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
  margin-left: 12px;
}

:deep(.custom-table) {
  .el-table__body-wrapper {
    overflow-y: auto;
  }
}

.no-data {
  color: #666;
}

.sensor-list {
  margin-top: 20px;
  padding: 16px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow-x: auto;
}

.chart-container {
  margin-top: 20px;
  margin-bottom: 20px;
  padding: 16px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  
  h3 {
    margin-bottom: 16px;
  }
  
  .chart-wrapper {
    height: 400px;
    width: 100%;
  }
}

.control-panel {
  margin-bottom: 16px;
  padding: 16px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.map-container {
  margin-bottom: 16px;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  position: relative; /* 確保容器是相對定位 */
  
  /* 隱藏 TGOS 地圖縮放控制項 */
  :deep(.TGMap .ZoomControl) {
    display: none !important;
  }
  
  :deep(.TGMap .NavigationControl) {
    display: none !important;
  }
  
  :deep(.TGMap div[style*="zIndex: 0"]) {
    display: none !important;
  }
}

/* 信息窗口樣式 */
.info-window {
  position: absolute;
  background: white;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
  width: 280px;
  z-index: 999; /* 確保在TGOS地圖上層但不超過其他UI元素 */
  border: 1px solid #dcdfe6;
  pointer-events: auto;
  bottom: 15px;   /* 距地圖底部的距離 */
  left: 15px;     /* 距地圖左側的距離，從右側改為左側 */
  max-height: 80%; /* 限制最大高度，避免超出地圖 */
  overflow-y: auto; /* 內容過多時可滾動 */
}

.info-window-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f5f7fa;
  padding: 8px 12px;
  border-bottom: 1px solid #e4e7ed;
  border-radius: 4px 4px 0 0;
}

.info-window-title {
  font-weight: bold;
  color: #409EFF;
}

.info-window-close {
  cursor: pointer;
  font-size: 18px;
  color: #909399;
}

.info-window-close:hover {
  color: #409EFF;
}

.info-window-content {
  padding: 12px;
}

.info-item {
  margin-bottom: 8px;
  display: flex;
}

.info-item span {
  font-weight: bold;
  margin-right: 8px;
  width: 80px;
}

/* 高亮顯示當前選中的感測器 */
.marker-highlight {
  background-color: #ecf5ff;
  padding: 5px;
  border-radius: 4px;
  margin-bottom: 10px;
  border-left: 3px solid #409EFF;
}

/* 添加指向標記的三角形 */
.info-window-pointer {
  display: none; /* 先隱藏三角形，因為窗口在角落不需要方向指示 */
  position: absolute;
  width: 0;
  height: 0;
  border-style: solid;
  border-width: 0 10px 10px 10px;
  border-color: transparent transparent white transparent;
  top: -10px;
  left: 50%;
  transform: translateX(-50%);
}

/* 添加指向標記的陰影三角形 */
.info-window-pointer::before {
  content: '';
  position: absolute;
  width: 0;
  height: 0;
  border-style: solid;
  border-width: 0 11px 11px 11px;
  border-color: transparent transparent #dcdfe6 transparent;
  top: -1px;
  left: -11px;
  z-index: -1;
}
</style>
