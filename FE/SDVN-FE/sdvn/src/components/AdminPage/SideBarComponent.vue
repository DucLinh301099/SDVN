<template>
  <div class="sidebar">
    <div class="search-bar">
      <input type="text" v-model="searchQuery" placeholder="Tìm kiếm..." />
      <i class="fa fa-search"></i>
    </div>
    <ul class="menu">
      <li>
        <router-link to="/admin"
          ><i class="fa fa-home"></i> Trang chủ</router-link
        >
      </li>
      <li>
        <router-link to="/"><i class="fa fa-cogs"></i> Hệ thống </router-link>
      </li>
      <li>
        <router-link to="/payment"><i class="fa fa-cubes"></i> QC</router-link>
      </li>
      <li>
        <router-link to="/withdraw-list"
          ><i class="fa fa-desktop"></i> IS
        </router-link>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  name: "SideBarComponent",
  data() {
    return {
      searchQuery: "",
    };
  },
};
</script>

<style scoped>
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css");

.sidebar {
  width: 203.5px;
  background-color: #ffffff;
  padding: 20px 0;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.sidebar .logo {
  display: flex;
  justify-content: center;
  align-items: center;
  padding-bottom: 18px;
}

.sidebar .logo img {
  width: 150px;
  height: auto;
  margin-top: 10px;
}

.sidebar .menu {
  list-style-type: none;
  padding: 0;
  margin: 0;
  flex-grow: 1;
}

.sidebar .menu li {
  margin: 0;
}

.sidebar .menu li a {
  display: flex;
  align-items: center;
  text-decoration: none;
  color: #333;
  font-size: 16px;
  padding: 18px 20px;
  border-left: 3px solid transparent;
  transition: background-color 0.3s ease, border-color 0.3s ease;
}

.sidebar .menu li a i {
  margin-right: 10px;
}

.sidebar .menu li a:hover,
.sidebar .menu li a.router-link-active {
  background-color: #f0f0f0;
  border-color: #007bff;
}
.search-bar {
  position: relative;
  display: flex;
  align-items: center;
  background: #f5f5f5;
  padding: 6px 10px;
  border-radius: 3px;
  margin-right: 10px;
  border: 1px solid #ccc;
}

.search-bar input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 14px;
  padding: 6px 10px;
  padding-right: 30px; /* Tạo khoảng trống để icon không bị chồng lên chữ */
  width: 100%;
}

.search-bar i {
  position: absolute;
  right: 15px;
  color: #777;
  font-size: 16px;
}
</style>
