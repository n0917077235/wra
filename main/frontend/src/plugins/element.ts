import {
  ElAside,
  ElButton,
  ElCard,
  ElCheckbox,
  ElCol,
  ElContainer,
  ElDatePicker,
  ElDivider,
  ElDrawer,
  ElForm,
  ElFormItem,
  ElHeader,
  ElIcon,
  ElInput,
  ElMain,
  ElMenu,
  ElMenuItem,
  ElMenuItemGroup,
  ElOption,
  ElPopover,
  ElRow,
  ElScrollbar,
  ElSelect,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';
import { App } from 'vue';

/**
 * @param app {App}
 */
export default function loadComponent(app: App) {
  app.use(ElRow);
  app.use(ElCol);
  app.use(ElContainer);
  app.use(ElHeader);
  app.use(ElMain);
  app.use(ElForm);
  app.use(ElFormItem);
  app.use(ElInput);
  app.use(ElButton);
  app.use(ElSelect);
  app.use(ElOption);
  app.use(ElCheckbox);
  app.use(ElIcon);
  app.use(ElMenu);
  app.use(ElMenuItem);
  app.use(ElMenuItemGroup);
  app.use(ElDrawer);
  app.use(ElAside);
  app.use(ElScrollbar);
  app.use(ElCard);
  app.use(ElTag);
  app.use(ElDivider);
  app.use(ElTable);
  app.use(ElTableColumn);
  app.use(ElPopover);
  app.use(ElDatePicker);
}
