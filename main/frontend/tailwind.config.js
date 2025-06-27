/** @type {import('tailwindcss').Config} */
module.exports = {
  corePlugins: {
    preflight: false,
  },
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      screens: {
        sm: '480px',
      },
    },
    colors: {
      primary: '#015495',
      hover: '#2A78B6',
      pressed: '#123C57',
      disabled: '#DDDDDD',
      red: '#F22A2A',
      black: '#333333',
      secondary: '#666666',
      tertiary: '#999999',
      disable: '#DDDDDD',
      line: '#F6F6F6',
      light: '#FCFCFC',
      white: '#FFFFFF',
    },
  },
  plugins: [],
};
