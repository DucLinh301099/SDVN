<template>
  <div class="admin-page">
    <Header />
    <div class="main-container">
      <Sidebar />
      <div class="content">
        <div class="main-content">
          <h2 class="title">Danh mục nhóm quyền</h2>
          <div class="search-content">
            <div class="title-search-1">
              <div class="title-search">
                <div class="icon">
                  <i class="fas fa-bars icon"></i>
                </div>
                <div class="title_1_2">
                  <p class="title_1">Tìm kiếm</p>
                </div>
              </div>
            </div>

            <div class="input-search">
              <div class="input-search-t">
                <div class="input-search-l">
                  <p>Tên nhóm quyền</p>
                  <input
                    type="text"
                    v-model="groupRole"
                    placeholder="Nhập nhóm quyền"
                    class="input-field"
                  />
                </div>
                <div class="input-search-r">
                  <p>Ghi chú</p>
                  <input
                    type="text"
                    v-model="note"
                    placeholder="Nhập ghi chú"
                    class="input-field"
                  />
                </div>
              </div>
            </div>
            <div>
              <button class="search-button" type="submit">Tìm Kiếm</button>
            </div>
          </div>
          <div class="grid-content">
            <div class="grid-01">
              <div class="grid-container">
                <div class="tile-grid">
                  <div class="title-search-1">
                    <div class="title-search-t">
                      <div class="icon">
                        <i class="fas fa-bars icon"></i>
                      </div>
                      <div class="title_1_2">
                        <p class="title_1">Danh sách nhóm quyền</p>
                      </div>
                    </div>
                    <div class="button-add">
                      <button class="search-button" type="submit">
                        Thêm mới
                      </button>
                    </div>
                  </div>
                </div>
                <div class="grid-content-t">
                  <div class="role-table-container">
                    <table class="role-table">
                      <thead>
                        <tr>
                          <th>STT</th>
                          <th>Tên nhóm quyền</th>
                          <th>Ghi chú</th>
                          <th>Hành động</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="(role, index) in roles" :key="role.id">
                          <td>{{ index + 1 }}</td>
                          <td>{{ role.name }}</td>
                          <td>{{ role.note }}</td>
                          <td>
                            <button @click="editItem">
                              <i class="fa fa-pencil" aria-hidden="true"></i>
                            </button>
                            <button @click="deleteItem">
                              <i class="fa fa-trash" aria-hidden="true"></i>
                            </button>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
                <div class="pagading-grid">
                  <div class="pagination">
                    <span>Số bản ghi: {{ roles.length }}</span>
                    <span class="pagination-span"
                      >Số trang: {{ totalPages }}</span
                    >
                    <span>
                      Trang:
                      <input
                        type="number"
                        v-model="currentPage"
                        min="1"
                        :max="totalPages"
                        @change="handlePageChange"
                        class="page-input"
                      />
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <slot></slot>
        <!-- Nội dung trang hiển thị ở đây -->
      </div>
    </div>
  </div>
</template>

<script>
import Header from "../components/AdminPage/HeaderComponent.vue";
import Sidebar from "../components/AdminPage/SideBarComponent.vue";

export default {
  components: {
    Header,
    Sidebar,
  },
  data() {
    return {
      roles: [
        { id: 1, name: "ROLE_ADMIN", note: "" },
        { id: 2, name: "ROLE_QC", note: "FOR QC DEPARTMENT" },
        { id: 3, name: "ROLE_ISS", note: "FOR ISS DEPARTMENT" },
        { id: 4, name: "ROLE_TEMP", note: "DO NOT DELETE" },
        { id: 5, name: "ROLE_ACC", note: "" },
      ],
      currentPage: 1, // Trang hiện tại
      itemsPerPage: 5, // Số bản ghi mỗi trang
    };
  },
  computed: {
    // Tính toán số trang tổng cộng
    totalPages() {
      return Math.ceil(this.roles.length / this.itemsPerPage);
    },
    // Lấy danh sách bản ghi cho trang hiện tại
    paginatedRoles() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      return this.roles.slice(start, end);
    },
  },
  methods: {
    // Xử lý khi người dùng thay đổi trang
    handlePageChange() {
      if (this.currentPage < 1) this.currentPage = 1;
      if (this.currentPage > this.totalPages)
        this.currentPage = this.totalPages;
    },
  },
};
</script>

