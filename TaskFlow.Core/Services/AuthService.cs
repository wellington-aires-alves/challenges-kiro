using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Validar campos obrigatórios
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ValidationException("Nome de usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("E-mail é obrigatório.");

        // Validar formato de e-mail (deve conter @ e .)
        if (!IsValidEmail(request.Email))
            throw new ValidationException("E-mail inválido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Senha é obrigatória.");

        if (request.Password.Length < 6)
            throw new ValidationException("A senha deve ter pelo menos 6 caracteres.");

        // Verificar unicidade de e-mail
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new ConflictException("E-mail já cadastrado.");

        // Verificar unicidade de username
        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new ConflictException("Nome de usuário já cadastrado.");

        // Fazer hash da senha com BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Criar e persistir o usuário
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Gerar JWT token após registro bem-sucedido
        var token = GenerateJwtToken(user);

        return new AuthResult(true, token, user.Username, null);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        // Validar campos obrigatórios
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("E-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Senha é obrigatória.");

        // Buscar usuário por e-mail
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            throw new UnauthorizedException("Credenciais inválidas.");

        // Verificar hash da senha
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Credenciais inválidas.");

        // Gerar JWT
        var token = GenerateJwtToken(user);

        return new AuthResult(true, token, user.Username, null);
    }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key not configured.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: "TaskFlow",
            audience: "TaskFlow",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool IsValidEmail(string email)
    {
        // Validação básica: deve conter @ e .
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return false;

        var dotIndex = email.LastIndexOf('.');
        return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
    }
}
