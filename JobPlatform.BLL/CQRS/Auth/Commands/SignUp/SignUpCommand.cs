using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignUp;

public sealed record SignUpCommand(string Email, string Password, string RoleCode) : IRequest<AuthResponse>
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;

        public SignUpCommandHandler(IApplicationDbContext dbContext, IPasswordService passwordService,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _dbContext.Set<User>().AnyAsync(p => p.Email == email, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует");
            }

            var role = await _dbContext.Set<Role>()
                           .FirstOrDefaultAsync(p => p.Code == request.RoleCode, cancellationToken)
                       ?? throw new InvalidOperationException("Указанная роль не найдена");

            var user = new User()
            {
                Email = email,
                Status = UserStatus.Active,
                EmailConfirmed = false
            };
            user.PasswordHash = _passwordService.HashPassword(user, request.Password);
            user.UserRoles.Add(new UserRole()
            {
                User = user,
                Role = role
            });

            var refreshToken = _jwtTokenService.CreateRefreshToken();
            user.RefreshTokens.Add(new RefreshToken()
            {
                User = user,
                TokenHash = _jwtTokenService.HashRefreshToken(refreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            });

            await _dbContext.Set<User>().AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync();

            var accessToken = _jwtTokenService.CreateAccessToken(user, new[] { role.Code }, Array.Empty<string>());
            return new AuthResponse(accessToken, refreshToken, user.Id, user.Email);
        }
    }
}