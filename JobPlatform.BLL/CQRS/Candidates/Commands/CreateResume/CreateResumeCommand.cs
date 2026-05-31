using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateResume;

public sealed record CreateResumeCommand(string Title, Guid FileId, bool IsActive) : IRequest<ResumeDto>
{
    public class CreateResumeCommandHandler : IRequestHandler<CreateResumeCommand, ResumeDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        public CreateResumeCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<ResumeDto> Handle(CreateResumeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var profile =
                await _db.Set<CandidateProfile>().FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted,
                    cancellationToken)
                ?? throw new InvalidOperationException("Сначала необходимо создать профиль кандидата.");

            var currentActiveResumes = await _db.Set<Resume>()
                .Where(x => x.CandidateProfileId == profile.Id && x.IsActive && !x.IsDeleted)
                .ToArrayAsync(cancellationToken);

            foreach (var resumeItem in currentActiveResumes)
            {
                resumeItem.IsActive = false;
                resumeItem.IsDefault = false;
                resumeItem.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var resume = new Resume
            {
                CandidateProfileId = profile.Id,
                Title = request.Title.Trim(),
                FileId = request.FileId,
                Status = "Published",
                IsActive = true,
                IsDefault = true
            };

            await _db.Set<Resume>().AddAsync(resume, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ResumeCreated,
                EntityType: nameof(Resume),
                EntityId: resume.Id,
                NewValue: new { resume.CandidateProfileId, resume.Title, resume.FileId, resume.Status, resume.IsActive, resume.IsDefault },
                UserId: userId), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);

            return new ResumeDto(resume.Id, resume.CandidateProfileId, resume.Title, resume.FileId, resume.Status,
                resume.IsDefault, resume.CreatedAt);
        }
    }
}