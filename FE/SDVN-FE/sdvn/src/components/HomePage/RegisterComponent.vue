<template>
    <div class="register-page">
      <div class="register-container">
        
     <div class="header-login">
      <div class="logo-container">
        <router-link to="/">
          <img src="D:\VueJs\SDVN\SDVN-FE\sdvn\src\assets\logo_sdvn.jpg" alt="SDVN Logo" class="main-logo1" />
        </router-link>
      </div>
      <div class="main-title">
       <span class="bold">Đăng Ký</span>
       </div>
      </div>
        <form @submit.prevent="handleSubmit">
          <div class="form-group-r">            
              <input
                type="text"
                v-model="name"
                class="register-input"
                placeholder="Tên"
              />           
          </div>
          <div class="form-group-r">
            <input
              type="text"
              v-model="email"
              class="register-input"
              placeholder="Email"
            />
          </div>
          <div class="form-group-r">
            <input
              type="text"
              v-model="phonenumber"
              class="register-input"
              placeholder="Số điện thoại"
            />
          </div>
          <div class="form-group-r">
            <input
              type="password"
              v-model="password_hash"
              class="register-input"
              placeholder="Mật khẩu"
            />
          </div>
          
          <button type="submit" class="register-button">Đăng ký</button>
  
          <div class="extra-links">
            <p>
              Bạn đã có tài khoản?
              <router-link to="/login">Đăng Nhập</router-link>
            </p>
            <p><router-link to="/href">Trợ giúp</router-link></p>
          </div>
        </form>
      </div>
    </div>
  </template>
  
  <script>
  
  import { account } from "../../api/account";
  export default {
    name: "RegisterComponent",
    data() {
      return {
        name: "",
        email: "",
        phonenumber: "",
        password_hash: "",
      };
    },
    methods: {
      async handleSubmit() {
        try {
          const response = await account.register(
            this.name,
            this.email,
            this.phonenumber,
            this.password_hash,
          );
  
          if (response.userId) {
            alert("Đăng ký thành công!");
            this.$router.push("/login");
          }
        } catch (error) {
          alert("Đăng ký thất bại. Vui lòng thử lại.");
        }
      },
    },
  };
  </script>
  
  <style scoped>
  .register-page {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #617d9b;
  }
  
  .register-container {
    background-color: #fff;
    padding: 20px 40px 40px 40px;
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
.form-group-r {
    margin-bottom: 20px;
  }
  .register-input {
    flex: 1;
    padding: 8px;
    border: 1px solid #ccc;
    border-radius: 3px;
    width: 96%;
    outline: none;
    transition: border-color 0.1s ease-in-out, box-shadow 0.3s ease-in-out;
  }
  .register-input:hover,
.register-input:focus {
  border-color: #66afe9; /* Màu xanh nhạt */
  box-shadow: 0 0 5px rgba(102, 175, 233, 0.5); /* Hiệu ứng ánh sáng */
  
}
  .register-button {
    width: 100%;
    padding: 10px;
    border: none;
    border-radius: 4px;
    background-color: #423c7b;
    color: white;
    cursor: pointer;
    font-size: 16px;
  }
  .disclaimer {
    font-size: 12px;
    color: #666;
    margin-bottom: 20px;
  }
  .extra-links a {
    color: #278f4b;
    text-decoration: none;
  }
  .extra-links a:hover {
    text-decoration: underline;
  }
  .extra-links {
    margin-top: 20px;
  }
  </style>
  