<style scoped>
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css");
.admin-page {
  display: flex;
  flex-direction: column;
  height: 100vh;
  font-family: AvertaStdCY_Semibold, Helvetica, Arial, sans-serif;
}

.main-container {
  display: flex;
  flex: 1;
}
.main-content {
  margin-right: 5px;
}

.content {
  flex: 1;
  padding-left: 20px;
  background: #e9e8e8;
}
.title {
  font-size: 26;
  font-weight: 300;
}
.title_1 {
  font-weight: 300;
  margin-left: 5px;
  margin-top: 5px;
}
.title_1_2 {
  display: flex;
  align-items: center;
}
.title-search {
  display: flex;
  align-items: center;
  margin-left: 15px;
  height: 32px;
}
.title-search-t {
  display: flex;
  align-items: center;
  margin-left: 15px;
  height: 40px;
}
.title-search-1 {
  border-bottom: 1px solid #c7cdd0;
  display: flex;
  justify-content: space-between;
}
.button-add {
  height: 38px;
  display: flex;
  align-items: center;
  margin-right: 16px;
  margin-top: 2px;
}
.search-content {
  background-color: #f5f5f5;
  border-radius: 3px;
  border: 1px solid #c7cdd0;
}
.input-search {
  margin-left: 15px;
  display: flex;
}
.input-search-t {
  display: flex;
  margin-top: 20px;
}
.input-search-r {
  margin-left: 30px;
}
.input-field {
  width: 300px;
  padding: 6px;
  border: 1px solid #ccc;
  border-radius: 2px;
  outline: none;
  transition: border-color 0.1s ease-in-out, box-shadow 0.3s ease-in-out;
}
p {
  margin-bottom: 4px;
  font-size: 15px;
  font-weight: bold;
  margin-top: 0px;
}
.search-button {
  margin-top: 15px;
  margin-left: 15px;
  margin-bottom: 15px;
  height: 26px;
  width: 90px;
  background-color: #2a87bd;
  border-radius: 1px;
  border: none;
  cursor: pointer;
  color: #f5f5f5;
}
.grid-content {
  background-color: #f5f5f5;
  border-radius: 3px;
  border: 1px solid #c7cdd0;
  margin-top: 23px;
}
.role-table-container {
  padding: 16px;
  background-color: #f9f9f9;

  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

h1 {
  font-size: 24px;
  color: #333;
  margin-bottom: 20px;
}

.role-table {
  width: 100%;
  border-collapse: collapse;
  background-color: #fff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.role-table th,
.role-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.role-table th {
  background-color: #2a87bd;
  color: white;
  font-weight: bold;
}
/* CSS cho button trong cột Hành động */
.role-table td button {
  background: none; /* Xóa nền */
  border: none; /* Xóa viền */
  cursor: pointer; /* Biểu tượng trỏ chuột */
  font-size: 18px; /* Tăng kích thước icon */
  margin: 0 5px; /* Khoảng cách giữa hai icon */
  transition: transform 0.2s ease-in-out, color 0.2s;
}

/* Hiệu ứng hover cho nút Edit */
.role-table td button .fa-pencil {
  color: #3498db; /* Màu xanh */
}

.role-table td button:hover .fa-pencil {
  color: #1d6fa5; /* Đậm hơn khi hover */
  transform: scale(1.2); /* Phóng to nhẹ */
}

/* Hiệu ứng hover cho nút Delete */
.role-table td button .fa-trash {
  color: #e74c3c; /* Màu đỏ */
}

.role-table td button:hover .fa-trash {
  color: #c0392b; /* Đậm hơn khi hover */
  transform: scale(1.2); /* Phóng to nhẹ */
}

.role-table tbody tr:hover {
  background-color: #f1f1f1;
}

.action-check {
  color: green;
  font-weight: bold;
}

.action-cross {
  color: red;
  font-weight: bold;
}

.pagination {
  margin-top: 10px;
  display: flex;
  align-items: center;
  gap: 20px;
  font-size: 14px;
  margin-bottom: 10px;
  margin-left: 28px;
}
.pagination-span {
  margin-left: 30px;
  margin-right: 30px;
}
.page-input {
  width: 50px;
  padding: 5px;
  border: 1px solid #ccc;
  border-radius: 4px;
  text-align: center;
}

.page-input:focus {
  border-color: #004a99;
  outline: none;
}
</style>
