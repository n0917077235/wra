<template>
  <div class="search">
    <div class="search-tabs flex">
      <el-tabs v-model="activeTab" tab-position="top">
        <el-tab-pane label="上傳檔案" name="upload" />
        <el-tab-pane label="查詢歷史資料" name="history" />
        <el-tab-pane label="刪除年分資料" name="delete" />
      </el-tabs>
    </div>
    <div class="search-main p-[16px]">
      <div v-if="activeTab === 'upload'">
        <!-- 上傳檔案內容 -->
        <div class="m-2 flex items-center">
          <label class="custom-file-label">
            <input
              type="file"
              accept=".xlsx,.xls"
              @change="handleFileChange"
              class="custom-file-input"
            />
            <span class="custom-file-span">
              {{ selectedFile?.name || '選擇檔案' }}
            </span>
          </label>
          <el-button
            type="primary"
            class="custom-button ml-2"
            @click="uploadFile"
            :disabled="!selectedFile"
          >
            上傳檔案
          </el-button>
        </div>
        <div v-if="devices.length">
          <div class="mb-2">
            年分：
            <el-input
              v-model.number="year"
              size="small"
              style="width: 120px;"
            />
          </div>
          <el-table
            :data="devices"
            style="width: 100%; margin-top: 24px; max-height: 600px; overflow: auto;"
            height="600"
          >
            <el-table-column prop="name" label="設備名稱">
              <template #default="{ row, $index }">
                <div>
                  {{ row.name }}
                  <div>
                    <el-button
                      type="text"
                      size="small"
                      @click="toggleEdit($index)"
                      style="margin-top: 4px;"
                    >
                      {{ editingIndex === $index ? '完成' : '編輯' }}
                    </el-button>
                  </div>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="unit" label="單位" width="80" />
            <el-table-column
              v-for="station in stationList"
              :key="station"
              :prop="'stationCounts.' + station"
              :label="station"
              width="80"
            >
              <template #default="{ row, $index }">
                <span v-if="station === '合計'">
                  {{ getDeviceTotal(row, stationList) }}
                </span>
                <el-input
                  v-else-if="editingIndex === $index"
                  v-model.number="row.stationCounts[station]"
                  size="small"
                  class="table-input"
                  @input="updateDeviceTotal(row, stationList)"
                />
                <span v-else>{{ row.stationCounts[station] }}</span>
              </template>
            </el-table-column>
          </el-table>
          <el-button type="primary" class="mt-4" @click="saveDevices">
            儲存
          </el-button>
        </div>
      </div>
      <div v-else-if="activeTab === 'history'">
        <!-- 查詢歷史資料內容 -->
        <div class="m-4">
          <h3>查詢歷史資料</h3>
          <div class="flex items-center mb-4">
            <div class="mr-8">
              <el-dropdown ref="dropdownYearRef" trigger="click">
                <span class="el-dropdown-link">
                  年分 <i class="el-icon-arrow-down el-icon--right"></i>
                </span>
                <template #dropdown>
                  <div style="padding: 6px 6px; min-width: 200px;">
                    <div style="display: flex; justify-content: space-between; align-items: center; padding: 0 6px; margin: 2px 0;">
                      <span>選擇年分</span>
                      <el-button type="text" size="small" @click="toggleSelectAllYears">
                        {{ isAllYearsSelected ? '取消全選' : '全選' }}
                      </el-button>
                    </div>
                    <el-checkbox-group
                      v-model="queryYears"
                      style="display: flex; flex-wrap: wrap; margin: 0px 0;"
                    >
                      <el-checkbox
                        v-for="year in yearOptions"
                        :key="year"
                        :label="year"
                        style="margin-right: 12px; margin-bottom: 6px;"
                      >{{ year }}</el-checkbox>
                    </el-checkbox-group>
                    <div style="text-align: right;">
                      <el-button type="primary" size="small" @click="closeYearDropdown">確定</el-button>
                    </div>
                  </div>
                </template>
              </el-dropdown>
            </div>
            <div class="mr-8">
              <!-- 設備 -->
              <el-dropdown ref="dropdownDeviceRef" trigger="click">
                <span class="el-dropdown-link">
                  設備 <i class="el-icon-arrow-down el-icon--right"></i>
                </span>
                <template #dropdown>
                  <div style="padding: 6px 6px; min-width: 200px;">
                    <div style="display: flex; justify-content: space-between; align-items: center; padding: 0 6px; margin: 2px 0;">
                      <span>選擇設備</span>
                      <el-button type="text" size="small" @click="toggleSelectAllDevices">
                        {{ isAllDevicesSelected ? '取消全選' : '全選' }}
                      </el-button>
                    </div>
                    <el-checkbox-group
                      v-model="queryDevices"
                      style="display: flex; flex-wrap: wrap; margin: 0px 0;"
                    >
                      <el-checkbox
                        v-for="device in deviceOptions"
                        :key="device"
                        :label="device"
                        class="option-checkbox"
                        :title="device"
                      >
                        <span class="option-checkbox-label">{{ device }}</span>
                      </el-checkbox>
                    </el-checkbox-group>
                    <div style="text-align: right;">
                      <el-button type="primary" size="small" @click="closeDeviceDropdown">確定</el-button>
                    </div>
                  </div>
                </template>
              </el-dropdown>
            </div>
            <div>
              <!-- 站點 -->
              <el-dropdown ref="dropdownStationRef" trigger="click">
                <span class="el-dropdown-link">
                  站點 <i class="el-icon-arrow-down el-icon--right"></i>
                </span>
                <template #dropdown>
                  <div style="padding: 6px 6px; min-width: 200px;">
                    <div style="display: flex; justify-content: space-between; align-items: center; padding: 0 6px; margin: 2px 0;">
                      <span>選擇站點</span>
                      <el-button type="text" size="small" @click="toggleSelectAllStations">
                        {{ isAllStationsSelected ? '取消全選' : '全選' }}
                      </el-button>
                    </div>
                    <el-checkbox-group
                      v-model="queryStations"
                      style="display: flex; flex-wrap: wrap; margin: 0px 0;"
                    >
                      <el-checkbox
                        v-for="station in stationOptions"
                        :key="station"
                        :label="station"
                        class="option-checkbox"
                        :title="station"
                      >
                        <span class="option-checkbox-label">{{ station }}</span>
                      </el-checkbox>
                    </el-checkbox-group>
                    <div style="text-align: right;">
                      <el-button type="primary" size="small" @click="closeStationDropdown">確定</el-button>
                    </div>
                  </div>
                </template>
              </el-dropdown>
            </div>
            <el-button
              type="primary"
              class="ml-8"
              @click="searchHistory"
            >
              查詢
            </el-button>
            <el-button
              v-if="historyDevices.length"
              type="primary"
              class="ml-2"
              @click="downloadHistoryExcel"
            >
              下載 Excel
            </el-button>
          </div>
          <!-- 查詢結果表格 -->
          <el-tabs v-model="activeYearTab" tab-position="top" style="margin-top: 24px;">
            <el-tab-pane
              v-for="group in historyDevices.slice().sort((a, b) => a.year - b.year)"
              :key="group.year"
              :label="group.year"
              :name="group.year"
            >
              <el-table
                :data="group.devices"
                style="width: 100%; margin-top: 8px; max-height: 600px; overflow: auto;"
                height="600"
              >
                <el-table-column prop="name" label="設備名稱" />
                <el-table-column prop="unit" label="單位" width="80" />
                <el-table-column
                  v-for="station in group.stationList"
                  :key="station"
                  :prop="'stationCounts.' + station"
                  :label="station"
                  width="80"
                >
                  <template #default="{ row }">
                    <span v-if="station === '合計'">{{ getDeviceTotal(row, group.stationList) }}</span>
                    <span v-else>{{ row.stationCounts[station] }}</span>
                  </template>
                </el-table-column>
              </el-table>
            </el-tab-pane>
          </el-tabs>
        </div>
      </div>
      <div v-else-if="activeTab === 'delete'">
        <!-- 刪除年分資料內容 -->
        <div class="m-4">
          <h3>刪除特定年分資料</h3>
          <div class="flex items-center mb-4">
            <span class="mr-2">選擇年分：</span>
            <el-select v-model="deleteYear" placeholder="請選擇年分" style="width: 120px;">
              <el-option
                v-for="year in yearOptions"
                :key="year"
                :label="year"
                :value="year"
              />
            </el-select>
            <el-button
              type="danger"
              class="ml-4"
              :disabled="!deleteYear"
              @click="handleDeleteYear"
            >
              刪除
            </el-button>
          </div>
          <div v-if="deleteResult !== null" class="mt-2">
            <span v-if="deleteResult.success" style="color: green;">
              刪除成功，已刪除 {{ deleteResult.deleted }} 筆資料。
            </span>
            <span v-else style="color: red;">
              刪除失敗。
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { apiUploadDeviceRepairExcel, apiSaveDeviceRepairHistory, apiQueryDeviceRepairHistory, apiGetDeviceRepairOptions, apiDeleteDeviceRepairYear } from '@/resource/devicerepair'
import { ElButton, ElTable, ElTableColumn, ElInput, ElTabs, ElTabPane, ElDropdown, ElSelect } from 'element-plus'
import * as XLSX from 'xlsx'

