import { createRouter, createWebHistory } from "vue-router";
import LoginComponent from "../components/HomePage/LoginComponent.vue";
import RegisterComponent from "../components/HomePage/RegisterComponent.vue";
import ListRoleComponent from "../components/AdminPage/ListRoleComponent.vue";
import AdminPageComponent from "../view/AdminPageComponent.vue";

const routes = [
  { path: "/", component:LoginComponent },
  { path: "/login", component: LoginComponent },
  { path: "/register", component: RegisterComponent },
  { path: "/list-role-dept", component: ListRoleComponent },
  { path: "/admin-page", component: AdminPageComponent },

];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
