<template>
    <div class="main m-4 p-[16px]">
        <el-form :inline="true" :model="ruleForm" class="m-2 flex flex-wrap ">
            <div class="box">
                <div class="title">年度</div>
                <wra-select v-model="ruleForm.year"
                            name="value"
                            :options="availableYears"
                            :withoutValueLabel="true"
                            class="el-select"></wra-select>

                <div class="title">月份</div>
                <wra-select v-model="ruleForm.month"
                            name="value"
                            :options="availableMonths"
                            :withoutValueLabel="true"
                            class="el-select"></wra-select>

                <div class="title">震度至少</div>
                <wra-select v-model="ruleForm.intensity"
                            name="value"
                            :options="options"
                            valueName="value"
                            labelName="label"
                            class="el-select"></wra-select>
                            
                <div>
                    <el-checkbox v-model="ruleForm.checked1" label="堤頂" size="large" class="mr-4"
                        @change="submit" />
                    <el-checkbox v-model="ruleForm.checked2" label="堤底" size="large" class="mr-4" 
                        @change="submit" />
                    <el-checkbox v-model="ruleForm.hasChart" label="等震度圖" size="large" class="mr-4" 
                        @change="submit" />
                </div>

                <div class="title">事件</div>
                <wra-select v-model="ruleForm.event"
                            name="value"
                            :options="eventOptions"
                            :withoutValueLabel="true"
                            class="el-select"
                            @update:model-value="updateRuleRange">
                </wra-select>
                
                <div class="flex items-center buttons">
                    <el-button type="primary"
                            class="custom-button w-full sm:w-fit"
                            @click="clearcondition">
                        重置條件
                    </el-button>
                </div>
            </div>

        </el-form>

        <a v-if="imageData" :href="imageData" target="_blank">
            <img :src="imageData"></img>
        </a>

        <div v-if="hasData"
             class="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-4">
            <el-card v-for="(event, index) in EQresult" :key="index" class="p-2" @click="showHistoryChart(event)">
                <div class="mb-1 text-center">{{ event.sensorName }}</div>
                <div class="flex flex-col items-center md:flex-row md:items-end md:justify-between">
                    <div>
                        <div>所屬流域:{{ event.areaName }}</div>
                        <div>紀錄時間:{{ event.recordTime }}</div>
                        <div>PGA:{{ event.pga !== '' ? (event.pga>=2?event.pga:'') : '' }} gal</div>
                        <div>PGV:{{ event.pgv !== '' ? (event.pgv>=2?event.pgv:'')  : '' }} mm/s</div>
                        <div>
                            震度:
                            {{event.intensity !== ''? (event.intensity > 1? getLabelForIntensity(event.intensity): '未達2級'): '未達2級'
                            }}
                        </div>
                    </div>
                    <img :src="
              require(
                `@/assets/EarthQuake/${
                  event.intensity !== '' ? parseInt(event.intensity) : '0'
                }.png`,
              )
            "
                         alt=""
                         class="w-[50%] border-0" />
                </div>
            </el-card>
        </div>
        <div id="myModal" class="modal"  >
            <div class="modal-content" >
                <span class="closeBtn">&times;</span>
                <el-dialog v-model="showChartDialog" title="歷線圖" width="50%">
                    <canvas id="historyChart"></canvas>
                </el-dialog>
            </div>
        </div>
    </div>
