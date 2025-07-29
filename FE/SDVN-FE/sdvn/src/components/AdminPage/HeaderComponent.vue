<template>
   <header class="header">
    <div class="logo">SDVN</div>
    <div class="user-info">
        <div>
            <i class="fas fa-bars icon" ></i>
        </div>
    <div>
        <span class="user-name">admin</span>
        <span class="status">🟢 Online</span>
        <span class="settings-icon">⚙</span>
    </div>
        
      </div>
    </header>
  </template>
  <script>
  export default {
    name: "HeaderComponent",
    }
</script>
  
  <style scoped>
  .header {
    display: flex;
    
    align-items: center;
    background-color: #2a87bd;
    color: white;
   
  }
  
  .logo {
    font-size: 20px;
    font-weight: bold;
    padding: 10px 20px;
    width: 192px;
    display: flex;
    justify-content: center;
    background-color: #18618b;
  }
  
  .user-info {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 20px;
    width: 100%;
    
    justify-content: space-between;
  }
  
  .settings-icon {
    cursor: pointer;
  }
  </style>
  