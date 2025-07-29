using SDVN.Models;
using SDVN.Models.ViewModel;
using System.Threading.Tasks;

namespace SDVN.Services
{
    public interface IAuthService
    {
        Task<AuthResultDTO> AuthenticateUserAsync(LoginRequestVM request); // Đăng nhập
        Task<int> RegisterAsync(RegisterRequestVM registerRequest); // Đăng ký
    }
}
