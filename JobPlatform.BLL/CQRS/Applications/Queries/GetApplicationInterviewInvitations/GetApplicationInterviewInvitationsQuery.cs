using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.GetApplicationInterviewInvitations;

public sealed record GetApplicationInterviewInvitationsQuery(Guid ApplicationId) : IRequest<IReadOnlyCollection<InterviewInvitationDto>>
{
    public sealed class Handler : IRequestHandler<GetApplicationInterviewInvitationsQuery, IReadOnlyCollection<InterviewInvitationDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<InterviewInvitationDto>> Handle(GetApplicationInterviewInvitationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var application = await _db.Set<JobApplication>()
                                  .AsNoTracking()
                                  .Include(x => x.Vacancy)
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new InvalidOperationException("Application not found.");

            var isCompanyMember = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            var isCandidateOwner = await _db.Set<CandidateProfile>()
                .AnyAsync(x => x.Id == application.CandidateProfileId && x.UserId == userId && !x.IsDeleted,
                    cancellationToken);

            if (!isCompanyMember && !isCandidateOwner)
            {
                throw new UnauthorizedAccessException();
            }

            return await _db.Set<InterviewInvitation>()
                .AsNoTracking()
                .Where(x => x.ApplicationId == request.ApplicationId && !x.IsDeleted)
                .OrderByDescending(x => x.ScheduledAt)
                .Select(x => new InterviewInvitationDto(x.Id, x.ApplicationId, x.ScheduledAt, x.Format, x.Location,
                    x.MeetingUrl, x.Message, x.Status, x.CreatedByUserId, x.CandidateRespondedAt, x.CreatedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}