using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Users.Commands.UnblockUser;

public sealed record UnblockUserCommand(Guid UserId) : IRequest
{
    public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand>
    {
        private readonly IApplicationDbContext _db;

        public UnblockUserCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Handle(UnblockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Set<User>()
                           .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Пользователь не найден.");

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}