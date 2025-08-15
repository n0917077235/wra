<template>
    <div class="flex items-center justify-between text-light md:text-black">
        <div class="flex items-center">
            <toggle-logo></toggle-logo>
            <div class="text-[14px] sm:text-[18px]">{{ routerName }}</div>

            <!-- 1130920 警告按鈕，當有異常時顯示 -->
            <el-button v-show="shouldShowButton" @click="showWarningPanel"
                class="alarm-btn h-10 px-2 text-xs ml-0 md:ml-[300px] flex items-center justify-center">
                <img src="@/assets/image/alarm.png" alt="Alarm" class="h-8 w-auto" />
            </el-button>

            <div id="alarmbox" class="modal">
                <div class="modal-content">
                    <el-dialog v-model="showAlarmMsg" custom-class="alarm-dialog" :append-to-body="false"
                        :show-close="false">

                        <span class="closeBtn2" @click="showAlarmMsg = false">&times;</span>

                        <!-- 警告內容：由 Canvas 改為 HTML 文字表格 + 字體大小控制 -->
                        <div class="alarm-body">
                            <div class="alarm-toolbar">
                                <span class="table-header">警告清單（{{ alarmCount }} 筆）</span>
                                <div class="toolbar-spacer"></div>
                                <el-button size="small" @click="decFont" title="縮小字體">A-</el-button>
                                <el-button size="small" @click="resetFont" title="重設字體">A</el-button>
                                <el-button size="small" @click="incFont" title="放大字體">A+</el-button>
                            </div>
                            <div class="table-scroll">
                                <table class="alarm-table" :style="alarmFontStyle" aria-label="警告清單">
                                    <thead>
                                        <tr>
                                            <th class="table-header">流域/區域</th>
                                            <th class="table-header">站點</th>
                                            <th class="table-header">狀態</th>
                                            <th class="table-header col-time">時間</th>
                                            <th class="table-header">數值</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="(c, i) in alarmrtn" :key="i">
                                            <td class="cell-text">{{ c.areaName }}</td>
                                            <td class="cell-text">{{ c.sensorName }}</td>
                                            <td class="status-cell">{{ c.status }}</td>
                                            <td class="cell-text col-time">{{ c.lastDataTime }}</td>
                                            <td class="cell-text">{{ c.value }}</td>
                                        </tr>
                                        <tr v-if="!alarmCount">
                                            <td colspan="5" class="empty-hint">目前沒有警告。</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </el-dialog>
                </div>
            </div>

        </div>
        <span class="text-[12px] sm:text-[16px]">{{ userName }}</span>



    </div>
</template>

