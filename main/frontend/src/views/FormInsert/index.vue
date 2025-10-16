<template>
  <div class="app-container">
    <!-- 頁面標題 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-icon"><i class="el-icon-data-analysis"></i></div>
        <h1 class="page-title">維護資料查詢</h1>
      </div>
    </div>

    <!-- 主要內容區 -->
    <div class="main-content">
      <!-- 查詢表單區塊 -->
      <div class="search-section">
        <el-form @submit.prevent="maintainQuery">
          <div class="form-grid">
            <!-- 年度月份輸入 -->
            <div class="form-item">
              <div class="form-label">年度/月份</div>
              <el-input
                v-model="ym"
                placeholder="輸入格式 YYYY/MM"
                :rules="[validateDate]"
                clearable
                class="date-input"
              >
                <template #prefix>
                  <i class="el-icon-date"></i>
                </template>
              </el-input>
              <div class="form-hint">請輸入西元年/月，例如：2025/10</div>
            </div>

            <!-- 查詢按鈕 -->
            <div class="action-area">
              <el-button
                type="primary"
                @click="maintainQuery"
                class="action-button"
                :icon="Search"
              >
                查詢資料
              </el-button>
            </div>
          </div>
        </el-form>
      </div>

      <!-- 結果區域 -->
      <div class="results-section" v-if="items.length > 0 || search">
        <!-- 搜尋工具列 -->
        <div class="toolbar">
          <div class="search-wrapper">
            <el-input
              v-model="search"
              placeholder="輸入關鍵字搜尋結果"
              clearable
              :prefix-icon="Search"
              class="filter-input"
            ></el-input>
          </div>
          
          <div class="result-stats" v-if="filteredItems.length > 0">
            找到 {{ filteredItems.length }} 筆符合的紀錄
          </div>
        </div>

        <!-- 資料表格 -->
        <div class="table-responsive">
          <el-table
            :data="filteredItems"
            border
            stripe
            :empty-text="'查無符合條件的資料'"
            class="data-table"
            :header-cell-class-name="'table-header'"
            :cell-class-name="'table-cell'"
            :row-class-name="'table-row'"
          >
            <el-table-column
              prop="row"
              label="序號"
              sortable
              align="center"
              width="70"
              class-name="column-id"
              fixed="left"
            ></el-table-column>
            <el-table-column
              prop="recordtime"
              label="日期"
              sortable
              align="center"
              class-name="column-date"
            ></el-table-column>
            <el-table-column
              prop="station"
              label="站台"
              sortable
              align="center"
              class-name="column-station"
              show-overflow-tooltip
            ></el-table-column>
            <el-table-column
              prop="location"
              label="維護地點"
              sortable
              align="center"
              class-name="column-location"
              show-overflow-tooltip
            ></el-table-column>
            <el-table-column
              prop="tabletype"
              label="維護項目"
              sortable
              align="center"
              class-name="column-type"
              show-overflow-tooltip
            ></el-table-column>
            <el-table-column
              label="下載"
              align="center"
              width="70"
              fixed="right"
              class-name="column-action"
            >
              <template #default="scope">
                <el-button
                  type="primary"
                  :icon="ArrowDown"
                  circle
                  size="small"
                  @click="downloadFile(scope.row.fileUrl)"
                  class="download-button"
                  title="下載"
                ></el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>

        <!-- 分頁控制 -->
        <div class="pagination-container">
          <el-pagination
            background
            layout="total, prev, pager, next"
            :total="filteredItems.length"
            :page-size="pageSize"
            @current-change="handleCurrentChange"
            :current-page="currentPage"
            hide-on-single-page
            class="pagination-control"
          ></el-pagination>
        </div>
      </div>
      
      <!-- 空狀態 -->
      <div class="empty-state" v-if="items.length === 0 && !search">
        <div class="empty-icon">
          <el-icon><Search /></el-icon>
        </div>
        <div class="empty-text">請輸入年度/月份進行查詢</div>
      </div>
    </div>
  </div>
</template>
 
<script>
import axios from "axios";
import { Search, ArrowDown } from '@element-plus/icons-vue';
import { ElMessage, ElLoading } from 'element-plus';

