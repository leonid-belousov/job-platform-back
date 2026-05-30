using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.CancelInterviewInvitation;

public sealed record CancelInterviewInvitationCommand(Guid InvitationId) : IRequest<InterviewInvitationDto>
{
    public sealed class Handler : IRequestHandler<CancelInterviewInvitationCommand, InterviewInvitationDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<InterviewInvitationDto> Handle(CancelInterviewInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var invitation = await _db.Set<InterviewInvitation>()
                                 .Include(x => x.Application)
                                 .ThenInclude(x => x.Vacancy)
                                 .FirstOrDefaultAsync(x => x.Id == request.InvitationId && !x.IsDeleted,
                                     cancellationToken)
                             ?? throw new InvalidOperationException("Interview invitation not found.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == invitation.Application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            invitation.Status = InterviewInvitationStatuses.Cancelled;
            invitation.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            return new InterviewInvitationDto(invitation.Id, invitation.ApplicationId, invitation.ScheduledAt,
                invitation.Format, invitation.Location, invitation.MeetingUrl, invitation.Message, invitation.Status,
                invitation.CreatedByUserId, invitation.CandidateRespondedAt, invitation.CreatedAt);
        }
    }
}