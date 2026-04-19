using CaseyHub.API.Data;
using CaseyHub.Core.Entities;
using CaseyHub.Models.DTOs.Internal.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CaseyHub.API.Services;

public class AuthService(
    CaseyHubDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration
) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        string name = request.Name?.Trim() ?? string.Empty;
        string email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            return new AuthResultDto(false, "Name is required.", null);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return new AuthResultDto(false, "Email is required.", null);
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return new AuthResultDto(false, "Password must be at least 8 characters.", null);
        }

        bool emailExists = await dbContext.Users.AnyAsync(user => user.Email == email);
        if (emailExists)
        {
            return new AuthResultDto(false, "An account with that email already exists.", null);
        }

        var passwordUser = new User(name, email, "pending-hash");
        string passwordHash = passwordHasher.HashPassword(passwordUser, request.Password);
        var user = new User(name, email, passwordHash);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return new AuthResultDto(true, null, BuildAuthResponse(user));
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        string email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResultDto(false, "Email and password are required.", null);
        }

        User? user = await dbContext.Users.SingleOrDefaultAsync(existingUser => existingUser.Email == email);
        if (user == null)
        {
            return new AuthResultDto(false, "Invalid email or password.", null);
        }

        PasswordVerificationResult verificationResult =
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return new AuthResultDto(false, "Invalid email or password.", null);
        }

        return new AuthResultDto(true, null, BuildAuthResponse(user));
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        int expiryMinutes = configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto(
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            Token: tokenValue,
            ExpiresAtUtc: expiresAtUtc
        );
    }
}
