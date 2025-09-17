<template>
  <div class="login flex w-[460px] flex-col p-6 md:p-0">
    <div class="login-title mb-[24px] text-[20px] font-semibold leading-normal text-primary">
      系統登入<span class="ml-2 text-[14px] text-secondary">Log in</span>
    </div>
    <div class="login-form max-w-[460px]">
      <el-form ref="ruleFormRef" :model="ruleForm" :rules="rules" :hide-required-asterisk="true">
        <div class="mb-[26px]">
          <hover-input v-model="ruleForm.username" label="帳號 Account" name="username" />
        </div>
        <div class="mb-[26px]">
          <hover-input v-model="ruleForm.password" label="密碼 Password" name="password" type="password" show-password />
        </div>
        <div class="mb-[24px] block md:flex">
          <div class="mb-[26px] mr-[4px] md:mb-0">
            <hover-input v-model="ruleForm.captcha" label="驗證碼 CAPTCHA" name="captcha" />
          </div>
          <div class="flex justify-end">
            <div class="mr-[4px] flex items-end justify-end rounded-[8px] bg-black text-5xl text-white line-through">
              {{ tempCaptcha }}
            </div>
            <div
              class="mr-[4px] flex h-[56px] w-[56px] items-center justify-center rounded-[8px] border border-tertiary">
              <el-icon :size="24" class="cursor-pointer" @click="refreshVerificationCode">
                <app-icon icon-name="refresh"></app-icon>
              </el-icon>
            </div>
          </div>
        </div>
        <el-form-item>
          <el-button type="primary" class="w-full" @click="submitForm(ruleFormRef)">
            登入
          </el-button>
        </el-form-item>
      </el-form>
      <div class="mt-[24px] flex items-end justify-end">
        <img src="@/assets/image/logo.png" alt="" />
      </div>

      <!-- 新增：QRCode 導向外部網站 -->
      <div class="mt-4 flex justify-center qr-code">
        <img src="@/assets/image/qr.ioi.tw.png"
          alt="River Monitoring QR Code" class="w-[150px] h-[150px]" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { LOGIN } from '@/store/user/actionTypes';
import type { FormInstance, FormRules } from 'element-plus';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';

interface RuleForm {
  username: string;
  password: string;
  captcha: string;
}

const validateCaptcha = (rule: any, value: any, callback: any) => {
  if (value !== tempCaptcha.value) {
    callback(new Error('Captcha validation failed'));
  } else {
    callback();
  }
};

const ruleFormRef = ref<FormInstance>();
const ruleForm = reactive<RuleForm>({
  username: '',
  password: '',
  captcha: '',
});

const rules = reactive<FormRules<RuleForm>>({
  username: [{ required: true, message: '請輸入帳號', trigger: 'blur' }],
  password: [{ required: true, message: '請輸入密碼', trigger: 'blur' }],
  captcha: [{ required: true, validator: validateCaptcha, trigger: 'blur' }],
});

const router = useRouter();

const store = useStore();
const submitForm = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;

  await formEl.validate();

  const response = await store.dispatch(`user/${LOGIN}`, ruleForm);
  if (response) {
    router.push('/cctv');
  }
};
const tempCaptcha = ref<string>('');
const refreshVerificationCode = (): void => {
  tempCaptcha.value = generateRandomCode();
};
const generateRandomCode = (): string => {
  return Math.floor(100000 + Math.random() * 900000).toString();
};

refreshVerificationCode();

document.addEventListener('keydown', event => {
    if (event.key === 'Enter') submitForm(ruleFormRef.value); 
});
</script>

<style lang="scss" scoped>
@media (max-width: $md) {
  .login {
    z-index: 10;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.9);
    box-shadow: -0.5px 0.5px 3px 0px rgba(0, 0, 0, 0.15);
  }
}

/* QRCode 區塊微調：置中、下方留白 */
.qr-code {
  margin-top: 16px;
}

.qr-code img {
  border: 1px solid #ddd;
  border-radius: 8px;
}
</style>
