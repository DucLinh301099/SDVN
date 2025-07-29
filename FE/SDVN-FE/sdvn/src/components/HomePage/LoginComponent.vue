<template>
    <div class="login-page">
      <div class="login-container">
    <div class="header-login">
      <div class="logo-container">
       <router-link to="/">
        <img src="D:\VueJs\SDVN\SDVN-FE\sdvn\src\assets\logo_sdvn.jpg" alt="SDVN Logo" class="main-logo1" />
      </router-link>
      </div>
      <div class="main-title">
       <span class="bold">Đăng Nhập</span>
       </div>
      </div>
        <form @submit.prevent="handleLogin">
          <div class="form-group">
            <input v-model="phonenumber" type="text" class="login-input" placeholder="Số điện thoại/Email" required />
          </div>
          <div class="form-group">
            <input v-model="password_hash" type="password" class="login-input" placeholder="Mật khẩu" required />
          </div>
          <button class="login-button" type="submit">Đăng nhập</button>
        </form>
        <div class="extra-links">
          <p>Bạn chưa có tài khoản? <router-link to="/register">Đăng ký</router-link></p>
          <p><router-link to="/forgot-password">Quên mật khẩu?</router-link></p>
        </div>
      </div>
    </div>
  </template>
  
  <script>
 
  import { account } from "../../api/account";
  
  export default {
    name: "LoginComponent",
    data() {
      return {
        phonenumber: "",
        password_hash: "",
      };
    },
    methods: {
      async handleLogin() {
        try {
          const response = await account.login(this.phonenumber, this. password_hash);
          
          if (response.token) {
            localStorage.setItem("token", response.token);
            alert("Đăng nhập thành công!");
            this.$router.push(response.role === "Admin" ? "/admin-page" : "/user-acount");
          } else {
            alert("Đăng nhập thất bại!");
          }
        } catch (error) {
          alert("Sai tài khoản hoặc mật khẩu!");
        }
      },
    },
  };
  </script>
  
  <style scoped>
  .login-page {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #617d9b;
  }
  .login-container {
    background-color: #fff;
    padding: 40px;
    border-radius: 4px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    text-align: center;
    width: 400px;
    font-family: AvertaStdCY_Semibold, Helvetica, Arial, sans-serif;
  }
  .header-login {
  display: flex;
  align-items: center;
  justify-content: space-between;
 
  margin-bottom: 10px;
}

.logo-container {
  flex: 0 0 80px; /* Tăng kích thước logo */
}

.main-logo1 {
  width: 70px; /* Tăng kích thước logo */
  height: auto;
}

.main-title {
  font-size: 24px; /* Tăng kích thước chữ Đăng nhập */
  font-weight: bold;
  margin-right: 15px; /* Giữ khoảng cách giữa logo và chữ */
}

  
  .form-group {
    margin-bottom: 20px;
  }
  .login-input {
    width: 95%;
    padding: 8.5px;
    border: 1px solid #ccc;
    border-radius: 3px;
    outline: none;
    transition: border-color 0.1s ease-in-out, box-shadow 0.3s ease-in-out;
  }
.login-input:hover,
.login-input:focus {
  border-color: #66afe9; /* Màu xanh nhạt */
  box-shadow: 0 0 5px rgba(102, 175, 233, 0.5); /* Hiệu ứng ánh sáng */
  
}
  .login-button {
    width: 100%;
    padding: 10px;
    border: none;
    border-radius: 4px;
    background-color: #423c7b;
    color: white;
    cursor: pointer;
    font-size: 16px;
  }
  .extra-links {
    margin-top: 20px;
  }
  .extra-links a {
    color: #278f4b;
    text-decoration: none;
  }
  .extra-links a:hover {
    text-decoration: underline;
  }
  </style>
  