namespace SDVN.Models
{
    public class AuthResultDTO
    {
        public string Token { get; set; }  // Token JWT
        public string Role { get; set; }  // Vai trò người dùng
        public int UserId { get; set; }  // Mã người dùng
    }
}
