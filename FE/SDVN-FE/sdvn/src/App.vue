<script setup>
import { useRouter, useRoute } from "vue-router";
import { watchEffect } from "vue";

const router = useRouter();
const route = useRoute();

// // Khi ứng dụng chạy, nếu đang ở trang chủ ("/"), tự động chuyển hướng đến "/login"
// watchEffect(() => {
//   if (route.path === "/") {
//     router.push("/login");
//   }
// });
</script>

<template>
  <router-view />
</template>
