<template>
  <div class="flex">
    <div class="absolute md:relative md:min-w-[50%]">
      <img
        src="@/assets/image/login-bg-pc.png"
        class="hidden min-h-screen object-cover md:block"
        alt="logo"
      />

      <img
        src="@/assets/image/login-bg-mobile.png"
        class="block min-h-screen w-screen object-cover md:hidden"
        alt="logo"
      />
    </div>
    <div
      class="flex h-screen min-w-full items-center justify-center px-5 md:min-w-[50%]"
    >
      <router-view></router-view>
    </div>
  </div>
</template>