const activeTab = ref('history')
const selectedFile = ref<File | null>(null)
const year = ref<number>(0)
const devices = ref<any[]>([])
const stationList = ref<string[]>([])
const editingIndex = ref<number | null>(null)
const queryYears = ref<string[]>([])
const queryDevices = ref<string[]>([])
const queryStations = ref<string[]>([])
const yearOptions = ref<string[]>(['111', '112', '113']) // 之後動態取得
const deviceOptions = ref<string[]>(['設備A', '設備B', '設備C']) // 之後動態取得
const stationOptions = ref<string[]>(['站點A', '站點B', '站點C']) // 之後動態取得
const historyData = ref<any[]>([])
const historyDevices = ref<any[]>([])
const historyStationList = ref<string[]>([])
const dropdownYearRef = ref()
const dropdownDeviceRef = ref()
const dropdownStationRef = ref()
const activeYearTab = ref('')
const deleteYear = ref<string>('')
const deleteResult = ref<any>(null)

onMounted(async () => {
  try {
    const res = await apiGetDeviceRepairOptions()
    yearOptions.value = res.years || []
    deviceOptions.value = res.devices || []
    stationOptions.value = res.stations || []
    
    // 自動初始化查詢參數
    queryYears.value = [...yearOptions.value]
    queryDevices.value = [...deviceOptions.value]
    queryStations.value = [...stationOptions.value]
    
    // 自動執行查詢，顯示歷史數據
    await searchHistory()
  } catch (e) {
    console.error(e)
  }
})