const apiUrl = "http://localhost:5194"; //(env.VITE_APP_API_ENDPOINT);

export default {
  computed: {
    filteredItems() {
      return this.items.filter((item) => {
        return (
          item.location?.toLowerCase().includes(this.search.toLowerCase()) ||
          item.station
            ?.toString()
            .toLowerCase()
            .includes(this.search.toLowerCase()) ||
          item.tabletype
            ?.toString()
            .toLowerCase()
            .includes(this.search.toLowerCase())
        );
      });
    },
  },
  
  data() {
    let d = new Date();
    d.setHours(d.getHours() + 8);
    let ddd = d.toISOString().split("/");
    return {
      search: "",
      items: [],
      yearMonth: ddd[0] + "/" + ddd[1],
  Search: Search,
  ArrowDown: ArrowDown,
      valid: false,
      ym: "",
      currentPage: 1,
      pageSize: 10
    };
  },
  
  methods: {
    validateDate(value) {
      if (value == null || value.length != 7) {
        return "年度/月份(格式YYYY/MM)";
      }
      value = value.substring(0, 7);
      const pattern = /^(19|20)\d{2}\/(0[1-9]|1[0-2])$/;
      this.ym = value;
      return pattern.test(value) || "年度/月份(格式YYYY/MM)";
    },
    
    handleCurrentChange(val) {
      this.currentPage = val;
    },
    
    async maintainQuery() {
      if (!this.ym) {
        ElMessage({
          message: '請輸入有效的年度/月份',
          type: 'warning',
          customClass: 'custom-message'
        });
        return;
      }
      
      try {
        // 顯示載入中狀態
        const loadingInstance = ElLoading.service({
          lock: true,
          text: '資料載入中...',
          spinner: 'el-icon-loading',
          background: 'rgba(255, 255, 255, 0.7)'
        });
        
        const ym = this.ym.split("T")[0].substring(0, 7);
        const response = await axios.get(
          `${apiUrl}/document/maintainquery?yearmonth=${ym}`
        );
        this.items = response.data;
        
        // 關閉載入中
        loadingInstance.close();
        
        if (this.items.length === 0) {
          ElMessage({
            message: '查無符合條件的資料',
            type: 'info',
            customClass: 'custom-message'
          });
        } else {
          ElMessage({
            message: `已找到 ${this.items.length} 筆資料`,
            type: 'success',
            customClass: 'custom-message'
          });
        }
        
        // 重設分頁
        this.currentPage = 1;
        
      } catch (error) {
        console.error("查詢資料時發生錯誤:", error);
        ElMessage({
          message: '查詢資料時發生錯誤',
          type: 'error',
          customClass: 'custom-message'
        });
      }
    },
    
    async downloadFile(fileUrl) {
      try {
        const urls = fileUrl.split(",");
        const encodedLocation = encodeURIComponent(urls[1]);
        const url = `${apiUrl}/document/Download?stationid=${urls[0]}&location=${encodedLocation}&tableType=${urls[2]}&recordtime=${urls[3]}`;
        
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", "");
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      } catch (error) {
        console.error("下載檔案時發生錯誤:", error);
        ElMessage({
          message: '下載檔案時發生錯誤',
          type: 'error',
          customClass: 'custom-message'
        });
      }
    },
  },
};
</script>

<style scoped>
/* 基礎佈局 */
.app-container {
  width: 100%;
  min-height: 100vh;
  background-color: #f6f8fa;
  color: #2c3e50;
  font-family: 'PingFang TC', 'Microsoft JhengHei', sans-serif;
}

