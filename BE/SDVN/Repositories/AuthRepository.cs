using Dapper;
using SDVN.Models;
using SDVN.Models.ViewModel;
using System.Data;
using System.Threading.Tasks;

namespace SDVN.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnection _dbConnection;

        public AuthRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Kiểm tra tài khoản và mật khẩu
        public async Task<bool> AuthenticateUserAsync(LoginRequestVM request)
        {
            var query = "SELECT * FROM users WHERE phonenumber = @PhoneNumber AND password_hash = @PasswordHash";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<User>(query, new
            {
                PhoneNumber = request.PhoneNumber,
                PasswordHash = request.Password // Mật khẩu đã mã hóa
            });

            return result != null;
        }

        // Lấy vai trò của người dùng
        public async Task<UserRole> GetUserRoleAsync(string phoneNumber)
        {
            var query = "SELECT role FROM users WHERE phonenumber = @PhoneNumber";
            var role = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { PhoneNumber = phoneNumber });

            if (role == 0)
                return new UserRole { RoleName = "User" };
            if (role == 1)
                return new UserRole { RoleName = "Admin" };

            return null;
        }

        // Đăng ký người dùng mới
        public async Task<int> RegisterUserAsync(User user)
        {
            var query = "INSERT INTO users (name, email, phonenumber, password_hash, role) VALUES (@Name, @Email, @PhoneNumber, @PasswordHash, @Role) RETURNING id";
            var userId = await _dbConnection.ExecuteScalarAsync<int>(query, user);
            return userId;
        }

        // Lấy thông tin người dùng theo số điện thoại
        public async Task<User> GetUserByPhoneAsync(string phoneNumber)
        {
            var query = "SELECT * FROM users WHERE phonenumber = @PhoneNumber";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, new { PhoneNumber = phoneNumber });
        }
    }
}
