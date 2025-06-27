<template>
    <div class="flex items-center justify-between text-light md:text-black">
        <div class="flex items-center">
            <toggle-logo></toggle-logo>
            <div class="text-[14px] sm:text-[18px]">{{ routerName }}</div>

            <!-- 1130920 警告按鈕，當有異常時顯示 -->
            <el-button v-show="shouldShowButton" @click="showWarningPanel" style="height:40px;margin-left: 300px; padding: 5px 10px; font-size: 12px;">
                <img src="@/assets/image/alarm.png" alt="Button Image" style="width: 100%; height: 35px; margin-right: 0px;">
            </el-button>
            <div id="alarmbox" class="modal">
                <div class="modal-content" style="width: 1300px; height: 500px; overflow-y: auto; border: 1px solid #ccc;">
                    <el-dialog v-model="showAlarmMsg" title="Alarm Message" style="width: 1300px; height: 500px; overflow-y: auto; border: 1px solid #ccc;">
                        <span class="closeBtn2">&times;</span>
                        <canvas id="AlarmCanvas" width="1300"></canvas>
                    </el-dialog>
                </div>
            </div>
        </div>
        <span class="text-[12px] sm:text-[16px]">{{ userName }}</span>



    </div>
</template>

<script lang="ts" setup>
    import { computed, ref,onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
    import ToggleLogo from './ToggleLogo.vue';
    import { apiGetWaterEmbankAlarm, SensorGeneralQueryDataResponse } from '@/resource/sensor';
    const alarmrtn=ref<SensorGeneralQueryDataResponse[]>();
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
    // 檢查是否需要顯示警告按鈕（模擬條件）
    async function checkForAlarms() {
        //1131025 do not show in phone
        if (isMobileDevice()) return;
        //alert("in");
        /* 模擬條件檢查，如果有異常，就顯示按鈕
            alert("in");
            alert(alarmrtn.length);
            let alarms = alarmrtn.filter(item => item.status !== '');
            alert(alarms.length);
            */
        alarmrtn.value = await apiGetWaterEmbankAlarm();
        let alarms = alarmrtn.value.filter(item => item.status!="");
        //alert(alarmrtn.value.length);
        //alert(alarmrtn.value[0].status.charCodeAt(0));
        //alert(alarms.length);
        alarmrtn.value = alarms;
        if (alarmrtn.value.length > 0) {
            shouldShowButton.value = true;
        }



    };

    // 當按下警告按鈕時觸發，顯示警告訊息的對話框
    const showWarningPanel = () => {
        //alert("showWarningPanel");
        setTimeout(() => {
            const box = document.getElementById('alarmbox') as HTMLElement;
            const Canvas = document.getElementById('AlarmCanvas');
            if (Canvas) {
                if (alarmrtn.value)
                    Canvas.height = 100 + alarmrtn.value.length * 30;
            }
            const ctx = Canvas?.getContext('2d');
            if (box && ctx) {
                box.style.display = "block";
                ctx.font = '20px Arial';
                //alert(ctx.height);

                //ctx.fillStyle = 'orange';
                //.fillRect(10, 10, 400, 500);
                ctx.fillStyle = 'black';
                //alert(ctx.height);

                let msg = "";
                alarmrtn.value?.forEach(function (c, index, array) {
                    let msg1 = c.areaName + " " + c.sensorName + " " + c.status + " " + c.lastDataTime + " " + c.value;
                    //let msg1 = c.status + " " + c.more;
                    ctx.fillText(msg1, 20, 50 + index * 30);
                    //if (index < 10)
                        //alert(msg1 + " 50+" + index * 30);
                    //msg = msg + msg1;
                });

            }
        }, 100);
        // 彈出視窗
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
    });

</script>

<style>
    .modal {
        display: none;
        position: fixed;
        z-index: 1;
        padding-top: 80px;
        left: 0;
        top: 0;
        width: 100%;
        height: 100%;
        overflow: auto;
        background-color: rgb(0, 0, 0);
        background-color: rgba(0, 0, 0, 0.4);
    }

    .modal-content {
        background-color: #fefefe;
        margin: auto;
        padding: 20px;
        border: 1px solid #888;
        width: 90%;
    }

    .closeBtn2 {
        position: relative;
        top: 0px; /* 距離頂部的距離，可以根據需要調整 */
        left: 0px; /* 靠右對齊，距離右邊的距離，可以根據需要調整 */
        font-size: 24px; /* 調整按鈕大小 */
        cursor: pointer; /* 鼠標懸停時顯示手型 */
    }
</style>
