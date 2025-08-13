<template>
    <dialog :id="modalId" class="v-dialog">
        <button type="button" class="close" @click="close()"> &times; </button>
        <div class="title">{{ title }}</div>
        <hr>
        <div v-if="isOpen">
            <div v-if="allFiles === undefined" class="notes">讀取檔案中...</div>
            <div v-else-if="allFiles.length === 0" class="notes">無檔案</div>
            <div v-else v-for="f in allFiles" @click="open(f)">{{ f }}</div>
        </div>

        <div v-if="isSave">
            <div v-if="allFiles === undefined" class="notes">讀取檔案中...</div>
            <div class="v-input-title">檔案名稱</div>
            <input type="text" v-model="fileName" class="v-input">
            <!-- <div v-else v-for="f in allFiles"></div> -->
            <div style="text-align: center;">
                <button class="v-button save" @click="save">儲存</button>
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
let props = defineProps(['mode', 'load', 'save']);

watch(
    () => props.mode,
    () => {
        let modal = getModal();

        if (props.mode === FileMode.HIDDEN) {
            modal.close();
        } else {
            allFiles.value = undefined;
            apiClient.get(`/GeoJson/ListDrawings`).then(res => allFiles.value = res.data);
            modal.showModal();
        }
    });

const emits = defineEmits(['closed']);

let showModal = computed(() => props.mode !== FileMode.HIDDEN);

let title = computed(() => {
    if (props.mode === FileMode.OPEN) return '開啟標記';
    if (props.mode === FileMode.SAVE) return '儲存標記';
});

let isOpen = computed(() => props.mode === FileMode.OPEN);
let isSave = computed(() => props.mode === FileMode.SAVE);

function getModal() {
    return document.getElementById(modalId);
}

function close() {
    getModal().close();
    emits('closed');
}

function save() {
    let f = fileName.value.trim();
    if (f.length === 0) {
        alert('檔名不可為空白');
        return;
    }

    if (f.length > 100) {
        alert('檔名不可超過100個字元');
        return;
    }

    props.save(f);
    alert('儲存成功');
}

function open(fileName) {
    // TODO: confirm with user
    props.load(fileName);
}
</script>

<style lang="scss" scoped>
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
</style>