<script lang="ts" setup>
import { computed, ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import ToggleLogo from './ToggleLogo.vue';
import { apiGetWaterEmbankAlarm, SensorGeneralQueryDataResponse } from '@/resource/sensor';
const alarmrtn = ref<SensorGeneralQueryDataResponse[]>();
const route = useRoute();
const store = useStore();

const userName = computed<string>(() => store.state.user.userName);
const routerName = computed<string>(() => {
    return (route.matched[1]?.meta?.routerName || '') as string;
});

// 控制警告訊息的顯示
const shouldShowButton = ref(false);  // 控制警告按鈕的顯示
const showAlarmMsg = ref(false);      // 控制警告對話框的顯示
const warningButton = ref<HTMLElement | null>(null);

// 警告訊息的內容
const warningMessage = ref('警告: 當前有異常事件需要處理！');
function isMobileDevice() {
    const mobileDevice = ['Android', 'webOS', 'iPhone', 'iPad', 'iPod', 'BlackBerry', 'Windows Phone']
    let isMobileDevice = mobileDevice.some(e => navigator.userAgent.match(e))
    return isMobileDevice
}
// 開發測試用：打開就注入 mock
const useMockAlarm = false; // true 代表使用 mock 資料

// 顯示的清單筆數
const alarmCount = computed(() => alarmrtn.value?.length ?? 0);

// 字體大小控制（預設 16px，可在 12~22 之間調整，記到 localStorage）
const fontPx = ref<number>(16);
const minFont = 12;
const maxFont = 22;

const alarmFontStyle = computed(() => ({
    fontSize: `${fontPx.value}px`,
    lineHeight: 1.5
}));

function incFont() {
    fontPx.value = Math.min(fontPx.value + 2, maxFont);
    localStorage.setItem('alarmFontPx', String(fontPx.value));
}
function decFont() {
    fontPx.value = Math.max(fontPx.value - 2, minFont);
    localStorage.setItem('alarmFontPx', String(fontPx.value));
}
function resetFont() {
    fontPx.value = 16;
    localStorage.setItem('alarmFontPx', String(fontPx.value));
}

async function checkForAlarms() {
    if (useMockAlarm) {
        alarmrtn.value = [
            {
                areaName: '大漢溪',
                sensorName: '塔寮坑閘門1',
                status: '水位過高',
                lastDataTime: '2025-08-07 09:15:00',
                value: '7.12 M'
            }
        ];
        shouldShowButton.value = true;
        return;
    }

    // 真實呼叫
    alarmrtn.value = await apiGetWaterEmbankAlarm();
    const alarms = alarmrtn.value.filter(item => item.status !== '');
    alarmrtn.value = alarms;
    shouldShowButton.value = alarms.length > 0;
}

// 當按下警告按鈕時觸發，顯示警告訊息的對話框
const showWarningPanel = () => {
    const box = document.getElementById('alarmbox') as HTMLElement | null;
    if (box) box.style.display = 'block';
    showAlarmMsg.value = true;
};


onMounted(() => {
    checkForAlarms(); // 模擬檢查是否有警告，決定是否顯示按鈕
    document.getElementsByClassName("closeBtn2")[0]?.addEventListener('click', function () {
        const modal = document.getElementById("alarmbox");
        if (modal) {
            modal.style.display = "none";
        }
    });
    // 讀取已儲存的字體大小
    const saved = Number(localStorage.getItem('alarmFontPx'));
    if (!Number.isNaN(saved) && saved > 0) {
        fontPx.value = Math.min(Math.max(saved, minFont), maxFont);
    }
});

</script>

<style scoped>
/* === 遮罩與外殼 === */
.modal {
    display: none;
    position: fixed;
    inset: 0;
    z-index: 1000;
    background: rgba(0, 0, 0, 0.4);
    padding-top: 5vh;
    overflow: hidden;
    /* 外層不滾動，避免雙捲軸 */
}

.modal-content {
    margin: 80px auto 0;
    position: relative;
    width: 90%;
    max-width: 900px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 8px 20px rgba(0, 0, 0, .15);
    box-sizing: border-box;
    overflow: hidden;
    /* 裁切內部外溢，圓角不被突破 */
    z-index: 1001;
}

/* === Element Plus Dialog === */
.alarm-dialog .el-dialog__wrapper,
.alarm-dialog .el-dialog {
    margin: 0;
    width: 100%;
    height: 100%;
    background: transparent;
    box-shadow: none;
}

.alarm-dialog .el-dialog__header {
    position: sticky;
    top: 0;
    z-index: 2;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24px;
    background: #fff;
    border-bottom: 1px solid #eee;
}

/* 不讓這層再滾動，捲軸留給 .table-scroll */
.alarm-dialog .el-dialog__body {
    padding: 0;
    overflow: hidden;
}

/* === 內容區 === */
.alarm-body {
    padding: 24px;
    box-sizing: border-box;
    max-width: 100%;
}

/* 工具列 */
.alarm-toolbar {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 12px;
}

.toolbar-spacer {
    flex: 1;
}

/* === 單一滾動容器：同時水平與垂直 === */
.table-scroll {
    position: relative;
    overflow: auto;
    /* 同時支援水平＋垂直 */
    max-height: calc(80vh - 56px - 24px*2);
    /* 視窗—header—上下 padding */
    /* 上式中 56px 為 header 估值、24px 為 .alarm-body 的 padding；如有變化可微調 */
    scrollbar-gutter: stable both-edges;
    /* 預留捲軸空間，避免內容跳動 */
    border-radius: 8px;
    /* 與外殼圓角協調 */
}

/* === 表格：設定最小寬，促使小螢幕可水平捲動 === */
.alarm-table {
    width: 100%;
    min-width: 800px;
    /* 小螢幕時會出現水平捲軸，依需要調整 */
    border-collapse: collapse;
    table-layout: auto;
    /* 防止長字撐破容器 */
    font-size: clamp(14px, 1.8vw, 18px);
    /* 會被 :style 覆蓋（alarmFontStyle） */
}

.alarm-table thead th {
    position: sticky;
    /* 對著 .table-scroll 這個容器吸附 */
    top: 0;
    background: #fff;
    z-index: 1;
    /* 低於 header(2) 高於內容 */
    text-align: center;
    border-bottom: 1px solid #eee;
    padding: 10px 12px;
}

.alarm-table th,
.alarm-table td {
    text-align: center;
    /* 水平置中 */
    padding: 10px 12px;
    border-bottom: 1px solid #f0f0f0;
    vertical-align: middle;
    white-space: nowrap;
    overflow: visible;
    text-overflow: clip;
}

.table-header {
    color: #000;
    font-weight: bold;
}

.cell-text {
    color: #000;
}

.status-cell {
    font-weight: 600;
    color: #d4380d;
}

.empty-hint {
    text-align: center;
    color: #888;
}

/* 關閉按鈕 */
.closeBtn2 {
    position: absolute;
    top: 8px;
    right: 8px;
    font-size: 24px;
    line-height: 1;
    cursor: pointer;
    color: #333;
    z-index: 3;
    /* 高於 header / table */
}

/* 讓「時間」欄位永遠顯示完整字串（不省略），並推動水平捲動 */
.alarm-table .col-time {
    white-space: nowrap;
    /* 不換行 */
    overflow: visible;
    /* 不裁切 */
    text-overflow: clip;
    /* 不顯示省略號 */
    min-width: 220px;
    /* 視你的字串長度可調 200~260px */
}

/* === RWD === */
@media (max-width: 768px) {
    .modal-content {
        width: 95%;
    }

    .alarm-dialog .el-dialog__header {
        padding: 0 16px;
    }

    .alarm-body {
        padding: 16px;
    }

    .table-scroll {
        max-height: calc(90vh - 52px - 16px*2);
    }

    .alarm-table th,
    .alarm-table td {
        padding: 8px 10px;
    }

    /* 若手機 sticky 仍有遮蓋問題，可改成下面這行（擇一） */
    /* .alarm-table thead th { position: static; } */
}
</style>
