using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignIn;

public sealed record SignInCommand(string Email, string Password) : IRequest<AuthResponse>
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;

        public SignInCommandHandler(IApplicationDbContext dbContext, IPasswordService passwordService,
            IJwtTokenService jwtTokenService, IAuditService auditService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        public async Task<AuthResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();
            var user = await _dbContext.Set<User>()
                           .Include(p => p.UserRoles)
                           .ThenInclude(p => p.Role)
                           .ThenInclude(p => p.RolePermissions)
                           .ThenInclude(p => p.Permission)
                           .FirstOrDefaultAsync(p => p.Email == email)
                       ?? throw new UnauthorizedAccessException("Неверный email или пароль");

            if (user.Status == UserStatus.Blocked)
            {
                throw new UnauthorizedAccessException("Пользователь заблокирован");
            }

            if (!_passwordService.VerifyPassword(user, request.Password))
            {
                throw new UnauthorizedAccessException("Неверный email или пароль");
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            var refreshToken = _jwtTokenService.CreateRefreshToken();
            await _dbContext.Set<Core.Entities.Users.RefreshToken>().AddAsync(new Core.Entities.Users.RefreshToken
            {
                UserId = user.Id,
                TokenHash = _jwtTokenService.HashRefreshToken(refreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            }, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthLoginSucceeded,
                EntityType: nameof(User),
                EntityId: user.Id,
                NewValue: new { user.Email },
                UserId: user.Id), cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            var roles = user.UserRoles.Select(p => p.Role.Code).ToArray();
            var permissions = user.UserRoles
                .SelectMany(p => p.Role.RolePermissions)
                .Select(p => p.Permission.Code)
                .Distinct().ToArray();

            var accessToken = _jwtTokenService.CreateAccessToken(user, roles, permissions);
            return new AuthResponse(accessToken, refreshToken, user.Id, user.Email);
        }
    }
}