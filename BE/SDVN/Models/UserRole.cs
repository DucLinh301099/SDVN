namespace SDVN.Models
{
    public class UserRole
    {
        public int UserId { get; set; }  // Mã người dùng
        public string RoleName { get; set; }  // Tên vai trò người dùng (User hoặc Admin)
    }
}
