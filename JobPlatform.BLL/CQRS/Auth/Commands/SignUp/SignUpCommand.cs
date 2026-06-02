using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Auth;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Legal;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JobPlatform.BLL.CQRS.Auth.Commands.SignUp;

public sealed record SignUpCommand(
    string Email,
    string Password,
    string RoleCode,
    bool AcceptTerms,
    string TermsVersion,
    bool AcceptPrivacyPolicy,
    string PrivacyPolicyVersion,
    string Language = "ru",
    string? IpAddress = null) : IRequest<AuthResponse>
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly IConfiguration _configuration;

        public SignUpCommandHandler(IApplicationDbContext dbContext, IPasswordService passwordService,
            IJwtTokenService jwtTokenService, IAuditService auditService, IEmailSender emailSender,
            IEmailTemplateRenderer emailTemplateRenderer, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var roleCode = request.RoleCode.Trim().ToLowerInvariant();
            var language = string.IsNullOrWhiteSpace(request.Language) ? "ru" : request.Language.Trim().ToLowerInvariant();
            var termsVersion = request.TermsVersion.Trim();
            var privacyPolicyVersion = request.PrivacyPolicyVersion.Trim();

            if (!request.AcceptTerms || !request.AcceptPrivacyPolicy)
            {
                throw new InvalidOperationException("Terms and privacy policy must be accepted.");
            }

            await EnsureActiveLegalDocumentAsync("terms", termsVersion, language, cancellationToken);
            await EnsureActiveLegalDocumentAsync("privacy_policy", privacyPolicyVersion, language, cancellationToken);

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
                Status = UserStatus.PendingConfirmation,
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

            var rawConfirmationToken = AuthTokenHelper.CreateToken();
            user.AuthTokens.Add(new UserAuthToken
            {
                User = user,
                Type = UserAuthTokenTypes.EmailConfirmation,
                TokenHash = AuthTokenHelper.HashToken(rawConfirmationToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
                CreatedByIp = request.IpAddress
            });

            await _dbContext.Set<User>().AddAsync(user, cancellationToken);

            await _dbContext.Set<UserLegalConsent>().AddRangeAsync(new[]
            {
                new UserLegalConsent
                {
                    User = user,
                    DocumentType = "terms",
                    Version = termsVersion,
                    Language = language,
                    AcceptedAt = DateTimeOffset.UtcNow,
                    IpAddress = request.IpAddress
                },
                new UserLegalConsent
                {
                    User = user,
                    DocumentType = "privacy_policy",
                    Version = privacyPolicyVersion,
                    Language = language,
                    AcceptedAt = DateTimeOffset.UtcNow,
                    IpAddress = request.IpAddress
                }
            }, cancellationToken);
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthRegistered,
                EntityType: nameof(User),
                EntityId: user.Id,
                NewValue: new
                {
                    user.Email,
                    Role = role.Code,
                    user.EmailConfirmed,
                    user.Status,
                    LegalConsents = new[] { "terms", "privacy_policy" }
                },
                UserId: user.Id), cancellationToken);

            var confirmationUrl = BuildUrl("EmailConfirmationUrl", "auth/confirm-email", rawConfirmationToken, request.Email);
            var emailTemplate = _emailTemplateRenderer.RenderEmailConfirmation(confirmationUrl);
            await _emailSender.SendAsync(user.Email, emailTemplate.Subject, emailTemplate.HtmlBody,
                emailTemplate.TextBody, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthEmailConfirmationSent,
                EntityType: nameof(User),
                EntityId: user.Id,
                NewValue: new { user.Email },
                UserId: user.Id), cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            var permissions = role.RolePermissions.Select(x => x.Permission.Code).Distinct().ToArray();
            var accessToken = _jwtTokenService.CreateAccessToken(user, new[] { role.Code }, permissions);
            return new AuthResponse(accessToken, refreshToken, user.Id, user.Email);
        }

        private async Task EnsureActiveLegalDocumentAsync(string type, string version, string language,
            CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Set<LegalDocument>().AnyAsync(x =>
                x.Type == type && x.Version == version && x.Language == language && x.IsActive && !x.IsDeleted,
                cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException($"Active legal document '{type}' version '{version}' was not found.");
            }
        }

        private string BuildUrl(string configurationKey, string path, string token, string email)
        {
            var template = _configuration[$"Auth:{configurationKey}"];
            if (!string.IsNullOrWhiteSpace(template))
            {
                return template.Replace("{token}", Uri.EscapeDataString(token));
            }

            var baseUrl = _configuration["Auth:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
            return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
        }
    }
}