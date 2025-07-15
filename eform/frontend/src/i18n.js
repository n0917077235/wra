import { createI18n } from 'vue-i18n';

const messages = {
    en: {
      welcome: 'Welcome',
      home: 'Home',
      NoData: 'No data available',
      // Add more translations here
    },
    zhHant: {
      welcome: '歡迎',
      home: '首頁',
      noData: '無資料',
      // Add more translations here
    },
    // Add more languages here
  };

const i18n = createI18n({
  locale: 'zhHant', // Set the default locale
  fallbackLocale: 'zhHant', // Set the fallback locale
  messages,
});

export default i18n;
