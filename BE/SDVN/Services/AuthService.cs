using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SDVN.Models;
using SDVN.Models.ViewModel;
using SDVN.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDVN.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Đăng nhập và trả về token JWT
        public async Task<AuthResultDTO> AuthenticateUserAsync(LoginRequestVM request)
        {
            if (request != null)
            {
                request.Password = GetMd5Hash(request.Password); // Mã hóa mật khẩu MD5

                if (await _authRepository.AuthenticateUserAsync(request))
                {
                    var userRole = await _authRepository.GetUserRoleAsync(request.PhoneNumber);
                    if (userRole == null)
                    {
                        throw new Exception("Role không tìm thấy của người dùng này");
                    }

                    var jwtTokenHandler = new JwtSecurityTokenHandler();
                    var secretKey = _configuration["Jwt:SecretKey"];
                    var tokenExpiryInHours = _configuration.GetValue<int>("Jwt:TokenExpiryInHours");
                    var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

                    var tokenDescription = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, request.PhoneNumber),
                            new Claim(ClaimTypes.Role, userRole.RoleName)
                        }),
                        Expires = DateTime.UtcNow.AddHours(tokenExpiryInHours),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256)
                    };

                    var token = jwtTokenHandler.CreateToken(tokenDescription);
                    var tokenString = jwtTokenHandler.WriteToken(token);

                    var cookieOptions = new CookieOptions
                    {
                        //HttpOnly = true,
                        // Secure = true, // Đảm bảo cookie chỉ được gửi qua HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(tokenExpiryInHours)
                    };
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("AuthToken", tokenString, cookieOptions);

                    return new AuthResultDTO
                    {
                        Token = tokenString,
                        Role = userRole.RoleName
                    };
                }
            }

            return null;
        }

        // Đăng ký người dùng mới
        public async Task<int> RegisterAsync(RegisterRequestVM registerRequest)
        {
            var existingUser = await _authRepository.GetUserByPhoneAsync(registerRequest.PhoneNumber);
            if (existingUser != null) return -1;

            var user = new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                PasswordHash = GetMd5Hash(registerRequest.Password), // Mã hóa mật khẩu MD5
                Role = 0 // Role user mặc định
            };

            return await _authRepository.RegisterUserAsync(user);
        }

        // Mã hóa mật khẩu thành MD5
        private string GetMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
