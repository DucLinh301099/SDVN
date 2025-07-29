using SDVN.Models;
using SDVN.Models.ViewModel;
using System.Threading.Tasks;

namespace SDVN.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> AuthenticateUserAsync(LoginRequestVM request); // Xác thực người dùng
        Task<UserRole> GetUserRoleAsync(string phoneNumber); // Lấy vai trò người dùng
        Task<int> RegisterUserAsync(User user); // Đăng ký người dùng mới
        Task<User> GetUserByPhoneAsync(string phoneNumber); // Lấy người dùng theo số điện thoại
    }
}