</template>
<script setup lang="ts">
    import {
        EQEventRangeResponse,
        EQEventResponse,
        EarthquakeByRange,
        EarthquakeBySearchPayload,
        EarthquakeBySensorId,
        apiGetEQEventDetail,
        apiGetEQEvent,
        apiGetEQEventRange,
        EQEventDetailResponse,
    } from '@/resource/earthquakes';
    
    import { reactive, ref, watch, onMounted, nextTick, computed } from 'vue';
    import { apiClient } from '@/resource/index';
    import { apiGetIsoseismal, GetIsoseismalRequest, GetIsoseismalResponse } from '@/resource/geojson';
    import { Chart, LineController, LineElement, PointElement, LinearScale, Title ,CategoryScale} from 'chart.js';
    Chart.register(LineController, LineElement, PointElement, LinearScale, Title, CategoryScale);
    import { useStore } from 'vuex';
    import { Line } from 'vue-chartjs';
     
    const imageData = ref(null);
    const hasData = ref<boolean>(false);
    const currentYear = new Date().getFullYear().toString();
    const currentMonth = (new Date().getMonth() + 1).toString();
    const options = [{ value: '2', label: '2' }, { value: '3', label: '3' }, { value: '4', label: '4' }, { value: '5.1', label: '5弱' }, { value: '5.9', label: '5強' }, { value: '6.1', label: '6弱' }, { value: '6.9', label: '6強' }, { value: '7', label: '7' }];
    
    const ruleForm = reactive<EarthquakeBySearchPayload>({
        year: currentYear,
        month: currentMonth,
        event: '',
        checked1: true,
        checked2: true,
        hasChart: false,
        intensity: '3',
    });

    const ruleRange = reactive<EarthquakeByRange>({
        range: '',
        eventTime: '',
        intensity: '',
    });

    const ruleEvent = reactive<EarthquakeBySensorId>({ sensorId: '', eventTag: '', });
    
    onMounted((): void => {
        init();
    });

    async function init(): Promise<void> {
        ruleForm.year = currentYear;
        ruleForm.month = currentMonth;
        getApiGetEQEventRange();
        document.getElementsByClassName('closeBtn')[0]?.addEventListener('click', function () {
            const modal = document.getElementById('myModal');
            if (modal) {
                modal.style.display = "none";
            }
        });
    }
    
    //產生等震度圖
    async function getIsoseismalMap() {
        let eventTime = ruleRange.eventTime;
        let t = encodeURIComponent(eventTime);
        let res = await apiClient.get(`Earthquake/Isoseismal?eventTime=${t}`, { responseType:"blob" });
        let reader = new FileReader();

        reader.onloadend = function () {
            imageData.value = reader.result;
        };
        
        let blob = res.data;
        reader.readAsDataURL(blob);
    }

    function clearcondition() {
        ruleForm.checked1 = true;
        ruleForm.checked2 = true;
        ruleForm.hasChart = false;
        ruleForm.intensity = "3";
        ruleRange.intensity = "3";
    }
    //震度轉換
    function getLabelForIntensity(intensity) {
        const option = options.find(opt => opt.value === intensity.toString());
        return (option ? option.label : intensity)+"級";
    }

    //function handleSelectChange(){ }
    //歷線圖 彈出視窗
    const showChartDialog = ref(false);
    // 初始化 Chart 实例为 null
    let chartInstance: Chart<'line', number[], string >  | null = null;
    let charttitle = ref<string>("");
    const showHistoryChart = (event: EQEventResponse) => {
        //沒event不作動
        if (!event.eventTag) return;
        charttitle = event.sensorName;
        // 在下一次 DOM 更新时创建新的 Chart 实例
        nextTick(() => {
            const modal = document.getElementById('myModal');
            const ctx = document.getElementById('historyChart') as HTMLCanvasElement | null;
            if (modal && ctx) {
                modal.style.display = "block";
                //取歷線圖資料
                ruleEvent.sensorId = event.sensorId;
                ruleEvent.eventTag = event.eventTag;
                getApiGetEQEventDetail();
                
            }
        });
    };

    const availableYears = ref<Array<number>>(
        Array.from({ length: 3 }, (_, i) => new Date().getFullYear() - i),
    );
    const availableMonths = ref<Array<number>>(
        Array.from({ length: (currentYear == ruleForm.year ? parseInt(currentMonth) : 12) }, (_, i) => i + 1).reverse(),
    );
    watch([() => ruleForm.year, () => ruleForm.month,  () => ruleForm.intensity], (): void => {
        if (ruleForm.year && ruleForm.month) {
            ruleForm.event = '';
            getApiGetEQEventRange();
        }
    });
    watch(() => ruleForm.year, (): void => { availableMonths.value = Array.from({ length: (currentYear == ruleForm.year ? parseInt(currentMonth) : 12) }, (_, i) => i + 1).reverse();});

    const availableEvents = ref<EQEventRangeResponse[]>([]);
    const EventDetail = ref<EQEventDetailResponse>([]);

    const eventOptions = computed(() => {
        return availableEvents.value.map(x => {
            let str = x.time;
            let hasChart = x.hasIsoseismalMap;
            if (hasChart) str = str + "(等震度圖)";
            return (ruleForm.hasChart && !hasChart) ? null : str;
        }).filter(x => x !== null);
    });

    const getApiGetEQEventRange = async (): Promise<void> => {
        try {
            const response = await apiGetEQEventRange(ruleForm);
            if (response) {
                availableEvents.value = response;
                submit();
            }
        } catch (error) {
            console.error(error);
        }
    };

    watch([() => ruleRange.range, () => ruleRange.eventTime, () => ruleRange.intensity], (): void => {
        getApiGetEQEvent();
    });

    watch(EventDetail, (n): void => {
        //alert(n.lstRecordTime);
        // 如果已经存在一个 Chart 实例，销毁它
        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null; // 确保 chartInstance 设置为 null
        }
        document.getElementById('historyChart').style.height = '430px'; // 限制圖表高度
        const modal = document.getElementById('myModal');
        const ctx = document.getElementById('historyChart') as HTMLCanvasElement;
        const ctx2 = ctx.getContext('2d') as CanvasRenderingContext2D;
        if (modal && ctx) {
            chartInstance = new Chart(ctx2, {
                type: 'line',
                data: {
                    labels: n.lstRecordTime,
                    datasets: [{
                        label: n.xLabel,
                        data: n.lstX,
                        fill: false,
                        borderColor: 'rgb(192, 0, 0)',
                        backgroundColor: 'rgb(192, 0, 0)',
                        tension: 0.1,
                        borderWidth: 1,
                        pointRadius: 0 // 移除資料端點的圓圈
                    },
                        {
                            label: n.yLabel,
                            data: n.lstY, // 第二條線的數據
                            fill: false,
                            borderColor: 'rgb(192, 192, 0)',
                            backgroundColor: 'rgb(192, 192, 0)',
                            tension: 0.1,
                            borderWidth: 1,
                            pointRadius: 0 // 移除資料端點的圓圈
                        },
                        {
                            label: n.zLabel,
                            data: n.lstZ, // 第三條線的數據
                            fill: false,
                            borderColor: 'rgb(0, 192, 0)',
                            backgroundColor: 'rgb(0, 192, 0)',
                            tension: 0.1,
                            borderWidth: 1,
                            pointRadius: 0 // 移除資料端點的圓圈
                        }]
                }, options: {
                    
                    plugins: {
                        tooltip: {
                            enabled: true, // 启用工具提示
                            callbacks: {
                                label: function (context) {
                                    const label = context.dataset.label || '';
                                    const value = context.raw || '';
                                    return `${label}: ${value}`;
                                }
                            }
                        },
                        title: {
                            display: true,
                            text: charttitle, // 這是你的圖表標題
                            font: {
                                size: 14 // 標題字體大小
                            },
                            color: 'black', // 標題顏色
                            padding: {
                                top: 10,
                                bottom: 10 // 添加標題上下邊距，讓它更清晰
                            }
                        },
                        legend: {
                            display: true, // 顯示圖例
                            position: 'top', 
                            labels: {
                                font: {
                                    size: 12 // 圖例字體大小
                                },
                                color: 'black' // 圖例字體顏色
                            }
                        }
                    },
                    scales: {
                        x: {
                            ticks: {
                                font: {
                                    size: 10 // 設定X軸字體大小，例如設為10
                                }
                            }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                font: {
                                    size: 12, // 增加Y軸刻度的字體大小
                                },
                                color: 'black' // 設置Y軸刻度的顏色
                            },
                            grid: {
                                color: 'rgba(0, 0, 0, 0.2)', // 調整網格線顏色，使其更明顯
                                lineWidth: 1, // 調整網格線的寬度
                            },
                            title: {
                                display: true,
                                text: '加速度 CM/s^2', // 這是X軸的標題
                                font: {
                                    size: 14 // X軸標題的字體大小
                                },
                                color: 'black' // X軸標題的顏色
                            },
                        },
                    },
                    maintainAspectRatio: false // 允許圖表的高寬比被調整
                }
            });
            // 彈出視窗
            showChartDialog.value = true;
        }        
    });

    const EQresult = ref<EQEventResponse[]>([]);
    const getApiGetEQEvent = async (): Promise<void> => {
        try {
            const response = await apiGetEQEvent(ruleRange);
            if (response) {
                EQresult.value = response;
                hasData.value = true;
            }
        } catch (error) {
            console.error(error);
        }
    };

    let dictEventDetail: { [key: string]: EQEventDetailResponse} = {};

    const getApiGetEQEventDetail = async (): Promise<void> => {
        try {
            if (ruleEvent.eventTag == "") return;
            let k = (ruleEvent.eventTag + "," + ruleEvent.sensorId) as string;
            
            if (dictEventDetail[k] == null) {
                const response = await apiGetEQEventDetail(ruleEvent);
                if (response) {
                    EventDetail.value = response;
                    dictEventDetail[k] = response;
                }
            } else {
                EventDetail.value = dictEventDetail[k];
            }
        } catch (error) {
            console.error(error);
        }
    };

    const submit = () => {
        ruleRange.range = getRuleRange(ruleForm);
        ruleRange.intensity = ruleForm.intensity;

        if (eventOptions.value.length > 0) {
            ruleForm.event = eventOptions.value[0];
            updateRuleRange();
        }
    };

    const updateRuleRange = () => {
        let words = ruleForm.event.split('(');
        ruleRange.eventTime = words[0];

        if (words.length >= 2) {
            getIsoseismalMap();
        } else {
            imageData.value = null;
        }
    }

    const getRuleRange = (form: EarthquakeBySearchPayload): string => {
        if (form.checked1 && form.checked2) {
            return '-1';
        } else if (form.checked1) {
            return '1';
        } else if (form.checked2) {
            return '0';
        } else {
            return '-1';
        }
    };
