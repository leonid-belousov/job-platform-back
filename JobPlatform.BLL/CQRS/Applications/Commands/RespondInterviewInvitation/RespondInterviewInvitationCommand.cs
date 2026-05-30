using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.RespondInterviewInvitation;

public sealed record RespondInterviewInvitationCommand(Guid InvitationId, string ResponseStatus)
    : IRequest<InterviewInvitationDto>
{
    public sealed class Handler : IRequestHandler<RespondInterviewInvitationCommand, InterviewInvitationDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<InterviewInvitationDto> Handle(RespondInterviewInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var responseStatus = request.ResponseStatus.Trim().ToLowerInvariant();

            if (responseStatus != InterviewInvitationStatuses.Accepted && responseStatus != InterviewInvitationStatuses.Declined)
            {
                throw new InvalidOperationException("Invalid interview response.");
            }

            var invitation = await _db.Set<InterviewInvitation>()
                                 .Include(x => x.Application)
                                 .FirstOrDefaultAsync(x => x.Id == request.InvitationId && !x.IsDeleted,
                                     cancellationToken)
                             ?? throw new InvalidOperationException("Interview invitation not found.");

            var candidate = await _db.Set<CandidateProfile>().AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == invitation.Application.CandidateProfileId && x.UserId == userId && !x.IsDeleted,
                                    cancellationToken)
                            ?? throw new UnauthorizedAccessException();

            invitation.Status = responseStatus;
            invitation.CandidateRespondedAt = DateTimeOffset.UtcNow;
            invitation.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return new InterviewInvitationDto(invitation.Id, invitation.ApplicationId, invitation.ScheduledAt,
                invitation.Format, invitation.Location, invitation.MeetingUrl, invitation.Message, invitation.Status,
                invitation.CreatedByUserId, invitation.CandidateRespondedAt, invitation.CreatedAt);
        }
    }
}