function toggleEdit(index: number) {
    editingIndex.value = editingIndex.value === index ? null : index
}

function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement
    if (target.files && target.files.length > 0) {
        selectedFile.value = target.files[0]
    } else {
        selectedFile.value = null
    }
}

async function uploadFile() {
    if (!selectedFile.value) return

    try {
        const response = await apiUploadDeviceRepairExcel(selectedFile.value)
        year.value = response.year
        devices.value = Array.isArray(response.devices) ? response.devices : []
        const stations = new Set<string>()
        devices.value.forEach(d => {
            Object.keys(d.stationCounts || {}).forEach(s => stations.add(s))
        })
        let arr = Array.from(stations)
        stationList.value = ['合計', ...arr]
        devices.value.forEach(d => {
            stationList.value.forEach(station => {
                if (typeof d.stationCounts[station] !== 'number') {
                    d.stationCounts[station] = Number(d.stationCounts[station]) || 0;
                }
            });
            updateDeviceTotal(d, stationList.value);
        })
        alert('檔案上傳成功')
    } catch (error) {
        alert('檔案上傳失敗')
    }
}

function updateDeviceTotal(device, stationList) {
    let total = 0;
    stationList.forEach(station => {
        if (station !== '合計') {
            total += Number(device.stationCounts[station]) || 0;
        }
    });
    device.stationCounts['合計'] = total;
}

