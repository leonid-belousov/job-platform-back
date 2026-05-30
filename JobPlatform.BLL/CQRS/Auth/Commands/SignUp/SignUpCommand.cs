using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignUp;

public sealed record SignUpCommand(string Email, string Password, string RoleCode, string? IpAddress = null) : IRequest<AuthResponse>
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;

        public SignUpCommandHandler(IApplicationDbContext dbContext, IPasswordService passwordService,
            IJwtTokenService jwtTokenService, IAuditService auditService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        public async Task<AuthResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var roleCode = request.RoleCode.Trim().ToLowerInvariant();
            if (await _dbContext.Set<User>().AnyAsync(p => p.Email == email, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует");
            }

            var role = await _dbContext.Set<Role>()
                           .Include(x => x.RolePermissions)
                           .ThenInclude(x => x.Permission)
                           .FirstOrDefaultAsync(p => p.Code == roleCode, cancellationToken)
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
            user.RefreshTokens.Add(new Core.Entities.Users.RefreshToken()
            {
                User = user,
                TokenHash = _jwtTokenService.HashRefreshToken(refreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                CreatedByIp = request.IpAddress
            });

            await _dbContext.Set<User>().AddAsync(user, cancellationToken);
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthRegistered,
                EntityType: nameof(User),
                EntityId: user.Id,
                NewValue: new { user.Email, Role = role.Code },
                UserId: user.Id), cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            var permissions = role.RolePermissions.Select(x => x.Permission.Code).Distinct().ToArray();
            var accessToken = _jwtTokenService.CreateAccessToken(user, new[] { role.Code }, permissions);
            return new AuthResponse(accessToken, refreshToken, user.Id, user.Email);
        }
    }
}