<template>
  <router-view />
</template>

<style lang="scss">
:root {
  font-family: '微軟正黑體', 'Inter', sans-serif;
}
@supports (font-variation-settings: normal) {
  :root {
    font-family: '微軟正黑體', 'Inter', sans-serif;
  }
}

@media (min-width: $lg) {
  /* width */
  ::-webkit-scrollbar {
    width: 6px;
    height: 6px;
  }

  /* Track */
  ::-webkit-scrollbar-track {
    border-radius: 10px;
    background-color: $disable;
  }

  /* Handle */
  ::-webkit-scrollbar-thumb {
    background-color: $primary;
    transition: background-color 0.3s;
    border-radius: 10px;
  }

  ::-webkit-scrollbar-corner {
    background-color: transparent;
  }

  /* Handle on hover */
  ::-webkit-scrollbar-thumb:hover {
    background-color: $primary;
  }
}
</style>
