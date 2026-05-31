using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Auth;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JobPlatform.BLL.CQRS.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email, string? IpAddress = null) : IRequest
{
    public sealed class Handler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;

        public Handler(IApplicationDbContext db, IEmailSender emailSender,
            IEmailTemplateRenderer emailTemplateRenderer, IAuditService auditService, IConfiguration configuration)
        {
            _db = db;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
            _auditService = auditService;
            _configuration = configuration;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Set<User>()
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);

            if (user is null || user.Status == UserStatus.Blocked)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var existingTokens = await _db.Set<UserAuthToken>()
                .Where(x => x.UserId == user.Id
                            && x.Type == UserAuthTokenTypes.PasswordReset
                            && x.UsedAt == null
                            && !x.IsDeleted)
                .ToArrayAsync(cancellationToken);
            foreach (var existingToken in existingTokens)
            {
                existingToken.UsedAt = now;
                existingToken.UpdatedAt = now;
            }

            var rawToken = AuthTokenHelper.CreateToken();
            var token = new UserAuthToken
            {
                UserId = user.Id,
                Type = UserAuthTokenTypes.PasswordReset,
                TokenHash = AuthTokenHelper.HashToken(rawToken),
                ExpiresAt = now.AddHours(2),
                CreatedByIp = request.IpAddress
            };
            await _db.Set<UserAuthToken>().AddAsync(token, cancellationToken);

            var resetUrl = BuildUrl("PasswordResetUrl", "reset-password", rawToken);
            var template = _emailTemplateRenderer.RenderPasswordReset(resetUrl);
            await _emailSender.SendAsync(user.Email, template.Subject, template.HtmlBody, template.TextBody, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthPasswordResetRequested,
                EntityType: nameof(User),
                EntityId: user.Id,
                NewValue: new { user.Email, token.ExpiresAt },
                UserId: user.Id), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }

        private string BuildUrl(string configurationKey, string path, string token)
        {
            var template = _configuration[$"Auth:{configurationKey}"];
            if (!string.IsNullOrWhiteSpace(template))
            {
                return template.Replace("{token}", Uri.EscapeDataString(token));
            }

            var baseUrl = _configuration["Auth:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
            return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
        }
    }
}
