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

                        <!-- 警告内容 -->
                        <div class="alarm-body">
                            <canvas id="AlarmCanvas"></canvas>
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
    if (isMobileDevice()) return;
    alarmrtn.value = await apiGetWaterEmbankAlarm();
    const alarms = alarmrtn.value.filter(item => item.status !== '');
    alarmrtn.value = alarms;
    shouldShowButton.value = alarms.length > 0;
}

// 當按下警告按鈕時觸發，顯示警告訊息的對話框
const showWarningPanel = () => {
    const box = document.getElementById('alarmbox') as HTMLElement | null;
    if (box) {
        box.style.display = 'block';
    }

    showAlarmMsg.value = true;

    setTimeout(() => {
        const wrapper = document.querySelector(
            '#alarmbox .modal-content'
        ) as HTMLElement | null;
        const Canvas = document.getElementById(
            'AlarmCanvas'
        ) as HTMLCanvasElement | null;

        if (!Canvas) {
            console.warn('找不到 AlarmCanvas');
            return;
        }

        const avail = wrapper ? wrapper.clientWidth : window.innerWidth * 0.9;
        Canvas.width = avail;
        Canvas.height = 50 + (alarmrtn.value?.length || 0) * 30;

        const ctx = Canvas.getContext('2d');
        if (!ctx) return;
        ctx.clearRect(0, 0, Canvas.width, Canvas.height);

        ctx.font = '20px Arial';
        ctx.fillStyle = 'black';
        alarmrtn.value?.forEach((c, i) => {
            const text = `${c.areaName} ${c.sensorName} ${c.status} ${c.lastDataTime} ${c.value}`;
            ctx.fillText(text, 0, 50 + i * 30);
        });
    }, 100);
};


onMounted(() => {
    checkForAlarms(); // 模擬檢查是否有警告，決定是否顯示按鈕
    document.getElementsByClassName("closeBtn2")[0]?.addEventListener('click', function () {
        const modal = document.getElementById("alarmbox");
        if (modal) {
            modal.style.display = "none";
        }
    });
});

</script>

<style scoped>
.modal {
    display: none;
    position: fixed;
    inset: 0;
    /* top/right/bottom/left = 0 */
    background: rgba(0, 0, 0, 0.4);
    padding-top: 5vh;
    overflow: auto;
}

.modal-content {
    margin: 80px auto 0 !important;
    position: relative;
    width: 90%;
    max-width: 800px;
    max-height: 80vh;
    padding: 16px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
    overflow-x: auto !important;
    overflow-y: hidden !important;
    box-sizing: border-box;
}

.alarm-dialog .el-dialog__wrapper,
.alarm-dialog .el-dialog {
    margin: 0;
    width: 100% !important;
    height: 100% !important;
}

.alarm-dialog .el-dialog__header {
    position: sticky !important;
    top: 0;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24px !important;
    background: #fff;
    border-bottom: 1px solid #eee;
}

.alarm-dialog .el-dialog__body {
    padding: 0 !important;
}

.alarm-body {
    padding: 24px;
    white-space: nowrap;
    overflow-x: auto;
    overflow-y: auto;
    box-sizing: border-box;
}

@media (max-width: 768px) {
    .modal-content {
        width: 95%;
        max-height: 90vh;
    }

    .alarm-dialog .el-dialog__header {
        padding: 0 16px !important;
    }

    .alarm-body {
        padding: 16px;
    }
}

.closeBtn2 {
  position: absolute;
  top: 8px;
  right: 8px;
  font-size: 24px;
  line-height: 1;
  cursor: pointer;
  color: #333;
  z-index: 20;
}
</style>
