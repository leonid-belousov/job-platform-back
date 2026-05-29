using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateResume;

public sealed record CreateResumeCommand(string Title, Guid? FileId, bool IsDefault) : IRequest<ResumeDto>
{
    public class CreateResumeCommandHandler : IRequestHandler<CreateResumeCommand, ResumeDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public CreateResumeCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ResumeDto> Handle(CreateResumeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var profile =
                await _db.Set<CandidateProfile>().FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted,
                    cancellationToken)
                ?? throw new InvalidOperationException("Сначала необходимо создать профиль кандидата.");

            if (request.IsDefault)
            {
                var currentDefaults = await _db.Set<Resume>()
                    .Where(x => x.CandidateProfileId == profile.Id && x.IsDefault)
                    .ToArrayAsync(cancellationToken);

                foreach (var resumeItem in currentDefaults)
                {
                    resumeItem.IsDefault = false;
                    resumeItem.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            var resume = new Resume
            {
                CandidateProfileId = profile.Id,
                Title = request.Title.Trim(),
                FileId = request.FileId,
                Status = "Published",
                IsDefault = request.IsDefault
            };

            _db.Set<Resume>().Add(resume);

            await _db.SaveChangesAsync(cancellationToken);

            return new ResumeDto(resume.Id, resume.CandidateProfileId, resume.Title, resume.FileId, resume.Status,
                resume.IsDefault, resume.CreatedAt);
        }
    }
}