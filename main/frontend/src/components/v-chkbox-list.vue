<template>
    <span>
        <label v-for="item in items" :key="item.id">
            <input type="checkbox" v-bind:value="item.value || item" />' 
            <span>{{item.text || item}}</span>
        </label>
    </span>
</template>
<script>
    import { computed, nextTick, onMounted, ref, watch, defineComponent } from 'vue';
    import { useStore } from 'vuex';
    export interface Props {
        value?: Array<string>,
        items?: Array<string>,
        exclusive: boolean,
    }

    export default defineComponent({
        data() {
            return {
                StrArrayOptions: ["Value1", "Value2", "Value3"],
                SelValues1: ["Value2"],
                ValueTextArrayOptions: [
                    { value: "Value1", text: "Text1" },
                    { value: "Value2", text: "Text2" },
                    { value: "Value3", text: "Text3" }
                ],
                SelValue2: "Value2"
            }
        }
    })
</script>

<style scoped>
</style>