function getDeviceTotal(device, stationList) {
    let total = 0
    stationList.forEach(station => {
        if (station !== '合計') {
            total += Number(device.stationCounts[station]) || 0
        }
    })
    return total
}

async function saveDevices() {
    try {
        const devicesToSave = devices.value.map(d => {
            const stationCounts = { ...d.stationCounts }
            delete stationCounts['合計']
            return { ...d, stationCounts }
        })
        await apiSaveDeviceRepairHistory({
            year: year.value,
            devices: devicesToSave
        })
        alert('儲存成功')
    } catch (error) {
        alert('儲存失敗')
    }
}

async function searchHistory() {
  // 1. 查詢參數
  const years = queryYears.value.length ? queryYears.value : yearOptions.value
  const devices = queryDevices.value.length ? queryDevices.value : deviceOptions.value
  const stations = queryStations.value.length ? queryStations.value : stationOptions.value
  console.log('查詢參數:', { years, devices, stations })

  // 2. 查詢 API 回應
  const response = await apiQueryDeviceRepairHistory({
    years,
    devices,
    stations
  });

  // 3. 原始資料
  const raw = Array.isArray(response) ? response : [];

  // 4. 分組 Map
  const yearDeviceMap = new Map<string, Map<string, any>>()
  const yearStationSet = new Map<string, Set<string>>()

  raw.forEach(item => {
    const year = String(item.year)
    if (!yearDeviceMap.has(year)) {
      yearDeviceMap.set(year, new Map())
      yearStationSet.set(year, new Set())
    }
    const deviceKey = item.device + '|' + item.unit
    if (!yearDeviceMap.get(year).has(deviceKey)) {
      yearDeviceMap.get(year).set(deviceKey, {
        name: item.device,
        unit: item.unit,
        stationCounts: {}
      })
    }
    yearDeviceMap.get(year).get(deviceKey).stationCounts[item.station] = item.count
    yearStationSet.get(year).add(item.station)
  })

  // 5. 整理成 historyDevices
  historyDevices.value = []
  yearDeviceMap.forEach((deviceMap, year) => {
    let devicesArr = Array.from(deviceMap.values())
    let stationListArr = ['合計', ...Array.from(yearStationSet.get(year))]

    // 計算合計
    devicesArr.forEach(d => {
      stationListArr.forEach(station => {
        if (station !== '合計') {
          if (typeof d.stationCounts[station] !== 'number') {
            d.stationCounts[station] = Number(d.stationCounts[station]) || 0
          }
        }
      })
      updateDeviceTotal(d, stationListArr)
    })

    // 過濾掉合計為0的設備（行）
    devicesArr = devicesArr.filter(d => d.stationCounts['合計'] > 0)

    // 過濾掉所有設備都為0的站點（列）
    // 取得要保留的站點（合計以外）
    const validStations = stationListArr.filter(station => {
      if (station === '合計') return true
      return devicesArr.some(d => d.stationCounts[station] > 0)
    })
    // 重新組合 stationListArr
    stationListArr = validStations

    // 重新計算合計
    devicesArr.forEach(d => {
      updateDeviceTotal(d, stationListArr)
    })

    historyDevices.value.push({
      year,
      devices: devicesArr,
      stationList: stationListArr
    })
  })
  // 預設分頁選最小年份
  if (historyDevices.value.length > 0) {
    const minYear = Math.min(...historyDevices.value.map(g => Number(g.year)))
    activeYearTab.value = String(minYear)
  }
}

function closeYearDropdown() {
  dropdownYearRef.value.handleClose()
}
function closeDeviceDropdown() {
  dropdownDeviceRef.value.handleClose()
}
function closeStationDropdown() {
  dropdownStationRef.value.handleClose()
}
function selectAllYears() {
  queryYears.value = [...yearOptions.value]
}
function selectAllDevices() {
  queryDevices.value = [...deviceOptions.value]
}
function selectAllStations() {
  queryStations.value = [...stationOptions.value]
}

