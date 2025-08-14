<template>
    <dialog :id="modalId" class="v-dialog">
        <button type="button" class="close" @click="close()"> &times; </button>
        <div class="title">{{ title }}</div>
        <hr>
        <div v-if="isOpen">
            <div v-if="deleting" class="notes">刪除中...</div>
            <div v-else-if="allFiles === undefined" class="notes">讀取檔案中...</div>
            <div v-else-if="allFiles.length === 0" class="notes">無檔案</div>
            <div v-else v-for="f in allFiles" @click="open(f)" class="file-row">
                {{ f }}
                <img :src="deleteIcon" class="icon" @click.stop="deleteDrawing(f)">
            </div>
        </div>

        <div v-if="isSave">
            <div v-if="allFiles === undefined" class="notes">讀取檔案中...</div>
            <div v-else-if="saveAs === null">
                <div @click="saveAs = false" class="action-row">儲存為現有檔名</div>
                <div @click="saveAs = true" class="action-row">另存新檔</div>
            </div>
            <div v-else-if="saveAs">
                <div class="v-input-title">另存新檔為</div>
                <input type="text" v-model="fileName" class="v-input">
                <div style="text-align: center;">
                    <button class="v-button save" @click="save">
                        {{ saving ? '儲存中...' : '儲存' }}
                    </button>
                </div>
            </div>
            <div v-else v-for="f in allFiles" @click="replace(f)" class="file-row">
                {{ saving && savingFileName === f ? '儲存中...' : f }}
            </div>
        </div>
    </dialog>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, defineProps, watch } from 'vue';
import MyUtil from '@/resource/myUtil.js';
import FileMode from '@/resource/map/fileMode.js';
import { apiClient } from '@/resource/index.ts';

let modalId = MyUtil.getRandomId();
let allFiles = ref(undefined);
let fileName = ref('');
let deleting = ref(false);
let saving = ref(false);
let savingFileName = ref(null);
let saveAs = ref(null);
let deleteIcon = require(`@/assets/image/map/delete.png`);

let props = defineProps(['mode', 'load', 'save', 'changesUnsaved']);
let emits = defineEmits(['closed']);

watch(
    () => props.mode,
    () => {
        let modal = getModal();

        if (props.mode === FileMode.HIDDEN) {
            modal.close();
        } else {
            updateFileList();
            modal.showModal();
        }
    });


let title = computed(() => {
    if (props.mode === FileMode.OPEN) return '開啟標記';
    if (props.mode === FileMode.SAVE) return '儲存標記';
});

let showModal = computed(() => props.mode !== FileMode.HIDDEN);
let isOpen = computed(() => props.mode === FileMode.OPEN);
let isSave = computed(() => props.mode === FileMode.SAVE);

function getModal() {
    return document.getElementById(modalId);
}

function close() {
    getModal().close();
}

async function replace(fileName) { 
    let msg = `確認要取代:${fileName}?\n儲存後無法復原`;
    if (!confirm(msg)) return;

    try {
        savingFileName.value = fileName;
        saving.value = true;
        await props.save(fileName);
        close();
    } catch {
        alert('儲存時發生錯誤');
    } finally {
        savingFileName.value = null;
        saving.value = false;
    }
}

async function save() {
    let f = fileName.value.trim();
    if (f.length === 0) {
        alert('檔名不可為空白');
        return;
    }

    if (f.length > 100) {
        alert('檔名不可超過100個字元');
        return;
    }

    try {
        saving.value = true;
        await props.save(f);
        close();
    } catch {
        alert('儲存時發生錯誤');
    } finally {
        saving.value = false;
    }
}

function open(fileName) {
    if (props.changesUnsaved) {
        let msg = '開啟後您將失去未儲存的標記，是否繼續?';
        if (!confirm(msg)) return;
    }

    props.load(fileName);
}

async function deleteDrawing(fileName) {
    let msg = `確認要刪除標記:${fileName}?\n刪除後無法復原`;
    if (!confirm(msg)) return;
    let n = encodeURIComponent(fileName);
    let success = false;

    try {
        deleting.value = true;
        let res = await apiClient.delete(`/GeoJson/DeleteDrawing?name=${n}`);
        success = res.status === 200;
        if (!success) throw new Error(`status code=${res.status}`);

    } catch {
        alert('刪除時發生錯誤');
    } finally {
        deleting.value = false;
    }

    if (success) updateFileList();
}

async function updateFileList() {
    allFiles.value = undefined;
    let res = await apiClient.get(`/GeoJson/ListDrawings`);
    allFiles.value = res.data;
}

onMounted(() => {
    getModal().addEventListener('close', () => {
        saveAs.value = null;
        emits('closed');
    });
});
</script>

<style lang="scss" scoped>
hr {
    margin-bottom: 15px;
}

.title {
    text-align: center;
    font-size: 20px;
}

.notes {
    padding: 50px 0;
    text-align: center;
}

.save {
    background-color: green;
    color: white;
    border: none;
    font-size: 16px;
    margin-top: 30px;
}

.file-row {
    border: 1px #999 solid;
    border-radius: 10px;
    padding: 6px;
    margin: 8px 0;
    cursor: pointer;
}

.action-row {
    border: 1.5px darkblue solid;
    border-radius: 10px;
    padding: 6px;
    margin: 8px 0;
    cursor: pointer;
    text-align: center;
    font-weight: bold;
}

.icon {
    width: 20px;
    float: right;
    margin-top: 2px;
    margin-right: 3px;
    cursor: pointer;
}
</style>