</script>

<style lang="scss" scoped>
    .main {
        border-radius: 0px 12px 0px 0px;
        box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);

        .el-form {
            width: 100%
        }

        .title {
            margin-bottom: 3px;
            font-size: 14px;
        }

        .el-card {
            @media (max-width: $lg) {
                width: 100%;
                height: min-content;
            }

            @media (max-width: $sm) {
                width: 100%;
                height: min-content;
            }
        }

        .el-dialog {
            display: block !important; /* */
            visibility: visible !important; /* */
        }
    }

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

        .closeBtn {
            color: #aaa;
            float: right;
            font-size: 28px;
            font-weight: bold;
        }

        .closeBtn:hover,
        .closeBtn:focus {
            color: black;
            text-decoration: none;
            cursor: pointer;
        }

        .mr-4 {
            margin-right: 1rem;
        }

        .modal-content {
            position: relative; /* 使 .closeBtn 相對於 .modal-content 定位 */
        }

        .closeBtn {
            position: absolute;
            top: 10px; /* 距離頂部的距離，可以根據需要調整 */
            right: 10px; /* 靠右對齊，距離右邊的距離，可以根據需要調整 */
            font-size: 24px; /* 調整按鈕大小 */
            cursor: pointer; /* 鼠標懸停時顯示手型 */
        }

        .custom-button {
            height: 25px; /* 指定按鈕的高度，可以根據需求調整 */
            padding: 0 20px; /* 調整內邊距，確保內容居中 */
            font-size: 14px; /* 調整字體大小 */
            line-height: 25px; /* 確保文字在按鈕內垂直居中 */
            display: inline-block;
            white-space: nowrap;
        }

        .box {
            max-width: 500px;
        }

        .buttons {
            margin-top: 15px;
            margin-bottom: 25px;
        }
</style>