// 年份全選
const isAllYearsSelected = computed(() =>
  queryYears.value.length === yearOptions.value.length
)
function toggleSelectAllYears() {
  if (isAllYearsSelected.value) {
    queryYears.value = []
  } else {
    queryYears.value = [...yearOptions.value]
  }
}

// 設備
const isAllDevicesSelected = computed(() =>
  queryDevices.value.length === deviceOptions.value.length
)
function toggleSelectAllDevices() {
  if (isAllDevicesSelected.value) {
    queryDevices.value = []
  } else {
    queryDevices.value = [...deviceOptions.value]
  }
}

// 站點
const isAllStationsSelected = computed(() =>
  queryStations.value.length === stationOptions.value.length
)
function toggleSelectAllStations() {
  if (isAllStationsSelected.value) {
    queryStations.value = []
  } else {
    queryStations.value = [...stationOptions.value]
  }
}

async function handleDeleteYear() {
  if (!deleteYear.value) return
  const confirm = window.confirm(`確定要刪除 ${deleteYear.value} 年所有資料嗎？此操作無法復原！`)
  if (!confirm) return
  try {
    const result = await apiDeleteDeviceRepairYear(Number(deleteYear.value))
    deleteResult.value = result
    if (result.success) {
      const res = await apiGetDeviceRepairOptions()
      yearOptions.value = res.years || []
    }
  } catch (e) {
    deleteResult.value = { success: false }
  }
}

function downloadHistoryExcel() {
  if (!historyDevices.value.length) return

  const workbook = XLSX.utils.book_new()

  historyDevices.value.forEach(group => {
    // 表頭
    const header = ['設備名稱', '單位', ...group.stationList]
    // 資料
    const data = group.devices.map(device => {
      return [
        device.name,
        device.unit,
        ...group.stationList.map(station => device.stationCounts[station] ?? 0)
      ]
    })
    // 合併表頭和資料
    const sheetData = [header, ...data]
    // 建立工作表
    const worksheet = XLSX.utils.aoa_to_sheet(sheetData)
    // 加入工作表到 workbook，名稱用年份
    XLSX.utils.book_append_sheet(workbook, worksheet, String(group.year))
  })

  // 下載
  XLSX.writeFile(workbook, '設備維修查詢.xlsx')
}
</script>

<style lang="scss" scoped>
.search {
  width: 100%;
}
.search-tabs {
  margin-bottom: 0;
  align-items: flex-start;
  padding-left: 12px; // 原本24px，減半
}
.search-main {
  background: #fff;
  border-radius: 0px 12px 0px 0px;
  box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
  margin-left: 12px; // 原本24px，減半
}
.main {
    border-radius: 0px 12px 0px 0px;
    box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
}
.custom-button {
    height: 25px;
    padding: 0 20px;
    font-size: 16px;
    line-height: 25px;
    display: inline-block;
    white-space: nowrap;
}
.ml-2 {
    margin-left: 0.5rem;
}
.custom-file-label {
    position: relative;
    display: inline-block;
    cursor: pointer;
    height: 32px;
    line-height: 32px;
    background: #f5f7fa;
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    padding: 0 16px;
    margin-right: 8px;
    transition: border-color 0.2s;
    color: #606266;
    font-size: 16px;
}
.custom-file-label:hover {
    border-color: #409eff;
}
.custom-file-input {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    opacity: 0;
    cursor: pointer;
}
.custom-file-span {
    pointer-events: none;
}
:deep(.table-input) {
    max-width: 40px;
    min-width: 20px;
    box-sizing: border-box;
}
:deep(.el-table__cell) {
    overflow: hidden;
}
:deep(.table-input .el-input__inner) {
    text-align: center;
    padding: 0;
    min-width: 0;
}
.option-checkbox {
  display: inline-block;
  min-width: 120px;
  max-width: 40vw;
  margin-right: 12px;
  margin-bottom: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  vertical-align: top;
}

.option-checkbox-label {
  display: inline-block;
  max-width: 100px; /* 或你想要的寬度 */
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  vertical-align: middle;
}

@media (max-width: 600px) {
  .option-checkbox {
    min-width: 90px;
    max-width: 80vw;
    margin-right: 8px;
    font-size: 14px;
  }
}
</style>