/* 頁面標題區塊 */
.page-header {
  background: linear-gradient(135deg, #3498db, #1565C0);
  color: white;
  padding: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  padding: 16px 20px;
}

.title-icon {
  font-size: 24px;
  margin-right: 12px;
}

.page-title {
  font-size: 22px;
  font-weight: 600;
  margin: 0;
  letter-spacing: 1px;
}

/* 主要內容區 */
.main-content {
  max-width: 1200px;
  margin: 24px auto;
  padding: 0 20px;
}

/* 查詢表單區域 */
.search-section {
  background-color: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 20px;
  align-items: center;
}

.form-item {
  display: flex;
  flex-direction: column;
}

.form-label {
  font-weight: 500;
  margin-bottom: 8px;
  color: #333;
  font-size: 15px;
}

.form-hint {
  color: #909399;
  font-size: 13px;
  margin-top: 6px;
}

.date-input {
  width: 100%;
}

.date-input :deep(.el-input__inner) {
  height: 42px;
  font-size: 15px;
}

.action-area {
  display: flex;
  align-items: flex-end;
}

.action-button {
  height: 42px;
  font-size: 15px;
  font-weight: 500;
  padding: 0 24px;
  background-color: #1890ff;
  border-color: #1890ff;
  transition: all 0.3s;
}

.action-button:hover {
  background-color: #40a9ff;
  border-color: #40a9ff;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.15);
}

/* 結果區域 */
.results-section {
  background-color: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  flex-wrap: wrap;
  gap: 12px;
}

.search-wrapper {
  flex-grow: 1;
  max-width: 400px;
}

.filter-input {
  width: 100%;
}

.result-stats {
  font-size: 14px;
  color: #606266;
}

/* 表格樣式 */
.table-responsive {
  overflow-x: auto;
  width: 100%;
  margin-bottom: 16px;
  border-radius: 8px;
}

.data-table {
  width: 100%;
  border-radius: 8px;
}

:deep(.table-header) {
  background-color: #f5f7fa !important;
  color: #303133 !important;
  font-weight: 600 !important;
  height: 48px;
  font-size: 14px;
}

:deep(.table-cell) {
  padding: 12px 0 !important;
}

:deep(.column-id) {
  font-weight: bold;
}

:deep(.column-action) {
  padding: 8px !important;
}




.download-button {
  transition: box-shadow 0.3s, transform 0.3s;
  width: 32px;
  height: 32px;
  min-width: 32px;
  min-height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  padding: 0;
  font-size: 14px;
  overflow: visible;
  line-height: 32px;
}

.download-button :deep(svg),
.download-button :deep(i) {
  width: 14px;
  height: 14px;
  font-size: 14px;
  margin: 0;
  transition: none;
}

.download-button:hover {
  background: #e3f2fd;
  box-shadow: 0 0 0 4px rgba(24, 144, 255, 0.12), 0 4px 12px rgba(24, 144, 255, 0.18);
  color: #1976d2;
}

/* 分頁控件 */
.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

.pagination-control {
  padding: 8px 0;
}

/* 空狀態 */
.empty-state {
  padding: 60px 0;
  text-align: center;
  color: #909399;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  color: #dcdfe6;
}

.empty-text {
  font-size: 16px;
}

/* 自定義消息 */
:global(.custom-message) {
  min-width: 240px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
}

/* 響應式設計 */
@media (max-width: 768px) {
  .header-content {
    padding: 14px 16px;
  }
  
  .page-title {
    font-size: 20px;
  }
  
  .main-content {
    margin: 16px auto;
    padding: 0 16px;
  }
  
  .search-section,
  .results-section {
    padding: 16px;
    border-radius: 8px;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .action-area {
    width: 100%;
  }
  
  .action-button {
    width: 100%;
  }
  
  .toolbar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-wrapper {
    max-width: 100%;
    margin-bottom: 12px;
  }
  
  .table-responsive {
    margin: 0 -16px;
    width: calc(100% + 32px);
    border-radius: 0;
  }
  
  :deep(.el-table) {
    border-radius: 0;
  }
  
  :deep(.table-header) {
    font-size: 13px;
    padding: 8px 0 !important;
  }
  
  :deep(.table-cell) {
    padding: 8px 4px !important;
    font-size: 13px;
  }
  
  .pagination-container {
    margin: 16px 0 0;
    overflow-x: auto;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 18px;
  }
  
  .main-content {
    padding: 0 12px;
  }
  
  .search-section,
  .results-section {
    padding: 16px 12px;
  }
  
  .table-responsive {
    margin: 0 -12px;
    width: calc(100% + 24px);
  }
  
  :deep(.table-header),
  :deep(.table-cell) {
    font-size: 12px;
    padding: 8px 4px !important;
  }
}
</style>