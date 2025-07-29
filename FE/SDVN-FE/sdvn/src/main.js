import { createApp } from "vue";
import App from "./App.vue";
import router from "./router"; // Import router
import '@fortawesome/fontawesome-free/css/all.css';

const app = createApp(App);
app.use(router); // Đăng ký router vào ứng dụng Vue
app.mount("#app");
