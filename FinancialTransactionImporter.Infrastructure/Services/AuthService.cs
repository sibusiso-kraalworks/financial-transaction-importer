using FinancialTransactionImporter.Core.Configurations.Jwts;
using FinancialTransactionImporter.Core.Interfaces.Authentications;
using FinancialTransactionImporter.Core.Models;
using FinancialTransactionImporter.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace FinancialTransactionImporter.Infrastructure.Services
{
    /// <summary>
    /// Provides authentication services including user registration, login, and JWT token generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthService with database context, JWT settings, and logger.
        /// </summary>
        public AuthService(
            ApplicationDbContext context,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with the provided email and password.
        /// Returns an AuthResult containing registration status and JWT token if successful.
        /// </summary>
        public async Task<AuthResult> RegisterAsync(string email, string password)
        {
            var result = new AuthResult();

            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    result.Message = "User already exists";
                    return result;
                }

                // Create new user
                var user = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Generate token
                result.Success = true;
                result.Token = GenerateJwtToken(user);
                result.Email = user.Email;
                result.Role = user.Role;
                result.Message = "Registration successful";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                result.Message = "Registration failed";
            }

            return result;
        }

        /// <summary>
        /// Authenticates a user with the provided email and password.
        /// Returns an AuthResult containing login status and JWT token if successful.
        /// </summary>
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var result = new AuthResult();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    result.Message = "Invalid email or password";
                    return result;
                }

                result.Success = true;
                result.Token = GenerateJwtToken(user);
                result.Email = user.Email;
                result.Role = user.Role;
                result.Message = "Login successful";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                result.Message = "Login failed";
            }

            return result;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <returns>JWT token string.</returns>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
