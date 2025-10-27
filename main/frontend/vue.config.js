const path = require('path');
const { defineConfig } = require('@vue/cli-service');

module.exports = defineConfig({
  // publicPath: '/v2/',
  transpileDependencies: true,
  devServer: {
    host: '0.0.0.0',
    allowedHosts: 'all',
    headers: {
      'Access-Control-Allow-Origin': '*',
    },
    historyApiFallback: true,
    hot: true,
    client: {
      webSocketURL: 'auto://0.0.0.0:0/ws',
    },
  },
  chainWebpack: (config) => {
    config.plugins.delete('preload');
    config.plugins.delete('prefetch');

    // 先刪除預設的 svg 配置
    config.module.rules.delete('svg');

    // 新增 svg-sprite-loader 設定
    config.module
      .rule('svg-sprite-loader')
      .test(/\.svg$/)
      .include.add(path.resolve('src/assets/icon'))
      .end()
      .use('svg-sprite-loader')
      .loader('svg-sprite-loader')
      .options({ symbolId: '[name]' });

    // 修改 images-loader 配置
      config.module.rule('images').exclude.add(path.resolve('src/assets/icon'));
      // 設置 filename 和 chunkFilename 包含內容哈希
      config.output
          .filename('[name].[contenthash].js')
          .chunkFilename('[name].[contenthash].js');
  },
  pluginOptions: {
    'style-resources-loader': {
      preProcessor: 'scss',
      patterns: [path.resolve(__dirname, 'src/styles/variables.scss')],
    },
  },
});
