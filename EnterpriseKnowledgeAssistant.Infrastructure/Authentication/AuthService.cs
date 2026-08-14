using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            IApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            email = email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.");

            if (password.Length < 8)
                throw new ArgumentException(
                    "Password must be at least 8 characters.");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "A user with this email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            _context.AddUser(user);
            await _context.SaveChangesAsync(cancellationToken);

            return GenerateToken(user);
        }

        public async Task<string> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            email = email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");

            var passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    password,
                    user.PasswordHash);

            if (!passwordValid)
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var jwtSettings =
                _configuration.GetSection("Jwt");

            var key = jwtSettings["Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is not configured.");

            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");

            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured.");

            var expirationMinutes =
                int.TryParse(
                    jwtSettings["ExpirationMinutes"],
                    out var minutes)
                    ? minutes
                    : 60;

            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email)
            };

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}