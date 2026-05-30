using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignOut;

public sealed record SignOutCommand(string RefreshToken) : IRequest
{
    public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;

        public SignOutCommandHandler(IApplicationDbContext db, IJwtTokenService jwtTokenService, IAuditService auditService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
            
            var refreshToken = await _db.Set<Core.Entities.Users.RefreshToken>()
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
            
            if (refreshToken is null)
                return;

            if (refreshToken.RevokedAt is null)
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthLogout,
                EntityType: "RefreshToken",
                EntityId: refreshToken.Id,
                NewValue: new { refreshToken.UserId },
                UserId: refreshToken.UserId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}