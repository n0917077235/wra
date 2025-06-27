import { App } from 'vue';

/**
 *
 * @param app
 */
export function install(app: App): void {
  const files = require.context('.', true, /\.ts$/);
  files.keys().forEach((key) => {
    if (typeof files(key).default === 'function') {
      files(key).default(app);
    }
  });
}
