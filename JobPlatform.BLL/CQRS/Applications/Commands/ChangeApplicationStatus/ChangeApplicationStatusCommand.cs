using JobPlatform.BLL.CQRS.Applications.DTO;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Commands.ChangeApplicationStatus;

public sealed record ChangeApplicationStatusCommand(Guid ApplicationId, string NewStatus, string? Comment)
    : IRequest<ApplicationDto>
{
    public class ChangeApplicationStatusCommandHandler : IRequestHandler<ChangeApplicationStatusCommand, ApplicationDto>
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Sent", "Viewed", "InProgress", "Interview", "Rejected", "Accepted", "Closed"
        };

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public ChangeApplicationStatusCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApplicationDto> Handle(ChangeApplicationStatusCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            if (!AllowedStatuses.Contains(request.NewStatus))
                throw new InvalidOperationException("Недопустимый статус отклика.");

            var application = await _db.Set<JobApplication>()
                                  .Include(x => x.Vacancy)
                                  .Include(x => x.CandidateProfile)
                                  .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted,
                                      cancellationToken)
                              ?? throw new InvalidOperationException("Отклик не найден.");
            

            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == application.Vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к откликам этой вакансии.");

            var oldStatus = application.Status;
            application.Status = request.NewStatus;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                JobApplicationId = application.Id,
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                ChangedByUserId = userId,
                Comment = request.Comment
            });

            await _db.SaveChangesAsync(cancellationToken);

            var candidateName =
                $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName}".Trim();

            return new ApplicationDto(application.Id, application.VacancyId, application.Vacancy.Title,
                application.CandidateProfileId, candidateName, application.ResumeId, application.Status,
                application.CoverLetter, application.CreatedAt);
        }
    }
}