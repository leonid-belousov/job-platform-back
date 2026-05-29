using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Users.Commands.BlockUser;

public sealed record BlockUserCommand(Guid UserId) : IRequest
{
    public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand>
    {
        private readonly IApplicationDbContext _db;

        public BlockUserCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Set<User>()
                           .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Пользователь не найден.");

            user.Status = UserStatus.Blocked;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}