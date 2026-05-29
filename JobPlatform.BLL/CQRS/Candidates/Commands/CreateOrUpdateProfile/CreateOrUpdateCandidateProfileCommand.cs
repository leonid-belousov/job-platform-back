using JobPlatform.BLL.CQRS.Candidates.DTO;
using JobPlatform.Core.Entities.Candidates;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Candidates.Commands.CreateOrUpdateProfile;

public sealed record CreateOrUpdateCandidateProfileCommand(
    string FirstName,
    string LastName,
    string? City,
    string? DesiredPosition,
    decimal? ExpectedSalary,
    string? About) : IRequest<CandidateProfileDto>
{
    public class
        CreateOrUpdateCandidateProfileCommandHandler : IRequestHandler<CreateOrUpdateCandidateProfileCommand,
        CandidateProfileDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public CreateOrUpdateCandidateProfileCommandHandler(IApplicationDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<CandidateProfileDto> Handle(CreateOrUpdateCandidateProfileCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
            var profile = await _dbContext.Set<CandidateProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (profile is null)
            {
                profile = new CandidateProfile()
                {
                    UserId = userId,
                };
                _dbContext.Set<CandidateProfile>().Add(profile);
            }

            profile.FirstName = request.FirstName;
            profile.LastName = request.LastName;
            profile.City = request.City;
            profile.DesiredPosition = request.DesiredPosition;
            profile.ExpectedSalary = request.ExpectedSalary;
            profile.About = request.About;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new CandidateProfileDto(profile.Id, profile.FirstName, profile.LastName, profile.City, profile.DesiredPosition, profile.ExpectedSalary);
        }
    }
}